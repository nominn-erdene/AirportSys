using System;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Drawing;
using System.Collections.Generic;
using Airport.Core.Models;

namespace Airport.CheckInApp.Forms
{
    public class SeatSelectionForm : Form
    {
        private readonly Passenger _passenger;
        private readonly HttpClient _httpClient;
        private readonly TableLayoutPanel _seatGrid;
        private readonly Label _flightInfoLabel;
        private readonly Button _nextButton;
        private readonly Label _messageLabel;
        private Button _selectedSeatButton;
        private List<Seat> _seats;

        public SeatSelectionForm(Passenger passenger)
        {
            _passenger = passenger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5268")
            };

            this.Text = "Суудал сонгох";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // Flight info label
            _flightInfoLabel = new Label
            {
                Text = $"Нислэг: {_passenger.Flight.FlightNumber} - {_passenger.Flight.Destination}",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };

            // Seat grid
            _seatGrid = new TableLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(700, 400),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            // Next button
            _nextButton = new Button
            {
                Text = "Үргэлжлүүлэх",
                Location = new Point(20, 480),
                Width = 120,
                Height = 30,
                Enabled = false
            };

            // Message label
            _messageLabel = new Label
            {
                Location = new Point(20, 520),
                AutoSize = true,
                ForeColor = Color.Red
            };

            _nextButton.Click += async (s, e) => await ProcessSeatSelection();

            this.Controls.AddRange(new Control[] { _flightInfoLabel, _seatGrid, _nextButton, _messageLabel });

            this.Load += async (s, e) => await LoadSeats();
        }

        private async Task LoadSeats()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/flights/{_passenger.FlightId}/seats");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _seats = JsonSerializer.Deserialize<List<Seat>>(content);

                    // Configure grid
                    _seatGrid.ColumnCount = 6; // A-F
                    _seatGrid.RowCount = 30; // 1-30

                    // Add column headers
                    for (int col = 0; col < 6; col++)
                    {
                        var header = new Label
                        {
                            Text = ((char)('A' + col)).ToString(),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Dock = DockStyle.Fill
                        };
                        _seatGrid.Controls.Add(header, col, 0);
                    }

                    // Add row headers and seats
                    for (int row = 1; row <= 30; row++)
                    {
                        var rowHeader = new Label
                        {
                            Text = row.ToString(),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Dock = DockStyle.Fill
                        };
                        _seatGrid.Controls.Add(rowHeader, 0, row);

                        for (char col = 'A'; col <= 'F'; col++)
                        {
                            var seatNumber = $"{row}{col}";
                            var seat = _seats.Find(s => s.SeatNumber == seatNumber);
                            var button = new Button
                            {
                                Text = seatNumber,
                                Dock = DockStyle.Fill,
                                Enabled = seat != null && !seat.IsOccupied,
                                BackColor = seat != null && !seat.IsOccupied ? Color.LightGreen : Color.LightGray
                            };

                            button.Click += (s, e) => SeatButton_Click(button, seat);
                            _seatGrid.Controls.Add(button, col - 'A', row);
                        }
                    }
                }
                else
                {
                    _messageLabel.Text = "Суудлын мэдээлэл ачаалахад алдаа гарлаа.";
                }
            }
            catch (Exception ex)
            {
                _messageLabel.Text = $"Алдаа гарлаа: {ex.Message}";
            }
        }

        private void SeatButton_Click(Button button, Seat seat)
        {
            if (_selectedSeatButton != null)
            {
                _selectedSeatButton.BackColor = Color.LightGreen;
            }

            button.BackColor = Color.Yellow;
            _selectedSeatButton = button;
            _nextButton.Enabled = true;
        }

        private async Task ProcessSeatSelection()
        {
            if (_selectedSeatButton == null) return;

            try
            {
                _nextButton.Enabled = false;
                _messageLabel.Text = "Суудал захиалж байна...";

                var seatNumber = _selectedSeatButton.Text;
                var selectedSeat = _seats.Find(s => s.SeatNumber == seatNumber);

                var response = await _httpClient.PutAsync(
                    $"/api/passengers/{_passenger.Id}/seat/{selectedSeat.Id}",
                    new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    var baggageForm = new BaggageInformationForm(_passenger);
                    this.Hide();
                    baggageForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    _messageLabel.Text = "Суудал захиалахад алдаа гарлаа.";
                    _nextButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _messageLabel.Text = $"Алдаа гарлаа: {ex.Message}";
                _nextButton.Enabled = true;
            }
        }
    }
} 