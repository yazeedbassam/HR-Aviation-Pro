-- MySQL Database Setup for HR Aviation System
-- Run this script in your MySQL database

-- Create database if not exists
CREATE DATABASE IF NOT EXISTS railway;
USE railway;

-- Create Controllers table
CREATE TABLE IF NOT EXISTS Controllers (
    controllerid INT AUTO_INCREMENT PRIMARY KEY,
    fullname VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'Controller',
    airportid INT,
    photopath VARCHAR(500),
    licensepath VARCHAR(500),
    job_title VARCHAR(100),
    education_level VARCHAR(100),
    date_of_birth DATE,
    marital_status VARCHAR(50),
    phone_number VARCHAR(20),
    email VARCHAR(255),
    address VARCHAR(500),
    hire_date DATE,
    employment_status VARCHAR(50),
    current_department VARCHAR(100),
    transfer_date DATE,
    emergency_contact VARCHAR(500),
    LicenseNumber VARCHAR(100),
    NeedLicense BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    CurrentSalary DECIMAL(10,2),
    AnnualIncreasePercentage DECIMAL(5,2),
    SalaryAfterAnnualIncrease DECIMAL(10,2),
    BankAccountNumber VARCHAR(50),
    BankName VARCHAR(100),
    TaxId VARCHAR(50),
    InsuranceNumber VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Airports table
CREATE TABLE IF NOT EXISTS Airports (
    airportid INT AUTO_INCREMENT PRIMARY KEY,
    airport_name VARCHAR(255) NOT NULL,
    airport_code VARCHAR(10) UNIQUE NOT NULL,
    country VARCHAR(100),
    city VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Countries table
CREATE TABLE IF NOT EXISTS Countries (
    countryid INT AUTO_INCREMENT PRIMARY KEY,
    country_name VARCHAR(100) NOT NULL,
    country_code VARCHAR(3) UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Employees table
CREATE TABLE IF NOT EXISTS Employees (
    employeeid INT AUTO_INCREMENT PRIMARY KEY,
    fullname VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'Employee',
    department VARCHAR(100),
    position VARCHAR(100),
    hire_date DATE,
    salary DECIMAL(10,2),
    email VARCHAR(255),
    phone VARCHAR(20),
    address VARCHAR(500),
    IsActive BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Licenses table
CREATE TABLE IF NOT EXISTS Licenses (
    licenseid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    license_type VARCHAR(100) NOT NULL,
    license_number VARCHAR(100) UNIQUE NOT NULL,
    issue_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    file_path VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Certificates table
CREATE TABLE IF NOT EXISTS Certificates (
    certificateid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    certificate_type VARCHAR(100) NOT NULL,
    certificate_number VARCHAR(100) UNIQUE NOT NULL,
    issue_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    file_path VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Observations table
CREATE TABLE IF NOT EXISTS Observations (
    observationid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    observation_type VARCHAR(100) NOT NULL,
    description TEXT,
    observation_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'Pending',
    file_path VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Projects table
CREATE TABLE IF NOT EXISTS Projects (
    projectid INT AUTO_INCREMENT PRIMARY KEY,
    project_name VARCHAR(255) NOT NULL,
    description TEXT,
    start_date DATE,
    end_date DATE,
    status VARCHAR(50) DEFAULT 'Active',
    budget DECIMAL(15,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Notifications table
CREATE TABLE IF NOT EXISTS Notifications (
    notificationid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    type VARCHAR(50) DEFAULT 'Info',
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    permissionid INT AUTO_INCREMENT PRIMARY KEY,
    permission_name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Roles table
CREATE TABLE IF NOT EXISTS Roles (
    roleid INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create RolePermissions table
CREATE TABLE IF NOT EXISTS RolePermissions (
    rolepermissionid INT AUTO_INCREMENT PRIMARY KEY,
    roleid INT NOT NULL,
    permissionid INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (roleid) REFERENCES Roles(roleid),
    FOREIGN KEY (permissionid) REFERENCES Permissions(permissionid)
);

-- Create ConfigurationCategories table
CREATE TABLE IF NOT EXISTS ConfigurationCategories (
    categoryid INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create ConfigurationValues table
CREATE TABLE IF NOT EXISTS ConfigurationValues (
    valueid INT AUTO_INCREMENT PRIMARY KEY,
    categoryid INT NOT NULL,
    key_name VARCHAR(100) NOT NULL,
    value_text TEXT,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (categoryid) REFERENCES ConfigurationCategories(categoryid)
);

-- Insert default admin user (password: 123)
INSERT INTO Controllers (fullname, username, password, role, IsActive) 
VALUES ('System Administrator', 'admin', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/HS.iK2', 'Admin', TRUE)
ON DUPLICATE KEY UPDATE IsActive = TRUE;

-- Insert default roles
INSERT INTO Roles (role_name, description) VALUES 
('Admin', 'System Administrator with full access'),
('Controller', 'Air Traffic Controller'),
('Employee', 'General Employee')
ON DUPLICATE KEY UPDATE role_name = role_name;

-- Insert default permissions
INSERT INTO Permissions (permission_name, description) VALUES 
('View_Dashboard', 'Can view dashboard'),
('Manage_Users', 'Can manage users'),
('Manage_Licenses', 'Can manage licenses'),
('Manage_Certificates', 'Can manage certificates'),
('Manage_Projects', 'Can manage projects'),
('View_Reports', 'Can view reports'),
('System_Admin', 'Full system administration')
ON DUPLICATE KEY UPDATE permission_name = permission_name;

-- Insert default configuration categories
INSERT INTO ConfigurationCategories (category_name, description) VALUES 
('System', 'System-wide configuration'),
('Email', 'Email settings'),
('Security', 'Security settings'),
('UI', 'User interface settings')
ON DUPLICATE KEY UPDATE category_name = category_name;

-- Insert default configuration values
INSERT INTO ConfigurationValues (categoryid, key_name, value_text, description) VALUES 
(1, 'SystemName', 'HR Aviation Pro', 'System display name'),
(1, 'Version', '1.0.0', 'System version'),
(2, 'SmtpServer', 'smtp-relay.brevo.com', 'SMTP server address'),
(2, 'SmtpPort', '587', 'SMTP server port'),
(3, 'SessionTimeout', '30', 'Session timeout in minutes'),
(4, 'Theme', 'default', 'Default UI theme')
ON DUPLICATE KEY UPDATE value_text = VALUES(value_text);

-- Create indexes for better performance
CREATE INDEX idx_controllers_username ON Controllers(username);
CREATE INDEX idx_controllers_role ON Controllers(role);
CREATE INDEX idx_controllers_isactive ON Controllers(IsActive);
CREATE INDEX idx_licenses_expiry ON Licenses(expiry_date);
CREATE INDEX idx_certificates_expiry ON Certificates(expiry_date);
CREATE INDEX idx_observations_date ON Observations(observation_date);
CREATE INDEX idx_notifications_userid ON Notifications(userid);
CREATE INDEX idx_notifications_isread ON Notifications(is_read);

-- Show created tables
SHOW TABLES;

-- Show admin user
SELECT controllerid, fullname, username, role, IsActive FROM Controllers WHERE username = 'admin'; 