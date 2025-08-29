using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionKey, int? departmentId = null);
        Task<PermissionCheckResult> CheckPermissionAsync(int userId, string permissionKey, int? departmentId = null);
        Task<List<PermissionViewModel>> GetUserPermissionsAsync(int userId);
        Task<List<int>> GetUserAccessibleDepartmentsAsync(int userId);
        Task<UserPermissionSummary> GetUserPermissionSummaryAsync(int userId);
        Task<List<PermissionViewModel>> GetAllPermissionsAsync();
        Task<List<RolePermissionViewModel>> GetRolePermissionsAsync(int? roleId = null);
        Task<List<UserDepartmentPermissionViewModel>> GetUserDepartmentPermissionsAsync(int? userId = null);
        Task<bool> AddRolePermissionAsync(int roleId, int permissionId);
        Task<bool> RemoveRolePermissionAsync(int rolePermissionId);
        Task<bool> AddUserDepartmentPermissionAsync(UserDepartmentPermissionViewModel model);
        Task<bool> UpdateUserDepartmentPermissionAsync(UserDepartmentPermissionViewModel model);
        Task<bool> RemoveUserDepartmentPermissionAsync(int userDepartmentPermissionId);
        Task LogPermissionAccessAsync(int userId, string status, string permissionKey, int? departmentId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        Task<List<PermissionLogViewModel>> GetPermissionLogsAsync(int? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
        List<SelectListItem> GetRolesSelectList();
        List<SelectListItem> GetDepartmentsSelectList();
        List<SelectListItem> GetPermissionsSelectList();
        List<SelectListItem> GetUsersSelectList();
    }
} 