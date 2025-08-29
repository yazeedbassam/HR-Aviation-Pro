using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using WebApplication1.Models;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AdvancedPermissionController : Controller
    {
        private readonly IAdvancedPermissionService _permissionService;
        private readonly ILogger<AdvancedPermissionController> _logger;

        public AdvancedPermissionController(IAdvancedPermissionService permissionService, ILogger<AdvancedPermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: /AdvancedPermission/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var permissionSummary = await _permissionService.GetUserPermissionSummaryAsync(userId);
                var visibleMenuItems = await _permissionService.GetVisibleMenuItemsAsync(userId);
                var accessibleDepartments = await _permissionService.GetAccessibleDepartmentIdsAsync(userId);

                var viewModel = new AdvancedPermissionDashboardViewModel
                {
                    CurrentUserId = userId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading advanced permission dashboard");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل لوحة التحكم";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /AdvancedPermission/UserPermissions/{userId}
        public async Task<IActionResult> UserPermissions(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user can access target user's data
                if (!await _permissionService.CanAccessUserDataAsync(currentUserId, userId))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية للوصول إلى بيانات هذا المستخدم";
                    return RedirectToAction("Dashboard");
                }

                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                var accessibleDepartments = await _permissionService.GetAccessibleDepartmentIdsAsync(userId);

                var viewModel = new UserPermissionsViewModel
                {
                    UserId = userId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user permissions for user {UserId}", userId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل صلاحيات المستخدم";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: /AdvancedPermission/UserPermissionManager/{userId}
        public async Task<IActionResult> UserPermissionManager(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get user information
                var users = await _permissionService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.UserId == userId);
                
                if (user == null)
                {
                    TempData["Error"] = "المستخدم غير موجود";
                    return RedirectToAction("Dashboard");
                }

                // Get user permissions
                var permissions = await _permissionService.GetUserDetailedPermissionsAsync(userId);
                
                // Get dropdown data
                var allPermissions = await _permissionService.GetAllPermissionsAsync();
                var allDepartments = await _permissionService.GetAllDepartmentsAsync();

                // Log the data for debugging
                _logger.LogInformation("Getting permissions: {Count} permissions found", allPermissions?.Count ?? 0);
                _logger.LogInformation("Getting departments: {Count} departments found", allDepartments?.Count ?? 0);

                // Ensure we have data for dropdowns
                if (allPermissions == null || !allPermissions.Any())
                {
                    _logger.LogWarning("No permissions found for dropdown - using hardcoded list");
                    allPermissions = new List<PermissionInfo>
                    {
                        new PermissionInfo { PermissionId = 1, PermissionName = "عرض لوحة التحكم", PermissionKey = "DASHBOARD_VIEW" },
                        new PermissionInfo { PermissionId = 2, PermissionName = "عرض المنظمة", PermissionKey = "ORGANIZATION_VIEW" },
                        new PermissionInfo { PermissionId = 3, PermissionName = "عرض الهيكل", PermissionKey = "STRUCTURE_VIEW" },
                        new PermissionInfo { PermissionId = 4, PermissionName = "عرض الأقسام", PermissionKey = "DIVISIONS_VIEW" },
                        new PermissionInfo { PermissionId = 5, PermissionName = "عرض الموظفين", PermissionKey = "STAFF_VIEW" },
                        new PermissionInfo { PermissionId = 6, PermissionName = "عرض المراقبين", PermissionKey = "CONTROLLERS_VIEW" },
                        new PermissionInfo { PermissionId = 7, PermissionName = "عرض AIS", PermissionKey = "AIS_VIEW" },
                        new PermissionInfo { PermissionId = 8, PermissionName = "عرض CNS", PermissionKey = "CNS_VIEW" },
                        new PermissionInfo { PermissionId = 9, PermissionName = "عرض AFTN", PermissionKey = "AFTN_VIEW" },
                        new PermissionInfo { PermissionId = 10, PermissionName = "عرض Ops Staff", PermissionKey = "OPS_STAFF_VIEW" },
                        new PermissionInfo { PermissionId = 11, PermissionName = "عرض الرخص", PermissionKey = "LICENSES_VIEW" },
                        new PermissionInfo { PermissionId = 12, PermissionName = "عرض الشهادات", PermissionKey = "CERTIFICATES_VIEW" },
                        new PermissionInfo { PermissionId = 13, PermissionName = "عرض الملاحظات", PermissionKey = "OBSERVATIONS_VIEW" },
                        new PermissionInfo { PermissionId = 14, PermissionName = "عرض الدورات", PermissionKey = "COURSES_VIEW" },
                        new PermissionInfo { PermissionId = 15, PermissionName = "إدارة الصلاحيات", PermissionKey = "PERMISSIONS_MANAGE" },
                        new PermissionInfo { PermissionId = 16, PermissionName = "إعدادات النظام", PermissionKey = "SYSTEM_SETTINGS_VIEW" },
                        new PermissionInfo { PermissionId = 17, PermissionName = "إدارة التكوين", PermissionKey = "CONFIGURATION_MANAGEMENT" },
                        new PermissionInfo { PermissionId = 18, PermissionName = "إدارة الأدوار", PermissionKey = "ROLES_MANAGEMENT" },
                        new PermissionInfo { PermissionId = 19, PermissionName = "عرض جميع المستخدمين", PermissionKey = "USERS_VIEW_ALL" },
                        new PermissionInfo { PermissionId = 20, PermissionName = "إضافة", PermissionKey = "ADD" },
                        new PermissionInfo { PermissionId = 21, PermissionName = "تعديل", PermissionKey = "EDIT" },
                        new PermissionInfo { PermissionId = 22, PermissionName = "حذف", PermissionKey = "DELETE" },
                        new PermissionInfo { PermissionId = 23, PermissionName = "تصدير", PermissionKey = "EXPORT" }
                    };
                }

                if (allDepartments == null || !allDepartments.Any())
                {
                    _logger.LogWarning("No departments found for dropdown - using hardcoded list");
                    allDepartments = new List<DepartmentInfo>
                    {
                        new DepartmentInfo { DepartmentId = 1, DepartmentName = "OJAI", Description = "Queen Alia International Airport" },
                        new DepartmentInfo { DepartmentId = 2, DepartmentName = "OJAM", Description = "Amman Civil Airport" },
                        new DepartmentInfo { DepartmentId = 3, DepartmentName = "OJAQ", Description = "Aqaba Airport" },
                        new DepartmentInfo { DepartmentId = 4, DepartmentName = "CARC", Description = "Civil Aviation Regulatory Commission" },
                        new DepartmentInfo { DepartmentId = 5, DepartmentName = "TACC", Description = "Training and Air Traffic Control Center" },
                        new DepartmentInfo { DepartmentId = 6, DepartmentName = "HQ", Description = "Headquarters - Main Office" },
                        new DepartmentInfo { DepartmentId = 7, DepartmentName = "AIS", Description = "Aeronautical Information Service" },
                        new DepartmentInfo { DepartmentId = 8, DepartmentName = "CNS", Description = "Communications, Navigation and Surveillance" },
                        new DepartmentInfo { DepartmentId = 9, DepartmentName = "AFTN", Description = "Aeronautical Fixed Telecommunication Network" },
                        new DepartmentInfo { DepartmentId = 10, DepartmentName = "Ops Staff", Description = "Operations Staff & Administration" }
                    };
                }

                ViewBag.Permissions = allPermissions.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.PermissionId.ToString(),
                    Text = p.PermissionName
                }).ToList();

                ViewBag.Departments = allDepartments.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();

                var viewModel = new UserPermissionManagerViewModel
                {
                    CurrentUserId = userId,
                    CurrentUserName = user.UserName,
                    Users = new List<UserInfo> { user },
                    Permissions = allPermissions,
                    Departments = allDepartments,
                    UserPermissions = permissions
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user permission manager for user {UserId}", userId);
                TempData["Error"] = "حدث خطأ أثناء تحميل إدارة صلاحيات المستخدم";
                return RedirectToAction("Dashboard");
            }
        }

        // POST: /AdvancedPermission/AddUserPermission
        [HttpPost]
        public async Task<IActionResult> AddUserPermission(int userId, int permissionId, int departmentId)
        {
            try
            {
                var result = await _permissionService.AddUserPermissionAsync(userId, permissionId, departmentId);
                if (result)
                {
                    TempData["Success"] = "تم إضافة الصلاحية بنجاح";
                }
                else
                {
                    TempData["Error"] = "فشل في إضافة الصلاحية";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user permission: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
                TempData["Error"] = "حدث خطأ أثناء إضافة الصلاحية";
            }

            return RedirectToAction("UserPermissionManager", new { userId });
        }

        // POST: /AdvancedPermission/RemoveUserPermission
        [HttpPost]
        public async Task<IActionResult> RemoveUserPermission(int userId, int permissionId)
        {
            try
            {
                var result = await _permissionService.RemoveUserPermissionAsync(userId, permissionId);
                if (result)
                {
                    TempData["Success"] = "تم إزالة الصلاحية بنجاح";
                }
                else
                {
                    TempData["Error"] = "فشل في إزالة الصلاحية";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user permission: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
                TempData["Error"] = "حدث خطأ أثناء إزالة الصلاحية";
            }

            return RedirectToAction("UserPermissionManager", new { userId });
        }

        // GET: /AdvancedPermission/DataAccess
        public async Task<IActionResult> DataAccess()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var accessibleUserIds = await _permissionService.GetAccessibleUserIdsAsync(userId);
                var accessibleDepartmentIds = await _permissionService.GetAccessibleDepartmentIdsAsync(userId);

                var viewModel = new DataAccessViewModel();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data access information for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل معلومات الوصول للبيانات";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: /AdvancedPermission/MenuVisibility
        public async Task<IActionResult> MenuVisibility()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var visibleMenuItems = await _permissionService.GetVisibleMenuItemsAsync(userId);

                var viewModel = new MenuVisibilityViewModel();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu visibility for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل رؤية القائمة";
                return RedirectToAction("Dashboard");
            }
        }

        // POST: /AdvancedPermission/ClearCache
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ClearCache()
        {
            try
            {
                _permissionService.ClearAllCache();
                TempData["SuccessMessage"] = "تم مسح الذاكرة المؤقتة بنجاح";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                TempData["ErrorMessage"] = "حدث خطأ أثناء مسح الذاكرة المؤقتة";
                return RedirectToAction("Dashboard");
            }
        }

        // POST: /AdvancedPermission/ClearUserCache/{userId}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ClearUserCache(int userId)
        {
            try
            {
                _permissionService.ClearUserCache(userId);
                TempData["SuccessMessage"] = $"تم مسح الذاكرة المؤقتة للمستخدم {userId} بنجاح";
                return RedirectToAction("UserPermissions", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء مسح الذاكرة المؤقتة للمستخدم";
                return RedirectToAction("UserPermissions", new { userId });
            }
        }

        // =====================================================
        // DETAILED USER PERMISSION MANAGEMENT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> UserPermissionManager(int? userId = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var viewModel = new UserPermissionManagerViewModel
                {
                    CurrentUserId = currentUserId,
                    CurrentUserName = GetCurrentUsername(),
                    Users = await _permissionService.GetAllUsersAsync(),
                    Permissions = await _permissionService.GetAllPermissionsAsync(),
                    Departments = await _permissionService.GetAllDepartmentsAsync()
                };

                if (userId.HasValue)
                {
                    viewModel.UserPermissions = await _permissionService.GetUserDetailedPermissionsAsync(userId.Value);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserPermissionManager");
                return RedirectToAction("Dashboard").WithDanger("حدث خطأ أثناء تحميل صفحة إدارة الصلاحيات");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserPermissions(int userId, List<UserPermissionUpdateModel> permissions)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Check if current user has permission to manage other users
                if (!await _permissionService.HasPermissionAsync(currentUserId, "USERS_MANAGE_PERMISSIONS"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة صلاحيات المستخدمين" });
                }

                var result = await _permissionService.UpdateUserPermissionsAsync(userId, permissions);
                
                if (result)
                {
                    // Clear cache for the updated user
                    _permissionService.ClearUserCache(userId);
                    
                    return Json(new { success = true, message = "تم تحديث صلاحيات المستخدم بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تحديث صلاحيات المستخدم" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permissions for user {UserId}", userId);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الصلاحيات" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PermissionMatrix()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MATRIX_VIEW"))
                {
                    return RedirectToAction("Dashboard").WithWarning("ليس لديك صلاحية لعرض مصفوفة الصلاحيات");
                }

                var viewModel = new PermissionMatrixViewModel
                {
                    Users = await _permissionService.GetAllUsersAsync(),
                    Permissions = await _permissionService.GetAllPermissionsAsync(),
                    PermissionMatrix = await _permissionService.GetPermissionMatrixAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PermissionMatrix");
                return RedirectToAction("Dashboard").WithDanger("حدث خطأ أثناء تحميل مصفوفة الصلاحيات");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BulkPermissionUpdate()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_BULK_UPDATE"))
                {
                    return RedirectToAction("Dashboard").WithWarning("ليس لديك صلاحية لتحديث الصلاحيات المجمعة");
                }

                var viewModel = new BulkPermissionUpdateViewModel
                {
                    UserIds = new List<int>(),
                    PermissionKeys = new List<string>(),
                    DepartmentIds = new List<int>(),
                    RoleNames = new List<string>(),
                    GrantPermission = true,
                    UpdateReason = string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkPermissionUpdate");
                return RedirectToAction("Dashboard").WithDanger("حدث خطأ أثناء تحميل صفحة التحديث المجمع");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteBulkPermissionUpdate(BulkPermissionUpdateViewModel model)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_BULK_UPDATE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لتحديث الصلاحيات المجمعة" });
                }

                var result = await _permissionService.ExecuteBulkPermissionUpdateAsync(model);
                
                if (result)
                {
                    return Json(new { 
                        success = true, 
                        message = "تم تحديث الصلاحيات بنجاح"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تحديث الصلاحيات" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteBulkPermissionUpdate");
                return Json(new { success = false, message = "حدث خطأ أثناء التحديث المجمع" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PermissionTemplates()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_TEMPLATES_MANAGE"))
                {
                    return RedirectToAction("Dashboard").WithWarning("ليس لديك صلاحية لإدارة قوالب الصلاحيات");
                }

                var viewModel = new PermissionTemplatesViewModel
                {
                    Templates = await _permissionService.GetPermissionTemplatesAsync(),
                    AvailablePermissions = await _permissionService.GetAllPermissionsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PermissionTemplates");
                return RedirectToAction("Dashboard").WithDanger("حدث خطأ أثناء تحميل قوالب الصلاحيات");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePermissionTemplate(Models.PermissionTemplateModel template)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_TEMPLATES_MANAGE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة قوالب الصلاحيات" });
                }

                var result = await _permissionService.SavePermissionTemplateAsync(template);
                
                if (result)
                {
                    return Json(new { success = true, message = "تم حفظ قالب الصلاحيات بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في حفظ قالب الصلاحيات" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving permission template");
                return Json(new { success = false, message = "حدث خطأ أثناء حفظ قالب الصلاحيات" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyPermissionTemplate(int userId, string templateName)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_TEMPLATES_APPLY"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لتطبيق قوالب الصلاحيات" });
                }

                var result = await _permissionService.ApplyPermissionTemplateAsync(userId, templateName);
                
                if (result)
                {
                    // Clear cache for the updated user
                    _permissionService.ClearUserCache(userId);

                    return Json(new { 
                        success = true, 
                        message = "تم تطبيق قالب الصلاحيات بنجاح" 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تطبيق قالب الصلاحيات" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying permission template");
                return Json(new { success = false, message = "حدث خطأ أثناء تطبيق قالب الصلاحيات" });
            }
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }

        private string GetCurrentUsername()
        {
            return User.Identity?.Name ?? "";
        }
    }

    // =====================================================
    // EXTENSION METHODS
    // =====================================================

    public static class ControllerExtensions
    {
        public static IActionResult WithSuccess(this IActionResult result, string message)
        {
            return result;
        }

        public static IActionResult WithWarning(this IActionResult result, string message)
        {
            return result;
        }

        public static IActionResult WithDanger(this IActionResult result, string message)
        {
            return result;
        }
    }

} 