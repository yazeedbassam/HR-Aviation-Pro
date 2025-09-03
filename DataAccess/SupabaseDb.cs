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
            var connectionString = configuration.GetConnectionString("SupabaseConnection") 
                                ?? throw new ArgumentNullException("Connection string 'SupabaseConnection' not found.");
            
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            
            var connectionStringForLogging = _connectionString;
            if (!string.IsNullOrEmpty(connectionStringForLogging))
            {
                var parts = connectionStringForLogging.Split(';');
                var safeParts = parts.Where(p => !p.ToLower().Contains("password") && !p.ToLower().Contains("pwd"));
                var safeConnectionString = string.Join(";", safeParts);
                Console.WriteLine($"Supabase Database connection configured: {safeConnectionString}");
            }
        }

        public SupabaseDb(IConfiguration configuration, IPasswordHasher<ControllerUser> passwordHasher, ILogger<SupabaseDb> logger)
        {
            var connectionString = configuration.GetConnectionString("SupabaseConnection")
                                ?? throw new ArgumentNullException("Connection string 'SupabaseConnection' not found.");
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        private string ReplaceEnvironmentVariables(string connectionString)
        {
            return connectionString
                .Replace("${SUPABASE_HOST}", Environment.GetEnvironmentVariable("SUPABASE_HOST") ?? "localhost")
                .Replace("${SUPABASE_DB}", Environment.GetEnvironmentVariable("SUPABASE_DB") ?? "postgres")
                .Replace("${SUPABASE_USER}", Environment.GetEnvironmentVariable("SUPABASE_USER") ?? "postgres")
                .Replace("${SUPABASE_PASSWORD}", Environment.GetEnvironmentVariable("SUPABASE_PASSWORD") ?? "")
                .Replace("${SUPABASE_PORT}", Environment.GetEnvironmentVariable("SUPABASE_PORT") ?? "5432");
        }

        public NpgsqlConnection GetConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(_connectionString);
                Console.WriteLine("Supabase connection created successfully");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Supabase connection: {ex.Message}");
                throw;
            }
        }

        public bool IsDatabaseAvailable()
        {
            try
            {
                Console.WriteLine("🔍 Testing Supabase database connection...");
                Console.WriteLine($"🔍 Connection string: {_connectionString}");
                
                using var connection = GetConnection();
                Console.WriteLine("🔍 Connection created, attempting to open...");
                connection.Open();
                Console.WriteLine("✅ Supabase Database connection opened successfully");
                connection.Close();
                Console.WriteLine("✅ Supabase Database connection closed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Supabase Database availability check failed: {ex.Message}");
                Console.WriteLine($"? Exception type: {ex.GetType().Name}");
                Console.WriteLine($"? Stack trace: {ex.StackTrace}");
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
                var sql = @"
                    SELECT userid, password, role 
                    FROM controller_users 
                    WHERE username = @username AND isactive = 1";

                var parameters = new[]
                {
                    new NpgsqlParameter("@username", username)
                };

                var result = ExecuteQuery(sql, parameters);

                if (result.Rows.Count > 0)
                {
                    var row = result.Rows[0];
                    var storedPassword = row["password"].ToString();
                    
                    // Verify password using BCrypt
                    if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                    {
                        userId = Convert.ToInt32(row["userid"]);
                        role = row["role"].ToString();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating credentials: {ex.Message}");
                return false;
            }
        }

        public async Task<DataTable> GetUsersAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        userid,
                        username,
                        fullname,
                        email,
                        role,
                        department,
                        isactive,
                        created_at,
                        last_login
                    FROM controller_users 
                    ORDER BY fullname";

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
                    INSERT INTO controller_users (username, password, fullname, email, role, department, isactive, created_at)
                    VALUES (@username, @password, @fullname, @email, @role, @department, @isactive, @created_at)";

                var parameters = new[]
                {
                    new NpgsqlParameter("@username", user.Username),
                    new NpgsqlParameter("@password", hashedPassword),
                    new NpgsqlParameter("@fullname", user.FullName),
                    new NpgsqlParameter("@email", user.Email),
                    new NpgsqlParameter("@role", user.Role),
                    new NpgsqlParameter("@department", user.CurrentDepartment),
                    new NpgsqlParameter("@isactive", user.IsActive),
                    new NpgsqlParameter("@created_at", DateTime.Now)
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
                    UPDATE controller_users 
                    SET fullname = @fullname, email = @email, role = @role, department = @department, isactive = @isactive
                    WHERE userid = @userid";

                var parameters = new[]
                {
                    new NpgsqlParameter("@userid", user.UserId),
                    new NpgsqlParameter("@fullname", user.FullName),
                    new NpgsqlParameter("@email", user.Email),
                    new NpgsqlParameter("@role", user.Role),
                    new NpgsqlParameter("@department", user.CurrentDepartment),
                    new NpgsqlParameter("@isactive", user.IsActive)
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
                var sql = "DELETE FROM controller_users WHERE userid = @userid";
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
                        employeeid,
                        fullname,
                        position,
                        department,
                        hire_date,
                        email,
                        phone,
                        isactive
                    FROM employees 
                    ORDER BY fullname";

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
                        certificateid,
                        employeeid,
                        certificatetype,
                        issuedate,
                        expirydate,
                        status,
                        notes
                    FROM certificates 
                    ORDER BY issuedate DESC";

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
                        observationid,
                        employeeid,
                        observationtype,
                        observationdate,
                        description,
                        status,
                        created_at
                    FROM observations 
                    ORDER BY observationdate DESC";

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
                        projectid,
                        projectname,
                        description,
                        startdate,
                        enddate,
                        status,
                        managerid,
                        created_at
                    FROM projects 
                    ORDER BY startdate DESC";

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
                    INSERT INTO user_activity_log (userid, username, action, details, ip_address, user_agent, created_at)
                    VALUES (@userid, @username, @action, @details, @ipaddress, @useragent, @created_at)";

                var parameters = new[]
                {
                    new NpgsqlParameter("@userid", userId),
                    new NpgsqlParameter("@username", username),
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
                        SELECT * FROM notifications 
                        WHERE userid = @userid 
                        ORDER BY created_at DESC";
                    parameters.Add(new NpgsqlParameter("@userid", userId));
                }
                else
                {
                    sql = @"
                        SELECT * FROM notifications 
                        ORDER BY created_at DESC";
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
                var sql = "UPDATE notifications SET is_read = true WHERE notificationid = @notificationid";
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
    }
} 
