-- =====================================================
-- Supabase Admin Setup Complete Script
-- إنشاء مستخدم admin مع جميع الصلاحيات
-- =====================================================

-- 1. إنشاء جدول Users إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Users" (
    "id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL DEFAULT 'Admin',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "LastLoginDate" TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT true
);

-- 2. إنشاء جدول UserActivityLogs إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "UserActivityLogs" (
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

-- 3. إنشاء جدول Employees إذا لم يكن موجوداً
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

-- 4. إنشاء جدول Certificates إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Certificates" (
    "CertificateId" SERIAL PRIMARY KEY,
    "EmployeeId" INTEGER REFERENCES "Employees"("EmployeeId"),
    "CertificateType" VARCHAR(100) NOT NULL,
    "IssueDate" DATE NOT NULL,
    "ExpiryDate" DATE,
    "Status" VARCHAR(50) DEFAULT 'Active',
    "Notes" TEXT,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. إنشاء جدول Observations إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Observations" (
    "ObservationId" SERIAL PRIMARY KEY,
    "EmployeeId" INTEGER REFERENCES "Employees"("EmployeeId"),
    "ObservationType" VARCHAR(100) NOT NULL,
    "ObservationDate" DATE NOT NULL,
    "Description" TEXT,
    "Status" VARCHAR(50) DEFAULT 'Open',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 6. إنشاء جدول Projects إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Projects" (
    "ProjectId" SERIAL PRIMARY KEY,
    "ProjectName" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "StartDate" DATE,
    "EndDate" DATE,
    "Status" VARCHAR(50) DEFAULT 'Active',
    "ManagerId" INTEGER,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 7. إنشاء جدول Notifications إذا لم يكن موجوداً
CREATE TABLE IF NOT EXISTS "Notifications" (
    "NotificationId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("id"),
    "Title" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "Type" VARCHAR(50) DEFAULT 'Info',
    "IsRead" BOOLEAN DEFAULT false,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 8. حذف مستخدم admin إذا كان موجوداً (لإعادة إنشائه)
DELETE FROM "Users" WHERE "Username" = 'admin';

-- 9. إنشاء مستخدم admin جديد مع hash كلمة المرور admin123
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive") 
VALUES (
    'admin', 
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
    'Admin', 
    true
);

-- 10. إنشاء فهارس لتحسين الأداء
CREATE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users"("Username");
CREATE INDEX IF NOT EXISTS "IX_Users_RoleName" ON "Users"("RoleName");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_UserId" ON "UserActivityLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_Timestamp" ON "UserActivityLogs"("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Employees_Email" ON "Employees"("Email");
CREATE INDEX IF NOT EXISTS "IX_Certificates_EmployeeId" ON "Certificates"("EmployeeId");
CREATE INDEX IF NOT EXISTS "IX_Certificates_ExpiryDate" ON "Certificates"("ExpiryDate");
CREATE INDEX IF NOT EXISTS "IX_Observations_EmployeeId" ON "Observations"("EmployeeId");
CREATE INDEX IF NOT EXISTS "IX_Projects_Status" ON "Projects"("Status");
CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId" ON "Notifications"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_IsRead" ON "Notifications"("IsRead");

-- 11. إضافة بيانات تجريبية للاختبار
INSERT INTO "Employees" ("FirstName", "LastName", "Position", "Department", "HireDate", "Email", "IsActive") 
VALUES 
    ('John', 'Doe', 'Air Traffic Controller', 'ATC', '2023-01-15', 'john.doe@aviation.com', true),
    ('Jane', 'Smith', 'Pilot', 'Flight Operations', '2023-02-20', 'jane.smith@aviation.com', true),
    ('Mike', 'Johnson', 'Maintenance Engineer', 'Maintenance', '2023-03-10', 'mike.johnson@aviation.com', true);

-- 12. إضافة شهادات تجريبية
INSERT INTO "Certificates" ("EmployeeId", "CertificateType", "IssueDate", "ExpiryDate", "Status") 
VALUES 
    (1, 'ATC License', '2023-01-15', '2025-01-15', 'Active'),
    (2, 'Pilot License', '2023-02-20', '2025-02-20', 'Active'),
    (3, 'Maintenance Certificate', '2023-03-10', '2025-03-10', 'Active');

-- 13. إضافة مشاريع تجريبية
INSERT INTO "Projects" ("ProjectName", "Description", "StartDate", "EndDate", "Status", "ManagerId") 
VALUES 
    ('Airport Expansion', 'Expansion of runway and terminal facilities', '2024-01-01', '2024-12-31', 'Active', 1),
    ('Safety Training Program', 'Comprehensive safety training for all staff', '2024-02-01', '2024-08-31', 'Active', 2);

-- 14. إضافة إشعارات تجريبية
INSERT INTO "Notifications" ("UserId", "Title", "Message", "Type", "IsRead") 
VALUES 
    (1, 'Welcome to HR Aviation Pro', 'Welcome to the HR Aviation Pro system!', 'Info', false),
    (1, 'System Update', 'The system has been updated with new features.', 'Info', false);

-- 15. التحقق من البيانات
SELECT 'Users Table' as TableName, COUNT(*) as RecordCount FROM "Users"
UNION ALL
SELECT 'Employees Table', COUNT(*) FROM "Employees"
UNION ALL
SELECT 'Certificates Table', COUNT(*) FROM "Certificates"
UNION ALL
SELECT 'Projects Table', COUNT(*) FROM "Projects"
UNION ALL
SELECT 'Notifications Table', COUNT(*) FROM "Notifications"
UNION ALL
SELECT 'UserActivityLogs Table', COUNT(*) FROM "UserActivityLogs";

-- 16. التحقق من مستخدم admin
SELECT "id", "Username", "RoleName", "IsActive", "CreatedDate" 
FROM "Users" 
WHERE "Username" = 'admin';

-- 17. إنشاء Views مفيدة
CREATE OR REPLACE VIEW "vw_UserSummary" AS
SELECT 
    u."id",
    u."Username",
    u."RoleName",
    u."IsActive",
    u."CreatedDate",
    u."LastLoginDate",
    COUNT(ual."Id") as "ActivityCount"
FROM "Users" u
LEFT JOIN "UserActivityLogs" ual ON u."id" = ual."UserId"
GROUP BY u."id", u."Username", u."RoleName", u."IsActive", u."CreatedDate", u."LastLoginDate";

CREATE OR REPLACE VIEW "vw_EmployeeSummary" AS
SELECT 
    e."EmployeeId",
    e."FirstName" || ' ' || e."LastName" as "FullName",
    e."Position",
    e."Department",
    e."Email",
    e."IsActive",
    COUNT(c."CertificateId") as "CertificateCount",
    COUNT(o."ObservationId") as "ObservationCount"
FROM "Employees" e
LEFT JOIN "Certificates" c ON e."EmployeeId" = c."EmployeeId"
LEFT JOIN "Observations" o ON e."EmployeeId" = o."EmployeeId"
GROUP BY e."EmployeeId", e."FirstName", e."LastName", e."Position", e."Department", e."Email", e."IsActive";

-- 18. إنشاء Functions مفيدة
CREATE OR REPLACE FUNCTION "sp_GetUserByUsername"(username_param VARCHAR(50))
RETURNS TABLE (
    user_id INTEGER,
    username VARCHAR(50),
    password_hash VARCHAR(255),
    role_name VARCHAR(50),
    is_active BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT u."id", u."Username", u."PasswordHash", u."RoleName", u."IsActive"
    FROM "Users" u
    WHERE u."Username" = username_param;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "sp_LogUserActivity"(
    user_id_param INTEGER,
    username_param VARCHAR(100),
    action_param VARCHAR(50),
    entity_type_param VARCHAR(50),
    details_param TEXT DEFAULT NULL,
    ip_address_param VARCHAR(45) DEFAULT NULL,
    user_agent_param VARCHAR(500) DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    log_id INTEGER;
BEGIN
    INSERT INTO "UserActivityLogs" (
        "UserId", "UserName", "Action", "EntityType", "Details", 
        "IpAddress", "UserAgent", "Timestamp", "IsSuccessful"
    ) VALUES (
        user_id_param, username_param, action_param, entity_type_param, 
        details_param, ip_address_param, user_agent_param, CURRENT_TIMESTAMP, true
    ) RETURNING "Id" INTO log_id;
    
    RETURN log_id;
END;
$$ LANGUAGE plpgsql;

-- 19. منح الصلاحيات
-- تأكد من أن المستخدم الحالي لديه صلاحيات كاملة
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres;

-- 20. رسالة النجاح
DO $$
BEGIN
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Supabase Admin Setup Complete!';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Admin User Created:';
    RAISE NOTICE 'Username: admin';
    RAISE NOTICE 'Password: admin123';
    RAISE NOTICE 'Role: Admin';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Tables Created: Users, UserActivityLogs, Employees,';
    RAISE NOTICE 'Certificates, Observations, Projects, Notifications';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Views Created: vw_UserSummary, vw_EmployeeSummary';
    RAISE NOTICE 'Functions Created: sp_GetUserByUsername, sp_LogUserActivity';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Setup completed successfully!';
    RAISE NOTICE '=====================================================';
END $$;