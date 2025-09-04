// In a new file: ViewModels/EmployeeDetailViewModel.cs
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// Holds all the necessary data for the Employee Details page.
    /// </summary>
    public class EmployeeDetailViewModel
    {
        public Employee Employee { get; set; } = new Employee { FullName = string.Empty };
        public List<License> Licenses { get; set; }
        public List<CertificateViewModel> Certificates { get; set; }
        public List<Observation> Observations { get; set; }
        public List<ProfileProjectViewModel> Projects { get; set; } // Reusing this from the Profile section

        public EmployeeDetailViewModel()
        {
            // Initialize lists to avoid null reference errors in the view
            Licenses = new List<License>();
            Certificates = new List<CertificateViewModel>();
            Observations = new List<Observation>();
            Projects = new List<ProfileProjectViewModel>();
        }
    }
}
