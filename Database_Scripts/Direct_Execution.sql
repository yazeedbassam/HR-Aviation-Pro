-- =====================================================
-- تنفيذ مباشر لإنشاء مستخدم admin
-- Direct execution to create admin user
-- =====================================================

-- 1. إنشاء/تحديث المستخدم admin
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive", "CreatedDate")
VALUES ('admin', '123', 'SuperAdmin', true, NOW())
ON CONFLICT ("Username") DO UPDATE SET
    "RoleName" = 'SuperAdmin',
    "IsActive" = true,
    "PasswordHash" = '123';

-- 2. التحقق من النتيجة
SELECT '✅ تم إنشاء المستخدم admin بنجاح!' as "Result",
       "Username",
       "RoleName",
       "IsActive"
FROM "Users" 
WHERE "Username" = 'admin';