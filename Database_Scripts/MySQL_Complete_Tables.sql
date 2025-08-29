-- =====================================================
-- AVIATION HR PRO - Complete MySQL Database Setup
-- =====================================================
-- This script creates ALL tables for MySQL including missing ones

USE railway;

-- =====================================================
-- STEP 1: CREATE ALL TABLES
-- =====================================================

-- Create Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId INT AUTO_INCREMENT PRIMARY KEY,
    PermissionName VARCHAR(100) NOT NULL,
    PermissionKey VARCHAR(50) NOT NULL UNIQUE,
    PermissionDescription VARCHAR(500) NULL,
    CategoryName VARCHAR(50) NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(200) NOT NULL,
    RoleName VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create Roles table
CREATE TABLE IF NOT EXISTS Roles (
    RoleId INT AUTO_INCREMENT PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(200) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create RolePermissions table
CREATE TABLE IF NOT EXISTS RolePermissions (
    RolePermissionId INT AUTO_INCREMENT PRIMARY KEY,
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Create UserDepartmentPermissions table
CREATE TABLE IF NOT EXISTS UserDepartmentPermissions (
    UserDepartmentPermissionId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    DepartmentId INT NOT NULL,
    PermissionId INT NOT NULL,
    CanView BOOLEAN NOT NULL DEFAULT FALSE,
    CanEdit BOOLEAN NOT NULL DEFAULT FALSE,
    CanDelete BOOLEAN NOT NULL DEFAULT FALSE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
);

-- Create PermissionLogs table
CREATE TABLE IF NOT EXISTS PermissionLogs (
    LogId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    PermissionId INT NULL,
    Action VARCHAR(50) NOT NULL,
    OldValue TEXT NULL,
    NewValue TEXT NULL,
    ChangedBy VARCHAR(100) NULL,
    ChangedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
);

-- Create ConfigurationCategories table
CREATE TABLE IF NOT EXISTS ConfigurationCategories (
    CategoryId INT AUTO_INCREMENT PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    DisplayName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    DisplayOrder INT DEFAULT 0,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create ConfigurationValues table
CREATE TABLE IF NOT EXISTS ConfigurationValues (
    ValueId INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NULL,
    ValueKey VARCHAR(100) NOT NULL,
    ValueText VARCHAR(200) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedBy VARCHAR(100) NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedBy VARCHAR(100) NULL,
    ModifiedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryId) REFERENCES ConfigurationCategories(CategoryId)
);

-- Create ConfigurationLog table
CREATE TABLE IF NOT EXISTS ConfigurationLog (
    LogId INT AUTO_INCREMENT PRIMARY KEY,
    ValueId INT NULL,
    Action VARCHAR(50) NOT NULL,
    OldValue TEXT NULL,
    NewValue TEXT NULL,
    ChangedBy VARCHAR(100) NULL,
    ChangedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ValueId) REFERENCES ConfigurationValues(ValueId)
);

-- Create Countries table
CREATE TABLE IF NOT EXISTS Countries (
    CountryId INT AUTO_INCREMENT PRIMARY KEY,
    CountryName VARCHAR(100) NOT NULL,
    CountryCode VARCHAR(3) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Airports table
CREATE TABLE IF NOT EXISTS Airports (
    AirportId INT AUTO_INCREMENT PRIMARY KEY,
    AirportName VARCHAR(100) NOT NULL,
    AirportCode VARCHAR(10) NULL,
    CountryId INT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)
);

-- Create DocumentTypes table
CREATE TABLE IF NOT EXISTS DocumentTypes (
    TypeId INT AUTO_INCREMENT PRIMARY KEY,
    TypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(200) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Employees table
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NULL,
    Phone VARCHAR(20) NULL,
    Department VARCHAR(50) NULL,
    Position VARCHAR(50) NULL,
    HireDate DATE NULL,
    Salary DECIMAL(10,2) NULL,
    TaxId VARCHAR(50) NULL,
    InsuranceNumber VARCHAR(50) NULL,
    PhotoPath VARCHAR(255) NULL,
    NeedLicense BOOLEAN DEFAULT TRUE,
    OrganizationalStructure VARCHAR(100) NULL,
    Division VARCHAR(100) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create Controllers table
CREATE TABLE IF NOT EXISTS Controllers (
    ControllerId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NULL,
    Phone VARCHAR(20) NULL,
    Department VARCHAR(50) NULL,
    Position VARCHAR(50) NULL,
    HireDate DATE NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create Licenses table
CREATE TABLE IF NOT EXISTS Licenses (
    LicenseId INT AUTO_INCREMENT PRIMARY KEY,
    LicenseNumber VARCHAR(50) NOT NULL,
    LicenseType VARCHAR(50) NOT NULL,
    TypeId INT NULL,
    IssuedDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    Notes TEXT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (TypeId) REFERENCES DocumentTypes(TypeId)
);

-- Create Certificates table
CREATE TABLE IF NOT EXISTS Certificates (
    CertificateId INT AUTO_INCREMENT PRIMARY KEY,
    CertificateNumber VARCHAR(50) NOT NULL,
    CertificateType VARCHAR(50) NOT NULL,
    TypeId INT NULL,
    IssuedDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    Notes TEXT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (TypeId) REFERENCES DocumentTypes(TypeId)
);

-- Create Projects table
CREATE TABLE IF NOT EXISTS Projects (
    ProjectId INT AUTO_INCREMENT PRIMARY KEY,
    ProjectName VARCHAR(100) NOT NULL,
    Description TEXT NULL,
    Location VARCHAR(100) NULL,
    AssociatedEntity VARCHAR(100) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    FolderPath VARCHAR(255) NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create ProjectParticipants table
CREATE TABLE IF NOT EXISTS ProjectParticipants (
    ParticipantId INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    ControllerId INT NULL,
    EmployeeId INT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (ControllerId) REFERENCES Controllers(ControllerId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);

-- Create ProjectDivisions table
CREATE TABLE IF NOT EXISTS ProjectDivisions (
    ProjectDivisionId INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    DivisionId INT NOT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (DivisionId) REFERENCES Airports(AirportId)
);

-- Create ProjectPhases table
CREATE TABLE IF NOT EXISTS ProjectPhases (
    PhaseId INT AUTO_INCREMENT PRIMARY KEY,
    ProjectId INT NOT NULL,
    PhaseName VARCHAR(100) NOT NULL,
    Description TEXT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
);

-- Create Observations table
CREATE TABLE IF NOT EXISTS Observations (
    ObservationId INT AUTO_INCREMENT PRIMARY KEY,
    ObservationType VARCHAR(50) NOT NULL,
    Description TEXT NULL,
    ObservationDate DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    Notes TEXT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Create Notifications table
CREATE TABLE IF NOT EXISTS Notifications (
    NotificationId INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    Type VARCHAR(50) DEFAULT 'Info',
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- STEP 2: INSERT DEFAULT DATA
-- =====================================================

-- Insert default permissions
INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName) VALUES
('View Dashboard', 'DASHBOARD_VIEW', 'Can view the main dashboard', 'Dashboard'),
('Export Dashboard Data', 'DASHBOARD_EXPORT', 'Can export dashboard data', 'Dashboard'),
('View Organization', 'ORGANIZATION_VIEW', 'Can view organization information', 'Organization'),
('Edit Organization', 'ORGANIZATION_EDIT', 'Can edit organization information', 'Organization'),
('View Employees', 'EMPLOYEES_VIEW', 'Can view employee list', 'Staff'),
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('View Controllers', 'CONTROLLERS_VIEW', 'Can view controller list', 'Staff'),
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff'),
('View Licenses', 'LICENSES_VIEW', 'Can view license list', 'Documents'),
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('View Certificates', 'CERTIFICATES_VIEW', 'Can view certificate list', 'Documents'),
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents'),
('View Projects', 'PROJECTS_VIEW', 'Can view project list', 'Projects'),
('Add Project', 'PROJECTS_ADD', 'Can add new projects', 'Projects'),
('Edit Project', 'PROJECTS_EDIT', 'Can edit project information', 'Projects'),
('Delete Project', 'PROJECTS_DELETE', 'Can delete projects', 'Projects'),
('View Observations', 'OBSERVATIONS_VIEW', 'Can view observation list', 'Observations'),
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Observations'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observation information', 'Observations'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Observations'),
('View Notifications', 'NOTIFICATIONS_VIEW', 'Can view notifications', 'System'),
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System'),
('System Configuration', 'SYSTEM_CONFIG', 'Can configure system settings', 'System'),
('Roles Management', 'ROLES_MANAGEMENT', 'Can manage roles', 'System'),
('Permission Logs View', 'PERMISSION_LOGS_VIEW', 'Can view permission logs', 'System');

-- Insert default roles
INSERT INTO Roles (RoleName, Description) VALUES
('Admin', 'System Administrator with full access'),
('Manager', 'Department Manager with limited administrative access'),
('Supervisor', 'Team Supervisor with team management access'),
('Employee', 'Regular employee with basic access'),
('Controller', 'Air Traffic Controller with specialized access');

-- Insert default admin user
INSERT INTO Users (Username, PasswordHash, RoleName) VALUES
('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin'); -- Password: 123

-- Insert default countries
INSERT INTO Countries (CountryName, CountryCode) VALUES
('Jordan', 'JOR'),
('Saudi Arabia', 'SAU'),
('United Arab Emirates', 'UAE'),
('Kuwait', 'KWT'),
('Qatar', 'QAT'),
('Bahrain', 'BHR'),
('Oman', 'OMN'),
('Egypt', 'EGY'),
('Lebanon', 'LBN'),
('Iraq', 'IRQ');

-- Insert default airports
INSERT INTO Airports (AirportName, AirportCode, CountryId) VALUES
('Queen Alia International Airport', 'AMM', 1),
('King Abdulaziz International Airport', 'JED', 2),
('King Khalid International Airport', 'RUH', 2),
('Dubai International Airport', 'DXB', 3),
('Abu Dhabi International Airport', 'AUH', 3),
('Kuwait International Airport', 'KWI', 4),
('Hamad International Airport', 'DOH', 5),
('Bahrain International Airport', 'BAH', 6),
('Muscat International Airport', 'MCT', 7),
('Cairo International Airport', 'CAI', 8);

-- Insert default document types
INSERT INTO DocumentTypes (TypeName, Description) VALUES
('Pilot License', 'Commercial Pilot License'),
('Controller License', 'Air Traffic Controller License'),
('Medical Certificate', 'Aviation Medical Certificate'),
('Training Certificate', 'Professional Training Certificate'),
('Safety Certificate', 'Safety and Security Certificate'),
('Technical Certificate', 'Technical Skills Certificate'),
('Management Certificate', 'Management and Leadership Certificate'),
('Language Certificate', 'Language Proficiency Certificate');

-- Insert default configuration categories
INSERT INTO ConfigurationCategories (CategoryName, DisplayName, Description, DisplayOrder) VALUES
('System', 'System Settings', 'General system configuration', 1),
('Email', 'Email Settings', 'Email notification settings', 2),
('Security', 'Security Settings', 'Security and authentication settings', 3),
('UI', 'User Interface', 'User interface customization', 4),
('Roles', 'Roles Management', 'System roles configuration', 5),
('Divisions', 'Divisions Management', 'Organizational divisions', 6);

-- Insert default configuration values
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(1, 'SystemName', 'AVIATION HR PRO', 1),
(1, 'SystemVersion', '2.0.0', 2),
(1, 'MaintenanceMode', 'false', 3),
(2, 'SmtpServer', 'smtp.gmail.com', 1),
(2, 'SmtpPort', '587', 2),
(2, 'SmtpUsername', 'noreply@aviationhr.com', 3),
(3, 'PasswordMinLength', '8', 1),
(3, 'PasswordRequireSpecialChar', 'true', 2),
(3, 'SessionTimeout', '30', 3),
(4, 'Theme', 'default', 1),
(4, 'Language', 'en', 2),
(5, 'Admin', 'System Administrator', 1),
(5, 'Manager', 'Department Manager', 2),
(5, 'Supervisor', 'Team Supervisor', 3),
(5, 'Employee', 'Regular Employee', 4),
(5, 'Controller', 'Air Traffic Controller', 5),
(6, 'AIS', 'Aeronautical Information Service', 1),
(6, 'CNS', 'Communications, Navigation and Surveillance', 2),
(6, 'Ops Staff', 'Operations Staff', 3),
(6, 'AFTN', 'Aeronautical Fixed Telecommunications Network', 4),
(6, 'Controller', 'Air Traffic Controller', 5);

-- Insert default role permissions (Admin gets all permissions)
INSERT INTO RolePermissions (RoleId, PermissionId) 
SELECT 1, PermissionId FROM Permissions WHERE IsActive = TRUE;

SELECT 'Complete database setup finished successfully!' AS Status; 