// File: Controllers/DataDetailsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Data; // لـ DataTable

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")] // يمكن تقييد الوصول للمدراء فقط
    public class DataDetailsController : Controller
    {
        private readonly SqlServerDb _db;

        public DataDetailsController(SqlServerDb db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var viewModel = new AllDetailsViewModel();

            // جلب جميع البيانات باستخدام الدوال الموجودة في SqlServerDb
            var controllersDt = _db.GetAllControllers();
            viewModel.AllControllers = controllersDt.AsEnumerable().Select(row => new ControllerUser
            {
                ControllerId = Convert.ToInt32(row["controllerid"]),
                FullName = row["fullname"].ToString(),
                Username = row["username"].ToString(),
                Email = row["email"]?.ToString(),
                PhoneNumber = row["phone_number"]?.ToString(),
                JobTitle = row["job_title"]?.ToString(),
                CurrentDepartment = row["current_department"]?.ToString(),
                EmploymentStatus = row["employment_status"]?.ToString(),
                // أضف بقية الخصائص التي تحتاجها
            }).ToList();

            try
            {
                viewModel.AllEmployees = _db.GetEmployees(null, null, null, null, null, null, null, null, null);
                System.Diagnostics.Debug.WriteLine($"DataDetails - AllEmployees loaded: {viewModel.AllEmployees?.Count ?? 0}");
                
                // إنشاء قائمة الموظفين المدمجين (AIS, CNS, AFTN, Ops Staff)
                var allEmployees = _db.GetEmployees(null, null, null, null, null, null, null, null, null);
                
                // فلترة أكثر مرونة للأقسام
                viewModel.AllEmployeesAndOpsStaff = allEmployees?.Where(e => 
                    !string.IsNullOrEmpty(e.Department) && (
                        e.Department.Contains("AIS", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("CNS", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("AFTN", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("Administration", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("Safety", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("Quality", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("Ops", StringComparison.OrdinalIgnoreCase) ||
                        e.Department.Contains("Operations", StringComparison.OrdinalIgnoreCase)
                    )
                ).ToList() ?? new List<Employee>();
                
                // إذا لم نجد أي موظفين بالفلترة، نعرض جميع الموظفين
                if (!viewModel.AllEmployeesAndOpsStaff.Any() && allEmployees?.Any() == true)
                {
                    System.Diagnostics.Debug.WriteLine("DataDetails - No employees found with department filter, showing all employees");
                    viewModel.AllEmployeesAndOpsStaff = allEmployees.ToList();
                }
                
                System.Diagnostics.Debug.WriteLine($"DataDetails - AllEmployeesAndOpsStaff filtered: {viewModel.AllEmployeesAndOpsStaff?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Error loading Employees: {ex.Message}");
                viewModel.AllEmployees = new List<Employee>();
                viewModel.AllEmployeesAndOpsStaff = new List<Employee>();
            }

            // الآن نستخدم الدوال الجديدة التي تجمع البيانات
            try
            {
                viewModel.AllLicenses = _db.GetAllLicensesDetailsMix();
                System.Diagnostics.Debug.WriteLine($"DataDetails - Licenses loaded: {viewModel.AllLicenses?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Error loading Licenses: {ex.Message}");
                viewModel.AllLicenses = new List<License>();
            }

            try
            {
                viewModel.AllCertificates = _db.GetAllCertificatesDetailsMix();
                System.Diagnostics.Debug.WriteLine($"DataDetails - Certificates loaded: {viewModel.AllCertificates?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Error loading Certificates: {ex.Message}");
                viewModel.AllCertificates = new List<CertificateViewModel>();
            }

            try
            {
                viewModel.AllObservations = _db.GetAllObservationsDetails();
                System.Diagnostics.Debug.WriteLine($"DataDetails - Observations loaded: {viewModel.AllObservations?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Error loading Observations: {ex.Message}");
                viewModel.AllObservations = new List<Observation>();
            }

            try
            {
                viewModel.AllProjects = _db.GetAllProjects();
                System.Diagnostics.Debug.WriteLine($"DataDetails - Projects loaded: {viewModel.AllProjects?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Error loading Projects: {ex.Message}");
                viewModel.AllProjects = new List<Project>();
            }

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"DataDetails - Controllers: {viewModel.AllControllers?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - Employees: {viewModel.AllEmployees?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - AllEmployeesAndOpsStaff: {viewModel.AllEmployeesAndOpsStaff?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - Licenses: {viewModel.AllLicenses?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - Certificates: {viewModel.AllCertificates?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - Observations: {viewModel.AllObservations?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"DataDetails - Projects: {viewModel.AllProjects?.Count ?? 0}");
            
            // Debug logging for employee departments
            if (viewModel.AllEmployees?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Employee departments found:");
                foreach (var emp in viewModel.AllEmployees.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"  - {emp.FullName}: {emp.Department}");
                }
            }
            
            if (viewModel.AllEmployeesAndOpsStaff?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - Filtered employees found:");
                foreach (var emp in viewModel.AllEmployeesAndOpsStaff.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"  - {emp.FullName}: {emp.Department}");
                }
            }

            // Additional detailed logging
            if (viewModel.AllLicenses?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - First License: {viewModel.AllLicenses[0].LicenseType ?? "NULL"} - {viewModel.AllLicenses[0].licensenumber ?? "NULL"}");
            }
            if (viewModel.AllCertificates?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - First Certificate: {viewModel.AllCertificates[0].TypeName ?? "NULL"} - {viewModel.AllCertificates[0].PersonName ?? "NULL"}");
            }
            if (viewModel.AllObservations?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"DataDetails - First Observation: {viewModel.AllObservations[0].ObservationNo} - {viewModel.AllObservations[0].PersonName ?? "NULL"}");
            }

            return View(viewModel);
        }
    }
}
