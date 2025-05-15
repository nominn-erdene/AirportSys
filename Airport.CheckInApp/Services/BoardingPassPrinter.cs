using System;
using System.Drawing;
using System.Drawing.Printing;
using Airport.Core.Models;
using ZXing;
using ZXing.Common;

namespace Airport.CheckInApp.Services
{
    public class BoardingPassPrinter
    {
        private readonly BoardingPass _boardingPass;
        private readonly Font _titleFont;
        private readonly Font _normalFont;
        private readonly int _margin = 10;
        private readonly int _lineHeight = 20;

        public BoardingPassPrinter(BoardingPass boardingPass)
        {
            _boardingPass = boardingPass;
            _titleFont = new Font("Arial", 12, FontStyle.Bold);
            _normalFont = new Font("Arial", 10);
        }

        public void Print()
        {
            var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.Print();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var graphics = e.Graphics;
            var currentY = _margin;

            // Гарчиг
            graphics.DrawString("BOARDING PASS", _titleFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight * 2;

            // Нислэгийн мэдээлэл
            graphics.DrawString($"Flight: {_boardingPass.Flight.FlightNumber}", _normalFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight;

            graphics.DrawString($"Date: {_boardingPass.Flight.DepartureTime:d}", _normalFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight;

            graphics.DrawString($"Gate: {_boardingPass.Flight.Gate}", _normalFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight;

            // Зорчигчийн мэдээлэл
            graphics.DrawString($"Passenger: {_boardingPass.Passenger.Name}", _normalFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight;

            graphics.DrawString($"Seat: {_boardingPass.Seat.SeatNumber}", _normalFont, Brushes.Black, _margin, currentY);
            currentY += _lineHeight * 2;

            // Баркод
            try 
            {
                var writer = new BarcodeWriter<Bitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 200,
                        Height = 100,
                        Margin = 0
                    }
                };

                var barcodeImage = writer.Write(_boardingPass.BarcodeData);
                graphics.DrawImage(barcodeImage, _margin, currentY);
            }
            catch (Exception ex)
            {
                graphics.DrawString("Error generating barcode", _normalFont, Brushes.Red, _margin, currentY);
                Console.WriteLine($"Barcode generation error: {ex.Message}");
            }

            e.HasMorePages = false;
        }
    }
} 