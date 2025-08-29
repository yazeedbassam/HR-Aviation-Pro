// استبدل كل الكود الحالي في ProfileViewModel.cs بهذا الكود

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using WebApplication1.ViewModels; // Ensure you have this

namespace WebApplication1.Models
{
    public class ProfileViewModel
    {
        // --- Common Properties for Both User Types ---
        public string UserType { get; set; } // "Controller" or "Employee"
        public int UserId { get; set; } // This will hold ControllerId or EmployeeID
        public string FullName { get; set; }
        public string Username { get; set; }
        public string? JobTitle { get; set; }
        public string? PhotoPath { get; set; }
        public IFormFile? PhotoFile { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public System.DateTime? HireDate { get; set; }
        public string? CurrentDepartment { get; set; }
        public string? Address { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? EmergencyContact { get; set; }

        // --- Controller-Specific Properties ---
        public System.DateTime? DateOfBirth { get; set; }
        public string? MaritalStatus { get; set; }
        public string? EducationLevel { get; set; }

        // --- Employee-Specific Properties ---
        public string? Gender { get; set; }
        public string? Location { get; set; }


        // --- Security ---
        public string? Password { get; set; } // Optional: only for changing password

        // --- Related Data ---
        public List<License>? Licenses { get; set; }
        public List<CertificateViewModel>? Certificates { get; set; }
        public List<Observation>? Observations { get; set; }
        public List<ProfileProjectViewModel> Projects { get; set; }

        public ProfileViewModel()
        {
            Licenses = new List<License>();
            Certificates = new List<CertificateViewModel>();
            Observations = new List<Observation>();
            Projects = new List<ProfileProjectViewModel>();
        }
    }
}
