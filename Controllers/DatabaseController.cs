using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Services;
using System.Data;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// وحدة تحكم لإدارة قاعدة البيانات
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DatabaseController : Controller
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(
            IDatabaseService databaseService,
            ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// صفحة إدارة قاعدة البيانات الرئيسية
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var databaseInfo = await _databaseService.GetDatabaseInfoAsync();
                var currentDatabase = _databaseService.GetCurrentDatabase();
                
                ViewBag.CurrentDatabase = currentDatabase;
                ViewBag.DatabaseInfo = databaseInfo;
                ViewBag.AvailableDatabases = new[] { "SqlServer", "Supabase" };
                
                return View(databaseInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Database Index");
                TempData["Error"] = "حدث خطأ أثناء جلب معلومات قاعدة البيانات";
                return View(new DatabaseInfo { Status = "Error" });
            }
        }

        /// <summary>
        /// تبديل قاعدة البيانات
        /// </summary>
        [HttpPost]
        public IActionResult SwitchDatabase(string databaseType)
        {
            try
            {
                if (string.IsNullOrEmpty(databaseType))
                {
                    TempData["Error"] = "نوع قاعدة البيانات مطلوب";
                    return RedirectToAction(nameof(Index));
                }

                _databaseService.SwitchDatabase(databaseType);
                
                TempData["Success"] = $"تم تبديل قاعدة البيانات إلى {databaseType} بنجاح";
                _logger.LogInformation("Database switched to {DatabaseType} by user {User}", databaseType, User.Identity?.Name);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching database to {DatabaseType}", databaseType);
                TempData["Error"] = $"حدث خطأ أثناء تبديل قاعدة البيانات: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// اختبار الاتصال بقاعدة البيانات
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var isAvailable = await _databaseService.IsDatabaseAvailableAsync();
                var currentDatabase = _databaseService.GetCurrentDatabase();
                
                if (isAvailable)
                {
                    TempData["Success"] = $"اتصال قاعدة البيانات {currentDatabase} يعمل بشكل صحيح";
                }
                else
                {
                    TempData["Warning"] = $"اتصال قاعدة البيانات {currentDatabase} غير متاح";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing database connection");
                TempData["Error"] = $"حدث خطأ أثناء اختبار الاتصال: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// محاولة التبديل التلقائي عند الفشل
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TryAutoSwitch()
        {
            try
            {
                var switched = await _databaseService.TryAutoSwitchOnFailureAsync();
                
                if (switched)
                {
                    var newDatabase = _databaseService.GetCurrentDatabase();
                    TempData["Success"] = $"تم التبديل التلقائي إلى قاعدة البيانات {newDatabase} بنجاح";
                }
                else
                {
                    TempData["Warning"] = "فشل التبديل التلقائي، قاعدة البيانات الحالية غير متاحة";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-switch");
                TempData["Error"] = $"حدث خطأ أثناء التبديل التلقائي: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// الحصول على معلومات قاعدة البيانات كـ JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var databaseInfo = await _databaseService.GetDatabaseInfoAsync();
                return Json(new { success = true, data = databaseInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info as JSON");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على حالة قاعدة البيانات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var isAvailable = await _databaseService.IsDatabaseAvailableAsync();
                var currentDatabase = _databaseService.GetCurrentDatabase();
                
                return Json(new
                {
                    success = true,
                    isAvailable = isAvailable,
                    currentDatabase = currentDatabase,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database status");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
} 