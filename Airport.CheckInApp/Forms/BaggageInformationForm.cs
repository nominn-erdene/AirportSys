using System;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Drawing;
using Airport.Core.Models;

namespace Airport.CheckInApp.Forms
{
    public class BaggageInformationForm : Form
    {
        private readonly Passenger _passenger;
        private readonly HttpClient _httpClient;
        private readonly NumericUpDown _weightInput;
        private readonly Button _nextButton;
        private readonly Label _messageLabel;
        private readonly Label _weightLimitLabel;
        private const double WEIGHT_LIMIT = 23.0; // Standard baggage weight limit in kg

        public BaggageInformationForm(Passenger passenger)
        {
            _passenger = passenger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5268")
            };

            this.Text = "Ачааны мэдээлэл";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // Flight info
            var flightInfoLabel = new Label
            {
                Text = $"Нислэг: {_passenger.Flight.FlightNumber} - {_passenger.Flight.Destination}",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };

            // Weight limit info
            _weightLimitLabel = new Label
            {
                Text = $"Зөвшөөрөгдөх жин: {WEIGHT_LIMIT}кг",
                Location = new Point(20, 60),
                AutoSize = true
            };

            // Weight input
            var weightLabel = new Label
            {
                Text = "Ачааны жин (кг):",
                Location = new Point(20, 100),
                AutoSize = true
            };

            _weightInput = new NumericUpDown
            {
                Location = new Point(20, 120),
                Width = 100,
                DecimalPlaces = 1,
                Increment = 0.1M,
                Minimum = 0,
                Maximum = 50 // Allow overweight for additional fees
            };

            // Next button
            _nextButton = new Button
            {
                Text = "Үргэлжлүүлэх",
                Location = new Point(20, 160),
                Width = 120,
                Height = 30
            };

            // Message label
            _messageLabel = new Label
            {
                Location = new Point(20, 200),
                AutoSize = true,
                ForeColor = Color.Red
            };

            _weightInput.ValueChanged += (s, e) =>
            {
                if (_weightInput.Value > (decimal)WEIGHT_LIMIT)
                {
                    _messageLabel.Text = "Анхааруулга: Нэмэлт төлбөр тооцогдоно.";
                    _messageLabel.ForeColor = Color.Orange;
                }
                else
                {
                    _messageLabel.Text = "";
                }
            };

            _nextButton.Click += async (s, e) => await ProcessBaggageInformation();

            this.Controls.AddRange(new Control[] 
            { 
                flightInfoLabel,
                _weightLimitLabel,
                weightLabel,
                _weightInput,
                _nextButton,
                _messageLabel
            });
        }

        private async Task ProcessBaggageInformation()
        {
            try
            {
                _nextButton.Enabled = false;
                _messageLabel.Text = "Мэдээлэл хадгалж байна...";

                                var baggage = new Baggage                {                    Weight = (double)_weightInput.Value,                    CheckInTime = DateTime.Now,                    Passenger = _passenger,                    Flight = _passenger.Flight,                    BarcodeNumber = $"{_passenger.Flight.FlightNumber}-{_passenger.PassportNumber}-BAG-{DateTime.Now:yyyyMMddHHmmss}"                };

                var content = new StringContent(
                    JsonSerializer.Serialize(baggage),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    $"/api/passengers/{_passenger.Id}/baggage",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var savedBaggage = JsonSerializer.Deserialize<Baggage>(responseContent);

                    var successForm = new SuccessForm(_passenger, savedBaggage);
                    this.Hide();
                    successForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    _messageLabel.Text = "Ачааны мэдээлэл хадгалахад алдаа гарлаа.";
                    _messageLabel.ForeColor = Color.Red;
                    _nextButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _messageLabel.Text = $"Алдаа гарлаа: {ex.Message}";
                _messageLabel.ForeColor = Color.Red;
                _nextButton.Enabled = true;
            }
        }
    }
} 
