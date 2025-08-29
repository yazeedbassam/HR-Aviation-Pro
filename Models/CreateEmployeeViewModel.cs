using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class CreateEmployeeViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Display(Name = "Employee Official ID")]
    public string? EmployeeOfficialID { get; set; }

    [Display(Name = "Job Title")]
    public string? JobTitle { get; set; }

    public string? Department { get; set; }

    public string? Location { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    // ===== الحقول الجديدة المضافة =====
    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateTime? HireDate { get; set; } = DateTime.Today; // قيمة افتراضية بتاريخ اليوم

    public string? Address { get; set; }

    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true; // قيمة افتراضية أن الموظف فعال
                                               // ===== نهاية الحقول الجديدة =====
    public string? Gender { get; set; }

    // --- حقول خاصة بإنشاء المستخدم ---
    [Required]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string RoleName { get; set; }

    //*************************
    // Additional Personal Information
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Marital Status")]
    public string? MaritalStatus { get; set; }

    [Display(Name = "Education Level")]
    public string? EducationLevel { get; set; }

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
    // License Information
    [Display(Name = "Need License")]
    public bool NeedLicense { get; set; } = true; // Default to true

    //*************************
    // Photo Information
    [Display(Name = "Photo")]
    public string? PhotoPath { get; set; }

    //*************************
    // Organizational Structure
    [Display(Name = "Organizational Structure")]
    public string? OrganizationalStructure { get; set; }

    [Display(Name = "Division")]
    public string? Division { get; set; }
}