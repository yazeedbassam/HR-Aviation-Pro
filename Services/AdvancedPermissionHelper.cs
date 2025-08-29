using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public static class AdvancedPermissionHelper
    {
        public static bool HasAdvancedPermission(this IHtmlHelper html, string permissionKey, int? departmentId = null)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: AdvancedPermissionService is null");
                    return false;
                }
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: User not authenticated or invalid user ID. Claim: {userIdClaim?.Value}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Checking permission '{permissionKey}' for user {userId}");
                
                // Use synchronous version for view rendering
                var result = permissionService.HasPermissionAsync(userId, permissionKey, departmentId).GetAwaiter().GetResult();
                
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Permission '{permissionKey}' for user {userId} = {result}");
                
                return result;
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
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionService.HasAnyPermissionAsync(userId, permissionKeys).GetAwaiter().GetResult();
                return result;
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
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionService.HasAllPermissionsAsync(userId, permissionKeys).GetAwaiter().GetResult();
                return result;
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
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionService.IsMenuItemVisibleAsync(userId, menuItemKey).GetAwaiter().GetResult();
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
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return new List<MenuItemPermission>();
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return new List<MenuItemPermission>();
                
                var result = permissionService.GetVisibleMenuItemsAsync(userId).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedPermissionHelper: Error getting visible menu items: {ex.Message}");
                return new List<MenuItemPermission>();
            }
        }
        
        public static bool CanAccessUserData(this IHtmlHelper html, int targetUserId)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                    return false;
                
                var result = permissionService.CanAccessUserDataAsync(currentUserId, targetUserId).GetAwaiter().GetResult();
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
                var permissionService = httpContext.RequestServices.GetService<IAdvancedPermissionService>();
                
                if (permissionService == null) 
                    return false;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return false;
                
                var result = permissionService.CanAccessDepartmentDataAsync(userId, departmentId).GetAwaiter().GetResult();
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
            return html.IsAdvancedInRole("Admin");
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
    }
} 