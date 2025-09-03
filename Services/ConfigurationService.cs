using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using System.Data;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IConfigurationService
    {
        // Category Operations
        List<ConfigurationCategory> GetAllCategories();
        ConfigurationCategory? GetCategoryById(int categoryId);
        ConfigurationCategory? GetCategoryByName(string categoryName);
        bool AddCategory(ConfigurationCategory category);
        bool UpdateCategory(ConfigurationCategory category);
        bool DeleteCategory(int categoryId);

        // Value Operations
        List<ConfigurationValue> GetValuesByCategory(string categoryName);
        List<ConfigurationValue> GetValuesByCategoryId(int categoryId);
        ConfigurationValue? GetValueById(int valueId);
        bool AddValue(ConfigurationValue value);
        bool UpdateValue(ConfigurationValue value);
        bool DeleteValue(int valueId);

        // Dropdown Operations
        List<SelectListItem> GetDropdownValues(string categoryName);
        List<ConfigurationSelectListItem> GetConfigurationSelectList(string categoryName);

        // Log Operations
        List<ConfigurationLog> GetLogs(int? valueId = null, DateTime? fromDate = null, DateTime? toDate = null);
        bool AddLog(ConfigurationLog log);

        // Statistics
        ConfigurationStatisticsViewModel GetStatistics();

        // Cache Operations
        void ClearCache();
        void RefreshCache();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY_PREFIX = "Configuration_";

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger, IMemoryCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        private SqlServerDb GetDb()
        {
            var passwordHasher = new PasswordHasher<ControllerUser>();
            return new SqlServerDb(_configuration, passwordHasher, null);
        }

        #region Category Operations

        public List<ConfigurationCategory> GetAllCategories()
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}Categories";
            
            if (_cache.TryGetValue(cacheKey, out List<ConfigurationCategory> cachedCategories))
            {
                return cachedCategories;
            }

            const string sql = @"
                SELECT CategoryId, CategoryName, DisplayName, Description, 
                       IsActive, DisplayOrder, CreatedDate, ModifiedDate
                FROM ConfigurationCategories 
                WHERE IsActive = 1 
                ORDER BY DisplayOrder, DisplayName";

            var dt = GetDb().ExecuteQuery(sql);
            var categories = new List<ConfigurationCategory>();

            foreach (DataRow row in dt.Rows)
            {
                categories.Add(new ConfigurationCategory
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    DisplayName = row["DisplayName"].ToString(),
                    Description = row["Description"]?.ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    ModifiedDate = row["ModifiedDate"] as DateTime?
                });
            }

            _cache.Set(cacheKey, categories, TimeSpan.FromMinutes(30));
            return categories;
        }

        public ConfigurationCategory? GetCategoryById(int categoryId)
        {
            return GetAllCategories().FirstOrDefault(c => c.CategoryId == categoryId);
        }

        public ConfigurationCategory? GetCategoryByName(string categoryName)
        {
            return GetAllCategories().FirstOrDefault(c => c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddCategory(ConfigurationCategory category)
        {
            try
            {
                const string sql = @"
                    INSERT INTO ConfigurationCategories (CategoryName, DisplayName, Description, IsActive, DisplayOrder)
                    VALUES (@CategoryName, @DisplayName, @Description, @IsActive, @DisplayOrder)";

                var parameters = new[]
                {
                    new SqlParameter("@CategoryName", category.CategoryName),
                    new SqlParameter("@DisplayName", category.DisplayName),
                    new SqlParameter("@Description", category.Description ?? (object)DBNull.Value),
                    new SqlParameter("@IsActive", category.IsActive),
                    new SqlParameter("@DisplayOrder", category.DisplayOrder)
                };

                GetDb().ExecuteNonQuery(sql, parameters);
                ClearCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding configuration category: {CategoryName}", category.CategoryName);
                return false;
            }
        }

        public bool UpdateCategory(ConfigurationCategory category)
        {
            try
            {
                const string sql = @"
                    UPDATE ConfigurationCategories 
                    SET DisplayName = @DisplayName, Description = @Description, 
                        IsActive = @IsActive, DisplayOrder = @DisplayOrder, ModifiedDate = GETDATE()
                    WHERE CategoryId = @CategoryId";

                var parameters = new[]
                {
                    new SqlParameter("@CategoryId", category.CategoryId),
                    new SqlParameter("@DisplayName", category.DisplayName),
                    new SqlParameter("@Description", category.Description ?? (object)DBNull.Value),
                    new SqlParameter("@IsActive", category.IsActive),
                    new SqlParameter("@DisplayOrder", category.DisplayOrder)
                };

                GetDb().ExecuteNonQuery(sql, parameters);
                ClearCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration category: {CategoryId}", category.CategoryId);
                return false;
            }
        }

        public bool DeleteCategory(int categoryId)
        {
            try
            {
                const string sql = "UPDATE ConfigurationCategories SET IsActive = 0, ModifiedDate = GETDATE() WHERE CategoryId = @CategoryId";
                GetDb().ExecuteNonQuery(sql, new SqlParameter("@CategoryId", categoryId));
                ClearCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration category: {CategoryId}", categoryId);
                return false;
            }
        }

        #endregion

        #region Value Operations

        public List<ConfigurationValue> GetValuesByCategory(string categoryName)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}Values_{categoryName}";
            
            if (_cache.TryGetValue(cacheKey, out List<ConfigurationValue> cachedValues))
            {
                return cachedValues;
            }

            const string sql = @"
                SELECT v.ValueId, v.CategoryId, v.ValueKey, v.ValueText, v.DisplayOrder, 
                       v.IsActive, v.CreatedBy, v.CreatedDate, v.ModifiedBy, v.ModifiedDate
                FROM ConfigurationValues v
                INNER JOIN ConfigurationCategories c ON v.CategoryId = c.CategoryId
                WHERE c.CategoryName = @CategoryName 
                AND v.IsActive = 1 
                AND c.IsActive = 1
                ORDER BY v.DisplayOrder, v.ValueText";

            var dt = GetDb().ExecuteQuery(sql, new SqlParameter("@CategoryName", categoryName));
            var values = new List<ConfigurationValue>();

            foreach (DataRow row in dt.Rows)
            {
                values.Add(new ConfigurationValue
                {
                    ValueId = Convert.ToInt32(row["ValueId"]),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    ValueKey = row["ValueKey"].ToString(),
                    ValueText = row["ValueText"].ToString(),
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedBy = row["CreatedBy"]?.ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    ModifiedBy = row["ModifiedBy"]?.ToString(),
                    ModifiedDate = row["ModifiedDate"] as DateTime?
                });
            }

            _cache.Set(cacheKey, values, TimeSpan.FromMinutes(30));
            return values;
        }

        public List<ConfigurationValue> GetValuesByCategoryId(int categoryId)
        {
            const string sql = @"
                SELECT ValueId, CategoryId, ValueKey, ValueText, DisplayOrder, 
                       IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
                FROM ConfigurationValues 
                WHERE CategoryId = @CategoryId AND IsActive = 1
                ORDER BY DisplayOrder, ValueText";

            var dt = GetDb().ExecuteQuery(sql, new SqlParameter("@CategoryId", categoryId));
            var values = new List<ConfigurationValue>();

            foreach (DataRow row in dt.Rows)
            {
                values.Add(new ConfigurationValue
                {
                    ValueId = Convert.ToInt32(row["ValueId"]),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    ValueKey = row["ValueKey"].ToString(),
                    ValueText = row["ValueText"].ToString(),
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedBy = row["CreatedBy"]?.ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    ModifiedBy = row["ModifiedBy"]?.ToString(),
                    ModifiedDate = row["ModifiedDate"] as DateTime?
                });
            }

            return values;
        }

        public ConfigurationValue? GetValueById(int valueId)
        {
            const string sql = @"
                SELECT ValueId, CategoryId, ValueKey, ValueText, DisplayOrder, 
                       IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
                FROM ConfigurationValues 
                WHERE ValueId = @ValueId";

            var dt = GetDb().ExecuteQuery(sql, new SqlParameter("@ValueId", valueId));
            
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new ConfigurationValue
            {
                ValueId = Convert.ToInt32(row["ValueId"]),
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                ValueKey = row["ValueKey"].ToString(),
                ValueText = row["ValueText"].ToString(),
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedBy = row["CreatedBy"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                ModifiedBy = row["ModifiedBy"]?.ToString(),
                ModifiedDate = row["ModifiedDate"] as DateTime?
            };
        }

        public bool AddValue(ConfigurationValue value)
        {
            try
            {
                _logger.LogInformation("AddValue called with: CategoryId={CategoryId}, ValueKey={ValueKey}, IsActive={IsActive}", 
                    value.CategoryId, value.ValueKey, value.IsActive);
                
                var db = GetDb();
                
                // 1. إضافة القيمة في ConfigurationValues
                const string configSql = @"
                    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder, IsActive, CreatedBy)
                    VALUES (@CategoryId, @ValueKey, @ValueText, @DisplayOrder, @IsActive, @CreatedBy)";

                var configParams = new[]
                {
                    new SqlParameter("@CategoryId", value.CategoryId),
                    new SqlParameter("@ValueKey", value.ValueKey),
                    new SqlParameter("@ValueText", value.ValueText),
                    new SqlParameter("@DisplayOrder", value.DisplayOrder),
                    new SqlParameter("@IsActive", value.IsActive),
                    new SqlParameter("@CreatedBy", value.CreatedBy ?? (object)DBNull.Value)
                };

                _logger.LogInformation("Executing SQL with parameters: CategoryId={CategoryId}, ValueKey={ValueKey}, IsActive={IsActive}", 
                    value.CategoryId, value.ValueKey, value.IsActive);

                var configResult = db.ExecuteNonQuery(configSql, configParams);
                _logger.LogInformation("SQL executed successfully");
                
                // 2. إذا كانت الفئة هي "Roles"، أضف أيضاً في جدول Roles
                var category = GetCategoryById(value.CategoryId);
                if (category != null && category.CategoryName.Equals("Roles", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var rolesSql = @"INSERT INTO Roles (RoleName, Description) VALUES (@RoleName, @Description)";
                        var rolesParams = new[]
                        {
                            new SqlParameter("@RoleName", value.ValueKey),
                            new SqlParameter("@Description", value.ValueText)
                        };
                        
                        var rolesResult = db.ExecuteNonQuery(rolesSql, rolesParams);
                        _logger.LogInformation("Role added to both ConfigurationValues and Roles tables: {RoleName}", value.ValueKey);
                    }
                    catch (Exception rolesEx)
                    {
                        _logger.LogWarning(rolesEx, "Failed to add role to Roles table, but added to ConfigurationValues: {RoleName}", value.ValueKey);
                    }
                }
                
                // 3. إذا كانت الفئة هي "CertificateTypes"، أضف أيضاً في جدول DocumentTypes
                if (category != null && category.CategoryName.Equals("CertificateTypes", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var documentTypesSql = @"INSERT INTO DocumentTypes (TypeName) VALUES (@TypeName)";
                        var documentTypesParams = new[]
                        {
                            new SqlParameter("@TypeName", value.ValueText)
                        };
                        var documentTypesResult = db.ExecuteNonQuery(documentTypesSql, documentTypesParams);
                        _logger.LogInformation("Certificate type added to both ConfigurationValues and DocumentTypes tables: {TypeName}", value.ValueText);
                    }
                    catch (Exception documentTypesEx)
                    {
                        _logger.LogWarning(documentTypesEx, "Failed to add certificate type to DocumentTypes table, but added to ConfigurationValues: {TypeName}", value.ValueText);
                    }
                }
                
                ClearCache();
                return configResult > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding configuration value: {ValueKey}", value.ValueKey);
                return false;
            }
        }

        public bool UpdateValue(ConfigurationValue value)
        {
            try
            {
                var db = GetDb();
                
                // 1. Get the value details before updating to check if it's a role
                var valueQuery = "SELECT CategoryId, ValueKey FROM ConfigurationValues WHERE ValueId = @ValueId";
                var valueData = db.ExecuteQuery(valueQuery, new SqlParameter("@ValueId", value.ValueId));
                
                if (valueData.Rows.Count == 0)
                {
                    _logger.LogWarning("Configuration value not found for update: {ValueId}", value.ValueId);
                    return false;
                }
                
                var row = valueData.Rows[0];
                var categoryId = row["CategoryId"];
                var oldValueKey = row["ValueKey"]?.ToString();
                
                // 2. Get category name to check if it's "Roles"
                var categoryQuery = "SELECT CategoryName FROM ConfigurationCategories WHERE CategoryId = @CategoryId";
                var categoryData = db.ExecuteQuery(categoryQuery, new SqlParameter("@CategoryId", categoryId));
                var categoryName = categoryData.Rows.Count > 0 ? categoryData.Rows[0]["CategoryName"]?.ToString() : "";
                
                // 3. Update ConfigurationValues
                const string configSql = @"
                    UPDATE ConfigurationValues 
                    SET ValueText = @ValueText, DisplayOrder = @DisplayOrder, 
                        IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                    WHERE ValueId = @ValueId";

                var parameters = new[]
                {
                    new SqlParameter("@ValueId", value.ValueId),
                    new SqlParameter("@ValueText", value.ValueText),
                    new SqlParameter("@DisplayOrder", value.DisplayOrder),
                    new SqlParameter("@IsActive", value.IsActive),
                    new SqlParameter("@ModifiedBy", value.ModifiedBy ?? (object)DBNull.Value)
                };

                var configResult = db.ExecuteNonQuery(configSql, parameters);
                
                // 4. If it's a role, also update Roles table
                if (categoryName != null && categoryName.Equals("Roles", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(oldValueKey))
                {
                    try
                    {
                        var rolesSql = @"
                            UPDATE Roles 
                            SET RoleName = @NewRoleName, Description = @NewDescription 
                            WHERE RoleName = @OldRoleName";
                        var rolesParams = new[]
                        {
                            new SqlParameter("@NewRoleName", value.ValueKey),
                            new SqlParameter("@NewDescription", value.ValueText),
                            new SqlParameter("@OldRoleName", oldValueKey)
                        };
                        var rolesResult = db.ExecuteNonQuery(rolesSql, rolesParams);
                        _logger.LogInformation("Role updated in both ConfigurationValues and Roles tables: {OldRoleName} -> {NewRoleName}", oldValueKey, value.ValueKey);
                    }
                    catch (Exception rolesEx)
                    {
                        _logger.LogWarning(rolesEx, "Failed to update role in Roles table, but updated in ConfigurationValues: {RoleName}", oldValueKey);
                    }
                }
                
                // 5. If it's a certificate type, also update DocumentTypes table
                if (categoryName != null && categoryName.Equals("CertificateTypes", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(oldValueKey))
                {
                    try
                    {
                        var documentTypesSql = @"
                            UPDATE DocumentTypes 
                            SET TypeName = @NewTypeName 
                            WHERE TypeName = @OldTypeName";
                        var documentTypesParams = new[]
                        {
                            new SqlParameter("@NewTypeName", value.ValueText),
                            new SqlParameter("@OldTypeName", oldValueKey)
                        };
                        var documentTypesResult = db.ExecuteNonQuery(documentTypesSql, documentTypesParams);
                        _logger.LogInformation("Certificate type updated in both ConfigurationValues and DocumentTypes tables: {OldTypeName} -> {NewTypeName}", oldValueKey, value.ValueText);
                    }
                    catch (Exception documentTypesEx)
                    {
                        _logger.LogWarning(documentTypesEx, "Failed to update certificate type in DocumentTypes table, but updated in ConfigurationValues: {TypeName}", oldValueKey);
                    }
                }
                
                ClearCache();
                return configResult > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration value: {ValueId}", value.ValueId);
                return false;
            }
        }

        public bool DeleteValue(int valueId)
        {
            try
            {
                var db = GetDb();
                
                // 1. Get the value details before deleting to check if it's a role
                var valueQuery = "SELECT CategoryId, ValueKey FROM ConfigurationValues WHERE ValueId = @ValueId";
                var valueData = db.ExecuteQuery(valueQuery, new SqlParameter("@ValueId", valueId));
                
                if (valueData.Rows.Count == 0)
                {
                    _logger.LogWarning("Configuration value not found for deletion: {ValueId}", valueId);
                    return false;
                }
                
                var row = valueData.Rows[0];
                var categoryId = row["CategoryId"];
                var valueKey = row["ValueKey"]?.ToString();
                
                // 2. Get category name to check if it's "Roles"
                var categoryQuery = "SELECT CategoryName FROM ConfigurationCategories WHERE CategoryId = @CategoryId";
                var categoryData = db.ExecuteQuery(categoryQuery, new SqlParameter("@CategoryId", categoryId));
                var categoryName = categoryData.Rows.Count > 0 ? categoryData.Rows[0]["CategoryName"]?.ToString() : "";
                
                // 3. Delete from ConfigurationValues (soft delete)
                const string configSql = "UPDATE ConfigurationValues SET IsActive = 0, ModifiedDate = GETDATE() WHERE ValueId = @ValueId";
                var configResult = db.ExecuteNonQuery(configSql, new SqlParameter("@ValueId", valueId));
                
                // 4. If it's a role, also delete from Roles table
                if (categoryName != null && categoryName.Equals("Roles", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(valueKey))
                {
                    try
                    {
                        var rolesSql = "DELETE FROM Roles WHERE RoleName = @RoleName";
                        var rolesResult = db.ExecuteNonQuery(rolesSql, new SqlParameter("@RoleName", valueKey));
                        _logger.LogInformation("Role deleted from both ConfigurationValues and Roles tables: {RoleName}", valueKey);
                    }
                    catch (Exception rolesEx)
                    {
                        _logger.LogWarning(rolesEx, "Failed to delete role from Roles table, but deleted from ConfigurationValues: {RoleName}", valueKey);
                    }
                }
                
                // 5. If it's a certificate type, also delete from DocumentTypes table
                if (categoryName != null && categoryName.Equals("CertificateTypes", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(valueKey))
                {
                    try
                    {
                        var documentTypesSql = "DELETE FROM DocumentTypes WHERE TypeName = @TypeName";
                        var documentTypesResult = db.ExecuteNonQuery(documentTypesSql, new SqlParameter("@TypeName", valueKey));
                        _logger.LogInformation("Certificate type deleted from both ConfigurationValues and DocumentTypes tables: {TypeName}", valueKey);
                    }
                    catch (Exception documentTypesEx)
                    {
                        _logger.LogWarning(documentTypesEx, "Failed to delete certificate type from DocumentTypes table, but deleted from ConfigurationValues: {TypeName}", valueKey);
                    }
                }
                
                ClearCache();
                return configResult > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration value: {ValueId}", valueId);
                return false;
            }
        }

        #endregion

        #region Dropdown Operations

        public List<SelectListItem> GetDropdownValues(string categoryName)
        {
            var values = GetValuesByCategory(categoryName);
            return values.Select(v => new SelectListItem
            {
                Value = v.ValueText,  // Use ValueText as Value instead of ValueKey
                Text = v.ValueText
            }).ToList();
        }

        public List<ConfigurationSelectListItem> GetConfigurationSelectList(string categoryName)
        {
            var values = GetValuesByCategory(categoryName);
            return values.Select(v => new ConfigurationSelectListItem
            {
                Value = v.ValueKey,
                Text = v.ValueText,
                DisplayOrder = v.DisplayOrder,
                IsActive = v.IsActive
            }).ToList();
        }

        #endregion

        #region Log Operations

        public List<ConfigurationLog> GetLogs(int? valueId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var sql = @"
                SELECT LogId, ValueId, Action, OldValue, NewValue, ChangedBy, ChangedDate
                FROM ConfigurationLog
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (valueId.HasValue)
            {
                sql += " AND ValueId = @ValueId";
                parameters.Add(new SqlParameter("@ValueId", valueId.Value));
            }

            if (fromDate.HasValue)
            {
                sql += " AND ChangedDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                sql += " AND ChangedDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value));
            }

            sql += " ORDER BY ChangedDate DESC";

            var dt = GetDb().ExecuteQuery(sql, parameters.ToArray());
            var logs = new List<ConfigurationLog>();

            foreach (DataRow row in dt.Rows)
            {
                logs.Add(new ConfigurationLog
                {
                    LogId = Convert.ToInt32(row["LogId"]),
                    ValueId = row["ValueId"] as int?,
                    Action = row["Action"].ToString(),
                    OldValue = row["OldValue"]?.ToString(),
                    NewValue = row["NewValue"]?.ToString(),
                    ChangedBy = row["ChangedBy"]?.ToString(),
                    ChangedDate = Convert.ToDateTime(row["ChangedDate"])
                });
            }

            return logs;
        }

        public bool AddLog(ConfigurationLog log)
        {
            try
            {
                const string sql = @"
                    INSERT INTO ConfigurationLog (ValueId, Action, OldValue, NewValue, ChangedBy)
                    VALUES (@ValueId, @Action, @OldValue, @NewValue, @ChangedBy)";

                var parameters = new[]
                {
                    new SqlParameter("@ValueId", log.ValueId ?? (object)DBNull.Value),
                    new SqlParameter("@Action", log.Action),
                    new SqlParameter("@OldValue", log.OldValue ?? (object)DBNull.Value),
                    new SqlParameter("@NewValue", log.NewValue ?? (object)DBNull.Value),
                    new SqlParameter("@ChangedBy", log.ChangedBy ?? (object)DBNull.Value)
                };

                GetDb().ExecuteNonQuery(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding configuration log");
                return false;
            }
        }

        #endregion

        #region Statistics

        public ConfigurationStatisticsViewModel GetStatistics()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM ConfigurationCategories WHERE IsActive = 1) as TotalCategories,
                    (SELECT COUNT(*) FROM ConfigurationValues WHERE IsActive = 1) as TotalValues,
                    (SELECT COUNT(*) FROM ConfigurationCategories WHERE IsActive = 1) as ActiveCategories,
                    (SELECT COUNT(*) FROM ConfigurationValues WHERE IsActive = 1) as ActiveValues,
                    (SELECT COUNT(*) FROM ConfigurationLog WHERE ChangedDate >= DATEADD(day, -7, GETDATE())) as RecentChanges";

            var dt = GetDb().ExecuteQuery(sql);
            var row = dt.Rows[0];

            var statistics = new ConfigurationStatisticsViewModel
            {
                TotalCategories = Convert.ToInt32(row["TotalCategories"]),
                TotalValues = Convert.ToInt32(row["TotalValues"]),
                ActiveCategories = Convert.ToInt32(row["ActiveCategories"]),
                ActiveValues = Convert.ToInt32(row["ActiveValues"]),
                RecentChanges = Convert.ToInt32(row["RecentChanges"]),
                RecentLogs = GetLogs(fromDate: DateTime.Now.AddDays(-7))
            };

            // Get values per category
            var categories = GetAllCategories();
            foreach (var category in categories)
            {
                var values = GetValuesByCategoryId(category.CategoryId);
                statistics.ValuesPerCategory[category.DisplayName] = values.Count;
            }

            return statistics;
        }

        #endregion

        #region Cache Operations

        public void ClearCache()
        {
            var cacheKeys = new List<string>
            {
                $"{CACHE_KEY_PREFIX}Categories",
                $"{CACHE_KEY_PREFIX}Values_JobTitles",
                $"{CACHE_KEY_PREFIX}Values_Departments",
                $"{CACHE_KEY_PREFIX}Values_Roles",
                $"{CACHE_KEY_PREFIX}Values_LicenseTypes",
                $"{CACHE_KEY_PREFIX}Values_EmploymentStatus",
                $"{CACHE_KEY_PREFIX}Values_EducationLevels",
                $"{CACHE_KEY_PREFIX}Values_MaritalStatus",
                $"{CACHE_KEY_PREFIX}Values_Gender",
                $"{CACHE_KEY_PREFIX}Values_ProjectStatuses",
                $"{CACHE_KEY_PREFIX}Values_ProjectTypes",
                $"{CACHE_KEY_PREFIX}Values_CertificateTypes",
                $"{CACHE_KEY_PREFIX}Values_IssuingAuthorities"
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }
        }

        public void RefreshCache()
        {
            ClearCache();
            // Force reload by calling methods that use cache
            GetAllCategories();
            GetValuesByCategory("JobTitles");
            GetValuesByCategory("Departments");
            GetValuesByCategory("Roles");
        }

        #endregion
    }
} 

