using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Microsoft.Data.SqlClient; // Required for SQL Server namespace
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Color = System.Drawing.Color;

namespace WebApplication1.Controllers
{
    public class AirportController : Controller
    {
        private readonly SqlServerDb _db;

        public AirportController(IConfiguration configuration)
        {
            _db = new SqlServerDb(configuration);
        }

        // ??????: ????? ?? ???? ????? ?????? ?? SqlServerDb.cs ???? ???????? ?? ???????
        private List<Airport> GetAirports(string airportId, string airportName, string countryName, string icaoCode)
        {
            // ??? ???? ????? ??? ???? ???? ??? ??????? ?? SqlServerDb.cs
            // ??????? ??????? ???? ???? ?? ???? ??? SQL
            var airports = new List<Airport>();
            const string baseSql = @"
                SELECT a.airportid, a.airportname, a.countryid, c.countryname, a.icao_code
                FROM airports a
                JOIN countries c ON a.countryid = c.countryid";

            // ??? ????? ???? ???? WHERE ?????????? ????? ??? ???????
            // ??? ???????? ????? ???? ???? ?? ??????? ?? ?????? ???? ??? ????? ?????? ??????

            DataTable dt = _db.ExecuteQuery(baseSql);

            foreach (DataRow row in dt.Rows)
            {
                airports.Add(new Airport
                {
                    AirportId = Convert.ToInt32(row["airportid"]),
                    AirportName = row["airportname"].ToString(),
                    CountryId = Convert.ToInt32(row["countryid"]),
                    CountryName = row["countryname"].ToString(),
                    IcaoCode = row["icao_code"].ToString()
                });
            }

            // ????? ??????? ???
            if (!string.IsNullOrEmpty(airportId))
                airports = airports.Where(a => a.AirportId.ToString().Contains(airportId)).ToList();
            if (!string.IsNullOrEmpty(airportName))
                airports = airports.Where(a => a.AirportName.ToLower().Contains(airportName.ToLower())).ToList();
            if (!string.IsNullOrEmpty(countryName))
                airports = airports.Where(a => a.CountryName.ToLower().Contains(countryName.ToLower())).ToList();
            if (!string.IsNullOrEmpty(icaoCode))
                airports = airports.Where(a => a.IcaoCode.ToLower().Contains(icaoCode.ToLower())).ToList();

            return airports;
        }


        public JsonResult GetAirportsByCountry(int countryId)
        {
            var dtAirports = _db.ExecuteQuery(
                "SELECT airportid, airportname, icao_code FROM airports WHERE countryid = @countryId ORDER BY airportname",
                new SqlParameter("@countryId", countryId));

            var airports = dtAirports.Rows.Cast<DataRow>()
                .Select(r => new Dictionary<string, string>
                {
                    {"id", r["airportid"].ToString()},
                    {"name", $"{r["airportname"]} ({r["icao_code"]})"}
                })
                .ToList();

            return Json(airports);
        }

        private List<SelectListItem> GetCountrySelectList()
        {
            var dt = _db.ExecuteQuery("SELECT countryid, countryname FROM countries ORDER BY countryname");
            return dt.Rows.Cast<DataRow>()
                         .Select(r => new SelectListItem
                         {
                             Value = r["countryid"].ToString(),
                             Text = r["countryname"].ToString()
                         })
                         .ToList();
        }

        // GET: /Airport
        public IActionResult Index(string airportId, string airportName, string countryName, string icaoCode)
        {
            var airports = GetAirports(airportId, airportName, countryName, icaoCode);
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(airports);
        }

        // GET: /Airport/Create
        public IActionResult Create()
        {
            ViewBag.CountryList = GetCountrySelectList();
            return View();
        }

        // POST: /Airport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Airport model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = GetCountrySelectList();
                return View(model);
            }

            const string sql = @"
            INSERT INTO airports(airportname, countryid, icao_code)
            VALUES(@name, @countryId, @IcaoCode)";

            _db.ExecuteNonQuery(sql,
                new SqlParameter("@name", model.AirportName),
                new SqlParameter("@countryId", model.CountryId),
                new SqlParameter("@IcaoCode", model.IcaoCode));

            TempData["SuccessMessage"] = "Division has been added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Airport/Edit/5
        public IActionResult Edit(int id)
        {
            DataTable dt = _db.ExecuteQuery(
                "SELECT airportid, airportname, countryid, icao_code FROM airports WHERE airportid = @id",
                new SqlParameter("@id", id));

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var model = new Airport
            {
                AirportId = Convert.ToInt32(row["airportid"]),
                AirportName = row["airportname"].ToString(),
                CountryId = Convert.ToInt32(row["countryid"]),
                IcaoCode = row["icao_code"].ToString()
            };

            ViewBag.CountryList = GetCountrySelectList();
            return View(model);
        }

        // POST: /Airport/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Airport model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = GetCountrySelectList();
                return View(model);
            }

            const string sql = @"
            UPDATE airports
            SET airportname = @name,
                countryid = @countryId,
                icao_code = @IcaoCode
            WHERE airportid = @id";

            _db.ExecuteNonQuery(sql,
                new SqlParameter("@name", model.AirportName),
                new SqlParameter("@countryId", model.CountryId),
                new SqlParameter("@IcaoCode", model.IcaoCode),
                new SqlParameter("@id", model.AirportId));

            TempData["SuccessMessage"] = "Division details have been updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Airport/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _db.ExecuteNonQuery(
                "DELETE FROM airports WHERE airportid = @id",
                new SqlParameter("@id", id));

            TempData["SuccessMessage"] = "Division has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // --- EXPORT ACTIONS ---

        public IActionResult ExportToPDF(string airportId, string airportName, string countryName, string icaoCode)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var divisions = GetAirports(airportId, airportName, countryName, icaoCode);
            var recordCount = divisions.Count;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
                            if (System.IO.File.Exists(logoPath))
                            {
                                row.ConstantColumn(70).Image(logoPath);
                            }

                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().AlignCenter().Text("???? ????? ??????? ?????? ???????").Bold().FontSize(12);
                                col.Item().AlignCenter().Text("JORDAN CIVIL AVIATION REGULATORY COMMISSION").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().PaddingTop(5).AlignCenter().Text($"Divisions Report - {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken2);
                            });
                        });
                        headerCol.Item().PaddingTop(10);
                        headerCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        headerCol.Item().PaddingTop(5);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1f); // ID
                            columns.RelativeColumn(3f); // Name
                            columns.RelativeColumn(2.5f); // Country
                            columns.RelativeColumn(1.5f); // Abbreviation
                        });

                        table.Header(header =>
                        {
                            static IContainer HeaderStyle(IContainer c) => c.Background(Colors.Blue.Medium).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(9).Bold());
                            header.Cell().Element(HeaderStyle).Text("Division ID");
                            header.Cell().Element(HeaderStyle).Text("Division Name");
                            header.Cell().Element(HeaderStyle).Text("Organizational Structure");
                            header.Cell().Element(HeaderStyle).Text("Abbreviation");
                        });

                        foreach (var division in divisions)
                        {
                            static IContainer BodyCellStyle(IContainer c) => c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).DefaultTextStyle(x => x.FontSize(8));
                            table.Cell().Element(BodyCellStyle).Text(division.AirportId.ToString());
                            table.Cell().Element(BodyCellStyle).Text(division.AirportName);
                            table.Cell().Element(BodyCellStyle).Text(division.CountryName);
                            table.Cell().Element(BodyCellStyle).Text(division.IcaoCode);
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn().Text(txt =>
                        {
                            txt.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                            txt.Span($"Total Records: {recordCount}");
                        });
                        row.RelativeColumn().AlignRight().Text(txt =>
                        {
                            txt.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                            txt.Span("Page ");
                            txt.CurrentPageNumber();
                            txt.Span(" of ");
                            txt.TotalPages();
                        });
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Divisions_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }


        public IActionResult ExportToExcel(string airportId, string airportName, string countryName, string icaoCode)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var divisions = GetAirports(airportId, airportName, countryName, icaoCode);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Divisions");
                worksheet.Cells.Style.Font.Name = "Arial";

                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                    excelImage.SetPosition(0, 0, 0, 15);
                    excelImage.SetSize(120, 65);
                }

                worksheet.Cells["C1"].Value = "???? ????? ??????? ?????? ???????";
                worksheet.Cells["C1"].Style.Font.Bold = true;
                worksheet.Cells["C1"].Style.Font.Size = 14;
                worksheet.Cells["C1:F1"].Merge = true;
                worksheet.Cells["C1:F1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["C2"].Value = "JORDAN CIVIL AVIATION REGULATORY COMMISSION";
                worksheet.Cells["C2"].Style.Font.Size = 10;
                worksheet.Cells["C2:F2"].Merge = true;
                worksheet.Cells["C2:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["C3"].Value = $"Divisions Report - {DateTime.Now:yyyy-MM-dd}";
                worksheet.Cells["C3"].Style.Font.Size = 9;
                worksheet.Cells["C3"].Style.Font.Italic = true;
                worksheet.Cells["C3:F3"].Merge = true;
                worksheet.Cells["C3:F3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var headers = new string[] { "Division ID", "Division Name", "Organizational Structure", "Abbreviation" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = headers[i];
                }

                using (var range = worksheet.Cells[5, 1, 5, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4F81BD"));
                    range.Style.Font.Color.SetColor(Color.White);
                }

                int row = 6;
                foreach (var division in divisions)
                {
                    worksheet.Cells[row, 1].Value = division.AirportId;
                    worksheet.Cells[row, 2].Value = division.AirportName;
                    worksheet.Cells[row, 3].Value = division.CountryName;
                    worksheet.Cells[row, 4].Value = division.IcaoCode;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Divisions_List_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }
}

