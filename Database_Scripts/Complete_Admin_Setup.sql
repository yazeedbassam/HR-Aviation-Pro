-- =====================================================
-- سكريبت شامل لإنشاء مستخدم admin مع جميع الصلاحيات
-- Complete Admin Setup Script for Railway
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: سكريبت شامل لإنشاء مستخدم admin مع جميع الصلاحيات الممكنة
-- =====================================================

-- 1. إنشاء/تحديث المستخدم admin
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive", "CreatedDate")
VALUES ('admin', '123', 'SuperAdmin', true, NOW())
ON CONFLICT ("Username") DO UPDATE SET
    "RoleName" = 'SuperAdmin',
    "IsActive" = true,
    "PasswordHash" = '123';

-- 2. منح الصلاحيات الأساسية (إذا كان جدول Permissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Permissions') THEN
        INSERT INTO "UserPermissions" ("UserId", "PermissionKey", "IsGranted", "CreatedDate")
        SELECT 
            u."id",
            p."PermissionKey",
            true,
            NOW()
        FROM "Users" u
        CROSS JOIN "Permissions" p
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId", "PermissionKey") DO UPDATE SET
            "IsGranted" = true;
    END IF;
END $$;

-- 3. منح صلاحيات الأقسام (إذا كان جدول UserDepartmentPermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserDepartmentPermissions') THEN
        INSERT INTO "UserDepartmentPermissions" ("UserId", "DepartmentId", "CanView", "CanEdit", "CanDelete", "CanManageUsers", "CreatedDate")
        SELECT 
            u."id",
            d."DepartmentId",
            true, true, true, true,
            NOW()
        FROM "Users" u
        CROSS JOIN "Departments" d
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId", "DepartmentId") DO UPDATE SET
            "CanView" = true,
            "CanEdit" = true,
            "CanDelete" = true,
            "CanManageUsers" = true;
    END IF;
END $$;

-- 4. منح صلاحيات الشهادات (إذا كان جدول UserCertificatePermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserCertificatePermissions') THEN
        INSERT INTO "UserCertificatePermissions" ("UserId", "CertificateType", "CanView", "CanEdit", "CanDelete", "CanApprove", "CreatedDate")
        SELECT 
            u."id",
            unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
            true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId", "CertificateType") DO UPDATE SET
            "CanView" = true,
            "CanEdit" = true,
            "CanDelete" = true,
            "CanApprove" = true;
    END IF;
END $$;

-- 5. منح صلاحيات الرخص (إذا كان جدول UserLicensePermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserLicensePermissions') THEN
        INSERT INTO "UserLicensePermissions" ("UserId", "LicenseType", "CanView", "CanEdit", "CanDelete", "CanApprove", "CreatedDate")
        SELECT 
            u."id",
            unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
            true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId", "LicenseType") DO UPDATE SET
            "CanView" = true,
            "CanEdit" = true,
            "CanDelete" = true,
            "CanApprove" = true;
    END IF;
END $$;

-- 6. منح صلاحيات المراقبة (إذا كان جدول UserObservationPermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserObservationPermissions') THEN
        INSERT INTO "UserObservationPermissions" ("UserId", "ObservationType", "CanView", "CanEdit", "CanDelete", "CanApprove", "CreatedDate")
        SELECT 
            u."id",
            unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
            true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId", "ObservationType") DO UPDATE SET
            "CanView" = true,
            "CanEdit" = true,
            "CanDelete" = true,
            "CanApprove" = true;
    END IF;
END $$;

-- 7. منح صلاحيات الموظفين (إذا كان جدول UserEmployeePermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserEmployeePermissions') THEN
        INSERT INTO "UserEmployeePermissions" ("UserId", "CanViewAll", "CanEditAll", "CanDeleteAll", "CanManageSalary", "CanManageContract", "CreatedDate")
        SELECT 
            u."id",
            true, true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId") DO UPDATE SET
            "CanViewAll" = true,
            "CanEditAll" = true,
            "CanDeleteAll" = true,
            "CanManageSalary" = true,
            "CanManageContract" = true;
    END IF;
END $$;

-- 8. منح صلاحيات النظام (إذا كان جدول UserSystemPermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserSystemPermissions') THEN
        INSERT INTO "UserSystemPermissions" ("UserId", "CanManageUsers", "CanManageRoles", "CanManagePermissions", "CanViewLogs", "CanManageConfig", "CanBackupRestore", "CreatedDate")
        SELECT 
            u."id",
            true, true, true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId") DO UPDATE SET
            "CanManageUsers" = true,
            "CanManageRoles" = true,
            "CanManagePermissions" = true,
            "CanViewLogs" = true,
            "CanManageConfig" = true,
            "CanBackupRestore" = true;
    END IF;
END $$;

-- 9. منح صلاحيات التقارير (إذا كان جدول UserReportPermissions موجود)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserReportPermissions') THEN
        INSERT INTO "UserReportPermissions" ("UserId", "CanViewAllReports", "CanExportReports", "CanScheduleReports", "CanManageReportTemplates", "CreatedDate")
        SELECT 
            u."id",
            true, true, true, true,
            NOW()
        FROM "Users" u
        WHERE u."Username" = 'admin'
        ON CONFLICT ("UserId") DO UPDATE SET
            "CanViewAllReports" = true,
            "CanExportReports" = true,
            "CanScheduleReports" = true,
            "CanManageReportTemplates" = true;
    END IF;
END $$;

-- 10. التحقق من النتيجة
SELECT 
    u."Username",
    u."RoleName",
    u."IsActive",
    u."CreatedDate"
FROM "Users" u
WHERE u."Username" = 'admin';

-- 11. عرض الجداول الموجودة
SELECT 
    table_name as "Tables Found"
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name LIKE '%Permission%'
ORDER BY table_name;

-- 12. رسالة النجاح
SELECT '✅ تم إنشاء المستخدم admin مع جميع الصلاحيات المتاحة بنجاح!' as "Result";