using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models
{
    public class ControllerUser
    {
        public int ControllerId { get; set; } // تأكد من أن هذا هو الحقل المطلوب
        public int UserId { get; set; } // تأكد من أن هذا هو الحقل المطلوب

        [Required(ErrorMessage = "The FullName field is required.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "The Username is required.")]
        public string? Username { get; set; }

        // اجعل Password قابلاً للقيم الفارغة للإشارة إلى أنه ليس مطلوبًا بشكل ضمني بواسطة NRTs
        // وللسماح بتركه فارغًا عند التعديل (لعدم تغيير كلمة المرور)
        //public string? Password { get; set; }
        [Required(ErrorMessage = "The Password is required.")]
        public string? Password { get; set; }    // <-- جديد
        //public string? ControllerPassword { get; set; }
        public int CountryId { get; set; }
        [Required(ErrorMessage = "The Division field is required.")] // المطار يجب أن يكون مطلوبًا
        public int AirportId { get; set; }
        // [Required]
        public string? AirportName { get; set; }
        public string? icao_code { get; set; }
        public string? CountryName { get; set; } // <-- ADD THIS LINE

        // [Required(ErrorMessage = "The Role is required.")]
        public string? Role { get; set; }    // جديد

        //[Required(ErrorMessage = "The Photo is required.")]
        public string? PhotoPath { get; set; }  // غير مطلوب
        public string? LicensePath { get; set; }  // غير مطلوب

        // لا تجعل PhotoFile و LicenseFile مطلوبين
        public IFormFile? PhotoFile { get; set; }  // ليس REQUIRED
        public IFormFile? LicenseFile { get; set; }  // ليس REQUIRED
        //**************Other Information
        public string? JobTitle { get; set; }
        public string? EducationLevel { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }
        public string? MaritalStatus { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? CurrentDepartment { get; set; }
        public DateTime? TransferDate { get; set; }
        public string? EmergencyContact { get; set; }

        public string? LicenseNumber { get; set; }

        //*************************
        // New fields for license and active status
        [Display(Name = "Need License")]
        public bool NeedLicense { get; set; } = true; // Default to true

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true; // Default to true

        //*************************

        //*************************
        // Financial Information
        [Display(Name = "Current Salary")]
        [DataType(DataType.Currency)]
        public decimal? CurrentSalary { get; set; }

        [Display(Name = "Annual Increase %")]
        [Range(0, 100, ErrorMessage = "Annual increase must be between 0 and 100")]
        public decimal? AnnualIncreasePercentage { get; set; }

        [Display(Name = "Salary After Annual Increase")]
        [DataType(DataType.Currency)]
        public decimal? SalaryAfterAnnualIncrease { get; set; }

        [Display(Name = "Bank Account Number")]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        [Display(Name = "Tax ID")]
        public string? TaxId { get; set; }

        [Display(Name = "Insurance Number")]
        public string? InsuranceNumber { get; set; }

        //*************************

    }


}
