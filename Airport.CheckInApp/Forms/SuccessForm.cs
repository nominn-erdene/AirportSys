using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using Airport.Core.Models;

namespace Airport.CheckInApp.Forms
{
    public class SuccessForm : Form
    {
        private readonly Passenger _passenger;
        private readonly Baggage _baggage;
        private readonly Panel _boardingPassPanel;
        private readonly Panel _baggageTagPanel;

        public SuccessForm(Passenger passenger, Baggage baggage)
        {
            _passenger = passenger;
            _baggage = baggage;

            this.Text = "Бүртгэл амжилттай";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // Success message
            var successLabel = new Label
            {
                Text = "Бүртгэл амжилттай хийгдлээ!",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 16, FontStyle.Bold),
                ForeColor = Color.Green
            };

            // Boarding pass panel
            _boardingPassPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(350, 400),
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateBoardingPass();

            // Print boarding pass button
            var printBoardingPassButton = new Button
            {
                Text = "Суудлын тийз хэвлэх",
                Location = new Point(20, 470),
                Width = 350,
                Height = 30
            };
            printBoardingPassButton.Click += (s, e) => PrintBoardingPass();

            // Baggage tag panel
            _baggageTagPanel = new Panel
            {
                Location = new Point(400, 60),
                Size = new Size(350, 400),
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateBaggageTag();

            // Print baggage tag button
            var printBaggageTagButton = new Button
            {
                Text = "Ачааны тийз хэвлэх",
                Location = new Point(400, 470),
                Width = 350,
                Height = 30
            };
            printBaggageTagButton.Click += (s, e) => PrintBaggageTag();

            // Close button
            var closeButton = new Button
            {
                Text = "Дуусгах",
                Location = new Point(350, 520),
                Width = 100,
                Height = 30
            };
            closeButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                successLabel,
                _boardingPassPanel,
                printBoardingPassButton,
                _baggageTagPanel,
                printBaggageTagButton,
                closeButton
            });
        }

        private void CreateBoardingPass()
        {
            var font = this.Font;
            var boldFont = new Font(font.FontFamily, font.Size, FontStyle.Bold);

            var controls = new Control[]
            {
                new Label
                {
                    Text = "СУУДЛЫН ТИЙЗ / BOARDING PASS",
                    Location = new Point(10, 10),
                    AutoSize = true,
                    Font = new Font(font.FontFamily, 12, FontStyle.Bold)
                },
                new Label
                {
                    Text = $"Нислэгийн дугаар / Flight: {_passenger.Flight.FlightNumber}",
                    Location = new Point(10, 50),
                    AutoSize = true,
                    Font = boldFont
                },
                new Label
                {
                    Text = $"Зорчигч / Passenger: {_passenger.LastName} {_passenger.FirstName}",
                    Location = new Point(10, 80),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Суудал / Seat: {_passenger.AssignedSeat.SeatNumber}",
                    Location = new Point(10, 110),
                    AutoSize = true,
                    Font = boldFont
                },
                new Label
                {
                    Text = $"Чиглэл / Destination: {_passenger.Flight.Destination}",
                    Location = new Point(10, 140),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Хөөрөх цаг / Departure: {_passenger.Flight.DepartureTime:yyyy-MM-dd HH:mm}",
                    Location = new Point(10, 170),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Гарц / Gate: {_passenger.Flight.Gate}",
                    Location = new Point(10, 200),
                    AutoSize = true,
                    Font = boldFont
                }
            };

            _boardingPassPanel.Controls.AddRange(controls);
        }

        private void CreateBaggageTag()
        {
            var font = this.Font;
            var boldFont = new Font(font.FontFamily, font.Size, FontStyle.Bold);

            var controls = new Control[]
            {
                new Label
                {
                    Text = "АЧААНЫ ТИЙЗ / BAGGAGE TAG",
                    Location = new Point(10, 10),
                    AutoSize = true,
                    Font = new Font(font.FontFamily, 12, FontStyle.Bold)
                },
                new Label
                {
                    Text = $"Баркод / Barcode: {_baggage.BarcodeNumber}",
                    Location = new Point(10, 50),
                    AutoSize = true,
                    Font = boldFont
                },
                new Label
                {
                    Text = $"Зорчигч / Passenger: {_passenger.LastName} {_passenger.FirstName}",
                    Location = new Point(10, 80),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Нислэг / Flight: {_passenger.Flight.FlightNumber}",
                    Location = new Point(10, 110),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Чиглэл / Destination: {_passenger.Flight.Destination}",
                    Location = new Point(10, 140),
                    AutoSize = true
                },
                new Label
                {
                    Text = $"Жин / Weight: {_baggage.Weight:F1} кг",
                    Location = new Point(10, 170),
                    AutoSize = true,
                    Font = boldFont
                },
                new Label
                {
                    Text = $"Бүртгэсэн цаг / Check-in: {_baggage.CheckInTime:yyyy-MM-dd HH:mm}",
                    Location = new Point(10, 200),
                    AutoSize = true
                }
            };

            _baggageTagPanel.Controls.AddRange(controls);
        }

        private void PrintBoardingPass()
        {
            var printDialog = new PrintDialog();
            var printDocument = new PrintDocument();
            printDocument.PrintPage += (s, e) =>
            {
                using (var bmp = new Bitmap(_boardingPassPanel.Width, _boardingPassPanel.Height))
                {
                    _boardingPassPanel.DrawToBitmap(bmp, new Rectangle(0, 0, _boardingPassPanel.Width, _boardingPassPanel.Height));
                    e.Graphics.DrawImage(bmp, e.MarginBounds.Location);
                }
            };

            printDialog.Document = printDocument;
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        private void PrintBaggageTag()
        {
            var printDialog = new PrintDialog();
            var printDocument = new PrintDocument();
            printDocument.PrintPage += (s, e) =>
            {
                using (var bmp = new Bitmap(_baggageTagPanel.Width, _baggageTagPanel.Height))
                {
                    _baggageTagPanel.DrawToBitmap(bmp, new Rectangle(0, 0, _baggageTagPanel.Width, _baggageTagPanel.Height));
                    e.Graphics.DrawImage(bmp, e.MarginBounds.Location);
                }
            };

            printDialog.Document = printDocument;
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }
    }
} 