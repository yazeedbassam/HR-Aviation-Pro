-- =====================================================
-- Simple Admin User Setup for Supabase
-- =====================================================
-- سكربت مبسط لإعداد المستخدم الإداري
-- يجب تنفيذه في Supabase SQL Editor

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

-- إدراج الصلاحيات الأساسية
INSERT INTO Permissions (PermissionName, Description, Category) VALUES
('SystemAdmin', 'إدارة النظام الكاملة', 'System'),
('UserManagement', 'إدارة المستخدمين', 'System'),
('EmployeeView', 'عرض الموظفين', 'Employee'),
('EmployeeCreate', 'إضافة موظفين', 'Employee'),
('EmployeeEdit', 'تعديل الموظفين', 'Employee'),
('EmployeeDelete', 'حذف الموظفين', 'Employee'),
('ControllerView', 'عرض المراقبين', 'Controller'),
('ControllerCreate', 'إضافة مراقبين', 'Controller'),
('ControllerEdit', 'تعديل المراقبين', 'Controller'),
('ControllerDelete', 'حذف المراقبين', 'Controller'),
('CertificateView', 'عرض الشهادات', 'Certificate'),
('CertificateCreate', 'إضافة شهادات', 'Certificate'),
('CertificateEdit', 'تعديل الشهادات', 'Certificate'),
('CertificateDelete', 'حذف الشهادات', 'Certificate'),
('LicenseView', 'عرض التراخيص', 'License'),
('LicenseCreate', 'إضافة تراخيص', 'License'),
('LicenseEdit', 'تعديل التراخيص', 'License'),
('LicenseDelete', 'حذف التراخيص', 'License'),
('ProjectView', 'عرض المشاريع', 'Project'),
('ProjectCreate', 'إضافة مشاريع', 'Project'),
('ProjectEdit', 'تعديل المشاريع', 'Project'),
('ProjectDelete', 'حذف المشاريع', 'Project'),
('ObservationView', 'عرض الملاحظات', 'Observation'),
('ObservationCreate', 'إضافة ملاحظات', 'Observation'),
('ObservationEdit', 'تعديل الملاحظات', 'Observation'),
('ObservationDelete', 'حذف الملاحظات', 'Observation'),
('ReportView', 'عرض التقارير', 'Report'),
('ReportCreate', 'إنشاء تقارير', 'Report'),
('AdvancedPermissionView', 'عرض الصلاحيات المتقدمة', 'Advanced'),
('AdvancedPermissionEdit', 'تعديل الصلاحيات المتقدمة', 'Advanced')
ON CONFLICT (PermissionName) DO NOTHING;

-- إنشاء المستخدم الإداري
-- كلمة المرور: admin123
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

-- إنشاء فهارس
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_userpermissions_userid ON UserPermissions(UserId);

-- عرض النتيجة
SELECT 
    'تم إنشاء المستخدم الإداري بنجاح' as Status,
    Username,
    FullName,
    Email,
    Role,
    IsActive
FROM Users 
WHERE Username = 'admin';

SELECT 
    'إجمالي الصلاحيات' as Info,
    COUNT(*) as TotalPermissions
FROM Permissions 
WHERE IsActive = TRUE;