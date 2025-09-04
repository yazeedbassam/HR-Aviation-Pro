-- 🚀 سكربت إنشاء قاعدة البيانات السريع
-- HR Aviation System - PostgreSQL Setup

-- إنشاء الجداول الأساسية
CREATE TABLE IF NOT EXISTS "Users" (
    id SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL DEFAULT 'User',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT true
);

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

CREATE TABLE IF NOT EXISTS "Employees" (
    "EmployeeId" SERIAL PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Position" VARCHAR(100),
    "Department" VARCHAR(100),
    "HireDate" DATE,
    "Email" VARCHAR(255),
    "Phone" VARCHAR(20),
    "IsActive" BOOLEAN DEFAULT true,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Certificates" (
    "CertificateId" SERIAL PRIMARY KEY,
    "EmployeeId" INTEGER,
    "CertificateType" VARCHAR(100) NOT NULL,
    "IssueDate" DATE,
    "ExpiryDate" DATE,
    "Status" VARCHAR(50) DEFAULT 'Active',
    "Notes" TEXT,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EmployeeId") REFERENCES "Employees"("EmployeeId") ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS "Observations" (
    "ObservationId" SERIAL PRIMARY KEY,
    "EmployeeId" INTEGER,
    "ObservationType" VARCHAR(100) NOT NULL,
    "ObservationDate" DATE,
    "Description" TEXT,
    "Status" VARCHAR(50) DEFAULT 'Open',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EmployeeId") REFERENCES "Employees"("EmployeeId") ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS "Projects" (
    "ProjectId" SERIAL PRIMARY KEY,
    "ProjectName" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "StartDate" DATE,
    "EndDate" DATE,
    "Status" VARCHAR(50) DEFAULT 'Active',
    "ManagerId" INTEGER,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("ManagerId") REFERENCES "Employees"("EmployeeId") ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS "Notifications" (
    "NotificationId" SERIAL PRIMARY KEY,
    "UserId" INTEGER,
    "Title" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "IsRead" BOOLEAN DEFAULT false,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "Users"(id) ON DELETE CASCADE
);

-- إنشاء الفهارس للأداء
CREATE INDEX IF NOT EXISTS idx_users_username ON "Users"("Username");
CREATE INDEX IF NOT EXISTS idx_users_role ON "Users"("RoleName");
CREATE INDEX IF NOT EXISTS idx_activity_logs_user_id ON "UserActivityLogs"("UserId");
CREATE INDEX IF NOT EXISTS idx_activity_logs_date ON "UserActivityLogs"("CreatedDate");
CREATE INDEX IF NOT EXISTS idx_employees_department ON "Employees"("Department");
CREATE INDEX IF NOT EXISTS idx_certificates_employee ON "Certificates"("EmployeeId");
CREATE INDEX IF NOT EXISTS idx_observations_employee ON "Observations"("EmployeeId");
CREATE INDEX IF NOT EXISTS idx_projects_status ON "Projects"("Status");
CREATE INDEX IF NOT EXISTS idx_notifications_user ON "Notifications"("UserId");

-- إنشاء المستخدم الإداري
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") 
VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin')
ON CONFLICT ("Username") DO NOTHING;

-- إضافة بعض البيانات التجريبية
INSERT INTO "Employees" ("FirstName", "LastName", "Position", "Department", "HireDate", "Email", "Phone") 
VALUES 
    ('أحمد', 'محمد', 'مدير النظام', 'IT', '2024-01-01', 'ahmed@company.com', '+966501234567'),
    ('فاطمة', 'علي', 'محاسبة', 'المالية', '2024-02-01', 'fatima@company.com', '+966501234568'),
    ('خالد', 'سعد', 'مهندس', 'الهندسة', '2024-03-01', 'khalid@company.com', '+966501234569')
ON CONFLICT DO NOTHING;

-- إضافة إشعار ترحيبي
INSERT INTO "Notifications" ("UserId", "Title", "Message") 
SELECT id, 'مرحباً بك في النظام', 'تم إنشاء النظام بنجاح. يمكنك الآن البدء في استخدام جميع الميزات المتاحة.'
FROM "Users" WHERE "Username" = 'admin'
ON CONFLICT DO NOTHING;

-- عرض النتائج
SELECT '✅ تم إنشاء الجداول بنجاح' as status;
SELECT COUNT(*) as total_users FROM "Users";
SELECT COUNT(*) as total_employees FROM "Employees";
SELECT COUNT(*) as total_notifications FROM "Notifications";

-- عرض معلومات المستخدم الإداري
SELECT "Username", "RoleName", "CreatedDate" FROM "Users" WHERE "Username" = 'admin';