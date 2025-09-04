-- =====================================================
-- Create Admin User Only
-- =====================================================
-- سكريبت لإنشاء المستخدم الإداري فقط
-- يجب تنفيذه في Supabase SQL Editor
-- يتطلب وجود الجداول الأساسية أولاً

-- التحقق من وجود الجداول المطلوبة
DO $$
BEGIN
    -- التحقق من وجود جدول Users
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Users') THEN
        RAISE EXCEPTION 'Table Users does not exist. Please run Create_Basic_Tables.sql first.';
    END IF;
    
    -- التحقق من وجود جدول Permissions
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Permissions') THEN
        RAISE EXCEPTION 'Table Permissions does not exist. Please run Create_Basic_Tables.sql first.';
    END IF;
    
    -- التحقق من وجود جدول UserPermissions
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserPermissions') THEN
        RAISE EXCEPTION 'Table UserPermissions does not exist. Please run Create_Basic_Tables.sql first.';
    END IF;
END $$;

-- إدراج الصلاحيات الأساسية إذا لم تكن موجودة
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

-- منح جميع الصلاحيات للمستخدم الإداري
INSERT INTO UserPermissions (UserId, PermissionId)
SELECT u.UserId, p.PermissionId
FROM Users u, Permissions p
WHERE u.Username = 'admin' AND p.IsActive = TRUE
ON CONFLICT (UserId, PermissionId) DO NOTHING;

-- إنشاء الأقسام إذا لم تكن موجودة
INSERT INTO Departments (DepartmentName, Description) VALUES
('إدارة النظام', 'إدارة النظام والصلاحيات'),
('الموارد البشرية', 'إدارة الموظفين والمراقبين'),
('التدريب والتطوير', 'إدارة الشهادات والتراخيص'),
('المشاريع', 'إدارة المشاريع والأنشطة'),
('الملاحظات والتقييم', 'إدارة الملاحظات والتقييمات'),
('التقارير والإحصائيات', 'إنتاج التقارير والإحصائيات')
ON CONFLICT (DepartmentName) DO NOTHING;

-- منح صلاحيات كاملة لجميع الأقسام للمستخدم الإداري (إذا كان جدول UserDepartmentPermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserDepartmentPermissions') THEN
        INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, CanView, CanEdit, CanDelete, CanCreate)
        SELECT u.UserId, d.DepartmentId, TRUE, TRUE, TRUE, TRUE
        FROM Users u, Departments d
        WHERE u.Username = 'admin' AND d.IsActive = TRUE
        ON CONFLICT (UserId, DepartmentId) DO UPDATE SET
            CanView = TRUE,
            CanEdit = TRUE,
            CanDelete = TRUE,
            CanCreate = TRUE;
    END IF;
END $$;

-- تسجيل إنشاء المستخدم الإداري في سجل الأنشطة (إذا كان جدول ActivityLog موجود)
DO $$
DECLARE
    admin_user_id INTEGER;
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ActivityLog') THEN
        SELECT UserId INTO admin_user_id FROM Users WHERE Username = 'admin';
        
        INSERT INTO ActivityLog (UserId, Username, Action, EntityType, Details, IsSuccessful)
        VALUES (admin_user_id, 'admin', 'Admin User Created', 'User', 'تم إنشاء المستخدم الإداري', TRUE);
    END IF;
END $$;

-- عرض معلومات المستخدم الإداري
SELECT 
    'Admin User Created Successfully' as Status,
    Username,
    FullName,
    Email,
    Role,
    IsActive,
    CreatedDate
FROM Users 
WHERE Username = 'admin';

-- عرض إجمالي الصلاحيات الممنوحة
SELECT 
    'Admin Permissions' as Info,
    COUNT(*) as TotalPermissions
FROM UserPermissions up
JOIN Users u ON up.UserId = u.UserId
WHERE u.Username = 'admin';

-- عرض الصلاحيات الممنوحة
SELECT 
    'Granted Permissions' as Info,
    p.PermissionName,
    p.Description,
    p.Category
FROM UserPermissions up
JOIN Users u ON up.UserId = u.UserId
JOIN Permissions p ON up.PermissionId = p.PermissionId
WHERE u.Username = 'admin'
ORDER BY p.Category, p.PermissionName;

-- رسالة النجاح
SELECT '✅ Admin user created successfully!' as Message
UNION ALL
SELECT '👤 Username: admin'
UNION ALL
SELECT '🔑 Password: admin123'
UNION ALL
SELECT '⚠️  Please change the password after first login'
UNION ALL
SELECT '📊 Admin user has all system permissions';