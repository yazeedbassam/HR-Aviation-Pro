-- Add missing observation permissions to the database
-- This script ensures all observation permissions exist with correct keys

USE [HR-Aviation];
GO

PRINT '=== Adding Missing Observation Permissions ===';
PRINT '';

-- Add ControllerObservation permissions
PRINT 'Adding ControllerObservation permissions...';

-- ControllerObservation View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLEROBSERVATION_VIEW')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('CONTROLLEROBSERVATION_VIEW', 'Controller Observation View', 'Permission to view controller observations', 'Controller Observation Management', 1, GETDATE());
    PRINT 'Added CONTROLLEROBSERVATION_VIEW permission';
END
ELSE
    PRINT 'CONTROLLEROBSERVATION_VIEW permission already exists';

-- ControllerObservation Add
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLEROBSERVATION_ADD')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('CONTROLLEROBSERVATION_ADD', 'Controller Observation Add', 'Permission to add controller observations', 'Controller Observation Management', 1, GETDATE());
    PRINT 'Added CONTROLLEROBSERVATION_ADD permission';
END
ELSE
    PRINT 'CONTROLLEROBSERVATION_ADD permission already exists';

-- ControllerObservation Edit
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLEROBSERVATION_EDIT')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('CONTROLLEROBSERVATION_EDIT', 'Controller Observation Edit', 'Permission to edit controller observations', 'Controller Observation Management', 1, GETDATE());
    PRINT 'Added CONTROLLEROBSERVATION_EDIT permission';
END
ELSE
    PRINT 'CONTROLLEROBSERVATION_EDIT permission already exists';

-- ControllerObservation Delete
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLEROBSERVATION_DELETE')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('CONTROLLEROBSERVATION_DELETE', 'Controller Observation Delete', 'Permission to delete controller observations', 'Controller Observation Management', 1, GETDATE());
    PRINT 'Added CONTROLLEROBSERVATION_DELETE permission';
END
ELSE
    PRINT 'CONTROLLEROBSERVATION_DELETE permission already exists';

-- ControllerObservation Export
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLEROBSERVATION_EXPORT')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('CONTROLLEROBSERVATION_EXPORT', 'Controller Observation Export', 'Permission to export controller observations', 'Controller Observation Management', 1, GETDATE());
    PRINT 'Added CONTROLLEROBSERVATION_EXPORT permission';
END
ELSE
    PRINT 'CONTROLLEROBSERVATION_EXPORT permission already exists';

PRINT '';

-- Add EmployeeObservation permissions
PRINT 'Adding EmployeeObservation permissions...';

-- EmployeeObservation View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'EMPLOYEEOBSERVATION_VIEW')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('EMPLOYEEOBSERVATION_VIEW', 'Employee Observation View', 'Permission to view employee observations', 'Employee Observation Management', 1, GETDATE());
    PRINT 'Added EMPLOYEEOBSERVATION_VIEW permission';
END
ELSE
    PRINT 'EMPLOYEEOBSERVATION_VIEW permission already exists';

-- EmployeeObservation Add
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'EMPLOYEEOBSERVATION_ADD')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('EMPLOYEEOBSERVATION_ADD', 'Employee Observation Add', 'Permission to add employee observations', 'Employee Observation Management', 1, GETDATE());
    PRINT 'Added EMPLOYEEOBSERVATION_ADD permission';
END
ELSE
    PRINT 'EMPLOYEEOBSERVATION_ADD permission already exists';

-- EmployeeObservation Edit
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'EMPLOYEEOBSERVATION_EDIT')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('EMPLOYEEOBSERVATION_EDIT', 'Employee Observation Edit', 'Permission to edit employee observations', 'Employee Observation Management', 1, GETDATE());
    PRINT 'Added EMPLOYEEOBSERVATION_EDIT permission';
END
ELSE
    PRINT 'EMPLOYEEOBSERVATION_EDIT permission already exists';

-- EmployeeObservation Delete
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'EMPLOYEEOBSERVATION_DELETE')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('EMPLOYEEOBSERVATION_DELETE', 'Employee Observation Delete', 'Permission to delete employee observations', 'Employee Observation Management', 1, GETDATE());
    PRINT 'Added EMPLOYEEOBSERVATION_DELETE permission';
END
ELSE
    PRINT 'EMPLOYEEOBSERVATION_DELETE permission already exists';

-- EmployeeObservation Export
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'EMPLOYEEOBSERVATION_EXPORT')
BEGIN
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('EMPLOYEEOBSERVATION_EXPORT', 'Employee Observation Export', 'Permission to export employee observations', 'Employee Observation Management', 1, GETDATE());
    PRINT 'Added EMPLOYEEOBSERVATION_EXPORT permission';
END
ELSE
    PRINT 'EMPLOYEEOBSERVATION_EXPORT permission already exists';

PRINT '';

-- Show all observation permissions
PRINT 'All observation permissions in the database:';
SELECT 
    PermissionId,
    PermissionKey,
    PermissionName,
    CategoryName,
    IsActive
FROM Permissions 
WHERE PermissionKey LIKE '%OBSERVATION%'
ORDER BY PermissionKey;

PRINT '';
PRINT '=== Script completed ===';