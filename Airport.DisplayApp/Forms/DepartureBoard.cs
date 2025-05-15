using System;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Airport.Core.Models;
using System.Drawing;

namespace Airport.DisplayApp.Forms
{
    public partial class DepartureBoard : Form
    {
        private readonly HubConnection _hubConnection;
        private readonly DataGridView _flightsGrid;

        public DepartureBoard()
        {
            InitializeComponent();
            
            // DataGridView тохиргоо
            _flightsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.Black,
                ForeColor = Color.Yellow,
                DefaultCellStyle = { Font = new Font("Arial", 14) },
                ColumnHeadersDefaultCellStyle = { Font = new Font("Arial", 16, FontStyle.Bold) }
            };

            Controls.Add(_flightsGrid);
            SetupColumns();

            // SignalR холболт
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/flightHub")
                .Build();

            _hubConnection.On<string, FlightStatus>("ReceiveFlightUpdate", UpdateFlight);
            
            ConnectToHub();
        }

        private void SetupColumns()
        {
            _flightsGrid.Columns.Add("Time", "Цаг");
            _flightsGrid.Columns.Add("Flight", "Нислэг");
            _flightsGrid.Columns.Add("Destination", "Чиглэл");
            _flightsGrid.Columns.Add("Gate", "Гарц");
            _flightsGrid.Columns.Add("Status", "Төлөв");
        }

        private void UpdateFlight(string flightNumber, FlightStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateFlight(flightNumber, status)));
                return;
            }

            foreach (DataGridViewRow row in _flightsGrid.Rows)
            {
                if (row.Cells["Flight"].Value?.ToString() == flightNumber)
                {
                    row.Cells["Status"].Value = status.ToString();
                    if (status == FlightStatus.Delayed)
                        row.DefaultCellStyle.ForeColor = Color.Red;
                    break;
                }
            }
        }

        private async void ConnectToHub()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Серверт холбогдоход алдаа гарлаа: {ex.Message}");
            }
        }
    }
} 