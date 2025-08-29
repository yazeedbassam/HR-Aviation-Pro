using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Models
{
    public class NotificationModel
    {
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public string ControllerId { get; set; }
        public string FullName { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Note { get; set; }
        public string LicenseType { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }


        public string phonenumber { get; set; }
        public string Email { get; set; }
        public string Currentdepartment { get; set; }
        public string Location { get; set; }
    }

}
