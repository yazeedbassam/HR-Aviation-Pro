-- HR Aviation System - Complete Supabase Database Schema
-- This script creates ALL necessary tables for the HR Aviation system

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- 1. الجداول الأساسية للمستخدمين والصلاحيات
-- =============================================

-- Users table
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP NULL,
    CurrentDepartment VARCHAR(100) NULL,
    Role VARCHAR(50) DEFAULT 'User'
);

-- Roles table
CREATE TABLE IF NOT EXISTS Roles (
    RoleId SERIAL PRIMARY KEY,
    RoleName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId SERIAL PRIMARY KEY,
    PermissionName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT NULL,
    Category VARCHAR(50) NOT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- RolePermissions table
CREATE TABLE IF NOT EXISTS RolePermissions (
    RolePermissionId SERIAL PRIMARY KEY,
    RoleId INTEGER REFERENCES Roles(RoleId),
    PermissionId INTEGER REFERENCES Permissions(PermissionId),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(RoleId, PermissionId)
);

-- UserDepartmentPermissions table
CREATE TABLE IF NOT EXISTS UserDepartmentPermissions (
    UserDepartmentPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    DepartmentId INTEGER NOT NULL,
    PermissionId INTEGER REFERENCES Permissions(PermissionId),
    CanView BOOLEAN NOT NULL DEFAULT false,
    CanEdit BOOLEAN NOT NULL DEFAULT false,
    CanDelete BOOLEAN NOT NULL DEFAULT false,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- UserMenuPermissions table
CREATE TABLE IF NOT EXISTS UserMenuPermissions (
    UserMenuPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    MenuId INTEGER NOT NULL,
    CanView BOOLEAN NOT NULL DEFAULT false,
    CanEdit BOOLEAN NOT NULL DEFAULT false,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- UserOperationPermissions table
CREATE TABLE IF NOT EXISTS UserOperationPermissions (
    UserOperationPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    OperationId INTEGER NOT NULL,
    CanExecute BOOLEAN NOT NULL DEFAULT false,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- UserOrganizationalPermissions table
CREATE TABLE IF NOT EXISTS UserOrganizationalPermissions (
    UserOrganizationalPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    OrganizationalId INTEGER NOT NULL,
    PermissionId INTEGER REFERENCES Permissions(PermissionId),
    CanView BOOLEAN NOT NULL DEFAULT false,
    CanEdit BOOLEAN NOT NULL DEFAULT false,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- PermissionLogs table
CREATE TABLE IF NOT EXISTS PermissionLogs (
    LogId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    PermissionId INTEGER REFERENCES Permissions(PermissionId),
    Action VARCHAR(50) NOT NULL,
    OldValue TEXT NULL,
    NewValue TEXT NULL,
    ChangedBy VARCHAR(100) NULL,
    ChangedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 2. جداول الموظفين والمستخدمين
-- =============================================

-- Employees table
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId SERIAL PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) UNIQUE NULL,
    Phone VARCHAR(20) NULL,
    Department VARCHAR(50) NULL,
    Position VARCHAR(50) NULL,
    HireDate DATE NULL,
    Salary DECIMAL(10,2) NULL,
    TaxId VARCHAR(50) NULL,
    InsuranceNumber VARCHAR(50) NULL,
    PhotoPath VARCHAR(255) NULL,
    NeedLicense BOOLEAN DEFAULT true,
    OrganizationalStructure VARCHAR(100) NULL,
    Division VARCHAR(100) NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Controllers table
CREATE TABLE IF NOT EXISTS Controllers (
    ControllerId SERIAL PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) UNIQUE NULL,
    Phone VARCHAR(20) NULL,
    Department VARCHAR(50) NULL,
    Position VARCHAR(50) NULL,
    HireDate DATE NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 3. جداول الشهادات والتراخيص
-- =============================================

-- DocumentTypes table
CREATE TABLE IF NOT EXISTS DocumentTypes (
    TypeId SERIAL PRIMARY KEY,
    TypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(200) NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Certificates table
CREATE TABLE IF NOT EXISTS Certificates (
    CertificateId SERIAL PRIMARY KEY,
    EmployeeId INTEGER REFERENCES Employees(EmployeeId),
    CertificateType VARCHAR(100) NOT NULL,
    CertificateNumber VARCHAR(100) UNIQUE NOT NULL,
    IssueDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    IssuingAuthority VARCHAR(100) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Licenses table
CREATE TABLE IF NOT EXISTS Licenses (
    LicenseId SERIAL PRIMARY KEY,
    LicenseNumber VARCHAR(50) NOT NULL,
    LicenseType VARCHAR(50) NOT NULL,
    TypeId INTEGER REFERENCES DocumentTypes(TypeId),
    IssuedDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    Notes TEXT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 4. جداول المشاريع
-- =============================================

-- Projects table
CREATE TABLE IF NOT EXISTS Projects (
    ProjectId SERIAL PRIMARY KEY,
    ProjectName VARCHAR(200) NOT NULL,
    Description TEXT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    Priority VARCHAR(20) DEFAULT 'Medium',
    AssignedTo INTEGER REFERENCES Users(Id),
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ProjectDivisions table
CREATE TABLE IF NOT EXISTS ProjectDivisions (
    DivisionId SERIAL PRIMARY KEY,
    ProjectId INTEGER REFERENCES Projects(ProjectId),
    DivisionName VARCHAR(100) NOT NULL,
    Description TEXT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ProjectPhases table
CREATE TABLE IF NOT EXISTS ProjectPhases (
    PhaseId SERIAL PRIMARY KEY,
    ProjectId INTEGER REFERENCES Projects(ProjectId),
    PhaseName VARCHAR(100) NOT NULL,
    Description TEXT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ProjectParticipants table
CREATE TABLE IF NOT EXISTS ProjectParticipants (
    ParticipantId SERIAL PRIMARY KEY,
    ProjectId INTEGER REFERENCES Projects(ProjectId),
    UserId INTEGER REFERENCES Users(Id),
    Role VARCHAR(100) NOT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 5. جداول الملاحظات والإشعارات
-- =============================================

-- Observations table
CREATE TABLE IF NOT EXISTS Observations (
    ObservationId SERIAL PRIMARY KEY,
    EmployeeId INTEGER REFERENCES Employees(EmployeeId),
    ObservationType VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Date DATE NOT NULL,
    Status VARCHAR(50) DEFAULT 'Open',
    AssignedTo INTEGER REFERENCES Users(Id),
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Notifications table
CREATE TABLE IF NOT EXISTS Notifications (
    NotificationId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    Type VARCHAR(50) DEFAULT 'Info',
    IsRead BOOLEAN DEFAULT false,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ReadAt TIMESTAMP NULL
);

-- =============================================
-- 6. جداول نظام الإعدادات
-- =============================================

-- ConfigurationCategories table
CREATE TABLE IF NOT EXISTS ConfigurationCategories (
    CategoryId SERIAL PRIMARY KEY,
    CategoryName VARCHAR(100) UNIQUE NOT NULL,
    DisplayName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    IsActive BOOLEAN DEFAULT true,
    DisplayOrder INTEGER DEFAULT 0,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ConfigurationValues table
CREATE TABLE IF NOT EXISTS ConfigurationValues (
    ValueId SERIAL PRIMARY KEY,
    CategoryId INTEGER REFERENCES ConfigurationCategories(CategoryId),
    ValueKey VARCHAR(100) NOT NULL,
    ValueText VARCHAR(200) NOT NULL,
    DisplayOrder INTEGER DEFAULT 0,
    IsActive BOOLEAN DEFAULT true,
    CreatedBy VARCHAR(100) NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedBy VARCHAR(100) NULL,
    ModifiedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(CategoryId, ValueKey)
);

-- ConfigurationLog table
CREATE TABLE IF NOT EXISTS ConfigurationLog (
    LogId SERIAL PRIMARY KEY,
    ValueId INTEGER REFERENCES ConfigurationValues(ValueId),
    Action VARCHAR(50) NOT NULL,
    OldValue TEXT NULL,
    NewValue TEXT NULL,
    ChangedBy VARCHAR(100) NULL,
    ChangedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 7. جداول المطارات والدول
-- =============================================

-- Countries table
CREATE TABLE IF NOT EXISTS Countries (
    CountryId SERIAL PRIMARY KEY,
    CountryName VARCHAR(100) UNIQUE NOT NULL,
    CountryCode VARCHAR(3) UNIQUE NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Airports table
CREATE TABLE IF NOT EXISTS Airports (
    AirportId SERIAL PRIMARY KEY,
    AirportName VARCHAR(100) NOT NULL,
    AirportCode VARCHAR(10) UNIQUE NULL,
    CountryId INTEGER REFERENCES Countries(CountryId),
    IsActive BOOLEAN DEFAULT true,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 8. جداول السجلات والنشاطات
-- =============================================

-- UserActivityLogs table
CREATE TABLE IF NOT EXISTS UserActivityLogs (
    LogId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    Action VARCHAR(100) NOT NULL,
    Details TEXT NULL,
    IpAddress VARCHAR(45) NULL,
    UserAgent TEXT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 9. إدخال البيانات الأساسية
-- =============================================

-- Insert default admin user
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, CurrentDepartment) 
VALUES ('admin', 'admin@aviation.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', 'User', 'Admin', 'IT')
ON CONFLICT (Username) DO NOTHING;

-- Insert default roles
INSERT INTO Roles (RoleName, Description) VALUES
('Admin', 'System Administrator with full access'),
('Controller', 'Air Traffic Controller'),
('Employee', 'General Employee'),
('Manager', 'Department Manager'),
('Supervisor', 'Team Supervisor')
ON CONFLICT (RoleName) DO NOTHING;

-- Insert default permissions
INSERT INTO Permissions (PermissionName, Description, Category) VALUES
('Users.View', 'View users', 'Users'),
('Users.Create', 'Create users', 'Users'),
('Users.Edit', 'Edit users', 'Users'),
('Users.Delete', 'Delete users', 'Users'),
('Employees.View', 'View employees', 'Employees'),
('Employees.Create', 'Create employees', 'Employees'),
('Employees.Edit', 'Edit employees', 'Employees'),
('Employees.Delete', 'Delete employees', 'Employees'),
('Certificates.View', 'View certificates', 'Certificates'),
('Certificates.Create', 'Create certificates', 'Certificates'),
('Certificates.Edit', 'Edit certificates', 'Certificates'),
('Certificates.Delete', 'Delete certificates', 'Certificates'),
('Projects.View', 'View projects', 'Projects'),
('Projects.Create', 'Create projects', 'Projects'),
('Projects.Edit', 'Edit projects', 'Projects'),
('Projects.Delete', 'Delete projects', 'Projects'),
('System.Admin', 'Full system administration', 'System'),
('Configuration.Manage', 'Manage system configuration', 'Configuration')
ON CONFLICT (PermissionName) DO NOTHING;

-- Insert default configuration categories
INSERT INTO ConfigurationCategories (CategoryName, DisplayName, Description, DisplayOrder) VALUES
('JobTitles', 'Job Titles', 'Different job positions in the organization', 1),
('Departments', 'Departments', 'Organization departments and divisions', 2),
('Roles', 'User Roles', 'System user roles and permissions', 3),
('LicenseTypes', 'License Types', 'Different types of licenses and certificates', 4),
('EmploymentStatus', 'Employment Status', 'Employee employment status options', 5),
('EducationLevels', 'Education Levels', 'Educational qualification levels', 6),
('MaritalStatus', 'Marital Status', 'Marital status options', 7),
('Gender', 'Gender', 'Gender options', 8),
('ProjectStatuses', 'Project Statuses', 'Project status options', 9),
('ProjectTypes', 'Project Types', 'Different project types', 10),
('CertificateTypes', 'Certificate Types', 'Different certificate types', 11),
('IssuingAuthorities', 'Issuing Authorities', 'Certificate issuing authorities', 12)
ON CONFLICT (CategoryName) DO NOTHING;

-- Insert sample countries
INSERT INTO Countries (CountryName, CountryCode) VALUES
('Jordan', 'JOR'),
('United States', 'USA'),
('United Kingdom', 'GBR'),
('Germany', 'DEU'),
('France', 'FRA')
ON CONFLICT (CountryCode) DO NOTHING;

-- Insert sample airports
INSERT INTO Airports (AirportName, AirportCode, CountryId) VALUES
('Queen Alia International Airport', 'AMM', (SELECT CountryId FROM Countries WHERE CountryCode = 'JOR')),
('John F. Kennedy International Airport', 'JFK', (SELECT CountryId FROM Countries WHERE CountryCode = 'USA')),
('London Heathrow Airport', 'LHR', (SELECT CountryId FROM Countries WHERE CountryCode = 'GBR'))
ON CONFLICT (AirportCode) DO NOTHING;

-- =============================================
-- 10. إنشاء الفهارس لتحسين الأداء
-- =============================================

CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_employees_employeid ON Employees(EmployeeId);
CREATE INDEX IF NOT EXISTS idx_employees_department ON Employees(Department);
CREATE INDEX IF NOT EXISTS idx_certificates_employeid ON Certificates(EmployeeId);
CREATE INDEX IF NOT EXISTS idx_certificates_expirydate ON Certificates(ExpiryDate);
CREATE INDEX IF NOT EXISTS idx_projects_status ON Projects(Status);
CREATE INDEX IF NOT EXISTS idx_notifications_userid ON Notifications(UserId);
CREATE INDEX IF NOT EXISTS idx_notifications_isread ON Notifications(IsRead);
CREATE INDEX IF NOT EXISTS idx_useractivitylogs_userid ON UserActivityLogs(UserId);
CREATE INDEX IF NOT EXISTS idx_useractivitylogs_createdate ON UserActivityLogs(CreatedDate);
CREATE INDEX IF NOT EXISTS idx_licenses_expirydate ON Licenses(ExpiryDate);
CREATE INDEX IF NOT EXISTS idx_observations_date ON Observations(Date);
CREATE INDEX IF NOT EXISTS idx_configurationvalues_categoryid ON ConfigurationValues(CategoryId);

-- =============================================
-- 11. إنشاء Triggers للتحديث التلقائي
-- =============================================

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedDate = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_employees_updated_at BEFORE UPDATE ON Employees
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_certificates_updated_at BEFORE UPDATE ON Certificates
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_projects_updated_at BEFORE UPDATE ON Projects
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_observations_updated_at BEFORE UPDATE ON Observations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_licenses_updated_at BEFORE UPDATE ON Licenses
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_configurationvalues_modified_at BEFORE UPDATE ON ConfigurationValues
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- 12. منح الصلاحيات
-- =============================================

-- Grant all permissions to admin user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete)
SELECT u.Id, 1, p.PermissionId, true, true, true
FROM Users u, Permissions p
WHERE u.Username = 'admin'
ON CONFLICT (UserId, DepartmentId, PermissionId) DO NOTHING;

-- Grant necessary permissions to postgres user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres;

-- =============================================
-- 13. رسالة نجاح
-- =============================================

DO $$
BEGIN
    RAISE NOTICE 'HR Aviation System database schema created successfully!';
    RAISE NOTICE 'Total tables created: 25+';
    RAISE NOTICE 'Default admin user: admin/password';
    RAISE NOTICE 'Default roles and permissions configured';
END $$; 