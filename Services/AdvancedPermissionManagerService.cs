using Microsoft.Extensions.Caching.Memory;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAdvancedPermissionManagerService
    {
        // Menu Permissions
        Task<bool> CanViewMenuAsync(int userId, string menuKey);
        Task<List<MenuPermission>> GetUserMenuPermissionsAsync(int userId);
        Task<bool> SetMenuPermissionAsync(int userId, string menuKey, bool isVisible);
        
        // Operation Permissions
        Task<bool> CanPerformOperationAsync(int userId, string entityType, string operationType, string scope = "All", int? scopeId = null);
        Task<List<OperationPermission>> GetUserOperationPermissionsAsync(int userId);
        Task<bool> SetOperationPermissionAsync(int userId, string entityType, string operationType, bool isAllowed, string scope = "All", int? scopeId = null);
        
        // Organizational Permissions
        Task<bool> CanAccessEntityAsync(int userId, string entityType, int entityId, string operation);
        Task<List<OrganizationalPermission>> GetUserOrganizationalPermissionsAsync(int userId);
        Task<bool> SetOrganizationalPermissionAsync(int userId, string entityType, int entityId, string entityName, bool canView, bool canEdit, bool canDelete, bool canCreate);
        
        // Permission Management
        Task<List<UserPermissionSummaryModel>> GetAllUsersWithPermissionsAsync();
        Task<UserPermissionDetails> GetUserPermissionDetailsAsync(int userId);
        Task<bool> CopyPermissionsFromUserAsync(int fromUserId, int toUserId);
        Task<bool> ResetUserPermissionsAsync(int userId);
        
        // Permission Checking (Legacy Support)
        Task<bool> HasPermissionAsync(int userId, string permissionKey);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        
        // Cache Management
        void ClearAllUserCaches(int userId);
        void ClearUserCacheDynamically(int userId);
        void ForceRefreshDepartmentOverviewCache(int userId);
    }

    public class AdvancedPermissionManagerService : IAdvancedPermissionManagerService
    {
        private readonly SqlServerDb _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdvancedPermissionManagerService> _logger;

        public AdvancedPermissionManagerService(SqlServerDb db, IMemoryCache cache, ILogger<AdvancedPermissionManagerService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        private SqlParameter[] ConvertToSqlParameters(Dictionary<string, object> parameters)
        {
            return parameters.Select(p => new SqlParameter(p.Key, p.Value ?? DBNull.Value)).ToArray();
        }

        #region Menu Permissions

        public async Task<bool> CanViewMenuAsync(int userId, string menuKey)
        {
            try
            {
                var cacheKey = $"menu_permission_{userId}_{menuKey}";
                if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                    return cachedResult;

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@MenuKey", menuKey }
                });

                var result = _db.ExecuteScalar("EXEC CanUserViewMenu @UserId, @MenuKey", parameters);
                var canView = Convert.ToBoolean(result);

                _cache.Set(cacheKey, canView, TimeSpan.FromMinutes(10));
                return canView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking menu permission for user {UserId} and menu {MenuKey}", userId, menuKey);
                return false;
            }
        }

        public async Task<List<MenuPermission>> GetUserMenuPermissionsAsync(int userId)
        {
            try
            {
                var cacheKey = $"user_menu_permissions_{userId}";
                if (_cache.TryGetValue(cacheKey, out List<MenuPermission> cachedResult))
                    return cachedResult;

                // Define all possible menu items
                var allMenuItems = new[] { "PROFILE", "NOTIFICATIONS", "DASHBOARD", "EMPLOYEES", "CONTROLLERS", "LICENSES", "CERTIFICATES", "OBSERVATIONS", "CONFIGURATION", "PERMISSIONS", "ORGANIZATION_VIEW", "PROJECT_VIEW" };
                
                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId }
                });

                var dataTable = _db.ExecuteQuery("EXEC GetUserMenuPermissions @UserId", parameters);
                var existingPermissions = new Dictionary<string, bool>();

                foreach (DataRow row in dataTable.Rows)
                {
                    existingPermissions[row["MenuKey"].ToString()] = Convert.ToBoolean(row["IsVisible"]);
                }

                var permissions = new List<MenuPermission>();
                foreach (var menuKey in allMenuItems)
                {
                    permissions.Add(new MenuPermission
                    {
                        MenuKey = menuKey,
                        IsVisible = existingPermissions.ContainsKey(menuKey) ? existingPermissions[menuKey] : false
                    });
                }

                _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu permissions for user {UserId}", userId);
                return new List<MenuPermission>();
            }
        }

        public async Task<bool> SetMenuPermissionAsync(int userId, string menuKey, bool isVisible)
        {
            try
            {
                // CRITICAL FIX: Check if user is Admin - Admin should have all permissions
                var isAdmin = _db.ExecuteScalar("SELECT CASE WHEN RoleName = 'Admin' THEN 1 ELSE 0 END FROM users WHERE UserId = @UserId", 
                    ConvertToSqlParameters(new Dictionary<string, object> { { "@UserId", userId } }));
                
                if (isAdmin != null && Convert.ToBoolean(isAdmin))
                {
                    _logger.LogInformation("User {UserId} is Admin - setting menu permission {MenuKey} = {IsVisible}", 
                        userId, menuKey, isVisible);
                    // Admin gets all permissions, so we proceed with the save
                }

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@MenuKey", menuKey },
                    { "@IsVisible", isVisible }
                });

                var query = @"
                    IF EXISTS (SELECT 1 FROM UserMenuPermissions WHERE UserId = @UserId AND MenuKey = @MenuKey)
                        UPDATE UserMenuPermissions 
                        SET IsVisible = @IsVisible, UpdatedAt = GETDATE()
                        WHERE UserId = @UserId AND MenuKey = @MenuKey
                    ELSE
                        INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt)
                        VALUES (@UserId, @MenuKey, @IsVisible, 1, GETDATE())
                ";

                _db.ExecuteNonQuery(query, parameters);

                // Clear cache
                _cache.Remove($"menu_permission_{userId}_{menuKey}");
                _cache.Remove($"user_menu_permissions_{userId}");
                _cache.Remove($"visible_menu_items_{userId}");

                _logger.LogInformation("Updated menu permission for user {UserId}: {MenuKey} = {IsVisible}", 
                    userId, menuKey, isVisible);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting menu permission for user {UserId} and menu {MenuKey}", userId, menuKey);
                return false;
            }
        }

        #endregion

        #region Operation Permissions

        public async Task<bool> CanPerformOperationAsync(int userId, string entityType, string operationType, string scope = "All", int? scopeId = null)
        {
            try
            {
                // Get user's last permission update timestamp
                var userUpdateKey = $"user_permission_updated_{userId}";
                _cache.TryGetValue(userUpdateKey, out DateTime? lastUpdate);
                
                var cacheKey = $"operation_permission_{userId}_{entityType}_{operationType}_{scopeId}";
                
                // Check if we have cached result and it's still valid
                if (_cache.TryGetValue(cacheKey, out bool cachedResult) && lastUpdate.HasValue)
                {
                    // Check if the cached result is newer than the last permission update
                    if (_cache.TryGetValue($"{cacheKey}_timestamp", out DateTime cacheTimestamp) && cacheTimestamp > lastUpdate.Value)
                    {
                        return cachedResult;
                    }
                    else
                    {
                        // Cache is stale, remove it
                        _cache.Remove(cacheKey);
                        _cache.Remove($"{cacheKey}_timestamp");
                    }
                }

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@EntityType", entityType },
                    { "@OperationType", operationType },
                    { "@Scope", scope ?? "All" },
                    { "@ScopeId", scopeId ?? (object)DBNull.Value }
                });

                var result = _db.ExecuteScalar("EXEC CanUserPerformOperation @UserId, @EntityType, @OperationType, @Scope, @ScopeId", parameters);
                var isAllowed = Convert.ToBoolean(result);

                // Cache the result with timestamp
                var now = DateTime.UtcNow;
                _cache.Set(cacheKey, isAllowed, TimeSpan.FromHours(1)); // Longer cache time since we have timestamp validation
                _cache.Set($"{cacheKey}_timestamp", now, TimeSpan.FromHours(1));
                
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking operation permission for user {UserId}, entity {EntityType}, operation {OperationType}", userId, entityType, operationType);
                return false;
            }
        }

        public async Task<List<OperationPermission>> GetUserOperationPermissionsAsync(int userId)
        {
            try
            {
                // Define all possible entity types and operations
                var entityTypes = new[] { "Employee", "Controller", "ControllerCertificate", "EmployeeCertificate", "ControllerObservation", "EmployeeObservation", "ControllerLicense", "EmployeeLicense", "Project", "Country", "Airport", "DepartmentOverview" };
                var operationTypes = new[] { "View", "Add", "Edit", "Delete", "Export" };
                
                var permissions = new List<OperationPermission>();

                // Get existing permissions from database
                var query = @"
                    SELECT 
                        uop.UserOperationPermissionId,
                        uop.EntityType,
                        uop.OperationType,
                        uop.IsAllowed,
                        uop.Scope,
                        uop.ScopeId,
                        ISNULL(p.PermissionName, uop.EntityType + ' ' + uop.OperationType) as PermissionName,
                        ISNULL(p.PermissionDescription, 'Permission to ' + LOWER(uop.OperationType) + ' ' + LOWER(uop.EntityType)) as PermissionDescription
                    FROM UserOperationPermissions uop
                    LEFT JOIN Permissions p ON uop.PermissionId = p.PermissionId
                    WHERE uop.UserId = @UserId AND uop.IsActive = 1
                    ORDER BY uop.EntityType, uop.OperationType
                ";

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId }
                });

                var dataTable = _db.ExecuteQuery(query, parameters);
                var existingPermissions = new Dictionary<string, OperationPermission>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var key = $"{row["EntityType"]}_{row["OperationType"]}";
                    existingPermissions[key] = new OperationPermission
                    {
                        Id = Convert.ToInt32(row["UserOperationPermissionId"]),
                        EntityType = row["EntityType"].ToString(),
                        OperationType = row["OperationType"].ToString(),
                        IsAllowed = Convert.ToBoolean(row["IsAllowed"]),
                        Scope = row["Scope"].ToString(),
                        ScopeId = row["ScopeId"] == DBNull.Value ? null : Convert.ToInt32(row["ScopeId"]),
                        PermissionName = row["PermissionName"].ToString(),
                        PermissionDescription = row["PermissionDescription"].ToString()
                    };
                }

                // Return all possible permissions (existing and missing)
                foreach (var entityType in entityTypes)
                {
                    foreach (var operationType in operationTypes)
                    {
                        var key = $"{entityType}_{operationType}";
                        if (existingPermissions.ContainsKey(key))
                        {
                            // Add existing permission
                            permissions.Add(existingPermissions[key]);
                        }
                        else
                        {
                            // Add missing permission (default: not allowed)
                            permissions.Add(new OperationPermission
                            {
                                Id = 0, // No ID for missing permissions
                                EntityType = entityType,
                                OperationType = operationType,
                                IsAllowed = false, // Default: not allowed
                                Scope = "All",
                                ScopeId = null,
                                PermissionName = $"{entityType} {operationType}",
                                PermissionDescription = $"Permission to {operationType.ToLower()} {entityType.ToLower()}"
                            });
                        }
                    }
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operation permissions for user {UserId}", userId);
                return new List<OperationPermission>();
            }
        }

        public async Task<bool> SetOperationPermissionAsync(int userId, string entityType, string operationType, bool isAllowed, string scope = "All", int? scopeId = null)
        {
            try
            {
                // CRITICAL FIX: Check if user is Admin - Admin should have all permissions
                var isAdmin = _db.ExecuteScalar("SELECT CASE WHEN RoleName = 'Admin' THEN 1 ELSE 0 END FROM users WHERE UserId = @UserId", 
                    ConvertToSqlParameters(new Dictionary<string, object> { { "@UserId", userId } }));
                
                if (isAdmin != null && Convert.ToBoolean(isAdmin))
                {
                    _logger.LogInformation("User {UserId} is Admin - setting operation permission {EntityType}.{OperationType} = {IsAllowed}", 
                        userId, entityType, operationType, isAllowed);
                    // Admin gets all permissions, so we proceed with the save
                }

                // Get or create permission ID
                // Fix: Convert entity type to proper permission key format
                var entityKey = entityType switch
                {
                    "ControllerObservation" => "CONTROLLEROBSERVATION",
                    "EmployeeObservation" => "EMPLOYEEOBSERVATION",
                    "ControllerCertificate" => "CONTROLLERCERTIFICATE",
                    "EmployeeCertificate" => "EMPLOYEECERTIFICATE",
                    "ControllerLicense" => "CONTROLLERLICENSE",
                    "EmployeeLicense" => "EMPLOYEELICENSE",
                    _ => entityType.ToUpper()
                };
                var permissionKey = $"{entityKey}_{operationType.ToUpper()}";
                var permissionQuery = "SELECT PermissionId FROM Permissions WHERE PermissionKey = @PermissionKey";
                var permissionParams = new Dictionary<string, object> { { "@PermissionKey", permissionKey } };
                var permissionId = _db.ExecuteScalar(permissionQuery, ConvertToSqlParameters(permissionParams));

                // If permission doesn't exist, create it
                if (permissionId == null)
                {
                    var createPermissionQuery = @"
                        INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
                        VALUES (@PermissionKey, @PermissionName, @PermissionDescription, @CategoryName, 1, GETDATE());
                        SELECT SCOPE_IDENTITY();
                    ";
                    
                    var createParams = ConvertToSqlParameters(new Dictionary<string, object>
                    {
                        { "@PermissionKey", permissionKey },
                        { "@PermissionName", $"{entityType} {operationType}" },
                        { "@PermissionDescription", $"Permission to {operationType.ToLower()} {entityType.ToLower()}" },
                        { "@CategoryName", GetCategoryName(entityType) }
                    });
                    
                    permissionId = _db.ExecuteScalar(createPermissionQuery, createParams);
                    _logger.LogInformation("Created new permission: {PermissionKey} with ID: {PermissionId}", permissionKey, permissionId);
                }

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@PermissionId", permissionId },
                    { "@EntityType", entityType },
                    { "@OperationType", operationType },
                    { "@IsAllowed", isAllowed },
                    { "@Scope", scope },
                    { "@ScopeId", scopeId ?? (object)DBNull.Value }
                });

                var query = @"
                    IF EXISTS (SELECT 1 FROM UserOperationPermissions 
                              WHERE UserId = @UserId AND PermissionId = @PermissionId AND EntityType = @EntityType AND OperationType = @OperationType)
                        UPDATE UserOperationPermissions 
                        SET IsAllowed = @IsAllowed, Scope = @Scope, ScopeId = @ScopeId, UpdatedAt = GETDATE()
                        WHERE UserId = @UserId AND PermissionId = @PermissionId AND EntityType = @EntityType AND OperationType = @OperationType
                    ELSE
                        INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt)
                        VALUES (@UserId, @PermissionId, @EntityType, @OperationType, @IsAllowed, @Scope, @ScopeId, 1, GETDATE())
                ";

                _db.ExecuteNonQuery(query, parameters);

                // Update user permission timestamp to invalidate all caches
                var userUpdateKey = $"user_permission_updated_{userId}";
                _cache.Set(userUpdateKey, DateTime.UtcNow, TimeSpan.FromDays(1));
                
                // Clear cache - Clear specific permission and all user caches
                _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}_{scopeId}");
                _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}_null");
                _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}_All");
                _cache.Remove($"user_operation_permissions_{userId}");
                
                // Clear all user caches to ensure immediate effect
                ClearAllUserCaches(userId);

                _logger.LogInformation("Updated operation permission for user {UserId}: {EntityType}.{OperationType} = {IsAllowed}", 
                    userId, entityType, operationType, isAllowed);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting operation permission for user {UserId}, entity {EntityType}, operation {OperationType}", userId, entityType, operationType);
                return false;
            }
        }

        #endregion

        #region Organizational Permissions

        public async Task<bool> CanAccessEntityAsync(int userId, string entityType, int entityId, string operation)
        {
            try
            {
                var query = @"
                    SELECT 
                        CASE 
                            WHEN @Operation = 'View' THEN CanView
                            WHEN @Operation = 'Edit' THEN CanEdit
                            WHEN @Operation = 'Delete' THEN CanDelete
                            WHEN @Operation = 'Create' THEN CanCreate
                            ELSE 0
                        END as IsAllowed
                    FROM UserOrganizationalPermissions
                    WHERE UserId = @UserId 
                        AND PermissionType = @EntityType 
                        AND EntityId = @EntityId 
                        AND IsActive = 1
                ";

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@EntityType", entityType },
                    { "@EntityId", entityId },
                    { "@Operation", operation }
                });

                var result = _db.ExecuteScalar(query, parameters);
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking organizational permission for user {UserId}, entity {EntityType}, id {EntityId}, operation {Operation}", userId, entityType, entityId, operation);
                return false;
            }
        }

        public async Task<List<OrganizationalPermission>> GetUserOrganizationalPermissionsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("GetUserOrganizationalPermissionsAsync: Loading permissions for user {UserId}", userId);
                
                var query = @"
                    SELECT 
                        UserOrganizationalPermissionId,
                        PermissionType,
                        EntityId,
                        EntityName,
                        CanView,
                        CanEdit,
                        CanDelete,
                        CanCreate
                    FROM UserOrganizationalPermissions
                    WHERE UserId = @UserId AND IsActive = 1
                    ORDER BY PermissionType, EntityName
                ";

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId }
                });

                _logger.LogInformation("GetUserOrganizationalPermissionsAsync: Executing query for user {UserId}", userId);
                var dataTable = _db.ExecuteQuery(query, parameters);
                _logger.LogInformation("GetUserOrganizationalPermissionsAsync: Found {RowCount} rows for user {UserId}", dataTable.Rows.Count, userId);
                
                var permissions = new List<OrganizationalPermission>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var canView = Convert.ToBoolean(row["CanView"]);
                    var canEdit = Convert.ToBoolean(row["CanEdit"]);
                    var canDelete = Convert.ToBoolean(row["CanDelete"]);
                    var canCreate = Convert.ToBoolean(row["CanCreate"]);
                    
                    // Add all permissions (both allowed and not allowed)
                    permissions.Add(new OrganizationalPermission
                    {
                        Id = Convert.ToInt32(row["UserOrganizationalPermissionId"]),
                        PermissionType = row["PermissionType"].ToString(),
                        EntityId = Convert.ToInt32(row["EntityId"]),
                        EntityName = row["EntityName"].ToString(),
                        CanView = canView,
                        CanEdit = canEdit,
                        CanDelete = canDelete,
                        CanCreate = canCreate
                    });
                }

                _logger.LogInformation("GetUserOrganizationalPermissionsAsync: Returning {PermissionCount} permissions for user {UserId}", permissions.Count, userId);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational permissions for user {UserId}", userId);
                return new List<OrganizationalPermission>();
            }
        }

        public async Task<bool> SetOrganizationalPermissionAsync(int userId, string entityType, int entityId, string entityName, bool canView, bool canEdit, bool canDelete, bool canCreate)
        {
            try
            {
                _logger.LogInformation("SetOrganizationalPermissionAsync: Called with userId={UserId}, entityType={EntityType}, entityId={EntityId}, entityName={EntityName}, canView={CanView}, canEdit={CanEdit}, canDelete={CanDelete}, canCreate={CanCreate}", 
                    userId, entityType, entityId, entityName, canView, canEdit, canDelete, canCreate);
                
                // CRITICAL FIX: Check if user is Admin - Admin should have all permissions
                var isAdmin = _db.ExecuteScalar("SELECT CASE WHEN RoleName = 'Admin' THEN 1 ELSE 0 END FROM users WHERE UserId = @UserId", 
                    ConvertToSqlParameters(new Dictionary<string, object> { { "@UserId", userId } }));
                
                if (isAdmin != null && Convert.ToBoolean(isAdmin))
                {
                    _logger.LogInformation("User {UserId} is Admin - setting organizational permission {EntityType}.{EntityId} = View:{CanView}, Edit:{CanEdit}, Delete:{CanDelete}, Create:{CanCreate}", 
                        userId, entityType, entityId, canView, canEdit, canDelete, canCreate);
                    // Admin gets all permissions, so we proceed with the save
                }

                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@EntityType", entityType },
                    { "@EntityId", entityId },
                    { "@EntityName", entityName },
                    { "@CanView", canView },
                    { "@CanEdit", canEdit },
                    { "@CanDelete", canDelete },
                    { "@CanCreate", canCreate }
                });

                var query = @"
                    IF EXISTS (SELECT 1 FROM UserOrganizationalPermissions 
                              WHERE UserId = @UserId AND PermissionType = @EntityType AND EntityId = @EntityId)
                        UPDATE UserOrganizationalPermissions 
                        SET EntityName = @EntityName, CanView = @CanView, CanEdit = @CanEdit, 
                            CanDelete = @CanDelete, CanCreate = @CanCreate, UpdatedAt = GETDATE()
                        WHERE UserId = @UserId AND PermissionType = @EntityType AND EntityId = @EntityId
                    ELSE
                        INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
                        VALUES (@UserId, @EntityType, @EntityId, @EntityName, @CanView, @CanEdit, @CanDelete, @CanCreate, 1, GETDATE())
                ";

                _logger.LogInformation("SetOrganizationalPermissionAsync: Executing SQL query for userId={UserId}, entityType={EntityType}, entityId={EntityId}", userId, entityType, entityId);
                _db.ExecuteNonQuery(query, parameters);
                _logger.LogInformation("SetOrganizationalPermissionAsync: SQL query executed successfully for userId={UserId}, entityType={EntityType}, entityId={EntityId}", userId, entityType, entityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting organizational permission for user {UserId}, entity {EntityType}, id {EntityId}", userId, entityType, entityId);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private string GetCategoryName(string entityType)
        {
            return entityType switch
            {
                "Employee" => "Employee Management",
                "Controller" => "Controller Management", 
                "License" => "License Management",
                "Certificate" => "Certificate Management",
                "ControllerCertificate" => "Controller Certificate Management",
                "EmployeeCertificate" => "Employee Certificate Management",
                "ControllerObservation" => "Controller Observation Management",
                "EmployeeObservation" => "Employee Observation Management",
                "ControllerLicense" => "Controller License Management",
                "EmployeeLicense" => "Employee License Management",
                "Project" => "Training & Development Management",
                "Country" => "Organization Management",
                "Airport" => "Organization Management",
                "Configuration" => "Configuration Management",
                "DepartmentOverview" => "System Settings",
                _ => "General"
            };
        }

        #endregion

        #region Permission Management

        public async Task<List<UserPermissionSummaryModel>> GetAllUsersWithPermissionsAsync()
        {
            try
            {
                var query = @"
                    SELECT DISTINCT
                        u.userid,
                        u.username,
                        COALESCE(c.fullname, e.fullname, u.username) as fullname,
                        u.rolename,
                        COUNT(DISTINCT CASE WHEN ump.IsVisible = 1 THEN ump.UserMenuPermissionId END) as MenuPermissionsCount,
                        COUNT(DISTINCT CASE WHEN uop.IsAllowed = 1 THEN uop.UserOperationPermissionId END) as OperationPermissionsCount,
                        COUNT(DISTINCT CASE WHEN (uorg.CanView = 1 OR uorg.CanEdit = 1 OR uorg.CanDelete = 1 OR uorg.CanCreate = 1) THEN uorg.UserOrganizationalPermissionId END) as OrganizationalPermissionsCount
                    FROM users u
                    LEFT JOIN controllers c ON u.userid = c.userid
                    LEFT JOIN employees e ON u.userid = e.userid
                    LEFT JOIN UserMenuPermissions ump ON u.userid = ump.UserId AND ump.IsActive = 1
                    LEFT JOIN UserOperationPermissions uop ON u.userid = uop.UserId AND uop.IsActive = 1
                    LEFT JOIN UserOrganizationalPermissions uorg ON u.userid = uorg.UserId AND uorg.IsActive = 1
                    WHERE (c.userid IS NOT NULL OR e.userid IS NOT NULL OR u.rolename = 'Admin')
                    GROUP BY u.userid, u.username, u.rolename, c.fullname, e.fullname
                    ORDER BY u.username
                ";

                var dataTable = _db.ExecuteQuery(query, new SqlParameter[0]);
                var users = new List<UserPermissionSummaryModel>();

                foreach (DataRow row in dataTable.Rows)
                {
                    users.Add(new UserPermissionSummaryModel
                    {
                        UserId = Convert.ToInt32(row["userid"]),
                        Username = row["username"].ToString(),
                        FullName = row["fullname"].ToString(),
                        RoleName = row["rolename"].ToString(),
                        MenuPermissionsCount = Convert.ToInt32(row["MenuPermissionsCount"]),
                        OperationPermissionsCount = Convert.ToInt32(row["OperationPermissionsCount"]),
                        OrganizationalPermissionsCount = Convert.ToInt32(row["OrganizationalPermissionsCount"])
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users with permissions");
                return new List<UserPermissionSummaryModel>();
            }
        }

        public async Task<UserPermissionDetails> GetUserPermissionDetailsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("GetUserPermissionDetailsAsync: Starting for user {UserId}", userId);
                
                var userQuery = @"
                    SELECT u.userid, u.username, 
                           COALESCE(c.fullname, e.fullname, u.username) as fullname, 
                           u.rolename 
                    FROM users u
                    LEFT JOIN controllers c ON u.userid = c.userid
                    LEFT JOIN employees e ON u.userid = e.userid
                    WHERE u.userid = @UserId";
                var userParams = new Dictionary<string, object> { { "@UserId", userId } };
                var userData = _db.ExecuteQuery(userQuery, ConvertToSqlParameters(userParams));

                if (userData.Rows.Count == 0)
                {
                    _logger.LogWarning("GetUserPermissionDetailsAsync: No user found for userId {UserId}", userId);
                    return null;
                }

                var userRow = userData.Rows[0];
                _logger.LogInformation("GetUserPermissionDetailsAsync: Found user {Username} with role {RoleName}", 
                    userRow["username"].ToString(), userRow["rolename"].ToString());
                
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loading menu permissions for user {UserId}", userId);
                var menuPermissions = await GetUserMenuPermissionsAsync(userId);
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loaded {MenuCount} menu permissions", menuPermissions.Count);
                
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loading operation permissions for user {UserId}", userId);
                var operationPermissions = await GetUserOperationPermissionsAsync(userId);
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loaded {OperationCount} operation permissions", operationPermissions.Count);
                
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loading organizational permissions for user {UserId}", userId);
                var organizationalPermissions = await GetUserOrganizationalPermissionsAsync(userId);
                _logger.LogInformation("GetUserPermissionDetailsAsync: Loaded {OrganizationalCount} organizational permissions", organizationalPermissions.Count);
                
                var userDetails = new UserPermissionDetails
                {
                    UserId = Convert.ToInt32(userRow["userid"]),
                    Username = userRow["username"].ToString(),
                    FullName = userRow["fullname"].ToString(),
                    RoleName = userRow["rolename"].ToString(),
                    MenuPermissions = menuPermissions,
                    OperationPermissions = operationPermissions,
                    OrganizationalPermissions = organizationalPermissions
                };

                _logger.LogInformation("GetUserPermissionDetailsAsync: Completed for user {UserId} - Menu: {MenuCount}, Operation: {OperationCount}, Organizational: {OrganizationalCount}", 
                    userId, menuPermissions.Count, operationPermissions.Count, organizationalPermissions.Count);
                
                return userDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permission details for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CopyPermissionsFromUserAsync(int fromUserId, int toUserId)
        {
            try
            {
                // Copy menu permissions
                var copyMenuQuery = @"
                    INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive)
                    SELECT @ToUserId, MenuKey, IsVisible, IsActive
                    FROM UserMenuPermissions
                    WHERE UserId = @FromUserId AND IsActive = 1
                ";

                var menuParams = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@FromUserId", fromUserId },
                    { "@ToUserId", toUserId }
                });

                _db.ExecuteNonQuery(copyMenuQuery, menuParams);

                // Copy operation permissions
                var copyOperationQuery = @"
                    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive)
                    SELECT @ToUserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive
                    FROM UserOperationPermissions
                    WHERE UserId = @FromUserId AND IsActive = 1
                ";

                _db.ExecuteNonQuery(copyOperationQuery, menuParams);

                // Copy organizational permissions
                var copyOrgQuery = @"
                    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive)
                    SELECT @ToUserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive
                    FROM UserOrganizationalPermissions
                    WHERE UserId = @FromUserId AND IsActive = 1
                ";

                _db.ExecuteNonQuery(copyOrgQuery, menuParams);

                // Clear cache for target user
                _cache.Remove($"user_menu_permissions_{toUserId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying permissions from user {FromUserId} to user {ToUserId}", fromUserId, toUserId);
                return false;
            }
        }

        public async Task<bool> ResetUserPermissionsAsync(int userId)
        {
            try
            {
                var parameters = ConvertToSqlParameters(new Dictionary<string, object> { { "@UserId", userId } });

                // Deactivate all permissions
                var deactivateQuery = @"
                    UPDATE UserMenuPermissions SET IsActive = 0, UpdatedAt = GETDATE() WHERE UserId = @UserId;
                    UPDATE UserOperationPermissions SET IsActive = 0, UpdatedAt = GETDATE() WHERE UserId = @UserId;
                    UPDATE UserOrganizationalPermissions SET IsActive = 0, UpdatedAt = GETDATE() WHERE UserId = @UserId;
                ";

                _db.ExecuteNonQuery(deactivateQuery, parameters);

                // Clear cache
                _cache.Remove($"user_menu_permissions_{userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting permissions for user {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region Legacy Support

        public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
        {
            try
            {
                // Special handling for DEPARTMENT_OVERVIEW permission
                if (permissionKey == "DEPARTMENT_OVERVIEW")
                {
                    // Clear any cached results for this permission
                    _cache.Remove($"dept_overview_permission_{userId}");
                    
                    // Direct database check for DepartmentOverview permission
                    var deptQuery = @"
                        SELECT COUNT(1) 
                        FROM UserOperationPermissions uop
                        WHERE uop.UserId = @UserId 
                        AND uop.EntityType = 'DepartmentOverview' 
                        AND uop.OperationType = 'View' 
                        AND uop.IsAllowed = 1 
                        AND uop.IsActive = 1";
                    
                    var deptParameters = ConvertToSqlParameters(new Dictionary<string, object>
                    {
                        { "@UserId", userId }
                    });

                    var deptResult = _db.ExecuteScalar(deptQuery, deptParameters);
                    var hasPermission = Convert.ToInt32(deptResult) > 0;
                    
                    // Cache the result with short expiration
                    _cache.Set($"dept_overview_permission_{userId}", hasPermission, TimeSpan.FromMinutes(5));
                    
                    _logger.LogInformation("HasPermissionAsync: DEPARTMENT_OVERVIEW check for user {UserId} = {Result}", userId, hasPermission);
                    return hasPermission;
                }

                // Check if it's a menu permission
                if (permissionKey.StartsWith("MENU_"))
                {
                    var menuKey = permissionKey.Replace("MENU_", "").Replace("_VIEW", "");
                    return await CanViewMenuAsync(userId, menuKey);
                }

                // Check if it's an operation permission
                var parts = permissionKey.Split('_');
                if (parts.Length >= 2)
                {
                    var entityType = parts[0];
                    var operationType = parts[1];
                    return await CanPerformOperationAsync(userId, entityType, operationType);
                }

                // Fallback to old permission system
                var query = "SELECT COUNT(1) FROM UserDepartmentPermissions udp INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId WHERE udp.UserId = @UserId AND p.PermissionKey = @PermissionKey AND udp.IsActive = 1 AND p.IsActive = 1";
                var parameters = ConvertToSqlParameters(new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@PermissionKey", permissionKey }
                });

                var result = _db.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {PermissionKey} for user {UserId}", permissionKey, userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var permissions = new List<string>();

                // Get menu permissions
                var menuPermissions = await GetUserMenuPermissionsAsync(userId);
                foreach (var menu in menuPermissions.Where(m => m.IsVisible))
                {
                    permissions.Add($"MENU_{menu.MenuKey.ToUpper()}_VIEW");
                }

                // Get operation permissions
                var operationPermissions = await GetUserOperationPermissionsAsync(userId);
                foreach (var op in operationPermissions.Where(o => o.IsAllowed))
                {
                    permissions.Add($"{op.EntityType.ToUpper()}_{op.OperationType.ToUpper()}");
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions for user {UserId}", userId);
                return new List<string>();
            }
        }

        #region Cache Management

        public void ClearAllUserCaches(int userId)
        {
            try
            {
                // Clear menu permission caches
                _cache.Remove($"user_menu_permissions_{userId}");
                _cache.Remove($"visible_menu_items_{userId}");
                
                // Clear operation permission caches
                _cache.Remove($"user_operation_permissions_{userId}");
                
                // Clear organizational permission caches
                _cache.Remove($"user_organizational_permissions_{userId}");
                
                // Clear individual permission caches
                var menuKeys = new[] { "PROFILE", "NOTIFICATIONS", "DASHBOARD", "EMPLOYEES", "CONTROLLERS", "LICENSES", "CERTIFICATES", "OBSERVATIONS", "CONFIGURATION", "PERMISSIONS" };
                foreach (var menuKey in menuKeys)
                {
                    _cache.Remove($"menu_permission_{userId}_{menuKey}");
                }
                
                // Clear individual operation permission caches
                var entityTypes = new[] { "EmployeeLicense", "ControllerLicense", "ControllerCertificate", "EmployeeCertificate", "ControllerObservation", "EmployeeObservation", "Employee", "Controller", "Certificate", "Observation", "Project", "Country", "Airport", "Configuration" };
                var operationTypes = new[] { "View", "Add", "Edit", "Delete", "Export" };
                
                foreach (var entityType in entityTypes)
                {
                    foreach (var operationType in operationTypes)
                    {
                        _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}");
                        _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}_null");
                        _cache.Remove($"operation_permission_{userId}_{entityType}_{operationType}_All");
                    }
                }
                
                // Update user permission timestamp
                UpdateUserPermissionTimestamp(userId);
                
                _logger.LogInformation("Cleared all caches for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing caches for user {UserId}", userId);
            }
        }

        public void ClearUserCacheDynamically(int userId)
        {
            ClearAllUserCaches(userId);
        }

        // NEW: Force refresh DepartmentOverview permission cache
        public void ForceRefreshDepartmentOverviewCache(int userId)
        {
            try
            {
                // Clear specific DepartmentOverview cache
                _cache.Remove($"dept_overview_permission_{userId}");
                
                // Clear all user caches
                ClearAllUserCaches(userId);
                
                // Update timestamp to force cache invalidation
                UpdateUserPermissionTimestamp(userId);
                
                _logger.LogInformation("ForceRefreshDepartmentOverviewCache: Cleared all caches for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force refreshing DepartmentOverview cache for user {UserId}", userId);
            }
        }

        // NEW: Comprehensive permission fix for any user
        public async Task<bool> FixUserCountryAirportPermissionsAsync(string username)
        {
            try
            {
                // Get user ID
                var userIdResult = _db.ExecuteScalar("SELECT UserId FROM Users WHERE Username = @Username", 
                    new SqlParameter("@Username", username));
                
                if (userIdResult == null || userIdResult == DBNull.Value)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return false;
                }
                
                int userId = Convert.ToInt32(userIdResult);
                
                // Ensure LastPermissionUpdate column exists
                EnsureLastPermissionUpdateColumnExists();
                
                // Clear existing Country and Airport permissions
                _db.ExecuteNonQuery("DELETE FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport')", 
                    new SqlParameter("@UserId", userId));
                
                // Add Country permissions
                var countryPermissions = new[]
                {
                    new { EntityType = "Country", OperationType = "View" },
                    new { EntityType = "Country", OperationType = "Add" },
                    new { EntityType = "Country", OperationType = "Edit" },
                    new { EntityType = "Country", OperationType = "Delete" },
                    new { EntityType = "Country", OperationType = "Export" }
                };
                
                foreach (var perm in countryPermissions)
                {
                    _db.ExecuteNonQuery(@"
                        INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
                        VALUES (@UserId, 1, @EntityType, @OperationType, 1, 'All', GETDATE(), GETDATE())",
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@EntityType", perm.EntityType),
                        new SqlParameter("@OperationType", perm.OperationType));
                }
                
                // Add Airport permissions
                var airportPermissions = new[]
                {
                    new { EntityType = "Airport", OperationType = "View" },
                    new { EntityType = "Airport", OperationType = "Add" },
                    new { EntityType = "Airport", OperationType = "Edit" },
                    new { EntityType = "Airport", OperationType = "Delete" },
                    new { EntityType = "Airport", OperationType = "Export" }
                };
                
                foreach (var perm in airportPermissions)
                {
                    _db.ExecuteNonQuery(@"
                        INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
                        VALUES (@UserId, 1, @EntityType, @OperationType, 1, 'All', GETDATE(), GETDATE())",
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@EntityType", perm.EntityType),
                        new SqlParameter("@OperationType", perm.OperationType));
                }
                
                // Update timestamp and clear cache
                UpdateUserPermissionTimestamp(userId);
                ClearAllUserCaches(userId);
                
                _logger.LogInformation("Successfully applied Country and Airport permissions to user {Username} (ID: {UserId})", username, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying Country and Airport permissions to user {Username}", username);
                return false;
            }
        }

        // NEW: Ensure LastPermissionUpdate column exists
        private void EnsureLastPermissionUpdateColumnExists()
        {
            try
            {
                var columnExists = _db.ExecuteScalar(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LastPermissionUpdate'");
                
                if (Convert.ToInt32(columnExists) == 0)
                {
                    _db.ExecuteNonQuery("ALTER TABLE Users ADD LastPermissionUpdate datetime NULL");
                    _logger.LogInformation("Added LastPermissionUpdate column to Users table");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring LastPermissionUpdate column exists");
            }
        }

        // NEW: Update user permission timestamp
        private void UpdateUserPermissionTimestamp(int userId)
        {
            try
            {
                _db.ExecuteNonQuery("UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId", 
                    new SqlParameter("@UserId", userId));
                
                // Update cache timestamp
                var userUpdateKey = $"user_permission_updated_{userId}";
                _cache.Set(userUpdateKey, DateTime.UtcNow, TimeSpan.FromDays(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission timestamp for user {UserId}", userId);
            }
        }

        #endregion

        #endregion
    }

    #region Models

    public class MenuPermission
    {
        public string MenuKey { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
    }

    public class OperationPermission
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
        public string Scope { get; set; } = string.Empty;
        public int? ScopeId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionDescription { get; set; } = string.Empty;
    }

    public class OrganizationalPermission
    {
        public int Id { get; set; }
        public string PermissionType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreate { get; set; }
    }

    public class UserPermissionSummaryModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int MenuPermissionsCount { get; set; }
        public int OperationPermissionsCount { get; set; }
        public int OrganizationalPermissionsCount { get; set; }
    }

    public class UserPermissionDetails
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
        public List<OperationPermission> OperationPermissions { get; set; } = new List<OperationPermission>();
        public List<OrganizationalPermission> OrganizationalPermissions { get; set; } = new List<OrganizationalPermission>();
    }

    #endregion
}