using CreateReport.Models;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using static System.Net.Mime.MediaTypeNames;
using static UglyToad.PdfPig.Writer.PdfDocumentBuilder;

namespace CreateReport
{
    public class PdfHelper : IDisposable
    {
        private readonly PdfDocumentBuilder _pdfDocumentBuilder;
        private readonly AddedFont _defaultFont;
        private readonly AddedFont _headlineFont;

        public PdfHelper()
        {
            this._pdfDocumentBuilder = new PdfDocumentBuilder
            {
                ArchiveStandard = PdfAStandard.A1A,
                DocumentInformation = new PdfDocumentBuilder.DocumentInformationBuilder
                {
                    Title = "Feinstaubgurke Report"
                }
            };

            this._defaultFont = this._pdfDocumentBuilder.AddTrueTypeFont(File.ReadAllBytes("Fonts/Roboto-Regular.ttf"));
            this._headlineFont = this._pdfDocumentBuilder.AddTrueTypeFont(File.ReadAllBytes("Fonts/Roboto-Bold.ttf"));
        }

        public void Dispose()
        {
            this._pdfDocumentBuilder?.Dispose();
        }

        public void CreateReport(DeviceInfo[] deviceInfos)
        {
            #region Cover page

            var coverPage = this._pdfDocumentBuilder.AddPage(PageSize.A4);

            this.DrawCenterText(coverPage, "Feinstaubgurke", fontSize: 50, font: this._headlineFont);
            this.DrawCenterText(coverPage, "Analyse und Visualisierung der Feinstaubbelastung", fontSize: 20, font: this._defaultFont, shiftY: -40);

            #endregion

            var paddingX = 10;

            for (var i = 0; i < deviceInfos.Length; i++)
            {
                var deviceInfo = deviceInfos[i];

                var page = this._pdfDocumentBuilder.AddPage(PageSize.A4, false);
                var pageTop = new PdfPoint(0, page.PageSize.Top);

                #region Page Info Text

                if (deviceInfo.Name != null)
                {
                    page.AddText($"{deviceInfo.City} {deviceInfo.District}", 20, pageTop.Translate(paddingX, -45), this._headlineFont);
                    page.AddText(deviceInfo.Name, 8, pageTop.Translate(paddingX, -54), this._defaultFont);

                    page.AddText("PM2.5", 18, pageTop.Translate(paddingX, -85), this._headlineFont);
                }

                #endregion

                var pagePadding = 10.0;

                page.AddText("Tagesmittelwert", 12, pageTop.Translate(pagePadding, -120), this._headlineFont);

                var dailyAverageValues = deviceInfo.Data.GroupBy(o => o.Timestamp.Date).Select(o => new { Date = o.Key, AveragePm2_5 = o.Average(o => o.PM2_5) });

                var position1 = pageTop.Translate(pagePadding, -180);
                var boxWidth = 80;
                var boxHeight = 50;

                foreach (var dayAverage in dailyAverageValues)
                {
                    if (dayAverage.AveragePm2_5 == null)
                    {
                        continue;
                    }

                    var color = this.GetColorFromMeasurement(dayAverage.AveragePm2_5.Value);

                    this.SetColor(page, color);
                    page.DrawRectangle(position1, boxWidth, boxHeight, fill: true);
                    this.SetColor(page, DrawColor.Black);
                    page.AddText($"{dayAverage.Date:dd.MM.yyyy}", 9, position1.Translate(5, 20), this._headlineFont);
                    page.AddText($"{dayAverage.AveragePm2_5:0.00}", 10, position1.Translate(5, 10), this._defaultFont);

                    position1 = position1.MoveX(boxWidth);
                }

                var dataPoints = deviceInfo.HourlyPM2_5StatisticData.OrderBy(o => o.Date).ThenBy(o => o.Hour).ToArray();
                this.DrawHourGraphic(page, 220, dataPoints, 200, pagePadding);
            }

            var fileBytes = this._pdfDocumentBuilder.Build();
            File.WriteAllBytes("Feinstaub-Report.pdf", fileBytes);
        }

        private void DrawHourGraphic(
            PdfPageBuilder page,
            int positionShiftY,
            HourlyStatisticData[] dataPoints,
            int chartElementHeight = 200,
            double pagePadding = 10)
        {
            var font = this._defaultFont;
            var dayInfoPositionShiftY = 0;

            var pageWidth = page.PageSize.Width - (pagePadding * 2);
            var chartElementWidth = pageWidth / dataPoints.Length;

            var drawInitPosition = new PdfPoint(pagePadding, page.PageSize.Top - positionShiftY);
            this.SetColor(page, DrawColor.Black);
            page.AddText("Tagesverlauf", 12, drawInitPosition.MoveY(10), this._headlineFont);

            var positionX = 0.0;
            var positionY = page.PageSize.Top - positionShiftY;
            var lastDayPositionX = 0.0;

            for (var i = 0; i < dataPoints.Length; i++)
            {
                var dataPoint = dataPoints[i];
                var nextDay = false;

                if (i > 0 && dataPoint.Date.DayOfWeek != dataPoints[i - 1].Date.DayOfWeek)
                {
                    nextDay = true;
                }

                positionX = i * chartElementWidth;
                var position = drawInitPosition.Translate(positionX, -chartElementHeight);

                var chartElementVeryGoodHeight = chartElementHeight * dataPoint.VeryGood;
                var chartElementVeryGoodPositionY = 0;
                var chartElementGoodHeight = chartElementHeight * dataPoint.Good;
                var chartElementGoodPositionY = chartElementVeryGoodHeight;
                var chartElementSatisfactoryHeight = chartElementHeight * dataPoint.Satisfactory;
                var chartElementSatisfactoryPositionY = chartElementVeryGoodHeight + chartElementGoodHeight;
                var chartElementPoorHeight = chartElementHeight * dataPoint.Poor;
                var chartElementPoorPositionY = chartElementVeryGoodHeight + chartElementGoodHeight + chartElementSatisfactoryHeight;
                var chartElementVeryPoorHeight = chartElementHeight * dataPoint.VeryPoor;
                var chartElementVeryPoorPositionY = chartElementVeryGoodHeight + chartElementGoodHeight + chartElementSatisfactoryHeight + chartElementPoorHeight;

                this.SetColor(page, DrawColor.Green);
                page.DrawRectangle(position.MoveY(chartElementVeryGoodPositionY), chartElementWidth, chartElementVeryGoodHeight, 0.1, fill: true);

                this.SetColor(page, DrawColor.DarkGreen);
                page.DrawRectangle(position.MoveY(chartElementGoodPositionY), chartElementWidth, chartElementGoodHeight, 0.1, fill: true);

                this.SetColor(page, DrawColor.Yellow);
                page.DrawRectangle(position.MoveY(chartElementSatisfactoryPositionY), chartElementWidth, chartElementSatisfactoryHeight, 0.1, fill: true);

                this.SetColor(page, DrawColor.Red);
                page.DrawRectangle(position.MoveY(chartElementPoorPositionY), chartElementWidth, chartElementPoorHeight, 0.1, fill: true);

                this.SetColor(page, DrawColor.DarkRed);
                page.DrawRectangle(position.MoveY(chartElementVeryPoorPositionY), chartElementWidth, chartElementVeryPoorHeight, 0.1, fill: true);

                #region Draw Day Info

                if (nextDay)
                {
                    var dayWidth = positionX - lastDayPositionX;
                    this.DrawDayBox(page, dataPoints[i - 1].Date, position.Translate(-dayWidth, -dayInfoPositionShiftY), dayWidth);

                    lastDayPositionX = positionX;
                }

                #endregion

                #region Draw Hour

                page.SetTextAndFillColor(100, 100, 100);
                page.AddText($"{dataPoint.Hour}", 3, position.MoveY(chartElementHeight + 2), font);

                #endregion
            }

            #region Draw LastDay Info

            var dayWidth1 = pageWidth - lastDayPositionX;
            this.DrawDayBox(page, dataPoints.Last().Date, drawInitPosition.Translate(lastDayPositionX, -(chartElementHeight + dayInfoPositionShiftY)), dayWidth1);

            #endregion

            #region Draw Legend

            var legendInformations = new[]
            { 
                new { Text = "Sehr gut", Color = DrawColor.Green },
                new { Text = "Gut", Color = DrawColor.DarkGreen },
                new { Text = "Befriedigend", Color = DrawColor.Yellow },
                new { Text = "Schlecht", Color = DrawColor.Red },
                new { Text = "Sehr schlecht", Color = DrawColor.DarkRed },
            };

            var legendBasePosition = drawInitPosition.MoveY(-chartElementHeight - 25);

            foreach (var legend in legendInformations)
            {
                this.SetColor(page, legend.Color);
                page.DrawRectangle(legendBasePosition, 10, 10, fill: true);
                this.SetColor(page, DrawColor.Black);
                page.AddText(legend.Text, 6, legendBasePosition.Translate(12, 2), font);

                legendBasePosition = legendBasePosition.MoveX(50);
            }

            #endregion

            this.SetColor(page, DrawColor.Black);
        }

        private DrawColor GetColorFromMeasurement(double measurement)
        {
            if (measurement >= 0 && measurement < 5)
            {
                return DrawColor.Green;
            }
            else if (measurement >= 5 && measurement < 10)
            {
                return DrawColor.DarkGreen;
            }
            else if (measurement >= 10 && measurement < 15)
            {
                return DrawColor.Yellow;
            }
            else if (measurement >= 15 && measurement < 20)
            {
                return DrawColor.Red;
            }
            else if (measurement >= 20 && measurement < 25)
            {
                return DrawColor.DarkRed;
            }

            return DrawColor.Purple;
        }

        private void SetColor(
            PdfPageBuilder page,
            DrawColor color)
        {
            switch (color)
            {
                case DrawColor.Black:
                    page.SetTextAndFillColor(0, 0, 0);
                    break;
                case DrawColor.Gray:
                    page.SetTextAndFillColor(10, 10, 10);
                    break;
                case DrawColor.Green:
                    page.SetTextAndFillColor(13, 205, 45);
                    break;
                case DrawColor.DarkGreen:
                    page.SetTextAndFillColor(32, 142, 43);
                    break;
                case DrawColor.Yellow:
                    page.SetTextAndFillColor(220, 193, 58);
                    break;
                case DrawColor.Red:
                    page.SetTextAndFillColor(255, 46, 53);
                    break;
                case DrawColor.DarkRed:
                    page.SetTextAndFillColor(190, 1, 25);
                    break;
                case DrawColor.Purple:
                    page.SetTextAndFillColor(218, 112, 214);
                    break;
                default:
                    break;
            }
        }

        private void DrawCenterText(
            PdfPageBuilder page,
            string text,
            int fontSize,
            AddedFont font,
            int shiftY = 0)
        {
            var pageCenter = new PdfPoint(page.PageSize.Width / 2, page.PageSize.Top / 2);

            var letterInfos = page.MeasureText(text, fontSize, pageCenter, font);
            var startPosition = letterInfos.Select(letterInfo => letterInfo.Location.X).Min();
            var endPosition = letterInfos.Select(letterInfo => letterInfo.EndBaseLine.X).Max();
            var textWidth = endPosition - startPosition;
            page.AddText(text, fontSize, pageCenter.Translate(-(textWidth / 2), shiftY), font);
        }

        private void DrawDayBox(
            PdfPageBuilder page,
            DateOnly date,
            PdfPoint pdfPoint,
            double width)
        {
            var height = 10;
            var fontSize = 6;

            var drawPosition = pdfPoint.Translate(0, -height);

            page.SetTextAndFillColor(240, 240, 240); //Light Gray
            page.DrawRectangle(drawPosition, width, height, lineWidth: 0, fill: true);
            this.SetColor(page, DrawColor.Gray);
            page.AddText($"{date:ddd dd.MM.yyyy}", 6, pdfPoint.Translate(2, -(fontSize + 1)), this._defaultFont);
        }
    }
}
