using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Airport.Core.Models;

namespace Airport.Server.Services
{
    public class SocketServer : BackgroundService
    {
        private readonly ILogger<SocketServer> _logger;
        private readonly ConcurrentDictionary<string, Socket> _clients;
        private Socket _listener;
        private const int PORT = 11000;

        public SocketServer(ILogger<SocketServer> logger)
        {
            _logger = logger;
            _clients = new ConcurrentDictionary<string, Socket>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(100);

                _logger.LogInformation($"Socket server started on port {PORT}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    Socket clientSocket = await Task.Factory.FromAsync(
                        _listener.BeginAccept,
                        _listener.EndAccept,
                        null);

                    _ = HandleClientAsync(clientSocket, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in socket server");
            }
        }

        private async Task HandleClientAsync(Socket clientSocket, CancellationToken stoppingToken)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, clientSocket);

            try
            {
                byte[] buffer = new byte[1024];
                while (!stoppingToken.IsCancellationRequested && clientSocket.Connected)
                {
                    int bytesRead = await Task.Factory.FromAsync(
                        (callback, state) => clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, state),
                        ar => clientSocket.EndReceive(ar),
                        null);

                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message, clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling client {clientId}");
            }
            finally
            {
                _clients.TryRemove(clientId, out _);
                clientSocket.Close();
            }
        }

        private Task ProcessMessageAsync(string message, string clientId)
        {
            try
            {
                var data = JsonSerializer.Deserialize<SocketMessage>(message);
                switch (data.Type)
                {
                    case "SeatAssignment":
                        BroadcastToOtherClients(message, clientId);
                        break;
                    case "FlightStatusUpdate":
                        BroadcastToAllClients(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }

            return Task.CompletedTask;
        }

        private void BroadcastToAllClients(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clients.Values)
            {
                if (client.Connected)
                {
                    client.Send(messageBytes);
                }
            }
        }

        private void BroadcastToOtherClients(string message, string excludeClientId)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clients)
            {
                if (client.Key != excludeClientId && client.Value.Connected)
                {
                    client.Value.Send(messageBytes);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var client in _clients.Values)
            {
                client.Close();
            }
            _listener?.Close();

            await base.StopAsync(cancellationToken);
        }
    }

    public class SocketMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
} 