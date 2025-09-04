-- =====================================================
-- Supabase Admin User Setup Script
-- =====================================================
-- هذا السكربت ينشئ مستخدم إداري مع جميع الصلاحيات
-- يجب تنفيذه في Supabase SQL Editor

-- إنشاء جدول المستخدمين إذا لم يكن موجوداً
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

-- إنشاء جدول الصلاحيات إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId SERIAL PRIMARY KEY,
    PermissionName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    Category VARCHAR(50),
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات المستخدمين إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS UserPermissions (
    UserPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE CASCADE,
    PermissionId INTEGER REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    GrantedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    GrantedBy INTEGER REFERENCES Users(UserId),
    UNIQUE(UserId, PermissionId)
);

-- إنشاء جدول الأقسام إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS Departments (
    DepartmentId SERIAL PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات الأقسام إذا لم يكن موجوداً
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

-- إدراج الصلاحيات الأساسية
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

-- إدراج الأقسام الأساسية
INSERT INTO Departments (DepartmentName, Description) VALUES
('إدارة النظام', 'إدارة النظام والصلاحيات'),
('الموارد البشرية', 'إدارة الموظفين والمراقبين'),
('التدريب والتطوير', 'إدارة الشهادات والتراخيص'),
('المشاريع', 'إدارة المشاريع والأنشطة'),
('الملاحظات والتقييم', 'إدارة الملاحظات والتقييمات'),
('التقارير والإحصائيات', 'إنتاج التقارير والإحصائيات')

ON CONFLICT (DepartmentName) DO NOTHING;

-- إنشاء المستخدم الإداري
-- كلمة المرور: admin123 (يجب تغييرها بعد أول تسجيل دخول)
INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES
('admin', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'مدير النظام', 'admin@hr-aviation.com', 'SystemAdmin')
ON CONFLICT (Username) DO UPDATE SET
    Password = EXCLUDED.Password,
    FullName = EXCLUDED.FullName,
    Email = EXCLUDED.Email,
    Role = EXCLUDED.Role,
    IsActive = TRUE;

-- الحصول على معرف المستخدم الإداري
DO $$
DECLARE
    admin_user_id INTEGER;
BEGIN
    -- الحصول على معرف المستخدم الإداري
    SELECT UserId INTO admin_user_id FROM Users WHERE Username = 'admin';
    
    -- منح جميع الصلاحيات للمستخدم الإداري
    INSERT INTO UserPermissions (UserId, PermissionId, GrantedBy)
    SELECT admin_user_id, PermissionId, admin_user_id
    FROM Permissions
    WHERE IsActive = TRUE
    ON CONFLICT (UserId, PermissionId) DO NOTHING;
    
    -- منح صلاحيات كاملة لجميع الأقسام للمستخدم الإداري
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, CanView, CanEdit, CanDelete, CanCreate)
    SELECT admin_user_id, DepartmentId, TRUE, TRUE, TRUE, TRUE
    FROM Departments
    WHERE IsActive = TRUE
    ON CONFLICT (UserId, DepartmentId) DO UPDATE SET
        CanView = TRUE,
        CanEdit = TRUE,
        CanDelete = TRUE,
        CanCreate = TRUE;
END $$;

-- إنشاء فهارس لتحسين الأداء
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_active ON Users(IsActive);
CREATE INDEX IF NOT EXISTS idx_userpermissions_userid ON UserPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userpermissions_permissionid ON UserPermissions(PermissionId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_userid ON UserDepartmentPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_departmentid ON UserDepartmentPermissions(DepartmentId);

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
    COUNT(DISTINCT udp.DepartmentId) as TotalDepartments,
    STRING_AGG(DISTINCT p.PermissionName, ', ') as PermissionNames,
    STRING_AGG(DISTINCT d.DepartmentName, ', ') as DepartmentNames
FROM Users u
LEFT JOIN UserPermissions up ON u.UserId = up.UserId
LEFT JOIN Permissions p ON up.PermissionId = p.PermissionId AND p.IsActive = TRUE
LEFT JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId
LEFT JOIN Departments d ON udp.DepartmentId = d.DepartmentId AND d.IsActive = TRUE
GROUP BY u.UserId, u.Username, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedDate, u.LastLoginDate;

-- إنشاء view لعرض الصلاحيات المتاحة
CREATE OR REPLACE VIEW AvailablePermissionsView AS
SELECT 
    p.PermissionId,
    p.PermissionName,
    p.Description,
    p.Category,
    COUNT(up.UserId) as AssignedUsersCount
FROM Permissions p
LEFT JOIN UserPermissions up ON p.PermissionId = up.PermissionId
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
    COUNT(CASE WHEN udp.CanCreate = TRUE THEN 1 END) as CanCreateCount
FROM Departments d
LEFT JOIN UserDepartmentPermissions udp ON d.DepartmentId = udp.DepartmentId
WHERE d.IsActive = TRUE
GROUP BY d.DepartmentId, d.DepartmentName, d.Description
ORDER BY d.DepartmentName;

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
    p_permission_type VARCHAR(20) -- 'View', 'Edit', 'Delete', 'Create'
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
    -- التحقق من وجود المستخدم
    SELECT EXISTS(
        SELECT 1 FROM Users WHERE Username = p_username AND IsActive = TRUE
    ) INTO user_exists;
    
    IF user_exists THEN
        -- تحديث تاريخ آخر تسجيل دخول
        UPDATE Users 
        SET LastLoginDate = CURRENT_TIMESTAMP 
        WHERE Username = p_username;
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتغيير كلمة مرور المستخدم
CREATE OR REPLACE FUNCTION ChangeUserPassword(
    p_username VARCHAR(50),
    p_old_password VARCHAR(255),
    p_new_password VARCHAR(255)
) RETURNS BOOLEAN AS $$
DECLARE
    password_matches BOOLEAN := FALSE;
BEGIN
    -- التحقق من كلمة المرور القديمة
    SELECT (Password = p_old_password) INTO password_matches
    FROM Users 
    WHERE Username = p_username AND IsActive = TRUE;
    
    IF password_matches THEN
        -- تحديث كلمة المرور
        UPDATE Users 
        SET Password = p_new_password 
        WHERE Username = p_username;
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

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

-- إنشاء فهرس لجدول سجل الأنشطة
CREATE INDEX IF NOT EXISTS idx_activitylog_userid ON ActivityLog(UserId);
CREATE INDEX IF NOT EXISTS idx_activitylog_username ON ActivityLog(Username);
CREATE INDEX IF NOT EXISTS idx_activitylog_action ON ActivityLog(Action);
CREATE INDEX IF NOT EXISTS idx_activitylog_createddate ON ActivityLog(CreatedDate);

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

-- إدراج سجل إنشاء النظام
INSERT INTO ActivityLog (UserId, Username, Action, EntityType, Details, IsSuccessful)
SELECT 
    UserId, 
    Username, 
    'System Setup', 
    'System', 
    'تم إنشاء النظام وإعداد المستخدم الإداري', 
    TRUE
FROM Users 
WHERE Username = 'admin';

-- عرض معلومات المستخدم الإداري
SELECT 
    'تم إنشاء المستخدم الإداري بنجاح' as Status,
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
    'إحصائيات النظام' as Info,
    (SELECT COUNT(*) FROM Permissions WHERE IsActive = TRUE) as TotalPermissions,
    (SELECT COUNT(*) FROM Departments WHERE IsActive = TRUE) as TotalDepartments,
    (SELECT COUNT(*) FROM Users WHERE IsActive = TRUE) as TotalUsers,
    (SELECT COUNT(*) FROM UserPermissions) as TotalUserPermissions;

-- رسالة النجاح
SELECT '✅ تم إعداد النظام بنجاح! يمكنك الآن تسجيل الدخول باستخدام:' as Message
UNION ALL
SELECT '👤 اسم المستخدم: admin'
UNION ALL
SELECT '🔑 كلمة المرور: admin123'
UNION ALL
SELECT '⚠️  يرجى تغيير كلمة المرور بعد أول تسجيل دخول'
UNION ALL
SELECT '📊 يمكنك عرض الصلاحيات والأقسام من خلال الـ Views المُنشأة';