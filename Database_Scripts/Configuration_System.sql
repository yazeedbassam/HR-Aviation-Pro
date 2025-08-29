-- =============================================
-- ANSMS Pro - Configuration System Database Script
-- =============================================

USE [HR-Aviation]
GO

-- =============================================
-- 1. إنشاء جداول نظام الإعدادات
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

-- جدول سجل التغييرات
CREATE TABLE ConfigurationLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    ValueId INT,
    Action NVARCHAR(50), -- INSERT, UPDATE, DELETE
    OldValue NVARCHAR(500),
    NewValue NVARCHAR(500),
    ChangedBy NVARCHAR(100),
    ChangedDate DATETIME DEFAULT GETDATE()
);

-- =============================================
-- 2. إدخال الفئات الأساسية
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

-- =============================================
-- 3. إدخال القيم الأساسية
-- =============================================

-- Job Titles (من ControllerUserController.cs و EmployeesController.cs)
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
-- من ControllerUserController
(1, 'ATC', 'Air Traffic Controller', 1),
(1, 'SUPERVISOR', 'Supervisor', 2),
(1, 'INSTRUCTOR', 'Instructor', 3),
(1, 'EXAMINER', 'Examiner', 4),
(1, 'MANAGER', 'Manager', 5),
(1, 'OJTI', 'OJTI', 6),
(1, 'OTHERS', 'Others', 7),
-- من EmployeesController
(1, 'AIS_OFFICER', 'AIS Officer', 8),
(1, 'AIS_TECHNICIAN', 'AIS Technician', 9),
(1, 'AFTN_TECHNICIAN', 'AFTN Technician', 10),
(1, 'ASSISTANT', 'Assistant', 11),
(1, 'SECTION_HEAD', 'Section Head', 12),
(1, 'DATA_ANALYST', 'Data Analyst', 13),
(1, 'COORDINATOR', 'Coordinator', 14),
(1, 'DATA_QUALITY_SPECIALIST', 'Data Quality Specialist', 15);

-- Departments (من EmployeesController.cs)
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(2, 'AIS', 'AIS - Aeronautical Information Services', 1),
(2, 'CNS', 'CNS - Communication, Navigation, and Surveillance', 2),
(2, 'AIRSPACE_MANAGEMENT', 'Airspace Management - ASM', 3),
(2, 'AFTN', 'AFTN - Aeronautical Fixed Telecommunication Network', 4),
(2, 'ADMINISTRATION', 'Administration', 5),
(2, 'SAFETY_QUALITY', 'Safety & Quality Management', 6),
-- من ControllerUserController
(2, 'QUEEN', 'Queen', 7),
(2, 'AQABA', 'Aqaba', 8),
(2, 'AMMAN', 'Amman', 9),
(2, 'TACC', 'TACC', 10),
(2, 'CARC', 'CARC', 11);

-- Roles
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(3, 'ADMIN', 'Administrator', 1),
(3, 'CONTROLLER', 'Controller', 2),
(3, 'EMPLOYEE', 'Employee', 3),
(3, 'SUPERVISOR', 'Supervisor', 4),
(3, 'MANAGER', 'Manager', 5);

-- License Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(4, 'ATC_LICENSE', 'ATC License', 1),
(4, 'MEDICAL_CERTIFICATE', 'Medical Certificate', 2),
(4, 'TRAINING_CERTIFICATE', 'Training Certificate', 3),
(4, 'PROFESSIONAL_LICENSE', 'Professional License', 4),
(4, 'ENDORSEMENT', 'Endorsement', 5);

-- Employment Status
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(5, 'ACTIVE', 'Active', 1),
(5, 'INACTIVE', 'Inactive', 2),
(5, 'ON_LEAVE', 'On Leave', 3),
(5, 'TERMINATED', 'Terminated', 4),
(5, 'SUSPENDED', 'Suspended', 5);

-- Education Levels
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(6, 'DIPLOMA', 'Diploma', 1),
(6, 'BACHELOR', 'Bachelor', 2),
(6, 'MASTER', 'Master', 3),
(6, 'PHD', 'PhD', 4),
(6, 'OTHER', 'Other', 5);

-- Marital Status
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(7, 'SINGLE', 'Single', 1),
(7, 'MARRIED', 'Married', 2),
(7, 'DIVORCED', 'Divorced', 3),
(7, 'WIDOWED', 'Widowed', 4),
(7, 'OTHER', 'Other', 5);

-- Gender
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(8, 'MALE', 'Male', 1),
(8, 'FEMALE', 'Female', 2);

-- Project Statuses
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(9, 'ACTIVE', 'Active', 1),
(9, 'COMPLETED', 'Completed', 2),
(9, 'ON_HOLD', 'On Hold', 3),
(9, 'CANCELLED', 'Cancelled', 4),
(9, 'PLANNING', 'Planning', 5);

-- Project Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(10, 'TRAINING', 'Training', 1),
(10, 'MAINTENANCE', 'Maintenance', 2),
(10, 'DEVELOPMENT', 'Development', 3),
(10, 'AUDIT', 'Audit', 4),
(10, 'RESEARCH', 'Research', 5);

-- Certificate Types
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(11, 'TRAINING_CERTIFICATE', 'Training Certificate', 1),
(11, 'PROFESSIONAL_LICENSE', 'Professional License', 2),
(11, 'MEDICAL_CERTIFICATE', 'Medical Certificate', 3),
(11, 'SAFETY_CERTIFICATE', 'Safety Certificate', 4),
(11, 'QUALITY_CERTIFICATE', 'Quality Certificate', 5);

-- Issuing Authorities
INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder) VALUES
(12, 'ICAO', 'ICAO', 1),
(12, 'LOCAL_AUTHORITY', 'Local Authority', 2),
(12, 'TRAINING_CENTER', 'Training Center', 3),
(12, 'INTERNATIONAL_ORGANIZATION', 'International Organization', 4),
(12, 'GOVERNMENT_AGENCY', 'Government Agency', 5);

-- =============================================
-- 4. إنشاء Indexes للأداء
-- =============================================

CREATE INDEX IX_ConfigurationValues_CategoryId ON ConfigurationValues(CategoryId);
CREATE INDEX IX_ConfigurationValues_IsActive ON ConfigurationValues(IsActive);
CREATE INDEX IX_ConfigurationValues_DisplayOrder ON ConfigurationValues(DisplayOrder);
CREATE INDEX IX_ConfigurationLog_ValueId ON ConfigurationLog(ValueId);
CREATE INDEX IX_ConfigurationLog_ChangedDate ON ConfigurationLog(ChangedDate);

-- =============================================
-- 5. إنشاء Stored Procedures
-- =============================================

-- إجراء لجلب قيم فئة معينة
CREATE PROCEDURE sp_GetConfigurationValues
    @CategoryName NVARCHAR(100)
AS
BEGIN
    SELECT 
        v.ValueId,
        v.ValueKey,
        v.ValueText,
        v.DisplayOrder,
        v.IsActive
    FROM ConfigurationValues v
    INNER JOIN ConfigurationCategories c ON v.CategoryId = c.CategoryId
    WHERE c.CategoryName = @CategoryName 
    AND v.IsActive = 1 
    AND c.IsActive = 1
    ORDER BY v.DisplayOrder, v.ValueText;
END
GO

-- إجراء لإضافة قيمة جديدة
CREATE PROCEDURE sp_AddConfigurationValue
    @CategoryName NVARCHAR(100),
    @ValueKey NVARCHAR(100),
    @ValueText NVARCHAR(200),
    @DisplayOrder INT = 0,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    DECLARE @CategoryId INT;
    
    SELECT @CategoryId = CategoryId 
    FROM ConfigurationCategories 
    WHERE CategoryName = @CategoryName;
    
    IF @CategoryId IS NOT NULL
    BEGIN
        INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, DisplayOrder, CreatedBy)
        VALUES (@CategoryId, @ValueKey, @ValueText, @DisplayOrder, @CreatedBy);
        
        -- تسجيل العملية
        INSERT INTO ConfigurationLog (ValueId, Action, NewValue, ChangedBy)
        VALUES (SCOPE_IDENTITY(), 'INSERT', @ValueText, @CreatedBy);
    END
END
GO

-- إجراء لتحديث قيمة
CREATE PROCEDURE sp_UpdateConfigurationValue
    @ValueId INT,
    @ValueText NVARCHAR(200),
    @DisplayOrder INT = NULL,
    @IsActive BIT = NULL,
    @ModifiedBy NVARCHAR(100) = NULL
AS
BEGIN
    DECLARE @OldValue NVARCHAR(200);
    
    SELECT @OldValue = ValueText 
    FROM ConfigurationValues 
    WHERE ValueId = @ValueId;
    
    UPDATE ConfigurationValues 
    SET ValueText = @ValueText,
        DisplayOrder = ISNULL(@DisplayOrder, DisplayOrder),
        IsActive = ISNULL(@IsActive, IsActive),
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE()
    WHERE ValueId = @ValueId;
    
    -- تسجيل العملية
    INSERT INTO ConfigurationLog (ValueId, Action, OldValue, NewValue, ChangedBy)
    VALUES (@ValueId, 'UPDATE', @OldValue, @ValueText, @ModifiedBy);
END
GO

-- إجراء لحذف قيمة (Soft Delete)
CREATE PROCEDURE sp_DeleteConfigurationValue
    @ValueId INT,
    @DeletedBy NVARCHAR(100) = NULL
AS
BEGIN
    DECLARE @OldValue NVARCHAR(200);
    
    SELECT @OldValue = ValueText 
    FROM ConfigurationValues 
    WHERE ValueId = @ValueId;
    
    UPDATE ConfigurationValues 
    SET IsActive = 0,
        ModifiedBy = @DeletedBy,
        ModifiedDate = GETDATE()
    WHERE ValueId = @ValueId;
    
    -- تسجيل العملية
    INSERT INTO ConfigurationLog (ValueId, Action, OldValue, ChangedBy)
    VALUES (@ValueId, 'DELETE', @OldValue, @DeletedBy);
END
GO

-- =============================================
-- 6. إنشاء Views
-- =============================================

-- View لجلب جميع القيم النشطة
CREATE VIEW vw_ActiveConfigurationValues AS
SELECT 
    c.CategoryName,
    c.DisplayName AS CategoryDisplayName,
    v.ValueKey,
    v.ValueText,
    v.DisplayOrder,
    v.CreatedDate,
    v.ModifiedDate
FROM ConfigurationValues v
INNER JOIN ConfigurationCategories c ON v.CategoryId = c.CategoryId
WHERE v.IsActive = 1 AND c.IsActive = 1;

-- =============================================
-- 7. إنشاء Triggers للتسجيل التلقائي
-- =============================================

CREATE TRIGGER tr_ConfigurationValues_Log
ON ConfigurationValues
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- تسجيل الإدراج
    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO ConfigurationLog (ValueId, Action, NewValue, ChangedBy)
        SELECT ValueId, 'INSERT', ValueText, CreatedBy
        FROM inserted;
    END
    
    -- تسجيل التحديث
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO ConfigurationLog (ValueId, Action, OldValue, NewValue, ChangedBy)
        SELECT 
            i.ValueId, 
            'UPDATE', 
            d.ValueText, 
            i.ValueText, 
            i.ModifiedBy
        FROM inserted i
        INNER JOIN deleted d ON i.ValueId = d.ValueId
        WHERE i.ValueText != d.ValueText OR i.IsActive != d.IsActive;
    END
END
GO

-- =============================================
-- 8. إنشاء Functions
-- =============================================

-- دالة لجلب قيم فئة معينة
CREATE FUNCTION fn_GetConfigurationValues
(
    @CategoryName NVARCHAR(100)
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        v.ValueKey,
        v.ValueText,
        v.DisplayOrder
    FROM ConfigurationValues v
    INNER JOIN ConfigurationCategories c ON v.CategoryId = c.CategoryId
    WHERE c.CategoryName = @CategoryName 
    AND v.IsActive = 1 
    AND c.IsActive = 1
);

GO

PRINT 'Configuration System Database Script completed successfully!'
PRINT 'Total Categories Created: ' + CAST((SELECT COUNT(*) FROM ConfigurationCategories) AS NVARCHAR(10))
PRINT 'Total Values Created: ' + CAST((SELECT COUNT(*) FROM ConfigurationValues) AS NVARCHAR(10)) 
