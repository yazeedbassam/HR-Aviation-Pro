-- =====================================================
-- سكريبت بسيط لإنشاء مستخدم admin مع الصلاحيات الأساسية
-- Simple Admin Setup Script for Railway
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: سكريبت مبسط لإنشاء مستخدم admin مع الصلاحيات الأساسية
-- =====================================================

-- 1. إنشاء/تحديث المستخدم admin
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive", "CreatedDate")
VALUES ('admin', '123', 'SuperAdmin', true, NOW())
ON CONFLICT ("Username") DO UPDATE SET
    "RoleName" = 'SuperAdmin',
    "IsActive" = true,
    "PasswordHash" = '123';

-- 2. التحقق من إنشاء المستخدم
SELECT 
    "Username",
    "RoleName",
    "IsActive",
    "CreatedDate"
FROM "Users" 
WHERE "Username" = 'admin';

-- 3. رسالة النجاح
SELECT '✅ تم إنشاء المستخدم admin بنجاح!' as "Result";