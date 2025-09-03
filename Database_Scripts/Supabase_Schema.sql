-- HR Aviation System - Supabase Database Schema
-- This script creates all necessary tables for the HR Aviation system

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

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

-- Employees table
CREATE TABLE IF NOT EXISTS Employees (
    Id SERIAL PRIMARY KEY,
    EmployeeNumber VARCHAR(20) UNIQUE NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Phone VARCHAR(20) NULL,
    Department VARCHAR(100) NOT NULL,
    Position VARCHAR(100) NOT NULL,
    HireDate DATE NOT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Certificates table
CREATE TABLE IF NOT EXISTS Certificates (
    Id SERIAL PRIMARY KEY,
    EmployeeId INTEGER REFERENCES Employees(Id),
    CertificateType VARCHAR(100) NOT NULL,
    CertificateNumber VARCHAR(100) UNIQUE NOT NULL,
    IssueDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    IssuingAuthority VARCHAR(100) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Projects table
CREATE TABLE IF NOT EXISTS Projects (
    Id SERIAL PRIMARY KEY,
    ProjectName VARCHAR(200) NOT NULL,
    Description TEXT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    Priority VARCHAR(20) DEFAULT 'Medium',
    AssignedTo INTEGER REFERENCES Users(Id),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Notifications table
CREATE TABLE IF NOT EXISTS Notifications (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    Type VARCHAR(50) DEFAULT 'Info',
    IsRead BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ReadAt TIMESTAMP NULL
);

-- UserActivityLog table
CREATE TABLE IF NOT EXISTS UserActivityLog (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    Action VARCHAR(100) NOT NULL,
    Details TEXT NULL,
    IpAddress VARCHAR(45) NULL,
    UserAgent TEXT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT NULL,
    Category VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- UserPermissions table
CREATE TABLE IF NOT EXISTS UserPermissions (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    PermissionId INTEGER REFERENCES Permissions(Id),
    GrantedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    GrantedBy INTEGER REFERENCES Users(Id),
    UNIQUE(UserId, PermissionId)
);

-- Airports table
CREATE TABLE IF NOT EXISTS Airports (
    Id SERIAL PRIMARY KEY,
    IcaoCode VARCHAR(4) UNIQUE NOT NULL,
    IataCode VARCHAR(3) NULL,
    Name VARCHAR(200) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    Latitude DECIMAL(10, 8) NULL,
    Longitude DECIMAL(11, 8) NULL,
    Elevation INTEGER NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Countries table
CREATE TABLE IF NOT EXISTS Countries (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) UNIQUE NOT NULL,
    Code VARCHAR(3) UNIQUE NOT NULL,
    PhoneCode VARCHAR(10) NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Observations table
CREATE TABLE IF NOT EXISTS Observations (
    Id SERIAL PRIMARY KEY,
    EmployeeId INTEGER REFERENCES Employees(Id),
    ObservationType VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Date DATE NOT NULL,
    Status VARCHAR(50) DEFAULT 'Open',
    AssignedTo INTEGER REFERENCES Users(Id),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Licenses table
CREATE TABLE IF NOT EXISTS Licenses (
    Id SERIAL PRIMARY KEY,
    EmployeeId INTEGER REFERENCES Employees(Id),
    LicenseType VARCHAR(100) NOT NULL,
    LicenseNumber VARCHAR(100) UNIQUE NOT NULL,
    IssueDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    IssuingAuthority VARCHAR(100) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Active',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Configuration table
CREATE TABLE IF NOT EXISTS Configuration (
    Id SERIAL PRIMARY KEY,
    Category VARCHAR(100) NOT NULL,
    Key VARCHAR(100) NOT NULL,
    Value TEXT NOT NULL,
    Description TEXT NULL,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(Category, Key)
);

-- Insert default admin user
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, CurrentDepartment) 
VALUES ('admin', 'admin@aviation.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', 'User', 'Admin', 'IT')
ON CONFLICT (Username) DO NOTHING;

-- Insert default permissions
INSERT INTO Permissions (Name, Description, Category) VALUES
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
('Projects.Delete', 'Delete projects', 'Projects')
ON CONFLICT (Name) DO NOTHING;

-- Grant all permissions to admin user
INSERT INTO UserPermissions (UserId, PermissionId, GrantedBy)
SELECT u.Id, p.Id, u.Id
FROM Users u, Permissions p
WHERE u.Username = 'admin'
ON CONFLICT (UserId, PermissionId) DO NOTHING;

-- Insert sample countries
INSERT INTO Countries (Name, Code, PhoneCode) VALUES
('Jordan', 'JOR', '+962'),
('United States', 'USA', '+1'),
('United Kingdom', 'GBR', '+44'),
('Germany', 'DEU', '+49'),
('France', 'FRA', '+33')
ON CONFLICT (Code) DO NOTHING;

-- Insert sample airports
INSERT INTO Airports (IcaoCode, IataCode, Name, City, Country) VALUES
('OJAI', 'AMM', 'Queen Alia International Airport', 'Amman', 'Jordan'),
('KJFK', 'JFK', 'John F. Kennedy International Airport', 'New York', 'United States'),
('EGLL', 'LHR', 'London Heathrow Airport', 'London', 'United Kingdom')
ON CONFLICT (IcaoCode) DO NOTHING;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_employees_employeenumber ON Employees(EmployeeNumber);
CREATE INDEX IF NOT EXISTS idx_employees_department ON Employees(Department);
CREATE INDEX IF NOT EXISTS idx_certificates_employeid ON Certificates(EmployeeId);
CREATE INDEX IF NOT EXISTS idx_certificates_expirydate ON Certificates(ExpiryDate);
CREATE INDEX IF NOT EXISTS idx_projects_status ON Projects(Status);
CREATE INDEX IF NOT EXISTS idx_notifications_userid ON Notifications(UserId);
CREATE INDEX IF NOT EXISTS idx_notifications_isread ON Notifications(IsRead);
CREATE INDEX IF NOT EXISTS idx_useractivitylog_userid ON UserActivityLog(UserId);
CREATE INDEX IF NOT EXISTS idx_useractivitylog_createdat ON UserActivityLog(CreatedAt);

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = CURRENT_TIMESTAMP;
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

CREATE TRIGGER update_configuration_updated_at BEFORE UPDATE ON Configuration
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Grant necessary permissions to postgres user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres; 