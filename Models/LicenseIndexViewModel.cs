using System.Collections.Generic;

namespace WebApplication1.Models
{
    /// <summary>
    /// This ViewModel is used to carry both controller licenses and employee licenses
    /// to the main Licenses Index view.
    /// </summary>
    public class LicenseIndexViewModel
    {
        public List<License> ControllerLicenses { get; set; }
        public List<License> EmployeesAndOpsStaffLicenses { get; set; }

        public LicenseIndexViewModel()
        {
            ControllerLicenses = new List<License>();
            EmployeesAndOpsStaffLicenses = new List<License>();
        }
    }
}
