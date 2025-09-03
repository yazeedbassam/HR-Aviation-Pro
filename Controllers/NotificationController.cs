using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using WebApplication1.DataAccess;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using SendGrid.Helpers.Mail;
using System.Drawing; // لعمليات الصورة إذا أردت
using System.Linq; // Required for .Any()
using WebApplication1.Services;
using Microsoft.Data.SqlClient; // Added for SqlCommand and SqlParameter
using System.Collections.Generic; // Added for List
using System; // Added for DateTime
using System.Data; // Added for DBNull
using WebApplication1.Models; // Added for NotificationModel

public class NotificationController : Controller
{
    private readonly SqlServerDb _db;
    private readonly ILicenseNotificationService _licenseNotificationService;

    public NotificationController(SqlServerDb db, ILicenseNotificationService licenseNotificationService)
    {
        _db = db;
        _licenseNotificationService = licenseNotificationService;
    }

    [HttpGet]
    public IActionResult ExportNotificationsToPDF(string filter = "")
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // استخدام جدول notifications بدلاً من GetNotifications
        // هذا يحل مشكلة التناقض في البيانات بين العرض والتصدير
        // البيانات الآن تأتي من نفس المصدر مما يضمن الاتساق
        string sql = @"
            SELECT 
                n.NotificationId,
                n.userid,
                n.controllerid,
                COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                n.message,
                n.link,
                n.created_at,
                n.is_read,
                n.note,
                n.licensetype,
                n.licenseexpirydate,
                DATEDIFF(day, GETDATE(), n.licenseexpirydate) AS RemainingDays,
                COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                COALESCE(c.email, e.email, 'N/A') AS email,
                COALESCE(c.current_department, e.department, 'N/A') AS current_department,
                COALESCE(a.airportname, 'HQ - Main Office') AS airportname
            FROM notifications n
            LEFT JOIN controllers c ON n.controllerid = c.controllerid
            LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
            LEFT JOIN airports a ON c.airportid = a.airportid
            WHERE n.licenseexpirydate IS NOT NULL";

        if (!string.IsNullOrWhiteSpace(filter))
        {
            sql += @" AND (LOWER(n.message) LIKE @filter
                      OR LOWER(n.note) LIKE @filter
                      OR LOWER(n.userid) LIKE @filter
                      OR LOWER(COALESCE(c.fullname, e.fullname)) LIKE @filter
                      OR LOWER(n.licensetype) LIKE @filter
                      OR LOWER(COALESCE(c.current_department, e.department)) LIKE @filter)";
        }

        sql += " ORDER BY RemainingDays ASC";

        // تنفيذ الاستعلام مباشرة للحصول على البيانات
        var notifications = new List<NotificationModel>();
        using (var conn = _db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%"));
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new NotificationModel
                        {
                            NotificationId = reader["NotificationId"]?.ToString() ?? "",
                            UserId = reader["userid"]?.ToString() ?? "",
                            ControllerId = reader["controllerid"]?.ToString() ?? "",
                            FullName = reader["ControllerName"]?.ToString() ?? "",
                            Message = reader["message"]?.ToString() ?? "",
                            Link = reader["link"]?.ToString() ?? "",
                            CreatedAt = reader["created_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["created_at"]),
                            IsRead = reader["is_read"] != DBNull.Value && (Convert.ToInt32(reader["is_read"]) == 1),
                            Note = reader["note"]?.ToString() ?? "",
                            LicenseType = reader["licensetype"]?.ToString() ?? "",
                            LicenseExpiryDate = reader["licenseexpirydate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["licenseexpirydate"]),
                            phonenumber = reader["phone_number"]?.ToString() ?? "",
                            Email = reader["email"]?.ToString() ?? "",
                            Currentdepartment = reader["current_department"]?.ToString() ?? "",
                            Location = reader["airportname"]?.ToString() ?? ""
                        });
                    }
                }
            }
        }

        // --- DEBUGGING CHECK ---
        // Check if any data was returned from the database.
        if (notifications == null || !notifications.Any())
        {
            return NotFound("No notifications found to export. The database query returned no results for the given filter.");
        }

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // صفحة عرضية
                page.Margin(30);

                // رأس الصفحة مع الشعار والعنوان
                page.Header()
                    .AlignCenter()
                    .Column(col =>
                    {
                        if (System.IO.File.Exists(logoPath))
                            col.Item().AlignCenter().Height(70).Image(logoPath, QuestPDF.Infrastructure.ImageScaling.FitHeight);

                        col.Item().PaddingTop(8);
                        col.Item().AlignCenter().Text("NOTIFICATIONS REPORT")
                            .Bold().FontSize(28).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                        col.Item().AlignCenter().Text(DateTime.Now.ToString("yyyy/MM/dd HH:mm")).FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });

                // جدول الإشعارات أعرض ومرتب
                page.Content().PaddingTop(22).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);       // #
                        columns.RelativeColumn(1.6f);     // UserId
                        columns.RelativeColumn(2.2f);     // ControllerName
                        columns.RelativeColumn(1.6f);     // Note
                        columns.RelativeColumn(1.8f);     // LicenseType
                        columns.RelativeColumn(1.8f);     // LicenseExpiry
                        columns.RelativeColumn(1.8f);     // phone No
                        columns.RelativeColumn(1.9f);     // Email
                        columns.RelativeColumn(1.8f);     // Location
                    });

                    // رأس الجدول
                    table.Header(header =>
                    {
                        header.Cell().Text("#").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("User").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("Controller Name").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("Note").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("License Type").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("License Expiry").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("Phone No").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("Email").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                        header.Cell().Text("Location").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                    });

                    int idx = 1;
                    foreach (var n in notifications)
                    {
                        table.Cell().Text(idx++);
                        table.Cell().Text(n.UserId ?? "N/A");
                        table.Cell().Text(n.FullName ?? "N/A");
                        table.Cell().Text(n.Note ?? "N/A");
                        table.Cell().Text(n.LicenseType ?? "N/A");
                        table.Cell().Text(n.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? "N/A");
                        table.Cell().Text(n.phonenumber ?? "N/A");
                        table.Cell().Text(n.Email ?? "N/A");
                        table.Cell().Text(n.Location ?? "N/A");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .PaddingTop(8)
                    .Text("Notifications Report - Aviation HR Management System - " + DateTime.Now.ToString("yyyy/MM/dd"))
                    .FontSize(10)
                    .FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", "notifications.pdf");


    }

    [HttpGet]
    public IActionResult ExportNotificationsToExcel(string filter = "")
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // استخدام جدول notifications بدلاً من GetNotifications
        // هذا يحل مشكلة التناقض في البيانات بين العرض والتصدير
        // البيانات الآن تأتي من نفس المصدر مما يضمن الاتساق
        string sql = @"
            SELECT 
                n.NotificationId,
                n.userid,
                n.controllerid,
                COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                n.message,
                n.link,
                n.created_at,
                n.is_read,
                n.note,
                n.licensetype,
                n.licenseexpirydate,
                DATEDIFF(day, GETDATE(), n.licenseexpirydate) AS RemainingDays,
                COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                COALESCE(c.email, e.email, 'N/A') AS email,
                COALESCE(c.current_department, e.department, 'N/A') AS current_department,
                COALESCE(a.airportname, 'HQ - Main Office') AS airportname
            FROM notifications n
            LEFT JOIN controllers c ON n.controllerid = c.controllerid
            LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
            LEFT JOIN airports a ON c.airportid = a.airportid
            WHERE n.licenseexpirydate IS NOT NULL";

        if (!string.IsNullOrWhiteSpace(filter))
        {
            sql += @" AND (LOWER(n.message) LIKE @filter
                      OR LOWER(n.note) LIKE @filter
                      OR LOWER(n.userid) LIKE @filter
                      OR LOWER(COALESCE(c.fullname, e.fullname)) LIKE @filter
                      OR LOWER(n.licensetype) LIKE @filter
                      OR LOWER(COALESCE(c.current_department, e.department)) LIKE @filter)";
        }

        sql += " ORDER BY RemainingDays ASC";

        // تنفيذ الاستعلام مباشرة للحصول على البيانات
        var notifications = new List<NotificationModel>();
        using (var conn = _db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%"));
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new NotificationModel
                        {
                            NotificationId = reader["NotificationId"]?.ToString() ?? "",
                            UserId = reader["userid"]?.ToString() ?? "",
                            ControllerId = reader["controllerid"]?.ToString() ?? "",
                            FullName = reader["ControllerName"]?.ToString() ?? "",
                            Message = reader["message"]?.ToString() ?? "",
                            Link = reader["link"]?.ToString() ?? "",
                            CreatedAt = reader["created_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["created_at"]),
                            IsRead = reader["is_read"] != DBNull.Value && (Convert.ToInt32(reader["is_read"]) == 1),
                            Note = reader["note"]?.ToString() ?? "",
                            LicenseType = reader["licensetype"]?.ToString() ?? "",
                            LicenseExpiryDate = reader["licenseexpirydate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["licenseexpirydate"]),
                            phonenumber = reader["phone_number"]?.ToString() ?? "",
                            Email = reader["email"]?.ToString() ?? "",
                            Currentdepartment = reader["current_department"]?.ToString() ?? "",
                            Location = reader["airportname"]?.ToString() ?? ""
                        });
                    }
                }
            }
        }

        // --- DEBUGGING CHECK ---
        // Check if any data was returned from the database.
        if (notifications == null || !notifications.Any())
        {
            return NotFound("No notifications found to export. The database query returned no results for the given filter.");
        }

        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Notifications");

            // إضافة الشعار إذا وجد
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
            if (System.IO.File.Exists(logoPath))
            {
                var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                excelImage.SetPosition(0, 0, 2, 0);
                excelImage.SetSize(130, 70);
            }

            // رأس التقرير
            worksheet.Cells[2, 2, 2, 8].Merge = true;
            worksheet.Cells[2, 2].Value = "تقرير الإشعارات";
            worksheet.Cells[2, 2].Style.Font.Size = 16;
            worksheet.Cells[2, 2].Style.Font.Bold = true;
            worksheet.Cells[2, 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(33, 150, 243));
            worksheet.Cells[2, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            worksheet.Cells[3, 2, 3, 8].Merge = true;
            worksheet.Cells[3, 2].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            worksheet.Cells[3, 2].Style.Font.Size = 10;
            worksheet.Cells[3, 2].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
            worksheet.Cells[3, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // عناوين الأعمدة
            worksheet.Cells[5, 1].Value = "#";
            worksheet.Cells[5, 2].Value = "User";
            worksheet.Cells[5, 3].Value = "Controller Name";
            worksheet.Cells[5, 4].Value = "Message";
            worksheet.Cells[5, 5].Value = "Created At";
            worksheet.Cells[5, 6].Value = "Note";
            worksheet.Cells[5, 7].Value = "License Type";
            worksheet.Cells[5, 8].Value = "License Expiry";
            worksheet.Cells[5, 9].Value = "Phone No";
            worksheet.Cells[5, 10].Value = "Email";
            worksheet.Cells[5, 11].Value = "Location";

            using (var range = worksheet.Cells[5, 1, 5, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(33, 150, 243));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            int row = 6;
            int idx = 1;
            foreach (var n in notifications)
            {
                worksheet.Cells[row, 1].Value = idx++;
                worksheet.Cells[row, 2].Value = n.UserId ?? "N/A";
                worksheet.Cells[row, 3].Value = n.FullName ?? "N/A";
                worksheet.Cells[row, 4].Value = n.Message ?? "N/A";
                worksheet.Cells[row, 5].Value = n.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
                worksheet.Cells[row, 6].Value = n.Note ?? "N/A";
                worksheet.Cells[row, 7].Value = n.LicenseType ?? "N/A";
                worksheet.Cells[row, 8].Value = n.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? "N/A";
                worksheet.Cells[row, 9].Value = n.phonenumber ?? "N/A";
                worksheet.Cells[row, 10].Value = n.Email ?? "N/A";
                worksheet.Cells[row, 11].Value = n.Location ?? "N/A";
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(6, 1);

            var excelBytes = package.GetAsByteArray();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "notifications.xlsx");
        }
    }

    /// <summary>
    /// عرض المراقبين الذين يحتاجون رخص
    /// </summary>
    [HttpGet]
    public IActionResult ControllersNeedingLicenses()
    {
        var controllers = _licenseNotificationService.GetControllersNeedingLicenses();
        ViewBag.Title = "Controllers Needing Licenses";
        ViewBag.Subtitle = "Active controllers who need licenses but do not have them";
        return View("ControllersNotifications", controllers);
    }

    /// <summary>
    /// عرض المراقبين غير النشطين
    /// </summary>
    [HttpGet]
    public IActionResult InactiveControllers()
    {
        var controllers = _licenseNotificationService.GetInactiveControllers();
        ViewBag.Title = "Inactive Controllers";
        ViewBag.Subtitle = "Controllers who have been disabled";
        return View("ControllersNotifications", controllers);
    }

    /// <summary>
    /// تصدير المراقبين الذين يحتاجون رخص إلى PDF
    /// </summary>
    [HttpGet]
    public IActionResult ExportControllersNeedingLicensesToPDF()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var controllers = _licenseNotificationService.GetControllersNeedingLicenses();

        if (controllers == null || !controllers.Any())
        {
            return NotFound("No controllers need licenses");
        }

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                // رأس الصفحة
                page.Header()
                    .AlignCenter()
                    .Column(col =>
                    {
                        if (System.IO.File.Exists(logoPath))
                            col.Item().AlignCenter().Height(70).Image(logoPath, QuestPDF.Infrastructure.ImageScaling.FitHeight);

                        col.Item().PaddingTop(8);
                        col.Item().AlignCenter().Text("Controllers Needing Licenses Report")
                            .Bold().FontSize(28).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                        col.Item().AlignCenter().Text(DateTime.Now.ToString("yyyy/MM/dd HH:mm")).FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });

                // جدول المراقبين
                page.Content().PaddingTop(22).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);       // #
                        columns.RelativeColumn(2.5f);     // Full Name
                        columns.RelativeColumn(2f);       // Username
                        columns.RelativeColumn(2f);       // Job Title
                        columns.RelativeColumn(2f);       // Department
                        columns.RelativeColumn(2f);       // Airport
                        columns.RelativeColumn(2f);       // Country
                        columns.RelativeColumn(2.5f);     // Email
                        columns.RelativeColumn(2f);       // Phone
                    });

                    // رأس الجدول
                    table.Header(header =>
                    {
                        header.Cell().Text("#").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Full Name").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Username").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Job Title").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Department").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Airport").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Country").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Email").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("Phone Number").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    });

                    int idx = 1;
                    foreach (var controller in controllers)
                    {
                        table.Cell().Text(idx++);
                        table.Cell().Text(controller.FullName ?? "");
                        table.Cell().Text(controller.Username ?? "");
                        table.Cell().Text(controller.JobTitle ?? "");
                        table.Cell().Text(controller.CurrentDepartment ?? "");
                        table.Cell().Text(controller.AirportName ?? "");
                        table.Cell().Text(controller.CountryName ?? "");
                        table.Cell().Text(controller.Email ?? "");
                        table.Cell().Text(controller.PhoneNumber ?? "");
                    }
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Controllers_Needing_Licenses_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// عرض الموظفين الذين يحتاجون رخص
    /// </summary>
    [HttpGet]
    public IActionResult EmployeesNeedingLicenses()
    {
        var employees = _db.GetEmployeesNeedingLicenses();
        return View(employees);
    }

    /// <summary>
    /// تصدير الموظفين الذين يحتاجون رخص إلى PDF
    /// </summary>
    [HttpGet]
    public IActionResult ExportEmployeesNeedingLicensesToPDF(string filter = "")
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var employees = _db.GetEmployeesNeedingLicenses();

        if (employees == null || employees.Rows.Count == 0)
        {
            return NotFound("لا يوجد موظفين يحتاجون رخص");
        }

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                // رأس الصفحة
                page.Header()
                    .AlignCenter()
                    .Column(col =>
                    {
                        if (System.IO.File.Exists(logoPath))
                            col.Item().AlignCenter().Height(70).Image(logoPath, QuestPDF.Infrastructure.ImageScaling.FitHeight);

                        col.Item().PaddingTop(8);
                        col.Item().AlignCenter().Text("تقرير الموظفين الذين يحتاجون رخص")
                            .Bold().FontSize(28).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                        col.Item().AlignCenter().Text(DateTime.Now.ToString("yyyy/MM/dd HH:mm")).FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });

                // جدول الموظفين
                page.Content().PaddingTop(22).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);       // #
                        columns.RelativeColumn(2.5f);     // Full Name
                        columns.RelativeColumn(2f);       // Phone Number
                        columns.RelativeColumn(2.5f);     // Email
                        columns.RelativeColumn(2f);       // Department
                        columns.RelativeColumn(2f);       // Job Title
                        columns.RelativeColumn(2f);       // Hire Date
                        columns.RelativeColumn(2f);       // Location
                    });

                    // رأس الجدول
                    table.Header(header =>
                    {
                        header.Cell().Text("#").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("اسم الموظف").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("رقم الهاتف").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("البريد الإلكتروني").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("القسم").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("المسمى الوظيفي").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("تاريخ التعيين").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("الموقع").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    });

                    int idx = 1;
                    foreach (System.Data.DataRow employee in employees.Rows)
                    {
                        table.Cell().Text(idx++);
                        table.Cell().Text(employee["FullName"]?.ToString() ?? "");
                        table.Cell().Text(employee["PhoneNumber"]?.ToString() ?? "");
                        table.Cell().Text(employee["Email"]?.ToString() ?? "");
                        table.Cell().Text(employee["Department"]?.ToString() ?? "");
                        table.Cell().Text(employee["JobTitle"]?.ToString() ?? "");
                        table.Cell().Text(employee["HireDate"] != DBNull.Value ? Convert.ToDateTime(employee["HireDate"]).ToString("yyyy-MM-dd") : "N/A");
                        table.Cell().Text(employee["OrganizationalStructure"]?.ToString() ?? "N/A");
                    }
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Employees_Needing_Licenses_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// تصدير الموظفين الذين يحتاجون رخص إلى Excel
    /// </summary>
    [HttpGet]
    public IActionResult ExportEmployeesNeedingLicensesToExcel(string filter = "")
    {
        var employees = _db.GetEmployeesNeedingLicenses();

        if (employees == null || employees.Rows.Count == 0)
        {
            return NotFound("لا يوجد موظفين يحتاجون رخص");
        }

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Employees Needing Licenses");

            // رأس الجدول
            worksheet.Cells[1, 1].Value = "#";
            worksheet.Cells[1, 2].Value = "اسم الموظف";
            worksheet.Cells[1, 3].Value = "رقم الهاتف";
            worksheet.Cells[1, 4].Value = "البريد الإلكتروني";
            worksheet.Cells[1, 5].Value = "القسم";
            worksheet.Cells[1, 6].Value = "المسمى الوظيفي";
            worksheet.Cells[1, 7].Value = "تاريخ التعيين";
            worksheet.Cells[1, 8].Value = "الموقع";

            // تنسيق رأس الجدول
            var headerRange = worksheet.Cells[1, 1, 1, 8];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            int row = 2;
            foreach (System.Data.DataRow employee in employees.Rows)
            {
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = employee["FullName"]?.ToString() ?? "";
                worksheet.Cells[row, 3].Value = employee["PhoneNumber"]?.ToString() ?? "";
                worksheet.Cells[row, 4].Value = employee["Email"]?.ToString() ?? "";
                worksheet.Cells[row, 5].Value = employee["Department"]?.ToString() ?? "";
                worksheet.Cells[row, 6].Value = employee["JobTitle"]?.ToString() ?? "";
                worksheet.Cells[row, 7].Value = employee["HireDate"] != DBNull.Value ? Convert.ToDateTime(employee["HireDate"]).ToString("yyyy-MM-dd") : "N/A";
                worksheet.Cells[row, 8].Value = employee["OrganizationalStructure"]?.ToString() ?? "N/A";
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(2, 1);

            var excelBytes = package.GetAsByteArray();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employees_Needing_Licenses_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}
