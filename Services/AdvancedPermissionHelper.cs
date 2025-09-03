using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services
{
    public static class AdvancedPermissionHelper
    {
        public static bool HasAdvancedPermission(this IHtmlHelper html, string permissionKey, int? departmentId = null)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: PermissionManagerService is null");
                    return false;
                }
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: User not authenticated or invalid user ID. Claim: {userIdClaim?.Value}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"🔥 AdvancedPermissionHelper: Checking permission '{permissionKey}' for user {userId}");
                
                        // NEW SYSTEM ONLY: Parse permission key to get entity type and operation
        if (permissionKey.StartsWith("EMPLOYEES_") || permissionKey.StartsWith("CONTROLLERS_") || 
            permissionKey.StartsWith("LICENSES_") || permissionKey.StartsWith("CERTIFICATES_") || 
            permissionKey.StartsWith("OBSERVATIONS_") || permissionKey.StartsWith("CONTROLLERLICENSE_") ||
            permissionKey.StartsWith("EMPLOYEELICENSE_") || permissionKey.StartsWith("CONTROLLERCERTIFICATE_") ||
            permissionKey.StartsWith("EMPLOYEECERTIFICATE_") || permissionKey.StartsWith("CONTROLLEROBSERVATION_") ||
            permissionKey.StartsWith("EMPLOYEEOBSERVATION_") || permissionKey.StartsWith("PROJECT_") ||
            permissionKey.StartsWith("COUNTRY_") || permissionKey.StartsWith("AIRPORT_"))
                {
                    // Parse the permission key to get entity type and operation
                    var parts = permissionKey.Split('_');
                    if (parts.Length >= 2)
                    {
                        string entityType;
                        var operationType = parts[1];
                        
                        // Handle special cases for license and certificate permissions
                        if (permissionKey.StartsWith("CONTROLLERLICENSE_"))
                        {
                            entityType = "ControllerLicense";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEELICENSE_"))
                        {
                            entityType = "EmployeeLicense";
                        }
                        else if (permissionKey.StartsWith("CONTROLLERCERTIFICATE_"))
                        {
                            entityType = "ControllerCertificate";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEECERTIFICATE_"))
                        {
                            entityType = "EmployeeCertificate";
                        }
                        else if (permissionKey.StartsWith("CONTROLLEROBSERVATION_"))
                        {
                            entityType = "ControllerObservation";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEEOBSERVATION_"))
                        {
                            entityType = "EmployeeObservation";
                        }
                        else if (permissionKey.StartsWith("PROJECT_"))
                        {
                            entityType = "Project";
                        }
                        else
                        {
                            entityType = parts[0].Substring(0, parts[0].Length - 1); // Remove 'S' from EMPLOYEES -> EMPLOYEE
                        }
                        
                        // Convert to proper case
                        entityType = char.ToUpper(entityType[0]) + entityType.Substring(1).ToLower();
                        operationType = char.ToUpper(operationType[0]) + operationType.Substring(1).ToLower();
                        
                        System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Checking permission: {entityType}.{operationType}");
                        
                        // Force cache invalidation by checking if permissions were updated
                        var cache = html.ViewContext.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                        var userUpdateKey = $"user_permission_updated_{userId}";
                        cache.TryGetValue(userUpdateKey, out object? lastUpdateObj);
                        DateTime? lastUpdate = lastUpdateObj as DateTime?;
                        
                        if (lastUpdate.HasValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Last permission update for user {userId}: {lastUpdate.Value}");
                        }
                        
                        var result = permissionManagerService.CanPerformOperationAsync(userId, entityType, operationType).GetAwaiter().GetResult();
                        
                        System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Permission '{entityType}.{operationType}' for user {userId} = {result}");
                        
                        return result;
                    }
                }
                
                // For other permissions, return false (old system removed)
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Unknown permission key '{permissionKey}' - returning false");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking permission '{permissionKey}': {ex.Message}");
                return false;
            }
        }
        
        public static bool HasAnyAdvancedPermission(this IHtmlHelper html, params string[] permissionKeys)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                // Check each permission using the new system
                foreach (var permissionKey in permissionKeys)
                {
                    if (html.HasAdvancedPermission(permissionKey))
                        return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking any permission: {ex.Message}");
                return false;
            }
        }
        
        public static bool HasAllAdvancedPermissions(this IHtmlHelper html, params string[] permissionKeys)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                // Check each permission using the new system
                foreach (var permissionKey in permissionKeys)
                {
                    if (!html.HasAdvancedPermission(permissionKey))
                        return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking all permissions: {ex.Message}");
                return false;
            }
        }
        
        public static bool IsMenuItemVisible(this IHtmlHelper html, string menuItemKey)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionManagerService.CanViewMenuAsync(userId, menuItemKey).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking menu item visibility: {ex.Message}");
                return false;
            }
        }
        
        public static List<MenuItemPermission> GetVisibleMenuItems(this IHtmlHelper html)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                {
                    System.Diagnostics.Debug.WriteLine("🔥 GetVisibleMenuItems: AdvancedPermissionManagerService is null!");
                    return new List<MenuItemPermission>();
                }
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"🔥 GetVisibleMenuItems: User not authenticated or invalid user ID. Claim: {userIdClaim?.Value}");
                    return new List<MenuItemPermission>();
                }
                
                System.Diagnostics.Debug.WriteLine($"🔥 GetVisibleMenuItems: Getting visible menu items for user {userId}");
                
                // Force clear cache first
                permissionManagerService.ClearAllUserCaches(userId);
                
                var result = permissionManagerService.GetUserMenuPermissionsAsync(userId).GetAwaiter().GetResult();
                var visibleItems = result.Where(m => m.IsVisible).Select(m => new MenuItemPermission 
                { 
                    Key = m.MenuKey, 
                    Text = m.MenuKey, 
                    IsVisible = m.IsVisible 
                }).ToList();
                
                System.Diagnostics.Debug.WriteLine($"🔥 GetVisibleMenuItems: User {userId} - Returning {visibleItems.Count} visible menu items: {string.Join(", ", visibleItems.Select(x => $"{x.Key}"))}");
                
                return visibleItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔥 GetVisibleMenuItems: Error getting visible menu items: {ex.Message}");
                return new List<MenuItemPermission>();
            }
        }
        
        public static bool CanAccessUserData(this IHtmlHelper html, int targetUserId)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                    return false;
                
                var result = permissionManagerService.CanAccessEntityAsync(currentUserId, "User", targetUserId, "View").GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking user data access: {ex.Message}");
                return false;
            }
        }
        
        public static bool CanAccessDepartmentData(this IHtmlHelper html, int departmentId)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionManagerService.CanAccessEntityAsync(userId, "Department", departmentId, "View").GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking department data access: {ex.Message}");
                return false;
            }
        }
        
        public static bool IsAdvancedInRole(this IHtmlHelper html, string role)
        {
            return html.ViewContext.HttpContext.User.IsInRole(role);
        }
        
        public static bool IsAdvancedAdmin(this IHtmlHelper html)
        {
            var result = html.IsAdvancedInRole("Admin");
            System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: IsAdvancedAdmin = {result}");
            return result;
        }
        
        public static bool IsAdvancedController(this IHtmlHelper html)
        {
            return html.IsAdvancedInRole("Controller") || html.IsAdvancedInRole("Admin");
        }
        
        public static int GetCurrentUserId(this IHtmlHelper html)
        {
            var userIdClaim = html.ViewContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
            return 0;
        }
        
        public static string GetCurrentUsername(this IHtmlHelper html)
        {
            return html.ViewContext.HttpContext.User.Identity?.Name ?? "";
        }

        // HttpContext Extension Methods for Controllers
        public static bool HasAdvancedPermission(this HttpContext httpContext, string permissionKey, int? departmentId = null)
        {
            try
            {
                var permissionManagerService = httpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
                
                if (permissionManagerService == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: PermissionManagerService is null");
                    return false;
                }
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: User not authenticated or invalid user ID. Claim: {userIdClaim?.Value}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"🔥 AdvancedPermissionHelper: Checking permission '{permissionKey}' for user {userId}");
                
                // NEW SYSTEM ONLY: Parse permission key to get entity type and operation
                if (permissionKey.StartsWith("EMPLOYEES_") || permissionKey.StartsWith("CONTROLLERS_") || 
                    permissionKey.StartsWith("LICENSES_") || permissionKey.StartsWith("CERTIFICATES_") || 
                    permissionKey.StartsWith("OBSERVATIONS_") || permissionKey.StartsWith("CONTROLLERLICENSE_") ||
                    permissionKey.StartsWith("EMPLOYEELICENSE_") || permissionKey.StartsWith("CONTROLLERCERTIFICATE_") ||
                    permissionKey.StartsWith("EMPLOYEECERTIFICATE_") || permissionKey.StartsWith("CONTROLLEROBSERVATION_") ||
                    permissionKey.StartsWith("EMPLOYEEOBSERVATION_") || permissionKey.StartsWith("PROJECT_") ||
                    permissionKey.StartsWith("COUNTRY_") || permissionKey.StartsWith("AIRPORT_"))
                {
                    // Parse the permission key to get entity type and operation
                    var parts = permissionKey.Split('_');
                    if (parts.Length >= 2)
                    {
                        string entityType;
                        var operationType = parts[1];
                        
                        // Handle special cases for license, certificate, observation, and project permissions
                        if (permissionKey.StartsWith("CONTROLLERLICENSE_"))
                        {
                            entityType = "ControllerLicense";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEELICENSE_"))
                        {
                            entityType = "EmployeeLicense";
                        }
                        else if (permissionKey.StartsWith("CONTROLLERCERTIFICATE_"))
                        {
                            entityType = "ControllerCertificate";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEECERTIFICATE_"))
                        {
                            entityType = "EmployeeCertificate";
                        }
                        else if (permissionKey.StartsWith("CONTROLLEROBSERVATION_"))
                        {
                            entityType = "ControllerObservation";
                        }
                        else if (permissionKey.StartsWith("EMPLOYEEOBSERVATION_"))
                        {
                            entityType = "EmployeeObservation";
                        }
                        else if (permissionKey.StartsWith("PROJECT_"))
                        {
                            entityType = "Project";
                        }
                        else if (permissionKey.StartsWith("COUNTRY_"))
                        {
                            entityType = "Country";
                        }
                        else if (permissionKey.StartsWith("AIRPORT_"))
                        {
                            entityType = "Airport";
                        }
                        else
                        {
                            entityType = parts[0].Substring(0, parts[0].Length - 1); // Remove 'S' from EMPLOYEES -> EMPLOYEE
                        }
                        
                        // Convert to proper case
                        entityType = char.ToUpper(entityType[0]) + entityType.Substring(1).ToLower();
                        operationType = char.ToUpper(operationType[0]) + operationType.Substring(1).ToLower();
                        
                        System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Checking permission: {entityType}.{operationType}");
                        
                        // Force cache invalidation by checking if permissions were updated
                        var cache = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                        var userUpdateKey = $"user_permission_updated_{userId}";
                        cache.TryGetValue(userUpdateKey, out object? lastUpdateObj);
                        DateTime? lastUpdate = lastUpdateObj as DateTime?;
                        
                        if (lastUpdate.HasValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Last permission update for user {userId}: {lastUpdate.Value}");
                        }
                        
                        var result = permissionManagerService.CanPerformOperationAsync(userId, entityType, operationType).GetAwaiter().GetResult();
                        
                        System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Permission '{entityType}.{operationType}' for user {userId} = {result}");
                        
                        return result;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking permission '{permissionKey}': {ex.Message}");
                return false;
            }
        }

        public static bool IsAdvancedAdmin(this HttpContext httpContext)
        {
            try
            {
                var user = httpContext.User;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    return false;
                }

                var roleClaim = user.FindFirst(ClaimTypes.Role);
                if (roleClaim == null)
                {
                    return false;
                }

                var isAdmin = roleClaim.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: IsAdvancedAdmin = {isAdmin} (Role: {roleClaim.Value})");
                
                return isAdmin;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error checking admin status: {ex.Message}");
                return false;
            }
        }
    }
} 