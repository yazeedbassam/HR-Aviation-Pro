using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; // For IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient; // Required for SqlParameter
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data; // Required for DataTable, DataRow
using System.IO;
using WebApplication1.DataAccess; // Make sure SqlDb is accessible
using WebApplication1.Models; // Make sure Certificate model is accessible
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

using System.Drawing;

namespace WebApplication1.Controllers
{
    public class CertificateController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CertificateController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _db = new SqlServerDb(configuration);
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: /Certificate
        public IActionResult Index()
        {
            try
            {
                var viewModel = new CertificateIndexViewModel
                {
                    ControllerCertificates = _db.GetControllerCertificates(),
                    AISCertificates = _db.GetAISCertificates(),
                    CNSCertificates = _db.GetCNSCertificates(),
                    AFTNCertificates = _db.GetAFTNCertificates(),
                    OpsStaffCertificates = _db.GetOpsStaffCertificates()
                };

                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while fetching data: {ex.Message}";
                return View(new CertificateIndexViewModel());
            }
        }
        // --- ?????? ??? ?????? ?? CertificateController.cs ---

        // GET: Certificate/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            // Initialize the model with today's date for issue and expiry
            var model = new CertificateViewModel
            {
                IssueDate = DateTime.Today,
                ExpiryDate = DateTime.Today
            };
            return View(model);
        }

        // POST: Certificate/Create
        // --- ?????? ??? ?????? ?? CertificateController.cs ---

        // --- ?????? ??? ?????? ?? CertificateController.cs ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CertificateViewModel certificate)
        {
            // --- ??? ?? ???? ??????? ??????? ---
            // ???? ???? ?????? ?? ??? ???? ????? ???? ??? ?????? ???? ???????
            ModelState.Remove(nameof(certificate.UploadFile));

            // ?????? ?? ?????? ????? ?? ????
            if (!certificate.ControllerId.HasValue && !certificate.EmployeeId.HasValue)
            {
                ModelState.AddModelError("", "You must select either a Controller or an Employee.");
            }

            // ??? ??? ??????? ?????? ??? ????? ??? ?????
            if (ModelState.IsValid)
            {
                string personIdentifier = null;
                string personTypeFolder = "";

                // ????? ??? ???????? ??????? ????? ??? ????????
                if (certificate.ControllerId.HasValue)
                {
                    var controllerTable = _db.GetControllerById(certificate.ControllerId.Value);
                    if (controllerTable != null && controllerTable.Rows.Count > 0)
                    {
                        personIdentifier = controllerTable.Rows[0]["Username"].ToString();
                        personTypeFolder = "controllers";
                    }
                }
                else if (certificate.EmployeeId.HasValue)
                {
                    var employee = _db.GetEmployeeById(certificate.EmployeeId.Value);
                    if (employee != null)
                    {
                        personIdentifier = employee.Username;
                        personTypeFolder = "employees";
                    }
                }

                // ???? ??? ????? (???? ??? ??? ?? ?????? ???)
                if (certificate.UploadFile != null && certificate.UploadFile.Length > 0)
                {
                    if (string.IsNullOrEmpty(personIdentifier))
                    {
                        ModelState.AddModelError("", "Cannot upload file because the associated person was not found.");
                    }
                    else
                    {
                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(certificate.UploadFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "certificates", personTypeFolder, personIdentifier);
                        Directory.CreateDirectory(uploadsFolder);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await certificate.UploadFile.CopyToAsync(stream);
                        }
                        certificate.FilePath = $"/certificates/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                    }
                }

                // ???????? ??? ??? ?? ??? ???? ?????
                if (ModelState.ErrorCount == 0)
                {
                    string insertQuery = @"
                INSERT INTO Certificates (ControllerId, EmployeeId, TypeId, CertificateTitle, IssueDate, ExpiryDate, Status, FilePath, Notes)
                VALUES (@ControllerId, @EmployeeId, @TypeId, @Title, @IssueDate, @ExpiryDate, @Status, @FilePath, @Notes)";

                    // --- ?? ????? ???????????? ??? ??? ??????? ---
                    var parameters = new[]
                    {
                                        new SqlParameter("@ControllerId", (object)certificate.ControllerId ?? DBNull.Value),
                        new SqlParameter("@EmployeeId", (object)certificate.EmployeeId ?? DBNull.Value),
                        new SqlParameter("@TypeId", certificate.TypeId),
                        new SqlParameter("@Title", (object)certificate.Title ?? string.Empty), // <-- The Fix
                        new SqlParameter("@IssueDate", certificate.IssueDate),
                        new SqlParameter("@ExpiryDate", (object)certificate.ExpiryDate ?? DBNull.Value),
                        new SqlParameter("@Status", (object)certificate.Status ?? DBNull.Value),
                        new SqlParameter("@FilePath", (object)certificate.FilePath ?? DBNull.Value),
                        new SqlParameter("@Notes", (object)certificate.Notes ?? DBNull.Value)
            };

                    try
                    {
                        _db.ExecuteNonQuery(insertQuery, parameters);
                        TempData["SuccessMessage"] = "Document added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Database operation failed: {ex.Message}");
                    }
                }
            }

            // ?? ??? ???? ?? ???? ??? ????? ????? ??????? ?????? ??? ??????
            LoadDropdowns();
            return View(certificate);
        }

        // GET: Certificate/Edit/5
        // --- ?????? ????? ??????? ??????? ?? CertificateController.cs ???? ?????? ??????? ---

        // GET: Certificate/Edit/5
        // --- ?????? ????? ??????? ??????? ?? CertificateController.cs ???? ?????? ??????? ---

        // GET: Certificate/Edit/5
        public IActionResult Edit(int id)
        {
            var certificate = _db.GetCertificateById(id);
            if (certificate == null)
            {
                return NotFound();
            }
            LoadDropdowns(); // Load necessary lists for the view
            return View(certificate);
        }

        // POST: Certificate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] CertificateViewModel certificate)
        {
            // --- ??? ?? ???? ??????? ??????? ---
            // ???? ?? ???? ????? ?????????? ?? ????? ?????? ?? ????? ?????? ?????? ????????
            var NewFile = Request.Form.Files.FirstOrDefault();

            if (id != certificate.CertificateId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingCertificate = _db.GetCertificateById(id);
                if (existingCertificate == null)
                {
                    return NotFound();
                }
                string currentFilePath = existingCertificate.FilePath;

                // If a new file is uploaded, process it
                if (NewFile != null && NewFile.Length > 0)
                {
                    string personIdentifier = null;
                    string personTypeFolder = "";

                    if (existingCertificate.ControllerId.HasValue)
                    {
                        var controllerTable = _db.GetControllerById(existingCertificate.ControllerId.Value);
                        if (controllerTable != null && controllerTable.Rows.Count > 0)
                        {
                            personIdentifier = controllerTable.Rows[0]["Username"].ToString();
                            personTypeFolder = "controllers";
                        }
                    }
                    else if (existingCertificate.EmployeeId.HasValue)
                    {
                        var employee = _db.GetEmployeeById(existingCertificate.EmployeeId.Value);
                        if (employee != null)
                        {
                            personIdentifier = employee.Username;
                            personTypeFolder = "employees";
                        }
                    }

                    if (!string.IsNullOrEmpty(personIdentifier))
                    {
                        if (!string.IsNullOrEmpty(currentFilePath))
                        {
                            string oldPhysicalPath = Path.Combine(_webHostEnvironment.WebRootPath, currentFilePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPhysicalPath))
                            {
                                System.IO.File.Delete(oldPhysicalPath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(NewFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "certificates", personTypeFolder, personIdentifier);
                        Directory.CreateDirectory(uploadsFolder);
                        string newPhysicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(newPhysicalPath, FileMode.Create))
                        {
                            await NewFile.CopyToAsync(stream);
                        }
                        currentFilePath = $"/certificates/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                    }
                }

                const string sql = @"
            UPDATE Certificates SET
                TypeId = @TypeId,
                CertificateTitle = @Title,
                IssueDate = @IssueDate,
                ExpiryDate = @ExpiryDate,
                Status = @Status,
                FilePath = @FilePath,
                Notes = @Notes
            WHERE CertificateId = @CertificateId";

                var parameters = new[]
                {
            new SqlParameter("@TypeId", certificate.TypeId),
            new SqlParameter("@Title", (object)certificate.Title ?? string.Empty),
            new SqlParameter("@IssueDate", certificate.IssueDate),
            new SqlParameter("@ExpiryDate", certificate.ExpiryDate),
            new SqlParameter("@Status", (object)certificate.Status ?? DBNull.Value),
            new SqlParameter("@FilePath", (object)currentFilePath ?? DBNull.Value),
            new SqlParameter("@Notes", (object)certificate.Notes ?? DBNull.Value),
            new SqlParameter("@CertificateId", id)
        };

                try
                {
                    _db.ExecuteNonQuery(sql, parameters);
                    TempData["SuccessMessage"] = "Document updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Database update failed: {ex.Message}");
                }
            }

            // If we reach here, something went wrong, so redisplay the form
            LoadDropdowns();
            // Repopulate the person name for the view to display it correctly
            var person = _db.GetCertificateById(id);
            if (person != null)
            {
                certificate.ControllerName = person.ControllerName;
                certificate.EmployeeName = person.EmployeeName;
            }
            return View(certificate);
        }



        // GET: Certificate/Delete/5
        public IActionResult Delete(int id)
        {
            var certificate = _db.GetCertificateById(id);
            if (certificate == null)
            {
                return NotFound();
            }
            return View(certificate);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _db.DeleteCertificateById(id);
            TempData["SuccessMessage"] = "Document deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        private void LoadControllersAndEmployeesForDropdown()
        {
            ViewBag.Controllers = _db.ExecuteQuery("SELECT ControllerId, FullName FROM Controllers ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["ControllerId"].ToString(), Text = row["FullName"].ToString() }).ToList();
            ViewBag.Employees = _db.ExecuteQuery("SELECT EmployeeID, FullName FROM Employees ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["EmployeeID"].ToString(), Text = row["FullName"].ToString() }).ToList();
        }

        private void LoadDropdowns()
        {
            ViewBag.Controllers = _db.ExecuteQuery("SELECT ControllerId, FullName FROM Controllers ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["ControllerId"].ToString(), Text = row["FullName"].ToString() }).ToList();
            ViewBag.Employees = _db.ExecuteQuery("SELECT EmployeeID, FullName FROM Employees ORDER BY FullName").AsEnumerable()
                                     .Select(row => new SelectListItem { Value = row["EmployeeID"].ToString(), Text = row["FullName"].ToString() }).ToList();
            ViewBag.CertificateTypes = _db.GetCertificateTypes();
        }
      
        // ???? ?? ?? ??? ?????? ???? ?????? ???????? ??????? ???????
        private void LoadTypesAndControllers()
        {
            ViewBag.Types = _db.ExecuteQuery("SELECT TypeId, TypeName FROM DocumentTypes")
                                 .AsEnumerable()
                                 .Select(r => new SelectListItem
                                 {
                                     Value = r["TypeId"].ToString(),
                                     Text = r["TypeName"].ToString()
                                 })
                                 .ToList();

            ViewBag.Controllers = _db.ExecuteQuery("SELECT ControllerId, FullName FROM Controllers")
                                     .AsEnumerable()
                                     .Select(r => new SelectListItem
                                     {
                                         Value = r["ControllerId"].ToString(),
                                         Text = r["FullName"].ToString()
                                     })
                                     .ToList();
        }

        private void LoadStatusList()
        {
            ViewBag.StatusList = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pending",   Text = "Pending"   },
        new SelectListItem { Value = "Completed", Text = "Completed" },
        new SelectListItem { Value = "Rejected",  Text = "Rejected"  }
                     };
        }

        // ???? CertificateController.cs
        public IActionResult ExportToPDF(string personType, string personName, string typeName, string title, string status)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var certificates = new List<CertificateViewModel>();
            string reportTitle = "";

            switch (personType)
            {
                case "Controller":
                    certificates = _db.GetControllerCertificates();
                    reportTitle = "Controllers - Certificates & Courses";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.ControllerName != null && c.ControllerName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AIS":
                    certificates = _db.GetAISCertificates();
                    reportTitle = "AIS - Aeronautical Information Services - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "CNS":
                    certificates = _db.GetCNSCertificates();
                    reportTitle = "CNS - Communication, Navigation & Surveillance - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AFTN":
                    certificates = _db.GetAFTNCertificates();
                    reportTitle = "AFTN - Aeronautical Fixed Telecommunication Network - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "OpsStaff":
                    certificates = _db.GetOpsStaffCertificates();
                    reportTitle = "Ops Staff & Administration - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                default: // Employee (legacy support)
                    certificates = _db.GetEmployeeCertificates();
                    reportTitle = "AIS / CNS / AFTN / Ops Staff - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(typeName)) certificates = certificates.Where(c => c.TypeName != null && c.TypeName.Contains(typeName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(title)) certificates = certificates.Where(c => c.Title != null && c.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(status)) certificates = certificates.Where(c => c.Status != null && c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            var document = Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8));

                    page.Header().Column(headerCol => {
                        headerCol.Item().Row(row => {
                            var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "carc.png");
                            if (System.IO.File.Exists(logoPath)) row.ConstantColumn(70).Image(logoPath);

                            row.RelativeColumn().Column(col => {
                                col.Item().AlignCenter().Text("???? ????? ??????? ?????? ???????").Bold().FontSize(12);
                                col.Item().AlignCenter().Text("JORDAN CIVIL AVIATION REGULATORY COMMISSION").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().PaddingTop(5).AlignCenter().Text($"{reportTitle} - {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken2);
                            });
                        });
                        headerCol.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().Table(table => {
                        static IContainer HeaderStyle(IContainer c) => c.Background(Colors.Blue.Medium).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(9).Bold());
                        static IContainer BodyCellStyle(IContainer c) => c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);

                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn(3); // Person Name
                            columns.RelativeColumn(2.5f); // Type
                            columns.RelativeColumn(3); // Title
                            columns.RelativeColumn(1.5f); // Issue Date
                            columns.RelativeColumn(1.5f); // Expiry Date
                            columns.RelativeColumn(1.5f); // Status
                        });

                        table.Header(header => {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text(personType == "Controller" ? "Controller" : "Employee");
                            header.Cell().Element(HeaderStyle).Text("Type");
                            header.Cell().Element(HeaderStyle).Text("Title");
                            header.Cell().Element(HeaderStyle).Text("Issue Date");
                            header.Cell().Element(HeaderStyle).Text("Expiry Date");
                            header.Cell().Element(HeaderStyle).Text("Status");
                        });

                        int i = 1;
                        foreach (var cert in certificates)
                        {
                            table.Cell().Element(BodyCellStyle).Text(i++);
                            table.Cell().Element(BodyCellStyle).Text(cert.PersonName);
                            table.Cell().Element(BodyCellStyle).Text(cert.TypeName);
                            table.Cell().Element(BodyCellStyle).Text(cert.Title);
                            table.Cell().Element(BodyCellStyle).Text(cert.IssueDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(BodyCellStyle).Text(cert.ExpiryDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(BodyCellStyle).Text(cert.Status);
                        }
                    });

                    page.Footer().Row(row => {
                        row.RelativeColumn().Text($"Total Records: {certificates.Count}").FontSize(8);
                        row.RelativeColumn().AlignRight().Text(txt => {
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
            return File(pdfBytes, "application/pdf", $"{personType}_Certificates_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportToExcel(string personType, string personName, string typeName, string title, string status)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var certificates = new List<CertificateViewModel>();
            string reportTitle = "";
            string[] headers;

            switch (personType)
            {
                case "Controller":
                    certificates = _db.GetControllerCertificates();
                    reportTitle = "Controllers - Certificates & Courses";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.ControllerName != null && c.ControllerName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AIS":
                    certificates = _db.GetAISCertificates();
                    reportTitle = "AIS - Aeronautical Information Services - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "CNS":
                    certificates = _db.GetCNSCertificates();
                    reportTitle = "CNS - Communication, Navigation & Surveillance - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AFTN":
                    certificates = _db.GetAFTNCertificates();
                    reportTitle = "AFTN - Aeronautical Fixed Telecommunication Network - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "OpsStaff":
                    certificates = _db.GetOpsStaffCertificates();
                    reportTitle = "Ops Staff & Administration - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                default: // Employee (legacy support)
                    certificates = _db.GetEmployeeCertificates();
                    reportTitle = "AIS / CNS / AFTN / Ops Staff - Documents";
                    if (!string.IsNullOrEmpty(personName)) certificates = certificates.Where(c => c.EmployeeName != null && c.EmployeeName.Contains(personName, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(typeName)) certificates = certificates.Where(c => c.TypeName != null && c.TypeName.Contains(typeName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(title)) certificates = certificates.Where(c => c.Title != null && c.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(status)) certificates = certificates.Where(c => c.Status != null && c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            headers = new string[] { "#", "Person Name", "Type", "Title", "Issue Date", "Expiry Date", "Status" };

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Certificates");
                // ... Excel Generation Logic similar to other controllers ...

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{personType}_Certificates_List_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

    }
}
