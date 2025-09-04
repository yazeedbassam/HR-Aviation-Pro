-- =============================================
-- HR Aviation Database Migration Script
-- From SQL Server to PostgreSQL (Railway)
-- =============================================

-- Create database (this will be handled by Railway)
-- CREATE DATABASE "HR-Aviation";

-- Set search path
SET search_path TO public;

-- =============================================
-- CREATE TABLES
-- =============================================

-- Configuration Categories
CREATE TABLE IF NOT EXISTS "ConfigurationCategories" (
    "CategoryId" SERIAL PRIMARY KEY,
    "CategoryName" VARCHAR(100) NOT NULL UNIQUE,
    "DisplayName" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "IsActive" BOOLEAN DEFAULT true,
    "DisplayOrder" INTEGER DEFAULT 0,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" TIMESTAMP
);

-- Configuration Values
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

-- Permissions
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

-- Users
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(200) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL,
    "LastPermissionUpdate" TIMESTAMP
);

-- Role Permissions
CREATE TABLE IF NOT EXISTS "RolePermissions" (
    "RolePermissionId" SERIAL PRIMARY KEY,
    "RoleId" INTEGER NOT NULL,
    "PermissionId" INTEGER NOT NULL REFERENCES "Permissions"("PermissionId"),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- User Department Permissions
CREATE TABLE IF NOT EXISTS "UserDepartmentPermissions" (
    "UserDepartmentPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
    "DepartmentId" INTEGER NOT NULL,
    "PermissionId" INTEGER NOT NULL REFERENCES "Permissions"("PermissionId"),
    "CanView" BOOLEAN NOT NULL DEFAULT false,
    "CanEdit" BOOLEAN NOT NULL DEFAULT false,
    "CanDelete" BOOLEAN NOT NULL DEFAULT false,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- User Activity Logs
CREATE TABLE IF NOT EXISTS "UserActivityLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
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

-- Airports
CREATE TABLE IF NOT EXISTS "Airports" (
    "AirportId" SERIAL PRIMARY KEY,
    "AirportName" VARCHAR(100) NOT NULL,
    "CountryId" INTEGER NOT NULL,
    "Icao_Code" VARCHAR(10)
);

-- Countries
CREATE TABLE IF NOT EXISTS "Countries" (
    "CountryId" SERIAL PRIMARY KEY,
    "CountryName" VARCHAR(100) NOT NULL
);

-- Document Types
CREATE TABLE IF NOT EXISTS "DocumentTypes" (
    "TypeId" SERIAL PRIMARY KEY,
    "TypeName" VARCHAR(50) NOT NULL
);

-- Controllers
CREATE TABLE IF NOT EXISTS "Controllers" (
    "ControllerId" SERIAL PRIMARY KEY,
    "FullName" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "Password" VARCHAR(500) NOT NULL,
    "AirportId" INTEGER NOT NULL REFERENCES "Airports"("AirportId"),
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

-- Certificates
CREATE TABLE IF NOT EXISTS "Certificates" (
    "CertificateId" SERIAL PRIMARY KEY,
    "ControllerId" INTEGER REFERENCES "Controllers"("ControllerId"),
    "TypeId" INTEGER NOT NULL REFERENCES "DocumentTypes"("TypeId"),
    "CertificateTitle" VARCHAR(200) NOT NULL,
    "IssuingAuthority" VARCHAR(200),
    "IssuingCountry" VARCHAR(100),
    "IssueDate" DATE,
    "ExpiryDate" DATE,
    "Status" VARCHAR(20) DEFAULT 'Pending',
    "StatusReason" VARCHAR(500),
    "FilePath" VARCHAR(500),
    "Notes" VARCHAR(1000),
    "EmployeeId" INTEGER
);

-- Configuration Log
CREATE TABLE IF NOT EXISTS "ConfigurationLog" (
    "LogId" SERIAL PRIMARY KEY,
    "ValueId" INTEGER,
    "Action" VARCHAR(50),
    "OldValue" VARCHAR(500),
    "NewValue" VARCHAR(500),
    "ChangedBy" VARCHAR(100),
    "ChangedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Employees
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

-- Licenses
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

-- Notifications
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

-- Observations
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
    "EmployeeId" INTEGER,
    "FlightNo" VARCHAR(100)
);

-- Permission Logs
CREATE TABLE IF NOT EXISTS "PermissionLogs" (
    "LogId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
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

-- Project Divisions
CREATE TABLE IF NOT EXISTS "ProjectDivisions" (
    "Id" SERIAL PRIMARY KEY,
    "ProjectId" INTEGER NOT NULL,
    "DivisionId" INTEGER NOT NULL
);

-- Project Participants
CREATE TABLE IF NOT EXISTS "ProjectParticipants" (
    "Id" SERIAL PRIMARY KEY,
    "ProjectId" INTEGER NOT NULL,
    "UserId" INTEGER,
    "UserType" INTEGER,
    "Role" VARCHAR(255),
    "ControllerId" INTEGER,
    "EmployeeId" INTEGER
);

-- Project Phases
CREATE TABLE IF NOT EXISTS "ProjectPhases" (
    "Id" SERIAL PRIMARY KEY,
    "ProjectId" INTEGER NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "Status" INTEGER NOT NULL,
    "Notes" TEXT
);

-- Projects
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

-- Roles
CREATE TABLE IF NOT EXISTS "Roles" (
    "RoleName" VARCHAR(50) PRIMARY KEY,
    "Description" VARCHAR(100)
);

-- User Menu Permissions
CREATE TABLE IF NOT EXISTS "UserMenuPermissions" (
    "UserMenuPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
    "MenuKey" VARCHAR(50) NOT NULL,
    "IsVisible" BOOLEAN NOT NULL DEFAULT true,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- User Operation Permissions
CREATE TABLE IF NOT EXISTS "UserOperationPermissions" (
    "UserOperationPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
    "PermissionId" INTEGER NOT NULL REFERENCES "Permissions"("PermissionId"),
    "EntityType" VARCHAR(50) NOT NULL,
    "OperationType" VARCHAR(50) NOT NULL,
    "IsAllowed" BOOLEAN NOT NULL DEFAULT true,
    "Scope" VARCHAR(50) NOT NULL DEFAULT 'All',
    "ScopeId" INTEGER,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- User Organizational Permissions
CREATE TABLE IF NOT EXISTS "UserOrganizationalPermissions" (
    "UserOrganizationalPermissionId" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("UserId"),
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

-- =============================================
-- CREATE INDEXES
-- =============================================

-- Configuration Log indexes
CREATE INDEX IF NOT EXISTS "IX_ConfigurationLog_ChangedDate" ON "ConfigurationLog"("ChangedDate");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationLog_ValueId" ON "ConfigurationLog"("ValueId");

-- Configuration Values indexes
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_CategoryId" ON "ConfigurationValues"("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_DisplayOrder" ON "ConfigurationValues"("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_ConfigurationValues_IsActive" ON "ConfigurationValues"("IsActive");

-- User Activity Logs indexes
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_Action" ON "UserActivityLogs"("Action");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_EntityType" ON "UserActivityLogs"("EntityType");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_IsSuccessful" ON "UserActivityLogs"("IsSuccessful");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_Timestamp" ON "UserActivityLogs"("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_UserId" ON "UserActivityLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserActivityLogs_UserName" ON "UserActivityLogs"("UserName");

-- User Menu Permissions indexes
CREATE INDEX IF NOT EXISTS "IX_UserMenuPermissions_MenuKey" ON "UserMenuPermissions"("MenuKey");
CREATE INDEX IF NOT EXISTS "IX_UserMenuPermissions_UserId" ON "UserMenuPermissions"("UserId");

-- User Operation Permissions indexes
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_EntityType" ON "UserOperationPermissions"("EntityType");
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_PermissionId" ON "UserOperationPermissions"("PermissionId");
CREATE INDEX IF NOT EXISTS "IX_UserOperationPermissions_UserId" ON "UserOperationPermissions"("UserId");

-- User Organizational Permissions indexes
CREATE INDEX IF NOT EXISTS "IX_UserOrganizationalPermissions_EntityType" ON "UserOrganizationalPermissions"("PermissionType");
CREATE INDEX IF NOT EXISTS "IX_UserOrganizationalPermissions_UserId" ON "UserOrganizationalPermissions"("UserId");

-- =============================================
-- CREATE VIEWS
-- =============================================

-- User Permissions Summary View
CREATE OR REPLACE VIEW "vw_UserPermissionsSummary" AS
SELECT 
    u."UserId",
    u."Username" AS "UserName",
    u."RoleName" AS "UserFullName",
    u."RoleName" AS "UserRole",
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

-- User Activity Logs View
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

-- =============================================
-- CREATE FUNCTIONS
-- =============================================

-- Get User Activity Summary Function
CREATE OR REPLACE FUNCTION "fn_GetUserActivitySummary"(
    p_UserId INTEGER DEFAULT NULL,
    p_StartDate TIMESTAMP DEFAULT NULL,
    p_EndDate TIMESTAMP DEFAULT NULL
)
RETURNS TABLE(
    "UserId" INTEGER,
    "UserName" VARCHAR(100),
    "Action" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "ActionCount" BIGINT,
    "SuccessCount" BIGINT,
    "FailureCount" BIGINT,
    "FirstAction" TIMESTAMP,
    "LastAction" TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COALESCE(p_UserId, ual."UserId") AS "UserId",
        ual."UserName",
        ual."Action",
        ual."EntityType",
        COUNT(*) AS "ActionCount",
        SUM(CASE WHEN ual."IsSuccessful" = true THEN 1 ELSE 0 END) AS "SuccessCount",
        SUM(CASE WHEN ual."IsSuccessful" = false THEN 1 ELSE 0 END) AS "FailureCount",
        MIN(ual."Timestamp") AS "FirstAction",
        MAX(ual."Timestamp") AS "LastAction"
    FROM "UserActivityLogs" ual
    WHERE (p_UserId IS NULL OR ual."UserId" = p_UserId)
        AND (p_StartDate IS NULL OR ual."Timestamp" >= p_StartDate)
        AND (p_EndDate IS NULL OR ual."Timestamp" <= p_EndDate)
    GROUP BY ual."UserId", ual."UserName", ual."Action", ual."EntityType";
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CREATE STORED PROCEDURES (Functions in PostgreSQL)
-- =============================================

-- Can User Perform Operation
CREATE OR REPLACE FUNCTION "CanUserPerformOperation"(
    p_UserId INTEGER,
    p_EntityType VARCHAR(50),
    p_OperationType VARCHAR(50),
    p_Scope VARCHAR(50) DEFAULT 'All',
    p_ScopeId INTEGER DEFAULT NULL
)
RETURNS BOOLEAN AS $$
DECLARE
    v_CanPerform BOOLEAN := false;
BEGIN
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM "Users" WHERE "UserId" = p_UserId AND "RoleName" = 'Admin') THEN
        v_CanPerform := true;
    ELSE
        -- Check if user has specific operation permission
        IF EXISTS (
            SELECT 1 FROM "UserOperationPermissions" uop
            WHERE uop."UserId" = p_UserId 
            AND uop."EntityType" = p_EntityType
            AND uop."OperationType" = p_OperationType
            AND uop."IsActive" = true 
            AND uop."IsAllowed" = true
            AND (uop."Scope" = 'All' OR (uop."Scope" = p_Scope AND uop."ScopeId" = p_ScopeId))
        ) THEN
            v_CanPerform := true;
        END IF;
    END IF;
    
    RETURN v_CanPerform;
END;
$$ LANGUAGE plpgsql;

-- Can User View Menu
CREATE OR REPLACE FUNCTION "CanUserViewMenu"(
    p_UserId INTEGER,
    p_MenuKey VARCHAR(50)
)
RETURNS BOOLEAN AS $$
DECLARE
    v_CanView BOOLEAN := false;
BEGIN
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM "Users" WHERE "UserId" = p_UserId AND "RoleName" = 'Admin') THEN
        v_CanView := true;
    ELSE
        -- Check if user has specific menu permission
        IF EXISTS (
            SELECT 1 FROM "UserMenuPermissions" ump
            WHERE ump."UserId" = p_UserId 
            AND ump."MenuKey" = p_MenuKey 
            AND ump."IsActive" = true 
            AND ump."IsVisible" = true
        ) THEN
            v_CanView := true;
        END IF;
    END IF;
    
    RETURN v_CanView;
END;
$$ LANGUAGE plpgsql;

-- Check User Operation Permission
CREATE OR REPLACE FUNCTION "CheckUserOperationPermission"(
    p_UserId INTEGER,
    p_EntityType VARCHAR(50),
    p_OperationType VARCHAR(50),
    p_ScopeId INTEGER DEFAULT NULL
)
RETURNS BOOLEAN AS $$
DECLARE
    v_IsAllowed BOOLEAN := false;
BEGIN
    -- Check if user has specific operation permission
    SELECT uop."IsAllowed" INTO v_IsAllowed
    FROM "UserOperationPermissions" uop
    INNER JOIN "Permissions" p ON uop."PermissionId" = p."PermissionId"
    WHERE uop."UserId" = p_UserId 
        AND uop."EntityType" = p_EntityType 
        AND uop."OperationType" = p_OperationType
        AND uop."IsActive" = true
        AND p."IsActive" = true
        AND (uop."Scope" = 'All' OR (uop."Scope" = 'Department' AND uop."ScopeId" = p_ScopeId));
    
    -- If no specific permission found, check if user has admin role
    IF v_IsAllowed IS NULL THEN
        SELECT CASE WHEN u."RoleName" = 'Admin' THEN true ELSE false END INTO v_IsAllowed
        FROM "Users" u
        WHERE u."UserId" = p_UserId;
    END IF;
    
    RETURN COALESCE(v_IsAllowed, false);
END;
$$ LANGUAGE plpgsql;

-- Check User Permission (legacy support)
CREATE OR REPLACE FUNCTION "CheckUserPermission"(
    p_UserId INTEGER,
    p_PermissionKey VARCHAR(50),
    p_DepartmentId INTEGER DEFAULT NULL
)
RETURNS BOOLEAN AS $$
DECLARE
    v_HasPermission BOOLEAN := false;
BEGIN
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM "Users" WHERE "UserId" = p_UserId AND "RoleName" = 'Admin') THEN
        v_HasPermission := true;
    ELSE
        -- Check if user has specific permission
        IF EXISTS (
            SELECT 1 FROM "UserOperationPermissions" uop
            INNER JOIN "Permissions" p ON uop."PermissionId" = p."PermissionId"
            WHERE uop."UserId" = p_UserId 
            AND p."PermissionKey" = p_PermissionKey
            AND uop."IsActive" = true 
            AND uop."IsAllowed" = true
        ) THEN
            v_HasPermission := true;
        END IF;
    END IF;
    
    RETURN v_HasPermission;
END;
$$ LANGUAGE plpgsql;

-- Get All Users With Permissions
CREATE OR REPLACE FUNCTION "GetAllUsersWithPermissions"()
RETURNS TABLE(
    "userid" INTEGER,
    "username" VARCHAR(50),
    "fullname" VARCHAR(150),
    "rolename" VARCHAR(50),
    "MenuPermissionsCount" BIGINT,
    "OperationPermissionsCount" BIGINT,
    "OrganizationalPermissionsCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u."UserId",
        u."Username",
        COALESCE(c."FullName", e."FullName", u."Username") as "fullname",
        u."RoleName",
        COALESCE(menu_count."MenuPermissionsCount", 0) as "MenuPermissionsCount",
        COALESCE(op_count."OperationPermissionsCount", 0) as "OperationPermissionsCount",
        COALESCE(org_count."OrganizationalPermissionsCount", 0) as "OrganizationalPermissionsCount"
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

-- Get User Department Permissions
CREATE OR REPLACE FUNCTION "GetUserDepartmentPermissions"(p_UserId INTEGER)
RETURNS TABLE("DepartmentId" INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT udp."DepartmentId"
    FROM "UserDepartmentPermissions" udp
    WHERE udp."UserId" = p_UserId 
      AND udp."IsActive" = true
      AND udp."CanView" = true;
END;
$$ LANGUAGE plpgsql;

-- Get User Menu Permissions
CREATE OR REPLACE FUNCTION "GetUserMenuPermissions"(p_UserId INTEGER)
RETURNS TABLE("MenuKey" VARCHAR(50), "IsVisible" BOOLEAN) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ump."MenuKey",
        ump."IsVisible"
    FROM "UserMenuPermissions" ump
    WHERE ump."UserId" = p_UserId AND ump."IsActive" = true;
END;
$$ LANGUAGE plpgsql;

-- Get User Permissions
CREATE OR REPLACE FUNCTION "GetUserPermissions"(p_UserId INTEGER)
RETURNS TABLE(
    "PermissionId" INTEGER,
    "PermissionName" VARCHAR(100),
    "PermissionKey" VARCHAR(50),
    "PermissionDescription" VARCHAR(500),
    "IsActive" BOOLEAN,
    "CreatedAt" TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "PermissionType" VARCHAR(20),
    "DepartmentId" INTEGER,
    "DepartmentName" VARCHAR(200),
    "CanView" BOOLEAN,
    "CanEdit" BOOLEAN,
    "CanDelete" BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    -- Get role-based permissions
    SELECT DISTINCT
        p."PermissionId",
        p."PermissionName",
        p."PermissionKey",
        p."PermissionDescription",
        p."IsActive",
        p."CreatedAt",
        p."UpdatedAt",
        'Role'::VARCHAR(20) AS "PermissionType",
        NULL::INTEGER AS "DepartmentId",
        NULL::VARCHAR(200) AS "DepartmentName",
        NULL::BOOLEAN AS "CanView",
        NULL::BOOLEAN AS "CanEdit",
        NULL::BOOLEAN AS "CanDelete"
    FROM "RolePermissions" rp
    JOIN "Permissions" p ON rp."PermissionId" = p."PermissionId"
    JOIN "Users" u ON u."RoleName" = (
        SELECT cv."ValueText" 
        FROM "ConfigurationValues" cv 
        JOIN "ConfigurationCategories" cc ON cv."CategoryId" = cc."CategoryId" 
        WHERE cc."CategoryName" = 'Roles' AND cv."ValueId" = rp."RoleId"
    )
    WHERE u."UserId" = p_UserId 
      AND rp."IsActive" = true
      AND p."IsActive" = true
    
    UNION ALL
    
    -- Get department-based permissions
    SELECT DISTINCT
        p."PermissionId",
        p."PermissionName",
        p."PermissionKey",
        p."PermissionDescription",
        p."IsActive",
        p."CreatedAt",
        p."UpdatedAt",
        'Department'::VARCHAR(20) AS "PermissionType",
        udp."DepartmentId",
        dept."ValueText"::VARCHAR(200) AS "DepartmentName",
        udp."CanView",
        udp."CanEdit",
        udp."CanDelete"
    FROM "UserDepartmentPermissions" udp
    JOIN "Permissions" p ON udp."PermissionId" = p."PermissionId"
    JOIN "ConfigurationValues" dept ON udp."DepartmentId" = dept."ValueId"
    JOIN "ConfigurationCategories" cc ON dept."CategoryId" = cc."CategoryId"
    WHERE udp."UserId" = p_UserId 
      AND udp."IsActive" = true
      AND p."IsActive" = true
      AND (cc."CategoryName" = 'Divisions' OR cc."CategoryName" = 'Departments')
    
    ORDER BY p."PermissionName";
END;
$$ LANGUAGE plpgsql;

-- Insert User Activity Log
CREATE OR REPLACE FUNCTION "sp_InsertUserActivityLog"(
    p_UserId INTEGER,
    p_UserName VARCHAR(100),
    p_Action VARCHAR(50),
    p_EntityType VARCHAR(50),
    p_EntityId VARCHAR(50) DEFAULT NULL,
    p_Details TEXT DEFAULT NULL,
    p_IpAddress VARCHAR(45) DEFAULT NULL,
    p_UserAgent VARCHAR(500) DEFAULT NULL,
    p_IsSuccessful BOOLEAN DEFAULT true,
    p_ErrorMessage TEXT DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    v_NewId INTEGER;
BEGIN
    INSERT INTO "UserActivityLogs" 
    ("UserId", "UserName", "Action", "EntityType", "EntityId", "Details", "IpAddress", "UserAgent", "Timestamp", "IsSuccessful", "ErrorMessage")
    VALUES 
    (p_UserId, p_UserName, p_Action, p_EntityType, p_EntityId, p_Details, p_IpAddress, p_UserAgent, CURRENT_TIMESTAMP, p_IsSuccessful, p_ErrorMessage)
    RETURNING "Id" INTO v_NewId;
    
    RETURN v_NewId;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- INSERT INITIAL DATA
-- =============================================

-- Insert basic roles
INSERT INTO "Roles" ("RoleName", "Description") VALUES 
('Admin', 'System Administrator'),
('User', 'Regular User'),
('Manager', 'Department Manager')
ON CONFLICT ("RoleName") DO NOTHING;

-- Insert basic configuration categories
INSERT INTO "ConfigurationCategories" ("CategoryName", "DisplayName", "Description", "IsActive", "DisplayOrder") VALUES 
('Roles', 'User Roles', 'System user roles', true, 1),
('Departments', 'Departments', 'Organizational departments', true, 2),
('Divisions', 'Divisions', 'Organizational divisions', true, 3),
('DocumentTypes', 'Document Types', 'Types of documents and certificates', true, 4)
ON CONFLICT ("CategoryName") DO NOTHING;

-- Insert basic document types
INSERT INTO "DocumentTypes" ("TypeName") VALUES 
('AFTN Certificate'),
('AIS Certificate'),
('CNS Certificate'),
('License'),
('Training Certificate'),
('Medical Certificate')
ON CONFLICT DO NOTHING;

-- Insert default admin user (password: admin123 - hashed with BCrypt)
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") VALUES 
('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Admin')
ON CONFLICT ("Username") DO NOTHING;

-- =============================================
-- COMMIT TRANSACTION
-- =============================================

COMMIT;

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Verify tables were created
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- Verify admin user exists
SELECT "UserId", "Username", "RoleName" 
FROM "Users" 
WHERE "Username" = 'admin';

-- Verify functions were created
SELECT routine_name 
FROM information_schema.routines 
WHERE routine_schema = 'public' 
AND routine_type = 'FUNCTION'
ORDER BY routine_name;

-- =============================================
-- MIGRATION COMPLETE
-- =============================================

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'HR Aviation Database Migration to PostgreSQL completed successfully!';
    RAISE NOTICE 'Admin user created: username=admin, password=admin123';
    RAISE NOTICE 'Database is ready for use with Railway PostgreSQL.';
END $$;