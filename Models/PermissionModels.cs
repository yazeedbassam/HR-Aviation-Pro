using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // =====================================================
    // PERMISSION MODELS
    // =====================================================

    public class Permission
    {
        public int PermissionId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string PermissionKey { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserDepartmentPermission> UserDepartmentPermissions { get; set; } = new List<UserDepartmentPermission>();
        public virtual ICollection<PermissionLog> PermissionLogs { get; set; } = new List<PermissionLog>();
    }

    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ConfigurationValue Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }

    public class UserDepartmentPermission
    {
        public int UserDepartmentPermissionId { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public int PermissionId { get; set; }
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ControllerUser User { get; set; } = null!; // Changed from User to ControllerUser
        public virtual ConfigurationValue Department { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }

    public class PermissionLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public int? PermissionId { get; set; }
        public int? DepartmentId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Details { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ControllerUser User { get; set; } = null!; // Changed from User to ControllerUser
        public virtual Permission? Permission { get; set; }
        public virtual ConfigurationValue? Department { get; set; }
    }

    // =====================================================
    // VIEW MODELS
    // =====================================================

    public class PermissionViewModel
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Category { get; set; } = string.Empty; // Added for grouping
        public int RolePermissionsCount { get; set; }
        public int UserDepartmentPermissionsCount { get; set; }
    }

    public class RolePermissionViewModel
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;
        public string? PermissionDescription { get; set; } // Added missing property
        public string PermissionCategory { get; set; } = string.Empty; // Added missing property
    }

    public class UserDepartmentPermissionViewModel
    {
        public int UserDepartmentPermissionId { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public int PermissionId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty; // Added missing property
        public string DepartmentName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;
    }

    public class PermissionLogViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public int? PermissionId { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; } = string.Empty; // Added missing property
        public string? Details { get; set; }
        public string? IpAddress { get; set; } // Added missing property
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } // Added missing property
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty; // Added missing property
        public string? PermissionName { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class PermissionCheckResult
    {
        public bool HasPermission { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.Now;
    }

    public class UserPermissionSummary
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // Added missing property
        public string UserFullName { get; set; } = string.Empty; // Added missing property
        public string UserRole { get; set; } = string.Empty; // Added missing property
        public int TotalPermissions { get; set; } // Added missing property
        public int AccessibleDepartments { get; set; } // Added missing property
        public int ActivePermissions { get; set; } // Added missing property
        public List<RolePermissionViewModel> RolePermissions { get; set; } = new List<RolePermissionViewModel>(); // Added missing property
        public List<UserDepartmentPermissionViewModel> DepartmentPermissions { get; set; } = new List<UserDepartmentPermissionViewModel>(); // Added missing property
        public List<string> AccessibleDepartmentsList { get; set; } = new List<string>(); // Added missing property
        public List<PermissionLogViewModel> RecentActivity { get; set; } = new List<PermissionLogViewModel>(); // Added missing property
    }

    // =====================================================
    // ADVANCED PERMISSION VIEW MODELS
    // =====================================================

    public class AdvancedPermissionDashboardViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserDepartment { get; set; } = string.Empty;
        public int TotalPermissions { get; set; }
        public int ActivePermissions { get; set; }
        public int AccessibleDepartments { get; set; }
        public int VisibleMenuItems { get; set; }
        public List<MenuItemPermission> VisibleMenuItemsList { get; set; } = new List<MenuItemPermission>();
        public List<string> AccessibleDepartmentNames { get; set; } = new List<string>();
        public List<PermissionViewModel> RecentPermissions { get; set; } = new List<PermissionViewModel>();
        public DateTime LastPermissionCheck { get; set; } = DateTime.Now;
    }

    public class UserPermissionsViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalPermissions { get; set; }
        public List<PermissionViewModel> BasicPermissions { get; set; } = new List<PermissionViewModel>();
        public List<UserDepartmentPermissionViewModel> DepartmentPermissions { get; set; } = new List<UserDepartmentPermissionViewModel>();
    }

    public class DataAccessViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserDepartment { get; set; } = string.Empty;
        public List<AccessibleUserInfo> AccessibleUsers { get; set; } = new List<AccessibleUserInfo>();
        public List<AccessibleDepartmentInfo> AccessibleDepartments { get; set; } = new List<AccessibleDepartmentInfo>();
        public List<string> ViewPermissions { get; set; } = new List<string>();
        public List<string> EditPermissions { get; set; } = new List<string>();
    }

    public class MenuVisibilityViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserDepartment { get; set; } = string.Empty;
        public int TotalMenuItems { get; set; }
        public int VisibleMenuItems { get; set; }
        public int HiddenMenuItems { get; set; }
        public int SubMenuItems { get; set; }
        public List<MenuItemPermission> MenuItems { get; set; } = new List<MenuItemPermission>();
        public List<PermissionViewModel> RequiredPermissions { get; set; } = new List<PermissionViewModel>();
    }

    public class AccessibleUserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsInSameDepartment { get; set; }
    }

    public class AccessibleDepartmentInfo
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUserDepartment { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class MenuItemPermission
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public List<MenuItemPermission> SubItems { get; set; } = new List<MenuItemPermission>();
    }

    // =====================================================
    // PERMISSION KEYS CONSTANTS
    // =====================================================

    public static class PermissionKeys
    {
        // Dashboard Permissions
        public const string DashboardView = "DASHBOARD_VIEW";
        public const string DashboardExport = "DASHBOARD_EXPORT";
        
        // Controller Management Permissions
        public const string ControllersView = "CONTROLLERS_VIEW";
        public const string ControllersCreate = "CONTROLLERS_CREATE";
        public const string ControllersEdit = "CONTROLLERS_EDIT";
        public const string ControllersDelete = "CONTROLLERS_DELETE";
        public const string ControllersExport = "CONTROLLERS_EXPORT";
        
        // Staff Management Permissions
        public const string StaffView = "STAFF_VIEW";
        public const string StaffCreate = "STAFF_CREATE";
        public const string StaffEdit = "STAFF_EDIT";
        public const string StaffDelete = "STAFF_DELETE";
        public const string StaffExport = "STAFF_EXPORT";
        
        // License Management Permissions
        public const string LicensesView = "LICENSES_VIEW";
        public const string LicensesCreate = "LICENSES_CREATE";
        public const string LicensesEdit = "LICENSES_EDIT";
        public const string LicensesDelete = "LICENSES_DELETE";
        public const string LicensesExport = "LICENSES_EXPORT";
        
        // Certificate Management Permissions
        public const string CertificatesView = "CERTIFICATES_VIEW";
        public const string CertificatesCreate = "CERTIFICATES_CREATE";
        public const string CertificatesEdit = "CERTIFICATES_EDIT";
        public const string CertificatesDelete = "CERTIFICATES_DELETE";
        public const string CertificatesExport = "CERTIFICATES_EXPORT";
        
        // Observation Management Permissions
        public const string ObservationsView = "OBSERVATIONS_VIEW";
        public const string ObservationsCreate = "OBSERVATIONS_CREATE";
        public const string ObservationsEdit = "OBSERVATIONS_EDIT";
        public const string ObservationsDelete = "OBSERVATIONS_DELETE";
        public const string ObservationsExport = "OBSERVATIONS_EXPORT";
        
        // System Settings Permissions
        public const string SystemSettingsView = "SYSTEM_SETTINGS_VIEW";
        public const string SystemSettingsEdit = "SYSTEM_SETTINGS_EDIT";
        public const string ConfigurationManagement = "CONFIGURATION_MANAGEMENT";
        public const string RolesManagement = "ROLES_MANAGEMENT";
        
        // User Management Permissions
        public const string UsersView = "USERS_VIEW";
        public const string UsersCreate = "USERS_CREATE";
        public const string UsersEdit = "USERS_EDIT";
        public const string UsersDelete = "USERS_DELETE";
        
        // Reports Permissions
        public const string ReportsView = "REPORTS_VIEW";
        public const string ReportsGenerate = "REPORTS_GENERATE";
        public const string ReportsExport = "REPORTS_EXPORT";
        
        // Audit Permissions
        public const string AuditLogsView = "AUDIT_LOGS_VIEW";
        public const string PermissionLogsView = "PERMISSION_LOGS_VIEW";
        
        // Department Overview Permissions
        public const string DepartmentOverview = "DEPARTMENT_OVERVIEW";
    }

    // =====================================================
    // PERMISSION CATEGORIES
    // =====================================================

    public static class PermissionCategories
    {
        public static readonly Dictionary<string, List<string>> Categories = new()
        {
            ["Dashboard"] = new List<string>
            {
                PermissionKeys.DashboardView,
                PermissionKeys.DashboardExport
            },
            ["Controllers"] = new List<string>
            {
                PermissionKeys.ControllersView,
                PermissionKeys.ControllersCreate,
                PermissionKeys.ControllersEdit,
                PermissionKeys.ControllersDelete,
                PermissionKeys.ControllersExport
            },
            ["Staff"] = new List<string>
            {
                PermissionKeys.StaffView,
                PermissionKeys.StaffCreate,
                PermissionKeys.StaffEdit,
                PermissionKeys.StaffDelete,
                PermissionKeys.StaffExport
            },
            ["Licenses"] = new List<string>
            {
                PermissionKeys.LicensesView,
                PermissionKeys.LicensesCreate,
                PermissionKeys.LicensesEdit,
                PermissionKeys.LicensesDelete,
                PermissionKeys.LicensesExport
            },
            ["Certificates"] = new List<string>
            {
                PermissionKeys.CertificatesView,
                PermissionKeys.CertificatesCreate,
                PermissionKeys.CertificatesEdit,
                PermissionKeys.CertificatesDelete,
                PermissionKeys.CertificatesExport
            },
            ["Observations"] = new List<string>
            {
                PermissionKeys.ObservationsView,
                PermissionKeys.ObservationsCreate,
                PermissionKeys.ObservationsEdit,
                PermissionKeys.ObservationsDelete,
                PermissionKeys.ObservationsExport
            },
            ["System Settings"] = new List<string>
            {
                PermissionKeys.SystemSettingsView,
                PermissionKeys.SystemSettingsEdit,
                PermissionKeys.ConfigurationManagement,
                PermissionKeys.RolesManagement,
                PermissionKeys.DepartmentOverview
            },
            ["Users"] = new List<string>
            {
                PermissionKeys.UsersView,
                PermissionKeys.UsersCreate,
                PermissionKeys.UsersEdit,
                PermissionKeys.UsersDelete
            },
            ["Reports"] = new List<string>
            {
                PermissionKeys.ReportsView,
                PermissionKeys.ReportsGenerate,
                PermissionKeys.ReportsExport
            },
            ["Audit"] = new List<string>
            {
                PermissionKeys.AuditLogsView,
                PermissionKeys.PermissionLogsView
            }
        };
    }

    // =====================================================
    // SIMPLIFIED PERMISSION MANAGEMENT MODELS
    // =====================================================

    public class SimplifiedPermissionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserDepartment { get; set; } = string.Empty;
        public List<SectionPermissionModel> SectionPermissions { get; set; } = new List<SectionPermissionModel>();
    }

    public class SectionPermissionModel
    {
        public string SectionKey { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string SectionIcon { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public List<ActionPermissionModel> Actions { get; set; } = new List<ActionPermissionModel>();
    }

    public class ActionPermissionModel
    {
        public string ActionKey { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string ActionIcon { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }

    public class UserPermissionUpdateModel
    {
        public int UserId { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public string ActionKey { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }

    public class PermissionManagerViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public List<SimplifiedPermissionViewModel> Users { get; set; } = new List<SimplifiedPermissionViewModel>();
        public List<SectionPermissionModel> AvailableSections { get; set; } = new List<SectionPermissionModel>();
    }

    // =====================================================
    // SIMPLIFIED PERMISSION KEYS
    // =====================================================

    public static class SimplifiedPermissionKeys
    {
        // Panel Sections
        public const string DASHBOARD = "DASHBOARD";
        public const string ORGANIZATION = "ORGANIZATION";
        public const string STAFF = "STAFF";
        public const string DOCUMENTS = "DOCUMENTS";
        public const string ACTIVITIES = "ACTIVITIES";
        public const string SYSTEM_SETTINGS = "SYSTEM_SETTINGS";

        // Actions
        public const string VIEW = "VIEW";
        public const string ADD = "ADD";
        public const string EDIT = "EDIT";
        public const string DELETE = "DELETE";
        public const string EXPORT = "EXPORT";

        // Section Names
        public static readonly Dictionary<string, string> SectionNames = new()
        {
            [DASHBOARD] = "لوحة التحكم",
            [ORGANIZATION] = "المنظمة",
            [STAFF] = "الموظفون",
            [DOCUMENTS] = "المستندات",
            [ACTIVITIES] = "الأنشطة",
            [SYSTEM_SETTINGS] = "إعدادات النظام"
        };

        // Action Names
        public static readonly Dictionary<string, string> ActionNames = new()
        {
            [VIEW] = "عرض",
            [ADD] = "إضافة",
            [EDIT] = "تعديل",
            [DELETE] = "حذف",
            [EXPORT] = "تصدير"
        };

        // Section Icons
        public static readonly Dictionary<string, string> SectionIcons = new()
        {
            [DASHBOARD] = "bi-speedometer2",
            [ORGANIZATION] = "bi-building",
            [STAFF] = "bi-people",
            [DOCUMENTS] = "bi-file-earmark-text",
            [ACTIVITIES] = "bi-activity",
            [SYSTEM_SETTINGS] = "bi-gear"
        };

        // Action Icons
        public static readonly Dictionary<string, string> ActionIcons = new()
        {
            [VIEW] = "bi-eye",
            [ADD] = "bi-plus-circle",
            [EDIT] = "bi-pencil",
            [DELETE] = "bi-trash",
            [EXPORT] = "bi-download"
        };
    }

    // =====================================================
    // ADVANCED PERMISSION MANAGEMENT MODELS
    // =====================================================

    public class UserPermissionManagerViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public List<PermissionInfo> Permissions { get; set; } = new List<PermissionInfo>();
        public List<DepartmentInfo> Departments { get; set; } = new List<DepartmentInfo>();
        public List<RoleInfo> Roles { get; set; } = new List<RoleInfo>();
        public List<UserPermissionUpdateModel> UserPermissions { get; set; } = new List<UserPermissionUpdateModel>();
    }

    public class PermissionMatrixViewModel
    {
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public List<PermissionInfo> Permissions { get; set; } = new List<PermissionInfo>();
        public Dictionary<string, Dictionary<string, bool>> PermissionMatrix { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }

    public class BulkPermissionUpdateViewModel
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public List<string> PermissionKeys { get; set; } = new List<string>();
        public List<int> DepartmentIds { get; set; } = new List<int>();
        public List<string> RoleNames { get; set; } = new List<string>();
        public bool GrantPermission { get; set; }
        public string UpdateReason { get; set; } = string.Empty;
    }

    public class BulkPermissionUpdateResult
    {
        public bool Success { get; set; }
        public int UpdatedUsers { get; set; }
        public int UpdatedPermissions { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }

    public class PermissionTemplateModel
    {
        public string TemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PermissionKeys { get; set; } = new List<string>();
        public Dictionary<string, bool> SectionVisibility { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, Dictionary<string, bool>> ActionPermissions { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }

    public class PermissionTemplatesViewModel
    {
        public List<PermissionTemplateModel> Templates { get; set; } = new List<PermissionTemplateModel>();
        public List<PermissionInfo> AvailablePermissions { get; set; } = new List<PermissionInfo>();
    }

    public class ApplyTemplateResult
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public int AppliedPermissions { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // =====================================================
    // SUPPORTING MODELS
    // =====================================================

    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserDepartment { get; set; } = string.Empty;
    }

    public class PermissionInfo
    {
        public int PermissionId { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class DepartmentInfo
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class DeleteUsersRequest
    {
        public List<int> UserIds { get; set; } = new List<int>();
    }

    public class PermissionTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, bool> MenuPermissions { get; set; } = new Dictionary<string, bool>();
        public List<OperationPermissionTemplate> OperationPermissions { get; set; } = new List<OperationPermissionTemplate>();
    }

    public class OperationPermissionTemplate
    {
        public string EntityType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
        public string Scope { get; set; } = "All";
    }
} 