using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;
using Airport.Core.Models;
using System.Drawing;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

namespace Airport.CheckInApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly Socket _clientSocket;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5000/api";
        private Flight _currentFlight;
        private List<Button> _seatButtons;

        public MainForm()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _seatButtons = new List<Button>();
            ConnectToServer();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Airport Check-in System";

            // Flight Info Panel
            var flightInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100
            };

            var flightNumberLabel = new Label
            {
                Text = "Flight Number:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            var flightNumberTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Width = 100
            };

            var searchButton = new Button
            {
                Text = "Search Flight",
                Location = new Point(230, 15),
                Width = 100,
                Height = 30
            };
            searchButton.Click += async (s, e) => await SearchFlight(flightNumberTextBox.Text);

            // Passenger Info Panel
            var passengerInfoPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300
            };

            var passportLabel = new Label
            {
                Text = "Passport Number:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            var passportTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Width = 150
            };

            var searchPassengerButton = new Button
            {
                Text = "Search Passenger",
                Location = new Point(10, 50),
                Width = 120,
                Height = 30
            };
            searchPassengerButton.Click += async (s, e) => await SearchPassenger(passportTextBox.Text);

            // Seat Selection Panel
            var seatPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Add controls to panels
            flightInfoPanel.Controls.AddRange(new Control[] { flightNumberLabel, flightNumberTextBox, searchButton });
            passengerInfoPanel.Controls.AddRange(new Control[] { passportLabel, passportTextBox, searchPassengerButton });

            // Add panels to form
            this.Controls.AddRange(new Control[] { flightInfoPanel, passengerInfoPanel, seatPanel });

            CreateSeatGrid(seatPanel);
        }

        private void CreateSeatGrid(Panel panel)
        {
            const int rows = 30;
            const int cols = 6;
            const int buttonSize = 40;
            const int spacing = 5;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var seatButton = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(350 + (buttonSize + spacing) * col, 20 + (buttonSize + spacing) * row),
                        Text = $"{(char)('A' + col)}{row + 1}",
                        Tag = false // false = available, true = occupied
                    };

                    seatButton.Click += async (s, e) => await AssignSeat(((Button)s).Text);
                    _seatButtons.Add(seatButton);
                    panel.Controls.Add(seatButton);
                }
            }
        }

        private async Task SearchFlight(string flightNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/flights/{flightNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _currentFlight = JsonSerializer.Deserialize<Flight>(content);
                    UpdateSeatGrid();
                    MessageBox.Show($"Flight {_currentFlight.FlightNumber} to {_currentFlight.Destination} found.");
                }
                else
                {
                    MessageBox.Show("Flight not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task SearchPassenger(string passportNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/passengers/{passportNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var passenger = JsonSerializer.Deserialize<Passenger>(content);
                    MessageBox.Show($"Passenger found: {passenger.FirstName} {passenger.LastName}");
                }
                else
                {
                    MessageBox.Show("Passenger not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task AssignSeat(string seatNumber)
        {
            if (_currentFlight == null)
            {
                MessageBox.Show("Please search for a flight first.");
                return;
            }

            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/flights/{_currentFlight.Id}/seats/{seatNumber}/assign",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Seat {seatNumber} assigned successfully.");
                    UpdateSeatGrid();
                    NotifyOtherClients(seatNumber);
                }
                else
                {
                    MessageBox.Show("Failed to assign seat. It might be already taken.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void UpdateSeatGrid()
        {
            if (_currentFlight?.Seats == null) return;

            foreach (var seat in _currentFlight.Seats)
            {
                var button = _seatButtons.Find(b => b.Text == seat.SeatNumber);
                if (button != null)
                {
                    button.BackColor = seat.IsOccupied ? Color.Red : Color.Green;
                    button.Tag = seat.IsOccupied;
                }
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _clientSocket.Connect("localhost", 11000);
                BeginReceive();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to server: {ex.Message}");
            }
        }

        private void BeginReceive()
        {
            var buffer = new byte[1024];
            _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), buffer);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var buffer = (byte[])ar.AsyncState;
                int received = _clientSocket.EndReceive(ar);

                if (received > 0)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, received);
                    var data = JsonSerializer.Deserialize<SocketMessage>(message);

                    if (data.Type == "SeatAssignment")
                    {
                        this.Invoke(new Action(() => UpdateSeatGrid()));
                    }
                }

                BeginReceive();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving data: {ex.Message}");
            }
        }

        private void NotifyOtherClients(string seatNumber)
        {
            try
            {
                var message = new SocketMessage
                {
                    Type = "SeatAssignment",
                    Data = JsonSerializer.Serialize(new { FlightId = _currentFlight.Id, SeatNumber = seatNumber })
                };

                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                _clientSocket.Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error notifying other clients: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_clientSocket.Connected)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
        }
    }

    public class SocketMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
} 