using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using Npgsql;

namespace WebApplication1.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly PostgreSQLDb _db;
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(PostgreSQLDb db, ILogger<LoggerService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogUserActivityAsync(int userId, string userName, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null, bool isSuccessful = true, string? errorMessage = null)
        {
            try
            {
                // إذا كان userId = 0، نحاول الحصول على معرف المستخدم الصحيح من قاعدة البيانات
                int actualUserId = userId;
                if (userId == 0 && !string.IsNullOrEmpty(userName))
                {
                    actualUserId = await GetUserIdByUsernameAsync(userName);
                    if (actualUserId == 0)
                    {
                        _logger.LogWarning("Could not find user ID for username: {UserName}, skipping activity log", userName);
                        return;
                    }
                }

                var sql = @"
                    INSERT INTO ""UserActivityLogs"" (""UserId"", ""UserName"", ""Action"", ""EntityType"", ""EntityId"", ""Details"", ""IpAddress"", ""UserAgent"", ""IsSuccessful"", ""ErrorMessage"", ""Timestamp"")
                    VALUES (@UserId, @UserName, @Action, @EntityType, @EntityId, @Details, @IpAddress, @UserAgent, @IsSuccessful, @ErrorMessage, @Timestamp)";

                await Task.Run(() => _db.ExecuteNonQuery(sql,
                    new Npgsql.NpgsqlParameter("@UserId", actualUserId),
                    new Npgsql.NpgsqlParameter("@UserName", userName),
                    new Npgsql.NpgsqlParameter("@Action", action),
                    new Npgsql.NpgsqlParameter("@EntityType", entityType),
                    new Npgsql.NpgsqlParameter("@EntityId", entityId ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@Details", details ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@IpAddress", ipAddress ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@UserAgent", userAgent ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@IsSuccessful", isSuccessful),
                    new Npgsql.NpgsqlParameter("@ErrorMessage", errorMessage ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@Timestamp", DateTime.Now)
                ));

                _logger.LogDebug("User activity logged: {Action} on {EntityType} by {UserName} (UserId: {UserId})", action, entityType, userName, actualUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user activity for user {UserName}", userName);
            }
        }

        private async Task<int> GetUserIdByUsernameAsync(string username)
        {
            try
            {
                var sql = "SELECT id FROM \"Users\" WHERE \"Username\" = @username";
                var result = await Task.Run(() => _db.ExecuteScalar(sql, new Npgsql.NpgsqlParameter("@username", username)));
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user ID for username: {UserName}", username);
                return 0;
            }
        }

        public async Task LogUserLoginAsync(int userId, string userName, string ipAddress, string userAgent, bool isSuccessful = true, string? errorMessage = null)
        {
            await LogUserActivityAsync(userId, userName, "Login", "System", null, 
                isSuccessful ? "User logged in successfully" : "Login failed", 
                ipAddress, userAgent, isSuccessful, errorMessage);
        }

        public async Task LogUserLogoutAsync(int userId, string userName, string ipAddress, string userAgent)
        {
            await LogUserActivityAsync(userId, userName, "Logout", "System", null, 
                "User logged out", ipAddress, userAgent, true, null);
        }

        public async Task LogEntityCreationAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Created new {entityType} with ID: {entityId}";
            await LogUserActivityAsync(userId, userName, "Create", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogEntityUpdateAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Updated {entityType} with ID: {entityId}";
            await LogUserActivityAsync(userId, userName, "Update", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogEntityDeletionAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Deleted {entityType} with ID: {entityId}";
            await LogUserActivityAsync(userId, userName, "Delete", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogEntityViewAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Viewed {entityType}" + (entityId != null ? $" with ID: {entityId}" : "");
            await LogUserActivityAsync(userId, userName, "View", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogExportAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Exported {entityType}" + (entityId != null ? $" with ID: {entityId}" : "");
            await LogUserActivityAsync(userId, userName, "Export", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogImportAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var logDetails = details ?? $"Imported {entityType}" + (entityId != null ? $" with ID: {entityId}" : "");
            await LogUserActivityAsync(userId, userName, "Import", entityType, entityId, logDetails, ipAddress, userAgent);
        }

        public async Task LogErrorAsync(int userId, string userName, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null, string errorMessage = "")
        {
            await LogUserActivityAsync(userId, userName, action, entityType, entityId, details, ipAddress, userAgent, false, errorMessage);
        }
    }
} 