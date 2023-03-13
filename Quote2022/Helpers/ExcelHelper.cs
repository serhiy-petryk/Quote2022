using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;
using Quote2022.Models;

namespace Quote2022.Helpers
{
    public class ExcelHelper
    {
        private static Dictionary<string, Color> excelColors = new Dictionary<string, Color>
        {
            {"Green", Color.FromArgb(0x92, 0xD0, 0x50)}, {"Yellow", Color.FromArgb(0xFF, 0xFF, 0)},
            {"Orange", Color.FromArgb(0xFF, 0xC0, 0)}
        };

        public class StatisticsData
        {
            public string Title;
            public string Header1;
            public string Header2;
            public string Header3;
            public List<object[]> Table;
        }

        public static void SaveQuotesToExcel_NotNeed(IList<Quote> data, string fileName)
        {
            using (var excelPackage = new ExcelPackage())
            {
                var ws = excelPackage.Workbook.Worksheets.Add("Quotes");

                ws.Cells[1, 1].Value = "Quotes";
                ws.Cells[2, 1].Value = "Symbol";
                ws.Cells[2, 2].Value = "Date";
                ws.Cells[2, 3].Value = "Open";
                ws.Cells[2, 4].Value = "High";
                ws.Cells[2, 5].Value = "Low";
                ws.Cells[2, 6].Value = "Close";
                ws.Cells[2, 7].Value = "Volume";

                var row = 3;
                foreach (var q in data)
                {
                    ws.Cells[row, 1].Value = q.Symbol;
                    ws.Cells[row, 2].Value = q.Timed;
                    ws.Cells[row, 3].Value = q.Open;
                    ws.Cells[row, 4].Value = q.High;
                    ws.Cells[row, 5].Value = q.Low;
                    ws.Cells[row, 6].Value = q.Close;
                    ws.Cells[row, 7].Value = q.Volume;
                    row++;
                }

                excelPackage.SaveAs(new FileInfo(fileName));

            }
        }

        public static void SaveStatisticsToExcel(Dictionary<string, StatisticsData> data, string fileName, string summaryHeader=null, string summarySubheader = null)
        {
            if (data.Count == 0)
                throw new Exception("SaveStatisticsToExcel error. No data");

            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage())
            {
                const int summaryRowOffset = 5;
                // var wsSummary = data.Count > 1 ? excelPackage.Workbook.Worksheets.Add("Summary") : null;
                var wsSummary = excelPackage.Workbook.Worksheets.Add("Summary");

                if (wsSummary != null)
                {
                    wsSummary.Cells["A1"].Value = "Summary";
                    wsSummary.Cells["A1"].Style.Font.Bold = true;
                    wsSummary.Cells["A1"].Style.Font.Size = 12;
                    wsSummary.Cells["K1"].Value = $"Generated at {CsUtils.GetString(DateTime.Now)}";

                    wsSummary.Cells[$"A{summaryRowOffset}:A{summaryRowOffset + 1}"].Merge = true;
                    wsSummary.Cells[$"B{summaryRowOffset}:D{summaryRowOffset}"].Merge = true;
                    wsSummary.Cells[$"E{summaryRowOffset}:G{summaryRowOffset}"].Merge = true;
                    wsSummary.Cells[$"H{summaryRowOffset}:J{summaryRowOffset}"].Merge = true;
                    wsSummary.Cells[$"K{summaryRowOffset}:M{summaryRowOffset}"].Merge = true;
                    wsSummary.Cells[$"A{summaryRowOffset}"].Value = "Sheet";
                    wsSummary.Cells[$"B{summaryRowOffset}"].Value = "BuyK";
                    wsSummary.Cells[$"E{summaryRowOffset}"].Value = "SellK";
                    wsSummary.Cells[$"H{summaryRowOffset}"].Value = "(BuyK+SellK)/2";
                    wsSummary.Cells[$"K{summaryRowOffset}"].Value = "(1-Open/CL),%";
                    wsSummary.Cells[$"B{summaryRowOffset + 1}"].Value = "Max";
                    wsSummary.Cells[$"C{summaryRowOffset + 1}"].Value = "Min";
                    wsSummary.Cells[$"D{summaryRowOffset + 1}"].Value = "Avg";
                    wsSummary.Cells[$"E{summaryRowOffset + 1}"].Value = "Max";
                    wsSummary.Cells[$"F{summaryRowOffset + 1}"].Value = "Min";
                    wsSummary.Cells[$"G{summaryRowOffset + 1}"].Value = "Avg";
                    wsSummary.Cells[$"H{summaryRowOffset + 1}"].Value = "Max";
                    wsSummary.Cells[$"I{summaryRowOffset + 1}"].Value = "Min";
                    wsSummary.Cells[$"J{summaryRowOffset + 1}"].Value = "Avg";
                    wsSummary.Cells[$"K{summaryRowOffset + 1}"].Value = "Max";
                    wsSummary.Cells[$"L{summaryRowOffset + 1}"].Value = "Min";
                    wsSummary.Cells[$"M{summaryRowOffset + 1}"].Value = "Avg";
                    wsSummary.Cells[$"A{summaryRowOffset}:M{summaryRowOffset + 1}"].Style.HorizontalAlignment
                        = ExcelHorizontalAlignment.Center;
                    wsSummary.Cells[$"A{summaryRowOffset}:M{summaryRowOffset + 1}"].Style.VerticalAlignment =
                        ExcelVerticalAlignment.Center;

                    wsSummary.Cells[$"A{summaryRowOffset + 2}"].Value = "Max";
                    wsSummary.Cells[$"A{summaryRowOffset + 3}"].Value = "Min";
                    wsSummary.Cells[$"A{summaryRowOffset + 4}"].Value = "Avg";
                    wsSummary.Cells[summaryRowOffset + 2, 1, summaryRowOffset + 4, 13].Style.Font.Bold = true;
                }

                var summaryRow = summaryRowOffset + 5;
                foreach (var kvp in data)
                {
                    var columnCount = kvp.Value.Table[0].Length;

                    var ws = excelPackage.Workbook.Worksheets.Add(kvp.Key);

                    if (kvp.Value.Table.Count == 0)
                        continue;

                    //Add table
                    using (var rg = ws.Cells[5, 1, kvp.Value.Table.Count + 4, columnCount])
                    {
                        var tbl = ws.Tables.Add(rg, kvp.Key);
                    }

                    // Wrap text for table headers
                    ws.Row(5).Style.WrapText = true;

                    for (var k1 = 0; k1 < kvp.Value.Table.Count; k1++)
                    {
                        var oo = kvp.Value.Table[k1];
                        for (var k2 = 0; k2 < oo.Length; k2++)
                        {
                            ws.Cells[k1 + 5, k2 + 1].Value = oo[k2];
                            if (oo[k2] is TimeSpan ts)
                                ws.Cells[k1 + 5, k2 + 1].Style.Numberformat.Format = "h:mm";
                            else if (oo[k2] is DateTime dt)
                                ws.Cells[k1 + 5, k2 + 1].Style.Numberformat.Format = "yyyy-MM-dd";
                        }
                    }

                    // Clear header and set color of separation column
                    ws.Cells[5, columnCount - 10].Value = " ";
                    using (var rg2 = ws.Cells[5, columnCount - 10, kvp.Value.Table.Count + 4, columnCount - 10])
                        SetColor(rg2, excelColors["Yellow"]);

                    ws.Cells.AutoFitColumns();

                    ws.Column(columnCount - 10).Width = 0.7 * 12.0 / 7.0;
                    ws.Column(columnCount - 11).Width = 8.5 + 5.0/7.0;
                    ws.Column(columnCount - 12).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 13).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 14).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 15).Width = 8.9 + 5.0 / 7.0;
                    ws.Column(columnCount - 16).Width = 8.9 + 5.0 / 7.0;
                    ws.Column(columnCount - 17).Width = 8.0 + 5.0 / 7.0;
                    ws.Column(columnCount - 18).Width = 8.0 + 5.0 / 7.0;
                    ws.Column(columnCount - 19).Width = 7.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 20).Width = 7.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 21).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 22).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 23).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 24).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 25).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 26).Width = 5.0 + 5.0 / 7.0;
                    ws.Column(columnCount - 27).Width = 5.0 + 5.0 / 7.0;
                    ws.Column(columnCount - 28).Width = 5.0 + 5.0 / 7.0;
                    ws.Column(columnCount - 29).Width = 8.5 + 5.0 / 7.0;
                    ws.Column(columnCount - 30).Width = 6 + 5.0 / 7.0;

                    if (!string.IsNullOrEmpty(kvp.Value.Title))
                        ws.Cells[1, 1].Value = kvp.Value.Title;
                    if (!string.IsNullOrEmpty(kvp.Value.Header1))
                        ws.Cells[2, 1].Value = kvp.Value.Header1;
                    if (!string.IsNullOrEmpty(kvp.Value.Header2))
                        ws.Cells[3, 1].Value = kvp.Value.Header2;
                    if (!string.IsNullOrEmpty(kvp.Value.Header3))
                        ws.Cells[4, 1].Value = kvp.Value.Header3;

                    // Save total values
                    ws.Cells[1, columnCount - 19, 4, columnCount - 19].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[1, columnCount - 18, 4, columnCount - 17].Style.Font.Bold = true;

                    ws.Cells[1, columnCount - 19].Value = $"{kvp.Value.Table[0][columnCount - 30]}, max/min/avg):";
                    ws.Cells[1, columnCount - 18].Formula = $"SUBTOTAL(104,{kvp.Key}[{kvp.Value.Table[0][columnCount - 30]}])";
                    ws.Cells[1, columnCount - 17].Formula = $"SUBTOTAL(105,{kvp.Key}[{kvp.Value.Table[0][columnCount - 30]}])";
                    ws.Cells[1, columnCount - 16].Formula = $"SUBTOTAL(101,{kvp.Key}[{kvp.Value.Table[0][columnCount - 30]}])";

                    ws.Cells[2, columnCount - 19].Value = $"{kvp.Value.Table[0][columnCount - 26]}, max/min/avg):";
                    ws.Cells[2, columnCount - 18].Formula = $"SUBTOTAL(104,{kvp.Key}[{kvp.Value.Table[0][columnCount - 26]}])";
                    ws.Cells[2, columnCount - 17].Formula = $"SUBTOTAL(105,{kvp.Key}[{kvp.Value.Table[0][columnCount - 26]}])";
                    ws.Cells[2, columnCount - 16].Formula = $"SUBTOTAL(101,{kvp.Key}[{kvp.Value.Table[0][columnCount - 26]}])";

                    ws.Cells[3, columnCount - 19].Value = $"{kvp.Value.Table[0][columnCount - 25]}, max/min/avg):";
                    ws.Cells[3, columnCount - 18].Formula = $"SUBTOTAL(104,{kvp.Key}[{kvp.Value.Table[0][columnCount - 25]}])";
                    ws.Cells[3, columnCount - 17].Formula = $"SUBTOTAL(105,{kvp.Key}[{kvp.Value.Table[0][columnCount - 25]}])";
                    ws.Cells[3, columnCount - 16].Formula = $"SUBTOTAL(101,{kvp.Key}[{kvp.Value.Table[0][columnCount - 25]}])";

                    ws.Cells[4, columnCount - 19].Value = $"{kvp.Value.Table[0][columnCount - 24]}, max/min/avg):";
                    ws.Cells[4, columnCount - 18].Formula = $"SUBTOTAL(104,{kvp.Key}[{kvp.Value.Table[0][columnCount - 24]}])";
                    ws.Cells[4, columnCount - 17].Formula = $"SUBTOTAL(105,{kvp.Key}[{kvp.Value.Table[0][columnCount - 24]}])";
                    ws.Cells[4, columnCount - 16].Formula = $"SUBTOTAL(101,{kvp.Key}[{kvp.Value.Table[0][columnCount - 24]}])";

                    SetColor(ws.Cells[1, columnCount - 18], excelColors["Yellow"]);
                    SetColor(ws.Cells[1, columnCount - 17], excelColors["Yellow"]);
                    SetColor(ws.Cells[2, columnCount - 18], excelColors["Yellow"]);
                    SetColor(ws.Cells[3, columnCount - 18], excelColors["Yellow"]);
                    SetColor(ws.Cells[4, columnCount - 18], excelColors["Yellow"]);

                    ws.View.FreezePanes(6, columnCount - 30);

                    if (wsSummary != null)
                    {
                        wsSummary.Cells[summaryRow, 1].Value = kvp.Key;
                        wsSummary.Cells[summaryRow, 2].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 18)}2";
                        wsSummary.Cells[summaryRow, 3].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 17)}2";
                        wsSummary.Cells[summaryRow, 4].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 16)}2";

                        wsSummary.Cells[summaryRow, 5].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 18)}3";
                        wsSummary.Cells[summaryRow, 6].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 17)}3";
                        wsSummary.Cells[summaryRow, 7].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 16)}3";

                        wsSummary.Cells[summaryRow, 8].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 18)}4";
                        wsSummary.Cells[summaryRow, 9].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 17)}4";
                        wsSummary.Cells[summaryRow, 10].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 16)}4";

                        wsSummary.Cells[summaryRow, 11].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 18)}1";
                        wsSummary.Cells[summaryRow, 12].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 17)}1";
                        wsSummary.Cells[summaryRow, 13].Formula = $"{kvp.Key}!{OfficeOpenXml.ExcelCellAddress.GetColumnLetter(columnCount - 16)}1";

                        summaryRow++;
                    }

                    if (kvp.Value.Table.Count > 2)
                    {
                        SetFormatting(ws, ws.Cells[6, columnCount - 23, kvp.Value.Table.Count + 4, columnCount - 23]);
                        SetFormatting(ws, ws.Cells[6, columnCount - 24, kvp.Value.Table.Count + 4, columnCount - 24]);
                        SetFormatting(ws, ws.Cells[6, columnCount - 25, kvp.Value.Table.Count + 4, columnCount - 25]);
                        SetFormatting2(ws, ws.Cells[6, columnCount - 29, kvp.Value.Table.Count + 4, columnCount - 29]);
                    }
                }

                if (wsSummary != null)
                {
                    for (var k1 = 0; k1 < 4; k1++)
                    for (var k2 = 0; k2 < 3; k2++)
                    {
                        var a = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(k1 * 3 + k2 + 2);
                        wsSummary.Cells[$"{a}{summaryRowOffset + 2}"].Formula = $"MAX({a}{summaryRowOffset + 5}:{a}{summaryRow - 1})";
                        wsSummary.Cells[$"{a}{summaryRowOffset + 3}"].Formula = $"MIN({a}{summaryRowOffset + 5}:{a}{summaryRow - 1})";
                        wsSummary.Cells[$"{a}{summaryRowOffset + 4}"].Formula = $"AVERAGE({a}{summaryRowOffset + 5}:{a}{summaryRow - 1})";
                    }

                    SetBorder(wsSummary.Cells[$"A{summaryRowOffset}:M{summaryRowOffset + 1}"]);
                    wsSummary.Cells[summaryRowOffset + 2, 1, summaryRowOffset + 4, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    wsSummary.Cells[summaryRowOffset + 5, 1, summaryRow - 1, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    wsSummary.Cells[summaryRowOffset + 2, 2, summaryRow - 1, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSummary.Cells[summaryRowOffset + 2, 5, summaryRow - 1, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSummary.Cells[summaryRowOffset + 2, 8, summaryRow - 1, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSummary.Cells[summaryRowOffset + 2, 11, summaryRow - 1, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    wsSummary.Column(1).AutoFit();

                    if (!string.IsNullOrEmpty(summaryHeader)) wsSummary.Cells["A2"].Value = summaryHeader;
                    if (!string.IsNullOrEmpty(summarySubheader)) wsSummary.Cells["A3"].Value = summarySubheader;

                    SetColor(wsSummary.Cells[$"B{summaryRowOffset + 2}"], excelColors["Yellow"]);
                    SetColor(wsSummary.Cells[$"E{summaryRowOffset + 2}"], excelColors["Yellow"]);
                    SetColor(wsSummary.Cells[$"H{summaryRowOffset + 2}"], excelColors["Yellow"]);
                    SetColor(wsSummary.Cells[$"K{summaryRowOffset + 2}"], excelColors["Yellow"]);
                    SetColor(wsSummary.Cells[$"L{summaryRowOffset + 3}"], excelColors["Yellow"]);
                }

                excelPackage.SaveAs(new FileInfo(fileName));
            }

            void SetBorder(ExcelRange cells)
            {
                cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            void SetColor(ExcelRange cells, Color color)
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(color);
            }
            void SetFormatting(ExcelWorksheet ws, ExcelRange cells)
            {
                var buyRule = ws.ConditionalFormatting.AddThreeColorScale(cells);
                buyRule.LowValue.Color = Color.FromArgb(0xF8, 0x69, 0x6B);
                buyRule.MiddleValue.Color = Color.FromArgb(0xFF, 0xEB, 0x84);
                buyRule.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Percentile;
                buyRule.MiddleValue.Value = 50;
                buyRule.HighValue.Color = Color.FromArgb(0x63, 0xBE, 0x7F);
            }
            void SetFormatting2(ExcelWorksheet ws, ExcelRange cells)
            {
                var buyRule = ws.ConditionalFormatting.AddThreeColorScale(cells);
                buyRule.LowValue.Color = Color.FromArgb(0x63, 0xBE, 0x7F);
                buyRule.MiddleValue.Color = Color.FromArgb(0xFF, 0xEB, 0x84);
                buyRule.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                buyRule.MiddleValue.Value = 0;
                buyRule.HighValue.Color = Color.FromArgb(0x63, 0xBE, 0x7F);
            }
        }
    }
}
