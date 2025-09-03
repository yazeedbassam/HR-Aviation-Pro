using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.Services;
using System.Security.Claims;

namespace WebApplication1.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionKey;
        private readonly bool _requireDepartmentAccess;

        public RequirePermissionAttribute(string permissionKey, bool requireDepartmentAccess = false)
        {
            _permissionKey = permissionKey;
            _requireDepartmentAccess = requireDepartmentAccess;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            
            // Check if user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user ID from claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get permission service
            var permissionService = httpContext.RequestServices.GetRequiredService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Get department ID from route data if required
            int? departmentId = null;
            if (_requireDepartmentAccess)
            {
                if (httpContext.Request.RouteValues.TryGetValue("departmentId", out var deptIdValue) && 
                    int.TryParse(deptIdValue?.ToString(), out int deptId))
                {
                    departmentId = deptId;
                }
                else if (httpContext.Request.Query.TryGetValue("departmentId", out var queryDeptId) && 
                         int.TryParse(queryDeptId, out int queryDept))
                {
                    departmentId = queryDept;
                }
            }

            // Check permission
            var hasPermission = await permissionService.HasPermissionAsync(userId, _permissionKey, departmentId);
            
            if (!hasPermission)
            {
                // Log the access attempt
                await permissionService.LogPermissionAccessAsync(
                    userId, 
                    "ACCESS_DENIED", 
                    _permissionKey, 
                    departmentId, 
                    $"Access denied to {httpContext.Request.Path}",
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers["User-Agent"].ToString()
                );

                context.Result = new ForbidResult();
                return;
            }

            // Log successful access
            await permissionService.LogPermissionAccessAsync(
                userId, 
                "ACCESS_GRANTED", 
                _permissionKey, 
                departmentId, 
                $"Access granted to {httpContext.Request.Path}",
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers["User-Agent"].ToString()
            );
        }
    }

    // =====================================================
    // CONVENIENCE ATTRIBUTES FOR COMMON PERMISSIONS
    // =====================================================

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireDashboardViewAttribute : RequirePermissionAttribute
    {
        public RequireDashboardViewAttribute() : base("DASHBOARD_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireControllersViewAttribute : RequirePermissionAttribute
    {
        public RequireControllersViewAttribute() : base("CONTROLLERS_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireControllersEditAttribute : RequirePermissionAttribute
    {
        public RequireControllersEditAttribute() : base("CONTROLLERS_EDIT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireStaffViewAttribute : RequirePermissionAttribute
    {
        public RequireStaffViewAttribute() : base("STAFF_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireStaffEditAttribute : RequirePermissionAttribute
    {
        public RequireStaffEditAttribute() : base("STAFF_EDIT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireLicensesViewAttribute : RequirePermissionAttribute
    {
        public RequireLicensesViewAttribute() : base("LICENSES_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireLicensesEditAttribute : RequirePermissionAttribute
    {
        public RequireLicensesEditAttribute() : base("LICENSES_EDIT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireCertificatesViewAttribute : RequirePermissionAttribute
    {
        public RequireCertificatesViewAttribute() : base("CERTIFICATES_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireCertificatesEditAttribute : RequirePermissionAttribute
    {
        public RequireCertificatesEditAttribute() : base("CERTIFICATES_EDIT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireObservationsViewAttribute : RequirePermissionAttribute
    {
        public RequireObservationsViewAttribute() : base("OBSERVATIONS_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireObservationsEditAttribute : RequirePermissionAttribute
    {
        public RequireObservationsEditAttribute() : base("OBSERVATIONS_EDIT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireSystemSettingsAttribute : RequirePermissionAttribute
    {
        public RequireSystemSettingsAttribute() : base("SYSTEM_SETTINGS_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireConfigurationManagementAttribute : RequirePermissionAttribute
    {
        public RequireConfigurationManagementAttribute() : base("CONFIGURATION_MANAGEMENT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRolesManagementAttribute : RequirePermissionAttribute
    {
        public RequireRolesManagementAttribute() : base("ROLES_MANAGEMENT") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireReportsViewAttribute : RequirePermissionAttribute
    {
        public RequireReportsViewAttribute() : base("REPORTS_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireAuditLogsViewAttribute : RequirePermissionAttribute
    {
        public RequireAuditLogsViewAttribute() : base("AUDIT_LOGS_VIEW") { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionLogsViewAttribute : RequirePermissionAttribute
    {
        public RequirePermissionLogsViewAttribute() : base("PERMISSION_LOGS_VIEW") { }
    }
} 