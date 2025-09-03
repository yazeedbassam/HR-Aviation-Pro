using Microsoft.Data.SqlClient; // For SqlConnection, SqlCommand, SqlParameter
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider.CreateScope
using Microsoft.Extensions.Hosting; // For BackgroundService
using Microsoft.Extensions.Logging;
// ?????? QuestPDF ?????? PDF - ???? ?? ??????? ??? NuGet
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data; // For DataTable, DataRow, DBNull
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Services;

namespace WebApplication1.Services // <== ?? ????? namespace ??? ?????? ?????? ???????
{
    // ????? ?? IHostedService ??? BackgroundService (??? ??? ?? ????? ???? ?????)
    public class LicenseExpiryNotificationService : BackgroundService
    {
        private readonly ILogger<LicenseExpiryNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider; // ?????? ???? (scope) ??? ?????

        // Constructor: ???? ??????? ???????? (ILogger, IServiceProvider, IConfiguration)
        public LicenseExpiryNotificationService(
            ILogger<LicenseExpiryNotificationService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // ??? ?? ?????? ???????? ???? ??? ??????? ???? ????? ????? ?????
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("License Expiry Notification Service starting background execution.");

            // ???? ????? ?? ??? ?? 24 ???? (?? ??? _checkInterval)
            // ?????? Task.Delay ????? ?? Timer ????? ????? Threading ?? Scoped services
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Performing daily tasks at: {time}", DateTimeOffset.Now);

                    // Skip license expiry check in production if database is not configured
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" && 
                        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_SERVER")))
                    {
                        _logger.LogInformation("Skipping license expiry check in production - database not configured.");
                        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        continue;
                    }

                    // ?? ???? ?????? ???? ???? ??? PerformLicenseExpiryCheck ????? ?????? ????? ????
                    // تم إزالة الإرسال التلقائي للبريد الإلكتروني - سيتم الإرسال فقط عند الطلب اليدوي
                    await PerformLicenseExpiryCheck(); // <== ?? ????? ?????????? ?? ????? db ???
                                                       //??? ????? ???????
                                                       // ????? ??????? ???????? ?? ??? ??? ??? ?? ????? ??? ???????
                                                       // تم إزالة الإرسال التلقائي الأسبوعي - سيتم الإرسال فقط عند الضغط على الزر
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled error occurred during license expiry check.");
                }

                // ?????? 24 ???? ??? ??????? ??????
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("License Expiry Notification Service background execution stopped.");
        }

        // ???? ??? ????????? ?? "?????" ??? ?????? ?????? ????? ??????
        // ???? ?? ???? ???? (private) ??? ?? ????????? ??????? ???
        // ?? ???? (public) ??? ???? ???????? ?? Controller ?? ???? ????
        private async Task TriggerLicenseExpiryCheck() // <== ?? ????? ??? ??????
        {
            _logger.LogInformation("PerformLicenseExpiryCheck triggered manually.");
            await PerformLicenseExpiryCheck(); // <== ?? ??????? ?????? ???? ?????
            _logger.LogInformation("PerformLicenseExpiryCheck completed from manual trigger.");
        }

        // ???? ??? ????? ???????? ?????? ????????? ???????
        // ?? ??????? ???? ???? SqlServerDb ???????
        public async Task PerformLicenseExpiryCheck()
        {
            // Skip license expiry check in production if database is not configured
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" && 
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_SERVER")))
            {
                _logger.LogInformation("Skipping license expiry check in production - database not configured.");
                return;
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    try
                    {
                        // Clear old notifications
                        try
                        {
                            db.ExecuteNonQuery("DELETE FROM notifications");
                            _logger.LogInformation("Notifications table cleared successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error clearing notifications table.");
                        }

                        // Get licenses expiring soon or expired
                        DataTable dt = new DataTable();
                        using (var connection = db.GetConnection())
                        {
                            connection.Open();
                            using (var cmd = new SqlCommand(@"
                   -- Query for licenses expiring soon
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
    'Controller' AS UserType,
    'Expiring Soon' AS Status
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
             e.department AS UserType,
             'Expiring Soon' AS Status
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

                        // Get employees and controllers who need licenses but don't have any
                        DataTable dtNeedingLicenses = new DataTable();
                        try
                        {
                            // Get employees needing licenses
                            DataTable dtEmployeesNeeding = db.GetEmployeesNeedingLicenses();
                            // Get controllers needing licenses
                            DataTable dtControllersNeeding = db.GetControllersNeedingLicenses();

                            // إنشاء جدول موحد مع أعمدة متوافقة
                            dtNeedingLicenses.Columns.Add("ControllerID", typeof(int));
                            dtNeedingLicenses.Columns.Add("FullName", typeof(string));
                            dtNeedingLicenses.Columns.Add("PhoneNumber", typeof(string));
                            dtNeedingLicenses.Columns.Add("Email", typeof(string));
                            dtNeedingLicenses.Columns.Add("Department", typeof(string));
                            dtNeedingLicenses.Columns.Add("UserType", typeof(string));
                            dtNeedingLicenses.Columns.Add("Status", typeof(string));

                            // إضافة الموظفين الذين يحتاجون رخص
                            foreach (DataRow row in dtEmployeesNeeding.Rows)
                            {
                                DataRow newRow = dtNeedingLicenses.NewRow();
                                newRow["ControllerID"] = DBNull.Value; // الموظفون ليس لديهم controllerid
                                newRow["FullName"] = row["FullName"];
                                newRow["PhoneNumber"] = row["PhoneNumber"];
                                newRow["Email"] = row["Email"];
                                newRow["Department"] = row["Department"];
                                newRow["UserType"] = "Employee";
                                newRow["Status"] = "Needs License";
                                dtNeedingLicenses.Rows.Add(newRow);
                            }

                            // إضافة المراقبين الذين يحتاجون رخص
                            foreach (DataRow row in dtControllersNeeding.Rows)
                            {
                                DataRow newRow = dtNeedingLicenses.NewRow();
                                newRow["ControllerID"] = row["ControllerID"];
                                newRow["FullName"] = row["FullName"];
                                newRow["PhoneNumber"] = row["PhoneNumber"];
                                newRow["Email"] = row["Email"];
                                newRow["Department"] = row["Department"];
                                newRow["UserType"] = row["UserType"];
                                newRow["Status"] = row["Status"];
                                dtNeedingLicenses.Rows.Add(newRow);
                            }

                            _logger.LogInformation("Found {count} people needing licenses.", dtNeedingLicenses.Rows.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error getting people needing licenses.");
                        }

                        // Combine both datasets
                        DataTable combinedDt = new DataTable();
                        if (dt.Rows.Count > 0 || dtNeedingLicenses.Rows.Count > 0)
                        {
                            // Create combined table structure
                            combinedDt.Columns.Add("licenseid", typeof(int));
                            combinedDt.Columns.Add("licensetype", typeof(string));
                            combinedDt.Columns.Add("expirydate", typeof(DateTime));
                            combinedDt.Columns.Add("userid", typeof(int));
                            combinedDt.Columns.Add("controllerid", typeof(int));
                            combinedDt.Columns.Add("fullname", typeof(string));
                            combinedDt.Columns.Add("phone_number", typeof(string));
                            combinedDt.Columns.Add("email", typeof(string));
                            combinedDt.Columns.Add("Department", typeof(string));
                            combinedDt.Columns.Add("airportname", typeof(string));
                            combinedDt.Columns.Add("UserType", typeof(string));
                            combinedDt.Columns.Add("Status", typeof(string));

                            // Add expiring licenses
                            foreach (DataRow row in dt.Rows)
                            {
                                combinedDt.ImportRow(row);
                            }

                            // Add people needing licenses
                            foreach (DataRow row in dtNeedingLicenses.Rows)
                            {
                                DataRow newRow = combinedDt.NewRow();
                                newRow["licenseid"] = DBNull.Value;
                                newRow["licensetype"] = "No License";
                                newRow["expirydate"] = DBNull.Value;
                                newRow["userid"] = DBNull.Value;
                                newRow["controllerid"] = row["ControllerID"] ?? DBNull.Value;
                                newRow["fullname"] = row["FullName"];
                                newRow["phone_number"] = row["PhoneNumber"];
                                newRow["email"] = row["Email"];
                                newRow["Department"] = row["Department"];
                                newRow["airportname"] = row["UserType"] == "Controller" ? "Airport" : "HQ - Main Office";
                                newRow["UserType"] = row["UserType"];
                                newRow["Status"] = "Needs License";
                                combinedDt.Rows.Add(newRow);
                            }
                        }

                        if (combinedDt.Rows.Count > 0)
                        {
                            _logger.LogInformation("Found {count} total notifications (expiring + needing licenses).", combinedDt.Rows.Count);

                            foreach (DataRow row in combinedDt.Rows)
                            {
                                // Handle nullable fields properly
                                int? userId = row.Field<int?>("userid");
                                int? controllerId = row.Field<int?>("controllerid");
                                DateTime? expiryDate = row.Field<DateTime?>("expirydate");
                                string licenseType = row.Field<string>("licensetype") ?? string.Empty;
                                string fullname = row.Field<string>("fullname") ?? string.Empty;
                                string toEmail = row.Field<string>("email") ?? string.Empty;

                                string status = row.Field<string>("Status") ?? "Expiring Soon";
                                string msg = "";
                                
                                if (status == "Needs License")
                                {
                                    msg = $"Dear {fullname}, You need to obtain a license for your position as {row.Field<string>("Department")}.\n\nPlease contact HR to arrange for license application.\n\n???? ????? ????????? ??????? ????????.";
                                }
                                else
                                {
                                    msg = $"Dear {fullname}, Your {licenseType} will expire on {expiryDate:yyyy-MM-dd}.\n\nPlease update your license before expiry.\n\n???? ????? ????????? ??????? ????????.";
                                }

                                // Insert notification into database
                                db.ExecuteNonQuery(
                                    "INSERT INTO notifications (userid, controllerid, message, licensetype, licenseexpirydate, created_at, is_read) VALUES (@userid, @controllerid, @message, @licensetype, @expirydate, GETDATE(), 0)",
                                    new SqlParameter("@userid", (object)userId ?? DBNull.Value),
                                    new SqlParameter("@controllerid", (object)controllerId ?? DBNull.Value),
                                    new SqlParameter("@message", msg),
                                    new SqlParameter("@licensetype", licenseType),
                                    new SqlParameter("@expirydate", (object)expiryDate ?? DBNull.Value)
                                );
                                _logger.LogInformation("Inserted new notification for user {userId}, controller {controllerId}.", userId, controllerId);

                                // Send email notification using EmailService
                                if (!string.IsNullOrWhiteSpace(toEmail))
                                {
                                    try
                                    {
                                        if (status == "Needs License")
                                        {
                                            // Send notification for people needing licenses
                                            await emailService.SendNotificationAsync(toEmail, "License Required", msg);
                                        }
                                        else
                                        {
                                            // Send license expiry alert
                                            await emailService.SendLicenseExpiryAlertAsync(toEmail, fullname, licenseType, expiryDate.Value);
                                        }
                                        
                                        _logger.LogInformation("Sent expiry email to {email}.", toEmail);
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

        // ???? ????? ????? PDF (????? ?????? QuestPDF)
        public byte[] GenerateWeeklyReportPDF(DataTable soonExpiringTable, int expiredCount, int soonExpiringCount) // <== ?? ????? ?????? ??? public
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
                        .Text("??????? ???????? ?????").Bold().FontSize(22).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content().PaddingTop(15).Column(mainCol =>
                    {
                        mainCol.Item().Text($"??? ????? ????????: {expiredCount}").FontSize(15).Bold().FontColor(QuestPDF.Helpers.Colors.Red.Medium);
                        mainCol.Item().Text($"??? ????? ???? ?????? ???? 30 ???: {soonExpiringCount}").FontSize(15).Bold().FontColor(QuestPDF.Helpers.Colors.Orange.Medium);

                        mainCol.Item().PaddingTop(8);

                        // ???? ?????
                        mainCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2); // ?????
                                columns.RelativeColumn(2); // ??? ??????
                                columns.RelativeColumn(2); // ????? ????????
                                columns.RelativeColumn(2); // Mobile
                                columns.RelativeColumn(2); // Email
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("?????").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("??? ??????").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
                                header.Cell().Text("????? ????????").Bold().BackgroundColor(QuestPDF.Helpers.Colors.Blue.Medium).FontColor(QuestPDF.Helpers.Colors.White);
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
                        .Text("????? ?????? - ???? ????? ???????? ?????? - " + DateTime.Now.ToString("yyyy/MM/dd"))
                        .FontSize(10)
                        .FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                });
            });

            return document.GeneratePdf();
        }

        // ???? ????? ?????? ?????????? ?? ??? PDF ????? HTML ?? ??? ???????
        public async Task SendWeeklyReportEmailWithPdfAndTable(byte[] pdfBytes, string recipientEmail, DataTable soonExpiringTable) // <== ?? ????? ?????? ??? public ?????? ?????
        {
            _logger.LogInformation("Sending weekly report email to {recipientEmail}...", recipientEmail);
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    
                    // إرسال البريد الإلكتروني باستخدام EmailService
                    await emailService.SendWeeklyReportAsync(recipientEmail, pdfBytes, soonExpiringTable);
                    
                    _logger.LogInformation("Weekly report email sent successfully to {recipientEmail}.", recipientEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending weekly report email to {recipientEmail}.", recipientEmail);
                throw; // إعادة رمي الخطأ للمعالجة في المستوى الأعلى
            }
        }
    }
}

