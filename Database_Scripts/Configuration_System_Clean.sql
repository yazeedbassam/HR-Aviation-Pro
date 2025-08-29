-- =============================================
-- ANSMS Pro - Configuration System Database Script (CLEAN)
-- =============================================

USE [HR-Aviation]
GO

-- =============================================
-- 1. حذف الجداول الموجودة (إذا وجدت)
-- =============================================

-- حذف الجداول بالترتيب الصحيح (من الأحدث إلى الأقدم)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigurationLog]') AND type in (N'U'))
    DROP TABLE [dbo].[ConfigurationLog];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigurationValues]') AND type in (N'U'))
    DROP TABLE [dbo].[ConfigurationValues];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigurationCategories]') AND type in (N'U'))
    DROP TABLE [dbo].[ConfigurationCategories];
GO

-- =============================================
-- 2. إنشاء الجداول من جديد
-- =============================================

-- جدول الفئات الرئيسية
CREATE TABLE ConfigurationCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) UNIQUE NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    DisplayOrder INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME
);
GO

-- جدول القيم
CREATE TABLE ConfigurationValues (
    ValueId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT FOREIGN KEY REFERENCES ConfigurationCategories(CategoryId),
    ValueKey NVARCHAR(100) NOT NULL,
    ValueText NVARCHAR(200) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
);
GO

-- جدول سجل التغييرات
CREATE TABLE ConfigurationLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    ValueId INT,
    Action NVARCHAR(50),
    OldValue NVARCHAR(500),
    NewValue NVARCHAR(500),
    ChangedBy NVARCHAR(100),
    ChangedDate DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- 3. إدخال الفئات الأساسية
-- =============================================

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
('IssuingAuthorities', 'Issuing Authorities', 'Certificate issuing authorities', 12);
GO

-- =============================================
-- 4. إدخال القيم الأساسية
-- =============================================

-- Job Titles
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(1, 'ATC', 'Air Traffic Controller', 1),
(1, 'SUPERVISOR', 'Supervisor', 2),
(1, 'INSTRUCTOR', 'Instructor', 3),
(1, 'EXAMINER', 'Examiner', 4),
(1, 'MANAGER', 'Manager', 5),
(1, 'OJTI', 'OJTI', 6),
(1, 'OTHERS', 'Others', 7);
GO

-- Departments
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(2, 'AIS', 'AIS - Aeronautical Information Services', 1),
(2, 'CNS', 'CNS - Communication, Navigation, and Surveillance', 2),
(2, 'ADMINISTRATION', 'Administration', 3),
(2, 'QUEEN', 'Queen', 4),
(2, 'AQABA', 'Aqaba', 5),
(2, 'AMMAN', 'Amman', 6),
(2, 'TACC', 'TACC', 7),
(2, 'CARC', 'CARC', 8);
GO

-- Roles
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(3, 'ADMIN', 'Administrator', 1),
(3, 'CONTROLLER', 'Controller', 2),
(3, 'EMPLOYEE', 'Employee', 3),
(3, 'SUPERVISOR', 'Supervisor', 4),
(3, 'MANAGER', 'Manager', 5);
GO

-- License Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(4, 'ATC_LICENSE', 'ATC License', 1),
(4, 'MEDICAL_CERTIFICATE', 'Medical Certificate', 2),
(4, 'TRAINING_CERTIFICATE', 'Training Certificate', 3),
(4, 'PROFESSIONAL_LICENSE', 'Professional License', 4);
GO

-- Employment Status
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(5, 'ACTIVE', 'Active', 1),
(5, 'INACTIVE', 'Inactive', 2),
(5, 'ON_LEAVE', 'On Leave', 3),
(5, 'TERMINATED', 'Terminated', 4);
GO

-- Education Levels
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(6, 'DIPLOMA', 'Diploma', 1),
(6, 'BACHELOR', 'Bachelor', 2),
(6, 'MASTER', 'Master', 3),
(6, 'PHD', 'PhD', 4),
(6, 'OTHER', 'Other', 5);
GO

-- Marital Status
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(7, 'SINGLE', 'Single', 1),
(7, 'MARRIED', 'Married', 2),
(7, 'DIVORCED', 'Divorced', 3),
(7, 'OTHER', 'Other', 4);
GO

-- Gender
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(8, 'MALE', 'Male', 1),
(8, 'FEMALE', 'Female', 2);
GO

-- Project Statuses
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(9, 'ACTIVE', 'Active', 1),
(9, 'COMPLETED', 'Completed', 2),
(9, 'ON_HOLD', 'On Hold', 3),
(9, 'CANCELLED', 'Cancelled', 4);
GO

-- Project Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(10, 'TRAINING', 'Training', 1),
(10, 'MAINTENANCE', 'Maintenance', 2),
(10, 'DEVELOPMENT', 'Development', 3),
(10, 'AUDIT', 'Audit', 4);
GO

-- Certificate Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(11, 'TRAINING_CERTIFICATE', 'Training Certificate', 1),
(11, 'PROFESSIONAL_LICENSE', 'Professional License', 2),
(11, 'MEDICAL_CERTIFICATE', 'Medical Certificate', 3);
GO

-- Issuing Authorities
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(12, 'ICAO', 'ICAO', 1),
(12, 'LOCAL_AUTHORITY', 'Local Authority', 2),
(12, 'TRAINING_CENTER', 'Training Center', 3);
GO

-- =============================================
-- 5. إنشاء Indexes للأداء
-- =============================================

CREATE INDEX IX_ConfigurationValues_CategoryId ON ConfigurationValues(CategoryId);
CREATE INDEX IX_ConfigurationValues_IsActive ON ConfigurationValues(IsActive);
CREATE INDEX IX_ConfigurationValues_DisplayOrder ON ConfigurationValues(DisplayOrder);
CREATE INDEX IX_ConfigurationLog_ValueId ON ConfigurationLog(ValueId);
CREATE INDEX IX_ConfigurationLog_ChangedDate ON ConfigurationLog(ChangedDate);
GO

-- =============================================
-- 6. التحقق من النتائج
-- =============================================

PRINT 'Configuration System Database Script completed successfully!'

DECLARE @CategoryCount INT, @ValueCount INT;
SELECT @CategoryCount = COUNT(*) FROM ConfigurationCategories;
SELECT @ValueCount = COUNT(*) FROM ConfigurationValues;

PRINT 'Total Categories Created: ' + CAST(@CategoryCount AS NVARCHAR(10))
PRINT 'Total Values Created: ' + CAST(@ValueCount AS NVARCHAR(10))

-- عرض عينة من البيانات
SELECT TOP 5 
    c.DisplayName AS Category,
    v.ValueText AS Value,
    v.ValueKey AS ValueKey
FROM ConfigurationValues v
INNER JOIN ConfigurationCategories c ON v.CategoryId = c.CategoryId
WHERE v.IsActive = 1
ORDER BY c.DisplayOrder, v.DisplayOrder;
GO 
