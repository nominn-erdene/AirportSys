using System;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Airport.Core.Models;

namespace Airport.CheckInApp.Forms
{
    public class PassportVerificationForm : Form
    {
        private readonly TextBox _passportNumberTextBox;
        private readonly Button _nextButton;
        private readonly Label _messageLabel;
        private readonly HttpClient _httpClient;
        private Passenger _verifiedPassenger;
        private const string SERVER_URL = "http://localhost:5268"; // Match the server's configured port

        public PassportVerificationForm()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(SERVER_URL)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            this.Text = "Паспортын баталгаажуулалт";
            this.Size = new System.Drawing.Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            var label = new Label
            {
                Text = "Паспортын дугаар:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            _passportNumberTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 40),
                Width = 200
            };

            _nextButton = new Button
            {
                Text = "Үргэлжлүүлэх",
                Location = new System.Drawing.Point(20, 70),
                Width = 100,
                Enabled = false
            };

            _messageLabel = new Label
            {
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true,
                ForeColor = System.Drawing.Color.Red
            };

            _passportNumberTextBox.TextChanged += (s, e) =>
            {
                _nextButton.Enabled = !string.IsNullOrWhiteSpace(_passportNumberTextBox.Text);
            };

            _nextButton.Click += async (s, e) => await VerifyPassport();

            this.Controls.AddRange(new Control[] { label, _passportNumberTextBox, _nextButton, _messageLabel });

            // Test server connection on form load
            this.Load += async (s, e) => await TestServerConnection();
        }

        private async Task TestServerConnection()
        {
            try
            {
                _messageLabel.Text = $"Сервертэй холбогдож байна ({SERVER_URL})...";
                var response = await _httpClient.GetAsync("/api/health");
                if (!response.IsSuccessStatusCode)
                {
                    _messageLabel.Text = $"Сервертэй холбогдоход алдаа гарлаа ({SERVER_URL}). Статус код: {response.StatusCode}";
                    _nextButton.Enabled = false;
                }
                else
                {
                    _messageLabel.Text = "Сервертэй амжилттай холбогдлоо.";
                    _messageLabel.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (HttpRequestException ex)
            {
                _messageLabel.Text = $"Сервертэй холбогдоход алдаа гарлаа ({SERVER_URL}). Алдаа: {ex.Message}";
                MessageBox.Show($"Дэлгэрэнгүй алдаа: {ex}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _nextButton.Enabled = false;
            }
            catch (Exception ex)
            {
                _messageLabel.Text = $"Тодорхойгүй алдаа гарлаа: {ex.Message}";
                _nextButton.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // PassportVerificationForm
            // 
            ClientSize = new Size(1078, 253);
            Name = "PassportVerificationForm";
            ResumeLayout(false);
        }

        private async Task VerifyPassport()
        {
            try
            {
                _nextButton.Enabled = false;
                _messageLabel.Text = "Шалгаж байна...";

                var response = await _httpClient.GetAsync($"/api/passengers/{_passportNumberTextBox.Text}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _verifiedPassenger = JsonSerializer.Deserialize<Passenger>(content);

                    if (_verifiedPassenger != null)
                    {
                        var seatSelectionForm = new SeatSelectionForm(_verifiedPassenger);
                        this.Hide();
                        seatSelectionForm.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        _messageLabel.Text = "Зорчигч олдсонгүй.";
                        _nextButton.Enabled = true;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _messageLabel.Text = "Зорчигч олдсонгүй.";
                    _nextButton.Enabled = true;
                }
                else
                {
                    _messageLabel.Text = $"Сервертэй холбогдоход алдаа гарлаа. Статус код: {response.StatusCode}";
                    _nextButton.Enabled = true;
                }
            }
            catch (HttpRequestException ex)
            {
                _messageLabel.Text = $"Сервертэй холбогдоход алдаа гарлаа. Алдаа: {ex.Message}";
                MessageBox.Show($"Дэлгэрэнгүй алдаа: {ex}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _nextButton.Enabled = true;
            }
            catch (Exception ex)
            {
                _messageLabel.Text = $"Алдаа гарлаа: {ex.Message}";
                _nextButton.Enabled = true;
            }
        }
    }
} 