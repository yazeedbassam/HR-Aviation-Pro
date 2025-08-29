using Microsoft.Data.SqlClient; // For SqlConnection, SqlCommand, SqlParameter, SqlDbType
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider.CreateScope
using Microsoft.Extensions.Hosting; // For BackgroundService
using Microsoft.Extensions.Logging;
// مكتبات QuestPDF لإنشاء PDF - تأكد من تثبيتها عبر NuGet
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data; // For DataTable, DataRow, DBNull
using System.Net; // For NetworkCredential
using System.Net.Mail; // For SmtpClient, MailMessage
using System.Text; // For StringBuilder (used in HTML table generation)
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using System.Net.Mail;
using System.Collections.Generic;
using Microsoft.Extensions.Logging; // بما أنك تستخدم _logger

namespace WebApplication1.Services // <== تم إضافة namespace هنا لتغليف الكلاس بالكامل
{
    // تغيير من IHostedService إلى BackgroundService (كما كان في الكود الذي قدمته)
    public class LicenseExpiryNotificationService : BackgroundService
    {
        private readonly ILogger<LicenseExpiryNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider; // لإنشاء نطاق (scope) لكل تشغيل
        private DateTime _lastWeeklyReportSent = DateTime.MinValue; // لتتبع آخر مرة تم فيها إرسال التقرير الأسبوعي

        // إعدادات SMTP من appsettings.json - يجب أن تُقرأ من IConfiguration
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail; // البريد الإلكتروني للمرسل

        // Constructor: لحقن الخدمات المطلوبة (ILogger, IServiceProvider, IConfiguration)
        public LicenseExpiryNotificationService(
            ILogger<LicenseExpiryNotificationService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            // قراءة إعدادات SMTP من التكوين (appsettings.json)
            // استخدام ?? throw new ArgumentNullException لضمان وجود القيم
            _smtpServer = configuration["SmtpSettings:Server"] ?? throw new ArgumentNullException("SMTP Server not found in appsettings.json");
            _smtpPort = int.Parse(configuration["SmtpSettings:Port"] ?? throw new ArgumentNullException("SMTP Port not found in appsettings.json"));
            _smtpUsername = configuration["SmtpSettings:Username"] ?? throw new ArgumentNullException("SMTP Username not found in appsettings.json");
            _smtpPassword = configuration["SmtpSettings:Password"] ?? throw new ArgumentNullException("SMTP Password not found in appsettings.json");
            _fromEmail = configuration["SmtpSettings:ReceiverEmail"] ?? throw new ArgumentNullException("SMTP ReceiverEmail (From Email) not found in appsettings.json");
        }

        // هذه هي الدالة الأساسية التي يتم تشغيلها بشكل متكرر كخدمة خلفية
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("License Expiry Notification Service starting background execution.");

            // ابدأ فورًا ثم كرر كل 24 ساعة (أو حسب _checkInterval)
            // استخدم Task.Delay بدلاً من Timer لتجنب مشاكل Threading مع Scoped services
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Performing daily tasks at: {time}", DateTimeOffset.Now);

                    // لا حاجة لإنشاء نطاق هنا، لأن PerformLicenseExpiryCheck ستنشئ نطاقها الخاص الآن
                    await PerformLicenseExpiryCheck(); // <== تم تحديث الاستدعاء، لا تمرير db هنا
                                                       //عند بداية التشغيل
                                                       // إرسال التقرير الأسبوعي كل أحد فقط إذا لم يُرسل هذا الأسبوع
                                                       // تأكد أن `GenerateWeeklyReportPDF` و `SendWeeklyReportEmailWithPdfAndTable` موجودين في هذه الفئة
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday &&
                        (DateTime.Now.Date > _lastWeeklyReportSent.Date)) // مقارنة بالتاريخ فقط
                    {
                        try
                        {
                            _logger.LogInformation("Attempting to send weekly report...");

                            // جلب البيانات اللازمة للتقرير من الداتا بيز
                            // يجب جلب DB هنا لأن GetSoonExpiringLicensesTable() ليست جزءًا من PerformLicenseExpiryCheck
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();

                                // نفترض وجود هذه الدوال في SqlServerDb
                                DataTable soonExpiringTable = db.GetSoonExpiringLicensesTable();
                                int expiredCount = db.GetExpiredLicensesCount();
                                int soonExpiringCount = db.GetSoonExpiringLicensesCount();

                                // توليد ملف PDF للتقرير
                                byte[] pdfBytes = GenerateWeeklyReportPDF(soonExpiringTable, expiredCount, soonExpiringCount);

                                // إرسال البريد الإلكتروني مع ملف PDF وجدول HTML (الآن تم دمجها)
                                // يمكنك تغيير "yazeedbassam@hotmail.com" ليكون بريد إلكتروني محدد للمسؤول الذي سيتلقى التقرير.
                                //   التشغيل لاحقا   //  await SendWeeklyReportEmailWithPdfAndTable(pdfBytes, "yazeedbassam@hotmail.com", soonExpiringTable);

                                _lastWeeklyReportSent = DateTime.Now.Date; // تحديث تاريخ آخر إرسال
                                _logger.LogInformation("Weekly report sent successfully at: {time}", DateTimeOffset.Now);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending weekly report.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled error occurred during license expiry check.");
                }

                // انتظار 24 ساعة قبل التشغيل التالي
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("License Expiry Notification Service background execution stopped.");
        }

        // دالة يتم استدعاؤها لـ "تفعيل" فحص انتهاء صلاحية الرخص يدويًا
        // يمكن أن تكون خاصة (private) إذا تم استدعاؤها داخليًا فقط
        // أو عامة (public) إذا كانت ستُستدعى من Controller أو خدمة أخرى
        private async Task TriggerLicenseExpiryCheck() // <== تم إضافة هذه الدالة
        {
            _logger.LogInformation("PerformLicenseExpiryCheck triggered manually.");
            await PerformLicenseExpiryCheck(); // <== تم استدعاء الدالة بدون معامل
            _logger.LogInformation("PerformLicenseExpiryCheck completed from manual trigger.");
        }

        // دالة فحص الرخص المنتهية وإرسال الإشعارات الفردية
        // تم تعديلها لجلب مثيل SqlServerDb داخليًا
        public async Task PerformLicenseExpiryCheck()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();

                    try
                    {
                        // تفريغ جدول notifications قبل إضافة التنبيهات الجديدة
                        try
                        {
                            db.ExecuteNonQuery("DELETE FROM notifications");
                            _logger.LogInformation("Notifications table cleared successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error clearing notifications table.");
                        }

                        DataTable dt = new DataTable();
                        using (var connection = db.GetConnection())
                        {
                            connection.Open();
                            using (var cmd = new SqlCommand(@"
                   -- القسم الأول: جلب الرخص التي ستنتهي للمراقبين
                 SELECT
    l.licenseid,
    l.licensetype,
    l.expirydate,
    c.userid,
    c.controllerid,
    c.fullname,
    c.phone_number,
    c.email,
    c.current_department AS Department,
    a.airportname,
    'Controller' AS UserType
FROM
    licenses l
INNER JOIN
    controllers c ON l.controllerid = c.controllerid
INNER JOIN
    airports a ON c.airportid = a.airportid
WHERE
    l.expirydate <= DATEADD(day, 90, GETDATE())

UNION ALL

         SELECT
             l.licenseid,
             l.licensetype,
             l.expirydate,
             e.userid,
             NULL AS controllerid,
             e.fullname,
             e.phonenumber,
             e.email,
             e.department AS Department,
             'HQ - Main Office' AS airportname,
             e.department AS UserType
         FROM
             licenses l
         INNER JOIN
             employees e ON l.employeeid = e.employeeid
         WHERE
             l.expirydate <= DATEADD(day, 90, GETDATE());
                    ", connection))
                            {
                                using (var adapter = new SqlDataAdapter(cmd))
                                {
                                    adapter.Fill(dt);
                                }
                            }
                        }

                        if (dt.Rows.Count > 0)
                        {
                            _logger.LogInformation("Found {count} licenses expiring soon or expired.", dt.Rows.Count);

                            foreach (DataRow row in dt.Rows)
                            {
                                // التعامل الآمن مع القيم التي قد تكون NULL
                                int? userId = row.Field<int?>("userid");
                                int? controllerId = row.Field<int?>("controllerid");
                                DateTime expiryDate = row.Field<DateTime>("expirydate");
                                string licenseType = row.Field<string>("licensetype") ?? string.Empty;
                                string fullname = row.Field<string>("fullname") ?? string.Empty;
                                string toEmail = row.Field<string>("email") ?? string.Empty;

                                string msg = $"Dear {fullname}, Your {licenseType} will expire : \n\n At {expiryDate:yyyy-MM-dd} :(.\n\n So, Please Update :). \n\nيرجى اتخاذ الإجراءات اللازمة لتجديدها.";

                                // إدراج التنبيه الجديد
                                db.ExecuteNonQuery(
                                    "INSERT INTO notifications (userid, controllerid, message, licensetype, licenseexpirydate, created_at, is_read) VALUES (@userid, @controllerid, @message, @licensetype, @expirydate, GETDATE(), 0)",
                                    new Microsoft.Data.SqlClient.SqlParameter("@userid", SqlDbType.Int) { Value = userId ?? (object)DBNull.Value }, // التعامل مع NULL
                                    new Microsoft.Data.SqlClient.SqlParameter("@controllerid", SqlDbType.Int) { Value = controllerId ?? (object)DBNull.Value }, // التعامل مع NULL
                                    new Microsoft.Data.SqlClient.SqlParameter("@message", SqlDbType.NVarChar, -1) { Value = msg },
                                    new Microsoft.Data.SqlClient.SqlParameter("@licensetype", SqlDbType.NVarChar, 255) { Value = licenseType },
                                    new Microsoft.Data.SqlClient.SqlParameter("@expirydate", SqlDbType.DateTime2) { Value = expiryDate }
                                );
                                _logger.LogInformation("Inserted new notification for user {userId}, controller {controllerId}.", userId, controllerId);

                                // إرسال البريد الإلكتروني
                                if (!string.IsNullOrWhiteSpace(toEmail))
                                {
                                    try
                                    {
                                        using (var smtp = new SmtpClient(_smtpServer))
                                        {
                                            smtp.Port = _smtpPort;
                                            smtp.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                                            smtp.EnableSsl = true;

                                            using (var mail = new MailMessage(_fromEmail, toEmail, "License Expiry Alert", msg))
                                            {
                                                // await smtp.SendMailAsync(mail);
                                                _logger.LogInformation("Sent expiry email to {email}.", toEmail);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error sending email notification to {email}.", toEmail);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Email address is missing for {fullname}.", fullname);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No licenses expiring soon or expired found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unhandled error occurred during license expiry check.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create service scope or access database in PerformLicenseExpiryCheck.");
            }
        }

        // دالة إنشاء تقرير PDF (تحتاج لمكتبة QuestPDF)
        public byte[] GenerateWeeklyReportPDF(DataTable soonExpiringTable, int expiredCount, int soonExpiringCount) // <== تم تغيير الوصول إلى public
        {
            _logger.LogInformation("Generating weekly report PDF...");
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header()
                        .AlignCenter()
                        .Text("التقرير الأسبوعي للرخص").Bold().FontSize(22).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content().PaddingTop(15).Column(mainCol =>
                    {
                        mainCol.Item().Text($"عدد الرخص المنتهية: {expiredCount}").FontSize(15).Bold().FontColor(QuestPDF.Helpers.Colors.Red.Medium);
                        mainCol.Item().Text($"عدد الرخص التي ستنتهي خلال 30 يوم: {soonExpiringCount}").FontSize(15).Bold().FontColor(QuestPDF.Helpers.Colors.Orange.Medium);

                        mainCol.Item().PaddingTop(8);

                        // جدول مختصر
                        mainCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2); // الاسم
                                columns.RelativeColumn(2); // نوع الرخصة
                                columns.RelativeColumn(2); // تاريخ الانتهاء
                                columns.RelativeColumn(2); // Mobile
                                columns.RelativeColumn(2); // Email
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("الاسم").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("نوع الرخصة").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("تاريخ الانتهاء").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("Mobile").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("Email").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                            });

                            int idx = 1;
                            foreach (DataRow r in soonExpiringTable.Rows)
                            {
                                table.Cell().Text(idx++);
                                table.Cell().Text(r["fullname"].ToString() ?? string.Empty);
                                table.Cell().Text(r["licensetype"].ToString() ?? string.Empty);
                                table.Cell().Text(Convert.ToDateTime(r["expirydate"]).ToString("yyyy-MM-dd"));
                                table.Cell().Text(r["phone_number"].ToString() ?? string.Empty);
                                table.Cell().Text(r["email"].ToString() ?? string.Empty);
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("تقرير أسبوعي - نظام إدارة المراقبة الجوية - " + DateTime.Now.ToString("yyyy/MM/dd"))
                        .FontSize(10)
                        .FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                });
            });

            return document.GeneratePdf();
        }

        // دالة إرسال البريد الإلكتروني مع ملف PDF وجدول HTML في جسم الرسالة
        public async Task SendWeeklyReportEmailWithPdfAndTable(byte[] pdfBytes, string recipientEmail, DataTable soonExpiringTable) // <== تم تغيير الوصول إلى public وتغيير الاسم
        {
            _logger.LogInformation("Sending weekly report email to {recipientEmail}...", recipientEmail);
            try
            {
                var fromAddress = new MailAddress(_fromEmail, "ANS Management system \r\n");
                var toAddress = new MailAddress(recipientEmail);
                const string subject = "Weekly License Expiry Report";

                // بناء جدول HTML من soonExpiringTable
                StringBuilder tableHtmlBuilder = new StringBuilder();
                tableHtmlBuilder.Append("<table border='1' cellpadding='6' style='border-collapse:collapse; font-family:Tahoma; font-size:14px; width:100%;'>");
                tableHtmlBuilder.Append("<thead><tr style='background-color:#f2f2f2;'><th>#</th><th>Full Name</th><th>License Type</th><th>Expiry Date</th><th>Phone</th><th>Email</th></tr></thead>");
                tableHtmlBuilder.Append("<tbody>");

                int idx = 1;
                foreach (DataRow r in soonExpiringTable.Rows)
                {
                    tableHtmlBuilder.Append($"<tr>");
                    tableHtmlBuilder.Append($"<td>{r["fullname"]?.ToString() ?? string.Empty}</td>");
                    tableHtmlBuilder.Append($"<td>{r["licensetype"]?.ToString() ?? string.Empty}</td>");
                    tableHtmlBuilder.Append($"<td>{Convert.ToDateTime(r["expirydate"]).ToString("yyyy-MM-dd")}</td>");
                    tableHtmlBuilder.Append($"<td>{r["phone_number"]?.ToString() ?? string.Empty}</td>");
                    tableHtmlBuilder.Append($"<td>{r["email"]?.ToString() ?? string.Empty}</td>");
                    tableHtmlBuilder.Append($"</tr>");
                }
                tableHtmlBuilder.Append("</tbody></table>");

                string body = "Please find attached your weekly license expiry report.<br/><br/>" +
                              "Here is a summary of licenses expiring soon or expired:<br/><br/>" +
                              tableHtmlBuilder.ToString();

                using (var smtp = new SmtpClient(_smtpServer))
                {
                    smtp.Port = _smtpPort;
                    smtp.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtp.EnableSsl = true;

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true // مهم لعرض الجدول في نص الرسالة بشكل صحيح
                    })
                    {
                        using (var ms = new System.IO.MemoryStream(pdfBytes)) // استخدام System.IO.MemoryStream
                        {
                            // استخدام System.Net.Mail.Attachment لحل التعارض
                            var attachment = new System.Net.Mail.Attachment(ms, "Weekly_Report.pdf", "application/pdf");
                            message.Attachments.Add(attachment);
                            await smtp.SendMailAsync(message);
                        }
                    }
                }
                _logger.LogInformation("Weekly report email sent successfully to {recipientEmail}.", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending weekly report email to {recipientEmail}.", recipientEmail);
            }
        }
    }
}
