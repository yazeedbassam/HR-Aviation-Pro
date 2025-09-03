using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApplication1.DataAccess;

namespace WebApplication1.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly SqlServerDb _db;
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(SqlServerDb db, ILogger<LoggerService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogUserActivityAsync(int userId, string userName, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null, bool isSuccessful = true, string? errorMessage = null)
        {
            try
            {
                var sql = @"
                    INSERT INTO UserActivityLogs (UserId, UserName, Action, EntityType, EntityId, Details, IpAddress, UserAgent, IsSuccessful, ErrorMessage)
                    VALUES (@UserId, @UserName, @Action, @EntityType, @EntityId, @Details, @IpAddress, @UserAgent, @IsSuccessful, @ErrorMessage)";

                await Task.Run(() => _db.ExecuteNonQuery(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                    new Microsoft.Data.SqlClient.SqlParameter("@UserName", userName),
                    new Microsoft.Data.SqlClient.SqlParameter("@Action", action),
                    new Microsoft.Data.SqlClient.SqlParameter("@EntityType", entityType),
                    new Microsoft.Data.SqlClient.SqlParameter("@EntityId", entityId ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@Details", details ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@IpAddress", ipAddress ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@UserAgent", userAgent ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@IsSuccessful", isSuccessful),
                    new Microsoft.Data.SqlClient.SqlParameter("@ErrorMessage", errorMessage ?? (object)DBNull.Value)
                ));

                _logger.LogDebug("User activity logged: {Action} on {EntityType} by {UserName}", action, entityType, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user activity for user {UserName}", userName);
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