using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

    public class AccountController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly IEmailService _emailService;
        private readonly ILoggerService _loggerService;
        private readonly IAdvancedPermissionManagerService _permissionService;
        private readonly IAdvancedPermissionService _advancedPermissionService;
        private readonly ILogger<AccountController> _logger;
        private readonly IDatabaseService _databaseService;

        public AccountController(SqlServerDb db, IEmailService emailService, ILoggerService loggerService, IAdvancedPermissionManagerService permissionService, IAdvancedPermissionService advancedPermissionService, ILogger<AccountController> logger, IDatabaseService databaseService)
        {
            _db = db;
            _emailService = emailService;
            _loggerService = loggerService;
            _permissionService = permissionService;
            _advancedPermissionService = advancedPermissionService;
            _logger = logger;
            _databaseService = databaseService;
        }

    [Authorize]
    public async Task<IActionResult> Notifications()
    {
        // Log view activity
        var userId = UserActivityHelper.GetCurrentUserId(User);
        var userName = UserActivityHelper.GetCurrentUserName(User);
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);
        
        _ = Task.Run(async () => await _loggerService.LogEntityViewAsync(userId, userName, "Notification", null, "Viewed notifications page", ipAddress, userAgent));

        try
        {
            string sql;
            var parameters = new List<SqlParameter>();

            if (User.IsInRole("Admin"))
            {
                // المسؤول يرى جميع الإشعارات من جدول notifications
                // لكن يتجاهل المستخدمين بدون رخص أو بدون تاريخ انتهاء صلاحية
                sql = @"
                    SELECT 
                        n.NotificationId,
                        n.userid,
                        n.controllerid,
                        COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                        n.message,
                        n.link,
                        n.created_at,
                        n.is_read,
                        n.note,
                        n.licensetype,
                        n.licenseexpirydate,
                        CASE 
                            WHEN n.licenseexpirydate IS NOT NULL THEN DATEDIFF(day, GETDATE(), n.licenseexpirydate)
                            ELSE NULL
                        END AS RemainingDays,
                        CASE 
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN 'Suspended'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN 'Critical'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN 'Warning'
                            ELSE 'Unknown'
                        END AS Status,
                        CASE 
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN '#dc3545'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN '#ffc107'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN '#28a745'
                            ELSE '#6c757d'
                        END AS StatusColor,
                        COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                        COALESCE(c.email, e.email, 'N/A') AS email,
                        COALESCE(c.current_department, e.department, 'N/A') AS Department,
                        COALESCE(a.airportname, 'HQ - Main Office') AS airportname,
                        CASE 
                            WHEN n.controllerid IS NOT NULL THEN 'Controller'
                            ELSE 'Employee'
                        END AS UserType,
                        CASE 
                            WHEN n.licenseexpirydate IS NOT NULL THEN 'Expiring Soon'
                            ELSE 'General'
                        END AS Status
                    FROM notifications n
                    LEFT JOIN controllers c ON n.controllerid = c.controllerid
                    LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                    LEFT JOIN airports a ON c.airportid = a.airportid
                    WHERE n.licensetype IS NOT NULL 
                      AND n.licensetype != 'No License'
                      AND n.licenseexpirydate IS NOT NULL
                      AND n.licenseexpirydate != ''
                    ORDER BY n.created_at DESC";
            }
            else
            {
                // التحقق من الصلاحية الجديدة
                var currentUserId = UserActivityHelper.GetCurrentUserId(User);
                var hasDepartmentOverview = await _permissionService.HasPermissionAsync(currentUserId, "DEPARTMENT_OVERVIEW");
                var currentUsername = User.Identity.Name;
                
                if (hasDepartmentOverview)
                {
                    // المستخدم مع صلاحية DEPARTMENT_OVERVIEW يرى إشعارات قسمه
                    var userDepartment = GetUserDepartment(currentUsername);
                    
                    if (!string.IsNullOrEmpty(userDepartment))
                    {
                        // عرض إشعارات القسم فقط
                        sql = @"
                            SELECT 
                                n.NotificationId,
                                n.userid,
                                n.controllerid,
                                COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                                n.message,
                                n.link,
                                n.created_at,
                                n.is_read,
                                n.note,
                                n.licensetype,
                                n.licenseexpirydate,
                                CASE 
                                    WHEN n.licenseexpirydate IS NOT NULL THEN DATEDIFF(day, GETDATE(), n.licenseexpirydate)
                                    ELSE NULL
                                END AS RemainingDays,
                                CASE 
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN 'Suspended'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN 'Critical'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN 'Warning'
                                    ELSE 'Unknown'
                                END AS Status,
                                CASE 
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN '#dc3545'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN '#ffc107'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN '#28a745'
                                    ELSE '#6c757d'
                                END AS StatusColor,
                                COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                                COALESCE(c.email, e.email, 'N/A') AS email,
                                COALESCE(c.current_department, e.department, 'N/A') AS Department,
                                COALESCE(a.airportname, 'HQ - Main Office') AS airportname,
                                CASE 
                                    WHEN n.controllerid IS NOT NULL THEN 'Controller'
                                    ELSE 'Employee'
                                END AS UserType,
                                CASE 
                                    WHEN n.licenseexpirydate IS NOT NULL THEN 'Expiring Soon'
                                    ELSE 'General'
                                END AS Status
                            FROM notifications n
                            LEFT JOIN controllers c ON n.controllerid = c.controllerid
                            LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                            LEFT JOIN airports a ON c.airportid = a.airportid
                            WHERE (c.current_department = @userDepartment OR e.department = @userDepartment)
                              AND n.licensetype IS NOT NULL 
                              AND n.licensetype != 'No License'
                              AND n.licenseexpirydate IS NOT NULL
                              AND n.licenseexpirydate != ''
                            ORDER BY n.created_at DESC";
                        
                        parameters.Add(new SqlParameter("@userDepartment", userDepartment));
                    }
                    else
                    {
                        // إذا لم يتم العثور على القسم، يعرض بياناته فقط
                        sql = @"
                            SELECT 
                                n.NotificationId,
                                n.userid,
                                n.controllerid,
                                COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                                n.message,
                                n.link,
                                n.created_at,
                                n.is_read,
                                n.note,
                                n.licensetype,
                                n.licenseexpirydate,
                                CASE 
                                    WHEN n.licenseexpirydate IS NOT NULL THEN DATEDIFF(day, GETDATE(), n.licenseexpirydate)
                                    ELSE NULL
                                END AS RemainingDays,
                                CASE 
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN 'Suspended'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN 'Critical'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN 'Warning'
                                    ELSE 'Unknown'
                                END AS Status,
                                CASE 
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN '#dc3545'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN '#ffc107'
                                    WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN '#28a745'
                                    ELSE '#6c757d'
                                END AS StatusColor,
                                COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                                COALESCE(c.email, e.email, 'N/A') AS email,
                                COALESCE(c.current_department, e.department, 'N/A') AS Department,
                                COALESCE(a.airportname, 'HQ - Main Office') AS airportname,
                                CASE 
                                    WHEN n.controllerid IS NOT NULL THEN 'Controller'
                                    ELSE 'Employee'
                                END AS UserType,
                                CASE 
                                    WHEN n.licenseexpirydate IS NOT NULL THEN 'Expiring Soon'
                                    ELSE 'General'
                                END AS Status
                            FROM notifications n
                            LEFT JOIN controllers c ON n.controllerid = c.controllerid
                            LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                            LEFT JOIN airports a ON c.airportid = a.airportid
                            WHERE (c.username = @currentUsername OR e.username = @currentUsername)
                              AND n.licensetype IS NOT NULL 
                              AND n.licensetype != 'No License'
                              AND n.licenseexpirydate IS NOT NULL
                              AND n.licenseexpirydate != ''
                            ORDER BY n.created_at DESC";
                        
                        parameters.Add(new SqlParameter("@currentUsername", currentUsername));
                    }
                }
                else
                {
                    // المستخدم بدون صلاحية يعرض بياناته فقط
                    sql = @"
                        SELECT 
                            n.NotificationId,
                            n.userid,
                            n.controllerid,
                            COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                            n.message,
                            n.link,
                            n.created_at,
                            n.is_read,
                            n.note,
                            n.licensetype,
                            n.licenseexpirydate,
                            CASE 
                                WHEN n.licenseexpirydate IS NOT NULL THEN DATEDIFF(day, GETDATE(), n.licenseexpirydate)
                                ELSE NULL
                            END AS RemainingDays,
                            CASE 
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN 'Suspended'
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN 'Critical'
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN 'Warning'
                                ELSE 'Unknown'
                            END AS Status,
                            CASE 
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN '#dc3545'
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN '#ffc107'
                                WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN '#28a745'
                                ELSE '#6c757d'
                            END AS StatusColor,
                            COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                            COALESCE(c.email, e.email, 'N/A') AS email,
                            COALESCE(c.current_department, e.department, 'N/A') AS Department,
                            COALESCE(a.airportname, 'HQ - Main Office') AS airportname,
                            CASE 
                                WHEN n.controllerid IS NOT NULL THEN 'Controller'
                                ELSE 'Employee'
                            END AS UserType,
                            CASE 
                                WHEN n.licenseexpirydate IS NOT NULL THEN 'Expiring Soon'
                                ELSE 'General'
                            END AS Status
                        FROM notifications n
                        LEFT JOIN controllers c ON n.controllerid = c.controllerid
                        LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                        LEFT JOIN airports a ON c.airportid = a.airportid
                        WHERE (c.username = @currentUsername OR e.username = @currentUsername)
                          AND n.licensetype IS NOT NULL 
                          AND n.licensetype != 'No License'
                          AND n.licenseexpirydate IS NOT NULL
                          AND n.licenseexpirydate != ''
                        ORDER BY n.created_at DESC";
                    
                    parameters.Add(new SqlParameter("@currentUsername", currentUsername));
                }
            }

            // تنفيذ الاستعلام مع المعاملات
            // استخدام جدول notifications يضمن اتساق البيانات بين العرض والتصدير
            var dt = parameters.Count > 0 
                ? _db.ExecuteQuery(sql, parameters.ToArray())
                : _db.ExecuteQuery(sql);

            return View("Notifications", dt);
        }
        catch (Exception ex)
        {
            // Log error using existing variables
            _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, userName, "View", "Notification", null, "Failed to load notifications", ipAddress, userAgent, ex.Message));
            
            // Consider returning an error view
            return View("Error"); // Make sure you have an Error.cshtml view
        }
    }

    /// <summary>
    /// الحصول على قسم المستخدم
    /// </summary>
    private string GetUserDepartment(string username)
    {
        try
        {
            // محاولة جلب قسم المراقب
            var controllerQuery = @"
                SELECT current_department 
                FROM controllers 
                WHERE username = @username";
            
            var controllerResult = _db.ExecuteScalar(controllerQuery, new SqlParameter("@username", username));
            if (controllerResult != null && controllerResult != DBNull.Value)
            {
                return controllerResult.ToString();
            }
            
            // محاولة جلب قسم الموظف
            var employeeQuery = @"
                SELECT Department 
                FROM employees 
                WHERE username = @username AND IsActive = 1";
            
            var employeeResult = _db.ExecuteScalar(employeeQuery, new SqlParameter("@username", username));
            if (employeeResult != null && employeeResult != DBNull.Value)
            {
                return employeeResult.ToString();
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// الحصول على موظفي القسم
    /// </summary>
    private List<object> GetDepartmentEmployees(string department)
    {
        try
        {
            var employees = new List<object>();
            
            // جلب المراقبين في القسم
            var controllerQuery = @"
                SELECT 
                    'Controller' AS UserType,
                    username,
                    FullName AS fullname,
                    Email AS email,
                    Phone_Number AS phone_number,
                    current_department
                FROM controllers 
                WHERE current_department = @department";
            
            var controllerData = _db.ExecuteQuery(controllerQuery, new SqlParameter("@department", department));
            System.Diagnostics.Debug.WriteLine($"GetDepartmentEmployees: Found {controllerData.Rows.Count} controllers in department '{department}'");
            
            // Debug: طباعة أسماء الأعمدة
            if (controllerData.Rows.Count > 0)
            {
                var columns = controllerData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                System.Diagnostics.Debug.WriteLine($"Controller columns: {string.Join(", ", columns)}");
            }
            
            foreach (DataRow row in controllerData.Rows)
            {
                employees.Add(new
                {
                    UserType = row["UserType"].ToString(),
                    Username = row["username"].ToString(),
                    FullName = row["fullname"].ToString(),
                    Email = row["email"].ToString(),
                    PhoneNumber = row["phone_number"].ToString(),
                    Department = row["current_department"].ToString()
                });
            }
            
            // جلب الموظفين في القسم - إصلاح العمود username
            var employeeQuery = @"
                SELECT 
                    'Employee' AS UserType,
                    FullName AS username,
                    FullName AS fullname,
                    Email AS email,
                    PhoneNumber AS phonenumber,
                    Department AS department
                FROM employees 
                WHERE Department = @department AND IsActive = 1";
            
            var employeeData = _db.ExecuteQuery(employeeQuery, new SqlParameter("@department", department));
            System.Diagnostics.Debug.WriteLine($"GetDepartmentEmployees: Found {employeeData.Rows.Count} employees in department '{department}'");
            
            // Debug: طباعة أسماء الأعمدة
            if (employeeData.Rows.Count > 0)
            {
                var columns = employeeData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                System.Diagnostics.Debug.WriteLine($"Employee columns: {string.Join(", ", columns)}");
            }
            
            foreach (DataRow row in employeeData.Rows)
            {
                employees.Add(new
                {
                    UserType = row["UserType"].ToString(),
                    Username = row["username"].ToString(),
                    FullName = row["fullname"].ToString(),
                    Email = row["email"].ToString(),
                    PhoneNumber = row["phonenumber"]?.ToString() ?? "-",
                    Department = row["department"].ToString()
                });
            }
            
            System.Diagnostics.Debug.WriteLine($"GetDepartmentEmployees: Total employees found for department '{department}': {employees.Count}");
            
            // Debug: طباعة تفاصيل كل موظف
            foreach (var emp in employees)
            {
                System.Diagnostics.Debug.WriteLine($"Employee: {emp}");
            }
            
            return employees;
        }
        catch (Exception ex)
        {
            // Log error using the available logger service
            _loggerService.LogErrorAsync(0, "System", "GetDepartmentEmployees", "Department", department, $"Error: {ex.Message}");
            return new List<object>();
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendLicenseExpiryEmails()
    {
        var userId = UserActivityHelper.GetCurrentUserId(User);
        var userName = UserActivityHelper.GetCurrentUserName(User);
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

        DataTable expiredLicenses = _db.ExecuteQuery(@"
            SELECT c.controllerid, c.email, l.licensetype, l.expirydate, c.fullname
            FROM licenses l
            INNER JOIN controllers c ON l.controllerid = c.controllerid
            WHERE l.expirydate <= DATEADD(day, 60, GETDATE())
    ");

        if (expiredLicenses.Rows.Count > 0)
        {
            foreach (DataRow row in expiredLicenses.Rows)
            {
                int controllerId = Convert.ToInt32(row["controllerid"]);
                string toEmail = row["email"].ToString();
                string licenseType = row["licensetype"].ToString();
                DateTime expiryDate = Convert.ToDateTime(row["expirydate"]);
                string fullname = row["fullname"].ToString();
                string subject = "Notify: اقتراب انتهاء صلاحية الرخصة (license expire)";
                string body = $"Dear {fullname}, Your {licenseType} will expire :  At {expiryDate:yyyy-MM-dd} 😊 \n\n So, Please Update 😔. \n\nيرجى اتخاذ الإجراءات اللازمة لتجديدها.";

                if (!string.IsNullOrWhiteSpace(toEmail))
                {
                    try
                    {
                        await _emailService.SendLicenseExpiryAlertAsync(toEmail, fullname, licenseType, expiryDate);
                        
                        // Log successful email sending
                        _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(userId, userName, "SendEmail", "License", controllerId.ToString(), 
                            $"Sent expiry alert to {fullname} for {licenseType}", ipAddress, userAgent));
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, userName, "SendEmail", "License", controllerId.ToString(), 
                            $"Failed to send expiry alert to {fullname}", ipAddress, userAgent, ex.Message));
                    }
                }
            }
            
            // Log bulk email action
            _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(userId, userName, "SendBulkEmails", "License", null, 
                $"Sent {expiredLicenses.Rows.Count} expiry alerts", ipAddress, userAgent));
            
            TempData["Message"] = "تم إرسال رسائل البريد الإلكتروني للمراقبين الذين ستقترب صلاحية رخصهم من الانتهاء.";
        }
        else
        {
            TempData["Message"] = "لا توجد رخص ستقترب صلاحيتها من الانتهاء لإرسال بريد إلكتروني بشأنها.";
        }

        return RedirectToAction("Notifications");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> TestNotificationService()
    {
        var userId = UserActivityHelper.GetCurrentUserId(User);
        var userName = UserActivityHelper.GetCurrentUserName(User);
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

        try
        {
            // اختبار خدمة الإشعارات يدوياً
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<LicenseExpiryNotificationService>();
                await notificationService.PerformLicenseExpiryCheck();
            }
            
            // Log successful test
            _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(userId, userName, "TestService", "System", null, 
                "Tested notification service manually", ipAddress, userAgent));
            
            TempData["Message"] = "تم تشغيل خدمة الإشعارات بنجاح! تحقق من جدول الإشعارات.";
        }
        catch (Exception ex)
        {
            // Log error
            _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, userName, "TestService", "System", null, 
                "Failed to test notification service", ipAddress, userAgent, ex.Message));
            
            TempData["Error"] = $"حدث خطأ أثناء تشغيل خدمة الإشعارات: {ex.Message}";
        }

        return RedirectToAction("Notifications");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "ATC Controller Portal Login";
        // فقط رجّع الـ View مع الموديل، لا تعيّن Layout هون
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // Temporary endpoint to fix admin password in PostgreSQL
    [AllowAnonymous]
    [HttpGet]
    public IActionResult FixAdminPasswordInPostgreSQL()
    {
        try
        {
            var postgresqlDb = HttpContext.RequestServices.GetRequiredService<PostgreSQLDb>();
            
            // Hash the password "123" using BCrypt (same as PostgreSQL uses)
            var hashedPassword = postgresqlDb.HashPassword("123");
            
            // Update the admin user's password in PostgreSQL database
            string sql = @"UPDATE ""Users"" SET ""PasswordHash"" = @passwordHash WHERE ""Username"" = 'admin'";
            var parameters = new[]
            {
                new Npgsql.NpgsqlParameter("@passwordHash", hashedPassword)
            };
            
            using (var connection = new Npgsql.NpgsqlConnection(postgresqlDb.ConnectionString))
            {
                connection.Open();
                using (var command = new Npgsql.NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    int rowsAffected = command.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        return Json(new { 
                            success = true, 
                            message = "Admin password updated successfully in PostgreSQL!",
                            hashedPassword = hashedPassword,
                            rowsAffected = rowsAffected,
                            note = "Password '123' hashed with BCrypt for PostgreSQL"
                        });
                    }
                    else
                    {
                        return Json(new { 
                            success = false, 
                            message = "No admin user found to update in PostgreSQL" 
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false, 
                message = $"Error updating admin password in PostgreSQL: {ex.Message}" 
            });
        }
    }

    // Temporary endpoint to get hashed password for "123"
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetHashedPassword()
    {
        try
        {
            var hashedPassword = _db.HashPassword("123");
            return Json(new { 
                success = true, 
                password = "123",
                hashedPassword = hashedPassword,
                message = "Use this hashed password to update the admin user in database"
            });
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false, 
                message = $"Error hashing password: {ex.Message}" 
            });
        }
    }

    // Temporary endpoint to recreate admin user with correct password
    [AllowAnonymous]
    [HttpGet]
    public IActionResult RecreateAdminUser()
    {
        try
        {
            // Delete existing admin user
            string deleteSql = "DELETE FROM users WHERE username = 'admin'";
            using (var connection = new SqlConnection(_db.ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(deleteSql, connection))
                {
                    int deletedRows = command.ExecuteNonQuery();
                    
                    // Create new admin user with properly hashed password
                    var hashedPassword = _db.HashPassword("123");
                    int newUserId = _db.CreateUser("admin", hashedPassword, "Admin");
                    
                    return Json(new { 
                        success = true, 
                        message = "Admin user recreated successfully!",
                        deletedRows = deletedRows,
                        newUserId = newUserId,
                        hashedPassword = hashedPassword
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false, 
                message = $"Error recreating admin user: {ex.Message}" 
            });
        }
    }

    // Temporary endpoint to fix admin password
    [AllowAnonymous]
    [HttpGet]
    public IActionResult FixAdminPassword()
    {
        try
        {
            // Hash the password "123" using the same PasswordHasher
            var hashedPassword = _db.HashPassword("123");
            
            // Update the admin user's password in the database
            string sql = "UPDATE Users SET PasswordHash = @passwordHash WHERE Username = 'admin'";
            var parameters = new[]
            {
                new SqlParameter("@passwordHash", hashedPassword)
            };
            
            using (var connection = new SqlConnection(_db.ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    int rowsAffected = command.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        return Json(new { 
                            success = true, 
                            message = "Admin password updated successfully!",
                            hashedPassword = hashedPassword,
                            rowsAffected = rowsAffected
                        });
                    }
                    else
                    {
                        return Json(new { 
                            success = false, 
                            message = "No admin user found to update" 
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false, 
                message = $"Error updating admin password: {ex.Message}" 
            });
        }
    }

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

        try
        {
            if (!ModelState.IsValid)
            {
                // Log failed login attempt
                _ = Task.Run(async () => await _loggerService.LogUserLoginAsync(0, model.Username, ipAddress, userAgent, false, "Invalid model state"));
                return View(model);
            }

            // Validate database type selection
            if (string.IsNullOrEmpty(model.DatabaseType))
            {
                ModelState.AddModelError("DatabaseType", "Please select a database connection type.");
                return View(model);
            }

            // Store database selection in session for later use
            HttpContext.Session.SetString("SelectedDatabase", model.DatabaseType);

            // Switch to the selected database
            if (model.DatabaseType != "skip")
            {
                _databaseService.SwitchDatabase(model.DatabaseType);
                _logger.LogInformation("🔄 Database switched to: {DatabaseType}", model.DatabaseType);
            }

            // Check if database is available based on selection
            bool isDatabaseAvailable = true; // Default to true
            
            if (model.DatabaseType != "skip")
            {
                _logger.LogInformation("🔍 Testing connection to: {DatabaseType}", model.DatabaseType);
                isDatabaseAvailable = await _databaseService.IsDatabaseAvailableAsync(model.DatabaseType);
                
                if (isDatabaseAvailable)
                {
                    _logger.LogInformation("✅ Connection to {DatabaseType} successful", model.DatabaseType);
                }
                else
                {
                    _logger.LogWarning("❌ Connection to {DatabaseType} failed", model.DatabaseType);
                }
            }

            if (!isDatabaseAvailable)
            {
                // Log failed login attempt
                _ = Task.Run(async () => await _loggerService.LogUserLoginAsync(0, model.Username, ipAddress, userAgent, false, "Database not available"));
                
                if (model.DatabaseType == "local")
                {
                    ModelState.AddModelError("", "Local SQL Server is not available. Please start SQL Server or choose Demo database.");
                }
                else
                {
                    ModelState.AddModelError("", $"Database connection ({model.DatabaseType}) is not available. Please check your configuration.");
                }
                return View(model);
            }
            
            // Validate credentials using the selected database
            bool isValidCredentials = false;
            int userId = 0;
            string role = "";
            
            if (model.DatabaseType == "supabase")
            {
                // Use Supabase for authentication
                try
                {
                    var postgresqlDb = HttpContext.RequestServices.GetRequiredService<PostgreSQLDb>();
                    _logger.LogInformation("🔍 Attempting PostgreSQL authentication for user: {Username}", model.Username);
                    
                    // Test PostgreSQL connection first
                    if (postgresqlDb.IsDatabaseAvailable())
                    {
                        _logger.LogInformation("✅ PostgreSQL connection is available, proceeding with authentication");
                        isValidCredentials = postgresqlDb.ValidateCredentials(model.Username, model.Password, out userId, out role);
                        _logger.LogInformation("🔍 PostgreSQL authentication result: {IsValid}, UserId: {UserId}, Role: {Role}", isValidCredentials, userId, role);
                    }
                    else
                    {
                        _logger.LogError("❌ PostgreSQL connection is not available");
                        ModelState.AddModelError("", "PostgreSQL database is not available. Please check your connection.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during Supabase authentication for user: {Username}", model.Username);
                    ModelState.AddModelError("", "Error connecting to Supabase database. Please try again.");
                    return View(model);
                }
            }
            else if (model.DatabaseType == "local")
            {
                // Use SQL Server for authentication
                try
                {
                    _logger.LogInformation("🔍 Attempting SQL Server authentication for user: {Username}", model.Username);
                    isValidCredentials = _db.ValidateCredentials(model.Username, model.Password, out userId, out role);
                    _logger.LogInformation("🔍 SQL Server authentication result: {IsValid}, UserId: {UserId}, Role: {Role}", isValidCredentials, userId, role);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during SQL Server authentication for user: {Username}", model.Username);
                    ModelState.AddModelError("", "Error connecting to SQL Server database. Please try again.");
                    return View(model);
                }
            }
            
            if (!isValidCredentials)
            {
                // Log failed login attempt
                _ = Task.Run(async () => await _loggerService.LogUserLoginAsync(0, model.Username, ipAddress, userAgent, false, "Invalid credentials"));
                
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserId", userId.ToString()), // Additional claim for compatibility
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          principal, authProperties);

            // Log successful login
            _ = Task.Run(async () => await _loggerService.LogUserLoginAsync(userId, model.Username, ipAddress, userAgent, true));
            
            // Debug: Check if user is authenticated
            _logger.LogInformation($"🔍 User authenticated: {HttpContext.User.Identity?.IsAuthenticated}");
            _logger.LogInformation($"🔍 User name: {HttpContext.User.Identity?.Name}");
            _logger.LogInformation($"🔍 User roles: {string.Join(", ", HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

            // Clear user permission cache on login to ensure fresh permissions
            try
            {
                var permissionService = HttpContext.RequestServices.GetRequiredService<IAdvancedPermissionManagerService>();
                permissionService.ClearAllUserCaches(userId);
            }
            catch (Exception ex)
            {
                // Log error silently
            }

            // بعد النجاح:
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // بناءً على الرول:
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // Log error
            _ = Task.Run(async () => await _loggerService.LogUserLoginAsync(0, model.Username, ipAddress, userAgent, false, ex.Message));
            
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = UserActivityHelper.GetCurrentUserId(User);
        var userName = UserActivityHelper.GetCurrentUserName(User);
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

        // Clear user permission cache on logout
        try
            {
                var permissionService = HttpContext.RequestServices.GetRequiredService<IAdvancedPermissionManagerService>();
                permissionService.ClearAllUserCaches(userId);
        }
        catch (Exception ex)
        {
            // Log error silently
        }

        // Log logout
        _ = Task.Run(async () => await _loggerService.LogUserLogoutAsync(userId, userName, ipAddress, userAgent));

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // استبدل أكشن Profile (GET) الحالي عندك بهذا الكود المحدث
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var userId = UserActivityHelper.GetCurrentUserId(User);
            var userName = UserActivityHelper.GetCurrentUserName(User);
            var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
            var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

            // Log profile view
            _ = Task.Run(async () => await _loggerService.LogEntityViewAsync(userId, userName, "Profile", userId.ToString(), "Viewed user profile", ipAddress, userAgent));

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // التحقق من الصلاحية باستخدام AdvancedPermissionService
            var hasDepartmentOverview = await _advancedPermissionService
                .CanUserPerformOperationAsync(username, "DEPARTMENT_OVERVIEW");
            
            System.Diagnostics.Debug.WriteLine($"Profile: User '{username}' has DEPARTMENT_OVERVIEW permission: {hasDepartmentOverview}");

            // Simply call the unified method from SqlServerDb
            var viewModel = _db.GetProfileDataByUsername(username);

            if (viewModel == null)
            {
                ViewBag.ErrorMessage = "User profile not found.";
                return View(new ProfileViewModel()!); // Return an empty page if user doesn't exist
            }

            // إضافة بيانات القسم إذا كان لديه الصلاحية
            if (hasDepartmentOverview)
            {
                var userDepartment = GetUserDepartment(username);
                System.Diagnostics.Debug.WriteLine($"User department: {userDepartment}");
                
                if (!string.IsNullOrEmpty(userDepartment))
                {
                    ViewBag.ShowDepartmentData = true;
                    ViewBag.UserDepartment = userDepartment;
                    
                    var deptEmployees = GetDepartmentEmployees(userDepartment);
                    ViewBag.DepartmentEmployees = deptEmployees;
                    
                    System.Diagnostics.Debug.WriteLine($"Department employees count: {deptEmployees?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"ViewBag.ShowDepartmentData set to: {ViewBag.ShowDepartmentData}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User department is null or empty");
                    ViewBag.ShowDepartmentData = false;
                }
            }
            else
            {
                ViewBag.ShowDepartmentData = false;
                System.Diagnostics.Debug.WriteLine("User does not have DEPARTMENT_OVERVIEW permission");
                
                // إضافة معلومات debugging أكثر
                System.Diagnostics.Debug.WriteLine($"Username: {username}");
                System.Diagnostics.Debug.WriteLine($"User ID: {UserActivityHelper.GetCurrentUserId(User)}");
                System.Diagnostics.Debug.WriteLine($"Permission check result: {hasDepartmentOverview}");
            }

            // Send the fully populated model to the view
            return View(viewModel);
        }
        catch (Exception ex)
        {
            // Log error
            _ = Task.Run(async () => await _loggerService.LogErrorAsync(0, "System", "Profile", "Profile", null, 
                "Failed to load profile", null, null, ex.Message));
            
            ModelState.AddModelError("", "An error occurred while loading profile: " + ex.Message);
            return View(new ProfileViewModel()!);
        }
    }

    // The POST action for Profile handles the update logic smartly.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = UserActivityHelper.GetCurrentUserId(User);
        var userName = UserActivityHelper.GetCurrentUserName(User);
        var ipAddress = UserActivityHelper.GetUserIpAddress(HttpContext);
        var userAgent = UserActivityHelper.GetUserAgent(HttpContext);

        // Password is optional, so we remove it from validation to allow empty submissions.
        ModelState.Remove("Password");
        // PhotoFile is also optional.
        ModelState.Remove("PhotoFile");

        // Validate required fields
        if (string.IsNullOrEmpty(model.FullName))
        {
            ModelState.AddModelError("FullName", "Full Name is required.");
        }
        
        if (string.IsNullOrEmpty(model.Email))
        {
            ModelState.AddModelError("Email", "Email is required.");
        }
        
        if (string.IsNullOrEmpty(model.PhoneNumber))
        {
            ModelState.AddModelError("PhoneNumber", "Phone Number is required.");
        }
        
        if (string.IsNullOrEmpty(model.Address))
        {
            ModelState.AddModelError("Address", "Address is required.");
        }
        
        if (string.IsNullOrEmpty(model.EmergencyContact))
        {
            ModelState.AddModelError("EmergencyContact", "Emergency Contact is required.");
        }
        
        // Controller-specific validations
        if (model.UserType == "Controller")
        {
            if (string.IsNullOrEmpty(model.EducationLevel))
            {
                ModelState.AddModelError("EducationLevel", "Education Level is required for Controllers.");
            }
        }

        // Financial Information validations (optional but recommended)
        if (model.CurrentSalary.HasValue && model.CurrentSalary.Value < 0)
        {
            ModelState.AddModelError("CurrentSalary", "Current Salary cannot be negative.");
        }
        
        if (model.SalaryAfterAnnualIncrease.HasValue && model.SalaryAfterAnnualIncrease.Value < 0)
        {
            ModelState.AddModelError("SalaryAfterAnnualIncrease", "Salary After Annual Increase cannot be negative.");
        }

        if (!ModelState.IsValid)
        {
            // If validation fails, repopulate the full model to avoid errors on page reload.
            var repopulatedModel = _db.GetProfileDataByUsername(model.Username);
            return View(repopulatedModel ?? new ProfileViewModel()!);
        }

        try
        {
            // Smart Update Logic based on UserType from the form
            if (model.UserType == "Controller")
            {
                // البحث عن ControllerId الحقيقي بناءً على Username
                var controllerData = _db.GetControllerByUsername(model.Username);
                if (controllerData != null)
                {
                    var controllerId = Convert.ToInt32(controllerData["controllerid"]);
                    
                    var controllerToUpdate = new ControllerUser
                    {
                        ControllerId = controllerId, // استخدام ControllerId الحقيقي
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        DateOfBirth = model.DateOfBirth,
                        MaritalStatus = model.MaritalStatus,
                        Address = model.Address,
                        EmergencyContact = model.EmergencyContact,
                        EducationLevel = model.EducationLevel,
                        CurrentSalary = model.CurrentSalary,
                        AnnualIncreasePercentage = model.AnnualIncreasePercentage,
                        SalaryAfterAnnualIncrease = model.SalaryAfterAnnualIncrease,
                        BankAccountNumber = model.BankAccountNumber,
                        BankName = model.BankName,
                        TaxId = model.TaxId,
                        InsuranceNumber = model.InsuranceNumber,
                    };
                    _db.UpdateControllerProfile(controllerToUpdate);
                    
                    // Log profile update
                    _ = Task.Run(async () => await _loggerService.LogEntityUpdateAsync(userId, userName, "Controller", controllerId.ToString(), 
                        $"Updated controller profile: {model.FullName}", ipAddress, userAgent));
                }
            }
            else if (model.UserType == "Employee")
            {
                // البحث عن EmployeeID الحقيقي بناءً على Username
                var employeeData = _db.GetEmployeeByUsername(model.Username);
                if (employeeData != null)
                {
                    var employeeId = employeeData.EmployeeID;
                    
                    var employeeToUpdate = new Employee
                    {
                        EmployeeID = employeeId, // استخدام EmployeeID الحقيقي
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        EmergencyContactPhone = model.EmergencyContact,
                        Gender = model.Gender,
                        Location = model.Location,
                        CurrentSalary = model.CurrentSalary,
                        AnnualIncreasePercentage = model.AnnualIncreasePercentage,
                        SalaryAfterAnnualIncrease = model.SalaryAfterAnnualIncrease,
                        BankAccountNumber = model.BankAccountNumber,
                        BankName = model.BankName,
                        TaxId = model.TaxId,
                        InsuranceNumber = model.InsuranceNumber,
                    };
                    _db.UpdateEmployeeProfile(employeeToUpdate);
                    
                    // Log profile update
                    _ = Task.Run(async () => await _loggerService.LogEntityUpdateAsync(userId, userName, "Employee", employeeId.ToString(), 
                        $"Updated employee profile: {model.FullName}", ipAddress, userAgent));
                }
            }

            // Handle photo upload if provided
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-photos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{model.Username}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(model.PhotoFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PhotoFile.CopyToAsync(stream);
                    }

                    var relativePath = $"/uploads/profile-photos/{fileName}";
                    
                    // Update photo path in database
                    if (model.UserType == "Controller")
                    {
                        _db.UpdateControllerPhoto(model.Username, relativePath);
                    }
                    else if (model.UserType == "Employee")
                    {
                        _db.UpdateEmployeePhoto(model.Username, relativePath);
                    }
                    
                    // Log photo update
                    _ = Task.Run(async () => await _loggerService.LogEntityUpdateAsync(userId, userName, model.UserType, model.UserId.ToString(), 
                        $"Updated profile photo", ipAddress, userAgent));
                }
                catch (Exception ex)
                {
                    // Log photo upload error but don't fail the entire update
                    _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, userName, "UpdatePhoto", "Profile", model.UserId.ToString(), 
                        "Failed to update profile photo", ipAddress, userAgent, ex.Message));
                }
            }

            // Update password separately only if a new one was provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                _db.UpdateUserPassword(model.Username, model.Password);
                
                // Log password update
                _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(userId, userName, "UpdatePassword", "System", null, 
                    "Updated user password", ipAddress, userAgent));
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            // Log error
            _ = Task.Run(async () => await _loggerService.LogErrorAsync(userId, userName, "UpdateProfile", "Profile", model.UserId.ToString(), 
                "Failed to update profile", ipAddress, userAgent, ex.Message));
            
            ModelState.AddModelError("", "An error occurred while updating: " + ex.Message);
            var repopulatedModel = _db.GetProfileDataByUsername(model.Username);
            return View(repopulatedModel);
        }
    }

    [Authorize]
    public IActionResult SomeAction()
    {
        ViewBag.Database = _db; // افترض أن _db هو حقل في الـ Controller
        return View();
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    #region Export Methods

    [HttpGet]
    public async Task<IActionResult> ExportToExcel()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var profileData = await GetProfileDataAsync(username);
            if (profileData == null)
            {
                return NotFound("Profile data not found");
            }

            var excelBytes = GenerateExcelFile(profileData);
            var fileName = $"Profile_{username}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile to Excel");
            return StatusCode(500, "Error generating Excel file");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToPDF()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var profileData = await GetProfileDataAsync(username);
            if (profileData == null)
            {
                return NotFound("Profile data not found");
            }

            var pdfBytes = GeneratePDFFile(profileData);
            var fileName = $"Profile_{username}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile to PDF");
            return StatusCode(500, "Error generating PDF file");
        }
    }

            private async Task<ProfileViewModel> GetProfileDataAsync(string username)
        {
            try
            {
                // استخدام نفس الكود الموجود في Profile action للحصول على البيانات
                var profileViewModel = _db.GetProfileDataByUsername(username);
                return profileViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile data for export");
                return null;
            }
        }

    private byte[] GenerateExcelFile(ProfileViewModel profileData)
    {
        using var package = new OfficeOpenXml.ExcelPackage();
        
        // Profile Summary Sheet
        var summarySheet = package.Workbook.Worksheets.Add("Profile Summary");
        summarySheet.Cells[1, 1].Value = "AVIATION HR PRO - PROFILE SUMMARY";
        summarySheet.Cells[1, 1, 1, 3].Merge = true;
        summarySheet.Cells[1, 1].Style.Font.Size = 16;
        summarySheet.Cells[1, 1].Style.Font.Bold = true;
        summarySheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        // Personal Information
        summarySheet.Cells[3, 1].Value = "Personal Information";
        summarySheet.Cells[3, 1].Style.Font.Bold = true;
        summarySheet.Cells[3, 1].Style.Font.Size = 14;

        summarySheet.Cells[4, 1].Value = "Full Name:";
        summarySheet.Cells[4, 2].Value = profileData.FullName;
        summarySheet.Cells[4, 1].Style.Font.Bold = true;

        summarySheet.Cells[5, 1].Value = "Job Title:";
        summarySheet.Cells[5, 2].Value = profileData.JobTitle;
        summarySheet.Cells[5, 1].Style.Font.Bold = true;

        summarySheet.Cells[6, 1].Value = "Department:";
        summarySheet.Cells[6, 2].Value = profileData.CurrentDepartment;
        summarySheet.Cells[6, 1].Style.Font.Bold = true;

        summarySheet.Cells[7, 1].Value = "Email:";
        summarySheet.Cells[7, 2].Value = profileData.Email;
        summarySheet.Cells[7, 1].Style.Font.Bold = true;

        summarySheet.Cells[8, 1].Value = "Phone:";
        summarySheet.Cells[8, 2].Value = profileData.PhoneNumber;
        summarySheet.Cells[8, 1].Style.Font.Bold = true;

        summarySheet.Cells[9, 1].Value = "Hire Date:";
        summarySheet.Cells[9, 2].Value = profileData.HireDate?.ToString("dd MMM, yyyy");
        summarySheet.Cells[9, 1].Style.Font.Bold = true;

        // Financial Information
        summarySheet.Cells[11, 1].Value = "Financial Information";
        summarySheet.Cells[11, 1].Style.Font.Bold = true;
        summarySheet.Cells[11, 1].Style.Font.Size = 14;

        if (profileData.CurrentSalary.HasValue)
        {
            summarySheet.Cells[12, 1].Value = "Current Salary:";
            summarySheet.Cells[12, 2].Value = profileData.CurrentSalary.Value.ToString("C");
            summarySheet.Cells[12, 1].Style.Font.Bold = true;
        }

        if (profileData.SalaryAfterAnnualIncrease.HasValue)
        {
            summarySheet.Cells[13, 1].Value = "Salary After Increase:";
            summarySheet.Cells[13, 2].Value = profileData.SalaryAfterAnnualIncrease.Value.ToString("C");
            summarySheet.Cells[13, 1].Style.Font.Bold = true;
        }

        // Licenses Sheet
        if (profileData.Licenses?.Any() == true)
        {
            var licensesSheet = package.Workbook.Worksheets.Add("Licenses");
            licensesSheet.Cells[1, 1].Value = "Licenses";
            licensesSheet.Cells[1, 1].Style.Font.Bold = true;
            licensesSheet.Cells[1, 1].Style.Font.Size = 14;

            licensesSheet.Cells[2, 1].Value = "Type";
            licensesSheet.Cells[2, 2].Value = "Issue Date";
            licensesSheet.Cells[2, 3].Value = "Expiry Date";
            licensesSheet.Cells[2, 1, 2, 3].Style.Font.Bold = true;

            for (int i = 0; i < profileData.Licenses.Count; i++)
            {
                var license = profileData.Licenses[i];
                licensesSheet.Cells[i + 3, 1].Value = license.TypeName;
                licensesSheet.Cells[i + 3, 2].Value = license.IssueDate?.ToString("dd MMM, yyyy");
                licensesSheet.Cells[i + 3, 3].Value = license.ExpiryDate?.ToString("dd MMM, yyyy");
            }
        }

        // Certificates Sheet
        if (profileData.Certificates?.Any() == true)
        {
            var certsSheet = package.Workbook.Worksheets.Add("Certificates");
            certsSheet.Cells[1, 1].Value = "Certificates";
            certsSheet.Cells[1, 1].Style.Font.Bold = true;
            certsSheet.Cells[1, 1].Style.Font.Size = 14;

            certsSheet.Cells[2, 1].Value = "Type";
            certsSheet.Cells[2, 2].Value = "Title";
            certsSheet.Cells[2, 3].Value = "Status";
            certsSheet.Cells[2, 1, 2, 3].Style.Font.Bold = true;

            for (int i = 0; i < profileData.Certificates.Count; i++)
            {
                var cert = profileData.Certificates[i];
                certsSheet.Cells[i + 3, 1].Value = cert.TypeName;
                certsSheet.Cells[i + 3, 2].Value = cert.Title;
                certsSheet.Cells[i + 3, 3].Value = cert.Status;
            }
        }

        // Observations Sheet
        if (profileData.Observations?.Any() == true)
        {
            var observationsSheet = package.Workbook.Worksheets.Add("Observations");
            observationsSheet.Cells[1, 1].Value = "Observations & Training";
            observationsSheet.Cells[1, 1].Style.Font.Bold = true;
            observationsSheet.Cells[1, 1].Style.Font.Size = 14;

            observationsSheet.Cells[2, 1].Value = "Travel Country";
            observationsSheet.Cells[2, 2].Value = "Duration (Days)";
            observationsSheet.Cells[2, 3].Value = "Notes";
            observationsSheet.Cells[2, 1, 2, 3].Style.Font.Bold = true;

            for (int i = 0; i < profileData.Observations.Count; i++)
            {
                var obs = profileData.Observations[i];
                observationsSheet.Cells[i + 3, 1].Value = obs.TravelCountry ?? "-";
                observationsSheet.Cells[i + 3, 2].Value = obs.DurationDays?.ToString() ?? "-";
                observationsSheet.Cells[i + 3, 3].Value = obs.Notes ?? "-";
            }
        }



        // Auto-fit columns
        foreach (var worksheet in package.Workbook.Worksheets)
        {
            worksheet.Cells.AutoFitColumns();
        }

        return package.GetAsByteArray();
    }

    private byte[] GeneratePDFFile(ProfileViewModel profileData)
    {
        try
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            
            // إنشاء PDF بسيط باستخدام QuestPDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    
                    page.Content().Column(column =>
                    {
                        // Title
                        column.Item().AlignCenter().Text("AVIATION HR PRO - PROFILE SUMMARY")
                            .FontSize(20)
                            .Bold();
                        
                        column.Item().Height(20);
                        
                        // Personal Information
                        column.Item().Text("Personal Information").FontSize(16).Bold();
                        column.Item().Text($"Full Name: {profileData.FullName ?? "-"}").FontSize(12);
                        column.Item().Text($"Job Title: {profileData.JobTitle ?? "-"}").FontSize(12);
                        column.Item().Text($"Department: {profileData.CurrentDepartment ?? "-"}").FontSize(12);
                        column.Item().Text($"Email: {profileData.Email ?? "-"}").FontSize(12);
                        column.Item().Text($"Phone: {profileData.PhoneNumber ?? "-"}").FontSize(12);
                        column.Item().Text($"Hire Date: {profileData.HireDate?.ToString("dd MMM, yyyy") ?? "-"}").FontSize(12);
                        
                        column.Item().Height(20);
                        
                        // Financial Information
                        if (profileData.CurrentSalary.HasValue || profileData.SalaryAfterAnnualIncrease.HasValue)
                        {
                            column.Item().Text("Financial Information").FontSize(16).Bold();
                            if (profileData.CurrentSalary.HasValue)
                                column.Item().Text($"Current Salary: {profileData.CurrentSalary.Value:C}").FontSize(12);
                            if (profileData.SalaryAfterAnnualIncrease.HasValue)
                                column.Item().Text($"Salary After Increase: {profileData.SalaryAfterAnnualIncrease.Value:C}").FontSize(12);
                            column.Item().Height(20);
                        }
                        
                        // Licenses
                        if (profileData.Licenses?.Any() == true)
                        {
                            column.Item().Text("Licenses").FontSize(16).Bold();
                            foreach (var license in profileData.Licenses)
                            {
                                column.Item().Text($"• {license.TypeName ?? "-"} | Issue: {license.IssueDate?.ToString("dd MMM, yyyy") ?? "-"} | Expiry: {license.ExpiryDate?.ToString("dd MMM, yyyy") ?? "-"}").FontSize(12);
                            }
                            column.Item().Height(20);
                        }
                        
                        // Certificates
                        if (profileData.Certificates?.Any() == true)
                        {
                            column.Item().Text("Certificates").FontSize(16).Bold();
                            foreach (var cert in profileData.Certificates)
                            {
                                column.Item().Text($"• {cert.TypeName ?? "-"} | Title: {cert.Title ?? "-"} | Status: {cert.Status ?? "-"}").FontSize(12);
                            }
                            column.Item().Height(20);
                        }
                        
                        // Observations
                        if (profileData.Observations?.Any() == true)
                        {
                            column.Item().Text("Observations & Training").FontSize(16).Bold();
                            foreach (var obs in profileData.Observations)
                            {
                                column.Item().Text($"• Country: {obs.TravelCountry ?? "-"} | Duration: {obs.DurationDays?.ToString() ?? "-"} days | Notes: {obs.Notes ?? "-"}").FontSize(12);
                            }
                            column.Item().Height(20);
                        }
                        
                        // Footer
                        column.Item().Height(20);
                        column.Item().AlignCenter().Text($"Generated on: {DateTime.Now:dd MMM, yyyy HH:mm}")
                            .FontSize(10);
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF file");
            throw new Exception("Error generating PDF file", ex);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendBulkEmailNotifications([FromBody] BulkEmailRequest request)
    {
        try
        {
            // تم إزالة فحص الصلاحيات - جميع المستخدمين يمكنهم إرسال الإيميلات
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name ?? "Unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            _logger.LogInformation("User {UserName} (ID: {UserId}) is attempting to send bulk emails", userName, userId);

            // التحقق من وجود صفوف محددة
            if (request.SelectedIds == null || request.SelectedIds.Count == 0)
            {
                return BadRequest(new { success = false, message = "لم يتم تحديد أي صفوف" });
            }

            // بناء استعلام SQL للصفوف المحددة فقط
            string sqlQuery = @"
                    SELECT 
                        n.NotificationId,
                        n.userid,
                        n.controllerid,
                        COALESCE(c.fullname, e.fullname, 'Unknown') AS ControllerName,
                        n.message,
                        n.link,
                        n.created_at,
                        n.is_read,
                        n.note,
                        n.licensetype,
                        n.licenseexpirydate,
                        CASE 
                            WHEN n.licenseexpirydate IS NOT NULL THEN DATEDIFF(day, GETDATE(), n.licenseexpirydate)
                            ELSE NULL
                        END AS RemainingDays,
                        CASE 
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN 'Suspended'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN 'Critical'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN 'Warning'
                            ELSE 'Unknown'
                        END AS Status,
                        CASE 
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 0 THEN '#dc3545'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) <= 30 THEN '#ffc107'
                            WHEN DATEDIFF(day, GETDATE(), n.licenseexpirydate) > 30 THEN '#28a745'
                            ELSE '#6c757d'
                        END AS StatusColor,
                        COALESCE(c.phone_number, e.phonenumber, 'N/A') AS phone_number,
                        COALESCE(c.email, e.email, 'N/A') AS email,
                        COALESCE(c.current_department, e.department, 'N/A') AS Department,
                        COALESCE(a.airportname, 'HQ - Main Office') AS airportname,
                        CASE 
                            WHEN n.controllerid IS NOT NULL THEN 'Controller'
                            ELSE 'Employee'
                        END AS UserType,
                        CASE 
                            WHEN n.licenseexpirydate IS NOT NULL THEN 'Expiring Soon'
                            ELSE 'General'
                        END AS Status
                    FROM notifications n
                    LEFT JOIN controllers c ON n.controllerid = c.controllerid
                    LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                    LEFT JOIN airports a ON c.airportid = a.airportid
                    WHERE n.NotificationId IN (" + string.Join(",", request.SelectedIds) + @")
                      AND n.licensetype IS NOT NULL 
                      AND n.licensetype != 'No License'
                      AND n.licenseexpirydate IS NOT NULL
                      AND n.licenseexpirydate != ''
                    ORDER BY n.created_at DESC";

            var notifications = _db.ExecuteQuery(sqlQuery);
            int emailsSent = 0;
            int totalEmails = notifications.Rows.Count;

            foreach (DataRow row in notifications.Rows)
            {
                try
                {
                    var emailAddress = row["email"]?.ToString();
                    var fullName = row["ControllerName"]?.ToString();
                    
                    // تسجيل البيانات للتشخيص
                    _logger.LogInformation("Processing row: Email={Email}, FullName={FullName}", emailAddress, fullName);
                    _logger.LogInformation("Row data: licensetype={LicenseType}, licenseexpirydate={ExpiryDate}, RemainingDays={RemainingDays}", 
                        row["licensetype"]?.ToString(), row["licenseexpirydate"]?.ToString(), row["RemainingDays"]?.ToString());
                    
                    if (string.IsNullOrEmpty(emailAddress))
                    {
                        _logger.LogWarning("Email address is empty for user: {FullName}", fullName);
                        continue;
                    }

                    var emailBody = GenerateCustomEmailBody(row, request.CustomMessage ?? "");
                    var emailRequest = new EmailRequest
                    {
                        To = emailAddress,
                        Subject = "تنبيه انتهاء صلاحية الرخصة - HR Aviation System",
                        Body = emailBody,
                        IsHtml = true
                    };

                    _logger.LogInformation("Sending email to {Email} with body length: {BodyLength}", emailAddress, emailBody.Length);

                    var emailResult = await _emailService.SendEmailAsync(emailRequest);
                    
                    if (emailResult)
                    {
                        emailsSent++;
                        _logger.LogInformation("✅ Email sent successfully to {Email}", emailAddress);
                    }
                    else
                    {
                        _logger.LogError("❌ Failed to send email to {Email}", emailAddress);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email to {Email}", row["email"]?.ToString());
                }
            }

            // تسجيل العملية
            _ = Task.Run(async () => await _loggerService.LogUserActivityAsync(int.Parse(userId ?? "0"), userName, "SendBulkEmails", "License", null, 
                $"Sent {emailsSent} emails out of {totalEmails} for selected rows", ipAddress, userAgent));

            return Json(new { 
                success = true, 
                message = $"تم إرسال {emailsSent} إيميل بنجاح من أصل {totalEmails}!", 
                emailsSent = emailsSent, 
                totalEmails = totalEmails 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendBulkEmailNotifications");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إرسال الإيميلات" });
        }
    }

    private string GenerateCustomEmailBody(DataRow row, string customMessage)
    {
        string fullName = row["ControllerName"]?.ToString() ?? "Unknown User";
        string licenseType = row["licensetype"]?.ToString() ?? "Unknown License";
        
        // التحقق من وجود تاريخ انتهاء الصلاحية
        DateTime expiryDate;
        if (row["licenseexpirydate"] != DBNull.Value && DateTime.TryParse(row["licenseexpirydate"].ToString(), out expiryDate))
        {
            expiryDate = Convert.ToDateTime(row["licenseexpirydate"]);
        }
        else
        {
            expiryDate = DateTime.Now.AddDays(30); // تاريخ افتراضي
        }
        
        // التحقق من وجود الأيام المتبقية
        int remainingDays = 0;
        if (row["RemainingDays"] != DBNull.Value && int.TryParse(row["RemainingDays"].ToString(), out remainingDays))
        {
            remainingDays = Convert.ToInt32(row["RemainingDays"]);
        }
        else
        {
            // حساب الأيام المتبقية يدوياً
            remainingDays = (int)(expiryDate - DateTime.Now).TotalDays;
        }
        
        string department = row["Department"]?.ToString() ?? "N/A";

        // English status text
        string statusTextEn = remainingDays < 0 
            ? $"Expired {Math.Abs(remainingDays)} days ago"
            : remainingDays == 0 
                ? "Expires today!"
                : $"Expires in {remainingDays} days";

        // Arabic status text
        string statusTextAr = remainingDays < 0 
            ? $"منتهية الصلاحية منذ {Math.Abs(remainingDays)} يوم"
            : remainingDays == 0 
                ? "تنتهي اليوم!"
                : $"تنتهي خلال {remainingDays} يوم";

        string statusColor = remainingDays < 0 ? "#dc3545" : remainingDays < 7 ? "#ffc107" : "#28a745";

        // English default message
        string defaultMessageEn = customMessage;
        if (string.IsNullOrEmpty(customMessage))
        {
            defaultMessageEn = remainingDays < 0 
                ? "Please take urgent action to renew your license."
                : "Please take the necessary actions to renew your license before it expires.";
        }

        // Arabic default message
        string defaultMessageAr = customMessage;
        if (string.IsNullOrEmpty(customMessage))
        {
            defaultMessageAr = remainingDays < 0 
                ? "يرجى اتخاذ الإجراءات العاجلة لتجديد رخصتك."
                : "يرجى اتخاذ الإجراءات اللازمة لتجديد رخصتك قبل انتهاء الصلاحية.";
        }

        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>License Expiry Alert - تنبيه انتهاء صلاحية الرخصة</title>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #147858, #28a745); color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center; }}
                    .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 10px 10px; }}
                    .alert {{ background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 15px 0; }}
                    .status {{ background: {statusColor}; color: white; padding: 8px 15px; border-radius: 20px; display: inline-block; font-weight: bold; }}
                    .details {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #147858; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #6c757d; font-size: 0.9em; }}
                    .language-section {{ margin: 20px 0; padding: 15px; background: white; border-radius: 5px; border: 1px solid #dee2e6; }}
                    .language-title {{ color: #147858; font-weight: bold; margin-bottom: 10px; font-size: 1.1em; }}
                    .arabic {{ direction: rtl; text-align: right; }}
                    .english {{ direction: ltr; text-align: left; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🚨 License Expiry Alert</h1>
                        <h2>تنبيه انتهاء صلاحية الرخصة</h2>
                    </div>
                    
                    <!-- English Section -->
                    <div class='language-section english'>
                        <div class='language-title'>🇺🇸 English</div>
                        <p>Dear <strong>{fullName}</strong>,</p>
                        
                        <div class='alert'>
                            <strong>Important Alert:</strong> Your license requires urgent renewal.
                        </div>
                        
                        <div class='details'>
                            <h3>License Details:</h3>
                            <ul>
                                <li><strong>License Type:</strong> {licenseType}</li>
                                <li><strong>Expiry Date:</strong> {expiryDate:yyyy-MM-dd}</li>
                                <li><strong>Department:</strong> {department}</li>
                                <li><strong>Status:</strong> <span class='status'>{statusTextEn}</span></li>
                            </ul>
                        </div>
                        
                        <p><strong>Message:</strong> {defaultMessageEn}</p>
                        
                        <div class='alert'>
                            <strong>Note:</strong> Please contact the Human Resources Department or your direct manager to renew your license as soon as possible.
                        </div>
                    </div>

                    <!-- Arabic Section -->
                    <div class='language-section arabic'>
                        <div class='language-title'>🇸🇦 العربية</div>
                        <p>عزيزي/عزيزتي <strong>{fullName}</strong>،</p>
                        
                        <div class='alert'>
                            <strong>تنبيه مهم:</strong> رخصتك تحتاج إلى تجديد عاجل.
                        </div>
                        
                        <div class='details'>
                            <h3>تفاصيل الرخصة:</h3>
                            <ul>
                                <li><strong>نوع الرخصة:</strong> {licenseType}</li>
                                <li><strong>تاريخ انتهاء الصلاحية:</strong> {expiryDate:yyyy-MM-dd}</li>
                                <li><strong>القسم:</strong> {department}</li>
                                <li><strong>الحالة:</strong> <span class='status'>{statusTextAr}</span></li>
                            </ul>
                        </div>
                        
                        <p><strong>رسالة:</strong> {defaultMessageAr}</p>
                        
                        <div class='alert'>
                            <strong>ملاحظة:</strong> يرجى التواصل مع قسم الموارد البشرية أو مديرك المباشر لتجديد رخصتك في أقرب وقت ممكن.
                        </div>
                    </div>
                    
                    <div class='footer'>
                        <p>This email was automatically sent from the Aviation HR Management System</p>
                        <p>هذا الإيميل تم إرساله تلقائياً من نظام إدارة الموارد البشرية للطيران</p>
                        <p>© {DateTime.Now.Year} Aviation HR Pro - All Rights Reserved - جميع الحقوق محفوظة</p>
                    </div>
                </div>
            </body>
            </html>";
    }


    #endregion
}
