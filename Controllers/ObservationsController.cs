using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;
using Color = System.Drawing.Color;

namespace WebApplication1.Controllers
{
    [Authorize]
public class ObservationsController : Controller // Make sure the name ends with 's'
    {
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAdvancedPermissionManagerService _permissionService;

        public ObservationsController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IAdvancedPermissionManagerService permissionService)
        {
            _db = new SqlServerDb(configuration);
            _webHostEnvironment = webHostEnvironment;
            _permissionService = permissionService;
        }

        // GET: /Observations/
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized();

                if (!await _permissionService.CanPerformOperationAsync(userId, "Observation", "View"))
                {
                    return Forbid();
                }
                var viewModel = new ObservationIndexViewModel
                {
                    ControllerObservations = _db.GetControllerObservations(),
                    EmployeesAndOpsStaffObservations = _db.GetAllEmployeeObservations()
                };
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while fetching data: {ex.Message}";
                return View(new ObservationIndexViewModel());
            }
        }

        // GET: /Observations/Create
        public IActionResult Create(string tab = "controllers")
        {
            LoadDropdowns();
            ViewBag.ActiveTab = tab;
            return View(new Observation());
        }

        // POST: /Observations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Observation observation, IFormFile UploadFile)
        {
            if (!observation.ControllerId.HasValue && !observation.EmployeeId.HasValue)
            {
                ModelState.AddModelError("", "You must select either a Controller or an Employee.");
            }

            ModelState.Remove(nameof(observation.FilePath));
            ModelState.Remove(nameof(observation.UploadFile));
            ModelState.Remove(nameof(observation.ObservationNo));

            if (ModelState.IsValid)
            {
                string personIdentifier = null;
                string personTypeFolder = "";

                if (observation.ControllerId.HasValue)
                {
                    var controllerTable = _db.GetControllerById(observation.ControllerId.Value);
                    if (controllerTable != null && controllerTable.Rows.Count > 0)
                    {
                        personIdentifier = controllerTable.Rows[0]["Username"].ToString();
                        personTypeFolder = "controllers";
                    }
                }
                else if (observation.EmployeeId.HasValue)
                {
                    var employee = _db.GetEmployeeById(observation.EmployeeId.Value);
                    if (employee != null)
                    {
                        personIdentifier = employee.Username;
                        personTypeFolder = "employees";
                    }
                }

                if (UploadFile != null && UploadFile.Length > 0)
                {
                    if (string.IsNullOrEmpty(personIdentifier))
                    {
                        ModelState.AddModelError("", "Cannot upload file because the associated person was not found.");
                    }
                    else
                    {
                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "observations", personTypeFolder, personIdentifier);
                        Directory.CreateDirectory(uploadsFolder);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await UploadFile.CopyToAsync(stream);
                        }
                        observation.FilePath = $"/observations/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                    }
                }

                observation.ObservationNo = _db.GetNextObservationNumberForPerson(observation.ControllerId, observation.EmployeeId);

                if (ModelState.ErrorCount == 0)
                {
                    string sql = @"
                        INSERT INTO Observations (ControllerId, EmployeeId, ObservationNo, FlightNo, Duration_Days, TravelCountry, DepartDate, ReturnDate, LicenseNumber, FilePath, Notes)
                        VALUES (@ControllerId, @EmployeeId, @ObservationNo, @FlightNo, @DurationDays, @TravelCountry, @DepartDate, @ReturnDate, @LicenseNumber, @FilePath, @Notes)";

                    var parameters = new[]
                    {
                        new SqlParameter("@ControllerId", (object)observation.ControllerId ?? DBNull.Value),
                        new SqlParameter("@EmployeeId", (object)observation.EmployeeId ?? DBNull.Value),
                        new SqlParameter("@ObservationNo", (object)observation.ObservationNo ?? DBNull.Value),
                        new SqlParameter("@FlightNo", (object)observation.FlightNo ?? DBNull.Value),
                        new SqlParameter("@DurationDays", (object)observation.DurationDays ?? DBNull.Value),
                        new SqlParameter("@TravelCountry", (object)observation.TravelCountry ?? string.Empty),
                        new SqlParameter("@DepartDate", (object)observation.DepartDate ?? DBNull.Value),
                        new SqlParameter("@ReturnDate", (object)observation.ReturnDate ?? DBNull.Value),
                        new SqlParameter("@LicenseNumber", (object)observation.LicenseNumber ?? string.Empty),
                        new SqlParameter("@FilePath", (object)observation.FilePath ?? DBNull.Value),
                        new SqlParameter("@Notes", (object)observation.Notes ?? DBNull.Value)
                    };

                    try
                    {
                        _db.ExecuteNonQuery(sql, parameters);
                        TempData["SuccessMessage"] = "Observation added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Database operation failed: {ex.Message}");
                    }
                }
            }

            LoadDropdowns();
            return View(observation);
        }

        // GET: Observation/Edit/5
        public IActionResult Edit(int id)
        {
            var observation = _db.GetObservationById(id);
            if (observation == null)
            {
                return NotFound();
            }
            return View(observation);
        }

        // POST: Observation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Observation observation, IFormFile UploadFile)
        {
            if (id != observation.ObservationId)
            {
                return BadRequest();
            }

            // --- ??? ?? ???? ?????? ????? ???????? ---
            // ???? ???? ?????? ?? ??? ???? ????? ???? ??? ?????? ???? ???????
            ModelState.Remove(nameof(observation.UploadFile));
            ModelState.Remove("UploadFile"); // As a safeguard

            // Also remove fields that are not meant to be edited
            ModelState.Remove(nameof(observation.PersonName));
            ModelState.Remove(nameof(observation.ControllerName));
            ModelState.Remove(nameof(observation.EmployeeName));
            ModelState.Remove(nameof(observation.ObservationNo));

            if (ModelState.IsValid)
            {
                var existingObs = _db.GetObservationById(id);
                if (existingObs == null)
                {
                    return NotFound();
                }
                string currentFilePath = existingObs.FilePath;

                // If a new file is uploaded, process it
                if (UploadFile != null && UploadFile.Length > 0)
                {
                    string personIdentifier = null;
                    string personTypeFolder = "";

                    if (existingObs.ControllerId.HasValue)
                    {
                        var controller = _db.GetControllerById(existingObs.ControllerId.Value);
                        if (controller != null && controller.Rows.Count > 0)
                        {
                            personIdentifier = controller.Rows[0]["Username"].ToString();
                            personTypeFolder = "controllers";
                        }
                    }
                    else if (existingObs.EmployeeId.HasValue)
                    {
                        var employee = _db.GetEmployeeById(existingObs.EmployeeId.Value);
                        if (employee != null)
                        {
                            personIdentifier = employee.Username;
                            personTypeFolder = "employees";
                        }
                    }

                    if (!string.IsNullOrEmpty(personIdentifier))
                    {
                        // Delete old file if it exists
                        if (!string.IsNullOrEmpty(currentFilePath))
                        {
                            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, currentFilePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Save new file
                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "observations", personTypeFolder, personIdentifier);
                        Directory.CreateDirectory(uploadsFolder);
                        string newPhysicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(newPhysicalPath, FileMode.Create))
                        {
                            await UploadFile.CopyToAsync(stream);
                        }
                        currentFilePath = $"/observations/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                    }
                }

                // Update the database record
                const string sql = @"
            UPDATE dbo.Observations SET 
                FlightNo = @FlightNo, 
                Duration_Days = @DurationDays, 
                TravelCountry = @TravelCountry, 
                DepartDate = @DepartDate, 
                ReturnDate = @ReturnDate, 
                LicenseNumber = @LicenseNumber, 
                FilePath = @FilePath, 
                Notes = @Notes
            WHERE ObservationId = @ObservationId";

                var parameters = new[]
                {
            new SqlParameter("@FlightNo", (object)observation.FlightNo ?? DBNull.Value),
            new SqlParameter("@DurationDays", (object)observation.DurationDays ?? DBNull.Value),
            new SqlParameter("@TravelCountry", (object)observation.TravelCountry ?? string.Empty),
            new SqlParameter("@DepartDate", (object)observation.DepartDate ?? DBNull.Value),
            new SqlParameter("@ReturnDate", (object)observation.ReturnDate ?? DBNull.Value),
            new SqlParameter("@LicenseNumber", (object)observation.LicenseNumber ?? string.Empty),
            new SqlParameter("@FilePath", (object)currentFilePath ?? DBNull.Value),
            new SqlParameter("@Notes", (object)observation.Notes ?? DBNull.Value),
            new SqlParameter("@ObservationId", id)
        };

                try
                {
                    _db.ExecuteNonQuery(sql, parameters);
                    TempData["SuccessMessage"] = "Observation updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Database update failed: {ex.Message}");
                }
            }

            // If model state is invalid, repopulate the person name and return to the view
            var person = _db.GetObservationById(id);
            if (person != null)
            {
                observation.ControllerName = person.ControllerName;
                observation.EmployeeName = person.EmployeeName;
            }
            return View(observation);
        }
        // POST: Observation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _db.DeleteObservationById(id);
            TempData["SuccessMessage"] = "Observation deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Controllers = _db.ExecuteQuery("SELECT ControllerId, FullName FROM dbo.Controllers ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["ControllerId"].ToString(), Text = row["FullName"].ToString() }).ToList();
            ViewBag.Employees = _db.ExecuteQuery("SELECT EmployeeID, FullName FROM dbo.Employees ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["EmployeeID"].ToString(), Text = row["FullName"].ToString() }).ToList();
        }


        public IActionResult ExportToPDF(string personType, string observationNo, string personName, string flightNo, string travelCountry)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            List<Observation> observations = new List<Observation>();
            string reportTitle = "";

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ExportToPDF - personType: {personType}, observationNo: {observationNo}, personName: {personName}, flightNo: {flightNo}, travelCountry: {travelCountry}");

            try
            {
                switch (personType)
                {
                    case "Controller":
                        observations = _db.GetControllerObservations();
                        reportTitle = "Controller Observations Report";
                        break;
                    case "Employees":
                        observations = _db.GetAllEmployeeObservations();
                        reportTitle = "Employees & Operation Staff Observations Report";
                        break;
                    default: // Legacy support for old individual tabs
                        observations = _db.GetAllEmployeeObservations();
                        reportTitle = "Employees & Operation Staff Observations Report";
                        break;
                }

                // Apply filters
                if (!string.IsNullOrEmpty(personName))
                {
                    if (personType == "Controller")
                    {
                        observations = observations.Where(o => o.ControllerName != null && o.ControllerName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        observations = observations.Where(o => o.EmployeeName != null && o.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(observationNo))
                {
                    observations = observations.Where(o => o.ObservationNo.ToString().Contains(observationNo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(flightNo))
                {
                    observations = observations.Where(o => o.FlightNo != null && o.FlightNo.Contains(flightNo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(travelCountry))
                {
                    observations = observations.Where(o => o.TravelCountry != null && o.TravelCountry.Contains(travelCountry, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Debug logging for filtered results
                System.Diagnostics.Debug.WriteLine($"ExportToPDF - Final filtered count: {observations.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportToPDF - Error: {ex.Message}");
                return BadRequest($"Error processing export: {ex.Message}");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8));

                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "carc.png");
                            if (System.IO.File.Exists(logoPath)) row.ConstantColumn(70).Image(logoPath);

                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().AlignCenter().Text("???? ????? ??????? ?????? ???????").Bold().FontSize(12);
                                col.Item().AlignCenter().Text("JORDAN CIVIL AVIATION REGULATORY COMMISSION").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().PaddingTop(5).AlignCenter().Text($"{reportTitle} - {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken2);
                            });
                        });
                        headerCol.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().Table(table =>
                    {
                        static IContainer HeaderStyle(IContainer c) => c.Background(Colors.Blue.Medium).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(9).Bold());
                        static IContainer BodyCellStyle(IContainer c) => c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);

                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Obs#");
                            header.Cell().Element(HeaderStyle).Text(personType == "Controller" ? "Controller" : "Employee");
                            header.Cell().Element(HeaderStyle).Text("Flight No.");
                            header.Cell().Element(HeaderStyle).Text("Country");
                            header.Cell().Element(HeaderStyle).Text("Days");
                            header.Cell().Element(HeaderStyle).Text("Depart Date");
                            header.Cell().Element(HeaderStyle).Text("Return Date");
                        });

                        foreach (var item in observations)
                        {
                            table.Cell().Element(BodyCellStyle).Text(item.ObservationNo.ToString());
                            table.Cell().Element(BodyCellStyle).Text(item.PersonName);
                            table.Cell().Element(BodyCellStyle).Text(item.FlightNo);
                            table.Cell().Element(BodyCellStyle).Text(item.TravelCountry);
                            table.Cell().Element(BodyCellStyle).Text(item.DurationDays.ToString());
                            table.Cell().Element(BodyCellStyle).Text(item.DepartDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(BodyCellStyle).Text(item.ReturnDate?.ToString("yyyy-MM-dd"));
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn().Text($"Total Records: {observations.Count}").FontSize(8);
                        row.RelativeColumn().AlignRight().Text(txt =>
                        {
                            txt.DefaultTextStyle(x => x.FontSize(8));
                            txt.Span("Page ");
                            txt.CurrentPageNumber();
                            txt.Span(" of ");
                            txt.TotalPages();
                        });
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"{personType}_Observations_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportToExcel(string personType, string observationNo, string personName, string flightNo, string travelCountry)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<Observation> observations = new List<Observation>();
            string reportTitle = "";
            string[] headers;

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ExportToExcel - personType: {personType}, observationNo: {observationNo}, personName: {personName}, flightNo: {flightNo}, travelCountry: {travelCountry}");

            try
            {
                switch (personType)
                {
                    case "Controller":
                        observations = _db.GetControllerObservations();
                        reportTitle = "Controller Observations Report";
                        headers = new string[] { "#", "Controller", "Obs#", "Flight No.", "Country", "Days", "Depart Date", "Return Date", "License Number", "Notes" };
                        break;
                    case "Employees":
                        observations = _db.GetAllEmployeeObservations();
                        reportTitle = "Employees & Operation Staff Observations Report";
                        headers = new string[] { "#", "Employee", "Department", "Obs#", "Flight No.", "Country", "Days", "Depart Date", "Return Date", "License Number", "Notes" };
                        break;
                    default: // Legacy support for old individual tabs
                        observations = _db.GetAllEmployeeObservations();
                        reportTitle = "Employees & Operation Staff Observations Report";
                        headers = new string[] { "#", "Employee", "Department", "Obs#", "Flight No.", "Country", "Days", "Depart Date", "Return Date", "License Number", "Notes" };
                        break;
                }

                // Apply filters
                if (!string.IsNullOrEmpty(personName))
                {
                    if (personType == "Controller")
                    {
                        observations = observations.Where(o => o.ControllerName != null && o.ControllerName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        observations = observations.Where(o => o.EmployeeName != null && o.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(observationNo))
                {
                    observations = observations.Where(o => o.ObservationNo.ToString().Contains(observationNo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(flightNo))
                {
                    observations = observations.Where(o => o.FlightNo != null && o.FlightNo.Contains(flightNo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(travelCountry))
                {
                    observations = observations.Where(o => o.TravelCountry != null && o.TravelCountry.Contains(travelCountry, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Debug logging for filtered results
                System.Diagnostics.Debug.WriteLine($"ExportToExcel - Final filtered count: {observations.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportToExcel - Error: {ex.Message}");
                return BadRequest($"Error processing export: {ex.Message}");
            }

            // 2. ????? ??? ?????? ???????
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Observations");
                worksheet.Cells.Style.Font.Name = "Arial";

                // --- ????? ?????? ????????? ---
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "carc.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                    excelImage.SetPosition(0, 0, 0, 15);
                    excelImage.SetSize(120, 65);
                }

                worksheet.Cells["C1"].Value = "???? ????? ??????? ?????? ???????";
                worksheet.Cells["C1"].Style.Font.Bold = true;
                worksheet.Cells["C1"].Style.Font.Size = 14;
                worksheet.Cells[1, 3, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 3, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["C2"].Value = reportTitle;
                worksheet.Cells["C2"].Style.Font.Size = 10;
                worksheet.Cells[2, 3, 2, headers.Length].Merge = true;
                worksheet.Cells[2, 3, 2, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // --- ????? ??? ?????? ---
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

                // --- ????? ???????? ---
                int row = 6;
                int index = 1;
                foreach (var item in observations)
                {
                    worksheet.Cells[row, 1].Value = index++;
                    worksheet.Cells[row, 2].Value = item.PersonName;
                    worksheet.Cells[row, 3].Value = item.ObservationNo;
                    worksheet.Cells[row, 4].Value = item.FlightNo;
                    worksheet.Cells[row, 5].Value = item.TravelCountry;
                    worksheet.Cells[row, 6].Value = item.DurationDays;
                    worksheet.Cells[row, 7].Value = item.DepartDate;
                    worksheet.Cells[row, 8].Value = item.ReturnDate;
                    worksheet.Cells[row, 9].Value = item.LicenseNumber;
                    worksheet.Cells[row, 10].Value = item.Notes;

                    // ????? ???????
                    worksheet.Cells[row, 7, row, 8].Style.Numberformat.Format = "yyyy-mm-dd";
                    row++;
                }

                // ??? ??? ??????? ????????
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{personType}_Observations_List_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }


        private string GetLicenseNumberForController(int controllerId, int ObservationId)
        {
            var controller = _db.GetControllerById(controllerId);
            var Observation = _db.GetObservationById(ObservationId);
            if (controller.Rows.Count > 0 && controller.Rows[0]["licensenumber"] != DBNull.Value)
            {
                return controller.Rows[0]["licensenumber"].ToString();
            }
            return ""; // ?? ????? ????? null
        }
        private string GetLatestLicenseNumberForController(int controllerId)
        {
            // ?? ????? ????????? ??????? ?? ????? SQL Server:
            // - :ctrlId ????? @ctrlId
            // - LENGTH(LicenseNumber) ????? LEN(LicenseNumber)
            // - FETCH FIRST 1 ROW ONLY ????? TOP 1
            const string sql = @"
        SELECT TOP 1 LicenseNumber
        FROM Observations
        WHERE ControllerId = @ctrlId AND LicenseNumber IS NOT NULL AND LEN(LicenseNumber) > 0
        ORDER BY ObservationId DESC";

            // ??????? Microsoft.Data.SqlClient.SqlParameter ????? ?? OracleParameter
            DataTable dt = _db.ExecuteQuery(sql, new Microsoft.Data.SqlClient.SqlParameter("@ctrlId", controllerId)); // <== ?? ???????

            if (dt.Rows.Count > 0 && dt.Rows[0]["LicenseNumber"] != DBNull.Value)
            {
                return dt.Rows[0]["LicenseNumber"].ToString();
            }
            return "";
        }


        //public IActionResult ExportObservationsToPDF(string filter = "")
        //{
        //    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        //    var observations = _db.GetObservations(filter);
        //    var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");

        //    var document = QuestPDF.Fluent.Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Size(PageSizes.A4.Landscape()); // <<== ??? ????? ???? ??????? ???? ????
        //            page.Margin(20); // ????? ??? ????? ????

        //            page.Header()
        //                .AlignCenter()
        //                .Column(col =>
        //                {
        //                    if (System.IO.File.Exists(logoPath))
        //                        col.Item().AlignCenter().Height(70).Image(logoPath, QuestPDF.Infrastructure.ImageScaling.FitHeight);

        //                    col.Item().PaddingTop(8);
        //                    col.Item().AlignCenter().Text("????? ????????? ????????")
        //                        .Bold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
        //                    col.Item().AlignCenter().Text(DateTime.Now.ToString("yyyy/MM/dd HH:mm")).FontSize(12).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
        //                });

        //            page.Content().PaddingTop(18).Table(table =>
        //            {
        //                table.ColumnsDefinition(columns =>
        //                {
        //                    columns.ConstantColumn(30);      // #
        //                    columns.RelativeColumn(2.2f);    // Controller
        //                    columns.RelativeColumn(1.7f);    // Country
        //                    columns.RelativeColumn(1.2f);    // Trips No
        //                    columns.RelativeColumn(1.2f);    // Days
        //                    columns.RelativeColumn(2.1f);    // Depart
        //                    columns.RelativeColumn(2.1f);    // Return
        //                    columns.RelativeColumn(2.1f);    // License Crew No
        //                    columns.RelativeColumn(1.0f);    // Obs#
        //                    columns.RelativeColumn(2.0f);    // Notes
        //                });

        //                table.Header(header =>
        //                {
        //                    header.Cell().Element(CellStyleHeader).Text("#");
        //                    header.Cell().Element(CellStyleHeader).Text("Controller");
        //                    header.Cell().Element(CellStyleHeader).Text("Country");
        //                    header.Cell().Element(CellStyleHeader).Text("Trips No");
        //                    header.Cell().Element(CellStyleHeader).Text("Days");
        //                    header.Cell().Element(CellStyleHeader).Text("Depart");
        //                    header.Cell().Element(CellStyleHeader).Text("Return");
        //                    header.Cell().Element(CellStyleHeader).Text("License Crew No");
        //                    header.Cell().Element(CellStyleHeader).Text("Obs#");
        //                    header.Cell().Element(CellStyleHeader).Text("Notes");
        //                });

        //                int index = 1;
        //                foreach (var o in observations)
        //                {
        //                    table.Cell().Element(CellStyleBody).Text(index++);
        //                    table.Cell().Element(CellStyleBody).Text(o.FullName ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.TravelCountry ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.TravelCount?.ToString() ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.DurationDays?.ToString() ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.DepartDate?.ToString("yyyy-MM-dd") ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.ReturnDate?.ToString("yyyy-MM-dd") ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.LicenseNumber ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.ObservationNo ?? "");
        //                    table.Cell().Element(CellStyleBody).Text(o.Notes ?? "");
        //                }
        //            });

        //            page.Footer()
        //                .AlignCenter()
        //                .PaddingTop(8)
        //                .Text("????? ???? - ???? ????? ???????? ?????? - " + DateTime.Now.ToString("yyyy/MM/dd"))
        //                .FontSize(10)
        //                .FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
        //        });
        //    });

        //    var pdfBytes = document.GeneratePdf();
        //    return File(pdfBytes, "application/pdf", "observations.pdf");

        //    // ??????? ??????
        //    IContainer CellStyleHeader(IContainer container) =>
        //        container
        //            .Background(QuestPDF.Helpers.Colors.Blue.Medium)
        //            .PaddingVertical(6).PaddingHorizontal(2)
        //            .DefaultTextStyle(x => x.FontColor(QuestPDF.Helpers.Colors.White).FontSize(14).SemiBold());

        //    IContainer CellStyleBody(IContainer container) =>
        //        container
        //            .BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
        //            .PaddingVertical(5).PaddingHorizontal(2)
        //            .DefaultTextStyle(x => x.FontSize(13).FontColor(QuestPDF.Helpers.Colors.Grey.Darken3));
        //}

        //public IActionResult ExportObservationsToExcel(string filter = "")
        //{
        //    OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //    var observations = _db.GetObservations(filter);
        //    using (var package = new OfficeOpenXml.ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Observations");
        //        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
        //        if (System.IO.File.Exists(logoPath))
        //        {
        //            var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
        //            excelImage.SetPosition(0, 0, 2, 0);
        //            excelImage.SetSize(130, 70);
        //        }

        //        worksheet.Cells[2, 2, 2, 9].Merge = true;
        //        worksheet.Cells[2, 2].Value = "????? ????????? ????????";
        //        worksheet.Cells[2, 2].Style.Font.Size = 18;
        //        worksheet.Cells[2, 2].Style.Font.Bold = true;
        //        worksheet.Cells[2, 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(33, 150, 243));
        //        worksheet.Cells[2, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[3, 2, 3, 9].Merge = true;
        //        worksheet.Cells[3, 2].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        //        worksheet.Cells[3, 2].Style.Font.Size = 11;
        //        worksheet.Cells[3, 2].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
        //        worksheet.Cells[3, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[5, 1].Value = "#";
        //        worksheet.Cells[5, 2].Value = "Controller";
        //        worksheet.Cells[5, 3].Value = "Country";
        //        worksheet.Cells[5, 4].Value = "Trips No";
        //        worksheet.Cells[5, 5].Value = "Days";
        //        worksheet.Cells[5, 6].Value = "Depart";
        //        worksheet.Cells[5, 7].Value = "Return";
        //        worksheet.Cells[5, 8].Value = "License Crew No";
        //        worksheet.Cells[5, 9].Value = "Obs#";
        //        worksheet.Cells[5, 10].Value = "Notes";

        //        using (var range = worksheet.Cells[5, 1, 5, 10])
        //        {
        //            range.Style.Font.Bold = true;
        //            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(33, 150, 243));
        //            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
        //            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //        }

        //        int row = 6;
        //        int index = 1;
        //        foreach (var o in observations)
        //        {
        //            worksheet.Cells[row, 1].Value = index++;
        //            worksheet.Cells[row, 2].Value = o.FullName;
        //            worksheet.Cells[row, 3].Value = o.TravelCountry;
        //            worksheet.Cells[row, 4].Value = o.TravelCount;
        //            worksheet.Cells[row, 5].Value = o.DurationDays;
        //            worksheet.Cells[row, 6].Value = o.DepartDate.HasValue ? o.DepartDate.Value.ToString("yyyy-MM-dd") : "";
        //            worksheet.Cells[row, 7].Value = o.ReturnDate.HasValue ? o.ReturnDate.Value.ToString("yyyy-MM-dd") : "";
        //            worksheet.Cells[row, 8].Value = o.LicenseNumber;
        //            worksheet.Cells[row, 9].Value = o.ObservationNo;
        //            worksheet.Cells[row, 10].Value = o.Notes;
        //            row++;
        //        }

        //        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //        worksheet.View.FreezePanes(6, 1);

        //        var excelBytes = package.GetAsByteArray();
        //        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "observations.xlsx");
        //    }
        //}

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
    }
    }
}





