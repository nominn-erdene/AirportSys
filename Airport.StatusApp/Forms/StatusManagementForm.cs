using System.Windows.Forms;
using System.Drawing;
using Airport.Core.Models;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace Airport.StatusApp.Forms
{
    public class StatusManagementForm : Form
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5268/api";
        private readonly HubConnection _hubConnection;
        private DataGridView _flightsGrid;
        private ComboBox _statusComboBox;
        private Button _updateButton;
        private Label _connectionStatusLabel;

        public StatusManagementForm()
        {
            _httpClient = new HttpClient();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5268/flightHub")
                .WithAutomaticReconnect()
                .Build();

            InitializeComponents();
            InitializeSignalRAsync();
            StartPeriodicRefresh();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);
            this.Text = "Нислэгийн Төлөв Удирдах";

            // Connection status label
            _connectionStatusLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 10)
            };

            // Status selection panel
            var statusPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            _statusComboBox = new ComboBox
            {
                Location = new Point(10, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusComboBox.Items.AddRange(Enum.GetNames(typeof(FlightStatus)));

            _updateButton = new Button
            {
                Text = "Төлөв Өөрчлөх",
                Location = new Point(220, 19),
                Width = 120,
                Height = 25
            };
            _updateButton.Click += async (s, e) => await UpdateFlightStatus();

            statusPanel.Controls.AddRange(new Control[] { _statusComboBox, _updateButton });

            // Flights grid
            _flightsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            _flightsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false },
                new DataGridViewTextBoxColumn { Name = "FlightNumber", HeaderText = "Нислэгийн дугаар" },
                new DataGridViewTextBoxColumn { Name = "Destination", HeaderText = "Чиглэл" },
                new DataGridViewTextBoxColumn { Name = "DepartureTime", HeaderText = "Нислэгийн цаг" },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Төлөв" },
                new DataGridViewTextBoxColumn { Name = "Gate", HeaderText = "Гарц" }
            });

            _flightsGrid.SelectionChanged += (s, e) =>
            {
                var hasSelection = _flightsGrid.SelectedRows.Count > 0;
                _statusComboBox.Enabled = hasSelection;
                _updateButton.Enabled = hasSelection;
            };

            this.Controls.AddRange(new Control[] { _connectionStatusLabel, statusPanel, _flightsGrid });
        }

        private async void InitializeSignalRAsync()
        {
            _hubConnection.Reconnecting += error =>
            {
                this.Invoke(() => {
                    _connectionStatusLabel.Text = "Сүлжээний холболт тасарлаа. Дахин холбогдож байна...";
                    _connectionStatusLabel.BackColor = Color.Yellow;
                    _connectionStatusLabel.ForeColor = Color.Black;
                });
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                this.Invoke(() => {
                    _connectionStatusLabel.Text = "Сүлжээний холболт сэргээгдлээ.";
                    _connectionStatusLabel.BackColor = Color.Green;
                    _connectionStatusLabel.ForeColor = Color.White;
                });
                return Task.CompletedTask;
            };

            _hubConnection.Closed += error =>
            {
                this.Invoke(() => {
                    _connectionStatusLabel.Text = "Сүлжээний холболт салсан.";
                    _connectionStatusLabel.BackColor = Color.Red;
                    _connectionStatusLabel.ForeColor = Color.White;
                });
                return Task.CompletedTask;
            };

            try
            {
                await _hubConnection.StartAsync();
                this.Invoke(() => {
                    _connectionStatusLabel.Text = "Сүлжээнд холбогдсон.";
                    _connectionStatusLabel.BackColor = Color.Green;
                    _connectionStatusLabel.ForeColor = Color.White;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SignalR холболт амжилтгүй: {ex.Message}\nПрограм SignalR холболтгүйгээр ажиллах болно.", 
                    "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Invoke(() => {
                    _connectionStatusLabel.Text = "Сүлжээний холболт амжилтгүй.";
                    _connectionStatusLabel.BackColor = Color.Red;
                    _connectionStatusLabel.ForeColor = Color.White;
                });
            }
        }

        private void StartPeriodicRefresh()
        {
            var timer = new System.Windows.Forms.Timer
            {
                Interval = 30000 // 30 seconds
            };
            timer.Tick += async (s, e) => await LoadFlights();
            timer.Start();

            // Initial load
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
                            flight.Id,
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

        private async Task UpdateFlightStatus()
        {
            if (_flightsGrid.SelectedRows.Count == 0 || _statusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Нислэг болон шинэ төлөв сонгоно уу!");
                return;
            }

            var selectedRow = _flightsGrid.SelectedRows[0];
            var flightId = (int)selectedRow.Cells["Id"].Value;
            var newStatus = (FlightStatus)Enum.Parse(typeof(FlightStatus), _statusComboBox.SelectedItem.ToString());

            try
            {
                var response = await _httpClient.PutAsync(
                    $"{_apiBaseUrl}/flights/{flightId}/status",
                    new StringContent(JsonSerializer.Serialize(newStatus), System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    await LoadFlights();
                    MessageBox.Show("Нислэгийн төлөв амжилттай өөрчлөгдлөө!");
                }
                else
                {
                    MessageBox.Show("Төлөв өөрчлөхөд алдаа гарлаа!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_hubConnection != null)
            {
                _ = _hubConnection.DisposeAsync();
            }
        }
    }
} 