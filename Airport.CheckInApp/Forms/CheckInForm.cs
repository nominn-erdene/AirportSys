using System.Windows.Forms;
using System.Drawing;
using Airport.Core.Models;
using System.Net.Http;
using System.Text.Json;

namespace Airport.CheckInApp.Forms
{
    public class CheckInForm : Form
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5268/api";
        private TextBox _flightNumberTextBox;
        private TextBox _passportNumberTextBox;
        private Button _searchFlightButton;
        private Button _searchPassengerButton;
        private Label _flightInfoLabel;
        private Label _passengerInfoLabel;
        private FlowLayoutPanel _seatPanel;
        private Flight? _currentFlight;

        public CheckInForm()
        {
            _httpClient = new HttpClient();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(800, 600);
            this.Text = "Нислэгийн Бүртгэл";

            // Flight search panel
            var flightPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10)
            };

            var flightLabel = new Label
            {
                Text = "Нислэгийн дугаар:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            _flightNumberTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Width = 100
            };

            _searchFlightButton = new Button
            {
                Text = "Хайх",
                Location = new Point(230, 15),
                Width = 80,
                Height = 25
            };
            _searchFlightButton.Click += async (s, e) => await SearchFlight();

            _flightInfoLabel = new Label
            {
                Location = new Point(10, 50),
                AutoSize = true
            };

            flightPanel.Controls.AddRange(new Control[] { flightLabel, _flightNumberTextBox, _searchFlightButton, _flightInfoLabel });

            // Passenger search panel
            var passengerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Top = 100,
                Padding = new Padding(10)
            };

            var passportLabel = new Label
            {
                Text = "Паспортын дугаар:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            _passportNumberTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Width = 100
            };

            _searchPassengerButton = new Button
            {
                Text = "Хайх",
                Location = new Point(230, 15),
                Width = 80,
                Height = 25
            };
            _searchPassengerButton.Click += async (s, e) => await SearchPassenger();

            _passengerInfoLabel = new Label
            {
                Location = new Point(10, 50),
                AutoSize = true
            };

            passengerPanel.Controls.AddRange(new Control[] { passportLabel, _passportNumberTextBox, _searchPassengerButton, _passengerInfoLabel });

            // Seat selection panel
            _seatPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };

            this.Controls.AddRange(new Control[] { flightPanel, passengerPanel, _seatPanel });
        }

        private async Task SearchFlight()
        {
            try
            {
                var flightNumber = _flightNumberTextBox.Text;
                if (string.IsNullOrEmpty(flightNumber))
                {
                    MessageBox.Show("Нислэгийн дугаар оруулна уу!");
                    return;
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/flights/{flightNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _currentFlight = JsonSerializer.Deserialize<Flight>(content);
                    _flightInfoLabel.Text = $"Нислэг: {_currentFlight?.FlightNumber}\nЧиглэл: {_currentFlight?.Destination}\nЦаг: {_currentFlight?.DepartureTime:g}";
                    CreateSeatButtons();
                }
                else
                {
                    MessageBox.Show("Нислэг олдсонгүй!");
                    _flightInfoLabel.Text = "";
                    _currentFlight = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}");
            }
        }

        private async Task SearchPassenger()
        {
            try
            {
                var passportNumber = _passportNumberTextBox.Text;
                if (string.IsNullOrEmpty(passportNumber))
                {
                    MessageBox.Show("Паспортын дугаар оруулна уу!");
                    return;
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/passengers/{passportNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var passenger = JsonSerializer.Deserialize<Passenger>(content);
                    _passengerInfoLabel.Text = $"Зорчигч: {passenger?.FirstName} {passenger?.LastName}\nҮндэс: {passenger?.Nationality}";
                }
                else
                {
                    MessageBox.Show("Зорчигч олдсонгүй!");
                    _passengerInfoLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}");
            }
        }

        private void CreateSeatButtons()
        {
            _seatPanel.Controls.Clear();
            if (_currentFlight?.Seats == null) return;

            foreach (var seat in _currentFlight.Seats)
            {
                var button = new Button
                {
                    Text = seat.SeatNumber,
                    Width = 50,
                    Height = 50,
                    Margin = new Padding(5),
                    BackColor = seat.IsOccupied ? Color.Red : Color.Green
                };

                button.Click += async (s, e) => await AssignSeat(seat.SeatNumber);
                _seatPanel.Controls.Add(button);
            }
        }

        private async Task AssignSeat(string seatNumber)
        {
            if (_currentFlight == null)
            {
                MessageBox.Show("Эхлээд нислэг сонгоно уу!");
                return;
            }

            var passportNumber = _passportNumberTextBox.Text;
            if (string.IsNullOrEmpty(passportNumber))
            {
                MessageBox.Show("Эхлээд зорчигч сонгоно уу!");
                return;
            }

            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/flights/{_currentFlight.Id}/seats/{seatNumber}/assign",
                    new StringContent(JsonSerializer.Serialize(passportNumber), System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"{seatNumber} суудал амжилттай захиалагдлаа!");
                    await SearchFlight(); // Refresh seat status
                }
                else
                {
                    MessageBox.Show("Суудал захиалахад алдаа гарлаа. Суудал захиалагдсан байж болно.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}");
            }
        }
    }
} 