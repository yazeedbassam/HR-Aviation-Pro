using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemLogsController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly ILoggerService _loggerService;

        public SystemLogsController(SqlServerDb db, ILoggerService loggerService)
        {
            _db = db;
            _loggerService = loggerService;
        }

        public IActionResult Index(int page = 1, int pageSize = 50, string? action = null, string? entityType = null, string? userName = null, string? startDate = null, string? endDate = null)
        {
            try
            {
                // Log view activity
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogEntityViewAsync(userId, currentUserName, "SystemLog", null, "Viewed system logs", ipAddress, userAgent));

                // Build query with filters
                var whereClause = "WHERE 1=1";
                var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();

                if (!string.IsNullOrEmpty(action))
                {
                    whereClause += " AND Action = @Action";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Action", action));
                }

                if (!string.IsNullOrEmpty(entityType))
                {
                    whereClause += " AND EntityType = @EntityType";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@EntityType", entityType));
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    whereClause += " AND UserName LIKE @UserName";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserName", $"%{userName}%"));
                }

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                {
                    whereClause += " AND Timestamp >= @StartDate";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@StartDate", start));
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                {
                    whereClause += " AND Timestamp <= @EndDate";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@EndDate", end));
                }

                // Get total count
                var countSql = $"SELECT COUNT(*) FROM UserActivityLogs {whereClause}";
                Microsoft.Data.SqlClient.SqlParameter[] sqlParams = parameters.ToArray();
                var totalCount = Convert.ToInt32(_db.ExecuteScalar(countSql, sqlParams));

                // Calculate pagination
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var offset = (page - 1) * pageSize;

                // Get paginated data
                var dataSql = $@"
                    SELECT Id, UserId, UserName, Action, EntityType, EntityId, Details, IpAddress, UserAgent, Timestamp, IsSuccessful, ErrorMessage
                    FROM UserActivityLogs 
                    {whereClause}
                    ORDER BY Timestamp DESC
                    OFFSET {offset} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY";

                var logs = _db.ExecuteQuery(dataSql, sqlParams);

                // Get distinct values for filters
                var actions = _db.ExecuteQuery("SELECT DISTINCT Action FROM UserActivityLogs ORDER BY Action");
                var entityTypes = _db.ExecuteQuery("SELECT DISTINCT EntityType FROM UserActivityLogs ORDER BY EntityType");
                var userNames = _db.ExecuteQuery("SELECT DISTINCT UserName FROM UserActivityLogs ORDER BY UserName");

                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Actions = actions;
                ViewBag.EntityTypes = entityTypes;
                ViewBag.UserNames = userNames;
                ViewBag.Filters = new
                {
                    Action = action,
                    EntityType = entityType,
                    UserName = userName,
                    StartDate = startDate,
                    EndDate = endDate
                };

                return View(logs);
            }
            catch (Exception ex)
            {
                // Log error
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, currentUserName, "View", "SystemLog", null, "Failed to load system logs", ipAddress, userAgent, ex.Message));

                TempData["Error"] = "حدث خطأ أثناء تحميل السجلات.";
                return View(new DataTable());
            }
        }

        [HttpPost]
        public IActionResult ExportLogs(string? action = null, string? entityType = null, string? userName = null, string? startDate = null, string? endDate = null)
        {
            try
            {
                // Log export activity
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogExportAsync(userId, currentUserName, "SystemLog", null, "Exported system logs", ipAddress, userAgent));

                // Build query with filters
                var whereClause = "WHERE 1=1";
                var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();

                if (!string.IsNullOrEmpty(action))
                {
                    whereClause += " AND Action = @Action";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Action", action));
                }

                if (!string.IsNullOrEmpty(entityType))
                {
                    whereClause += " AND EntityType = @EntityType";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@EntityType", entityType));
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    whereClause += " AND UserName LIKE @UserName";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserName", $"%{userName}%"));
                }

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                {
                    whereClause += " AND Timestamp >= @StartDate";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@StartDate", start));
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                {
                    whereClause += " AND Timestamp <= @EndDate";
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@EndDate", end));
                }

                var exportSql = $@"
                    SELECT 
                        Id,
                        UserId,
                        UserName,
                        Action,
                        EntityType,
                        EntityId,
                        Details,
                        IpAddress,
                        UserAgent,
                        Timestamp,
                        IsSuccessful,
                        ErrorMessage
                    FROM UserActivityLogs 
                    {whereClause}
                    ORDER BY Timestamp DESC";

                Microsoft.Data.SqlClient.SqlParameter[] exportParams = parameters.ToArray();
                var logs = _db.ExecuteQuery(exportSql, exportParams);

                // Generate CSV
                var csv = GenerateCsv(logs);
                var fileName = $"SystemLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                // Log error
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, currentUserName, "Export", "SystemLog", null, "Failed to export system logs", ipAddress, userAgent, ex.Message));

                TempData["Error"] = "حدث خطأ أثناء تصدير السجلات.";
                return RedirectToAction("Index");
            }
        }

        private string GenerateCsv(DataTable dataTable)
        {
            var csv = new System.Text.StringBuilder();

            // Add headers
            var headers = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                headers.Add(column.ColumnName);
            }
            csv.AppendLine(string.Join(",", headers));

            // Add data rows
            foreach (DataRow row in dataTable.Rows)
            {
                var values = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column]?.ToString() ?? "";
                    // Escape commas and quotes
                    if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    values.Add(value);
                }
                csv.AppendLine(string.Join(",", values));
            }

            return csv.ToString();
        }

        [HttpPost]
        public IActionResult ClearOldLogs(int daysToKeep = 90)
        {
            try
            {
                // Log cleanup activity
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(userId, currentUserName, "Cleanup", "SystemLog", null, $"Cleared logs older than {daysToKeep} days", ipAddress, userAgent));

                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var deleteSql = "DELETE FROM UserActivityLogs WHERE Timestamp < @CutoffDate";
                var deletedCount = _db.ExecuteNonQuery(deleteSql, new Microsoft.Data.SqlClient.SqlParameter("@CutoffDate", cutoffDate));

                TempData["Message"] = $"تم حذف {deletedCount} سجل قديم بنجاح.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log error
                var userId = UserActivityHelper.GetCurrentUserId(User);
                var currentUserName = UserActivityHelper.GetCurrentUserName(User);
                var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
                var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
                
                _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, currentUserName, "Cleanup", "SystemLog", null, "Failed to clear old logs", ipAddress, userAgent, ex.Message));

                TempData["Error"] = "حدث خطأ أثناء تنظيف السجلات القديمة.";
                return RedirectToAction("Index");
            }
        }
    }
} 