using FeinstaubGurke.PdfReport.Models;
using System.Globalization;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using static UglyToad.PdfPig.Writer.PdfDocumentBuilder;

namespace FeinstaubGurke.PdfReport
{
    public class PdfProcessor : IDisposable
    {
        private readonly PdfDocumentBuilder _pdfDocumentBuilder;
        private readonly AddedFont _defaultFont;
        private readonly AddedFont _headlineFont;

        public PdfProcessor(string fontPath)
        {
            this._pdfDocumentBuilder = new PdfDocumentBuilder
            {
                ArchiveStandard = PdfAStandard.A1A,
                DocumentInformation = new DocumentInformationBuilder
                {
                    Title = "Feinstaubgurke Report"
                }
            };

            this._defaultFont = this._pdfDocumentBuilder.AddTrueTypeFont(File.ReadAllBytes(Path.Combine(fontPath, "Roboto-Regular.ttf")));
            this._headlineFont = this._pdfDocumentBuilder.AddTrueTypeFont(File.ReadAllBytes(Path.Combine(fontPath, "Roboto-Bold.ttf")));

            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._pdfDocumentBuilder?.Dispose();
        }

        public byte[] CreateReport(
            DateOnly[] reportDates,
            DeviceInfo[] deviceInfos)
        {
            var coverPage = this._pdfDocumentBuilder.AddPage(PageSize.A4);
            this.DrawCoverPage(coverPage);

            var paddingX = 10;
            var dayPerPage = 14;

            for (var i = 0; i < deviceInfos.Length; i++)
            {
                var deviceInfo = deviceInfos[i];

                if (deviceInfo.DailySensorRecords.Count == 0)
                {
                    continue;
                }

                //var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

                var sensorRecordsWithCorrectTimezone = deviceInfo.SensorRecords.Select(sensorRecord => new SensorRecord
                {
                    Timestamp = sensorRecord.Timestamp.ToLocalTime(),
                    PM1 = sensorRecord.PM1,
                    PM2_5 = sensorRecord.PM2_5,
                    PM4 = sensorRecord.PM4,
                    PM10 = sensorRecord.PM10,
                    Temperature = sensorRecord.Temperature,
                    Humidity = sensorRecord.Humidity
                })
                .GroupBy(o => DateOnly.FromDateTime(o.Timestamp.Date))
                .ToDictionary(o => o.Key, o => o.ToArray());

                var requiredPages = Math.Ceiling(reportDates.Length / (double)dayPerPage);

                for (var pageIndex = 0; pageIndex < requiredPages; pageIndex++)
                {
                    var reportDatesOfPage = reportDates.Skip(pageIndex * dayPerPage).Take(dayPerPage).ToArray();
                    var firstRecord = reportDatesOfPage.First();
                    var lastRecord = reportDatesOfPage.Last();

                    var reportingPeriod = $"{firstRecord} - {lastRecord}";

                    var headline1 = $"{deviceInfo.City} {deviceInfo.District}";
                    var headline2 = $"{deviceInfo.Name} | Berichtszeitraum: {reportingPeriod}";
                    var headline3 = "PM2.5";

                    var page = this._pdfDocumentBuilder.AddPage(PageSize.A4, isPortrait: false);
                    var pageTop = new PdfPoint(0, page.PageSize.Top);

                    var pagePadding = 10.0;

                    this.DrawPageTitle(page, pageTop, headline1, headline2, headline3, paddingX);
                    this.DrawDayAverageGraphic(page, pageTop, reportDatesOfPage, sensorRecordsWithCorrectTimezone, pagePadding);
                    this.DrawDayDetailGraphic(page, 240, reportDatesOfPage, sensorRecordsWithCorrectTimezone, 200, pagePadding);
                    this.DrawLegend(page, new PdfPoint(page.PageSize.Width - 300, page.PageSize.Top - 50));
                }
            }

            return this._pdfDocumentBuilder.Build();
        }

        private void DrawCoverPage(PdfPageBuilder coverPage)
        {
            this.DrawCenterText(coverPage, "Feinstaubgurke", fontSize: 50, font: this._headlineFont, shiftY: 200);
            this.DrawCenterText(coverPage, "Analyse und Visualisierung der Feinstaubbelastung", fontSize: 20, font: this._defaultFont, shiftY: 160);
            this.DrawCenterText(coverPage, "https://www.consilium.europa.eu/de/infographics/air-pollution-in-the-eu", fontSize: 10, font: this._defaultFont, shiftY: 140);
        }

        private void DrawPageTitle(
            PdfPageBuilder page,
            PdfPoint pageTop,
            string headline1,
            string subHeadline1,
            string headline2,
            double paddingX)
        {
            page.AddText(headline1, 20, pageTop.Translate(paddingX, -45), this._headlineFont);
            page.AddText(subHeadline1, 8, pageTop.Translate(paddingX, -57), this._defaultFont);

            page.AddText(headline2, 18, pageTop.Translate(paddingX, -85), this._headlineFont);
        }

        private void DrawDayAverageGraphic(
            PdfPageBuilder page,
            PdfPoint pageTop,
            DateOnly[] reportDates,
            Dictionary<DateOnly, SensorRecord[]> sensorRecords,
            double pagePadding = 10)
        {
            var hourInfoWidth = 10;

            var headlinePosition = pageTop.Translate(pagePadding, -120);

            page.AddText("Tagesmittelwert", 12, headlinePosition, this._headlineFont);

            var drawAreaWidth = page.PageSize.Width - (pagePadding * 2) - hourInfoWidth;
            var boxWidth = drawAreaWidth / reportDates.Length;

            var placeholderPoint = new PdfPoint();

            var boxHeight = 50;
            var boxPadding = 10;

            var fontSizeAverage = 16;
            var fontSizeMaxMinValues = 7;
            var fontSizeLabel = 4;

            var dailyMinimum = "Min";
            var dailyMaximum = "Max";

            var textWidthMinimum = this.GetTextWidth(page.MeasureText(dailyMinimum, fontSizeLabel, placeholderPoint, this._headlineFont));
            var textWidthMaximum = this.GetTextWidth(page.MeasureText(dailyMaximum, fontSizeLabel, placeholderPoint, this._headlineFont));

            var boxDrawPosition = headlinePosition.Translate(hourInfoWidth, -60);

            foreach (var reportDate in reportDates)
            {
                try
                {
                    sensorRecords.TryGetValue(reportDate, out var dailySensorRecords);

                    var averagePm2_5 = dailySensorRecords?.Average(o => o.PM2_5);
                    var minimumPm2_5 = dailySensorRecords?.Min(o => o.PM2_5) ?? 0;
                    var maximumPm2_5 = dailySensorRecords?.Max(o => o.PM2_5) ?? 0;

                    var dailyAverage = $"{averagePm2_5:0.00}";
                    var dailyMinimumValue = $"{minimumPm2_5:0}";
                    var dailyMaximumValue = $"{maximumPm2_5:0}";

                    var color = averagePm2_5 is not null ? this.GetColorFromMeasurement(averagePm2_5.Value) : DrawColor.White;
                    this.SetColor(page, color);
                    page.DrawRectangle(boxDrawPosition, boxWidth, boxHeight, lineWidth: 0.1, fill: true);
                    this.SetColor(page, DrawColor.Black);

                    this.SetColor(page, DrawColor.LightGray);
                    page.DrawRectangle(boxDrawPosition.MoveY(-15), boxWidth, 15, lineWidth: 0.1, fill: true);
                    this.SetColor(page, DrawColor.Gray);
                    page.AddText($"{reportDate:ddd dd.MM.yyyy}", 7, boxDrawPosition.Translate(5, -10), this._defaultFont);

                    if (dailySensorRecords is null)
                    {
                        continue;
                    }

                    var textWidth = this.GetTextWidth(page.MeasureText(dailyAverage, fontSizeAverage, placeholderPoint, this._headlineFont));
                    var paddingLeft = (boxWidth - textWidth) / 2.0;
                    page.AddText(dailyAverage, fontSizeAverage, boxDrawPosition.Translate(paddingLeft, 25), this._headlineFont);

                    var textWidthMinimumValue = this.GetTextWidth(page.MeasureText(dailyMinimumValue, fontSizeMaxMinValues, placeholderPoint, this._headlineFont));
                    var textWidthMaximumValue = this.GetTextWidth(page.MeasureText(dailyMaximumValue, fontSizeMaxMinValues, placeholderPoint, this._headlineFont));

                    var paddingLeft2 = boxWidth - textWidthMaximum - boxPadding;
                    var paddingLeft3 = boxWidth - textWidthMaximumValue - boxPadding;

                    this.SetColor(page, DrawColor.Gray);
                    page.AddText(dailyMinimum, fontSizeLabel, boxDrawPosition.Translate(boxPadding, 6), this._defaultFont);
                    page.AddText(dailyMaximum, fontSizeLabel, boxDrawPosition.Translate(paddingLeft2, 6), this._defaultFont);

                    this.SetColor(page, DrawColor.Black);
                    page.AddText(dailyMinimumValue, fontSizeMaxMinValues, boxDrawPosition.Translate(boxPadding, 11), this._defaultFont);
                    page.AddText(dailyMaximumValue, fontSizeMaxMinValues, boxDrawPosition.Translate(paddingLeft3, 11), this._defaultFont);
                }
                finally
                {
                    boxDrawPosition = boxDrawPosition.MoveX(boxWidth);
                }
            }
        }

        private void DrawDayDetailGraphic(
            PdfPageBuilder page,
            int positionShiftY,
            DateOnly[] reportDates,
            Dictionary<DateOnly, SensorRecord[]> dailySensorRecords,
            int drawElementHeight = 200,
            double pagePadding = 10)
        {
            var font = this._defaultFont;
            var pageWidth = page.PageSize.Width - (pagePadding * 2);

            var columnCount = reportDates.Length;

            var fontSize = 5;
            var hourCount = 24;
            var hourInfoWidth = 10;

            var hourBlocks = 4;
            var hoursPerBlock = hourCount / hourBlocks;
            var spaceBetweenBlocks = 4;

            var chartElementWidth = (pageWidth - hourInfoWidth) / columnCount;

            var drawInitPosition = new PdfPoint(pagePadding, page.PageSize.Top - positionShiftY);
            this.SetColor(page, DrawColor.Black);
            page.AddText("Tagesverlauf (24h)", 12, drawInitPosition.MoveY(10), this._headlineFont);

            var blockHeight = drawElementHeight / (double)hourCount;

            for (var hour = 0; hour < hourCount; hour++)
            {
                var hourBlock = hour / hoursPerBlock;
                var extraMoveHourBlock = hourBlock * spaceBetweenBlocks;

                var moveY = (blockHeight * (hour + 1)) + extraMoveHourBlock;
                var boxPosition = drawInitPosition.MoveY(-moveY);

                this.SetColor(page, DrawColor.LightGray);
                page.DrawRectangle(boxPosition, hourInfoWidth, blockHeight, 0.1, fill: true);
                this.SetColor(page, DrawColor.Black);
                page.AddText(hour.ToString("00"), fontSize, boxPosition.Translate(2, 2.5), this._defaultFont);
            }

            for (var i = 0; i < columnCount; i++)
            {
                var columnDate = reportDates[i];

                dailySensorRecords.TryGetValue(columnDate, out var sensorRecords);

                HourAverage[] dataGroupedByHour = [];

                if (sensorRecords is not null)
                {
                    dataGroupedByHour = [.. sensorRecords
                        .GroupBy(o => o.Timestamp.Hour)
                        .Select(o => new HourAverage
                        {
                            Hour = o.Key,
                            Average = o.Average(x => x.PM2_5) ?? 0
                        })];
                }

                double positionX = hourInfoWidth + (i * chartElementWidth);
                var position = drawInitPosition.Translate(positionX, 0);
                var moveY = 0;

                for (var hour = 0; hour < hourCount; hour++)
                {
                    var hourBlock = hour / hoursPerBlock;
                    moveY = hourBlock * spaceBetweenBlocks;

                    var average = dataGroupedByHour.Where(o => o.Hour == hour).SingleOrDefault()?.Average;
                    var blockPosition = position.MoveY(-((hour + 1) * blockHeight) - moveY);

                    if (average is null)
                    {
                        this.SetColor(page, DrawColor.White);
                        page.DrawRectangle(blockPosition, chartElementWidth, blockHeight, 0.1, fill: true);
                    }
                    else
                    {
                        this.SetColor(page, this.GetColorFromMeasurement(average.Value));
                        page.DrawRectangle(blockPosition, chartElementWidth, blockHeight, 0.1, fill: true);

                        var pmValue = average.Value.ToString("0.00");

                        var letterInfos = page.MeasureText(pmValue, fontSize, new PdfPoint(0, 0), font);
                        var textWidth = this.GetTextWidth(letterInfos);

                        var paddingRight = 1;
                        var textMoveX = chartElementWidth - textWidth - paddingRight;
                        var textMoveY = 2.5;

                        this.SetColor(page, DrawColor.LightGray);
                        page.AddText(pmValue, fontSize, blockPosition.Translate(textMoveX, textMoveY), this._defaultFont);
                    }
                }

                #region Draw Day Description WeekDay and Date

                this.DrawDayDescriptionBox(page, columnDate, position.Translate(0, -drawElementHeight - moveY), chartElementWidth);

                #endregion
            }

            this.SetColor(page, DrawColor.Black);
        }

        private void DrawLegend(
            PdfPageBuilder page,
            PdfPoint drawPosition)
        {
            var legendInformations = new[]
            {
                new { Text = "Sehr gut", Color = DrawColor.Green },
                new { Text = "Gut", Color = DrawColor.DarkGreen },
                new { Text = "Befriedigend", Color = DrawColor.Yellow },
                new { Text = "Schlecht", Color = DrawColor.Red },
                new { Text = "Sehr schlecht", Color = DrawColor.DarkRed },
                new { Text = "Grenzwert", Color = DrawColor.Purple }
            };

            var legendBasePosition = drawPosition;

            var legendBoxWidth = 10;
            var legendBoxHeight = 10;
            var fontSize = 6;

            foreach (var legend in legendInformations)
            {
                var textPosition = legendBasePosition.Translate(legendBoxWidth + 4, 2);

                this.SetColor(page, legend.Color);
                page.DrawRectangle(legendBasePosition, legendBoxWidth, legendBoxHeight, fill: true);

                this.SetColor(page, DrawColor.Black);
                page.AddText(legend.Text, fontSize, textPosition, this._defaultFont);
                var letterInfos = page.MeasureText(legend.Text, fontSize, textPosition, this._defaultFont);
                var textWidth = this.GetTextWidth(letterInfos);
                legendBasePosition = legendBasePosition.MoveX(textWidth + 20);
            }
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
                case DrawColor.White:
                    page.SetTextAndFillColor(255, 255, 255);
                    break;
                case DrawColor.Black:
                    page.SetTextAndFillColor(0, 0, 0);
                    break;
                case DrawColor.Gray:
                    page.SetTextAndFillColor(50, 50, 50);
                    break;
                case DrawColor.LightGray:
                    page.SetTextAndFillColor(240, 240, 240);
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
            var textWidth = this.GetTextWidth(letterInfos);
            page.AddText(text, fontSize, pageCenter.Translate(-(textWidth / 2), shiftY), font);
        }

        private double GetTextWidth(IReadOnlyList<Letter> letterInfos)
        {
            var startPosition = letterInfos.Select(letterInfo => letterInfo.Location.X).Min();
            var endPosition = letterInfos.Select(letterInfo => letterInfo.EndBaseLine.X).Max();
            return endPosition - startPosition;
        }

        private void DrawDayDescriptionBox(
            PdfPageBuilder page,
            DateOnly date,
            PdfPoint pdfPoint,
            double width)
        {
            var height = 10;
            var fontSize = 6;

            var drawPosition = pdfPoint.Translate(0, -height);

            this.SetColor(page, DrawColor.LightGray);
            page.DrawRectangle(drawPosition, width, height, lineWidth: 0, fill: true);
            this.SetColor(page, DrawColor.Gray);
            page.AddText($"{date:ddd dd.MM.yyyy}", 6, pdfPoint.Translate(2, -(fontSize + 1)), this._defaultFont);
        }
    }
}
