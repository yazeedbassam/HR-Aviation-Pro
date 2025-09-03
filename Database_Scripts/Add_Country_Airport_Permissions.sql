-- Add Country and Airport permissions to the database
USE [HR-Aviation];

-- Add Country permissions
INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt, UpdatedAt)
VALUES 
    ('COUNTRY_VIEW', 'Country View', 'View Countries', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('COUNTRY_ADD', 'Country Add', 'Add New Country', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('COUNTRY_EDIT', 'Country Edit', 'Edit Country', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('COUNTRY_DELETE', 'Country Delete', 'Delete Country', 'Organization Management', 1, GETDATE(), GETDATE());

-- Add Airport permissions
INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt, UpdatedAt)
VALUES 
    ('AIRPORT_VIEW', 'Airport View', 'View Airports', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('AIRPORT_ADD', 'Airport Add', 'Add New Airport', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('AIRPORT_EDIT', 'Airport Edit', 'Edit Airport', 'Organization Management', 1, GETDATE(), GETDATE()),
    ('AIRPORT_DELETE', 'Airport Delete', 'Delete Airport', 'Organization Management', 1, GETDATE(), GETDATE());

-- Check the results
SELECT PermissionId, PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive
FROM Permissions 
WHERE PermissionKey LIKE 'COUNTRY_%' OR PermissionKey LIKE 'AIRPORT_%'
ORDER BY PermissionKey;