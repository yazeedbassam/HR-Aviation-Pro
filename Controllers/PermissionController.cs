using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    [Obsolete("هذا النظام قديم. يرجى استخدام SimplifiedPermissionController أو AdvancedPermissionController")]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: Permission
        public async Task<IActionResult> Index()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                var rolePermissions = await _permissionService.GetRolePermissionsAsync();
                var userDepartmentPermissions = await _permissionService.GetUserDepartmentPermissionsAsync();

                ViewBag.TotalPermissions = permissions.Count;
                ViewBag.TotalRolePermissions = rolePermissions.Count;
                ViewBag.TotalUserDepartmentPermissions = userDepartmentPermissions.Count;

                return View(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permission index");
                TempData["Error"] = "An error occurred while loading permissions.";
                return View(new List<PermissionViewModel>());
            }
        }

        // GET: Permission/RolePermissions
        public async Task<IActionResult> RolePermissions(int? roleId = null)
        {
            try
            {
                var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);
                var roles = _permissionService.GetRolesSelectList();
                var permissions = _permissionService.GetPermissionsSelectList();

                ViewBag.Roles = roles;
                ViewBag.Permissions = permissions;
                ViewBag.SelectedRoleId = roleId;

                return View(rolePermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role permissions");
                TempData["Error"] = "An error occurred while loading role permissions.";
                return View(new List<RolePermissionViewModel>());
            }
        }

        // POST: Permission/AddRolePermission
        [HttpPost]
        public async Task<IActionResult> AddRolePermission(int roleId, int permissionId)
        {
            try
            {
                var result = await _permissionService.AddRolePermissionAsync(roleId, permissionId);
                if (result)
                {
                    TempData["Success"] = "Role permission added successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to add role permission.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role permission: RoleId={RoleId}, PermissionId={PermissionId}", roleId, permissionId);
                TempData["Error"] = "An error occurred while adding role permission.";
            }

            return RedirectToAction(nameof(RolePermissions));
        }

        // POST: Permission/RemoveRolePermission
        [HttpPost]
        public async Task<IActionResult> RemoveRolePermission(int rolePermissionId)
        {
            try
            {
                var result = await _permissionService.RemoveRolePermissionAsync(rolePermissionId);
                if (result)
                {
                    TempData["Success"] = "Role permission removed successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to remove role permission.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role permission: RolePermissionId={RolePermissionId}", rolePermissionId);
                TempData["Error"] = "An error occurred while removing role permission.";
            }

            return RedirectToAction(nameof(RolePermissions));
        }

        // GET: Permission/UserDepartmentPermissions
        [AllowAnonymous]
        public async Task<IActionResult> UserDepartmentPermissions(int? userId = null)
        {
            try
            {
                _logger.LogInformation("UserDepartmentPermissions action called with userId: {UserId}", userId);
                
                var userDepartmentPermissions = await _permissionService.GetUserDepartmentPermissionsAsync(userId);
                _logger.LogInformation("Retrieved {Count} user department permissions", userDepartmentPermissions.Count);
                
                var users = _permissionService.GetUsersSelectList();
                var departments = _permissionService.GetDepartmentsSelectList();
                var permissions = _permissionService.GetPermissionsSelectList();

                ViewBag.Users = users;
                ViewBag.Departments = departments;
                ViewBag.Permissions = permissions;
                ViewBag.SelectedUserId = userId;

                _logger.LogInformation("ViewBag populated with {UserCount} users, {DeptCount} departments, {PermCount} permissions", 
                    users.Count, departments.Count, permissions.Count);

                return View(userDepartmentPermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user department permissions");
                TempData["Error"] = "An error occurred while loading user department permissions.";
                return View(new List<UserDepartmentPermissionViewModel>());
            }
        }

        // GET: Permission/CreateUserDepartmentPermission
        public IActionResult CreateUserDepartmentPermission()
        {
            try
            {
                _logger.LogInformation("CreateUserDepartmentPermission action called");
                
                var users = _permissionService.GetUsersSelectList();
                _logger.LogInformation("Users loaded: {Count} users", users.Count);
                foreach (var user in users.Take(3))
                {
                    _logger.LogInformation("User: Value={Value}, Text={Text}", user.Value, user.Text);
                }
                
                var departments = _permissionService.GetDepartmentsSelectList();
                _logger.LogInformation("Departments loaded: {Count} departments", departments.Count);
                foreach (var dept in departments)
                {
                    _logger.LogInformation("Department: Value={Value}, Text={Text}", dept.Value, dept.Text);
                }
                
                var permissions = _permissionService.GetPermissionsSelectList();
                _logger.LogInformation("Permissions loaded: {Count} permissions", permissions.Count);
                foreach (var perm in permissions.Take(3))
                {
                    _logger.LogInformation("Permission: Value={Value}, Text={Text}", perm.Value, perm.Text);
                }

                ViewBag.Users = users;
                ViewBag.Departments = departments;
                ViewBag.Permissions = permissions;

                _logger.LogInformation("ViewBag populated successfully");
                return View(new UserDepartmentPermissionViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create user department permission form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(UserDepartmentPermissions));
            }
        }

        // POST: Permission/CreateUserDepartmentPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserDepartmentPermission(UserDepartmentPermissionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _permissionService.AddUserDepartmentPermissionAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "User department permission created successfully.";
                        return RedirectToAction(nameof(UserDepartmentPermissions));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create user department permission.";
                    }
                }
                else
                {
                    TempData["Error"] = "Please correct the errors in the form.";
                }

                // Reload dropdowns for the form
                var users = _permissionService.GetUsersSelectList();
                var departments = _permissionService.GetDepartmentsSelectList();
                var permissions = _permissionService.GetPermissionsSelectList();

                ViewBag.Users = users;
                ViewBag.Departments = departments;
                ViewBag.Permissions = permissions;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user department permission");
                TempData["Error"] = "An error occurred while creating user department permission.";
                return RedirectToAction(nameof(UserDepartmentPermissions));
            }
        }

        // GET: Permission/EditUserDepartmentPermission
        public async Task<IActionResult> EditUserDepartmentPermission(int id)
        {
            try
            {
                var userDepartmentPermissions = await _permissionService.GetUserDepartmentPermissionsAsync();
                var permission = userDepartmentPermissions.FirstOrDefault(p => p.UserDepartmentPermissionId == id);

                if (permission == null)
                {
                    TempData["Error"] = "User department permission not found.";
                    return RedirectToAction(nameof(UserDepartmentPermissions));
                }

                var users = _permissionService.GetUsersSelectList();
                var departments = _permissionService.GetDepartmentsSelectList();
                var permissions = _permissionService.GetPermissionsSelectList();

                ViewBag.Users = users;
                ViewBag.Departments = departments;
                ViewBag.Permissions = permissions;

                return View(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit user department permission form: Id={Id}", id);
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(UserDepartmentPermissions));
            }
        }

        // POST: Permission/EditUserDepartmentPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserDepartmentPermission(UserDepartmentPermissionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _permissionService.UpdateUserDepartmentPermissionAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "User department permission updated successfully.";
                        return RedirectToAction(nameof(UserDepartmentPermissions));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to update user department permission.";
                    }
                }
                else
                {
                    TempData["Error"] = "Please correct the errors in the form.";
                }

                // Reload dropdowns for the form
                var users = _permissionService.GetUsersSelectList();
                var departments = _permissionService.GetDepartmentsSelectList();
                var permissions = _permissionService.GetPermissionsSelectList();

                ViewBag.Users = users;
                ViewBag.Departments = departments;
                ViewBag.Permissions = permissions;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user department permission");
                TempData["Error"] = "An error occurred while updating user department permission.";
                return RedirectToAction(nameof(UserDepartmentPermissions));
            }
        }

        // POST: Permission/DeleteUserDepartmentPermission/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Permission/DeleteUserDepartmentPermission/{id}")]
        public async Task<IActionResult> DeleteUserDepartmentPermission(int id)
        {
            try
            {
                var result = await _permissionService.RemoveUserDepartmentPermissionAsync(id);
                if (result)
                {
                    TempData["Success"] = "User department permission deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user department permission.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user department permission: Id={Id}", id);
                TempData["Error"] = "An error occurred while deleting user department permission.";
            }

            return RedirectToAction(nameof(UserDepartmentPermissions));
        }

        // GET: Permission/Logs
        public async Task<IActionResult> Logs(int? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var logs = await _permissionService.GetPermissionLogsAsync(userId, fromDate, toDate);
                var users = _permissionService.GetUsersSelectList();

                ViewBag.Users = users;
                ViewBag.SelectedUserId = userId;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permission logs");
                TempData["Error"] = "An error occurred while loading permission logs.";
                return View(new List<PermissionLogViewModel>());
            }
        }

        // GET: Permission/UserSummary
        public async Task<IActionResult> UserSummary(int userId)
        {
            try
            {
                var summary = await _permissionService.GetUserPermissionSummaryAsync(userId);
                if (summary == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user permission summary: UserId={UserId}", userId);
                TempData["Error"] = "An error occurred while loading user permission summary.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Permission/CheckPermission
        [HttpPost]
        public async Task<IActionResult> CheckPermission(int userId, string permissionKey, int? departmentId = null)
        {
            try
            {
                var result = await _permissionService.CheckPermissionAsync(userId, permissionKey, departmentId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission: UserId={UserId}, PermissionKey={PermissionKey}", userId, permissionKey);
                return Json(new PermissionCheckResult { HasPermission = false, Message = "Error occurred while checking permission." });
            }
        }

        // AJAX: Permission/GetUserPermissions
        [HttpPost]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                return Json(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions: UserId={UserId}", userId);
                return Json(new List<PermissionViewModel>());
            }
        }

        // AJAX: Permission/GetUserAccessibleDepartments
        [HttpPost]
        public async Task<IActionResult> GetUserAccessibleDepartments(int userId)
        {
            try
            {
                var departments = await _permissionService.GetUserAccessibleDepartmentsAsync(userId);
                return Json(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user accessible departments: UserId={UserId}", userId);
                return Json(new List<int>());
            }
        }

        // AJAX: Permission/GetRoles
        [HttpGet]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = _permissionService.GetRolesSelectList();
                return Json(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return Json(new List<SelectListItem>());
            }
        }

        // AJAX: Permission/GetPermissions
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPermissions()
        {
            try
            {
                var permissions = _permissionService.GetPermissionsSelectList();
                return Json(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions");
                return Json(new List<SelectListItem>());
            }
        }

        // AJAX: Permission/GetDepartments
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetDepartments()
        {
            try
            {
                _logger.LogInformation("GetDepartments AJAX endpoint called");
                var departments = _permissionService.GetDepartmentsSelectList();
                _logger.LogInformation("GetDepartments returned {Count} departments", departments.Count);
                
                foreach (var dept in departments)
                {
                    _logger.LogInformation("Department: Value={Value}, Text={Text}", dept.Value, dept.Text);
                }
                
                return Json(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return Json(new List<SelectListItem>());
            }
        }

        // AJAX: Permission/GetUsers
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _permissionService.GetUsersSelectList();
                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return Json(new List<SelectListItem>());
            }
        }

        // Temporary endpoint for testing database connection
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestDatabase()
        {
            try
            {
                _logger.LogInformation("TestDatabase endpoint called");
                
                // Test ConfigurationCategories
                var categories = _permissionService.GetRolesSelectList();
                _logger.LogInformation("Categories found: {Count}", categories.Count);
                
                // Test ConfigurationValues for Divisions
                var departments = _permissionService.GetDepartmentsSelectList();
                _logger.LogInformation("Departments found: {Count}", departments.Count);
                
                // Test Users
                var users = _permissionService.GetUsersSelectList();
                _logger.LogInformation("Users found: {Count}", users.Count);
                
                // Test Permissions
                var permissions = _permissionService.GetPermissionsSelectList();
                _logger.LogInformation("Permissions found: {Count}", permissions.Count);
                
                var result = new
                {
                    Categories = categories.Count,
                    Departments = departments.Count,
                    Users = users.Count,
                    Permissions = permissions.Count,
                    DepartmentDetails = departments.Select(d => new { d.Value, d.Text }).ToList()
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestDatabase");
                return Json(new { Error = ex.Message });
            }
        }
    }
} 