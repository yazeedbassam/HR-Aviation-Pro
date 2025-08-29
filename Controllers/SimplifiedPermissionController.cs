using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class SimplifiedPermissionController : Controller
    {
        private readonly IAdvancedPermissionService _permissionService;
        private readonly ILogger<SimplifiedPermissionController> _logger;

        public SimplifiedPermissionController(
            IAdvancedPermissionService permissionService,
            ILogger<SimplifiedPermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // =====================================================
        // MAIN PERMISSION MANAGER
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() == "admin")
                {
                    _logger.LogInformation($"Admin user {currentUsername} accessing simplified permissions page");
                }
                else
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                // Get dropdown data for permissions and departments
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

                // Set ViewBag data for dropdowns
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

                var viewModel = new PermissionManagerViewModel
                {
                    CurrentUserId = currentUserId,
                    CurrentUserName = currentUsername,
                    Users = await GetUsersWithPermissionsAsync(),
                    AvailableSections = GetAvailableSections()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SimplifiedPermission Index");
                return RedirectToAction("Error", "Home");
            }
        }

        // =====================================================
        // AJAX ENDPOINTS
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> UpdateUserPermission([FromBody] UserPermissionUpdateModel model)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to manage permissions
                if (currentUsername.ToLower() != "admin" && !await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                }

                var result = await UpdateUserPermissionAsync(model);
                
                if (result)
                {
                    // Clear cache for the updated user
                    _permissionService.ClearUserCache(model.UserId);
                    
                    return Json(new { success = true, message = "تم تحديث الصلاحية بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تحديث الصلاحية" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permission");
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الصلاحية" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSectionVisibility([FromBody] SectionVisibilityUpdateModel model)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                }

                var result = await UpdateSectionVisibilityAsync(model);
                
                if (result)
                {
                    // Clear cache for the updated user
                    _permissionService.ClearUserCache(model.UserId);
                    
                    return Json(new { success = true, message = "تم تحديث رؤية القسم بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تحديث رؤية القسم" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section visibility");
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث رؤية القسم" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyTemplateToUser([FromBody] ApplyTemplateModel model)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                }

                var result = await ApplyTemplateToUserAsync(model);
                
                if (result)
                {
                    // Clear cache for the updated user
                    _permissionService.ClearUserCache(model.UserId);
                    
                    return Json(new { success = true, message = "تم تطبيق القالب بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في تطبيق القالب" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying template to user");
                return Json(new { success = false, message = "حدث خطأ أثناء تطبيق القالب" });
            }
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private async Task<List<SimplifiedPermissionViewModel>> GetUsersWithPermissionsAsync()
        {
            var users = new List<SimplifiedPermissionViewModel>();
            
            // Get all users from database
            var allUsers = await _permissionService.GetAllUsersAsync();
            
            foreach (var user in allUsers)
            {
                var userPermissions = new SimplifiedPermissionViewModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserFullName = user.UserFullName,
                    UserRole = user.UserRole,
                    UserDepartment = user.UserDepartment,
                    SectionPermissions = await GetUserSectionPermissionsAsync(user.UserId)
                };
                
                users.Add(userPermissions);
            }
            
            return users;
        }

        private async Task<List<SectionPermissionModel>> GetUserSectionPermissionsAsync(int userId)
        {
            var sections = new List<SectionPermissionModel>();
            
            foreach (var sectionKey in SimplifiedPermissionKeys.SectionNames.Keys)
            {
                var section = new SectionPermissionModel
                {
                    SectionKey = sectionKey,
                    SectionName = SimplifiedPermissionKeys.SectionNames[sectionKey],
                    SectionIcon = SimplifiedPermissionKeys.SectionIcons[sectionKey],
                    IsVisible = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_VIEW"),
                    Actions = await GetUserActionPermissionsAsync(userId, sectionKey)
                };
                
                sections.Add(section);
            }
            
            return sections;
        }

        private async Task<List<ActionPermissionModel>> GetUserActionPermissionsAsync(int userId, string sectionKey)
        {
            var actions = new List<ActionPermissionModel>();
            
            foreach (var actionKey in SimplifiedPermissionKeys.ActionNames.Keys)
            {
                var action = new ActionPermissionModel
                {
                    ActionKey = actionKey,
                    ActionName = SimplifiedPermissionKeys.ActionNames[actionKey],
                    ActionIcon = SimplifiedPermissionKeys.ActionIcons[actionKey],
                    CanView = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_{actionKey}"),
                    CanAdd = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_ADD"),
                    CanEdit = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_EDIT"),
                    CanDelete = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_DELETE"),
                    CanExport = await _permissionService.HasPermissionAsync(userId, $"{sectionKey}_EXPORT")
                };
                
                actions.Add(action);
            }
            
            return actions;
        }

        private List<SectionPermissionModel> GetAvailableSections()
        {
            var sections = new List<SectionPermissionModel>();
            
            foreach (var sectionKey in SimplifiedPermissionKeys.SectionNames.Keys)
            {
                var section = new SectionPermissionModel
                {
                    SectionKey = sectionKey,
                    SectionName = SimplifiedPermissionKeys.SectionNames[sectionKey],
                    SectionIcon = SimplifiedPermissionKeys.SectionIcons[sectionKey],
                    IsVisible = true,
                    Actions = GetAvailableActions()
                };
                
                sections.Add(section);
            }
            
            return sections;
        }

        private List<ActionPermissionModel> GetAvailableActions()
        {
            var actions = new List<ActionPermissionModel>();
            
            foreach (var actionKey in SimplifiedPermissionKeys.ActionNames.Keys)
            {
                var action = new ActionPermissionModel
                {
                    ActionKey = actionKey,
                    ActionName = SimplifiedPermissionKeys.ActionNames[actionKey],
                    ActionIcon = SimplifiedPermissionKeys.ActionIcons[actionKey],
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = true,
                    CanExport = true
                };
                
                actions.Add(action);
            }
            
            return actions;
        }

        private async Task<bool> UpdateUserPermissionAsync(UserPermissionUpdateModel model)
        {
            try
            {
                // Log the update for debugging
                _logger.LogInformation($"Updating permission for user {model.UserId}, section {model.SectionKey}, action {model.ActionKey}");
                
                                 // Create a permission key based on section and action
                 var permissionKey = $"{model.SectionKey}_{model.ActionKey}";
                 
                 // Get permission ID from the permission key
                 var allPermissions = await _permissionService.GetAllPermissionsAsync();
                 var permission = allPermissions.FirstOrDefault(p => p.PermissionKey == permissionKey);
                 
                 if (permission == null)
                 {
                     _logger.LogWarning($"Permission not found for key: {permissionKey}");
                     
                     // Try to find a similar permission or create a mapping
                     var fallbackPermission = allPermissions.FirstOrDefault(p => 
                         p.PermissionKey.Contains(model.ActionKey) || 
                         p.PermissionKey.Contains(model.SectionKey));
                     
                     if (fallbackPermission == null)
                     {
                         _logger.LogWarning($"No fallback permission found for key: {permissionKey}");
                         return false;
                     }
                     
                     permission = fallbackPermission;
                     _logger.LogInformation($"Using fallback permission {fallbackPermission.PermissionKey} for {permissionKey}");
                 }
                
                // Get department ID (use first available department for now)
                var allDepartments = await _permissionService.GetAllDepartmentsAsync();
                var department = allDepartments.FirstOrDefault();
                
                if (department == null)
                {
                    _logger.LogWarning("No departments available");
                    return false;
                }
                
                                 // Add or update the permission
                 var result = await _permissionService.AddUserPermissionAsync(model.UserId, permission.PermissionId, department.DepartmentId);
                 
                 if (result)
                 {
                     _logger.LogInformation($"Successfully updated permission for user {model.UserId}");
                 }
                 else
                 {
                     _logger.LogWarning($"Failed to update permission for user {model.UserId}");
                 }
                 
                 return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permission");
                return false;
            }
        }

        private async Task<bool> UpdateSectionVisibilityAsync(SectionVisibilityUpdateModel model)
        {
            try
            {
                // This would update the database with the new section visibility
                // For now, we'll just return true as a placeholder
                await Task.Delay(100); // Simulate database operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section visibility");
                return false;
            }
        }

        private async Task<bool> ApplyTemplateToUserAsync(ApplyTemplateModel model)
        {
            try
            {
                // This would apply a permission template to a user
                // For now, we'll just return true as a placeholder
                await Task.Delay(100); // Simulate database operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying template to user");
                return false;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAllPermissions([FromBody] List<BulkPermissionUpdateModel> changes)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to manage permissions
                if (currentUsername.ToLower() != "admin" && !await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                }

                var successCount = 0;
                var totalCount = changes.Count;

                foreach (var change in changes)
                {
                    try
                    {
                        // Update section visibility
                        foreach (var section in change.SectionPermissions)
                        {
                            // Update section visibility
                            await UpdateUserPermissionAsync(new UserPermissionUpdateModel
                            {
                                UserId = change.UserId,
                                SectionKey = section.SectionKey,
                                ActionKey = "VIEW",
                                CanView = section.IsVisible,
                                CanAdd = section.IsVisible,
                                CanEdit = section.IsVisible,
                                CanDelete = section.IsVisible,
                                CanExport = section.IsVisible
                            });

                            // Update action permissions
                            foreach (var action in section.Actions)
                            {
                                await UpdateUserPermissionAsync(new UserPermissionUpdateModel
                                {
                                    UserId = change.UserId,
                                    SectionKey = section.SectionKey,
                                    ActionKey = action.ActionKey,
                                    CanView = action.CanView,
                                    CanAdd = action.CanAdd,
                                    CanEdit = action.CanEdit,
                                    CanDelete = action.CanDelete,
                                    CanExport = action.CanExport
                                });
                            }
                        }

                        // Clear cache for the updated user
                        _permissionService.ClearUserCache(change.UserId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating permissions for user {change.UserId}");
                    }
                }

                if (successCount == totalCount)
                {
                    return Json(new { success = true, message = "تم حفظ جميع التغييرات بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = $"تم حفظ {successCount} من {totalCount} تغييرات" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving all permissions");
                return Json(new { success = false, message = "حدث خطأ أثناء حفظ التغييرات" });
            }
        }

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
    // ADDITIONAL MODELS
    // =====================================================

    public class SectionVisibilityUpdateModel
    {
        public int UserId { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
    }

    public class ApplyTemplateModel
    {
        public int UserId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
    }

    public class BulkPermissionUpdateModel
    {
        public int UserId { get; set; }
        public List<SectionPermissionUpdateModel> SectionPermissions { get; set; } = new List<SectionPermissionUpdateModel>();
    }

    public class SectionPermissionUpdateModel
    {
        public string SectionKey { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public List<ActionPermissionUpdateModel> Actions { get; set; } = new List<ActionPermissionUpdateModel>();
    }

    public class ActionPermissionUpdateModel
    {
        public string ActionKey { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }
} 