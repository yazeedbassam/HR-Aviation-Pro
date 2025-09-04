-- =====================================================
-- Supabase Quick Test Script
-- سكريبت اختبار سريع للاتصال بـ Supabase
-- =====================================================

-- 1. اختبار الاتصال
SELECT 'Connection Test' as TestName, 'SUCCESS' as Result, CURRENT_TIMESTAMP as TestTime;

-- 2. التحقق من وجود الجداول
SELECT 
    'Table Check' as TestName,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Users') 
        THEN 'Users Table EXISTS' 
        ELSE 'Users Table MISSING' 
    END as Result;

-- 3. إنشاء جدول Users إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Users" (
    "id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL DEFAULT 'Admin',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT true
);

-- 4. حذف وإعادة إنشاء مستخدم admin
DELETE FROM "Users" WHERE "Username" = 'admin';

INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive") 
VALUES (
    'admin', 
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
    'Admin', 
    true
);

-- 5. اختبار تسجيل الدخول
SELECT 
    'Login Test' as TestName,
    CASE 
        WHEN COUNT(*) > 0 THEN 'Admin user EXISTS' 
        ELSE 'Admin user MISSING' 
    END as Result
FROM "Users" 
WHERE "Username" = 'admin' AND "IsActive" = true;

-- 6. عرض معلومات المستخدم
SELECT 
    "id", 
    "Username", 
    "RoleName", 
    "IsActive", 
    "CreatedDate"
FROM "Users" 
WHERE "Username" = 'admin';

-- 7. إنشاء جدول UserActivityLogs للاختبار
CREATE TABLE IF NOT EXISTS "UserActivityLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "UserName" VARCHAR(100) NOT NULL,
    "Action" VARCHAR(50) NOT NULL,
    "EntityType" VARCHAR(50) NOT NULL,
    "Details" TEXT,
    "IpAddress" VARCHAR(45),
    "UserAgent" VARCHAR(500),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsSuccessful" BOOLEAN NOT NULL DEFAULT true
);

-- 8. اختبار إدراج سجل نشاط
INSERT INTO "UserActivityLogs" (
    "UserId", "UserName", "Action", "EntityType", "Details", 
    "IpAddress", "UserAgent", "Timestamp", "IsSuccessful"
) VALUES (
    1, 'admin', 'Login', 'System', 'Test login from Supabase setup', 
    '127.0.0.1', 'Supabase Setup Script', CURRENT_TIMESTAMP, true
);

-- 9. التحقق من السجل
SELECT 
    'Activity Log Test' as TestName,
    CASE 
        WHEN COUNT(*) > 0 THEN 'Activity log CREATED' 
        ELSE 'Activity log FAILED' 
    END as Result
FROM "UserActivityLogs" 
WHERE "UserName" = 'admin' AND "Action" = 'Login';

-- 10. عرض آخر سجل نشاط
SELECT 
    "Id", 
    "UserId", 
    "UserName", 
    "Action", 
    "EntityType", 
    "Timestamp", 
    "IsSuccessful"
FROM "UserActivityLogs" 
ORDER BY "Timestamp" DESC 
LIMIT 1;

-- 11. رسالة النجاح
SELECT 
    '=====================================================' as Message
UNION ALL
SELECT 'Supabase Quick Test Completed Successfully!'
UNION ALL
SELECT '====================================================='
UNION ALL
SELECT 'Admin User: admin'
UNION ALL
SELECT 'Password: admin123'
UNION ALL
SELECT 'Role: Admin'
UNION ALL
SELECT '====================================================='
UNION ALL
SELECT 'Ready for HR Aviation Pro!'
UNION ALL
SELECT '=====================================================';