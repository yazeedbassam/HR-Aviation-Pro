-- =====================================================
-- Create Basic Tables for HR Aviation System
-- =====================================================
-- سكريبت لإنشاء الجداول الأساسية فقط
-- يجب تنفيذه في Supabase SQL Editor

-- إنشاء جدول المستخدمين
CREATE TABLE IF NOT EXISTS Users (
    UserId SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLoginDate TIMESTAMP,
    Role VARCHAR(50) DEFAULT 'User'
);

-- إنشاء جدول الصلاحيات
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId SERIAL PRIMARY KEY,
    PermissionName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    Category VARCHAR(50),
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات المستخدمين
CREATE TABLE IF NOT EXISTS UserPermissions (
    UserPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE CASCADE,
    PermissionId INTEGER REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    GrantedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, PermissionId)
);

-- إنشاء جدول الأقسام
CREATE TABLE IF NOT EXISTS Departments (
    DepartmentId SERIAL PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE
);

-- إنشاء جدول صلاحيات الأقسام
CREATE TABLE IF NOT EXISTS UserDepartmentPermissions (
    UserDepartmentPermissionId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE CASCADE,
    DepartmentId INTEGER REFERENCES Departments(DepartmentId) ON DELETE CASCADE,
    CanView BOOLEAN DEFAULT FALSE,
    CanEdit BOOLEAN DEFAULT FALSE,
    CanDelete BOOLEAN DEFAULT FALSE,
    CanCreate BOOLEAN DEFAULT FALSE,
    GrantedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, DepartmentId)
);

-- إنشاء جدول الموظفين
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId SERIAL PRIMARY KEY,
    EmployeeNumber VARCHAR(20) UNIQUE NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) GENERATED ALWAYS AS (FirstName || ' ' || LastName) STORED,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Department VARCHAR(100),
    Position VARCHAR(100),
    HireDate DATE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول المراقبين
CREATE TABLE IF NOT EXISTS Controllers (
    ControllerId SERIAL PRIMARY KEY,
    ControllerNumber VARCHAR(20) UNIQUE NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) GENERATED ALWAYS AS (FirstName || ' ' || LastName) STORED,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Department VARCHAR(100),
    Position VARCHAR(100),
    HireDate DATE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول الشهادات
CREATE TABLE IF NOT EXISTS Certificates (
    CertificateId SERIAL PRIMARY KEY,
    CertificateNumber VARCHAR(50) UNIQUE NOT NULL,
    CertificateTitle VARCHAR(200) NOT NULL,
    IssuingAuthority VARCHAR(200),
    IssuingCountry VARCHAR(100),
    IssueDate DATE,
    ExpiryDate DATE,
    CertificateType VARCHAR(100),
    IsValid BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول التراخيص
CREATE TABLE IF NOT EXISTS Licenses (
    LicenseId SERIAL PRIMARY KEY,
    LicenseNumber VARCHAR(50) UNIQUE NOT NULL,
    LicenseTitle VARCHAR(200) NOT NULL,
    IssuingAuthority VARCHAR(200),
    IssuingCountry VARCHAR(100),
    IssueDate DATE,
    ExpiryDate DATE,
    LicenseType VARCHAR(100),
    IsValid BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول المشاريع
CREATE TABLE IF NOT EXISTS Projects (
    ProjectId SERIAL PRIMARY KEY,
    ProjectName VARCHAR(200) NOT NULL,
    ProjectDescription TEXT,
    StartDate DATE,
    EndDate DATE,
    Status VARCHAR(50) DEFAULT 'Planning',
    ProjectManager VARCHAR(100),
    Budget DECIMAL(15,2),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول الملاحظات
CREATE TABLE IF NOT EXISTS Observations (
    ObservationId SERIAL PRIMARY KEY,
    ObservationTitle VARCHAR(200) NOT NULL,
    ObservationDescription TEXT,
    ObservationDate DATE,
    Observer VARCHAR(100),
    ObservedPerson VARCHAR(100),
    ObservationType VARCHAR(100),
    Status VARCHAR(50) DEFAULT 'Pending',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate TIMESTAMP
);

-- إنشاء جدول سجل الأنشطة
CREATE TABLE IF NOT EXISTS ActivityLog (
    LogId SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(UserId) ON DELETE SET NULL,
    Username VARCHAR(50),
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50),
    EntityId VARCHAR(50),
    Details TEXT,
    IpAddress INET,
    UserAgent TEXT,
    IsSuccessful BOOLEAN DEFAULT TRUE,
    ErrorMessage TEXT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- إنشاء فهارس لتحسين الأداء
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_active ON Users(IsActive);
CREATE INDEX IF NOT EXISTS idx_userpermissions_userid ON UserPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userpermissions_permissionid ON UserPermissions(PermissionId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_userid ON UserDepartmentPermissions(UserId);
CREATE INDEX IF NOT EXISTS idx_userdepartmentpermissions_departmentid ON UserDepartmentPermissions(DepartmentId);
CREATE INDEX IF NOT EXISTS idx_employees_employeenumber ON Employees(EmployeeNumber);
CREATE INDEX IF NOT EXISTS idx_employees_active ON Employees(IsActive);
CREATE INDEX IF NOT EXISTS idx_controllers_controllernumber ON Controllers(ControllerNumber);
CREATE INDEX IF NOT EXISTS idx_controllers_active ON Controllers(IsActive);
CREATE INDEX IF NOT EXISTS idx_certificates_certificatenumber ON Certificates(CertificateNumber);
CREATE INDEX IF NOT EXISTS idx_certificates_valid ON Certificates(IsValid);
CREATE INDEX IF NOT EXISTS idx_licenses_licensenumber ON Licenses(LicenseNumber);
CREATE INDEX IF NOT EXISTS idx_licenses_valid ON Licenses(IsValid);
CREATE INDEX IF NOT EXISTS idx_projects_name ON Projects(ProjectName);
CREATE INDEX IF NOT EXISTS idx_projects_active ON Projects(IsActive);
CREATE INDEX IF NOT EXISTS idx_observations_title ON Observations(ObservationTitle);
CREATE INDEX IF NOT EXISTS idx_observations_active ON Observations(IsActive);
CREATE INDEX IF NOT EXISTS idx_activitylog_userid ON ActivityLog(UserId);
CREATE INDEX IF NOT EXISTS idx_activitylog_username ON ActivityLog(Username);
CREATE INDEX IF NOT EXISTS idx_activitylog_action ON ActivityLog(Action);
CREATE INDEX IF NOT EXISTS idx_activitylog_createddate ON ActivityLog(CreatedDate);

-- عرض معلومات الجداول المُنشأة
SELECT 'Tables Created Successfully' as Status;

SELECT 
    'Created Tables' as Info,
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Users', 'Permissions', 'UserPermissions', 'Departments', 
                   'UserDepartmentPermissions', 'Employees', 'Controllers', 
                   'Certificates', 'Licenses', 'Projects', 'Observations', 'ActivityLog')
ORDER BY table_name;

-- عرض عدد الفهارس المُنشأة
SELECT 
    'Created Indexes' as Info,
    COUNT(*) as IndexCount
FROM pg_indexes 
WHERE schemaname = 'public' 
AND tablename IN ('Users', 'Permissions', 'UserPermissions', 'Departments', 
                  'UserDepartmentPermissions', 'Employees', 'Controllers', 
                  'Certificates', 'Licenses', 'Projects', 'Observations', 'ActivityLog');

-- رسالة النجاح
SELECT '✅ Basic tables created successfully!' as Message
UNION ALL
SELECT '📊 You can now run the admin setup script to create users and permissions'
UNION ALL
SELECT '🔧 Or run the simple admin setup script for quick admin user creation';