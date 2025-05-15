using System.Windows.Forms;
using System.Drawing;
using Airport.Core.Models;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace Airport.DisplayApp.Forms
{
    public class DisplayForm : Form
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5268/api";
        private readonly HubConnection _hubConnection;
        private DataGridView _flightsGrid;
        private System.Windows.Forms.Timer _refreshTimer;

        public DisplayForm()
        {
            _httpClient = new HttpClient();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5268/flighthub")
                .Build();

            InitializeComponents();
            InitializeSignalR();
            StartRefreshTimer();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);
            this.Text = "Нислэгийн Мэдээлэл";

            _flightsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            _flightsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "FlightNumber", HeaderText = "Нислэгийн дугаар" },
                new DataGridViewTextBoxColumn { Name = "Destination", HeaderText = "Чиглэл" },
                new DataGridViewTextBoxColumn { Name = "DepartureTime", HeaderText = "Нислэгийн цаг" },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Төлөв" },
                new DataGridViewTextBoxColumn { Name = "Gate", HeaderText = "Гарц" }
            });

            this.Controls.Add(_flightsGrid);
        }

        private async void InitializeSignalR()
        {
            _hubConnection.On<int, FlightStatus>("FlightStatusChanged", async (flightId, newStatus) =>
            {
                await LoadFlights();
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SignalR холболт амжилтгүй: {ex.Message}");
            }
        }

        private void StartRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 30000; // 30 секунд
            _refreshTimer.Tick += async (s, e) => await LoadFlights();
            _refreshTimer.Start();

            // Load flights immediately
            _ = LoadFlights();
        }

        private async Task LoadFlights()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/flights");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var flights = JsonSerializer.Deserialize<List<Flight>>(content);

                    _flightsGrid.Rows.Clear();
                    foreach (var flight in flights)
                    {
                        _flightsGrid.Rows.Add(
                            flight.FlightNumber,
                            flight.Destination,
                            flight.DepartureTime.ToString("g"),
                            flight.Status.ToString(),
                            flight.Gate
                        );

                        // Color coding based on status
                        var row = _flightsGrid.Rows[_flightsGrid.Rows.Count - 1];
                        switch (flight.Status)
                        {
                            case FlightStatus.CheckingIn:
                                row.DefaultCellStyle.BackColor = Color.LightGreen;
                                break;
                            case FlightStatus.Delayed:
                                row.DefaultCellStyle.BackColor = Color.LightYellow;
                                break;
                            case FlightStatus.Boarding:
                                row.DefaultCellStyle.BackColor = Color.LightBlue;
                                break;
                            case FlightStatus.Departed:
                                row.DefaultCellStyle.BackColor = Color.Gray;
                                break;
                            case FlightStatus.Cancelled:
                                row.DefaultCellStyle.BackColor = Color.LightPink;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Нислэгийн мэдээлэл авахад алдаа гарлаа: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _ = _hubConnection?.DisposeAsync();
        }
    }
} 