using System.Collections.Generic;
using WebApplication1.Models; // Ensure access to the Project model

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// The main container for all dashboard data, structured for strategic analysis.
    /// </summary>
    public class DashboardViewModel
    {
        // Section 1: SWOT Analysis KPIs
        public SwotSectionViewModel SwotAnalysis { get; set; }

        // Section 2: Core Entity Summaries
        public DivisionSummaryViewModel DivisionSummary { get; set; }
        public LicenseSummaryViewModel LicenseSummary { get; set; }
        public CertificateSummaryViewModel CertificateSummary { get; set; }
        public ObservationSummaryViewModel ObservationSummary { get; set; }
        public ProjectSummaryViewModel ProjectSummary { get; set; }

        public DashboardViewModel()
        {
            SwotAnalysis = new SwotSectionViewModel();
            DivisionSummary = new DivisionSummaryViewModel();
            LicenseSummary = new LicenseSummaryViewModel();
            CertificateSummary = new CertificateSummaryViewModel();
            ObservationSummary = new ObservationSummaryViewModel();
            ProjectSummary = new ProjectSummaryViewModel();
        }
    }

    // --- Supporting ViewModels for each Dashboard Section ---

    #region SWOT and KPI ViewModels

    /// <summary>
    /// Represents a single KPI card used for the SWOT analysis.
    /// </summary>
    public class KpiViewModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public string IconCssClass { get; set; }
        public string ColorCssClass { get; set; }
        public string DetailsUrl { get; set; } // URL to see the detailed data
    }

    /// <summary>
    /// Holds the four main components of the SWOT analysis.
    /// </summary>
    public class SwotSectionViewModel
    {
        public KpiViewModel Strengths { get; set; }      // e.g., Total active projects, Recently certified staff
        public KpiViewModel Weaknesses { get; set; }     // e.g., Expired licenses, Overdue projects
        public KpiViewModel Opportunities { get; set; }  // e.g., Staff in new courses, New high-value projects
        public KpiViewModel Threats { get; set; }        // e.g., Licenses expiring soon
    }

    #endregion

    #region Detailed Summary ViewModels

    /// <summary>
    /// Holds summary data for all Divisions/Airports.
    /// </summary>
    public class DivisionSummaryViewModel
    {
        public int TotalDivisions { get; set; }
        public int TotalPersonnel { get; set; }
        public List<ChartData> PersonnelByDivisionChart { get; set; }
        // When clicked, this will show a modal with a detailed list of personnel per division.
    }

    /// <summary>
    /// Holds summary data for all Licenses.
    /// </summary>
    public class LicenseSummaryViewModel
    {
        public int TotalLicenses { get; set; }
        public int ExpiredToday { get; set; }
        public int ExpiringSoon { get; set; }
        public List<ChartData> LicensesByTypeChart { get; set; }
        // Each part of the summary is clickable for details.
    }

    /// <summary>
    /// Holds summary data for all Certificates and Courses.
    /// </summary>
    public class CertificateSummaryViewModel
    {
        public int TotalCertificates { get; set; }
        public int ValidPercentage { get; set; }
        public int ExpiredPercentage { get; set; }
        public List<ChartData> CertificatesByPersonChart { get; set; } // Top 5 staff with most certs
    }

    /// <summary>
    /// Holds summary data for Observations/Trips.
    /// </summary>
    public class ObservationSummaryViewModel
    {
        public int TotalTrips { get; set; }
        public int TotalDays { get; set; }
        public List<ChartData> TripsByPersonChart { get; set; } // Top 5 staff with most trips
    }

    /// <summary>
    /// Holds summary data for Projects.
    /// </summary>
    public class ProjectSummaryViewModel
    {
        public int TotalProjects { get; set; }
        public int InProgressCount { get; set; }
        public List<ChartData> ProjectsByStatusChart { get; set; }
    }

    #endregion

    #region Generic Charting ViewModel

    /// <summary>
    /// A flexible model for chart data. It can be used for Pie, Bar, etc.
    /// </summary>
    public class ChartData
    {
        public string Label { get; set; }   // e.g., "Planning", "Amman TACC", "Lina Murad"
        public int Value { get; set; }      // e.g., 5, 20, 15
        public string Color { get; set; }   // Optional: e.g., "rgba(255, 193, 7, 0.7)"
        public string DetailsUrl { get; set; } // Optional: URL to navigate to when a segment is clicked
    }

    #endregion
}
