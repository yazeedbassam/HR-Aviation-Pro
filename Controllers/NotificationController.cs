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

        // --- FIX V2: Pass null for empty/whitespace filters ---
        // If the filter is empty, pass null to GetNotifications to signify "no filter".
        string effectiveFilter = string.IsNullOrWhiteSpace(filter) ? null : filter;
        var notifications = _db.GetNotifications(effectiveFilter);

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
                        col.Item().AlignCenter().Text("تقرير الإشعارات")
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
                        // columns.RelativeColumn(3f);       // Message
                        // columns.RelativeColumn(1.6f);     // Link
                        // columns.RelativeColumn(1.8f);     // CreatedAt
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
                        table.Cell().Text(n.UserId ?? "");
                        // هنا ضع اسم المراقب وليس رقم الـController
                        table.Cell().Text(n.FullName ?? "");  // تأكد أن جلب الاسم في الموديل
                        table.Cell().Text(n.Note ?? "");
                        table.Cell().Text(n.LicenseType ?? "");
                        table.Cell().Text(n.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? "");
                        table.Cell().Text(n.phonenumber ?? "");
                        table.Cell().Text(n.Email ?? "");
                        table.Cell().Text(n.Location ?? "");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .PaddingTop(8)
                    .Text("تقرير إشعارات - نظام إدارة المراقبة الجوية - " + DateTime.Now.ToString("yyyy/MM/dd"))
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

        // --- FIX V2: Pass null for empty/whitespace filters ---
        // If the filter is empty, pass null to GetNotifications to signify "no filter".
        string effectiveFilter = string.IsNullOrWhiteSpace(filter) ? null : filter;
        var notifications = _db.GetNotifications(effectiveFilter);

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
                worksheet.Cells[row, 2].Value = n.UserId;
                worksheet.Cells[row, 3].Value = n.FullName; // اسم المراقب الجوي (تأكد موجود في الموديل)
                worksheet.Cells[row, 4].Value = n.Message;
                worksheet.Cells[row, 5].Value = n.CreatedAt?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 6].Value = n.Note;
                worksheet.Cells[row, 7].Value = n.LicenseType;
                worksheet.Cells[row, 8].Value = n.LicenseExpiryDate?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 9].Value = n.phonenumber;
                worksheet.Cells[row, 10].Value = n.Email;
                worksheet.Cells[row, 11].Value = n.Location;
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
        ViewBag.Title = "المراقبين الذين يحتاجون رخص";
        ViewBag.Subtitle = "المراقبين النشطين الذين يحتاجون رخص ولكن ليس لديهم رخص";
        return View("ControllersNotifications", controllers);
    }

    /// <summary>
    /// عرض المراقبين غير النشطين
    /// </summary>
    [HttpGet]
    public IActionResult InactiveControllers()
    {
        var controllers = _licenseNotificationService.GetInactiveControllers();
        ViewBag.Title = "المراقبين غير النشطين";
        ViewBag.Subtitle = "المراقبين الذين تم تعطيلهم";
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
            return NotFound("لا يوجد مراقبين يحتاجون رخص");
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
                        col.Item().AlignCenter().Text("تقرير المراقبين الذين يحتاجون رخص")
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
                        header.Cell().Text("الاسم الكامل").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("اسم المستخدم").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("المسمى الوظيفي").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("القسم").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("المطار").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("البلد").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("البريد الإلكتروني").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        header.Cell().Text("رقم الهاتف").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
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
}
