using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
public class LicenseController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfigurationService _configurationService;
        private readonly IAdvancedPermissionManagerService _permissionService;

        public LicenseController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IConfigurationService configurationService, IAdvancedPermissionManagerService permissionService)
        {
            _db = new SqlServerDb(configuration);
            _webHostEnvironment = webHostEnvironment;
            _configurationService = configurationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized();

                // Check if user has permission to view either controller licenses or employee licenses
                var canViewControllerLicenses = await _permissionService.CanPerformOperationAsync(userId, "ControllerLicense", "View");
                var canViewEmployeeLicenses = await _permissionService.CanPerformOperationAsync(userId, "EmployeeLicense", "View");
                
                if (!canViewControllerLicenses && !canViewEmployeeLicenses)
                {
                    return Forbid();
                }
                // Get all employee licenses (including all departments)
                var allEmployeeAndOpsStaffLicenses = _db.GetEmployeeLicenses();

                var viewModel = new LicenseIndexViewModel
                {
                    ControllerLicenses = _db.GetControllerLicenses(),
                    EmployeesAndOpsStaffLicenses = allEmployeeAndOpsStaffLicenses
                };

                // Calculate alert messages for all licenses
                var allLicenses = viewModel.ControllerLicenses
                    .Concat(viewModel.EmployeesAndOpsStaffLicenses);

                foreach (var license in allLicenses)
                {
                    if (license.ExpiryDate.HasValue)
                    {
                        var daysUntilExpiry = (license.ExpiryDate.Value - DateTime.Now).Days;
                        if (daysUntilExpiry < 0)
                        {
                            license.AlertMessage = $"Your license expired {Math.Abs(daysUntilExpiry)} days ago.";
                        }
                        else if (daysUntilExpiry <= 90)
                        {
                            license.AlertMessage = $"Your license will expire in {daysUntilExpiry} days.";
                        }
                        else
                        {
                            license.AlertMessage = $"Your license is active. ({daysUntilExpiry} days remaining)";
                        }
                    }
                }

                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(new LicenseIndexViewModel());
            }
        }

        public IActionResult Create(string tab = "controllers")
        {
            LoadControllersAndEmployeesForDropdown();
            ViewBag.ActiveTab = tab;
            return View(new License());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] License license)
        {
            if (!license.ControllerId.HasValue && !license.EmployeeId.HasValue)
            {
                ModelState.AddModelError("", "You must select either a Controller or an Employee for the license.");
            }

            if (!ModelState.IsValid)
            {
                LoadControllersAndEmployeesForDropdown();
                return View(license);
            }

            string username = null;
            string personTypeFolder = "";

            if (license.ControllerId.HasValue)
            {
                var controllerTable = _db.GetControllerById(license.ControllerId.Value);
                if (controllerTable != null && controllerTable.Rows.Count > 0)
                {
                    username = controllerTable.Rows[0]["Username"].ToString();
                    personTypeFolder = "controllers";
                }
            }
            else if (license.EmployeeId.HasValue)
            {
                // -- CORRECTED LOGIC HERE --
                // GetEmployeeById returns a single Employee object, not a DataTable.
                var employeeObject = _db.GetEmployeeById(license.EmployeeId.Value);
                if (employeeObject != null)
                {
                    // We get the username directly from the object's property.
                    username = employeeObject.Username;
                    personTypeFolder = "employees";
                }
            }

            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("", "Could not find the selected person to create the directory.");
                LoadControllersAndEmployeesForDropdown();
                return View(license);
            }

            if (license.LicenseFile != null && license.LicenseFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(license.LicenseFile.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "licenses", personTypeFolder, username);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await license.LicenseFile.CopyToAsync(stream);
                }
                license.PDFPath = $"/licenses/{personTypeFolder}/{username}/{uniqueFileName}";
            }
            else
            {
                ModelState.AddModelError(nameof(license.LicenseFile), "The License File field is required.");
                LoadControllersAndEmployeesForDropdown();
                return View(license);
            }

            if (license.LicenseType == "English Proficiency Test" && license.RANGE == "6")
            {
                license.ExpiryDate = new DateTime(2100, 1, 1);
            }

            string insertQuery = @"
            INSERT INTO Licenses (ControllerId, EmployeeId, LicenseType, ExpiryDate, PDFPath, IssueDate, licensenumber, Note, [RANGE])
            VALUES (@ControllerId, @EmployeeId, @LicenseType, @ExpiryDate, @PDFPath, @IssueDate, @licensenumber, @Note, @Range)";

            var parameters = new[]
            {
                                        new SqlParameter("@ControllerId", (object)license.ControllerId ?? DBNull.Value),
                        new SqlParameter("@EmployeeId", (object)license.EmployeeId ?? DBNull.Value),
                        new SqlParameter("@LicenseType", (object)license.LicenseType ?? DBNull.Value),
                        new SqlParameter("@ExpiryDate", (object)license.ExpiryDate ?? DBNull.Value),
                        new SqlParameter("@PDFPath", (object)license.PDFPath ?? DBNull.Value),
                        new SqlParameter("@IssueDate", (object)license.IssueDate ?? DBNull.Value),
                        new SqlParameter("@licensenumber", (object)license.licensenumber ?? DBNull.Value),
                        new SqlParameter("@Note", (object)license.Note ?? DBNull.Value),
                        new SqlParameter("@Range", (object)license.RANGE ?? DBNull.Value)
            };

            try
            {
                _db.ExecuteNonQuery(insertQuery, parameters);
                
                // Update notifications immediately after adding new license
                try
                {
                    // Clear old notifications first
                    _db.ExecuteNonQuery("DELETE FROM notifications");
                    
                    // Get licenses expiring soon and create notifications
                    var licenseNotificationsSql = @"
                        INSERT INTO notifications (userid, controllerid, message, licensetype, licenseexpirydate, created_at, is_read)
                        SELECT 
                            c.userid,
                            c.controllerid,
                            'Dear ' + c.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN controllers c ON l.controllerid = c.controllerid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())
                        
                        UNION ALL
                        
                        SELECT 
                            e.userid,
                            NULL,
                            'Dear ' + e.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN employees e ON l.employeeid = e.employeeid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())";
                    
                    _db.ExecuteNonQuery(licenseNotificationsSql);
                }
                catch (Exception notificationEx)
                {
                    // Log the error but don't fail the license creation
                    Console.WriteLine($"Failed to update notifications: {notificationEx.Message}");
                }
                
                TempData["SuccessMessage"] = "License added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database operation failed: {ex.Message}");
            }

            LoadControllersAndEmployeesForDropdown();
            return View(license);
        }



        // GET: License/Edit/5
        public IActionResult Edit(int id)
        {
            var license = _db.GetLicenseById(id);
            if (license == null)
            {
                return NotFound();
            }
            LoadControllersAndEmployeesForDropdown();
            return View(license);
        }

        // POST: License/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] License license)
        {
            if (id != license.LicenseId)
            {
                return BadRequest();
            }

            // ?????? ??????? ?????? ?? ????? ???????? ??????? ??? PDF ??????
            var originalLicense = _db.GetLicenseById(id);
            if (originalLicense == null)
            {
                return NotFound(); // ??????? ??? ?????
            }

            // ?? ????? ?? ControllerName, AirportName, EmployeeName, EmployeeDepartment
            // ?? ModelState.IsValid ????? ???? ??????? ??? ?? ??? View
            // ???? ??? ?? ????? ?? ?? ControllerId ?? EmployeeId ???????
            if (!license.ControllerId.HasValue && !license.EmployeeId.HasValue)
            {
                ModelState.AddModelError("", "You must select either a Controller or an Employee for the license.");
            }

            if (!ModelState.IsValid)
            {
                LoadControllersAndEmployeesForDropdown();
                // ????? ????? ControllerName, AirportName, EmployeeName, EmployeeDepartment
                // ????? ????? ??? ???? ?? ??? View ??? ??? ??????
                license.ControllerName = originalLicense.ControllerName;
                license.AirportName = originalLicense.AirportName;
                license.EmployeeName = originalLicense.EmployeeName;
                license.EmployeeDepartment = originalLicense.EmployeeDepartment;
                return View(license);
            }

            // 1. ??????? ?? ??? PDF ?????? (??? ?? ??????)
            string newPdfPath = originalLicense.PDFPath; // ????? ??????? ?????? ?????????

            if (license.LicenseFile != null && license.LicenseFile.Length > 0)
            {
                string username = null;
                string personTypeFolder = "";

                if (license.ControllerId.HasValue)
                {
                    var controllerTable = _db.GetControllerById(license.ControllerId.Value);
                    if (controllerTable != null && controllerTable.Rows.Count > 0)
                    {
                        username = controllerTable.Rows[0]["Username"].ToString();
                        personTypeFolder = "controllers";
                    }
                }
                else if (license.EmployeeId.HasValue)
                {
                    var employeeObject = _db.GetEmployeeById(license.EmployeeId.Value);
                    if (employeeObject != null)
                    {
                        username = employeeObject.Username;
                        personTypeFolder = "employees";
                    }
                }

                if (string.IsNullOrEmpty(username))
                {
                    ModelState.AddModelError("", "Could not find the selected person to create the directory for the new PDF.");
                    LoadControllersAndEmployeesForDropdown();
                    // ????? ????? ?????? ????? ??? ??? ??????
                    license.ControllerName = originalLicense.ControllerName;
                    license.AirportName = originalLicense.AirportName;
                    license.EmployeeName = originalLicense.EmployeeName;
                    license.EmployeeDepartment = originalLicense.EmployeeDepartment;
                    return View(license);
                }

                // ??? ????? ?????? ??? ??? ???????
                if (!string.IsNullOrEmpty(originalLicense.PDFPath))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, originalLicense.PDFPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // ??? ????? ??????
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(license.LicenseFile.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "licenses", personTypeFolder, username);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await license.LicenseFile.CopyToAsync(stream);
                }
                newPdfPath = $"/licenses/{personTypeFolder}/{username}/{uniqueFileName}";
            }

            // 2. ????? ????? ?????? ???????? ?? "English Proficiency Test"
            if (license.LicenseType == "English Proficiency Test" && license.RANGE == "6")
            {
                license.ExpiryDate = new DateTime(2100, 1, 1);
            }

            // 3. ???? ??????? ???????
            string updateQuery = @"
        UPDATE Licenses
        SET
            ControllerId = @ControllerId,
            EmployeeId = @EmployeeId,
            LicenseType = @LicenseType,
            ExpiryDate = @ExpiryDate,
            PDFPath = @PDFPath,
            IssueDate = @IssueDate,
            licensenumber = @licensenumber,
            Note = @Note,
            [RANGE] = @Range
        WHERE LicenseId = @LicenseId";

            var parameters = new[]
            {
                                new SqlParameter("@ControllerId", (object)license.ControllerId ?? DBNull.Value),
                        new SqlParameter("@EmployeeId", (object)license.EmployeeId ?? DBNull.Value),
                        new SqlParameter("@LicenseType", (object)license.LicenseType ?? DBNull.Value),
                        new SqlParameter("@ExpiryDate", (object)license.ExpiryDate ?? DBNull.Value),
                        new SqlParameter("@PDFPath", (object)newPdfPath ?? DBNull.Value), // ?????? newPdfPath ???
                        new SqlParameter("@IssueDate", (object)license.IssueDate ?? DBNull.Value),
                        new SqlParameter("@licensenumber", (object)license.licensenumber ?? DBNull.Value),
                        new SqlParameter("@Note", (object)license.Note ?? DBNull.Value),
                        new SqlParameter("@Range", (object)license.RANGE ?? DBNull.Value),
                        new SqlParameter("@LicenseId", license.LicenseId) // ??? ???????
    };

            try
            {
                _db.ExecuteNonQuery(updateQuery, parameters);
                
                // Update notifications immediately after updating license
                try
                {
                    // Clear old notifications first
                    _db.ExecuteNonQuery("DELETE FROM notifications");
                    
                    // Get licenses expiring soon and create notifications
                    var licenseNotificationsSql = @"
                        INSERT INTO notifications (userid, controllerid, message, licensetype, licenseexpirydate, created_at, is_read)
                        SELECT 
                            c.userid,
                            c.controllerid,
                            'Dear ' + c.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN controllers c ON l.controllerid = c.controllerid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())
                        
                        UNION ALL
                        
                        SELECT 
                            e.userid,
                            NULL,
                            'Dear ' + e.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN employees e ON l.employeeid = e.employeeid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())";
                    
                    _db.ExecuteNonQuery(licenseNotificationsSql);
                }
                catch (Exception notificationEx)
                {
                    // Log the error but don't fail the license update
                    Console.WriteLine($"Failed to update notifications: {notificationEx.Message}");
                }
                
                TempData["SuccessMessage"] = "License updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database update failed: {ex.Message}");
            }

            LoadControllersAndEmployeesForDropdown();
            // ????? ????? ?????? ????? ??? ??? ???????
            license.ControllerName = originalLicense.ControllerName;
            license.AirportName = originalLicense.AirportName;
            license.EmployeeName = originalLicense.EmployeeName;
            license.EmployeeDepartment = originalLicense.EmployeeDepartment;
            return View(license);
        }
        // POST: License/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _db.DeleteLicenseById(id);
                
                // Update notifications immediately after deleting license
                try
                {
                    // Clear old notifications first
                    _db.ExecuteNonQuery("DELETE FROM notifications");
                    
                    // Get licenses expiring soon and create notifications
                    var licenseNotificationsSql = @"
                        INSERT INTO notifications (userid, controllerid, message, licensetype, licenseexpirydate, created_at, is_read)
                        SELECT 
                            c.userid,
                            c.controllerid,
                            'Dear ' + c.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN controllers c ON l.controllerid = c.controllerid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())
                        
                        UNION ALL
                        
                        SELECT 
                            e.userid,
                            NULL,
                            'Dear ' + e.fullname + ', Your ' + l.licensetype + ' will expire on ' + CONVERT(varchar, l.expirydate, 23) + '. Please update your license before expiry.',
                            l.licensetype,
                            l.expirydate,
                            GETDATE(),
                            0
                        FROM licenses l
                        INNER JOIN employees e ON l.employeeid = e.employeeid
                        WHERE l.expirydate <= DATEADD(day, 90, GETDATE())";
                    
                    _db.ExecuteNonQuery(licenseNotificationsSql);
                }
                catch (Exception notificationEx)
                {
                    // Log the error but don't fail the license deletion
                    Console.WriteLine($"Failed to update notifications: {notificationEx.Message}");
                }
                
                TempData["SuccessMessage"] = "License deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to delete license: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to load dropdown lists for the view
        private void LoadControllersAndEmployeesForDropdown()
        {
            // You should have methods in SqlServerDb to get lists of controllers and employees
            ViewBag.Controllers = _db.GetControllers(null, null, null, null, null, null, null, null, null, null, null)
                                     .Select(c => new SelectListItem { Value = c.ControllerId.ToString(), Text = c.FullName })
                                     .ToList();
            ViewBag.Employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null)
                                     .Select(e => new SelectListItem { Value = e.EmployeeID.ToString(), Text = e.FullName })
                                     .ToList();
            
            // Load LicenseTypes from Configuration Service
            this.LoadConfigurationDropdown(_configurationService, "LicenseTypes", "LicenseTypes");
        }






        // --- EXPORT ACTIONS ---
        // ?????? ????? ??????? ??????? ???? ??????? ??????? ????????

        // --- EXPORT ACTIONS ---

        public IActionResult ExportToPDF(string personType,
            string controllerName, string employeeName, string department,
            string licenseType, string licenseNumber, string note, string airport)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var licenses = new List<License>();
            string reportTitle = "";

            // Fetch and filter data
            switch (personType)
            {
                case "Controller":
                licenses = _db.GetControllerLicenses();
                reportTitle = "Air Traffic Controllers Licenses Report";
                if (!string.IsNullOrEmpty(controllerName)) licenses = licenses.Where(l => l.ControllerName != null && l.ControllerName.Contains(controllerName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.ControllerCurrentDepartment != null && l.ControllerCurrentDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(airport)) licenses = licenses.Where(l => l.AirportName != null && l.AirportName.Contains(airport, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "Employees":
                    // Get all employee licenses (including all departments)
                    licenses = _db.GetEmployeeLicenses();
                    reportTitle = "Employees & Operation Staff Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AIS":
                    licenses = _db.GetAISLicenses();
                    reportTitle = "AIS - Aeronautical Information Services Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "CNS":
                    licenses = _db.GetCNSLicenses();
                    reportTitle = "CNS - Communication, Navigation & Surveillance Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "AFTN":
                    licenses = _db.GetAFTNLicenses();
                    reportTitle = "AFTN - Aeronautical Fixed Telecommunication Network Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "OpsStaff":
                    licenses = _db.GetOpsStaffLicenses();
                    reportTitle = "Ops Staff & Administration Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                default: // Employee (legacy support)
                licenses = _db.GetEmployeeLicenses();
                reportTitle = "Users Permissions Report";
                if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
            }
            if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();

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

                        if (personType == "Controller")
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(2); // Controller Name
                                columns.RelativeColumn(1.5f); // Department
                                columns.RelativeColumn(2.5f); // License Type
                                columns.RelativeColumn(1.5f); // License No.
                                columns.RelativeColumn(1.5f); // Issue Date
                                columns.RelativeColumn(1.5f); // Expiry Date
                                columns.RelativeColumn(1.5f); // Note
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("#");
                                header.Cell().Element(HeaderStyle).Text("Controller");
                                header.Cell().Element(HeaderStyle).Text("Department");
                                header.Cell().Element(HeaderStyle).Text("License Type");
                                header.Cell().Element(HeaderStyle).Text("License No.");
                                header.Cell().Element(HeaderStyle).Text("Issue Date");
                                header.Cell().Element(HeaderStyle).Text("Expiry Date");
                                header.Cell().Element(HeaderStyle).Text("Note");
                            });

                            int i = 1;
                            foreach (var license in licenses)
                            {
                                table.Cell().Element(BodyCellStyle).Text(i++);
                                table.Cell().Element(BodyCellStyle).Text(license.ControllerName);
                                table.Cell().Element(BodyCellStyle).Text(license.ControllerCurrentDepartment);
                                table.Cell().Element(BodyCellStyle).Text(license.LicenseType + (license.LicenseType == "English Proficiency Test" && !string.IsNullOrEmpty(license.RANGE) ? $" - {license.RANGE}" : ""));
                                table.Cell().Element(BodyCellStyle).Text(license.licensenumber);
                                table.Cell().Element(BodyCellStyle).Text(license.IssueDate?.ToString("yyyy-MM-dd"));
                                table.Cell().Element(BodyCellStyle).Text(license.ExpiryDate?.ToString("yyyy-MM-dd"));
                                table.Cell().Element(BodyCellStyle).Text(license.Note);
                            }
                        }
                        else // All Employee types (AIS, CNS, AFTN, OpsStaff, Employee)
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(2.5f); // Employee Name
                                columns.RelativeColumn(2); // Department
                                columns.RelativeColumn(2.5f); // License Type
                                columns.RelativeColumn(1.5f); // License No.
                                columns.RelativeColumn(1.5f); // Issue Date
                                columns.RelativeColumn(1.5f); // Expiry Date
                                columns.RelativeColumn(1.5f); // Note
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("#");
                                header.Cell().Element(HeaderStyle).Text("Employee");
                                header.Cell().Element(HeaderStyle).Text("Department");
                                header.Cell().Element(HeaderStyle).Text("License Type");
                                header.Cell().Element(HeaderStyle).Text("License No.");
                                header.Cell().Element(HeaderStyle).Text("Issue Date");
                                header.Cell().Element(HeaderStyle).Text("Expiry Date");
                                header.Cell().Element(HeaderStyle).Text("Note");
                            });

                            int i = 1;
                            foreach (var license in licenses)
                            {
                                table.Cell().Element(BodyCellStyle).Text(i++);
                                table.Cell().Element(BodyCellStyle).Text(license.EmployeeName);
                                table.Cell().Element(BodyCellStyle).Text(license.EmployeeDepartment);
                                table.Cell().Element(BodyCellStyle).Text(license.LicenseType + (license.LicenseType == "English Proficiency Test" && !string.IsNullOrEmpty(license.RANGE) ? $" - {license.RANGE}" : ""));
                                table.Cell().Element(BodyCellStyle).Text(license.licensenumber);
                                table.Cell().Element(BodyCellStyle).Text(license.IssueDate?.ToString("yyyy-MM-dd"));
                                table.Cell().Element(BodyCellStyle).Text(license.ExpiryDate?.ToString("yyyy-MM-dd"));
                                table.Cell().Element(BodyCellStyle).Text(license.Note);
                            }
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn().Text($"Total Records: {licenses.Count}").FontSize(8);
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
            return File(pdfBytes, "application/pdf", $"{personType}_Licenses_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportToExcel(string personType,
            string controllerName, string employeeName, string department,
            string licenseType, string licenseNumber, string note, string airport)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var licenses = new List<License>();
            string reportTitle = "";
            string[] headers;

            switch (personType)
            {
                case "Controller":
                licenses = _db.GetControllerLicenses();
                reportTitle = "Air Traffic Controllers Licenses Report";
                if (!string.IsNullOrEmpty(controllerName)) licenses = licenses.Where(l => l.ControllerName != null && l.ControllerName.Contains(controllerName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.ControllerCurrentDepartment != null && l.ControllerCurrentDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(airport)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.AirportName) && l.AirportName.Contains(airport, StringComparison.OrdinalIgnoreCase)).ToList();
                headers = new string[] { "#", "Controller", "Department", "Airport", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                case "Employees":
                    // Get all employee licenses (including all departments)
                    licenses = _db.GetEmployeeLicenses();
                    reportTitle = "Employees & Operation Staff Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                    headers = new string[] { "#", "Employee", "Department", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                case "AIS":
                    licenses = _db.GetAISLicenses();
                    reportTitle = "AIS - Aeronautical Information Services Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                    headers = new string[] { "#", "Employee", "Department", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                case "CNS":
                    licenses = _db.GetCNSLicenses();
                    reportTitle = "CNS - Communication, Navigation & Surveillance Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                    headers = new string[] { "#", "Employee", "Department", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                case "AFTN":
                    licenses = _db.GetAFTNLicenses();
                    reportTitle = "AFTN - Aeronautical Fixed Telecommunication Network Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                    headers = new string[] { "#", "Employee", "Department", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                case "OpsStaff":
                    licenses = _db.GetOpsStaffLicenses();
                    reportTitle = "Ops Staff & Administration Licenses Report";
                    if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!string.IsNullOrEmpty(licenseNumber)) licenses = licenses.Where(l => !string.IsNullOrEmpty(l.licensenumber) && l.licensenumber.Contains(licenseNumber, StringComparison.OrdinalIgnoreCase)).ToList();
                    headers = new string[] { "#", "Employee", "Department", "License Type", "License No.", "Issue Date", "Expiry Date", "Note" };
                    break;
                default: // Employee (legacy support)
                licenses = _db.GetEmployeeLicenses();
                reportTitle = "Users Permissions Report";
                if (!string.IsNullOrEmpty(employeeName)) licenses = licenses.Where(l => l.EmployeeName != null && l.EmployeeName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(department)) licenses = licenses.Where(l => l.EmployeeDepartment != null && l.EmployeeDepartment.Contains(department, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(licenseType)) licenses = licenses.Where(l => l.LicenseType != null && l.LicenseType.Contains(licenseType, StringComparison.OrdinalIgnoreCase)).ToList();
                headers = new string[] { "#", "Employee", "Department", "Permission Type", "Issue Date", "Expiry Date" };
                    break;
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(personType + " Licenses");
                worksheet.Cells.Style.Font.Name = "Arial";

                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "carc.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                    excelImage.SetPosition(0, 0, 0, 15);
                    excelImage.SetSize(120, 65);
                }

                worksheet.Cells["C1"].Value = reportTitle;
                worksheet.Cells["C1"].Style.Font.Bold = true;
                worksheet.Cells["C1"].Style.Font.Size = 14;
                worksheet.Cells[1, 3, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 3, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

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
                int index = 1;
                foreach (var license in licenses)
                {
                    worksheet.Cells[row, 1].Value = index++;
                    if (personType == "Controller")
                    {
                        worksheet.Cells[row, 2].Value = license.ControllerName;
                        worksheet.Cells[row, 3].Value = license.ControllerCurrentDepartment;
                        worksheet.Cells[row, 4].Value = license.AirportName;
                        worksheet.Cells[row, 5].Value = license.LicenseType + (!string.IsNullOrEmpty(license.RANGE) ? " - " + license.RANGE : "");
                        worksheet.Cells[row, 6].Value = license.licensenumber;
                        worksheet.Cells[row, 7].Value = license.IssueDate;
                        worksheet.Cells[row, 8].Value = license.ExpiryDate;
                        worksheet.Cells[row, 9].Value = license.Note;
                        worksheet.Cells[row, 7, row, 8].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                    else // All Employee types (AIS, CNS, AFTN, OpsStaff, Employee)
                    {
                        worksheet.Cells[row, 2].Value = license.EmployeeName;
                        worksheet.Cells[row, 3].Value = license.EmployeeDepartment;
                        worksheet.Cells[row, 4].Value = license.LicenseType + (license.LicenseType == "English Proficiency Test" && !string.IsNullOrEmpty(license.RANGE) ? $" - {license.RANGE}" : "");
                        worksheet.Cells[row, 5].Value = license.licensenumber;
                        worksheet.Cells[row, 6].Value = license.IssueDate;
                        worksheet.Cells[row, 7].Value = license.ExpiryDate;
                        worksheet.Cells[row, 8].Value = license.Note;
                        worksheet.Cells[row, 6, row, 7].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{personType}_Licenses_List_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}


