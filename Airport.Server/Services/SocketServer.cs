using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Airport.Core.Models;
using Airport.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Airport.Server.Services
{
    public class SocketServer : IHostedService
    {
        private readonly ILogger<SocketServer> _logger;
        private readonly IFlightService _flightService;
        private readonly Socket _serverSocket;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _seatLocks;
        private readonly ConcurrentDictionary<string, Socket> _clients;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenTask;

        public SocketServer(ILogger<SocketServer> logger, IFlightService flightService)
        {
            _logger = logger;
            _flightService = flightService;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _seatLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _clients = new ConcurrentDictionary<string, Socket>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 11000));
            _serverSocket.Listen(100);

            _listenTask = ListenForClientsAsync(_cancellationTokenSource.Token);
            
            _logger.LogInformation("Socket server started on port 11000");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource?.Cancel();
            
            if (_listenTask != null)
            {
                await _listenTask;
            }

            foreach (var client in _clients.Values)
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing client connection");
                }
            }

            _serverSocket.Close();
            _logger.LogInformation("Socket server stopped");
        }

        private async Task ListenForClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await Task.Factory.FromAsync(
                        _serverSocket.BeginAccept,
                        _serverSocket.EndAccept,
                        null);

                    var clientId = Guid.NewGuid().ToString();
                    _clients.TryAdd(clientId, clientSocket);

                    // Start handling client in a separate task
                    _ = HandleClientAsync(clientSocket, clientId, cancellationToken)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Error handling client {ClientId}", clientId);
                            }
                        }, TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error accepting client connection");
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket, string clientId, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];

            try
            {
                while (!cancellationToken.IsCancellationRequested && clientSocket.Connected)
                {
                    var received = await Task.Factory.FromAsync(
                        (callback, state) => clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, state),
                        ar => clientSocket.EndReceive(ar),
                        null);

                    if (received == 0)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, received);
                    await ProcessMessageAsync(clientSocket, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client {ClientId}", clientId);
            }
            finally
            {
                _clients.TryRemove(clientId, out _);
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing client {ClientId} connection", clientId);
                }
            }
        }

        private async Task ProcessMessageAsync(Socket clientSocket, string messageJson)
        {
            try
            {
                var message = JsonSerializer.Deserialize<SocketMessage>(messageJson);
                
                switch (message.Type)
                {
                    case "SeatReservation":
                        await HandleSeatReservationAsync(clientSocket, message.Data);
                        break;
                    
                    case "FlightStatusUpdate":
                        await HandleFlightStatusUpdateAsync(clientSocket, message.Data);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", messageJson);
                await SendErrorAsync(clientSocket, "Invalid message format");
            }
        }

        private async Task HandleSeatReservationAsync(Socket clientSocket, string data)
        {
            var request = JsonSerializer.Deserialize<SeatReservationRequest>(data);
            var lockKey = $"{request.FlightId}_{request.SeatNumber}";
            
            var seatLock = _seatLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
            
            try
            {
                // Try to acquire lock with timeout
                if (await seatLock.WaitAsync(TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        // Check if seat is still available
                        var isAvailable = await _flightService.IsSeatAvailableAsync(request.FlightId, request.SeatNumber);
                        
                        if (isAvailable)
                        {
                            // Try to assign seat
                            var success = await _flightService.AssignSeatToPassengerAsync(
                                request.FlightId,
                                request.SeatNumber,
                                request.PassportNumber);

                            if (success)
                            {
                                await BroadcastToClientsAsync(new SocketMessage
                                {
                                    Type = "SeatAssigned",
                                    Data = JsonSerializer.Serialize(new
                                    {
                                        FlightId = request.FlightId,
                                        SeatNumber = request.SeatNumber
                                    })
                                });

                                await SendSuccessAsync(clientSocket, "Seat assigned successfully");
                            }
                            else
                            {
                                await SendErrorAsync(clientSocket, "Failed to assign seat");
                            }
                        }
                        else
                        {
                            await SendErrorAsync(clientSocket, "Seat is no longer available");
                        }
                    }
                    finally
                    {
                        seatLock.Release();
                    }
                }
                else
                {
                    await SendErrorAsync(clientSocket, "Request timed out. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing seat reservation");
                await SendErrorAsync(clientSocket, "Internal server error");
            }
        }

        private async Task HandleFlightStatusUpdateAsync(Socket clientSocket, string data)
        {
            var request = JsonSerializer.Deserialize<FlightStatusUpdateRequest>(data);
            
            try
            {
                var success = await _flightService.UpdateFlightStatusAsync(request.FlightId, request.NewStatus);
                
                if (success)
                {
                    await BroadcastToClientsAsync(new SocketMessage
                    {
                        Type = "FlightStatusChanged",
                        Data = JsonSerializer.Serialize(new
                        {
                            FlightId = request.FlightId,
                            Status = request.NewStatus
                        })
                    });

                    await SendSuccessAsync(clientSocket, "Flight status updated successfully");
                }
                else
                {
                    await SendErrorAsync(clientSocket, "Failed to update flight status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flight status");
                await SendErrorAsync(clientSocket, "Internal server error");
            }
        }

        private async Task BroadcastToClientsAsync(SocketMessage message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            
            foreach (var client in _clients.Values)
            {
                try
                {
                    if (client.Connected)
                    {
                        await Task.Factory.FromAsync(
                            (callback, state) => client.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, callback, state),
                            ar => client.EndSend(ar),
                            null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting message to client");
                }
            }
        }

        private async Task SendSuccessAsync(Socket client, string message)
        {
            var response = new SocketMessage
            {
                Type = "Success",
                Data = message
            };
            
            await SendMessageAsync(client, response);
        }

        private async Task SendErrorAsync(Socket client, string error)
        {
            var response = new SocketMessage
            {
                Type = "Error",
                Data = error
            };
            
            await SendMessageAsync(client, response);
        }

        private async Task SendMessageAsync(Socket client, SocketMessage message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            
            await Task.Factory.FromAsync(
                (callback, state) => client.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, callback, state),
                ar => client.EndSend(ar),
                null);
        }
    }

    public class SocketMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }

    public class SeatReservationRequest
    {
        public int FlightId { get; set; }
        public string SeatNumber { get; set; }
        public string PassportNumber { get; set; }
    }

    public class FlightStatusUpdateRequest
    {
        public int FlightId { get; set; }
        public FlightStatus NewStatus { get; set; }
    }
} 