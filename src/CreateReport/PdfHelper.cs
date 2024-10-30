using CreateReport.Models;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
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
                    Title = "Feinstaub Report"
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

            this.DrawCenterText(coverPage, "Feinstaubgurke", fontSize: 50);
            this.DrawCenterText(coverPage, "Report für die Messwerte", fontSize: 20, shiftY: -40);

            #endregion

            var paddingX = 10;

            for (var i = 0; i < deviceInfos.Length; i++)
            {
                var deviceDataStatistic = deviceInfos[i];

                var page = this._pdfDocumentBuilder.AddPage(PageSize.A4, false);
                var pageTop = new PdfPoint(0, page.PageSize.Top);

                #region Page Info Text

                if (deviceDataStatistic.Name != null)
                {
                    page.AddText($"{deviceDataStatistic.City} {deviceDataStatistic.District}", 20, pageTop.Translate(paddingX, -45), this._headlineFont);
                    page.AddText(deviceDataStatistic.Name, 8, pageTop.Translate(paddingX, -54), this._defaultFont);
                }

                #endregion

                var dataPoints = deviceDataStatistic.HourlyPM2_5StatisticData.OrderBy(o => o.Date).ThenBy(o => o.Hour).ToArray();
                this.DrawHourGraphic(page, 100, dataPoints, 200);
            }

            var fileBytes = this._pdfDocumentBuilder.Build();
            File.WriteAllBytes("AirQualityReport.pdf", fileBytes);
        }

        private void DrawHourGraphic(
            PdfPageBuilder page,
            int positionShiftY,
            HourlyStatisticData[] dataPoints,
            int chartElementHeight = 200)
        {
            var font = this._defaultFont;
            var pagePadding = 10.0;

            var dayInfoPositionShiftY = 0;

            var pageWidth = page.PageSize.Width - (pagePadding * 2);
            var chartElementWidth = pageWidth / dataPoints.Length;

            var drawInitPosition = new PdfPoint(pagePadding, page.PageSize.Top - positionShiftY);
            page.AddText("Detail Report PM2.5", 12, drawInitPosition.MoveY(10), font);

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

        private void SetColor(
            PdfPageBuilder page,
            DrawColor color)
        {
            switch (color)
            {
                case DrawColor.Black:
                    page.SetTextAndFillColor(0, 0, 0);
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
                default:
                    break;
            }
        }

        private void DrawCenterText(
            PdfPageBuilder page,
            string text,
            int fontSize,
            int shiftY = 0)
        {
            var pageCenter = new PdfPoint(page.PageSize.Width / 2, page.PageSize.Top / 2);

            var letterInfos = page.MeasureText(text, fontSize, pageCenter, this._headlineFont);
            var startPosition = letterInfos.Select(letterInfo => letterInfo.Location.X).Min();
            var endPosition = letterInfos.Select(letterInfo => letterInfo.EndBaseLine.X).Max();
            var textWidth = endPosition - startPosition;
            page.AddText(text, fontSize, pageCenter.Translate(-(textWidth / 2), shiftY), this._headlineFont);
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
            page.SetTextAndFillColor(10, 10, 10);
            page.AddText($"{date:ddd dd.MM.yyyy}", 6, pdfPoint.Translate(2, -(fontSize + 1)), this._defaultFont);
        }
    }
}
