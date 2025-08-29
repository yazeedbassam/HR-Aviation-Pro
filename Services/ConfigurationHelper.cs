using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Services;

namespace WebApplication1.Services
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Extension method to load dropdown values from configuration
        /// </summary>
        public static void LoadConfigurationDropdowns(this Controller controller, IConfigurationService configService)
        {
            // Load all configuration dropdowns
            controller.ViewBag.JobTitles = configService.GetDropdownValues("JobTitles");
            controller.ViewBag.Departments = configService.GetDropdownValues("Departments");
            controller.ViewBag.Roles = configService.GetDropdownValues("Roles");
            controller.ViewBag.LicenseTypes = configService.GetDropdownValues("LicenseTypes");
            controller.ViewBag.EmploymentStatus = configService.GetDropdownValues("EmploymentStatus");
            controller.ViewBag.EducationLevels = configService.GetDropdownValues("EducationLevels");
            controller.ViewBag.MaritalStatus = configService.GetDropdownValues("MaritalStatus");
            controller.ViewBag.Gender = configService.GetDropdownValues("Gender");
            controller.ViewBag.ProjectStatuses = configService.GetDropdownValues("ProjectStatuses");
            controller.ViewBag.ProjectTypes = configService.GetDropdownValues("ProjectTypes");
            controller.ViewBag.CertificateTypes = configService.GetDropdownValues("CertificateTypes");
            controller.ViewBag.IssuingAuthorities = configService.GetDropdownValues("IssuingAuthorities");
        }

        /// <summary>
        /// Extension method to load specific dropdown values
        /// </summary>
        public static void LoadConfigurationDropdown(this Controller controller, IConfigurationService configService, string categoryName, string viewBagName)
        {
            var values = configService.GetDropdownValues(categoryName);
            controller.ViewData[viewBagName] = values;
        }

        /// <summary>
        /// Extension method to get configuration value text by key
        /// </summary>
        public static string GetConfigurationValueText(this IConfigurationService configService, string categoryName, string valueKey)
        {
            var values = configService.GetValuesByCategory(categoryName);
            var value = values.FirstOrDefault(v => v.ValueKey.Equals(valueKey, StringComparison.OrdinalIgnoreCase));
            return value?.ValueText ?? valueKey;
        }

        /// <summary>
        /// Extension method to check if configuration value exists
        /// </summary>
        public static bool ConfigurationValueExists(this IConfigurationService configService, string categoryName, string valueKey)
        {
            var values = configService.GetValuesByCategory(categoryName);
            return values.Any(v => v.ValueKey.Equals(valueKey, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Extension method to get all configuration values as dictionary
        /// </summary>
        public static Dictionary<string, string> GetConfigurationDictionary(this IConfigurationService configService, string categoryName)
        {
            var values = configService.GetValuesByCategory(categoryName);
            return values.ToDictionary(v => v.ValueKey, v => v.ValueText);
        }

        /// <summary>
        /// Extension method to validate configuration value
        /// </summary>
        public static bool ValidateConfigurationValue(this IConfigurationService configService, string categoryName, string valueKey)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(valueKey))
                return false;

            return configService.ConfigurationValueExists(categoryName, valueKey);
        }

        /// <summary>
        /// Extension method to get configuration select list with default option
        /// </summary>
        public static List<SelectListItem> GetConfigurationSelectListWithDefault(this IConfigurationService configService, string categoryName, string defaultText = "-- Select --")
        {
            var values = configService.GetDropdownValues(categoryName);
            var result = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = defaultText }
            };
            result.AddRange(values);
            return result;
        }

        /// <summary>
        /// Extension method to get configuration select list with selected value
        /// </summary>
        public static List<SelectListItem> GetConfigurationSelectListWithSelected(this IConfigurationService configService, string categoryName, string selectedValue)
        {
            var values = configService.GetDropdownValues(categoryName);
            foreach (var item in values)
            {
                if (item.Value.Equals(selectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                    break;
                }
            }
            return values;
        }
    }

    /// <summary>
    /// Configuration constants for category names
    /// </summary>
    public static class ConfigurationCategories
    {
        public const string JobTitles = "JobTitles";
        public const string Departments = "Departments";
        public const string Roles = "Roles";
        public const string LicenseTypes = "LicenseTypes";
        public const string EmploymentStatus = "EmploymentStatus";
        public const string EducationLevels = "EducationLevels";
        public const string MaritalStatus = "MaritalStatus";
        public const string Gender = "Gender";
        public const string ProjectStatuses = "ProjectStatuses";
        public const string ProjectTypes = "ProjectTypes";
        public const string CertificateTypes = "CertificateTypes";
        public const string IssuingAuthorities = "IssuingAuthorities";
    }

    /// <summary>
    /// Configuration constants for common value keys
    /// </summary>
    public static class ConfigurationValues
    {
        // Job Titles
        public const string ATC = "ATC";
        public const string SUPERVISOR = "SUPERVISOR";
        public const string INSTRUCTOR = "INSTRUCTOR";
        public const string MANAGER = "MANAGER";
        public const string AIS_OFFICER = "AIS_OFFICER";
        public const string AIS_TECHNICIAN = "AIS_TECHNICIAN";

        // Departments
        public const string AIS = "AIS";
        public const string CNS = "CNS";
        public const string ADMINISTRATION = "ADMINISTRATION";
        public const string QUEEN = "QUEEN";
        public const string AQABA = "AQABA";
        public const string AMMAN = "AMMAN";

        // Roles
        public const string ADMIN = "ADMIN";
        public const string CONTROLLER = "CONTROLLER";
        public const string EMPLOYEE = "EMPLOYEE";

        // Employment Status
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
        public const string ON_LEAVE = "ON_LEAVE";
        public const string TERMINATED = "TERMINATED";

        // Education Levels
        public const string DIPLOMA = "DIPLOMA";
        public const string BACHELOR = "BACHELOR";
        public const string MASTER = "MASTER";
        public const string PHD = "PHD";

        // Marital Status
        public const string SINGLE = "SINGLE";
        public const string MARRIED = "MARRIED";
        public const string DIVORCED = "DIVORCED";
        public const string WIDOWED = "WIDOWED";

        // Gender
        public const string MALE = "MALE";
        public const string FEMALE = "FEMALE";

        // Project Statuses
        public const string COMPLETED = "COMPLETED";
        public const string ON_HOLD = "ON_HOLD";
        public const string CANCELLED = "CANCELLED";
        public const string PLANNING = "PLANNING";

        // License Types
        public const string ATC_LICENSE = "ATC_LICENSE";
        public const string MEDICAL_CERTIFICATE = "MEDICAL_CERTIFICATE";
        public const string TRAINING_CERTIFICATE = "TRAINING_CERTIFICATE";
        public const string PROFESSIONAL_LICENSE = "PROFESSIONAL_LICENSE";
    }
} 