
-- Quick Setup for HR Aviation Database
-- This script creates the essential tables and admin user

-- Set search path
SET search_path TO public;

-- Create essential tables
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(200) NOT NULL,
    "RoleName" VARCHAR(50) NOT NULL,
    "LastPermissionUpdate" TIMESTAMP
);

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

CREATE TABLE IF NOT EXISTS "Countries" (
    "CountryId" SERIAL PRIMARY KEY,
    "CountryName" VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Airports" (
    "AirportId" SERIAL PRIMARY KEY,
    "AirportName" VARCHAR(100) NOT NULL,
    "CountryId" INTEGER NOT NULL,
    "Icao_Code" VARCHAR(10)
);

CREATE TABLE IF NOT EXISTS "DocumentTypes" (
    "TypeId" SERIAL PRIMARY KEY,
    "TypeName" VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Controllers" (
    "ControllerId" SERIAL PRIMARY KEY,
    "FullName" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "Password" VARCHAR(500) NOT NULL,
    "AirportId" INTEGER NOT NULL,
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
    "InsuranceNumber" VARCHAR(50)
);

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

-- Insert basic data
INSERT INTO "ConfigurationCategories" ("CategoryName", "DisplayName", "Description", "IsActive", "DisplayOrder") VALUES 
('Roles', 'User Roles', 'System user roles', true, 1),
('Departments', 'Departments', 'Organizational departments', true, 2),
('Divisions', 'Divisions', 'Organizational divisions', true, 3),
('DocumentTypes', 'Document Types', 'Types of documents and certificates', true, 4)
ON CONFLICT ("CategoryName") DO NOTHING;

INSERT INTO "DocumentTypes" ("TypeName") VALUES 
('AFTN Certificate'),
('AIS Certificate'),
('CNS Certificate'),
('License'),
('Training Certificate'),
('Medical Certificate')
ON CONFLICT DO NOTHING;

-- Insert admin user (password: admin123 - hashed with BCrypt)
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") VALUES 
('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Admin')
ON CONFLICT ("Username") DO NOTHING;

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'HR Aviation Database Quick Setup completed successfully!';
    RAISE NOTICE 'Admin user created: username=admin, password=admin123';
    RAISE NOTICE 'Database is ready for use!';
END $$;