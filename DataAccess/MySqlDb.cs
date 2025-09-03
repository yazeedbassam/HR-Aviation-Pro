using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.DataAccess
{
    public class MySqlDb
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;
        private readonly ILogger<MySqlDb> _logger;
        private readonly IPasswordHasher<ControllerUser> _passwordHasher;

        public MySqlDb(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MySqlDbConnection") 
                                ?? throw new ArgumentNullException("Connection string 'MySqlDbConnection' not found.");
            
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            
            var connectionStringForLogging = _connectionString;
            if (!string.IsNullOrEmpty(connectionStringForLogging))
            {
                var parts = connectionStringForLogging.Split(';');
                var safeParts = parts.Where(p => !p.ToLower().Contains("password") && !p.ToLower().Contains("pwd"));
                var safeConnectionString = string.Join(";", safeParts);
                Console.WriteLine($"MySQL Database connection configured: {safeConnectionString}");
            }
        }

        public MySqlDb(IConfiguration configuration, IPasswordHasher<ControllerUser> passwordHasher, ILogger<MySqlDb> logger)
        {
            var connectionString = configuration.GetConnectionString("MySqlDbConnection")
                                ?? throw new ArgumentNullException("Connection string 'MySqlDbConnection' not found.");
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        private string ReplaceEnvironmentVariables(string connectionString)
        {
            return connectionString
                .Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost")
                .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "railway")
                .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "root")
                .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "")
                .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? "3306");
        }

        public MySqlConnection GetConnection()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                Console.WriteLine("MySQL connection created successfully");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating MySQL connection: {ex.Message}");
                throw;
            }
        }

        public bool IsDatabaseAvailable()
        {
            try
            {
                Console.WriteLine("🔍 Testing MySQL database connection...");
                Console.WriteLine($"🔍 Connection string: {_connectionString}");
                
                using var connection = GetConnection();
                Console.WriteLine("🔍 Connection created, attempting to open...");
                connection.Open();
                Console.WriteLine("✅ MySQL Database connection opened successfully");
                connection.Close();
                Console.WriteLine("✅ MySQL Database connection closed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? MySQL Database availability check failed: {ex.Message}");
                Console.WriteLine($"? Exception type: {ex.GetType().Name}");
                Console.WriteLine($"? Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public DataTable ExecuteQuery(string sql, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            using var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            try
            {
                adapter.Fill(dt);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
                throw;
            }
            return dt;
        }

        public object ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        // Check which table exists and use the appropriate one
        private string GetUserTableName()
        {
            try
            {
                // Check if Users table exists
                var usersExists = ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Users'");
                if (Convert.ToInt32(usersExists) > 0)
                {
                    Console.WriteLine("📋 Using 'Users' table for user management");
                    return "Users";
                }

                // Check if Controllers table exists
                var controllersExists = ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Controllers'");
                if (Convert.ToInt32(controllersExists) > 0)
                {
                    Console.WriteLine("📋 Using 'Controllers' table for user management");
                    return "Controllers";
                }

                Console.WriteLine("⚠️ No user table found, creating Users table");
                CreateUsersTable();
                return "Users";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking table existence: {ex.Message}");
                return "Users"; // Default to Users table
            }
        }

        private void CreateUsersTable()
        {
            try
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INT AUTO_INCREMENT PRIMARY KEY,
                        Username VARCHAR(100) UNIQUE NOT NULL,
                        PasswordHash VARCHAR(255) NOT NULL,
                        RoleName VARCHAR(50) DEFAULT 'Controller',
                        IsActive BOOLEAN DEFAULT TRUE,
                        CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";
                ExecuteNonQuery(sql);
                Console.WriteLine("✅ Users table created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Users table: {ex.Message}");
            }
        }

        // User Management Methods - Updated to work with existing database structure
        public ControllerUser GetUserByUsername(string username)
        {
            try
            {
                var tableName = GetUserTableName();
                string sql;
                
                if (tableName == "Users")
                {
                    sql = "SELECT * FROM Users WHERE Username = @username";
                }
                else
                {
                    sql = "SELECT * FROM Controllers WHERE username = @username";
                }
                
                var dt = ExecuteQuery(sql, new MySqlParameter("@username", username));
                
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return new ControllerUser
                    {
                        ControllerId = Convert.ToInt32(row[tableName == "Users" ? "UserId" : "controllerid"]),
                        FullName = tableName == "Users" ? username : (row["fullname"]?.ToString() ?? username),
                        Username = row[tableName == "Users" ? "Username" : "username"].ToString(),
                        Password = row[tableName == "Users" ? "PasswordHash" : "password"].ToString(),
                        Role = row[tableName == "Users" ? "RoleName" : "role"].ToString(),
                        IsActive = Convert.ToBoolean(row[tableName == "Users" ? "IsActive" : "IsActive"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by username: {ex.Message}");
                return null;
            }
        }

        public bool CreateUser(string username, string password, string role = "Controller")
        {
            try
            {
                var hashedPassword = _passwordHasher?.HashPassword(null, password) ?? password;
                var tableName = GetUserTableName();
                
                string sql;
                MySqlParameter[] parameters;
                
                if (tableName == "Users")
                {
                    sql = "INSERT INTO Users (Username, PasswordHash, RoleName, IsActive) VALUES (@username, @password, @role, 1)";
                    parameters = new[]
                    {
                        new MySqlParameter("@username", username),
                        new MySqlParameter("@password", hashedPassword),
                        new MySqlParameter("@role", role)
                    };
                }
                else
                {
                    sql = "INSERT INTO Controllers (fullname, username, password, role, IsActive) VALUES (@fullname, @username, @password, @role, 1)";
                    parameters = new[]
                    {
                        new MySqlParameter("@fullname", username),
                        new MySqlParameter("@username", username),
                        new MySqlParameter("@password", hashedPassword),
                        new MySqlParameter("@role", role)
                    };
                }
                
                ExecuteNonQuery(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public bool VerifyPassword(string username, string password)
        {
            try
            {
                var user = GetUserByUsername(username);
                if (user == null) return false;
                
                if (_passwordHasher != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(null, user.Password, password);
                    return result == PasswordVerificationResult.Success;
                }
                
                // Fallback to direct comparison if no hasher
                return user.Password == password;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying password: {ex.Message}");
                return false;
            }
        }

        public List<ControllerUser> GetAllControllers()
        {
            try
            {
                var tableName = GetUserTableName();
                string sql;
                
                if (tableName == "Users")
                {
                    sql = "SELECT * FROM Users WHERE IsActive = 1";
                }
                else
                {
                    sql = "SELECT * FROM Controllers WHERE IsActive = 1";
                }
                
                var dt = ExecuteQuery(sql);
                
                var controllers = new List<ControllerUser>();
                foreach (DataRow row in dt.Rows)
                {
                    controllers.Add(new ControllerUser
                    {
                        ControllerId = Convert.ToInt32(row[tableName == "Users" ? "UserId" : "controllerid"]),
                        FullName = tableName == "Users" ? row["Username"].ToString() : row["fullname"].ToString(),
                        Username = row[tableName == "Users" ? "Username" : "username"].ToString(),
                        Role = row[tableName == "Users" ? "RoleName" : "role"].ToString(),
                        IsActive = Convert.ToBoolean(row[tableName == "Users" ? "IsActive" : "IsActive"])
                    });
                }
                return controllers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all controllers: {ex.Message}");
                return new List<ControllerUser>();
            }
        }
    }
} 