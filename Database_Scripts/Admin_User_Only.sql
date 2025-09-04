-- 🚀 إنشاء المستخدم الإداري فقط
-- HR Aviation System - Admin User Setup

-- إنشاء جدول المستخدمين
CREATE TABLE IF NOT EXISTS "Users" (
    id SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL DEFAULT 'User',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT true
);

-- إنشاء جدول سجل النشاط
CREATE TABLE IF NOT EXISTS "UserActivityLogs" (
    "LogId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "Action" VARCHAR(100) NOT NULL,
    "Details" TEXT,
    "IpAddress" VARCHAR(45),
    "UserAgent" TEXT,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "Users"(id) ON DELETE CASCADE
);

-- إنشاء المستخدم الإداري
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") 
VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin')
ON CONFLICT ("Username") DO NOTHING;

-- عرض النتيجة
SELECT '✅ تم إنشاء المستخدم الإداري بنجاح' as status;
SELECT "Username", "RoleName", "CreatedDate" FROM "Users" WHERE "Username" = 'admin';