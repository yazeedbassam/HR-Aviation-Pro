-- =====================================================
-- Supabase Complete Migration Script
-- تحويل كامل من SQL Server إلى PostgreSQL/Supabase
-- =====================================================

-- 1. إنشاء الجداول الأساسية
-- =====================================================

-- جدول ConfigurationCategories
CREATE TABLE IF NOT EXISTS "ConfigurationCategories" (
    "CategoryId" SERIAL PRIMARY KEY,
    "CategoryName" VARCHAR(100) UNIQUE NOT NULL,
    "DisplayName" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "IsActive" BOOLEAN DEFAULT true,
    "DisplayOrder" INTEGER DEFAULT 0,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" TIMESTAMP
);

-- جدول ConfigurationValues
CREATE TABLE IF NOT EXISTS "ConfigurationValues" (
    "ValueId" SERIAL PRIMARY KEY,
    "CategoryId" INTEGER REFERENCES "ConfigurationCategories"("CategoryId"),
    "ValueKey" VARCHAR(100) NOT NULL,
    "ValueText" VARCHAR(200) NOT NULL,
    "DisplayOrder" INTEGER DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT true,
    "CreatedBy" VARCHAR(100),
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedBy" VARCHAR(100),
    "ModifiedDate" TIMESTAMP
);

-- جدول Permissions
CREATE TABLE IF NOT EXISTS "Permissions" (
    "PermissionId" SERIAL PRIMARY KEY,
    "PermissionName" VARCHAR(100) NOT NULL,
    "PermissionKey" VARCHAR(50) NOT NULL,
    "PermissionDescription" VARCHAR(500),
    "CategoryName" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- جدول Users
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(200) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL,
    "LastPermissionUpdate" TIMESTAMP
);

-- جدول RolePermissions
CREATE TABLE IF NOT EXISTS "RolePermissions" (
    "RolePermissionId" SERIAL PRIMARY KEY,
    "RoleId" INTEGER NOT NULL,
    "PermissionId" INTEGER REFERENCES "Permissions"("PermissionId"),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- جدول UserDepartmentPermissions
CREATE TABLE IF NOT EXISTS "UserDepartmentPermissions" (
    "UserDepartmentPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "DepartmentId" INTEGER NOT NULL,
    "PermissionId" INTEGER REFERENCES "Permissions"("PermissionId"),
    "CanView" BOOLEAN NOT NULL DEFAULT false,
    "CanEdit" BOOLEAN NOT NULL DEFAULT false,
    "CanDelete" BOOLEAN NOT NULL DEFAULT false,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- جدول UserActivityLogs
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

-- جدول Airports
CREATE TABLE IF NOT EXISTS "Airports" (
    "AirportId" SERIAL PRIMARY KEY,
    "AirportName" VARCHAR(100) NOT NULL,
    "CountryId" INTEGER NOT NULL,
    "Icao_Code" VARCHAR(10)
);

-- جدول Countries
CREATE TABLE IF NOT EXISTS "Countries" (
    "CountryId" SERIAL PRIMARY KEY,
    "CountryName" VARCHAR(100) NOT NULL
);

-- جدول DocumentTypes
CREATE TABLE IF NOT EXISTS "DocumentTypes" (
    "TypeId" SERIAL PRIMARY KEY,
    "TypeName" VARCHAR(50) NOT NULL
);

-- جدول Controllers
CREATE TABLE IF NOT EXISTS "Controllers" (
    "ControllerId" SERIAL PRIMARY KEY,
    "FullName" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "Password" VARCHAR(500) NOT NULL,
    "AirportId" INTEGER REFERENCES "Airports"("AirportId"),
    "PhotoPath" VARCHAR(200),
    "LicensePath" VARCHAR(200),
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "LicenseNumber" VARCHAR(100),
    "Job_Title" VARCHAR(100),
    "Education_Level" VARCHAR(100),
    "Date_Of_Birth" DATE,
    "Marital_Status" VARCHAR(20),
    "Phone_Number" VARCHAR(20),
    "Email" VARCHAR(200),
    "Address" VARCHAR(500),
    "Hire_Date" DATE,
    "Employment_Status" VARCHAR(20),
    "Current_Department" VARCHAR(100),
    "Transfer_Date" DATE,
    "Emergency_Contact" VARCHAR(200),
    "NeedLicense" BOOLEAN NOT NULL DEFAULT true,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CurrentSalary" DECIMAL(18, 2),
    "AnnualIncreasePercentage" DECIMAL(5, 2),
    "SalaryAfterAnnualIncrease" DECIMAL(18, 2),
    "BankAccountNumber" VARCHAR(50),
    "BankName" VARCHAR(100),
    "TaxId" VARCHAR(50),
    "InsuranceNumber" VARCHAR(50),
    "current_salary" DECIMAL(18, 2),
    "annual_increase_percentage" DECIMAL(5, 2),
    "salary_after_annual_increase" DECIMAL(18, 2),
    "bank_account_number" VARCHAR(50),
    "bank_name" VARCHAR(100),
    "tax_id" VARCHAR(50),
    "insurance_number" VARCHAR(50)
);

-- جدول Employees
CREATE TABLE IF NOT EXISTS "Employees" (
    "EmployeeID" SERIAL PRIMARY KEY,
    "EmployeeOfficialID" VARCHAR(50) NOT NULL,
    "UserID" INTEGER REFERENCES "Users"("UserId"),
    "FullName" VARCHAR(150) NOT NULL,
    "JobTitle" VARCHAR(100),
    "Department" VARCHAR(100),
    "PhoneNumber" VARCHAR(20),
    "Email" VARCHAR(100),
    "HireDate" DATE,
    "TerminationDate" DATE,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "Address" VARCHAR(255),
    "Location" VARCHAR(150),
    "EmergencyContactPhone" VARCHAR(20),
    "Gender" VARCHAR(20),
    "DateOfBirth" DATE,
    "MaritalStatus" VARCHAR(50),
    "EducationLevel" VARCHAR(100),
    "CurrentSalary" DECIMAL(18, 2),
    "AnnualIncreasePercentage" DECIMAL(5, 2),
    "SalaryAfterAnnualIncrease" DECIMAL(18, 2),
    "BankAccountNumber" VARCHAR(100),
    "BankName" VARCHAR(100),
    "TaxId" VARCHAR(50),
    "InsuranceNumber" VARCHAR(50),
    "PhotoPath" VARCHAR(500),
    "NeedLicense" BOOLEAN NOT NULL DEFAULT true,
    "OrganizationalStructure" VARCHAR(100),
    "Division" VARCHAR(100),
    "Role" VARCHAR(50),
    "Username" VARCHAR(100)
);

-- جدول Certificates
CREATE TABLE IF NOT EXISTS "Certificates" (
    "CertificateId" SERIAL PRIMARY KEY,
    "ControllerId" INTEGER REFERENCES "Controllers"("ControllerId"),
    "TypeId" INTEGER REFERENCES "DocumentTypes"("TypeId"),
    "CertificateTitle" VARCHAR(200) NOT NULL,
    "IssuingAuthority" VARCHAR(200),
    "IssuingCountry" VARCHAR(100),
    "IssueDate" DATE,
    "ExpiryDate" DATE,
    "Status" VARCHAR(20) DEFAULT 'Pending',
    "StatusReason" VARCHAR(500),
    "FilePath" VARCHAR(500),
    "Notes" VARCHAR(1000),
    "EmployeeId" INTEGER REFERENCES "Employees"("EmployeeID")
);

-- جدول Licenses
CREATE TABLE IF NOT EXISTS "Licenses" (
    "LicenseId" SERIAL PRIMARY KEY,
    "ControllerId" INTEGER REFERENCES "Controllers"("ControllerId"),
    "LicenseType" VARCHAR(255) NOT NULL,
    "ExpiryDate" DATE,
    "PdfPath" VARCHAR(255),
    "PhotoPath" VARCHAR(255),
    "Range" VARCHAR(255),
    "Note" VARCHAR(255),
    "licensenumber" VARCHAR(255),
    "F2" VARCHAR(255),
    "IssueDate" DATE,
    "EmployeeID" INTEGER REFERENCES "Employees"("EmployeeID")
);

-- جدول Notifications
CREATE TABLE IF NOT EXISTS "Notifications" (
    "NotificationId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "ControllerId" INTEGER,
    "Message" VARCHAR(500),
    "Link" VARCHAR(300),
    "Created_At" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "Is_Read" BOOLEAN DEFAULT false,
    "Note" VARCHAR(500),
    "LicenseType" VARCHAR(500),
    "LicenseExpiryDate" DATE
);

-- جدول Observations
CREATE TABLE IF NOT EXISTS "Observations" (
    "ObservationId" SERIAL PRIMARY KEY,
    "ControllerId" INTEGER REFERENCES "Controllers"("ControllerId"),
    "TravelCount" INTEGER DEFAULT 1,
    "Duration_Days" INTEGER,
    "TravelCountry" VARCHAR(100),
    "DepartDate" DATE,
    "ReturnDate" DATE,
    "LicenseNumber" VARCHAR(100) NOT NULL,
    "FilePath" VARCHAR(500),
    "ObservationNo" INTEGER,
    "Notes" VARCHAR(1000),
    "EmployeeId" INTEGER REFERENCES "Employees"("EmployeeID"),
    "FlightNo" VARCHAR(100)
);

-- جدول Projects
CREATE TABLE IF NOT EXISTS "Projects" (
    "ProjectId" SERIAL PRIMARY KEY,
    "ProjectName" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Location" VARCHAR(255),
    "AssociatedEntity" VARCHAR(255),
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "Duration" VARCHAR(100),
    "Status" VARCHAR(50) NOT NULL,
    "FolderPath" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- جدول Roles
CREATE TABLE IF NOT EXISTS "Roles" (
    "RoleName" VARCHAR(50) PRIMARY KEY,
    "Description" VARCHAR(100)
);

-- جدول UserMenuPermissions
CREATE TABLE IF NOT EXISTS "UserMenuPermissions" (
    "UserMenuPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "MenuKey" VARCHAR(50) NOT NULL,
    "IsVisible" BOOLEAN NOT NULL DEFAULT true,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- جدول UserOperationPermissions
CREATE TABLE IF NOT EXISTS "UserOperationPermissions" (
    "UserOperationPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "PermissionId" INTEGER REFERENCES "Permissions"("PermissionId"),
    "EntityType" VARCHAR(50) NOT NULL,
    "OperationType" VARCHAR(50) NOT NULL,
    "IsAllowed" BOOLEAN NOT NULL DEFAULT true,
    "Scope" VARCHAR(50) NOT NULL DEFAULT 'All',
    "ScopeId" INTEGER,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- جدول UserOrganizationalPermissions
CREATE TABLE IF NOT EXISTS "UserOrganizationalPermissions" (
    "UserOrganizationalPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("UserId"),
    "PermissionType" VARCHAR(50) NOT NULL,
    "EntityId" INTEGER NOT NULL,
    "EntityName" VARCHAR(100) NOT NULL,
    "CanView" BOOLEAN NOT NULL DEFAULT true,
    "CanEdit" BOOLEAN NOT NULL DEFAULT false,
    "CanDelete" BOOLEAN NOT NULL DEFAULT false,
    "CanCreate" BOOLEAN NOT NULL DEFAULT false,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- جدول ConfigurationLog
CREATE TABLE IF NOT EXISTS "ConfigurationLog" (
    "LogId" SERIAL PRIMARY KEY,
    "ValueId" INTEGER,
    "Action" VARCHAR(50),
    "OldValue" VARCHAR(500),
    "NewValue" VARCHAR(500),
    "ChangedBy" VARCHAR(100),
    "ChangedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- جدول PermissionLogs
CREATE TABLE IF NOT EXISTS "PermissionLogs" (
    "LogId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "PermissionKey" VARCHAR(50) NOT NULL,
    "DepartmentId" INTEGER,
    "Action" VARCHAR(50) NOT NULL,
    "Result" BOOLEAN NOT NULL,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IPAddress" VARCHAR(45),
    "UserAgent" VARCHAR(500),
    "Status" VARCHAR(20) NOT NULL DEFAULT 'UNKNOWN',
    "Details" VARCHAR(500)
);

-- 2. إنشاء الفهارس
-- =====================================================

-- فهارس ConfigurationValues
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_CategoryId" ON "ConfigurationValues"("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_DisplayOrder" ON "ConfigurationValues"("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_IsActive" ON "ConfigurationValues"("IsActive");

-- فهارس UserActivityLogs
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_UserId" ON "UserActivityLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_Timestamp" ON "UserActivityLogs"("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_Action" ON "UserActivityLogs"("Action");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_EntityType" ON "UserActivityLogs"("EntityType");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_IsSuccessful" ON "UserActivityLogs"("IsSuccessful");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_UserName" ON "UserActivityLogs"("UserName");

-- فهارس UserMenuPermissions
CREATE INDEX IF NOT EXISTS "IX_UserMenuPermissions_UserId" ON "UserMenuPermissions"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserMenuPermissions_MenuKey" ON "UserMenuPermissions"("MenuKey");

-- فهارس UserOperationPermissions
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_UserId" ON "UserOperationPermissions"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_PermissionId" ON "UserOperationPermissions"("PermissionId");
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_EntityType" ON "UserOperationPermissions"("EntityType");

-- فهارس UserOrganizationalPermissions
CREATE INDEX IF NOT EXISTS "IX_UserOrganizationalPermissions_UserId" ON "UserOrganizationalPermissions"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserOrganizationalPermissions_EntityType" ON "UserOrganizationalPermissions"("PermissionType");

-- فهارس ConfigurationLog
CREATE INDEX IF NOT EXISTS "IX_ConfigurationLog_ValueId" ON "ConfigurationLog"("ValueId");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationLog_ChangedDate" ON "ConfigurationLog"("ChangedDate");

-- 3. إنشاء Views
-- =====================================================

-- View لملخص صلاحيات المستخدمين
CREATE OR REPLACE VIEW "vw_UserPermissionsSummary" AS
SELECT 
    u."UserId",
    u."Username",
    u."RoleName" as "UserFullName",
    u."RoleName" as "UserRole",
    (SELECT COUNT(DISTINCT p."PermissionId") 
     FROM "RolePermissions" rp
     JOIN "Permissions" p ON rp."PermissionId" = p."PermissionId"
     JOIN "Users" u2 ON u2."RoleName" = (
         SELECT cv."ValueText" 
         FROM "ConfigurationValues" cv 
         JOIN "ConfigurationCategories" cc ON cv."CategoryId" = cc."CategoryId" 
         WHERE cc."CategoryName" = 'Roles' AND cv."ValueId" = rp."RoleId"
     )
     WHERE u2."UserId" = u."UserId" 
       AND rp."IsActive" = true
       AND p."IsActive" = true) +
    (SELECT COUNT(DISTINCT p."PermissionId") 
     FROM "UserDepartmentPermissions" udp
     JOIN "Permissions" p ON udp."PermissionId" = p."PermissionId"
     WHERE udp."UserId" = u."UserId" 
       AND udp."IsActive" = true
       AND p."IsActive" = true
       AND udp."CanView" = true) AS "TotalPermissions",
    (SELECT COUNT(DISTINCT udp."DepartmentId") 
     FROM "UserDepartmentPermissions" udp
     WHERE udp."UserId" = u."UserId" 
       AND udp."IsActive" = true
       AND udp."CanView" = true) AS "AccessibleDepartments",
    (SELECT COUNT(DISTINCT p."PermissionId") 
     FROM "RolePermissions" rp
     JOIN "Permissions" p ON rp."PermissionId" = p."PermissionId"
     JOIN "Users" u2 ON u2."RoleName" = (
         SELECT cv."ValueText" 
         FROM "ConfigurationValues" cv 
         JOIN "ConfigurationCategories" cc ON cv."CategoryId" = cc."CategoryId" 
         WHERE cc."CategoryName" = 'Roles' AND cv."ValueId" = rp."RoleId"
     )
     WHERE u2."UserId" = u."UserId" 
       AND rp."IsActive" = true
       AND p."IsActive" = true) +
    (SELECT COUNT(DISTINCT p."PermissionId") 
     FROM "UserDepartmentPermissions" udp
     JOIN "Permissions" p ON udp."PermissionId" = p."PermissionId"
     WHERE udp."UserId" = u."UserId" 
       AND udp."IsActive" = true
       AND p."IsActive" = true
       AND udp."CanView" = true) AS "ActivePermissions"
FROM "Users" u;

-- View لسجلات النشاط
CREATE OR REPLACE VIEW "vw_UserActivityLogs" AS
SELECT 
    ual."Id",
    ual."UserId",
    ual."UserName",
    ual."Action",
    ual."EntityType",
    ual."EntityId",
    ual."Details",
    ual."IpAddress",
    ual."UserAgent",
    ual."Timestamp",
    ual."IsSuccessful",
    ual."ErrorMessage",
    CASE 
        WHEN ual."Action" = 'Create' THEN 'إنشاء'
        WHEN ual."Action" = 'Update' THEN 'تحديث'
        WHEN ual."Action" = 'Delete' THEN 'حذف'
        WHEN ual."Action" = 'Login' THEN 'تسجيل دخول'
        WHEN ual."Action" = 'Logout' THEN 'تسجيل خروج'
        WHEN ual."Action" = 'View' THEN 'عرض'
        WHEN ual."Action" = 'Export' THEN 'تصدير'
        WHEN ual."Action" = 'Import' THEN 'استيراد'
        ELSE ual."Action"
    END AS "ActionArabic",
    CASE 
        WHEN ual."EntityType" = 'Employee' THEN 'موظف'
        WHEN ual."EntityType" = 'Controller' THEN 'مراقب'
        WHEN ual."EntityType" = 'License' THEN 'رخصة'
        WHEN ual."EntityType" = 'Certificate' THEN 'شهادة'
        WHEN ual."EntityType" = 'Observation' THEN 'ملاحظة'
        WHEN ual."EntityType" = 'Project' THEN 'مشروع'
        WHEN ual."EntityType" = 'Airport' THEN 'مطار'
        WHEN ual."EntityType" = 'Country' THEN 'دولة'
        WHEN ual."EntityType" = 'System' THEN 'النظام'
        WHEN ual."EntityType" = 'Profile' THEN 'الملف الشخصي'
        WHEN ual."EntityType" = 'SystemLog' THEN 'سجل النظام'
        ELSE ual."EntityType"
    END AS "EntityTypeArabic"
FROM "UserActivityLogs" ual;

-- 4. إنشاء Functions
-- =====================================================

-- Function لجلب ملخص سجلات النشاط
CREATE OR REPLACE FUNCTION "fn_GetUserActivitySummary"(
    p_user_id INTEGER DEFAULT NULL,
    p_start_date TIMESTAMP DEFAULT NULL,
    p_end_date TIMESTAMP DEFAULT NULL
)
RETURNS TABLE (
    user_id INTEGER,
    user_name VARCHAR(100),
    action VARCHAR(50),
    entity_type VARCHAR(50),
    action_count BIGINT,
    success_count BIGINT,
    failure_count BIGINT,
    first_action TIMESTAMP,
    last_action TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COALESCE(p_user_id, ual."UserId")::INTEGER as user_id,
        ual."UserName",
        ual."Action",
        ual."EntityType",
        COUNT(*)::BIGINT as action_count,
        SUM(CASE WHEN ual."IsSuccessful" = true THEN 1 ELSE 0 END)::BIGINT as success_count,
        SUM(CASE WHEN ual."IsSuccessful" = false THEN 1 ELSE 0 END)::BIGINT as failure_count,
        MIN(ual."Timestamp") as first_action,
        MAX(ual."Timestamp") as last_action
    FROM "UserActivityLogs" ual
    WHERE (p_user_id IS NULL OR ual."UserId" = p_user_id)
        AND (p_start_date IS NULL OR ual."Timestamp" >= p_start_date)
        AND (p_end_date IS NULL OR ual."Timestamp" <= p_end_date)
    GROUP BY ual."UserId", ual."UserName", ual."Action", ual."EntityType";
END;
$$ LANGUAGE plpgsql;

-- 5. إنشاء Stored Procedures (Functions في PostgreSQL)
-- =====================================================

-- Function للتحقق من صلاحيات المستخدم
CREATE OR REPLACE FUNCTION "CanUserPerformOperation"(
    p_user_id INTEGER,
    p_entity_type VARCHAR(50),
    p_operation_type VARCHAR(50),
    p_scope VARCHAR(50) DEFAULT 'All',
    p_scope_id INTEGER DEFAULT NULL
)
RETURNS BOOLEAN AS $$
DECLARE
    can_perform BOOLEAN := false;
BEGIN
    -- التحقق من كون المستخدم Admin
    IF EXISTS (SELECT 1 FROM "Users" WHERE "UserId" = p_user_id AND "RoleName" = 'Admin') THEN
        can_perform := true;
    ELSE
        -- التحقق من الصلاحيات المحددة
        IF EXISTS (
            SELECT 1 FROM "UserOperationPermissions" uop
            WHERE uop."UserId" = p_user_id 
            AND uop."EntityType" = p_entity_type
            AND uop."OperationType" = p_operation_type
            AND uop."IsActive" = true 
            AND uop."IsAllowed" = true
            AND (uop."Scope" = 'All' OR (uop."Scope" = p_scope AND uop."ScopeId" = p_scope_id))
        ) THEN
            can_perform := true;
        END IF;
    END IF;
    
    RETURN can_perform;
END;
$$ LANGUAGE plpgsql;

-- Function للتحقق من إمكانية عرض القائمة
CREATE OR REPLACE FUNCTION "CanUserViewMenu"(
    p_user_id INTEGER,
    p_menu_key VARCHAR(50)
)
RETURNS BOOLEAN AS $$
DECLARE
    can_view BOOLEAN := false;
BEGIN
    -- التحقق من كون المستخدم Admin
    IF EXISTS (SELECT 1 FROM "Users" WHERE "UserId" = p_user_id AND "RoleName" = 'Admin') THEN
        can_view := true;
    ELSE
        -- التحقق من صلاحيات القائمة
        IF EXISTS (
            SELECT 1 FROM "UserMenuPermissions" ump
            WHERE ump."UserId" = p_user_id 
            AND ump."MenuKey" = p_menu_key 
            AND ump."IsActive" = true 
            AND ump."IsVisible" = true
        ) THEN
            can_view := true;
        END IF;
    END IF;
    
    RETURN can_view;
END;
$$ LANGUAGE plpgsql;

-- Function لجلب جميع المستخدمين مع الصلاحيات
CREATE OR REPLACE FUNCTION "GetAllUsersWithPermissions"()
RETURNS TABLE (
    userid INTEGER,
    username VARCHAR(50),
    fullname VARCHAR(100),
    rolename VARCHAR(50),
    menu_permissions_count BIGINT,
    operation_permissions_count BIGINT,
    organizational_permissions_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u."UserId"::INTEGER,
        u."Username",
        COALESCE(c."FullName", e."FullName", u."Username")::VARCHAR(100) as fullname,
        u."RoleName",
        COALESCE(menu_count."MenuPermissionsCount", 0)::BIGINT,
        COALESCE(op_count."OperationPermissionsCount", 0)::BIGINT,
        COALESCE(org_count."OrganizationalPermissionsCount", 0)::BIGINT
    FROM "Users" u
    LEFT JOIN "Controllers" c ON u."UserId" = c."UserId"
    LEFT JOIN "Employees" e ON u."UserId" = e."UserID"
    LEFT JOIN (
        SELECT "UserId", COUNT(*) as "MenuPermissionsCount"
        FROM "UserMenuPermissions"
        WHERE "IsActive" = true AND "IsVisible" = true
        GROUP BY "UserId"
    ) menu_count ON u."UserId" = menu_count."UserId"
    LEFT JOIN (
        SELECT "UserId", COUNT(*) as "OperationPermissionsCount"
        FROM "UserOperationPermissions"
        WHERE "IsActive" = true AND "IsAllowed" = true
        GROUP BY "UserId"
    ) op_count ON u."UserId" = op_count."UserId"
    LEFT JOIN (
        SELECT "UserId", COUNT(*) as "OrganizationalPermissionsCount"
        FROM "UserOrganizationalPermissions"
        WHERE "IsActive" = true AND ("CanView" = true OR "CanEdit" = true OR "CanDelete" = true OR "CanCreate" = true)
        GROUP BY "UserId"
    ) org_count ON u."UserId" = org_count."UserId"
    ORDER BY u."Username";
END;
$$ LANGUAGE plpgsql;

-- Function لإدراج سجل نشاط
CREATE OR REPLACE FUNCTION "sp_InsertUserActivityLog"(
    p_user_id INTEGER,
    p_user_name VARCHAR(100),
    p_action VARCHAR(50),
    p_entity_type VARCHAR(50),
    p_entity_id VARCHAR(50) DEFAULT NULL,
    p_details TEXT DEFAULT NULL,
    p_ip_address VARCHAR(45) DEFAULT NULL,
    p_user_agent VARCHAR(500) DEFAULT NULL,
    p_is_successful BOOLEAN DEFAULT true,
    p_error_message TEXT DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    log_id INTEGER;
BEGIN
    INSERT INTO "UserActivityLogs" 
    ("UserId", "UserName", "Action", "EntityType", "EntityId", "Details", "IpAddress", "UserAgent", "Timestamp", "IsSuccessful", "ErrorMessage")
    VALUES 
    (p_user_id, p_user_name, p_action, p_entity_type, p_entity_id, p_details, p_ip_address, p_user_agent, CURRENT_TIMESTAMP, p_is_successful, p_error_message)
    RETURNING "Id" INTO log_id;
    
    RETURN log_id;
END;
$$ LANGUAGE plpgsql;

-- 6. إدراج البيانات الأساسية
-- =====================================================

-- إدراج الأدوار
INSERT INTO "Roles" ("RoleName", "Description") VALUES 
('Admin', 'System Administrator'),
('Controller', 'Air Traffic Controller'),
('Manager', 'Department Manager'),
('HR', 'Human Resources'),
('Viewer', 'Read Only Access')
ON CONFLICT ("RoleName") DO NOTHING;

-- إدراج فئات التكوين
INSERT INTO "ConfigurationCategories" ("CategoryName", "DisplayName", "Description", "IsActive", "DisplayOrder") VALUES 
('Roles', 'User Roles', 'Available user roles in the system', true, 1),
('Departments', 'Departments', 'Organizational departments', true, 2),
('Divisions', 'Divisions', 'Organizational divisions', true, 3),
('LicenseTypes', 'License Types', 'Types of aviation licenses', true, 4),
('CertificateTypes', 'Certificate Types', 'Types of certificates', true, 5)
ON CONFLICT ("CategoryName") DO NOTHING;

-- إدراج قيم التكوين
INSERT INTO "ConfigurationValues" ("CategoryId", "ValueKey", "ValueText", "DisplayOrder", "IsActive") VALUES 
(1, 'Admin', 'Admin', 1, true),
(1, 'Controller', 'Controller', 2, true),
(1, 'Manager', 'Manager', 3, true),
(1, 'HR', 'HR', 4, true),
(1, 'Viewer', 'Viewer', 5, true),
(2, 'ATC', 'Air Traffic Control', 1, true),
(2, 'Maintenance', 'Maintenance', 2, true),
(2, 'Operations', 'Operations', 3, true),
(2, 'HR', 'Human Resources', 4, true),
(3, 'Technical', 'Technical Division', 1, true),
(3, 'Administrative', 'Administrative Division', 2, true),
(3, 'Support', 'Support Division', 3, true)
ON CONFLICT DO NOTHING;

-- إدراج أنواع المستندات
INSERT INTO "DocumentTypes" ("TypeName") VALUES 
('ATC License'),
('Pilot License'),
('Maintenance Certificate'),
('Training Certificate'),
('Medical Certificate'),
('Security Clearance')
ON CONFLICT DO NOTHING;

-- إدراج الدول
INSERT INTO "Countries" ("CountryName") VALUES 
('Jordan'),
('Saudi Arabia'),
('UAE'),
('Kuwait'),
('Qatar'),
('Bahrain'),
('Oman'),
('Egypt'),
('Lebanon'),
('Syria')
ON CONFLICT DO NOTHING;

-- إدراج المطارات
INSERT INTO "Airports" ("AirportName", "CountryId", "Icao_Code") VALUES 
('Queen Alia International Airport', 1, 'OJAI'),
('King Abdulaziz International Airport', 2, 'OEJN'),
('Dubai International Airport', 3, 'OMDB'),
('Kuwait International Airport', 4, 'OKBK'),
('Hamad International Airport', 5, 'OTHH')
ON CONFLICT DO NOTHING;

-- 7. إنشاء مستخدم admin
-- =====================================================

-- حذف مستخدم admin إذا كان موجوداً
DELETE FROM "Users" WHERE "Username" = 'admin';

-- إنشاء مستخدم admin جديد
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") VALUES 
('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin');

-- إدراج سجل نشاط للاختبار
INSERT INTO "UserActivityLogs" (
    "UserId", "UserName", "Action", "EntityType", "Details", 
    "IpAddress", "UserAgent", "Timestamp", "IsSuccessful"
) VALUES (
    1, 'admin', 'System Setup', 'System', 'Supabase complete migration applied', 
    '127.0.0.1', 'Supabase Migration Script', CURRENT_TIMESTAMP, true
);

-- 8. منح الصلاحيات
-- =====================================================

-- منح جميع الصلاحيات للمستخدم الحالي
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres;

-- 9. رسالة النجاح
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Supabase Complete Migration Successful!';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Database: %', current_database();
    RAISE NOTICE 'User: %', current_user;
    RAISE NOTICE 'Admin User: admin';
    RAISE NOTICE 'Password: admin123';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'All tables, views, functions, and data migrated!';
    RAISE NOTICE 'Ready for HR Aviation Pro!';
    RAISE NOTICE '=====================================================';
END $$;

-- 10. التحقق النهائي
-- =====================================================

-- عرض إحصائيات الجداول
SELECT 
    'Table Statistics' as InfoType,
    schemaname,
    tablename,
    n_tup_ins as "Total Inserts",
    n_tup_upd as "Total Updates",
    n_tup_del as "Total Deletes"
FROM pg_stat_user_tables 
WHERE schemaname = 'public'
ORDER BY tablename;

-- عرض معلومات المستخدم
SELECT 
    'User Information' as InfoType,
    "UserId",
    "Username",
    "RoleName"
FROM "Users" 
WHERE "Username" = 'admin';