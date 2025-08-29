using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(IConfiguration configuration, IMemoryCache cache, ILogger<PermissionService> logger, IHttpContextAccessor httpContextAccessor)
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
                _logger.LogInformation("HasPermissionAsync: Checking permission '{PermissionKey}' for user {UserId}, department {DepartmentId}", permissionKey, userId, departmentId);
                
                var cacheKey = $"permission_{userId}_{permissionKey}_{departmentId}";
                if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                {
                    _logger.LogInformation("HasPermissionAsync: Returning cached result for user {UserId}, permission '{PermissionKey}' = {Result}", userId, permissionKey, cachedResult);
                    return cachedResult;
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("HasPermissionAsync: Database connection opened");
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@PermissionKey", permissionKey);
                parameters.Add("@DepartmentId", departmentId);
                
                _logger.LogInformation("HasPermissionAsync: Executing stored procedure CheckUserPermission");
                var result = await connection.QueryFirstOrDefaultAsync<bool>(
                    "EXEC CheckUserPermission @UserId, @PermissionKey, @DepartmentId", parameters);
                
                _logger.LogInformation("HasPermissionAsync: Result for user {UserId}, permission '{PermissionKey}' = {Result}", userId, permissionKey, result);
                
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HasPermissionAsync: Error checking permission for user {UserId} and permission {PermissionKey}", userId, permissionKey);
                return false;
            }
        }

        public async Task<PermissionCheckResult> CheckPermissionAsync(int userId, string permissionKey, int? departmentId = null)
        {
            try
            {
                var hasPermission = await HasPermissionAsync(userId, permissionKey, departmentId);
                return new PermissionCheckResult
                {
                    HasPermission = hasPermission,
                    Message = hasPermission ? "Permission granted" : "Permission denied",
                    UserId = userId,
                    PermissionKey = permissionKey,
                    DepartmentId = departmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for user {UserId}, permission {PermissionKey}", userId, permissionKey);
                return new PermissionCheckResult
                {
                    HasPermission = false,
                    Message = "Error occurred while checking permission",
                    UserId = userId,
                    PermissionKey = permissionKey,
                    DepartmentId = departmentId
                };
            }
        }

        public async Task<List<PermissionViewModel>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<PermissionViewModel> cachedResult)) return cachedResult;

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
            var cacheKey = $"user_departments_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<int> cachedResult)) return cachedResult;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await connection.QueryAsync<int>(
                "EXEC GetUserDepartmentPermissions @UserId", parameters);
            var departments = result.ToList();
            _cache.Set(cacheKey, departments, TimeSpan.FromMinutes(10));
            return departments;
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

        public async Task<List<PermissionViewModel>> GetAllPermissionsAsync()
        {
            var cacheKey = "all_permissions";
            if (_cache.TryGetValue(cacheKey, out List<PermissionViewModel> cachedResult)) return cachedResult;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var result = await connection.QueryAsync<PermissionViewModel>(
                "SELECT * FROM Permissions WHERE IsActive = 1 ORDER BY PermissionName");
            var permissions = result.ToList();
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(15));
            return permissions;
        }

        public async Task<List<RolePermissionViewModel>> GetRolePermissionsAsync(int? roleId = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var sql = @"SELECT rp.*, p.PermissionName, p.PermissionKey, 
                           ISNULL(p.PermissionDescription, '') as PermissionDescription, 
                           ISNULL(p.CategoryName, 'General') as CategoryName, 
                           cv.ValueText as RoleName 
                           FROM RolePermissions rp 
                           JOIN Permissions p ON rp.PermissionId = p.PermissionId 
                           JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId 
                           JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
                           WHERE cc.CategoryName = 'Roles'";
                
                if (roleId.HasValue)
                    sql += " AND rp.RoleId = @RoleId";
                
                sql += " ORDER BY cv.ValueText, p.PermissionName";
                
                var parameters = new DynamicParameters();
                if (roleId.HasValue)
                    parameters.Add("@RoleId", roleId.Value);
                
                var result = await connection.QueryAsync<RolePermissionViewModel>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role permissions");
                return new List<RolePermissionViewModel>();
            }
        }

        public async Task<List<UserDepartmentPermissionViewModel>> GetUserDepartmentPermissionsAsync(int? userId = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var sql = @"SELECT udp.*, p.PermissionName, p.PermissionKey, 
                           ISNULL(p.PermissionDescription, '') as PermissionDescription, 
                           ISNULL(p.CategoryName, 'General') as CategoryName, 
                           dept.ValueText as DepartmentName, u.username as UserName, u.rolename as UserFullName 
                           FROM UserDepartmentPermissions udp 
                           JOIN Permissions p ON udp.PermissionId = p.PermissionId 
                           JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId 
                           JOIN ConfigurationCategories cc ON dept.CategoryId = cc.CategoryId 
                           JOIN users u ON udp.UserId = u.userid 
                           WHERE (cc.CategoryName = 'Divisions' OR cc.CategoryName = 'Departments')";
                
                if (userId.HasValue)
                    sql += " AND udp.UserId = @UserId";
                
                sql += " ORDER BY u.username, dept.ValueText, p.PermissionName";
                
                var parameters = new DynamicParameters();
                if (userId.HasValue)
                    parameters.Add("@UserId", userId.Value);
                
                _logger.LogInformation("Executing GetUserDepartmentPermissionsAsync with SQL: {Sql}", sql);
                var result = await connection.QueryAsync<UserDepartmentPermissionViewModel>(sql, parameters);
                var permissions = result.ToList();
                _logger.LogInformation("Found {Count} user department permissions", permissions.Count);
                
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user department permissions");
                return new List<UserDepartmentPermissionViewModel>();
            }
        }

        public async Task<bool> AddRolePermissionAsync(int roleId, int permissionId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@RoleId", roleId);
                parameters.Add("@PermissionId", permissionId);
                await connection.ExecuteAsync(
                    "INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt) VALUES (@RoleId, @PermissionId, 1, GETDATE())", parameters);
                
                // Clear cache
                _cache.Remove("all_permissions");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role permission: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
                return false;
            }
        }

        public async Task<bool> RemoveRolePermissionAsync(int rolePermissionId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@RolePermissionId", rolePermissionId);
                await connection.ExecuteAsync(
                    "DELETE FROM RolePermissions WHERE RolePermissionId = @RolePermissionId", parameters);
                
                // Clear cache
                _cache.Remove("all_permissions");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role permission: RolePermissionId={RolePermissionId}", rolePermissionId);
                return false;
            }
        }

        public async Task<bool> AddUserDepartmentPermissionAsync(UserDepartmentPermissionViewModel model)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", model.UserId);
                parameters.Add("@DepartmentId", model.DepartmentId);
                parameters.Add("@PermissionId", model.PermissionId);
                parameters.Add("@CanView", model.CanView);
                parameters.Add("@CanEdit", model.CanEdit);
                parameters.Add("@CanDelete", model.CanDelete);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@CreatedAt", DateTime.Now);
                
                await connection.ExecuteAsync(
                    @"INSERT INTO UserDepartmentPermissions 
                    (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt) 
                    VALUES (@UserId, @DepartmentId, @PermissionId, @CanView, @CanEdit, @CanDelete, @IsActive, @CreatedAt)", parameters);
                
                // Clear cache
                _cache.Remove($"user_permissions_{model.UserId}");
                _cache.Remove($"user_departments_{model.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user department permission for user {UserId}", model.UserId);
                return false;
            }
        }

        public async Task<bool> UpdateUserDepartmentPermissionAsync(UserDepartmentPermissionViewModel model)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserDepartmentPermissionId", model.UserDepartmentPermissionId);
                parameters.Add("@UserId", model.UserId);
                parameters.Add("@DepartmentId", model.DepartmentId);
                parameters.Add("@PermissionId", model.PermissionId);
                parameters.Add("@CanView", model.CanView);
                parameters.Add("@CanEdit", model.CanEdit);
                parameters.Add("@CanDelete", model.CanDelete);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@UpdatedAt", DateTime.Now);
                
                await connection.ExecuteAsync(
                    @"UPDATE UserDepartmentPermissions 
                    SET UserId = @UserId, DepartmentId = @DepartmentId, PermissionId = @PermissionId, 
                        CanView = @CanView, CanEdit = @CanEdit, CanDelete = @CanDelete, 
                        IsActive = @IsActive, UpdatedAt = @UpdatedAt 
                    WHERE UserDepartmentPermissionId = @UserDepartmentPermissionId", parameters);
                
                // Clear cache
                _cache.Remove($"user_permissions_{model.UserId}");
                _cache.Remove($"user_departments_{model.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user department permission {Id}", model.UserDepartmentPermissionId);
                return false;
            }
        }

        public async Task<bool> RemoveUserDepartmentPermissionAsync(int userDepartmentPermissionId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserDepartmentPermissionId", userDepartmentPermissionId);
                await connection.ExecuteAsync(
                    "DELETE FROM UserDepartmentPermissions WHERE UserDepartmentPermissionId = @UserDepartmentPermissionId", parameters);
                
                // Clear cache
                _cache.Remove("all_permissions");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user department permission {Id}", userDepartmentPermissionId);
                return false;
            }
        }

        public async Task LogPermissionAccessAsync(int userId, string status, string permissionKey, int? departmentId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Status", status);
                parameters.Add("@PermissionKey", permissionKey);
                parameters.Add("@DepartmentId", departmentId);
                parameters.Add("@Details", details);
                parameters.Add("@IpAddress", ipAddress);
                parameters.Add("@UserAgent", userAgent);
                parameters.Add("@Timestamp", DateTime.Now);
                
                await connection.ExecuteAsync(
                    @"INSERT INTO PermissionLogs 
                    (UserId, Status, PermissionKey, DepartmentId, Details, IpAddress, UserAgent, Timestamp) 
                    VALUES (@UserId, @Status, @PermissionKey, @DepartmentId, @Details, @IpAddress, @UserAgent, @Timestamp)", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging permission access for user {UserId}", userId);
            }
        }

        public async Task<List<PermissionLogViewModel>> GetPermissionLogsAsync(int? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = @"SELECT pl.*, u.username as UserName, u.rolename as UserFullName, 
                              p.PermissionName, dept.ValueText as DepartmentName
                       FROM PermissionLogs pl
                       LEFT JOIN users u ON pl.UserId = u.userid
                       LEFT JOIN Permissions p ON pl.PermissionKey = p.PermissionKey
                       LEFT JOIN ConfigurationValues dept ON pl.DepartmentId = dept.ValueId
                       WHERE 1=1";
            
            var parameters = new DynamicParameters();
            
            if (userId.HasValue)
            {
                sql += " AND pl.UserId = @UserId";
                parameters.Add("@UserId", userId.Value);
            }
            
            if (fromDate.HasValue)
            {
                sql += " AND pl.Timestamp >= @FromDate";
                parameters.Add("@FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND pl.Timestamp <= @ToDate";
                parameters.Add("@ToDate", toDate.Value);
            }
            
            sql += " ORDER BY pl.Timestamp DESC";
            
            var result = await connection.QueryAsync<PermissionLogViewModel>(sql, parameters);
            return result.ToList();
        }

        // SelectList methods for dropdowns
        public List<SelectListItem> GetRolesSelectList()
        {
            var cacheKey = "roles_selectlist";
            if (_cache.TryGetValue(cacheKey, out List<SelectListItem> cachedResult)) return cachedResult;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                var result = connection.Query<SelectListItem>(
                    @"SELECT cv.ValueId as Value, cv.ValueText as Text 
                      FROM ConfigurationValues cv 
                      JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
                      WHERE cc.CategoryName = 'Roles' AND cv.IsActive = 1 
                      ORDER BY cv.ValueText");
                var roles = result.ToList();
                _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(30));
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles select list");
                return new List<SelectListItem>();
            }
        }

        public List<SelectListItem> GetDepartmentsSelectList()
        {
            var cacheKey = "departments_selectlist";
            if (_cache.TryGetValue(cacheKey, out List<SelectListItem> cachedResult)) 
            {
                _logger.LogInformation("Returning cached departments: {Count} items", cachedResult.Count);
                return cachedResult;
            }

            try
            {
                _logger.LogInformation("Fetching departments from database...");
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                _logger.LogInformation("Database connection opened successfully");
                
                // Get all departments from both 'Divisions' and 'Departments' categories
                var sql = @"SELECT cv.ValueId as Value, cv.ValueText as Text 
                           FROM ConfigurationValues cv 
                           JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
                           WHERE (cc.CategoryName = 'Divisions' OR cc.CategoryName = 'Departments') 
                           AND cv.IsActive = 1 
                           ORDER BY cc.CategoryName, cv.ValueText";
                
                _logger.LogInformation("Executing SQL: {Sql}", sql);
                var result = connection.Query<SelectListItem>(sql);
                var departments = result.ToList();
                
                _logger.LogInformation("Found {Count} departments in database", departments.Count);
                foreach (var dept in departments)
                {
                    _logger.LogInformation("Department: Value={Value}, Text={Text}", dept.Value, dept.Text);
                }
                
                _cache.Set(cacheKey, departments, TimeSpan.FromMinutes(30));
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments select list");
                return new List<SelectListItem>();
            }
        }

        public List<SelectListItem> GetPermissionsSelectList()
        {
            var cacheKey = "permissions_selectlist";
            if (_cache.TryGetValue(cacheKey, out List<SelectListItem> cachedResult)) return cachedResult;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var result = connection.Query<SelectListItem>(
                "SELECT PermissionId as Value, PermissionName as Text FROM Permissions WHERE IsActive = 1 ORDER BY PermissionName");
            var permissions = result.ToList();
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(30));
            return permissions;
        }

        public List<SelectListItem> GetUsersSelectList()
        {
            var cacheKey = "users_selectlist";
            if (_cache.TryGetValue(cacheKey, out List<SelectListItem> cachedResult)) return cachedResult;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var result = connection.Query<SelectListItem>(
                "SELECT userid as Value, username + ' (' + rolename + ')' as Text FROM users ORDER BY username");
            var users = result.ToList();
            _cache.Set(cacheKey, users, TimeSpan.FromMinutes(30));
            return users;
        }
    }
} 
