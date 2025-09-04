-- =====================================================
-- Supabase Quick Setup - إعداد سريع وبسيط
-- =====================================================

-- 1. حذف الجداول الموجودة لتجنب التعارض
-- =====================================================

DROP TABLE IF EXISTS "UserActivityLogs" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

-- 2. إنشاء الجداول الأساسية فقط
-- =====================================================

-- جدول Users
CREATE TABLE "Users" (
    "UserId" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(200) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL,
    "LastPermissionUpdate" TIMESTAMP
);

-- جدول UserActivityLogs
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

-- 3. إنشاء مستخدم admin
-- =====================================================

INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") VALUES 
('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin');

-- 4. إدراج سجل اختبار
-- =====================================================

INSERT INTO "UserActivityLogs" (
    "UserId", "UserName", "Action", "EntityType", "Details", 
    "IpAddress", "UserAgent", "Timestamp", "IsSuccessful"
) VALUES (
    1, 'admin', 'System Setup', 'System', 'Quick setup completed', 
    '127.0.0.1', 'Supabase Quick Setup', CURRENT_TIMESTAMP, true
);

-- 5. رسالة النجاح
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Supabase Quick Setup Successful!';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Admin User: admin';
    RAISE NOTICE 'Password: admin123';
    RAISE NOTICE 'Ready for testing!';
    RAISE NOTICE '=====================================================';
END $$;

-- 6. التحقق من البيانات
-- =====================================================

SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM "Users"
UNION ALL
SELECT 'UserActivityLogs' as TableName, COUNT(*) as RecordCount FROM "UserActivityLogs";

SELECT 'Admin User' as Info, "Username", "RoleName" FROM "Users" WHERE "Username" = 'admin';