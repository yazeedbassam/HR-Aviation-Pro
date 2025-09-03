using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DataAccess;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using WebApplication1.Services;
using WebApplication1.ViewModels; // Ensure this is included for the new ViewModels
using Microsoft.Extensions.Configuration; // Required if you inject IConfiguration
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider

namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly LicenseExpiryNotificationService _licenseExpiryNotificationService;

        // Constructor remains the same, injecting both services.
        public DashboardController(SqlServerDb db, LicenseExpiryNotificationService licenseExpiryNotificationService)
        {
            _db = db;
            _licenseExpiryNotificationService = licenseExpiryNotificationService;
        }

        // The main Dashboard action, now populated with strategic data.
        [Authorize(Roles = "Admin")]
        [Route("/dashboard")] // Kept your custom route
        public IActionResult Dashboard() // Changed from Index() to Dashboard()
        {
            var viewModel = new DashboardViewModel();
            var projectStatusCounts = _db.GetProjectStatusCounts();
            var licenseStatusCounts = _db.GetLicenseStatusCounts();
            var allCertsCount = _db.GetEmployeeCertificates().Count + _db.GetControllerCertificates().Count;

            // --- SWOT Analysis KPIs ---
            viewModel.SwotAnalysis.Strengths = new KpiViewModel { Title = "Active Courses", Value = projectStatusCounts.GetValueOrDefault("In Progress", 0).ToString(), Subtitle = "Operational Strength" };
            viewModel.SwotAnalysis.Weaknesses = new KpiViewModel { Title = "Expired Licenses", Value = licenseStatusCounts.GetValueOrDefault("Expired", 0).ToString(), Subtitle = "Compliance Risk" };
            viewModel.SwotAnalysis.Opportunities = new KpiViewModel { Title = "Staff in Training", Value = _db.GetTopStaffByCertificateCount().Count.ToString(), Subtitle = "Skill Development" };
            viewModel.SwotAnalysis.Threats = new KpiViewModel { Title = "Licenses Expiring Soon", Value = licenseStatusCounts.GetValueOrDefault("ExpiringSoon", 0).ToString(), Subtitle = "Operational Risk" };

            // --- Core Entity Summaries ---
            viewModel.DivisionSummary.TotalDivisions = _db.GetAllDivisionsForSelectList().Count;
            viewModel.DivisionSummary.TotalPersonnel = _db.GetTotalPersonnelCount();
            viewModel.DivisionSummary.PersonnelByDivisionChart = _db.GetPersonnelCountByDivision();

            viewModel.LicenseSummary.TotalLicenses = licenseStatusCounts.Sum(x => x.Value);
            viewModel.LicenseSummary.ExpiredToday = licenseStatusCounts.GetValueOrDefault("Expired", 0);
            viewModel.LicenseSummary.ExpiringSoon = licenseStatusCounts.GetValueOrDefault("ExpiringSoon", 0);

            viewModel.CertificateSummary.TotalCertificates = allCertsCount;
            // Placeholder for percentage calculation logic
            viewModel.CertificateSummary.ValidPercentage = 0; // You might want to calculate this based on your data
            viewModel.CertificateSummary.CertificatesByPersonChart = _db.GetTopStaffByCertificateCount();

            viewModel.ObservationSummary.TotalTrips = _db.GetAllObservations().Rows.Count; // This returns DataTable, you might want to map it to a specific model if needed for charts
            viewModel.ObservationSummary.TotalDays = _db.GetTotalObservationDays();
            // viewModel.ObservationSummary.TripsByPersonChart = _db.GetTripsByPersonChart(); // You need to implement this if you want to use it

            viewModel.ProjectSummary.TotalProjects = projectStatusCounts.Sum(x => x.Value);
            viewModel.ProjectSummary.InProgressCount = projectStatusCounts.GetValueOrDefault("In Progress", 0);
            viewModel.ProjectSummary.ProjectsByStatusChart = projectStatusCounts.Select(kvp => new ChartData { Label = kvp.Key, Value = kvp.Value }).ToList();


            return View("Dashboard", viewModel);
        }

        // --- NEW HTTPGET ENDPOINTS FOR DETAILS MODALS ---

        [HttpGet]
        public IActionResult GetActiveProjectsDetails()
        {
            // Assuming you have a method in SqlServerDb to get active projects
            var projects = _db.GetActiveProjectsDetails(); // You need to implement this method in SqlServerDb
            return Json(projects);
        }

        [HttpGet]
        public IActionResult GetExpiredLicensesDetails()
        {
            // This method already exists in your SqlServerDb.cs
            var licenses = _db.GetExpiredLicensesDetails(); // Make sure this returns List<License>
            return Json(licenses);
        }

        [HttpGet]
        public IActionResult GetStaffInTrainingDetails()
        {
            // Assuming GetTopStaffByCertificateCount returns List<ChartData>
            var staff = _db.GetTopStaffByCertificateCount(int.MaxValue); // Get all staff, not just top 5
            return Json(staff);
        }

        [HttpGet]
        public IActionResult GetSoonExpiringLicensesDetails()
        {
            // This method already exists in your SqlServerDb.cs
            var licenses = _db.GetSoonExpiringLicensesDetails(); // Make sure this returns List<License>
            return Json(licenses);
        }

        // --- Email sending functions remain unchanged ---

        public async Task SendWeeklyReport()
        {
            try
            {
                DataTable soonExpiringTable = _db.GetSoonExpiringLicensesTable();
                int expiredCount = _db.GetExpiredLicensesCount();
                int soonExpiringCount = _db.GetSoonExpiringLicensesCount();
                byte[] pdfBytes = _licenseExpiryNotificationService.GenerateWeeklyReportPDF(soonExpiringTable, expiredCount, soonExpiringCount);
                await _licenseExpiryNotificationService.SendWeeklyReportEmailWithPdfAndTable(pdfBytes, "yazeedbassam@hotmail.com", soonExpiringTable);
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendWeeklyReportEmail()
        {
            try
            {
                DataTable soonExpiringTable = _db.GetSoonExpiringLicensesTable();
                int expiredCount = _db.GetExpiredLicensesCount();
                int soonExpiringCount = _db.GetSoonExpiringLicensesCount();
                byte[] pdfBytes = _licenseExpiryNotificationService.GenerateWeeklyReportPDF(soonExpiringTable, expiredCount, soonExpiringCount);
                await _licenseExpiryNotificationService.SendWeeklyReportEmailWithPdfAndTable(pdfBytes, "yazeedbassam@hotmail.com", soonExpiringTable);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
