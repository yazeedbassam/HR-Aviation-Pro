// File: ViewModels/AllDetailsViewModel.cs
using System.Collections.Generic;
using WebApplication1.Models; // تأكد من وجود هذا الـ using
using WebApplication1.ViewModels; // تأكد من وجود هذا الـ using لـ ChartData إذا كنت ستستخدمها

namespace WebApplication1.ViewModels
{
    public class AllDetailsViewModel
    {
        public List<ControllerUser> AllControllers { get; set; }
        public List<Employee> AllEmployees { get; set; }
        public List<Employee> AllEmployeesAndOpsStaff { get; set; }
        public List<License> AllLicenses { get; set; } // قد تحتاج لـ ViewModel مخصص للرخص إذا أردت عرض ControllerName/EmployeeName
        public List<CertificateViewModel> AllCertificates { get; set; }
        public List<Observation> AllObservations { get; set; }
        public List<Project> AllProjects { get; set; } // إذا أردت تفاصيل المشاركين والأقسام، ستحتاج لـ ProfileProjectViewModel
                                                       // خصائص إضافية لجلب اسم الشخص والقسم والمطار
        public string ControllerName { get; set; } // اسم المراقب
        public string EmployeeName { get; set; }   // اسم الموظف
        public string ControllerCurrentDepartment { get; set; }
        public string EmployeeDepartment { get; set; }
        public string AirportName { get; set; }
        public string AirportIcao { get; set; }

        // خاصية جديدة لاسم الشخص الموحد
        public string PersonName
        {
            get { return ControllerName ?? EmployeeName; }
        }
        // يمكنك إضافة خصائص أخرى حسب الحاجة، مثل:
        // public List<Airport> AllDivisions { get; set; }
        // public List<Country> AllCountries { get; set; }

        public AllDetailsViewModel()
        {
            AllControllers = new List<ControllerUser>();
            AllEmployees = new List<Employee>();
            AllEmployeesAndOpsStaff = new List<Employee>();
            AllLicenses = new List<License>();
            AllCertificates = new List<CertificateViewModel>();
            AllObservations = new List<Observation>();
            AllProjects = new List<Project>();
        }
    }
}
