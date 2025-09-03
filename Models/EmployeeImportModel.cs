using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class EmployeeImportModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [Display(Name = "Custom Password")]
    public string? CustomPassword { get; set; }

    [Display(Name = "Employee Official ID")]
    public string? EmployeeOfficialID { get; set; }

    [Display(Name = "Job Title")]
    public string? JobTitle { get; set; }

    [Display(Name = "Department")]
    public string? Department { get; set; }

    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }

    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Marital Status")]
    public string? MaritalStatus { get; set; }

    [Display(Name = "Education Level")]
    public string? EducationLevel { get; set; }

    [Display(Name = "Hire Date")]
    public DateTime? HireDate { get; set; }

    [Display(Name = "Current Salary")]
    public decimal? CurrentSalary { get; set; }

    [Display(Name = "Annual Increase Percentage")]
    public decimal? AnnualIncreasePercentage { get; set; }

    [Display(Name = "Bank Account Number")]
    public string? BankAccountNumber { get; set; }

    [Display(Name = "Bank Name")]
    public string? BankName { get; set; }

    [Display(Name = "Tax ID")]
    public string? TaxId { get; set; }

    [Display(Name = "Insurance Number")]
    public string? InsuranceNumber { get; set; }

    [Display(Name = "Organizational Structure")]
    public string? OrganizationalStructure { get; set; }

    [Display(Name = "Division")]
    public string? Division { get; set; }

    [Display(Name = "Role")]
    public string Role { get; set; } = "Employee";

    [Display(Name = "Need License")]
    public bool NeedLicense { get; set; } = true;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
} 