-- =====================================================
-- Complete HR Aviation System Setup
-- =====================================================
-- سكريبت شامل لإنشاء النظام الكامل
-- يجب تنفيذه في Supabase SQL Editor
-- يتضمن جميع الجداول، الصلاحيات، المستخدمين، Views، و Functions

-- بداية السكريبت
SELECT 'Starting Complete System Setup...' as Status, NOW() as Timestamp;

-- =====================================================
-- 1. إنشاء الجداول الأساسية
-- =====================================================

-- إنشاء جدول المستخدمين
CREATE TABLE IF NOT EXISTS Users (
    UserId SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLoginDate TIMESTAMP,
    Role VARCHAR(50) DEFAULT 'User'
);

-- إنشاء جدول الصلاحيات
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId SERIAL PRIMARY KEY,
    PermissionName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    Category VARCHAR(50),
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات المستخدمين
CREATE TABLE IF NOT EXISTS UserPermissions (
    UserPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE CASCADE,
    PermissionId INTEGER REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    GrantedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, PermissionId)
);

-- إنشاء جدول الأقسام
CREATE TABLE IF NOT EXISTS Departments (
    DepartmentId SERIAL PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات الأقسام
CREATE TABLE IF NOT EXISTS UserDepartmentPermissions (
    UserDepartmentPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE CASCADE,
    DepartmentId INTEGER REFERENCES Departments(DepartmentId) ON DELETE CASCADE,
    CanView BOOLEAN DEFAULT FALSE,
    CanEdit BOOLEAN DEFAULT FALSE,
    CanDelete BOOLEAN DEFAULT FALSE,
    CanCreate BOOLEAN DEFAULT FALSE,
    GrantedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, DepartmentId)
);

-- إنشاء جدول الموظفين
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId SERIAL PRIMARY KEY,
    EmployeeNumber VARCHAR(20) UNIQUE NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) GENERATED ALWAYS AS (FirstName || ' ' || LastName) STORED,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Department VARCHAR(100),
    Position VARCHAR(100),
    HireDate DATE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول المراقبين
CREATE TABLE IF NOT EXISTS Controllers (
    ControllerId SERIAL PRIMARY KEY,
    ControllerNumber VARCHAR(20) UNIQUE NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) GENERATED ALWAYS AS (FirstName || ' ' || LastName) STORED,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Department VARCHAR(100),
    Position VARCHAR(100),
    HireDate DATE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول الشهادات
CREATE TABLE IF NOT EXISTS Certificates (
    CertificateId SERIAL PRIMARY KEY,
    CertificateNumber VARCHAR(50) UNIQUE NOT NULL,
    CertificateTitle VARCHAR(200) NOT NULL,
    IssuingAuthority VARCHAR(200),
    IssuingCountry VARCHAR(100),
    IssueDate DATE,
    ExpiryDate DATE,
    CertificateType VARCHAR(100),
    IsValid BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول التراخيص
CREATE TABLE IF NOT EXISTS Licenses (
    LicenseId SERIAL PRIMARY KEY,
    LicenseNumber VARCHAR(50) UNIQUE NOT NULL,
    LicenseTitle VARCHAR(200) NOT NULL,
    IssuingAuthority VARCHAR(200),
    IssuingCountry VARCHAR(100),
    IssueDate DATE,
    ExpiryDate DATE,
    LicenseType VARCHAR(100),
    IsValid BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول المشاريع
CREATE TABLE IF NOT EXISTS Projects (
    ProjectId SERIAL PRIMARY KEY,
    ProjectName VARCHAR(200) NOT NULL,
    ProjectDescription TEXT,
    StartDate DATE,
    EndDate DATE,
    Status VARCHAR(50) DEFAULT 'Planning',
    ProjectManager VARCHAR(100),
    Budget DECIMAL(15,2),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول الملاحظات
CREATE TABLE IF NOT EXISTS Observations (
    ObservationId SERIAL PRIMARY KEY,
    ObservationTitle VARCHAR(200) NOT NULL,
    ObservationDescription TEXT,
    ObservationDate DATE,
    Observer VARCHAR(100),
    ObservedPerson VARCHAR(100),
    ObservationType VARCHAR(100),
    Status VARCHAR(50) DEFAULT 'Pending',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول سجل الأنشطة
CREATE TABLE IF NOT EXISTS ActivityLog (
    LogId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE SET NULL,
    Username VARCHAR(50),
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50),
    EntityId VARCHAR(50),
    Details TEXT,
    IpAddress INET,
    UserAgent TEXT,
    IsSuccessful BOOLEAN DEFAULT TRUE,
    ErrorMessage TEXT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

SELECT 'Basic tables created successfully' as Status;

-- =====================================================
-- 2. إنشاء الفهارس
-- =====================================================

-- فهارس المستخدمين
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_active ON Users(IsActive);

-- فهارس الصلاحيات
CREATE INDEX IF NOT EXISTS idx_userpermissions_userid ON UserPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userpermissions_permissionid ON UserPermissions(PermissionId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_userid ON UserDepartmentPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_departmentid ON UserDepartmentPermissions(DepartmentId);

-- فهارس الموظفين والمراقبين
CREATE INDEX IF NOT EXISTS idx_employees_employeenumber ON Employees(EmployeeNumber);
CREATE INDEX IF NOT EXISTS idx_employees_active ON Employees(IsActive);
CREATE INDEX IF NOT EXISTS idx_controllers_controllernumber ON Controllers(ControllerNumber);
CREATE INDEX IF NOT EXISTS idx_controllers_active ON Controllers(IsActive);

-- فهارس الشهادات والتراخيص
CREATE INDEX IF NOT EXISTS idx_certificates_certificatenumber ON Certificates(CertificateNumber);
CREATE INDEX IF NOT EXISTS idx_certificates_valid ON Certificates(IsValid);
CREATE INDEX IF NOT EXISTS idx_licenses_licensenumber ON Licenses(LicenseNumber);
CREATE INDEX IF NOT EXISTS idx_licenses_valid ON Licenses(IsValid);

-- فهارس المشاريع والملاحظات
CREATE INDEX IF NOT EXISTS idx_projects_name ON Projects(ProjectName);
CREATE INDEX IF NOT EXISTS idx_projects_active ON Projects(IsActive);
CREATE INDEX IF NOT EXISTS idx_observations_title ON Observations(ObservationTitle);
CREATE INDEX IF NOT EXISTS idx_observations_active ON Observations(IsActive);

-- فهارس سجل الأنشطة
CREATE INDEX IF NOT EXISTS idx_activitylog_userid ON ActivityLog(UserId);
CREATE INDEX IF NOT EXISTS idx_activitylog_username ON ActivityLog(Username);
CREATE INDEX IF NOT EXISTS idx_activitylog_action ON ActivityLog(Action);
CREATE INDEX IF NOT EXISTS idx_activitylog_createddate ON ActivityLog(CreatedDate);

SELECT 'Indexes created successfully' as Status;

-- =====================================================
-- 3. إدراج البيانات الأساسية
-- =====================================================

-- إدراج الصلاحيات
INSERT INTO Permissions (PermissionName, Description, Category) VALUES
-- صلاحيات النظام الأساسية
('SystemAdmin', 'إدارة النظام الكاملة', 'System'),
('UserManagement', 'إدارة المستخدمين', 'System'),
('RoleManagement', 'إدارة الأدوار والصلاحيات', 'System'),
('SystemConfiguration', 'إعدادات النظام', 'System'),
('DatabaseManagement', 'إدارة قاعدة البيانات', 'System'),

-- صلاحيات الموظفين
('EmployeeView', 'عرض الموظفين', 'Employee'),
('EmployeeCreate', 'إضافة موظفين', 'Employee'),
('EmployeeEdit', 'تعديل الموظفين', 'Employee'),
('EmployeeDelete', 'حذف الموظفين', 'Employee'),

-- صلاحيات المراقبين
('ControllerView', 'عرض المراقبين', 'Controller'),
('ControllerCreate', 'إضافة مراقبين', 'Controller'),
('ControllerEdit', 'تعديل المراقبين', 'Controller'),
('ControllerDelete', 'حذف المراقبين', 'Controller'),

-- صلاحيات الشهادات
('CertificateView', 'عرض الشهادات', 'Certificate'),
('CertificateCreate', 'إضافة شهادات', 'Certificate'),
('CertificateEdit', 'تعديل الشهادات', 'Certificate'),
('CertificateDelete', 'حذف الشهادات', 'Certificate'),

-- صلاحيات التراخيص
('LicenseView', 'عرض التراخيص', 'License'),
('LicenseCreate', 'إضافة تراخيص', 'License'),
('LicenseEdit', 'تعديل التراخيص', 'License'),
('LicenseDelete', 'حذف التراخيص', 'License'),

-- صلاحيات المشاريع
('ProjectView', 'عرض المشاريع', 'Project'),
('ProjectCreate', 'إضافة مشاريع', 'Project'),
('ProjectEdit', 'تعديل المشاريع', 'Project'),
('ProjectDelete', 'حذف المشاريع', 'Project'),

-- صلاحيات الملاحظات
('ObservationView', 'عرض الملاحظات', 'Observation'),
('ObservationCreate', 'إضافة ملاحظات', 'Observation'),
('ObservationEdit', 'تعديل الملاحظات', 'Observation'),
('ObservationDelete', 'حذف الملاحظات', 'Observation'),

-- صلاحيات التقارير
('ReportView', 'عرض التقارير', 'Report'),
('ReportCreate', 'إنشاء تقارير', 'Report'),
('ReportExport', 'تصدير التقارير', 'Report'),

-- صلاحيات الإعدادات المتقدمة
('AdvancedPermissionView', 'عرض الصلاحيات المتقدمة', 'Advanced'),
('AdvancedPermissionEdit', 'تعديل الصلاحيات المتقدمة', 'Advanced'),
('MenuVisibilityControl', 'التحكم في إظهار القوائم', 'Advanced'),
('DataAccessControl', 'التحكم في الوصول للبيانات', 'Advanced')

ON CONFLICT (PermissionName) DO NOTHING;

-- إدراج الأقسام
INSERT INTO Departments (DepartmentName, Description) VALUES
('إدارة النظام', 'إدارة النظام والصلاحيات'),
('الموارد البشرية', 'إدارة الموظفين والمراقبين'),
('التدريب والتطوير', 'إدارة الشهادات والتراخيص'),
('المشاريع', 'إدارة المشاريع والأنشطة'),
('الملاحظات والتقييم', 'إدارة الملاحظات والتقييمات'),
('التقارير والإحصائيات', 'إنتاج التقارير والإحصائيات')

ON CONFLICT (DepartmentName) DO NOTHING;

SELECT 'Basic data inserted successfully' as Status;

-- =====================================================
-- 4. إنشاء المستخدم الإداري
-- =====================================================

-- إنشاء المستخدم الإداري
INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES
('admin', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'مدير النظام', 'admin@hr-aviation.com', 'SystemAdmin')
ON CONFLICT (Username) DO UPDATE SET
    Password = EXCLUDED.Password,
    FullName = EXCLUDED.FullName,
    Email = EXCLUDED.Email,
    Role = EXCLUDED.Role,
    IsActive = TRUE;

-- منح جميع الصلاحيات للمستخدم الإداري
INSERT INTO UserPermissions (UserId, PermissionId)
SELECT u.UserId, p.PermissionId
FROM Users u, Permissions p
WHERE u.Username = 'admin' AND p.IsActive = TRUE
ON CONFLICT (UserId, PermissionId) DO NOTHING;

-- منح صلاحيات كاملة لجميع الأقسام للمستخدم الإداري
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, CanView, CanEdit, CanDelete, CanCreate)
SELECT u.UserId, d.DepartmentId, TRUE, TRUE, TRUE, TRUE
FROM Users u, Departments d
WHERE u.Username = 'admin' AND d.IsActive = TRUE
ON CONFLICT (UserId, DepartmentId) DO UPDATE SET
    CanView = TRUE,
    CanEdit = TRUE,
    CanDelete = TRUE,
    CanCreate = TRUE;

SELECT 'Admin user created successfully' as Status;

-- =====================================================
-- 5. إنشاء Views
-- =====================================================

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

SELECT 'Views created successfully' as Status;

-- =====================================================
-- 6. إنشاء Functions
-- =====================================================

-- إنشاء دالة للتحقق من صلاحية المستخدم
CREATE OR REPLACE FUNCTION CheckUserPermission(
    p_username VARCHAR(50),
    p_permission_name VARCHAR(100)
) RETURNS BOOLEAN AS $$
DECLARE
    has_permission BOOLEAN := FALSE;
BEGIN
    SELECT EXISTS(
        SELECT 1 
        FROM Users u
        JOIN UserPermissions up ON u.UserId = up.UserId
        JOIN Permissions p ON up.PermissionId = p.PermissionId
        WHERE u.Username = p_username 
        AND p.PermissionName = p_permission_name
        AND u.IsActive = TRUE
        AND p.IsActive = TRUE
    ) INTO has_permission;
    
    RETURN has_permission;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للتحقق من صلاحية المستخدم في قسم معين
CREATE OR REPLACE FUNCTION CheckUserDepartmentPermission(
    p_username VARCHAR(50),
    p_department_name VARCHAR(100),
    p_permission_type VARCHAR(20)
) RETURNS BOOLEAN AS $$
DECLARE
    has_permission BOOLEAN := FALSE;
BEGIN
    SELECT EXISTS(
        SELECT 1 
        FROM Users u
        JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId
        JOIN Departments d ON udp.DepartmentId = d.DepartmentId
        WHERE u.Username = p_username 
        AND d.DepartmentName = p_department_name
        AND u.IsActive = TRUE
        AND d.IsActive = TRUE
        AND (
            (p_permission_type = 'View' AND udp.CanView = TRUE) OR
            (p_permission_type = 'Edit' AND udp.CanEdit = TRUE) OR
            (p_permission_type = 'Delete' AND udp.CanDelete = TRUE) OR
            (p_permission_type = 'Create' AND udp.CanCreate = TRUE)
        )
    ) INTO has_permission;
    
    RETURN has_permission;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتسجيل دخول المستخدم
CREATE OR REPLACE FUNCTION LogUserLogin(
    p_username VARCHAR(50)
) RETURNS BOOLEAN AS $$
DECLARE
    user_exists BOOLEAN := FALSE;
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM Users WHERE Username = p_username AND IsActive = TRUE
    ) INTO user_exists;
    
    IF user_exists THEN
        UPDATE Users 
        SET LastLoginDate = CURRENT_TIMESTAMP 
        WHERE Username = p_username;
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتسجيل الأنشطة
CREATE OR REPLACE FUNCTION LogUserActivity(
    p_user_id INTEGER,
    p_username VARCHAR(50),
    p_action VARCHAR(100),
    p_entity_type VARCHAR(50) DEFAULT NULL,
    p_entity_id VARCHAR(50) DEFAULT NULL,
    p_details TEXT DEFAULT NULL,
    p_ip_address INET DEFAULT NULL,
    p_user_agent TEXT DEFAULT NULL,
    p_is_successful BOOLEAN DEFAULT TRUE,
    p_error_message TEXT DEFAULT NULL
) RETURNS INTEGER AS $$
DECLARE
    log_id INTEGER;
BEGIN
    INSERT INTO ActivityLog (
        UserId, Username, Action, EntityType, EntityId, 
        Details, IpAddress, UserAgent, IsSuccessful, ErrorMessage
    ) VALUES (
        p_user_id, p_username, p_action, p_entity_type, p_entity_id,
        p_details, p_ip_address, p_user_agent, p_is_successful, p_error_message
    ) RETURNING LogId INTO log_id;
    
    RETURN log_id;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على إحصائيات النظام
CREATE OR REPLACE FUNCTION GetSystemStatistics()
RETURNS TABLE(
    StatName VARCHAR(100),
    StatValue BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 'Active Users'::VARCHAR(100), COUNT(*)::BIGINT FROM Users WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Employees'::VARCHAR(100), COUNT(*)::BIGINT FROM Employees WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Controllers'::VARCHAR(100), COUNT(*)::BIGINT FROM Controllers WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Valid Certificates'::VARCHAR(100), COUNT(*)::BIGINT FROM Certificates WHERE IsValid = TRUE
    UNION ALL
    SELECT 'Valid Licenses'::VARCHAR(100), COUNT(*)::BIGINT FROM Licenses WHERE IsValid = TRUE
    UNION ALL
    SELECT 'Active Projects'::VARCHAR(100), COUNT(*)::BIGINT FROM Projects WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Observations'::VARCHAR(100), COUNT(*)::BIGINT FROM Observations WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Total Permissions'::VARCHAR(100), COUNT(*)::BIGINT FROM Permissions WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Total Departments'::VARCHAR(100), COUNT(*)::BIGINT FROM Departments WHERE IsActive = TRUE;
END;
$$ LANGUAGE plpgsql;

SELECT 'Functions created successfully' as Status;

-- =====================================================
-- 7. تسجيل إنشاء النظام
-- =====================================================

-- تسجيل إنشاء النظام في سجل الأنشطة
INSERT INTO ActivityLog (UserId, Username, Action, EntityType, Details, IsSuccessful)
SELECT 
    UserId, 
    Username, 
    'System Setup', 
    'System', 
    'تم إنشاء النظام الكامل وإعداد المستخدم الإداري', 
    TRUE
FROM Users 
WHERE Username = 'admin';

-- =====================================================
-- 8. عرض النتائج النهائية
-- =====================================================

-- عرض معلومات المستخدم الإداري
SELECT 
    'Admin User Information' as Info,
    Username,
    FullName,
    Email,
    Role,
    IsActive,
    CreatedDate
FROM Users 
WHERE Username = 'admin';

-- عرض إجمالي الصلاحيات والأقسام
SELECT 
    'System Summary' as Info,
    (SELECT COUNT(*) FROM Permissions WHERE IsActive = TRUE) as TotalPermissions,
    (SELECT COUNT(*) FROM Departments WHERE IsActive = TRUE) as TotalDepartments,
    (SELECT COUNT(*) FROM Users WHERE IsActive = TRUE) as TotalUsers,
    (SELECT COUNT(*) FROM UserPermissions) as TotalUserPermissions;

-- عرض الجداول المُنشأة
SELECT 
    'Created Tables' as Info,
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- عرض الـ Views المُنشأة
SELECT 
    'Created Views' as Info,
    viewname
FROM pg_views 
WHERE schemaname = 'public'
ORDER BY viewname;

-- عرض الـ Functions المُنشأة
SELECT 
    'Created Functions' as Info,
    routine_name,
    routine_type
FROM information_schema.routines 
WHERE routine_schema = 'public'
ORDER BY routine_name;

-- رسالة النجاح النهائية
SELECT '✅ Complete System Setup Completed Successfully!' as Message
UNION ALL
SELECT '👤 Admin Username: admin'
UNION ALL
SELECT '🔑 Admin Password: admin123'
UNION ALL
SELECT '⚠️  Please change the password after first login'
UNION ALL
SELECT '📊 System is ready for use'
UNION ALL
SELECT '🔧 All tables, views, and functions are created'
UNION ALL
SELECT '🎉 HR Aviation System is now fully operational!';