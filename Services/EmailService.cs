using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService, IDisposable
    {
        private readonly EmailConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IOptions<EmailConfiguration> config, ILogger<EmailService> logger)
        {
            try
            {
                _config = config.Value;
                _logger = logger;
                
                // تسجيل الإعدادات المستخدمة للتشخيص
                _logger.LogInformation("🔧 EmailService Configuration:");
                _logger.LogInformation("   SMTP Server: {SmtpServer}", _config.SmtpServer);
                _logger.LogInformation("   SMTP Port: {SmtpPort}", _config.SmtpPort);
                _logger.LogInformation("   Username: {Username}", _config.Username);
                _logger.LogInformation("   From Email: {FromEmail}", _config.FromEmail);
                _logger.LogInformation("   From Name: {FromName}", _config.FromName);
                _logger.LogInformation("   SSL: {EnableSsl}", _config.EnableSsl);
                _logger.LogInformation("   Timeout: {Timeout}ms", _config.Timeout);
                
                // التحقق من صحة الإعدادات
                if (string.IsNullOrEmpty(_config.SmtpServer))
                {
                    _logger.LogWarning("SMTP Server not configured, using default settings");
                    _config.SmtpServer = "smtp-relay.brevo.com";
                }
                
                if (_config.SmtpPort <= 0)
                {
                    _logger.LogWarning("SMTP Port not configured, using default port 587");
                    _config.SmtpPort = 587;
                }
                
                // لا نعيد تعيين Username - نعتمد على Program.cs
                if (string.IsNullOrEmpty(_config.Password))
                {
                    _logger.LogWarning("SMTP Password not configured, email service will not work properly");
                }
                
                // لا نعيد تعيين FromEmail - نعتمد على Program.cs
                if (string.IsNullOrEmpty(_config.FromName))
                {
                    _logger.LogWarning("From Name not configured, using default");
                    _config.FromName = "HR Aviation System";
                }
                
                _smtpClient = new SmtpClient(_config.SmtpServer)
                {
                    Port = _config.SmtpPort,
                    Credentials = new NetworkCredential(_config.Username, _config.Password),
                    EnableSsl = _config.EnableSsl,
                    Timeout = _config.Timeout,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };
                
                _logger.LogInformation("Email service initialized successfully with server: {Server}:{Port}, Username: {Username}, FromEmail: {FromEmail}", 
                    _config.SmtpServer, _config.SmtpPort, _config.Username, _config.FromEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize email service");
                throw;
            }
        }

        public async Task<bool> SendEmailAsync(EmailRequest request)
        {
            try
            {
                // التحقق من صحة البيانات
                if (string.IsNullOrEmpty(request.To))
                {
                    _logger.LogWarning("Email address is empty or null");
                    return false;
                }

                if (string.IsNullOrEmpty(_config.Password))
                {
                    _logger.LogError("SMTP Password not configured, cannot send email");
                    return false;
                }

                _logger.LogInformation("Attempting to send email to {To} from {From} using server {Server}:{Port}", 
                    request.To, _config.FromEmail, _config.SmtpServer, _config.SmtpPort);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config.FromEmail, _config.FromName),
                    Subject = request.Subject ?? "No Subject",
                    Body = request.Body ?? "No Body",
                    IsBodyHtml = request.IsHtml
                };

                mailMessage.To.Add(request.To);
                
                if (!string.IsNullOrEmpty(request.ReplyTo))
                    mailMessage.ReplyToList.Add(request.ReplyTo);

                // إضافة المرفقات
                if (request.Attachments != null)
                {
                    foreach (var attachment in request.Attachments)
                    {
                        if (attachment != null && attachment.Content != null && attachment.Content.Length > 0)
                        {
                            var ms = new MemoryStream(attachment.Content);
                            mailMessage.Attachments.Add(new Attachment(ms, attachment.FileName, attachment.ContentType));
                        }
                    }
                }

                // إرسال الإيميل
                await _smtpClient.SendMailAsync(mailMessage);
                
                if (_config.EnableLogging)
                {
                    _logger.LogInformation("Email sent successfully to {To} from {From}", request.To, _config.FromEmail);
                }
                
                return true;
            }
            catch (SmtpException smtpEx)
            {
                if (_config.EnableLogging)
                {
                    _logger.LogError(smtpEx, "SMTP Error sending email to {To} from {From}. Status: {Status}, Response: {Response}", 
                        request.To, _config.FromEmail, smtpEx.StatusCode, smtpEx.StatusCode);
                }
                return false;
            }
            catch (Exception ex)
            {
                if (_config.EnableLogging)
                {
                    _logger.LogError(ex, "Failed to send email to {To} from {From}. Error: {Error}", request.To, _config.FromEmail, ex.Message);
                }
                return false;
            }
        }

        public async Task<bool> SendLicenseExpiryAlertAsync(string to, string fullName, string licenseType, DateTime expiryDate)
        {
            var subject = "License Expiry Alert - تنبيه انتهاء صلاحية الرخصة";
            var body = GenerateLicenseExpiryEmailBody(fullName, licenseType, expiryDate);
            
            var request = new EmailRequest
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(request);
        }

        public async Task<bool> SendWeeklyReportAsync(string to, byte[] pdfAttachment, DataTable data)
        {
            var subject = "Weekly License Report - التقرير الأسبوعي للرخص";
            var body = GenerateWeeklyReportEmailBody(data);
            
            var request = new EmailRequest
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        FileName = "Weekly_Report.pdf",
                        Content = pdfAttachment,
                        ContentType = "application/pdf"
                    }
                }
            };

            return await SendEmailAsync(request);
        }

        public async Task<bool> SendContactFormAsync(string fromName, string fromEmail, string message)
        {
            var subject = "رسالة جديدة من نموذج الاتصال";
            var body = GenerateContactFormEmailBody(fromName, fromEmail, message);
            
            var request = new EmailRequest
            {
                To = _config.FromEmail, // إرسال إلى الإيميل الرسمي
                Subject = subject,
                Body = body,
                IsHtml = true,
                ReplyTo = fromEmail // للرد على المرسل
            };

            return await SendEmailAsync(request);
        }

        public async Task<bool> SendNotificationAsync(string to, string subject, string message)
        {
            var request = new EmailRequest
            {
                To = to,
                Subject = subject,
                Body = message,
                IsHtml = true
            };

            return await SendEmailAsync(request);
        }

        private string GenerateLicenseExpiryEmailBody(string fullName, string licenseType, DateTime expiryDate)
        {
            return $@"
                <html dir='rtl'>
                <body>
                    <h2>تنبيه انتهاء صلاحية الرخصة</h2>
                    <p>عزيزي {fullName}،</p>
                    <p>رخصة {licenseType} ستنتهي صلاحيتها في {expiryDate:yyyy-MM-dd}</p>
                    <p>يرجى اتخاذ الإجراءات اللازمة لتجديدها.</p>
                    <hr>
                    <p><strong>License Expiry Alert</strong></p>
                    <p>Dear {fullName},</p>
                    <p>Your {licenseType} license will expire on {expiryDate:yyyy-MM-dd}</p>
                    <p>Please take necessary actions to renew it.</p>
                </body>
                </html>";
        }

        private string GenerateWeeklyReportEmailBody(DataTable data)
        {
            var html = new StringBuilder();
            html.Append("<html dir='rtl'><body>");
            html.Append("<h2>التقرير الأسبوعي للرخص</h2>");
            html.Append("<p>مرفق التقرير الأسبوعي للرخص التي ستنتهي صلاحيتها قريباً.</p>");
            
            if (data.Rows.Count > 0)
            {
                html.Append("<table border='1' style='border-collapse: collapse; width: 100%;'>");
                html.Append("<tr><th>الاسم</th><th>نوع الرخصة</th><th>تاريخ الانتهاء</th></tr>");
                
                foreach (DataRow row in data.Rows)
                {
                    html.Append($"<tr><td>{row["fullname"]}</td><td>{row["licensetype"]}</td><td>{Convert.ToDateTime(row["expirydate"]):yyyy-MM-dd}</td></tr>");
                }
                
                html.Append("</table>");
            }
            
            html.Append("</body></html>");
            return html.ToString();
        }

        private string GenerateContactFormEmailBody(string fromName, string fromEmail, string message)
        {
            return $@"
                <html dir='rtl'>
                <body>
                    <h2>رسالة جديدة من نموذج الاتصال</h2>
                    <p><strong>الاسم:</strong> {fromName}</p>
                    <p><strong>البريد الإلكتروني:</strong> {fromEmail}</p>
                    <p><strong>الرسالة:</strong></p>
                    <p>{message}</p>
                </body>
                </html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
} 