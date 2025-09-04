-- =====================================================
-- Create Useful Views for HR Aviation System
-- =====================================================
-- سكريبت لإنشاء Views مفيدة للنظام
-- يجب تنفيذه في Supabase SQL Editor

-- إنشاء view لعرض معلومات المستخدم مع صلاحياته
CREATE OR REPLACE VIEW UserPermissionsView AS
SELECT 
    u.UserId,
    u.Username,
    u.FullName,
    u.Email,
    u.Role,
    u.IsActive,
    u.CreatedDate,
    u.LastLoginDate,
    COUNT(DISTINCT up.PermissionId) as TotalPermissions,
    STRING_AGG(DISTINCT p.PermissionName, ', ' ORDER BY p.PermissionName) as PermissionNames,
    STRING_AGG(DISTINCT p.Category, ', ' ORDER BY p.Category) as PermissionCategories
FROM Users u
LEFT JOIN UserPermissions up ON u.UserId = up.UserId
LEFT JOIN Permissions p ON up.PermissionId = p.PermissionId AND p.IsActive = TRUE
GROUP BY u.UserId, u.Username, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedDate, u.LastLoginDate;

-- إنشاء view لعرض الصلاحيات المتاحة
CREATE OR REPLACE VIEW AvailablePermissionsView AS
SELECT 
    p.PermissionId,
    p.PermissionName,
    p.Description,
    p.Category,
    COUNT(up.UserId) as AssignedUsersCount,
    STRING_AGG(DISTINCT u.Username, ', ' ORDER BY u.Username) as AssignedUsers
FROM Permissions p
LEFT JOIN UserPermissions up ON p.PermissionId = up.PermissionId
LEFT JOIN Users u ON up.UserId = u.UserId AND u.IsActive = TRUE
WHERE p.IsActive = TRUE
GROUP BY p.PermissionId, p.PermissionName, p.Description, p.Category
ORDER BY p.Category, p.PermissionName;

-- إنشاء view لعرض الأقسام مع عدد المستخدمين
CREATE OR REPLACE VIEW DepartmentUsersView AS
SELECT 
    d.DepartmentId,
    d.DepartmentName,
    d.Description,
    COUNT(DISTINCT udp.UserId) as UsersCount,
    COUNT(CASE WHEN udp.CanView = TRUE THEN 1 END) as CanViewCount,
    COUNT(CASE WHEN udp.CanEdit = TRUE THEN 1 END) as CanEditCount,
    COUNT(CASE WHEN udp.CanDelete = TRUE THEN 1 END) as CanDeleteCount,
    COUNT(CASE WHEN udp.CanCreate = TRUE THEN 1 END) as CanCreateCount,
    STRING_AGG(DISTINCT u.Username, ', ' ORDER BY u.Username) as Users
FROM Departments d
LEFT JOIN UserDepartmentPermissions udp ON d.DepartmentId = udp.DepartmentId
LEFT JOIN Users u ON udp.UserId = u.UserId AND u.IsActive = TRUE
WHERE d.IsActive = TRUE
GROUP BY d.DepartmentId, d.DepartmentName, d.Description
ORDER BY d.DepartmentName;

-- إنشاء view لعرض سجل الأنشطة مع تفاصيل المستخدم
CREATE OR REPLACE VIEW ActivityLogView AS
SELECT 
    al.LogId,
    al.UserId,
    al.Username,
    u.FullName,
    al.Action,
    al.EntityType,
    al.EntityId,
    al.Details,
    al.IpAddress,
    al.UserAgent,
    al.IsSuccessful,
    al.ErrorMessage,
    al.CreatedDate
FROM ActivityLog al
LEFT JOIN Users u ON al.UserId = u.UserId
ORDER BY al.CreatedDate DESC;

-- إنشاء view لعرض الموظفين النشطين
CREATE OR REPLACE VIEW ActiveEmployeesView AS
SELECT 
    EmployeeId,
    EmployeeNumber,
    FirstName,
    LastName,
    FullName,
    Email,
    Phone,
    Department,
    Position,
    HireDate,
    CreatedDate,
    ModifiedDate
FROM Employees
WHERE IsActive = TRUE
ORDER BY FullName;

-- إنشاء view لعرض المراقبين النشطين
CREATE OR REPLACE VIEW ActiveControllersView AS
SELECT 
    ControllerId,
    ControllerNumber,
    FirstName,
    LastName,
    FullName,
    Email,
    Phone,
    Department,
    Position,
    HireDate,
    CreatedDate,
    ModifiedDate
FROM Controllers
WHERE IsActive = TRUE
ORDER BY FullName;

-- إنشاء view لعرض الشهادات الصالحة
CREATE OR REPLACE VIEW ValidCertificatesView AS
SELECT 
    CertificateId,
    CertificateNumber,
    CertificateTitle,
    IssuingAuthority,
    IssuingCountry,
    IssueDate,
    ExpiryDate,
    CertificateType,
    CASE 
        WHEN ExpiryDate < CURRENT_DATE THEN 'Expired'
        WHEN ExpiryDate <= CURRENT_DATE + INTERVAL '30 days' THEN 'Expiring Soon'
        ELSE 'Valid'
    END as Status,
    CreatedDate,
    ModifiedDate
FROM Certificates
WHERE IsValid = TRUE
ORDER BY ExpiryDate;

-- إنشاء view لعرض التراخيص الصالحة
CREATE OR REPLACE VIEW ValidLicensesView AS
SELECT 
    LicenseId,
    LicenseNumber,
    LicenseTitle,
    IssuingAuthority,
    IssuingCountry,
    IssueDate,
    ExpiryDate,
    LicenseType,
    CASE 
        WHEN ExpiryDate < CURRENT_DATE THEN 'Expired'
        WHEN ExpiryDate <= CURRENT_DATE + INTERVAL '30 days' THEN 'Expiring Soon'
        ELSE 'Valid'
    END as Status,
    CreatedDate,
    ModifiedDate
FROM Licenses
WHERE IsValid = TRUE
ORDER BY ExpiryDate;

-- إنشاء view لعرض المشاريع النشطة
CREATE OR REPLACE VIEW ActiveProjectsView AS
SELECT 
    ProjectId,
    ProjectName,
    ProjectDescription,
    StartDate,
    EndDate,
    Status,
    ProjectManager,
    Budget,
    CreatedDate,
    ModifiedDate,
    CASE 
        WHEN EndDate < CURRENT_DATE THEN 'Completed'
        WHEN StartDate > CURRENT_DATE THEN 'Not Started'
        ELSE 'In Progress'
    END as ProjectStatus
FROM Projects
WHERE IsActive = TRUE
ORDER BY StartDate DESC;

-- إنشاء view لعرض الملاحظات النشطة
CREATE OR REPLACE VIEW ActiveObservationsView AS
SELECT 
    ObservationId,
    ObservationTitle,
    ObservationDescription,
    ObservationDate,
    Observer,
    ObservedPerson,
    ObservationType,
    Status,
    CreatedDate,
    ModifiedDate
FROM Observations
WHERE IsActive = TRUE
ORDER BY ObservationDate DESC;

-- إنشاء view لعرض إحصائيات النظام
CREATE OR REPLACE VIEW SystemStatisticsView AS
SELECT 
    'System Statistics' as Category,
    (SELECT COUNT(*) FROM Users WHERE IsActive = TRUE) as ActiveUsers,
    (SELECT COUNT(*) FROM Employees WHERE IsActive = TRUE) as ActiveEmployees,
    (SELECT COUNT(*) FROM Controllers WHERE IsActive = TRUE) as ActiveControllers,
    (SELECT COUNT(*) FROM Certificates WHERE IsValid = TRUE) as ValidCertificates,
    (SELECT COUNT(*) FROM Licenses WHERE IsValid = TRUE) as ValidLicenses,
    (SELECT COUNT(*) FROM Projects WHERE IsActive = TRUE) as ActiveProjects,
    (SELECT COUNT(*) FROM Observations WHERE IsActive = TRUE) as ActiveObservations,
    (SELECT COUNT(*) FROM Permissions WHERE IsActive = TRUE) as TotalPermissions,
    (SELECT COUNT(*) FROM Departments WHERE IsActive = TRUE) as TotalDepartments;

-- إنشاء view لعرض الشهادات والتراخيص المنتهية الصلاحية قريباً
CREATE OR REPLACE VIEW ExpiringSoonView AS
SELECT 
    'Certificate' as Type,
    CertificateId as Id,
    CertificateNumber as Number,
    CertificateTitle as Title,
    ExpiryDate,
    'Certificate' as EntityType
FROM Certificates
WHERE IsValid = TRUE 
AND ExpiryDate <= CURRENT_DATE + INTERVAL '30 days'
AND ExpiryDate > CURRENT_DATE

UNION ALL

SELECT 
    'License' as Type,
    LicenseId as Id,
    LicenseNumber as Number,
    LicenseTitle as Title,
    ExpiryDate,
    'License' as EntityType
FROM Licenses
WHERE IsValid = TRUE 
AND ExpiryDate <= CURRENT_DATE + INTERVAL '30 days'
AND ExpiryDate > CURRENT_DATE

ORDER BY ExpiryDate;

-- إنشاء view لعرض الشهادات والتراخيص المنتهية الصلاحية
CREATE OR REPLACE VIEW ExpiredView AS
SELECT 
    'Certificate' as Type,
    CertificateId as Id,
    CertificateNumber as Number,
    CertificateTitle as Title,
    ExpiryDate,
    'Certificate' as EntityType
FROM Certificates
WHERE IsValid = TRUE 
AND ExpiryDate < CURRENT_DATE

UNION ALL

SELECT 
    'License' as Type,
    LicenseId as Id,
    LicenseNumber as Number,
    LicenseTitle as Title,
    ExpiryDate,
    'License' as EntityType
FROM Licenses
WHERE IsValid = TRUE 
AND ExpiryDate < CURRENT_DATE

ORDER BY ExpiryDate;

-- عرض معلومات الـ Views المُنشأة
SELECT 'Views Created Successfully' as Status;

SELECT 
    'Created Views' as Info,
    viewname,
    definition
FROM pg_views 
WHERE schemaname = 'public'
AND viewname IN ('UserPermissionsView', 'AvailablePermissionsView', 'DepartmentUsersView', 
                 'ActivityLogView', 'ActiveEmployeesView', 'ActiveControllersView',
                 'ValidCertificatesView', 'ValidLicensesView', 'ActiveProjectsView',
                 'ActiveObservationsView', 'SystemStatisticsView', 'ExpiringSoonView', 'ExpiredView')
ORDER BY viewname;

-- رسالة النجاح
SELECT '✅ Views created successfully!' as Message
UNION ALL
SELECT '📊 You can now use these views for reporting and data analysis'
UNION ALL
SELECT '🔍 Views provide easy access to filtered and aggregated data'
UNION ALL
SELECT '📈 Use SystemStatisticsView for dashboard statistics';