using System;

namespace WebApplication1.Models
{
    public class CertificateViewModel
    {
        public int CertificateId { get; set; }

        public int TypeId { get; set; }
        public string? ControllerName { get; set; } = "";
        public string? TypeName { get; set; } = "";
        public string? Title { get; set; } = "";
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Status { get; set; } = "";
        public string? FilePath { get; set; }    // ← علشان رابط التحميل
        public string? Notes { get; set; }       // ← علشان الملاحظات

        public IFormFile? UploadFile { get; set; }
        // --- Person's Info ---
        public int? ControllerId { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public string? EmployeeDepartment { get; set; }

        // This property will hold either the Controller's or Employee's name
        public string? PersonName { get; set; }
        //public string? PersonName => ControllerName ?? EmployeeName;
    }
}
