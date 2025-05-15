using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using Airport.Core.Models;

namespace Airport.CheckInApp.Forms
{
    public partial class SeatSelection : Form
    {
        private readonly HubConnection _hubConnection;
        private readonly Flight _flight;
        private readonly TableLayoutPanel _seatsPanel;
        private Button _selectedSeat;

        public string SelectedSeatNumber { get; private set; }

        public SeatSelection(Flight flight)
        {
            InitializeComponent();
            _flight = flight;

            // Суудлын панел үүсгэх
            _seatsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = _flight.TotalSeats / 6
            };

            Controls.Add(_seatsPanel);
            CreateSeats();

            // SignalR холболт
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5268/flightHub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, string>("SeatAssigned", UpdateSeatStatus);
            
            ConnectToHub();
        }

        private void CreateSeats()
        {
            if (_flight.Seats == null) return;

            foreach (var seat in _flight.Seats)
            {
                var btn = new Button
                {
                    Text = seat.SeatNumber,
                    Size = new Size(50, 50),
                    BackColor = seat.IsOccupied ? Color.Red : Color.Green,
                    Margin = new Padding(5),
                    Tag = seat
                };

                btn.Click += SeatButton_Click;
                _seatsPanel.Controls.Add(btn);
            }
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var seat = (Seat)btn.Tag;
            
            if (seat.IsOccupied)
            {
                MessageBox.Show("Энэ суудал захиалагдсан байна!", "Анхааруулга", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedSeat != null)
                _selectedSeat.BackColor = Color.Green;

            btn.BackColor = Color.Yellow;
            _selectedSeat = btn;
            SelectedSeatNumber = seat.SeatNumber;
        }

        private void UpdateSeatStatus(int flightId, string seatNumber)
        {
            if (_flight.Id != flightId)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateSeatStatus(flightId, seatNumber)));
                return;
            }

            foreach (Button btn in _seatsPanel.Controls)
            {
                var seat = (Seat)btn.Tag;
                if (seat.SeatNumber == seatNumber)
                {
                    seat.IsOccupied = true;
                    btn.BackColor = Color.Red;
                    
                    if (_selectedSeat == btn)
                    {
                        _selectedSeat = null;
                        SelectedSeatNumber = null;
                    }
                    break;
                }
            }
        }

        private async void ConnectToHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("JoinFlightGroup", _flight.FlightNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сүлжээний алдаа гарлаа: {ex.Message}", "Алдаа", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                _hubConnection.InvokeAsync("LeaveFlightGroup", _flight.FlightNumber).Wait();
                _hubConnection.StopAsync().Wait();
            }
        }
    }
} 