using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace WebApplication1.Services
{
    public static class DynamicPermissionHelper
    {
        // Employee License Permission Methods
        public static bool CanViewEmployeeLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("EMPLOYEELICENSE_VIEW") || html.IsAdvancedAdmin();
        }

        public static bool CanAddEmployeeLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("EMPLOYEELICENSE_ADD") || html.IsAdvancedAdmin();
        }

        public static bool CanEditEmployeeLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("EMPLOYEELICENSE_EDIT") || html.IsAdvancedAdmin();
        }

        public static bool CanDeleteEmployeeLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("EMPLOYEELICENSE_DELETE") || html.IsAdvancedAdmin();
        }

        public static bool CanExportEmployeeLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("EMPLOYEELICENSE_EXPORT") || html.IsAdvancedAdmin();
        }

        // Controller License Permission Methods
        public static bool CanViewControllerLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("CONTROLLERLICENSE_VIEW") || html.IsAdvancedAdmin();
        }

        public static bool CanAddControllerLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("CONTROLLERLICENSE_ADD") || html.IsAdvancedAdmin();
        }

        public static bool CanEditControllerLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("CONTROLLERLICENSE_EDIT") || html.IsAdvancedAdmin();
        }

        public static bool CanDeleteControllerLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("CONTROLLERLICENSE_DELETE") || html.IsAdvancedAdmin();
        }

        public static bool CanExportControllerLicense(this IHtmlHelper html)
        {
            return html.HasAdvancedPermission("CONTROLLERLICENSE_EXPORT") || html.IsAdvancedAdmin();
        }

        /// <summary>
        /// Generic permission checker for any entity type and operation
        /// </summary>
        public static bool CanPerformOperation(this IHtmlHelper html, string entityType, string operationType)
        {
            var permissionKey = $"{entityType.ToUpper()}_{operationType.ToUpper()}";
            return html.HasAdvancedPermission(permissionKey) || html.IsAdvancedAdmin();
        }

        /// <summary>
        /// Get all available operations for a specific entity type based on user permissions
        /// </summary>
        public static List<string> GetAvailableOperations(this IHtmlHelper html, string entityType)
        {
            var operations = new List<string>();
            var operationTypes = new[] { "View", "Add", "Edit", "Delete", "Export" };

            foreach (var operation in operationTypes)
            {
                if (html.CanPerformOperation(entityType, operation))
                {
                    operations.Add(operation);
                }
            }

            return operations;
        }

        /// <summary>
        /// Check if user has any permission for a specific entity type
        /// </summary>
        public static bool HasAnyPermissionForEntity(this IHtmlHelper html, string entityType)
        {
            var operations = new[] { "View", "Add", "Edit", "Delete", "Export" };
            return operations.Any(op => html.CanPerformOperation(entityType, op));
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        public static int GetCurrentUserIdDynamic(this IHtmlHelper html)
        {
            var userIdClaim = html.ViewContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        /// <summary>
        /// Get current user role
        /// </summary>
        public static string GetCurrentUserRoleDynamic(this IHtmlHelper html)
        {
            var user = html.ViewContext.HttpContext.User;
            if (user.IsInRole("Admin")) return "Admin";
            if (user.IsInRole("Controller")) return "Controller";
            if (user.IsInRole("Employee")) return "Employee";
            return "Unknown";
        }

        /// <summary>
        /// Log permission check for debugging
        /// </summary>
        public static void LogPermissionCheck(this IHtmlHelper html, string entityType, string operationType, bool result)
        {
            var userId = html.GetCurrentUserIdDynamic();
            var userRole = html.GetCurrentUserRoleDynamic();
            System.Diagnostics.Debug.WriteLine($"🔍 Permission Check - User: {userId} ({userRole}), Entity: {entityType}, Operation: {operationType}, Result: {result}");
        }
    }
}