-- =====================================================
-- SQL Server Local Database Setup
-- =====================================================
-- Run this script in SQL Server Management Studio
-- Server: localhost\SQLEXPRESS
-- =====================================================

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HR-Aviation')
BEGIN
    CREATE DATABASE [HR-Aviation];
    PRINT 'Database HR-Aviation created successfully';
END
ELSE
BEGIN
    PRINT 'Database HR-Aviation already exists';
END

-- Use the database
USE [HR-Aviation];

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE [Users] (
        [UserId] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) UNIQUE NOT NULL,
        [PasswordHash] NVARCHAR(255) NOT NULL,
        [FullName] NVARCHAR(255) NOT NULL,
        [Email] NVARCHAR(255),
        [RoleName] NVARCHAR(50) NOT NULL DEFAULT 'Controller',
        [Department] NVARCHAR(100),
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [LastLogin] DATETIME2 NULL
    );
    PRINT 'Users table created successfully';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END

-- Create Employees table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Employees' AND xtype='U')
BEGIN
    CREATE TABLE [Employees] (
        [EmployeeId] INT IDENTITY(1,1) PRIMARY KEY,
        [FullName] NVARCHAR(255) NOT NULL,
        [EmployeeNumber] NVARCHAR(50) UNIQUE NOT NULL,
        [Department] NVARCHAR(100),
        [Position] NVARCHAR(100),
        [Email] NVARCHAR(255),
        [Phone] NVARCHAR(20),
        [HireDate] DATETIME2,
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 DEFAULT GETDATE()
    );
    PRINT 'Employees table created successfully';
END
ELSE
BEGIN
    PRINT 'Employees table already exists';
END

-- Create Certificates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Certificates' AND xtype='U')
BEGIN
    CREATE TABLE [Certificates] (
        [CertificateId] INT IDENTITY(1,1) PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [CertificateType] NVARCHAR(100) NOT NULL,
        [CertificateNumber] NVARCHAR(100),
        [IssuedDate] DATETIME2,
        [ExpiryDate] DATETIME2,
        [IssuingAuthority] NVARCHAR(255),
        [Status] NVARCHAR(50) DEFAULT 'Active',
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([EmployeeId])
    );
    PRINT 'Certificates table created successfully';
END
ELSE
BEGIN
    PRINT 'Certificates table already exists';
END

-- Create Projects table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Projects' AND xtype='U')
BEGIN
    CREATE TABLE [Projects] (
        [ProjectId] INT IDENTITY(1,1) PRIMARY KEY,
        [ProjectName] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(MAX),
        [StartDate] DATETIME2,
        [EndDate] DATETIME2,
        [Status] NVARCHAR(50) DEFAULT 'Active',
        [CreatedBy] INT,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([UserId])
    );
    PRINT 'Projects table created successfully';
END
ELSE
BEGIN
    PRINT 'Projects table already exists';
END

-- Create Notifications table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [Title] NVARCHAR(255) NOT NULL,
        [Message] NVARCHAR(MAX) NOT NULL,
        [Type] NVARCHAR(50) DEFAULT 'Info',
        [IsRead] BIT DEFAULT 0,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
    );
    PRINT 'Notifications table created successfully';
END
ELSE
BEGIN
    PRINT 'Notifications table already exists';
END

-- Create UserActivityLogs table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActivityLogs' AND xtype='U')
BEGIN
    CREATE TABLE [UserActivityLogs] (
        [LogId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [Activity] NVARCHAR(255) NOT NULL,
        [Details] NVARCHAR(MAX),
        [IpAddress] NVARCHAR(45),
        [UserAgent] NVARCHAR(MAX),
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
    );
    PRINT 'UserActivityLogs table created successfully';
END
ELSE
BEGIN
    PRINT 'UserActivityLogs table already exists';
END

-- Insert admin user (password: admin123)
IF NOT EXISTS (SELECT * FROM [Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [Users] ([Username], [PasswordHash], [FullName], [Email], [RoleName], [Department], [IsActive])
    VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'System Administrator', 'admin@aviation.com', 'Admin', 'IT', 1);
    PRINT 'Admin user created successfully';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- Insert sample employee
IF NOT EXISTS (SELECT * FROM [Employees] WHERE [EmployeeNumber] = 'EMP001')
BEGIN
    INSERT INTO [Employees] ([FullName], [EmployeeNumber], [Department], [Position], [Email], [Phone], [HireDate])
    VALUES ('John Doe', 'EMP001', 'Air Traffic Control', 'Senior Controller', 'john.doe@aviation.com', '+1234567890', GETDATE());
    PRINT 'Sample employee created successfully';
END
ELSE
BEGIN
    PRINT 'Sample employee already exists';
END

-- Insert sample project
IF NOT EXISTS (SELECT * FROM [Projects] WHERE [ProjectName] = 'System Maintenance')
BEGIN
    INSERT INTO [Projects] ([ProjectName], [Description], [StartDate], [EndDate], [Status], [CreatedBy])
    VALUES ('System Maintenance', 'Regular system maintenance and updates', GETDATE(), DATEADD(MONTH, 1, GETDATE()), 'Active', 1);
    PRINT 'Sample project created successfully';
END
ELSE
BEGIN
    PRINT 'Sample project already exists';
END

-- Verify setup
PRINT '=== Database Setup Verification ===';
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM [Users]
UNION ALL
SELECT 'Employees', COUNT(*) FROM [Employees]
UNION ALL
SELECT 'Certificates', COUNT(*) FROM [Certificates]
UNION ALL
SELECT 'Projects', COUNT(*) FROM [Projects]
UNION ALL
SELECT 'Notifications', COUNT(*) FROM [Notifications]
UNION ALL
SELECT 'UserActivityLogs', COUNT(*) FROM [UserActivityLogs];

-- Show admin user
SELECT [UserId], [Username], [FullName], [Email], [RoleName], [Department], [IsActive], [CreatedAt]
FROM [Users] 
WHERE [Username] = 'admin';

PRINT '=== SQL Server Local Database Setup Complete ===';
PRINT 'Database: HR-Aviation';
PRINT 'Server: localhost\SQLEXPRESS';
PRINT 'Admin User: admin';
PRINT 'Admin Password: admin123';
PRINT 'Connection String: Server=localhost\\SQLEXPRESS;Database=HR-Aviation;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;';