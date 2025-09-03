using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PermissionManagerController : Controller
    {
        private readonly IAdvancedPermissionManagerService _permissionService;
        private readonly ILogger<PermissionManagerController> _logger;

        public PermissionManagerController(
            IAdvancedPermissionManagerService permissionService,
            ILogger<PermissionManagerController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        #region Main Dashboard

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
                    _logger.LogInformation($"Admin user {currentUsername} accessing permission manager");
                }
                else
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                var users = await _permissionService.GetAllUsersWithPermissionsAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permission manager index");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل صفحة إدارة الصلاحيات";
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region User Permission Management

        [HttpGet]
        public async Task<IActionResult> UserPermissions(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() != "admin")
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                var userDetails = await _permissionService.GetUserPermissionDetailsAsync(userId);
                if (userDetails == null)
                {
                    TempData["ErrorMessage"] = "المستخدم غير موجود";
                    return RedirectToAction("Index");
                }

                return View(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user permissions for user {UserId}", userId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل صلاحيات المستخدم";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMenuPermissions(int userId, Dictionary<string, bool> menuPermissions)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() != "admin")
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                    }
                }

                foreach (var menuPermission in menuPermissions)
                {
                    await _permissionService.SetMenuPermissionAsync(userId, menuPermission.Key, menuPermission.Value);
                }

                // Clear all user caches dynamically to ensure immediate effect
                _permissionService.ClearUserCacheDynamically(userId);
                _logger.LogInformation("🔥 Dynamically cleared all caches for user {UserId} after menu permission changes", userId);

                return Json(new { success = true, message = "تم تحديث صلاحيات القائمة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu permissions for user {UserId}", userId);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث صلاحيات القائمة" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOperationPermissions([FromBody] UpdateOperationPermissionsRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("UpdateOperationPermissions: Request is null");
                    return Json(new { success = false, message = "طلب غير صحيح" });
                }

                _logger.LogInformation("UpdateOperationPermissions: Called with userId={UserId}, permissionsCount={Count}", request.UserId, request.OperationPermissions?.Count ?? 0);

                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() != "admin")
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                    }
                }

                foreach (var operationPermission in request.OperationPermissions)
                {
                    _logger.LogInformation("UpdateOperationPermissions: Processing permission - EntityType={EntityType}, OperationType={OperationType}, IsAllowed={IsAllowed}, Scope={Scope}", 
                        operationPermission.EntityType, operationPermission.OperationType, operationPermission.IsAllowed, operationPermission.Scope);
                    
                    var result = await _permissionService.SetOperationPermissionAsync(
                        request.UserId, 
                        operationPermission.EntityType, 
                        operationPermission.OperationType, 
                        operationPermission.IsAllowed, 
                        operationPermission.Scope, 
                        operationPermission.ScopeId);
                    
                    _logger.LogInformation("UpdateOperationPermissions: SetOperationPermissionAsync result for {EntityType}-{OperationType}: {Result}", 
                        operationPermission.EntityType, operationPermission.OperationType, result);
                }

                // Clear all user caches to ensure immediate effect
                if (request?.UserId > 0)
                {
                    // Use dynamic cache clearing instead of regular cache clearing
                    _permissionService.ClearUserCacheDynamically(request.UserId);
                    
                    // Force refresh DepartmentOverview cache specifically
                    _permissionService.ForceRefreshDepartmentOverviewCache(request.UserId);
                    
                    _logger.LogInformation("🔥 Dynamically cleared all caches for user {UserId} after permission changes", request.UserId);
                    _logger.LogInformation("🔥 Force refreshed DepartmentOverview cache for user {UserId}", request.UserId);
                }
                
                return Json(new { success = true, message = "تم تحديث صلاحيات العمليات بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating operation permissions for user {UserId}", request?.UserId ?? 0);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث صلاحيات العمليات" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrganizationalPermissions([FromBody] UpdateOrganizationalPermissionsRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("UpdateOrganizationalPermissions: Request is null");
                    return Json(new { success = false, message = "طلب غير صحيح" });
                }

                _logger.LogInformation("UpdateOrganizationalPermissions: Called with userId={UserId}, permissionsCount={Count}", request.UserId, request.OrganizationalPermissions?.Count ?? 0);
                
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() != "admin")
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                    }
                }

                foreach (var orgPermission in request.OrganizationalPermissions)
                {
                    _logger.LogInformation("UpdateOrganizationalPermissions: Processing permission - EntityType={EntityType}, EntityId={EntityId}, EntityName={EntityName}, CanView={CanView}, CanEdit={CanEdit}, CanDelete={CanDelete}, CanCreate={CanCreate}", 
                        orgPermission.PermissionType, orgPermission.EntityId, orgPermission.EntityName, orgPermission.CanView, orgPermission.CanEdit, orgPermission.CanDelete, orgPermission.CanCreate);
                    
                    var result = await _permissionService.SetOrganizationalPermissionAsync(
                        request.UserId, 
                        orgPermission.PermissionType, 
                        orgPermission.EntityId, 
                        orgPermission.EntityName, 
                        orgPermission.CanView, 
                        orgPermission.CanEdit, 
                        orgPermission.CanDelete, 
                        orgPermission.CanCreate);
                    
                    _logger.LogInformation("UpdateOrganizationalPermissions: SetOrganizationalPermissionAsync result for {EntityName}: {Result}", 
                        orgPermission.EntityName, result);
                }

                // Clear all user caches dynamically to ensure immediate effect
                _permissionService.ClearUserCacheDynamically(request.UserId);
                _logger.LogInformation("🔥 Dynamically cleared all caches for user {UserId} after organizational permission changes", request.UserId);

                return Json(new { success = true, message = "تم تحديث صلاحيات الهيكل التنظيمي بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organizational permissions for user {UserId}", request?.UserId ?? 0);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث صلاحيات الهيكل التنظيمي" });
            }
        }

        #endregion

        #region Cache Management

        [HttpPost]
        public async Task<IActionResult> ClearUserCacheDynamically(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUsername = GetCurrentUsername();
                
                // Allow admin user to access the page regardless of permissions
                if (currentUsername.ToLower() != "admin")
                {
                    // Check if user has permission to manage permissions
                    if (!await _permissionService.HasPermissionAsync(currentUserId, "PERMISSIONS_MANAGE"))
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لإدارة الصلاحيات" });
                    }
                }

                // Force refresh DepartmentOverview cache specifically
                _permissionService.ForceRefreshDepartmentOverviewCache(userId);
                
                // Clear all user caches dynamically
                _permissionService.ClearUserCacheDynamically(userId);
                
                _logger.LogInformation("🔥 Admin {AdminUser} manually cleared all caches for user {UserId}", currentUsername, userId);
                
                return Json(new { success = true, message = "تم مسح الكاش الديناميكي بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user cache dynamically for user {UserId}", userId);
                return Json(new { success = false, message = "حدث خطأ أثناء مسح الكاش" });
            }
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "";
        }

        #endregion
    }
} 