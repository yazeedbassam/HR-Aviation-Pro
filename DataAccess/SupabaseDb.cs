using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.DataAccess
{
    public class SupabaseDb
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;
        private readonly ILogger<SupabaseDb> _logger;
        private readonly IPasswordHasher<ControllerUser> _passwordHasher;

        public SupabaseDb(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgreSQLConnection") 
                                ?? throw new ArgumentNullException("Connection string 'PostgreSQLConnection' not found.");
            
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            
            var connectionStringForLogging = _connectionString;
            if (!string.IsNullOrEmpty(connectionStringForLogging))
            {
                var parts = connectionStringForLogging.Split(';');
                var safeParts = parts.Where(p => !p.ToLower().Contains("password") && !p.ToLower().Contains("pwd"));
                var safeConnectionString = string.Join(";", safeParts);
                Console.WriteLine($"PostgreSQL Database connection configured: {safeConnectionString}");
            }
        }

        public SupabaseDb(IConfiguration configuration, IPasswordHasher<ControllerUser> passwordHasher, ILogger<SupabaseDb> logger)
        {
            var connectionString = configuration.GetConnectionString("PostgreSQLConnection")
                                ?? throw new ArgumentNullException("Connection string 'PostgreSQLConnection' not found.");
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        private string ReplaceEnvironmentVariables(string connectionString)
        {
            return connectionString
                .Replace("${PGHOST}", Environment.GetEnvironmentVariable("PGHOST") ?? "localhost")
                .Replace("${PGDATABASE}", Environment.GetEnvironmentVariable("PGDATABASE") ?? "railway")
                .Replace("${PGUSER}", Environment.GetEnvironmentVariable("PGUSER") ?? "postgres")
                .Replace("${PGPASSWORD}", Environment.GetEnvironmentVariable("PGPASSWORD") ?? "")
                .Replace("${PGPORT}", Environment.GetEnvironmentVariable("PGPORT") ?? "5432");
        }

        public NpgsqlConnection GetConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(_connectionString);
                Console.WriteLine("PostgreSQL connection created successfully");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PostgreSQL connection: {ex.Message}");
                throw;
            }
        }

        public bool IsDatabaseAvailable()
        {
            try
            {
                Console.WriteLine("🔍 Testing Supabase database connection...");
                Console.WriteLine($"🔍 Connection string: {_connectionString.Replace("Password=Y@Z105213eed", "Password=***")}");
                
                using var connection = GetConnection();
                Console.WriteLine("🔍 Connection created, attempting to open...");
                connection.Open();
                Console.WriteLine("✅ Supabase Database connection opened successfully");
                
                // Test if Users table exists
                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
                cmd.CommandTimeout = 30;
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"✅ Users table exists with {count} records");
                
                connection.Close();
                Console.WriteLine("✅ Supabase Database connection closed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Supabase Database availability check failed: {ex.Message}");
                Console.WriteLine($"❌ Exception type: {ex.GetType().Name}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                // Additional debugging for specific error types
                if (ex is NpgsqlException npgsqlEx)
                {
                    Console.WriteLine($"❌ PostgreSQL Error Code: {npgsqlEx.SqlState}");
                    Console.WriteLine($"❌ PostgreSQL Error Detail: {npgsqlEx.Data}");
                }
                
                return false;
            }
        }

        public DataTable ExecuteQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            
            using var adapter = new NpgsqlDataAdapter(cmd);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            
            return dataTable;
        }

        public int ExecuteNonQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.CommandTimeout = 30; // Add timeout
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            
            return cmd.ExecuteScalar();
        }

        public bool ValidateCredentials(string username, string password, out int userId, out string role)
        {
            userId = 0;
            role = string.Empty;

            try
            {
                Console.WriteLine($"🔍 Validating credentials for user: {username}");
                
                var sql = @"
                    SELECT id, ""PasswordHash"", ""RoleName"" 
                    FROM ""Users"" 
                    WHERE ""Username"" = @username";

                var parameters = new[]
                {
                    new NpgsqlParameter("@username", username)
                };

                var result = ExecuteQuery(sql, parameters);
                Console.WriteLine($"🔍 Query returned {result.Rows.Count} rows");

                if (result.Rows.Count > 0)
                {
                    var row = result.Rows[0];
                    var storedPassword = row["PasswordHash"].ToString();
                    Console.WriteLine($"🔍 Found user, stored password hash: {storedPassword.Substring(0, Math.Min(20, storedPassword.Length))}...");
                    
                    // Verify password using ASP.NET Core Identity Password Hasher
                    if (_passwordHasher != null)
                    {
                        var verificationResult = _passwordHasher.VerifyHashedPassword(null, storedPassword, password);
                        if (verificationResult == PasswordVerificationResult.Success)
                        {
                            userId = Convert.ToInt32(row["id"]);
                            role = row["RoleName"].ToString();
                            Console.WriteLine($"✅ Password verified successfully. UserId: {userId}, Role: {role}");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("❌ Password verification failed");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ Password hasher not available");
                    }
                }
                else
                {
                    Console.WriteLine("❌ No user found with this username");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error validating credentials: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<DataTable> GetUsersAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        id as userid,
                        ""Username"" as username,
                        ""Username"" as fullname,
                        ""Username"" as email,
                        ""RoleName"" as role,
                        'IT' as department,
                        true as isactive,
                        CURRENT_TIMESTAMP as created_at,
                        NULL as last_login
                    FROM ""Users"" 
                    ORDER BY ""Username""";

                return await Task.Run(() => ExecuteQuery(sql));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateUserAsync(ControllerUser user, string password)
        {
            try
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                
                var sql = @"
                    INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""RoleName"")
                    VALUES (@username, @password, @role)";

                var parameters = new[]
                {
                    new NpgsqlParameter("@username", user.Username),
                    new NpgsqlParameter("@password", hashedPassword),
                    new NpgsqlParameter("@role", user.Role)
                };

                var result = await Task.Run(() => ExecuteNonQuery(sql, parameters));
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(ControllerUser user)
        {
            try
            {
                var sql = @"
                    UPDATE ""Users"" 
                    SET ""RoleName"" = @role
                    WHERE id = @userid";

                var parameters = new[]
                {
                    new NpgsqlParameter("@userid", user.UserId),
                    new NpgsqlParameter("@role", user.Role)
                };

                var result = await Task.Run(() => ExecuteNonQuery(sql, parameters));
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var sql = "DELETE FROM \"Users\" WHERE id = @userid";
                var parameters = new[] { new NpgsqlParameter("@userid", userId) };

                var result = await Task.Run(() => ExecuteNonQuery(sql, parameters));
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        public async Task<DataTable> GetEmployeesAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ""EmployeeId"" as employeeid,
                        ""FirstName"" || ' ' || ""LastName"" as fullname,
                        ""Position"" as position,
                        ""Department"" as department,
                        ""HireDate"" as hire_date,
                        ""Email"" as email,
                        ""Phone"" as phone,
                        ""IsActive"" as isactive
                    FROM ""Employees"" 
                    ORDER BY ""FirstName""";

                return await Task.Run(() => ExecuteQuery(sql));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employees: {ex.Message}");
                throw;
            }
        }

        public async Task<DataTable> GetCertificatesAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ""CertificateId"" as certificateid,
                        ""EmployeeId"" as employeeid,
                        ""CertificateType"" as certificatetype,
                        ""IssueDate"" as issuedate,
                        ""ExpiryDate"" as expirydate,
                        ""Status"" as status,
                        ""Notes"" as notes
                    FROM ""Certificates"" 
                    ORDER BY ""IssueDate"" DESC";

                return await Task.Run(() => ExecuteQuery(sql));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting certificates: {ex.Message}");
                throw;
            }
        }

        public async Task<DataTable> GetObservationsAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ""ObservationId"" as observationid,
                        ""EmployeeId"" as employeeid,
                        ""ObservationType"" as observationtype,
                        ""ObservationDate"" as observationdate,
                        ""Description"" as description,
                        ""Status"" as status,
                        ""CreatedDate"" as created_at
                    FROM ""Observations"" 
                    ORDER BY ""ObservationDate"" DESC";

                return await Task.Run(() => ExecuteQuery(sql));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting observations: {ex.Message}");
                throw;
            }
        }

        public async Task<DataTable> GetProjectsAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ""ProjectId"" as projectid,
                        ""ProjectName"" as projectname,
                        ""Description"" as description,
                        ""StartDate"" as startdate,
                        ""EndDate"" as enddate,
                        ""Status"" as status,
                        ""ManagerId"" as managerid,
                        ""CreatedDate"" as created_at
                    FROM ""Projects"" 
                    ORDER BY ""StartDate"" DESC";

                return await Task.Run(() => ExecuteQuery(sql));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting projects: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> LogUserActivityAsync(int userId, string username, string action, string details, string ipAddress, string userAgent)
        {
            try
            {
                var sql = @"
                    INSERT INTO ""UserActivityLogs"" (""UserId"", ""Action"", ""Details"", ""IpAddress"", ""UserAgent"", ""CreatedDate"")
                    VALUES (@userid, @action, @details, @ipaddress, @useragent, @created_at)";

                var parameters = new[]
                {
                    new NpgsqlParameter("@userid", userId),
                    new NpgsqlParameter("@action", action),
                    new NpgsqlParameter("@details", details),
                    new NpgsqlParameter("@ipaddress", ipAddress),
                    new NpgsqlParameter("@useragent", userAgent),
                    new NpgsqlParameter("@created_at", DateTime.Now)
                };

                var result = await Task.Run(() => ExecuteNonQuery(sql, parameters));
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging user activity: {ex.Message}");
                return false;
            }
        }

        public async Task<DataTable> GetNotificationsAsync(int userId = 0)
        {
            try
            {
                string sql;
                var parameters = new List<NpgsqlParameter>();

                if (userId > 0)
                {
                    sql = @"
                        SELECT * FROM ""Notifications"" 
                        WHERE ""UserId"" = @userid 
                        ORDER BY ""CreatedDate"" DESC";
                    parameters.Add(new NpgsqlParameter("@userid", userId));
                }
                else
                {
                    sql = @"
                        SELECT * FROM ""Notifications"" 
                        ORDER BY ""CreatedDate"" DESC";
                }

                return await Task.Run(() => ExecuteQuery(sql, parameters.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notifications: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var sql = "UPDATE \"Notifications\" SET \"IsRead\" = true WHERE \"NotificationId\" = @notificationid";
                var parameters = new[] { new NpgsqlParameter("@notificationid", notificationId) };

                var result = await Task.Run(() => ExecuteNonQuery(sql, parameters));
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// إنشاء مستخدم admin إذا لم يكن موجوداً
        /// </summary>
        public async Task<bool> EnsureAdminUserExistsAsync()
        {
            try
            {
                Console.WriteLine("🔧 Checking if admin user exists in Supabase...");
                
                // التحقق من وجود مستخدم admin
                var checkSql = "SELECT COUNT(*) FROM \"Users\" WHERE \"Username\" = @username";
                var checkParams = new[] { new NpgsqlParameter("@username", "admin") };
                
                var count = await Task.Run(() => ExecuteScalar(checkSql, checkParams));
                var userExists = Convert.ToInt32(count) > 0;

                if (!userExists)
                {
                    Console.WriteLine("🔧 Admin user not found in Supabase, creating...");
                    
                    // إنشاء مستخدم admin - استخدام نفس hash الموجود في الجدول
                    var hashedPassword = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi";
                    Console.WriteLine($"🔧 Using existing password hash: {hashedPassword.Substring(0, Math.Min(20, hashedPassword.Length))}...");
                    
                    var insertSql = @"
                        INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""RoleName"")
                        VALUES (@username, @password, @role)";
                    
                    var insertParams = new[]
                    {
                        new NpgsqlParameter("@username", "admin"),
                        new NpgsqlParameter("@password", hashedPassword),
                        new NpgsqlParameter("@role", "Admin")
                    };

                    var result = await Task.Run(() => ExecuteNonQuery(insertSql, insertParams));
                    
                    if (result > 0)
                    {
                        Console.WriteLine("✅ Admin user created successfully in Supabase");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("❌ Failed to create admin user in Supabase");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("✅ Admin user already exists in Supabase");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ensuring admin user exists: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
} 
