using System.Threading.Tasks;
using System.Data;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailRequest request);
        Task<bool> SendLicenseExpiryAlertAsync(string to, string fullName, string licenseType, DateTime expiryDate);
        Task<bool> SendWeeklyReportAsync(string to, byte[] pdfAttachment, DataTable data);
        Task<bool> SendContactFormAsync(string fromName, string fromEmail, string message);
        Task<bool> SendNotificationAsync(string to, string subject, string message);
    }

    public class EmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachment> Attachments { get; set; } = new();
        public string? ReplyTo { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = new byte[0];
        public string ContentType { get; set; } = string.Empty;
    }
} 