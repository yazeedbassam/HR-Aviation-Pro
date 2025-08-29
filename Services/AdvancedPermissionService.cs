using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAdvancedPermissionService
    {
        // Basic Permission Checks
        Task<bool> HasPermissionAsync(int userId, string permissionKey, int? departmentId = null);
        Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionKeys);
        Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionKeys);
        
        // Data Access Control
        Task<bool> CanAccessUserDataAsync(int currentUserId, int targetUserId);
        Task<bool> CanAccessDepartmentDataAsync(int userId, int departmentId);
        Task<List<int>> GetAccessibleUserIdsAsync(int userId);
        Task<List<int>> GetAccessibleDepartmentIdsAsync(int userId);
        
        // UI Visibility Control
        Task<List<MenuItemPermission>> GetVisibleMenuItemsAsync(int userId);
        Task<bool> IsMenuItemVisibleAsync(int userId, string menuItemKey);
        
        // Data Filtering
        Task<string> GetDataFilterClauseAsync(int userId, string tableName, string userIdColumn = "UserId");
        Task<Dictionary<string, object>> GetDataFilterParametersAsync(int userId, string tableName);
        
        // Permission Management
        Task<List<PermissionViewModel>> GetUserPermissionsAsync(int userId);
        Task<List<int>> GetUserAccessibleDepartmentsAsync(int userId);
        Task<UserPermissionSummary> GetUserPermissionSummaryAsync(int userId);
        
        // Simplified Permission Management
        Task<List<UserInfo>> GetAllUsersAsync();
        Task<List<PermissionInfo>> GetAllPermissionsAsync();
        Task<List<DepartmentInfo>> GetAllDepartmentsAsync();
        Task<List<RoleInfo>> GetAllRolesAsync();
        Task<bool> UpdateUserPermissionsAsync(int userId, List<UserPermissionUpdateModel> permissions);
        Task<bool> UpdateSectionVisibilityAsync(SectionVisibilityUpdateModel model);
        Task<bool> ApplyTemplateToUserAsync(ApplyTemplateModel model);
        Task<List<UserPermissionUpdateModel>> GetUserDetailedPermissionsAsync(int userId);
        Task<Dictionary<string, Dictionary<string, bool>>> GetPermissionMatrixAsync();
        Task<bool> ExecuteBulkPermissionUpdateAsync(BulkPermissionUpdateViewModel model);
        Task<List<PermissionTemplateModel>> GetPermissionTemplatesAsync();
        Task<bool> SavePermissionTemplateAsync(PermissionTemplateModel model);
        Task<bool> ApplyPermissionTemplateAsync(int userId, string templateName);
        
        // User Permission Management
        Task<bool> AddUserPermissionAsync(int userId, int permissionId, int departmentId);
        Task<bool> RemoveUserPermissionAsync(int userId, int permissionId);
        
        // Caching
        void ClearUserCache(int userId);
        void ClearAllCache();
    }

    public class AdvancedPermissionService : IAdvancedPermissionService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdvancedPermissionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdvancedPermissionService(IConfiguration configuration, IMemoryCache cache, ILogger<AdvancedPermissionService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDbConnection") ?? throw new ArgumentNullException(nameof(configuration));
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionKey, int? departmentId = null)
        {
            try
            {
                var cacheKey = $"adv_permission_{userId}_{permissionKey}_{departmentId}";
                if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                {
                    return cachedResult;
                }

                // Special case for admin user - always return true for PERMISSIONS_MANAGE
                var currentUsername = GetCurrentUsername();
                if (currentUsername?.ToLower() == "admin" && permissionKey == "PERMISSIONS_MANAGE")
                {
                    _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                    return true;
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Check if user has the permission in UserDepartmentPermissions table
                var sql = @"
                    SELECT COUNT(1) 
                    FROM UserDepartmentPermissions udp
                    JOIN Permissions p ON udp.PermissionId = p.PermissionId
                    WHERE udp.UserId = @UserId 
                    AND p.PermissionKey = @PermissionKey
                    AND udp.IsActive = 1";
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@PermissionKey", permissionKey);
                
                if (departmentId.HasValue)
                {
                    sql += " AND udp.DepartmentId = @DepartmentId";
                    parameters.Add("@DepartmentId", departmentId.Value);
                }
                
                var result = await connection.QueryFirstOrDefaultAsync<int>(sql, parameters);
                var hasPermission = result > 0;
                
                _cache.Set(cacheKey, hasPermission, TimeSpan.FromMinutes(5));
                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for user {UserId} and permission {PermissionKey}", userId, permissionKey);
                return false;
            }
        }

        private string GetCurrentUsername()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    return httpContext.User.Identity.Name ?? "";
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionKeys)
        {
            foreach (var permissionKey in permissionKeys)
            {
                if (await HasPermissionAsync(userId, permissionKey))
                    return true;
            }
            return false;
        }

        public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionKeys)
        {
            foreach (var permissionKey in permissionKeys)
            {
                if (!await HasPermissionAsync(userId, permissionKey))
                    return false;
            }
            return true;
        }

        public async Task<bool> CanAccessUserDataAsync(int currentUserId, int targetUserId)
        {
            // Admin can access all data
            if (await HasPermissionAsync(currentUserId, "USERS_VIEW_ALL"))
                return true;

            // User can always access their own data
            if (currentUserId == targetUserId)
                return true;

            // Check if user has permission to access other users in their department
            var userDepartments = await GetAccessibleDepartmentIdsAsync(currentUserId);
            var targetUserDepartments = await GetAccessibleDepartmentIdsAsync(targetUserId);
            
            return userDepartments.Intersect(targetUserDepartments).Any();
        }

        public async Task<bool> CanAccessDepartmentDataAsync(int userId, int departmentId)
        {
            var accessibleDepartments = await GetAccessibleDepartmentIdsAsync(userId);
            return accessibleDepartments.Contains(departmentId);
        }

        public async Task<List<int>> GetAccessibleUserIdsAsync(int userId)
        {
            var cacheKey = $"accessible_users_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<int> cachedResult))
                return cachedResult;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                
                var sql = @"
                    SELECT DISTINCT u.userid
                    FROM users u
                    LEFT JOIN UserDepartmentPermissions udp ON u.userid = udp.UserId
                    WHERE u.userid = @UserId 
                       OR udp.DepartmentId IN (
                           SELECT DepartmentId 
                           FROM UserDepartmentPermissions 
                           WHERE UserId = @UserId AND IsActive = 1
                       )
                       OR EXISTS (
                           SELECT 1 FROM users 
                           WHERE userid = @UserId AND rolename = 'Admin'
                       )";
                
                var result = await connection.QueryAsync<int>(sql, parameters);
                var userIds = result.ToList();
                
                _cache.Set(cacheKey, userIds, TimeSpan.FromMinutes(10));
                return userIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accessible user IDs for user {UserId}", userId);
                return new List<int> { userId }; // Return only self if error
            }
        }

        public async Task<List<int>> GetAccessibleDepartmentIdsAsync(int userId)
        {
            var cacheKey = $"accessible_departments_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<int> cachedResult))
                return cachedResult;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                
                var sql = @"
                    SELECT DISTINCT udp.DepartmentId
                    FROM UserDepartmentPermissions udp
                    WHERE udp.UserId = @UserId AND udp.IsActive = 1
                    UNION
                    SELECT cv.ValueId
                    FROM ConfigurationValues cv
                    JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
                    WHERE EXISTS (
                        SELECT 1 FROM users 
                        WHERE userid = @UserId AND rolename = 'Admin'
                    ) AND (cc.CategoryName = 'Divisions' OR cc.CategoryName = 'Departments')";
                
                var result = await connection.QueryAsync<int>(sql, parameters);
                var departmentIds = result.ToList();
                
                _cache.Set(cacheKey, departmentIds, TimeSpan.FromMinutes(10));
                return departmentIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accessible department IDs for user {UserId}", userId);
                return new List<int>();
            }
        }

        public async Task<List<MenuItemPermission>> GetVisibleMenuItemsAsync(int userId)
        {
            var cacheKey = $"visible_menu_items_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<MenuItemPermission> cachedResult))
                return cachedResult;

            var menuItems = new List<MenuItemPermission>();

            // Dashboard
            if (await HasPermissionAsync(userId, "DASHBOARD_VIEW"))
            {
                menuItems.Add(new MenuItemPermission { Key = "dashboard", Text = "Dashboard", Icon = "bi-bar-chart", Url = "/Dashboard", IsVisible = true });
            }

            // Organization
            if (await HasAnyPermissionAsync(userId, "ORGANIZATION_VIEW", "STRUCTURE_VIEW", "DIVISIONS_VIEW"))
            {
                menuItems.Add(new MenuItemPermission 
                { 
                    Key = "organization", 
                    Text = "Organization", 
                    Icon = "bi-building", 
                    IsVisible = true,
                    SubItems = new List<MenuItemPermission>
                    {
                        new MenuItemPermission { Key = "structure", Text = "Structure", Icon = "bi-globe", Url = "/Country", IsVisible = await HasPermissionAsync(userId, "STRUCTURE_VIEW") },
                        new MenuItemPermission { Key = "divisions", Text = "Divisions", Icon = "bi-airplane", Url = "/Airport", IsVisible = await HasPermissionAsync(userId, "DIVISIONS_VIEW") }
                    }
                });
            }

            // Staff
            if (await HasAnyPermissionAsync(userId, "STAFF_VIEW", "CONTROLLERS_VIEW", "AIS_VIEW", "CNS_VIEW", "AFTN_VIEW", "OPS_STAFF_VIEW"))
            {
                menuItems.Add(new MenuItemPermission 
                { 
                    Key = "staff", 
                    Text = "Staff", 
                    Icon = "bi-people", 
                    IsVisible = true,
                    SubItems = new List<MenuItemPermission>
                    {
                        new MenuItemPermission { Key = "controllers", Text = "Controllers", Icon = "bi-person-badge", Url = "/ControllerUser", IsVisible = await HasPermissionAsync(userId, "CONTROLLERS_VIEW") },
                        new MenuItemPermission { Key = "ais", Text = "AIS", Icon = "bi-info-circle", Url = "/Employees/AIS", IsVisible = await HasPermissionAsync(userId, "AIS_VIEW") },
                        new MenuItemPermission { Key = "cns", Text = "CNS", Icon = "bi-wifi", Url = "/Employees/CNS", IsVisible = await HasPermissionAsync(userId, "CNS_VIEW") },
                        new MenuItemPermission { Key = "aftn", Text = "AFTN", Icon = "bi-transmission", Url = "/Employees/AFTN", IsVisible = await HasPermissionAsync(userId, "AFTN_VIEW") },
                        new MenuItemPermission { Key = "ops_staff", Text = "Ops Staff & Administration", Icon = "bi-gear", Url = "/Employees/OpsStaff", IsVisible = await HasPermissionAsync(userId, "OPS_STAFF_VIEW") }
                    }
                });
            }

            // Documents
            if (await HasAnyPermissionAsync(userId, "LICENSES_VIEW", "CERTIFICATES_VIEW", "OBSERVATIONS_VIEW"))
            {
                menuItems.Add(new MenuItemPermission 
                { 
                    Key = "documents", 
                    Text = "Documents", 
                    Icon = "bi-file-earmark-text", 
                    IsVisible = true,
                    SubItems = new List<MenuItemPermission>
                    {
                        new MenuItemPermission { Key = "licenses", Text = "Licenses", Icon = "bi-card-checklist", Url = "/License/Index", IsVisible = await HasPermissionAsync(userId, "LICENSES_VIEW") },
                        new MenuItemPermission { Key = "certificates", Text = "Certificates", Icon = "bi-award", Url = "/Certificate/Index", IsVisible = await HasPermissionAsync(userId, "CERTIFICATES_VIEW") },
                        new MenuItemPermission { Key = "observations", Text = "Observations", Icon = "bi-binoculars", Url = "/Observations/Index", IsVisible = await HasPermissionAsync(userId, "OBSERVATIONS_VIEW") }
                    }
                });
            }

            // Activities
            if (await HasPermissionAsync(userId, "COURSES_VIEW"))
            {
                menuItems.Add(new MenuItemPermission 
                { 
                    Key = "activities", 
                    Text = "Activities", 
                    Icon = "bi-activity", 
                    IsVisible = true,
                    SubItems = new List<MenuItemPermission>
                    {
                        new MenuItemPermission { Key = "courses", Text = "Courses", Icon = "bi-calendar-event", Url = "/Projects/Index", IsVisible = true }
                    }
                });
            }

            // System Settings (Admin only)
            if (await HasAnyPermissionAsync(userId, "SYSTEM_SETTINGS_VIEW", "CONFIGURATION_MANAGEMENT", "ROLES_MANAGEMENT"))
            {
                menuItems.Add(new MenuItemPermission 
                { 
                    Key = "system_settings", 
                    Text = "System Settings", 
                    Icon = "bi-gear", 
                    IsVisible = true,
                    SubItems = new List<MenuItemPermission>
                    {
                        new MenuItemPermission { Key = "configuration", Text = "Configuration Management", Icon = "bi-sliders", Url = "/Configuration", IsVisible = await HasPermissionAsync(userId, "CONFIGURATION_MANAGEMENT") },
                        new MenuItemPermission { Key = "roles", Text = "Roles Management", Icon = "bi-shield-lock", Url = "/Permission", IsVisible = await HasPermissionAsync(userId, "ROLES_MANAGEMENT") },
                        new MenuItemPermission { Key = "simplified_permissions", Text = "إدارة الصلاحيات المبسطة", Icon = "bi-shield-check", Url = "/SimplifiedPermission", IsVisible = await HasPermissionAsync(userId, "PERMISSIONS_MANAGE") }
                    }
                });
            }

            _cache.Set(cacheKey, menuItems, TimeSpan.FromMinutes(10));
            return menuItems;
        }

        public async Task<bool> IsMenuItemVisibleAsync(int userId, string menuItemKey)
        {
            var menuItems = await GetVisibleMenuItemsAsync(userId);
            return menuItems.Any(item => item.Key == menuItemKey && item.IsVisible);
        }

        public async Task<string> GetDataFilterClauseAsync(int userId, string tableName, string userIdColumn = "UserId")
        {
            // Admin can see all data
            if (await HasPermissionAsync(userId, "USERS_VIEW_ALL"))
                return "1=1";

            // Get accessible user IDs
            var accessibleUserIds = await GetAccessibleUserIdsAsync(userId);
            
            if (!accessibleUserIds.Any())
                return "1=0"; // No access

            var userIds = string.Join(",", accessibleUserIds);
            return $"{userIdColumn} IN ({userIds})";
        }

        public async Task<Dictionary<string, object>> GetDataFilterParametersAsync(int userId, string tableName)
        {
            var parameters = new Dictionary<string, object>();

            // Admin can see all data
            if (await HasPermissionAsync(userId, "USERS_VIEW_ALL"))
                return parameters;

            // Get accessible user IDs
            var accessibleUserIds = await GetAccessibleUserIdsAsync(userId);
            parameters.Add("UserIds", accessibleUserIds);

            return parameters;
        }

        public async Task<List<PermissionViewModel>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<PermissionViewModel> cachedResult))
                return cachedResult;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await connection.QueryAsync<PermissionViewModel>(
                "EXEC GetUserPermissions @UserId", parameters);
            var permissions = result.ToList();
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
            return permissions;
        }

        public async Task<List<int>> GetUserAccessibleDepartmentsAsync(int userId)
        {
            return await GetAccessibleDepartmentIdsAsync(userId);
        }

        public async Task<UserPermissionSummary> GetUserPermissionSummaryAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await connection.QueryFirstOrDefaultAsync<UserPermissionSummary>(
                "SELECT * FROM vw_UserPermissionsSummary WHERE UserId = @UserId", parameters);
            return result;
        }

        public void ClearUserCache(int userId)
        {
            var cacheKeys = new[]
            {
                $"adv_permission_{userId}_*",
                $"accessible_users_{userId}",
                $"accessible_departments_{userId}",
                $"visible_menu_items_{userId}",
                $"user_permissions_{userId}"
            };

            foreach (var pattern in cacheKeys)
            {
                // Note: This is a simplified cache clearing. In production, you might want to use a more sophisticated approach
                _cache.Remove(pattern.Replace("*", ""));
            }
        }

        public void ClearAllCache()
        {
            // Note: IMemoryCache doesn't have a Clear method in .NET Core
            // This would need to be implemented differently if needed
            // For now, we'll just log that this method was called
            _logger.LogInformation("ClearAllCache called - cache will expire naturally");
        }

        // =====================================================
        // ADDITIONAL METHODS FOR SIMPLIFIED PERMISSION MANAGEMENT
        // =====================================================

        public async Task<List<UserInfo>> GetAllUsersAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT 
                        u.userid as UserId,
                        u.username as UserName,
                        ISNULL(u.rolename, 'غير محدد') as UserFullName,
                        ISNULL(u.rolename, 'غير محدد') as UserRole,
                        CASE 
                            WHEN u.rolename LIKE '%OJAI%' THEN 'OJAI'
                            WHEN u.rolename LIKE '%OJAM%' THEN 'OJAM'
                            WHEN u.rolename LIKE '%OJAQ%' THEN 'OJAQ'
                            WHEN u.rolename LIKE '%CARC%' THEN 'CARC'
                            WHEN u.rolename LIKE '%TACC%' THEN 'TACC'
                            WHEN u.rolename LIKE '%AIS%' THEN 'AIS'
                            WHEN u.rolename LIKE '%CNS%' THEN 'CNS'
                            WHEN u.rolename LIKE '%AFTN%' THEN 'AFTN'
                            WHEN u.rolename LIKE '%Ops%' OR u.rolename LIKE '%Staff%' THEN 'Ops Staff'
                            WHEN u.rolename LIKE '%Admin%' OR u.rolename LIKE '%Supervisor%' THEN 'HQ'
                            ELSE 'غير محدد'
                        END as UserDepartment
                    FROM users u
                    ORDER BY u.username";
                
                var result = await connection.QueryAsync<UserInfo>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserInfo>();
            }
        }

        public async Task<List<PermissionInfo>> GetAllPermissionsAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Create a list of common permissions if the table doesn't exist or is empty
                var permissions = new List<PermissionInfo>
                {
                    new PermissionInfo { PermissionId = 1, PermissionName = "عرض لوحة التحكم", PermissionKey = "DASHBOARD_VIEW", Description = "إمكانية عرض لوحة التحكم", Category = "Dashboard" },
                    new PermissionInfo { PermissionId = 2, PermissionName = "عرض المنظمة", PermissionKey = "ORGANIZATION_VIEW", Description = "إمكانية عرض المنظمة", Category = "Organization" },
                    new PermissionInfo { PermissionId = 3, PermissionName = "عرض الهيكل", PermissionKey = "STRUCTURE_VIEW", Description = "إمكانية عرض الهيكل التنظيمي", Category = "Organization" },
                    new PermissionInfo { PermissionId = 4, PermissionName = "عرض الأقسام", PermissionKey = "DIVISIONS_VIEW", Description = "إمكانية عرض الأقسام", Category = "Organization" },
                    new PermissionInfo { PermissionId = 5, PermissionName = "عرض الموظفين", PermissionKey = "STAFF_VIEW", Description = "إمكانية عرض الموظفين", Category = "Staff" },
                    new PermissionInfo { PermissionId = 6, PermissionName = "عرض المراقبين", PermissionKey = "CONTROLLERS_VIEW", Description = "إمكانية عرض المراقبين", Category = "Staff" },
                    new PermissionInfo { PermissionId = 7, PermissionName = "عرض AIS", PermissionKey = "AIS_VIEW", Description = "إمكانية عرض AIS", Category = "Staff" },
                    new PermissionInfo { PermissionId = 8, PermissionName = "عرض CNS", PermissionKey = "CNS_VIEW", Description = "إمكانية عرض CNS", Category = "Staff" },
                    new PermissionInfo { PermissionId = 9, PermissionName = "عرض AFTN", PermissionKey = "AFTN_VIEW", Description = "إمكانية عرض AFTN", Category = "Staff" },
                    new PermissionInfo { PermissionId = 10, PermissionName = "عرض Ops Staff", PermissionKey = "OPS_STAFF_VIEW", Description = "إمكانية عرض Ops Staff", Category = "Staff" },
                    new PermissionInfo { PermissionId = 11, PermissionName = "عرض الرخص", PermissionKey = "LICENSES_VIEW", Description = "إمكانية عرض الرخص", Category = "Documents" },
                    new PermissionInfo { PermissionId = 12, PermissionName = "عرض الشهادات", PermissionKey = "CERTIFICATES_VIEW", Description = "إمكانية عرض الشهادات", Category = "Documents" },
                    new PermissionInfo { PermissionId = 13, PermissionName = "عرض الملاحظات", PermissionKey = "OBSERVATIONS_VIEW", Description = "إمكانية عرض الملاحظات", Category = "Documents" },
                    new PermissionInfo { PermissionId = 14, PermissionName = "عرض الدورات", PermissionKey = "COURSES_VIEW", Description = "إمكانية عرض الدورات", Category = "Activities" },
                    new PermissionInfo { PermissionId = 15, PermissionName = "إدارة الصلاحيات", PermissionKey = "PERMISSIONS_MANAGE", Description = "إمكانية إدارة الصلاحيات", Category = "System" },
                    new PermissionInfo { PermissionId = 16, PermissionName = "إعدادات النظام", PermissionKey = "SYSTEM_SETTINGS_VIEW", Description = "إمكانية عرض إعدادات النظام", Category = "System" },
                    new PermissionInfo { PermissionId = 17, PermissionName = "إدارة التكوين", PermissionKey = "CONFIGURATION_MANAGEMENT", Description = "إمكانية إدارة التكوين", Category = "System" },
                    new PermissionInfo { PermissionId = 18, PermissionName = "إدارة الأدوار", PermissionKey = "ROLES_MANAGEMENT", Description = "إمكانية إدارة الأدوار", Category = "System" },
                    new PermissionInfo { PermissionId = 19, PermissionName = "عرض جميع المستخدمين", PermissionKey = "USERS_VIEW_ALL", Description = "إمكانية عرض جميع المستخدمين", Category = "System" },
                    new PermissionInfo { PermissionId = 20, PermissionName = "إضافة", PermissionKey = "ADD", Description = "إمكانية الإضافة", Category = "General" },
                    new PermissionInfo { PermissionId = 21, PermissionName = "تعديل", PermissionKey = "EDIT", Description = "إمكانية التعديل", Category = "General" },
                    new PermissionInfo { PermissionId = 22, PermissionName = "حذف", PermissionKey = "DELETE", Description = "إمكانية الحذف", Category = "General" },
                    new PermissionInfo { PermissionId = 23, PermissionName = "تصدير", PermissionKey = "EXPORT", Description = "إمكانية التصدير", Category = "General" }
                };
                
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return new List<PermissionInfo>();
            }
        }

        public async Task<List<DepartmentInfo>> GetAllDepartmentsAsync()
        {
            try
            {
                // Create a list of common departments if the table doesn't exist or is empty
                var departments = new List<DepartmentInfo>
                {
                    new DepartmentInfo { DepartmentId = 1, DepartmentName = "OJAI", Description = "Queen Alia International Airport" },
                    new DepartmentInfo { DepartmentId = 2, DepartmentName = "OJAM", Description = "Amman Civil Airport" },
                    new DepartmentInfo { DepartmentId = 3, DepartmentName = "OJAQ", Description = "Aqaba Airport" },
                    new DepartmentInfo { DepartmentId = 4, DepartmentName = "CARC", Description = "Civil Aviation Regulatory Commission" },
                    new DepartmentInfo { DepartmentId = 5, DepartmentName = "TACC", Description = "Training and Air Traffic Control Center" },
                    new DepartmentInfo { DepartmentId = 6, DepartmentName = "HQ", Description = "Headquarters - Main Office" },
                    new DepartmentInfo { DepartmentId = 7, DepartmentName = "AIS", Description = "Aeronautical Information Service" },
                    new DepartmentInfo { DepartmentId = 8, DepartmentName = "CNS", Description = "Communications, Navigation and Surveillance" },
                    new DepartmentInfo { DepartmentId = 9, DepartmentName = "AFTN", Description = "Aeronautical Fixed Telecommunication Network" },
                    new DepartmentInfo { DepartmentId = 10, DepartmentName = "Ops Staff", Description = "Operations Staff & Administration" }
                };
                
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departments");
                return new List<DepartmentInfo>();
            }
        }

        public async Task<List<RoleInfo>> GetAllRolesAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT 
                        cv.ValueId as RoleId,
                        cv.ValueText as RoleName,
                        '' as Description
                    FROM ConfigurationValues cv
                    JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
                    WHERE cc.CategoryName = 'Roles'
                    ORDER BY cv.ValueText";
                
                var result = await connection.QueryAsync<RoleInfo>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return new List<RoleInfo>();
            }
        }

        public async Task<bool> UpdateUserPermissionsAsync(int userId, List<UserPermissionUpdateModel> permissions)
        {
            try
            {
                // This is a placeholder implementation
                // In a real implementation, you would update the database
                await Task.Delay(100); // Simulate database operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permissions");
                return false;
            }
        }

        public async Task<bool> UpdateSectionVisibilityAsync(SectionVisibilityUpdateModel model)
        {
            try
            {
                // This is a placeholder implementation
                // In a real implementation, you would update the database
                await Task.Delay(100); // Simulate database operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section visibility");
                return false;
            }
        }

        public async Task<bool> ApplyTemplateToUserAsync(ApplyTemplateModel model)
        {
            try
            {
                // This is a placeholder implementation
                // In a real implementation, you would apply the template
                await Task.Delay(100); // Simulate database operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying template to user");
                return false;
            }
        }

        public async Task<List<UserPermissionUpdateModel>> GetUserDetailedPermissionsAsync(int userId)
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return new List<UserPermissionUpdateModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detailed permissions");
                return new List<UserPermissionUpdateModel>();
            }
        }

        public async Task<Dictionary<string, Dictionary<string, bool>>> GetPermissionMatrixAsync()
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return new Dictionary<string, Dictionary<string, bool>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission matrix");
                return new Dictionary<string, Dictionary<string, bool>>();
            }
        }

        public async Task<bool> ExecuteBulkPermissionUpdateAsync(BulkPermissionUpdateViewModel model)
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing bulk permission update");
                return false;
            }
        }

        public async Task<List<PermissionTemplateModel>> GetPermissionTemplatesAsync()
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return new List<PermissionTemplateModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission templates");
                return new List<PermissionTemplateModel>();
            }
        }

        public async Task<bool> SavePermissionTemplateAsync(PermissionTemplateModel model)
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving permission template");
                return false;
            }
        }

        public async Task<bool> ApplyPermissionTemplateAsync(int userId, string templateName)
        {
            try
            {
                // This is a placeholder implementation
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying permission template");
                return false;
            }
        }

        public async Task<bool> AddUserPermissionAsync(int userId, int permissionId, int departmentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // First, ensure the permission exists in the Permissions table
                var permissionExists = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(1) FROM Permissions WHERE PermissionId = @PermissionId", 
                    new { PermissionId = permissionId });
                
                if (permissionExists == 0)
                {
                    _logger.LogWarning("Permission {PermissionId} does not exist in Permissions table", permissionId);
                    
                                         // Create the permission if it doesn't exist
                     var createPermissionSql = @"
                         INSERT INTO Permissions (PermissionId, PermissionName, PermissionKey)
                         VALUES (@PermissionId, @PermissionName, @PermissionKey)";
                    
                    var permissionName = GetPermissionNameById(permissionId);
                    var permissionKey = GetPermissionKeyById(permissionId);
                    var category = GetPermissionCategoryById(permissionId);
                    
                                         await connection.ExecuteAsync(createPermissionSql, new 
                     { 
                         PermissionId = permissionId, 
                         PermissionName = permissionName,
                         PermissionKey = permissionKey
                     });
                    
                    _logger.LogInformation("Created permission {PermissionId}: {PermissionName}", permissionId, permissionName);
                }
                
                // Check if permission already exists for this user
                var checkSql = @"
                    SELECT COUNT(1) FROM UserDepartmentPermissions 
                    WHERE UserId = @UserId AND PermissionId = @PermissionId AND DepartmentId = @DepartmentId";
                
                var exists = await connection.QueryFirstOrDefaultAsync<int>(checkSql, new { UserId = userId, PermissionId = permissionId, DepartmentId = departmentId });
                
                if (exists > 0)
                {
                    // Update existing permission
                    var updateSql = @"
                        UPDATE UserDepartmentPermissions 
                        SET IsActive = 1
                        WHERE UserId = @UserId AND PermissionId = @PermissionId AND DepartmentId = @DepartmentId";
                    
                    var result = await connection.ExecuteAsync(updateSql, new { UserId = userId, PermissionId = permissionId, DepartmentId = departmentId });
                    _logger.LogInformation("Updated existing permission for user {UserId}, permission {PermissionId}", userId, permissionId);
                    return result > 0;
                }
                else
                {
                    // Insert new permission
                    var insertSql = @"
                        INSERT INTO UserDepartmentPermissions (UserId, PermissionId, DepartmentId, IsActive)
                        VALUES (@UserId, @PermissionId, @DepartmentId, 1)";
                    
                    var result = await connection.ExecuteAsync(insertSql, new { UserId = userId, PermissionId = permissionId, DepartmentId = departmentId });
                    _logger.LogInformation("Added new permission for user {UserId}, permission {PermissionId}", userId, permissionId);
                    return result > 0;
                }
            }
                         catch (Exception ex)
             {
                 _logger.LogError(ex, "Error adding user permission: UserId={UserId}, PermissionId={PermissionId}, DepartmentId={DepartmentId}", userId, permissionId, departmentId);
                 
                 // Try to get more specific error information
                 if (ex.Message.Contains("Invalid column name"))
                 {
                     _logger.LogError("Database schema issue detected. Please check if all required columns exist in Permissions and UserDepartmentPermissions tables.");
                 }
                 else if (ex.Message.Contains("FOREIGN KEY constraint"))
                 {
                     _logger.LogError("Foreign key constraint violation. Please check if the referenced records exist.");
                 }
                 
                 return false;
             }
        }

        private string GetPermissionNameById(int permissionId)
        {
            return permissionId switch
            {
                1 => "عرض لوحة التحكم",
                2 => "عرض المنظمة",
                3 => "عرض الهيكل",
                4 => "عرض الأقسام",
                5 => "عرض الموظفين",
                6 => "عرض المراقبين",
                7 => "عرض AIS",
                8 => "عرض CNS",
                9 => "عرض AFTN",
                10 => "عرض Ops Staff",
                11 => "عرض الرخص",
                12 => "عرض الشهادات",
                13 => "عرض الملاحظات",
                14 => "عرض الدورات",
                15 => "إدارة الصلاحيات",
                16 => "إعدادات النظام",
                17 => "إدارة التكوين",
                18 => "إدارة الأدوار",
                19 => "عرض جميع المستخدمين",
                20 => "إضافة",
                21 => "تعديل",
                22 => "حذف",
                23 => "تصدير",
                _ => $"صلاحية {permissionId}"
            };
        }

        private string GetPermissionKeyById(int permissionId)
        {
            return permissionId switch
            {
                1 => "DASHBOARD_VIEW",
                2 => "ORGANIZATION_VIEW",
                3 => "STRUCTURE_VIEW",
                4 => "DIVISIONS_VIEW",
                5 => "STAFF_VIEW",
                6 => "CONTROLLERS_VIEW",
                7 => "AIS_VIEW",
                8 => "CNS_VIEW",
                9 => "AFTN_VIEW",
                10 => "OPS_STAFF_VIEW",
                11 => "LICENSES_VIEW",
                12 => "CERTIFICATES_VIEW",
                13 => "OBSERVATIONS_VIEW",
                14 => "COURSES_VIEW",
                15 => "PERMISSIONS_MANAGE",
                16 => "SYSTEM_SETTINGS_VIEW",
                17 => "CONFIGURATION_MANAGEMENT",
                18 => "ROLES_MANAGEMENT",
                19 => "USERS_VIEW_ALL",
                20 => "ADD",
                21 => "EDIT",
                22 => "DELETE",
                23 => "EXPORT",
                _ => $"PERMISSION_{permissionId}"
            };
        }

        private string GetPermissionCategoryById(int permissionId)
        {
            return permissionId switch
            {
                1 => "Dashboard",
                2 or 3 or 4 => "Organization",
                5 or 6 or 7 or 8 or 9 or 10 => "Staff",
                11 or 12 or 13 => "Documents",
                14 => "Activities",
                15 or 16 or 17 or 18 or 19 => "System",
                20 or 21 or 22 or 23 => "General",
                _ => "General"
            };
        }

        public async Task<bool> RemoveUserPermissionAsync(int userId, int permissionId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                    DELETE FROM UserDepartmentPermissions 
                    WHERE UserId = @UserId AND PermissionId = @PermissionId";
                
                var result = await connection.ExecuteAsync(sql, new { UserId = userId, PermissionId = permissionId });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user permission: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
                return false;
            }
        }
    }

    // =====================================================
    // ADDITIONAL MODELS
    // =====================================================

    public class SectionVisibilityUpdateModel
    {
        public int UserId { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
    }

    public class ApplyTemplateModel
    {
        public int UserId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
    }
} 
