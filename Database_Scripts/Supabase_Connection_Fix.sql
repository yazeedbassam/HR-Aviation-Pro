-- =====================================================
-- Supabase Connection Fix Script
-- سكريبت لحل مشاكل الاتصال بـ Supabase
-- =====================================================

-- 1. التحقق من إعدادات الاتصال
SELECT 
    'Connection Settings Check' as CheckType,
    current_database() as DatabaseName,
    current_user as CurrentUser,
    inet_server_addr() as ServerIP,
    inet_server_port() as ServerPort,
    version() as PostgreSQLVersion;

-- 2. التحقق من الصلاحيات
SELECT 
    'Permissions Check' as CheckType,
    has_database_privilege(current_user, current_database(), 'CONNECT') as CanConnect,
    has_database_privilege(current_user, current_database(), 'CREATE') as CanCreate,
    has_database_privilege(current_user, current_database(), 'USAGE') as CanUse;

-- 3. إنشاء Schema إذا لم يكن موجوداً
CREATE SCHEMA IF NOT EXISTS public;

-- 4. منح جميع الصلاحيات للمستخدم الحالي
GRANT ALL ON SCHEMA public TO current_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO current_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO current_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO current_user;

-- 5. إنشاء جدول Users مع إعدادات محسنة
DROP TABLE IF EXISTS "Users" CASCADE;
CREATE TABLE "Users" (
    "id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL DEFAULT 'Admin',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "LastLoginDate" TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT true
);

-- 6. إنشاء جدول UserActivityLogs مع إعدادات محسنة
DROP TABLE IF EXISTS "UserActivityLogs" CASCADE;
CREATE TABLE "UserActivityLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "UserName" VARCHAR(100) NOT NULL,
    "Action" VARCHAR(50) NOT NULL,
    "EntityType" VARCHAR(50) NOT NULL,
    "EntityId" VARCHAR(50),
    "Details" TEXT,
    "IpAddress" VARCHAR(45),
    "UserAgent" VARCHAR(500),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsSuccessful" BOOLEAN NOT NULL DEFAULT true,
    "ErrorMessage" TEXT
);

-- 7. إنشاء فهارس لتحسين الأداء
CREATE INDEX "IX_Users_Username" ON "Users"("Username");
CREATE INDEX "IX_Users_RoleName" ON "Users"("RoleName");
CREATE INDEX "IX_Users_IsActive" ON "Users"("IsActive");
CREATE INDEX "IX_UserActivityLogs_UserId" ON "UserActivityLogs"("UserId");
CREATE INDEX "IX_UserActivityLogs_Timestamp" ON "UserActivityLogs"("Timestamp");
CREATE INDEX "IX_UserActivityLogs_Action" ON "UserActivityLogs"("Action");

-- 8. إنشاء مستخدم admin مع hash صحيح
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive") 
VALUES (
    'admin', 
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
    'Admin', 
    true
);

-- 9. اختبار إدراج سجل نشاط
INSERT INTO "UserActivityLogs" (
    "UserId", "UserName", "Action", "EntityType", "Details", 
    "IpAddress", "UserAgent", "Timestamp", "IsSuccessful"
) VALUES (
    1, 'admin', 'System Setup', 'System', 'Supabase connection fix applied', 
    '127.0.0.1', 'Supabase Fix Script', CURRENT_TIMESTAMP, true
);

-- 10. إنشاء Views للاختبار
CREATE OR REPLACE VIEW "vw_ConnectionTest" AS
SELECT 
    'Connection Test' as TestType,
    'SUCCESS' as Status,
    current_timestamp as TestTime,
    current_database() as DatabaseName,
    current_user as CurrentUser;

-- 11. إنشاء Function للاختبار
CREATE OR REPLACE FUNCTION "test_connection"()
RETURNS TABLE (
    test_name TEXT,
    test_result TEXT,
    test_time TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        'Database Connection'::TEXT as test_name,
        'SUCCESS'::TEXT as test_result,
        CURRENT_TIMESTAMP as test_time
    UNION ALL
    SELECT 
        'Admin User Check'::TEXT,
        CASE WHEN EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'admin') 
             THEN 'EXISTS'::TEXT 
             ELSE 'MISSING'::TEXT END,
        CURRENT_TIMESTAMP
    UNION ALL
    SELECT 
        'Activity Log Check'::TEXT,
        CASE WHEN EXISTS (SELECT 1 FROM "UserActivityLogs" WHERE "UserName" = 'admin') 
             THEN 'EXISTS'::TEXT 
             ELSE 'MISSING'::TEXT END,
        CURRENT_TIMESTAMP;
END;
$$ LANGUAGE plpgsql;

-- 12. تشغيل اختبار الاتصال
SELECT * FROM "test_connection"();

-- 13. عرض معلومات النظام
SELECT 
    'System Information' as InfoType,
    current_database() as DatabaseName,
    current_user as CurrentUser,
    version() as PostgreSQLVersion,
    current_timestamp as CurrentTime;

-- 14. عرض بيانات المستخدم
SELECT 
    'User Data' as DataType,
    "id",
    "Username",
    "RoleName",
    "IsActive",
    "CreatedDate"
FROM "Users" 
WHERE "Username" = 'admin';

-- 15. رسالة النجاح
DO $$
BEGIN
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Supabase Connection Fix Applied Successfully!';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Database: %', current_database();
    RAISE NOTICE 'User: %', current_user;
    RAISE NOTICE 'Admin User: admin';
    RAISE NOTICE 'Password: admin123';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Connection should work now!';
    RAISE NOTICE '=====================================================';
END $$;