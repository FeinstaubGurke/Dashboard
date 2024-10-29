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
            var pageCenter = new PdfPoint(coverPage.PageSize.Width / 2, coverPage.PageSize.Top / 2);

            var titleText = "Feinstaubgurke";
            var fontSize = 50;
            var paddingX = 10;

            var letterInfos = coverPage.MeasureText(titleText, fontSize, pageCenter, this._headlineFont);
            var startPosition = letterInfos.Select(letterInfo => letterInfo.Location.X).Min();
            var endPosition = letterInfos.Select(letterInfo => letterInfo.EndBaseLine.X).Max();
            var textWidth = endPosition - startPosition;
            coverPage.AddText(titleText, fontSize, pageCenter.Translate(-(textWidth / 2), 0), this._headlineFont);

            #endregion

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

                var dataPoints = deviceDataStatistic.HourlyStatisticData.OrderBy(o => o.Date).ThenBy(o => o.Hour).ToArray();
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

                page.SetTextAndFillColor(13, 205, 45); //Green
                page.DrawRectangle(position.MoveY(chartElementVeryGoodPositionY), chartElementWidth, chartElementVeryGoodHeight, 0.1, fill: true);

                page.SetTextAndFillColor(32, 142, 43); //Dark Green
                page.DrawRectangle(position.MoveY(chartElementGoodPositionY), chartElementWidth, chartElementGoodHeight, 0.1, fill: true);

                page.SetTextAndFillColor(220, 193, 58); //Yellow
                page.DrawRectangle(position.MoveY(chartElementSatisfactoryPositionY), chartElementWidth, chartElementSatisfactoryHeight, 0.1, fill: true);

                page.SetTextAndFillColor(255, 46, 53); //Red
                page.DrawRectangle(position.MoveY(chartElementPoorPositionY), chartElementWidth, chartElementPoorHeight, 0.1, fill: true);

                page.SetTextAndFillColor(190, 1, 25); //Dark Red
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

            page.SetTextAndFillColor(0, 0, 0);
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
