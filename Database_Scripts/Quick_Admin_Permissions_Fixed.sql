-- =====================================================
-- سكريبت سريع لمنح صلاحيات admin - مصحح
-- Quick Admin Permissions Script - Fixed
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: سكريبت مبسط وسريع لمنح جميع الصلاحيات للمستخدم admin
-- تم تصحيح أسماء الجداول والحقول لتتطابق مع قاعدة البيانات الفعلية
-- =====================================================

-- 1. التأكد من وجود المستخدم admin
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive", "CreatedDate")
VALUES ('admin', '123', 'SuperAdmin', true, NOW())
ON CONFLICT ("Username") DO UPDATE SET
    "RoleName" = 'SuperAdmin',
    "IsActive" = true;

-- 2. منح جميع الصلاحيات الأساسية (إذا كان جدول Permissions موجود)
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

-- 3. منح صلاحيات الأقسام (إذا كان جدول UserDepartmentPermissions موجود)
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

-- 4. منح صلاحيات الشهادات (إذا كان جدول UserCertificatePermissions موجود)
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

-- 5. منح صلاحيات الرخص (إذا كان جدول UserLicensePermissions موجود)
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

-- 6. منح صلاحيات المراقبة (إذا كان جدول UserObservationPermissions موجود)
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

-- 7. منح صلاحيات الموظفين (إذا كان جدول UserEmployeePermissions موجود)
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

-- 8. منح صلاحيات النظام (إذا كان جدول UserSystemPermissions موجود)
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

-- 9. منح صلاحيات التقارير (إذا كان جدول UserReportPermissions موجود)
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

-- 10. التحقق من النتيجة
SELECT 
    u."Username",
    u."RoleName",
    u."IsActive",
    u."CreatedDate"
FROM "Users" u
WHERE u."Username" = 'admin';

-- رسالة النجاح
SELECT '✅ تم منح جميع الصلاحيات للمستخدم admin بنجاح!' as "Result";