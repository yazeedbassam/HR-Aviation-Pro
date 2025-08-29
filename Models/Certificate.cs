using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models
{
    public class Certificate
    {
        public int CertificateId { get; set; }

        [Required]
        public int ControllerId { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public string CertificateTitle { get; set; } = "-";

        public string? IssuingAuthority { get; set; }
        public string? IssuingCountry { get; set; }

        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        //[Required]
        public string? Status { get; set; } = "";

        public string? StatusReason { get; set; }

        public string? FilePath { get; set; }

        public string? Notes { get; set; }
        public string? TypeName { get; set; }
        // **שדה הקובץ** שצריך ב־Create/Edit
        [Display(Name = "Upload File")]
        public IFormFile? UploadFile { get; set; }  // إشارة الـ '?' بتخليه اختياري
      


    }
}
