using System;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Airport.Core.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;

namespace Airport.StatusApp.Forms
{
    public partial class StatusManagementForm : Form
    {
        private readonly DataGridView _flightsGrid;
        private readonly ComboBox _statusComboBox;
        private readonly Button _updateButton;
        private readonly Label _connectionStatusLabel;
        private readonly HubConnection _hubConnection;
        private readonly HttpClient _httpClient;

                public StatusManagementForm()        {            // Initialize HttpClient
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5268")
            };

            // Initialize SignalR hub connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5268/flightHub")
                .Build();

            // Create controls
            _flightsGrid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            _statusComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(10, 320),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _updateButton = new Button
            {
                Text = "Статус шинэчлэх",
                Location = new System.Drawing.Point(220, 320),
                Width = 120
            };

            _connectionStatusLabel = new Label
            {
                Text = "Холболт: Салсан",
                Location = new System.Drawing.Point(10, 360),
                ForeColor = System.Drawing.Color.Red
            };

            // Add controls to form
            Controls.AddRange(new Control[] { _flightsGrid, _statusComboBox, _updateButton, _connectionStatusLabel });

            // Set up event handlers
            Load += StatusManagementForm_Load;
            _updateButton.Click += UpdateButton_Click;
            _flightsGrid.SelectionChanged += FlightsGrid_SelectionChanged;

            // Set up SignalR event handlers
            _hubConnection.On<int, FlightStatus>("FlightStatusChanged", OnFlightStatusChanged);
            _hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await ConnectToHub();
            };
        }

        private async void StatusManagementForm_Load(object sender, EventArgs e)
        {
            // Set up grid columns
            _flightsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "FlightNumber", HeaderText = "Нислэгийн дугаар", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Destination", HeaderText = "Очих газар", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "DepartureTime", HeaderText = "Нислэгийн цаг", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Төлөв", Width = 100 }
            });

            // Populate status combo box
            _statusComboBox.Items.AddRange(new object[] { FlightStatus.Delayed, FlightStatus.Cancelled });
            _statusComboBox.Enabled = false;

            await ConnectToHub();
            await LoadFlights();
        }

        private async Task ConnectToHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                _connectionStatusLabel.Text = "Холболт: Холбогдсон";
                _connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                _connectionStatusLabel.Text = $"Холболт: Алдаа - {ex.Message}";
                _connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private async Task LoadFlights()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/flights");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var flights = JsonSerializer.Deserialize<Flight[]>(content);

                    _flightsGrid.Rows.Clear();
                    foreach (var flight in flights)
                    {
                        _flightsGrid.Rows.Add(
                            flight.Id,
                            flight.FlightNumber,
                            flight.Destination,
                            flight.DepartureTime.ToString("yyyy-MM-dd HH:mm"),
                            flight.Status.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Нислэгийн мэдээлэл ачаалахад алдаа гарлаа: {ex.Message}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FlightsGrid_SelectionChanged(object sender, EventArgs e)
        {
            _statusComboBox.Enabled = _flightsGrid.SelectedRows.Count > 0;
            if (_flightsGrid.SelectedRows.Count > 0)
            {
                var currentStatus = (FlightStatus)Enum.Parse(typeof(FlightStatus), _flightsGrid.SelectedRows[0].Cells["Status"].Value.ToString());
                if (currentStatus != FlightStatus.Delayed && currentStatus != FlightStatus.Cancelled)
                {
                    _statusComboBox.SelectedIndex = -1;
                }
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_flightsGrid.SelectedRows.Count == 0 || _statusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Нислэг болон шинэ төлөв сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var flightId = (int)_flightsGrid.SelectedRows[0].Cells["Id"].Value;
            var newStatus = (FlightStatus)_statusComboBox.SelectedItem;

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { status = newStatus }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"/api/flights/{flightId}/status", content);
                if (response.IsSuccessStatusCode)
                {
                    _flightsGrid.SelectedRows[0].Cells["Status"].Value = newStatus.ToString();
                    MessageBox.Show("Нислэгийн төлөв амжилттай шинэчлэгдлээ.", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Нислэгийн төлөв шинэчлэхэд алдаа гарлаа.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Нислэгийн төлөв шинэчлэхэд алдаа гарлаа: {ex.Message}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnFlightStatusChanged(int flightId, FlightStatus newStatus)
        {
            // Update grid on UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnFlightStatusChanged(flightId, newStatus)));
                return;
            }

            foreach (DataGridViewRow row in _flightsGrid.Rows)
            {
                if ((int)row.Cells["Id"].Value == flightId)
                {
                    row.Cells["Status"].Value = newStatus.ToString();
                    break;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_hubConnection != null)
            {
                _hubConnection.DisposeAsync().AsTask().Wait();
            }
        }
    }
} 