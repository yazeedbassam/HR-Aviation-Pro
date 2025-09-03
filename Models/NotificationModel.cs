using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Models
{
    public class NotificationModel
    {
        public string NotificationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ControllerId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Note { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime? LicenseExpiryDate { get; set; }

        public string phonenumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Currentdepartment { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

}
