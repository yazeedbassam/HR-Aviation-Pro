using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CacheMonitorController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheMonitorController> _logger;

        public CacheMonitorController(ICacheService cacheService, ILogger<CacheMonitorController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var statistics = _cacheService.GetStatistics();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                await _cacheService.ClearAllAsync();
                _logger.LogInformation("Cache cleared by user {UserName}", User.Identity?.Name);
                
                TempData["SuccessMessage"] = "تم مسح الكاش بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                TempData["ErrorMessage"] = "حدث خطأ أثناء مسح الكاش";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearByPattern(string pattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    TempData["ErrorMessage"] = "يجب تحديد نمط للبحث";
                    return RedirectToAction(nameof(Index));
                }

                await _cacheService.RemoveByPatternAsync(pattern);
                _logger.LogInformation("Cache cleared by pattern '{Pattern}' by user {UserName}", pattern, User.Identity?.Name);
                
                TempData["SuccessMessage"] = $"تم مسح الكاش بنمط '{pattern}' بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache by pattern: {Pattern}", pattern);
                TempData["ErrorMessage"] = "حدث خطأ أثناء مسح الكاش";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCache()
        {
            try
            {
                // This would typically refresh specific cache entries
                // For now, we'll just clear and let them rebuild
                await _cacheService.ClearAllAsync();
                _logger.LogInformation("Cache refreshed by user {UserName}", User.Identity?.Name);
                
                TempData["SuccessMessage"] = "تم تحديث الكاش بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث الكاش";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult GetStatistics()
        {
            try
            {
                var statistics = _cacheService.GetStatistics();
                return Json(new
                {
                    success = true,
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء جلب إحصائيات الكاش"
                });
            }
        }
    }
} 