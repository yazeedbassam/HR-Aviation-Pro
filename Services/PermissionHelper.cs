using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace WebApplication1.Services
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this IHtmlHelper html, string permissionKey, int? departmentId = null)
        {
            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
                
                if (permissionService == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"PermissionHelper: PermissionService is null");
                    return false;
                }
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"PermissionHelper: User not authenticated or invalid user ID. Claim: {userIdClaim?.Value}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"PermissionHelper: Checking permission '{permissionKey}' for user {userId}");
                
                // Use synchronous version for view rendering
                var result = permissionService.HasPermissionAsync(userId, permissionKey, departmentId).GetAwaiter().GetResult();
                
                System.Diagnostics.Debug.WriteLine($"PermissionHelper: Permission '{permissionKey}' for user {userId} = {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PermissionHelper: Error checking permission '{permissionKey}': {ex.Message}");
                return false;
            }
        }
        
        public static bool HasAnyPermission(this IHtmlHelper html, params string[] permissionKeys)
        {
            foreach (var permissionKey in permissionKeys)
            {
                if (html.HasPermission(permissionKey))
                    return true;
            }
            return false;
        }
        
        public static bool HasAllPermissions(this IHtmlHelper html, params string[] permissionKeys)
        {
            foreach (var permissionKey in permissionKeys)
            {
                if (!html.HasPermission(permissionKey))
                    return false;
            }
            return true;
        }
        
        public static bool IsInRole(this IHtmlHelper html, string role)
        {
            return html.ViewContext.HttpContext.User.IsInRole(role);
        }
        
        public static bool IsAdmin(this IHtmlHelper html)
        {
            return html.IsInRole("Admin");
        }
        
        public static bool IsController(this IHtmlHelper html)
        {
            return html.IsInRole("Controller") || html.IsInRole("Admin");
        }
    }
} 