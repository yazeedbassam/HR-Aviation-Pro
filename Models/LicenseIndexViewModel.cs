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
        public List<License> AISLicenses { get; set; }
        public List<License> CNSLicenses { get; set; }
        public List<License> AFTNLicenses { get; set; }
        public List<License> OpsStaffLicenses { get; set; }

        public LicenseIndexViewModel()
        {
            ControllerLicenses = new List<License>();
            AISLicenses = new List<License>();
            CNSLicenses = new List<License>();
            AFTNLicenses = new List<License>();
            OpsStaffLicenses = new List<License>();
        }
    }
}
