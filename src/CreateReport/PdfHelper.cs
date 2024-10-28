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

            var trueTypeFontPath = "Roboto-Regular.ttf";
            var font = builder.AddTrueTypeFont(File.ReadAllBytes(trueTypeFontPath));

            var page = builder.AddPage(PageSize.A2, false);
            var pageTop = new PdfPoint(0, page.PageSize.Top);

            page.AddText("Feinstaub Messwerte PM2.5", 20, pageTop.Translate(5, -25), font);

            for (var i = 0; i < deviceDataStatistics.Length; i++)
            {
                var chartElementWidth = 9.8;
                var chartElementHeight = 80;

                var chartElementSpace = 5.0;
                var chartElementSpaceY = 50;

                var positionY = page.PageSize.Top - ((i + 1) * (chartElementHeight + chartElementSpaceY));

                var deviceDataStatistic = deviceDataStatistics[i];
                var dataPoints = deviceDataStatistic.HourlyStatisticData.OrderBy(o => o.Date).ThenBy(o => o.Hour).ToArray();

                var nextDay = false;
                var lastDayPositionX = chartElementSpace;

                for (var j = 0; j < dataPoints.Length; j++)
                {
                    var dataPoint = dataPoints[j];

                    if (j > 0 && dataPoint.Date.DayOfWeek != dataPoints[j - 1].Date.DayOfWeek || j == dataPoints.Length - 1)
                    {
                        nextDay = true;
                    }
                    else
                    {
                        nextDay = false;
                    }

                    var positionX = (j * chartElementWidth) + chartElementSpace;

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
                    page.DrawRectangle(new PdfPoint(positionX, chartElementVeryGoodPositionY), chartElementWidth, chartElementVeryGoodHeight, 1, fill: true);

                    page.SetTextAndFillColor(32, 142, 43); //Green
                    page.DrawRectangle(new PdfPoint(positionX, chartElementGoodPositionY), chartElementWidth, chartElementGoodHeight, 1, fill: true);

                    page.SetTextAndFillColor(220, 193, 58); //Orange
                    page.DrawRectangle(new PdfPoint(positionX, chartElementSatisfactoryPositionY), chartElementWidth, chartElementSatisfactoryHeight, 1, fill: true);

                    page.SetTextAndFillColor(209, 67, 67); //Red
                    page.DrawRectangle(new PdfPoint(positionX, chartElementPoorPositionY), chartElementWidth, chartElementPoorHeight, 1, fill: true);

                    page.SetTextAndFillColor(223, 2, 2); //Dark Red
                    page.DrawRectangle(new PdfPoint(positionX, chartElementVeryPoorPositionY), chartElementWidth, chartElementVeryPoorHeight, 1, fill: true);

                    if (nextDay)
                    {
                        page.SetTextAndFillColor(220, 220, 220); //Light Gray
                        page.DrawRectangle(new PdfPoint(lastDayPositionX, positionY - 8), positionX - lastDayPositionX, 8, lineWidth: 0, fill: true);

                        page.SetTextAndFillColor(10, 10, 10);
                        page.AddText($"{dataPoint.Date:ddd}", 6, new PdfPoint(lastDayPositionX + 2, positionY - 6), font);

                        lastDayPositionX = positionX;
                    }

                    page.SetTextAndFillColor(100, 100, 100);
                    page.AddText($"{dataPoint.Hour}", 5, new PdfPoint(positionX, positionY - 14), font);
                }

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
