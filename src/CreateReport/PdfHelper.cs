using CreateReport.Models;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace CreateReport
{
    public static class PdfHelper
    {
        public static void Draw(DeviceDataStatistic[] deviceDataStatistics)
        {
            var builder = new PdfDocumentBuilder
            {
                ArchiveStandard = PdfAStandard.A1A,
                DocumentInformation = new PdfDocumentBuilder.DocumentInformationBuilder
                {
                    Title = "Feinstaub Report"
                }
            };

            var trueTypeFontPath = "Fonts/Roboto-Regular.ttf";
            var font = builder.AddTrueTypeFont(File.ReadAllBytes(trueTypeFontPath));

            #region Cover page

            var page1 = builder.AddPage(PageSize.A4);
            var pageCenter = new PdfPoint(page1.PageSize.Width / 2, page1.PageSize.Top / 2);

            var titleText = "Feinstaubgurke";
            var fontSize = 50;

            var letterInfos = page1.MeasureText(titleText, fontSize, pageCenter, font);
            var startPosition = letterInfos.Select(letterInfo => letterInfo.Location.X).Min();
            var endPosition = letterInfos.Select(letterInfo => letterInfo.EndBaseLine.X).Max();
            var textWidth = endPosition - startPosition;
            page1.AddText(titleText, fontSize, pageCenter.Translate(-(textWidth / 2), 0), font);

            #endregion

            for (var i = 0; i < deviceDataStatistics.Length; i++)
            {
                var page = builder.AddPage(PageSize.A4, false);
                var pageTop = new PdfPoint(0, page.PageSize.Top);

                var width = page.PageSize.Width - 50;

                page.AddText("Messwerte PM2.5", 20, pageTop.Translate(5, -25), font);

                var chartElementWidth = width / deviceDataStatistics[i].HourlyStatisticData.Length;
                var chartElementHeight = 80;

                var pagePaddingLeft = 5.0;
                var chartElementSpaceY = 50;

                var positionY = page.PageSize.Top - (chartElementHeight + chartElementSpaceY);

                var deviceDataStatistic = deviceDataStatistics[i];
                var dataPoints = deviceDataStatistic.HourlyStatisticData.OrderBy(o => o.Date).ThenBy(o => o.Hour).ToArray();

                var nextDay = false;
                var positionX = 0.0;
                var lastDayPositionX = pagePaddingLeft;

                for (var j = 0; j < dataPoints.Length; j++)
                {
                    var dataPoint = dataPoints[j];
                    nextDay = false;

                    if (j > 0 && dataPoint.Date.DayOfWeek != dataPoints[j - 1].Date.DayOfWeek)
                    {
                        nextDay = true;
                    }

                    positionX = (j * chartElementWidth) + pagePaddingLeft;

                    var chartElementVeryGoodHeight = chartElementHeight * dataPoint.VeryGood;
                    var chartElementVeryGoodPositionY = positionY;
                    var chartElementGoodHeight = chartElementHeight * dataPoint.Good;
                    var chartElementGoodPositionY = positionY + chartElementVeryGoodHeight;
                    var chartElementSatisfactoryHeight = chartElementHeight * dataPoint.Satisfactory;
                    var chartElementSatisfactoryPositionY = positionY + chartElementVeryGoodHeight + chartElementGoodHeight;
                    var chartElementPoorHeight = chartElementHeight * dataPoint.Poor;
                    var chartElementPoorPositionY = positionY + chartElementVeryGoodHeight + chartElementGoodHeight + chartElementSatisfactoryHeight;
                    var chartElementVeryPoorHeight = chartElementHeight * dataPoint.VeryPoor;
                    var chartElementVeryPoorPositionY = positionY + chartElementVeryGoodHeight + chartElementGoodHeight + chartElementSatisfactoryHeight + chartElementPoorHeight;

                    page.SetTextAndFillColor(13, 205, 45); //Light Green
                    page.DrawRectangle(new PdfPoint(positionX, chartElementVeryGoodPositionY), chartElementWidth, chartElementVeryGoodHeight, 0.1, fill: true);

                    page.SetTextAndFillColor(32, 142, 43); //Green
                    page.DrawRectangle(new PdfPoint(positionX, chartElementGoodPositionY), chartElementWidth, chartElementGoodHeight, 0.1, fill: true);

                    page.SetTextAndFillColor(220, 193, 58); //Orange
                    page.DrawRectangle(new PdfPoint(positionX, chartElementSatisfactoryPositionY), chartElementWidth, chartElementSatisfactoryHeight, 0.1, fill: true);

                    page.SetTextAndFillColor(255, 46, 53); //Red
                    page.DrawRectangle(new PdfPoint(positionX, chartElementPoorPositionY), chartElementWidth, chartElementPoorHeight, 0.1, fill: true);

                    page.SetTextAndFillColor(190, 1, 25); //Dark Red
                    page.DrawRectangle(new PdfPoint(positionX, chartElementVeryPoorPositionY), chartElementWidth, chartElementVeryPoorHeight, 0.1, fill: true);

                    #region Draw Day Info

                    if (nextDay)
                    {
                        page.SetTextAndFillColor(240, 240, 240); //Light Gray
                        page.DrawRectangle(new PdfPoint(lastDayPositionX, positionY - 8), positionX - lastDayPositionX, 8, lineWidth: 0, fill: true);

                        page.SetTextAndFillColor(10, 10, 10);
                        page.AddText($"{dataPoints[j - 1].Date:ddd dd.MM.yyyy}", 6, new PdfPoint(lastDayPositionX + 2, positionY - 6), font);

                        lastDayPositionX = positionX;
                    }

                    #endregion

                    page.SetTextAndFillColor(100, 100, 100);
                    page.AddText($"{dataPoint.Hour}", 5, new PdfPoint(positionX, positionY - 14), font);
                }

                #region LastDay Info

                page.SetTextAndFillColor(240, 240, 240); //Light Gray
                page.DrawRectangle(new PdfPoint(lastDayPositionX, positionY - 8), (positionX - lastDayPositionX + chartElementWidth), 8, lineWidth: 0, fill: true);

                page.SetTextAndFillColor(10, 10, 10);
                page.AddText($"{dataPoints[dataPoints.Length - 1].Date:ddd dd.MM.yyyy}", 6, new PdfPoint(lastDayPositionX + 2, positionY - 6), font);

                #endregion

                page.SetTextAndFillColor(0, 0, 0);

                var firstDataPoint = dataPoints.FirstOrDefault();
                if (firstDataPoint != null)
                {
                    page.AddText($"{firstDataPoint.Date:yyyy-MM-dd}", 11, new PdfPoint(5, positionY - 25), font);
                }

                var lastDataPoint = dataPoints.LastOrDefault();
                if (lastDataPoint != null)
                {
                    page.AddText($"{lastDataPoint.Date:yyyy-MM-dd}", 11, new PdfPoint(page.PageSize.Right - 100, positionY - 25), font);
                }

                if (deviceDataStatistic.DeviceTitle != null)
                {
                    page.AddText(deviceDataStatistic.DeviceTitle, 12, new PdfPoint(5, positionY + chartElementHeight + 5), font);
                }
            }

            var fileBytes = builder.Build();
            File.WriteAllBytes("AirQualityReport.pdf", fileBytes);
        }
    }
}
