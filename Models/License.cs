using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
namespace WebApplication1.Models
{
    public class License
    {
        public int LicenseId { get; set; }
        public string? LicenseType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Note { get; set; }
        public string? PDFPath { get; set; }
        public string? licensenumber { get; set; }
        public string? RANGE { get; set; } // For TACC Endorsement etc.
        public string? AlertMessage { get; set; }

        // --- Controller Specific Fields ---
        // We make these nullable (int?) because a license will belong to either a controller OR an employee, not both.
        public int? ControllerId { get; set; }
        public string? ControllerName { get; set; }
        public string? ControllerCurrentDepartment { get; set; }
        public string? AirportName { get; set; }
        public string? AirportIcao { get; set; }

        // --- Employee Specific Fields ---
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeDepartment { get; set; }


        [NotMapped] // This is important: It tells the database not to create a column for this field.
        public IFormFile? LicenseFile { get; set; }

        public string? PersonName { get; set; }
        public string? FilePath { get; set; }
        public string? TypeName { get; set; }
        public string? Status { get; set; }

    }
}
