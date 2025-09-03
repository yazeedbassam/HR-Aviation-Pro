using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Data.Common;

namespace WebApplication1.Services
{
    /// <summary>
    /// خدمة قاعدة البيانات الذكية
    /// تختار قاعدة البيانات المناسبة تلقائياً
    /// </summary>
    public class SmartDatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SmartDatabaseService> _logger;
        private string _currentDatabase;

        public SmartDatabaseService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<SmartDatabaseService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            
            // تحديد قاعدة البيانات الافتراضية
            _currentDatabase = _configuration["DatabaseSettings:DefaultDatabase"] ?? "SqlServer";
            
            _logger.LogInformation("SmartDatabaseService initialized with default database: {DatabaseType}", _currentDatabase);
        }

        /// <summary>
        /// الحصول على نوع قاعدة البيانات الحالية
        /// </summary>
        public string GetCurrentDatabase()
        {
            return _currentDatabase;
        }

        /// <summary>
        /// الحصول على اتصال قاعدة البيانات المناسبة
        /// </summary>
        public IDbConnection GetConnection()
        {
            try
            {
                IDbConnection connection;

                switch (_currentDatabase.ToLower())
                {
                    case "local":
                        var sqlServerConnectionString = _configuration.GetConnectionString("SqlServerDbConnection");
                        if (string.IsNullOrEmpty(sqlServerConnectionString))
                        {
                            throw new InvalidOperationException("SQL Server connection string is not configured");
                        }
                        connection = new SqlConnection(sqlServerConnectionString);
                        _logger.LogDebug("Created SQL Server connection");
                        break;

                    case "supabase":
                        var supabaseConnectionString = _configuration.GetConnectionString("SupabaseConnection");
                        if (string.IsNullOrEmpty(supabaseConnectionString))
                        {
                            throw new InvalidOperationException("Supabase connection string is not configured");
                        }
                        connection = new NpgsqlConnection(supabaseConnectionString);
                        _logger.LogDebug("Created Supabase connection");
                        break;

                    case "mysql":
                        var mysqlConnectionString = _configuration.GetConnectionString("MySqlDbConnection");
                        if (string.IsNullOrEmpty(mysqlConnectionString))
                        {
                            throw new InvalidOperationException("MySQL connection string is not configured");
                        }
                        connection = new MySql.Data.MySqlClient.MySqlConnection(mysqlConnectionString);
                        _logger.LogDebug("Created MySQL connection");
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported database type: {_currentDatabase}");
                }

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database connection for type: {DatabaseType}", _currentDatabase);
                throw;
            }
        }

        /// <summary>
        /// التحقق من توفر قاعدة البيانات
        /// </summary>
        public async Task<bool> IsDatabaseAvailableAsync()
        {
            return await IsDatabaseAvailableAsync(_currentDatabase);
        }

        /// <summary>
        /// التحقق من توفر قاعدة البيانات بناءً على النوع
        /// </summary>
        public async Task<bool> IsDatabaseAvailableAsync(string databaseType)
        {
            try
            {
                if (databaseType == "local")
                {
                    // اختبار SQL Server المحلي
                    using var connection = new SqlConnection(_configuration.GetConnectionString("SqlServerDbConnection"));
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT 1";
                    command.CommandType = CommandType.Text;
                    
                    command.ExecuteScalar();
                    
                    _logger.LogDebug("Local SQL Server connection test successful");
                    return true;
                }
                else if (databaseType == "supabase")
                {
                    // اختبار Supabase
                    var connectionString = _configuration.GetConnectionString("SupabaseConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        _logger.LogWarning("Supabase connection string is not configured");
                        return false;
                    }
                    
                    try
                    {
                        using var connection = new NpgsqlConnection(connectionString);
                        connection.Open();
                        
                        using var command = connection.CreateCommand();
                        command.CommandText = "SELECT 1";
                        command.CommandType = CommandType.Text;
                        
                        command.ExecuteScalar();
                        
                        _logger.LogDebug("Supabase connection test successful");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Supabase connection test failed");
                        return false;
                    }
                }
                else if (databaseType == "mysql")
                {
                    // اختبار MySQL
                    var connectionString = _configuration.GetConnectionString("MySqlDbConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        _logger.LogWarning("MySQL connection string is not configured");
                        return false;
                    }
                    
                    try
                    {
                        using var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                        connection.Open();
                        
                        using var command = connection.CreateCommand();
                        command.CommandText = "SELECT 1";
                        command.CommandType = CommandType.Text;
                        
                        command.ExecuteScalar();
                        
                        _logger.LogDebug("MySQL connection test successful");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "MySQL connection test failed");
                        return false;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database connection test failed for: {DatabaseType}", databaseType);
                return false;
            }
        }

        /// <summary>
        /// الحصول على معلومات قاعدة البيانات
        /// </summary>
        public async Task<DatabaseInfo> GetDatabaseInfoAsync()
        {
            var info = new DatabaseInfo
            {
                DatabaseType = _currentDatabase,
                LastChecked = DateTime.Now,
                IsAvailable = await IsDatabaseAvailableAsync()
            };

            try
            {
                using var connection = GetConnection();
                connection.Open();
                
                using var command = connection.CreateCommand();
                command.CommandText = GetVersionQuery();
                command.CommandType = CommandType.Text;
                
                var version = command.ExecuteScalar();
                info.Version = version?.ToString() ?? "Unknown";
                info.Status = "Connected";
                info.ConnectionString = GetMaskedConnectionString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info for: {DatabaseType}", _currentDatabase);
                info.Status = $"Error: {ex.Message}";
                info.IsAvailable = false;
            }

            return info;
        }

        /// <summary>
        /// تبديل قاعدة البيانات يدوياً
        /// </summary>
        public void SwitchDatabase(string databaseType)
        {
            var validTypes = new[] { "local", "demo" };
            
            if (!validTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid database type. Must be one of: {string.Join(", ", validTypes)}");
            }

            var oldDatabase = _currentDatabase;
            _currentDatabase = databaseType;
            
            _logger.LogInformation("Database switched from {OldDatabase} to {NewDatabase}", oldDatabase, _currentDatabase);
        }

        /// <summary>
        /// الحصول على استعلام الاختبار المناسب
        /// </summary>
        private string GetTestQuery()
        {
            return _currentDatabase.ToLower() switch
            {
                "local" => "SELECT 1",
                "demo" => "SELECT 1",
                _ => "SELECT 1"
            };
        }

        /// <summary>
        /// الحصول على استعلام الإصدار المناسب
        /// </summary>
        private string GetVersionQuery()
        {
            return _currentDatabase.ToLower() switch
            {
                "local" => "SELECT @@VERSION",
                "demo" => "SELECT version()",
                _ => "SELECT 1"
            };
        }

        /// <summary>
        /// الحصول على سلسلة الاتصال المخفية (بدون كلمات مرور)
        /// </summary>
        private string GetMaskedConnectionString()
        {
            var connectionString = _configuration.GetConnectionString($"{_currentDatabase}Connection");
            if (string.IsNullOrEmpty(connectionString))
                return "Not configured";

            // إخفاء كلمة المرور
            if (connectionString.Contains("Password="))
            {
                var parts = connectionString.Split(';');
                var maskedParts = parts.Select(p => 
                    p.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) 
                        ? "Password=***" 
                        : p);
                return string.Join(";", maskedParts);
            }

            return connectionString;
        }

        /// <summary>
        /// تبديل تلقائي لقاعدة البيانات عند الفشل
        /// </summary>
        public async Task<bool> TryAutoSwitchOnFailureAsync()
        {
            if (!_configuration.GetValue<bool>("DatabaseSettings:AutoSwitch", false))
                return false;

            var currentType = _currentDatabase;
            var otherType = currentType.Equals("local", StringComparison.OrdinalIgnoreCase) ? "demo" : "local";

            _logger.LogInformation("Attempting auto-switch from {CurrentType} to {OtherType}", currentType, otherType);

            // تجربة قاعدة البيانات الأخرى
            var tempDatabase = _currentDatabase;
            _currentDatabase = otherType;

            if (await IsDatabaseAvailableAsync())
            {
                _logger.LogInformation("Auto-switch successful to {DatabaseType}", _currentDatabase);
                return true;
            }

            // إعادة قاعدة البيانات الأصلية
            _currentDatabase = tempDatabase;
            _logger.LogWarning("Auto-switch failed, reverting to {DatabaseType}", _currentDatabase);
            return false;
        }
    }
} 