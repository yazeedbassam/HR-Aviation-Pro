-- Clean up duplicate Country and Airport permissions
USE [HR-Aviation];

-- Delete old duplicate permissions (keep the new ones with Organization Management category)
DELETE FROM Permissions 
WHERE PermissionKey IN ('COUNTRY_VIEW', 'COUNTRY_ADD', 'COUNTRY_EDIT', 'COUNTRY_DELETE', 'AIRPORT_VIEW', 'AIRPORT_ADD', 'AIRPORT_EDIT', 'AIRPORT_DELETE')
AND CategoryName = 'Operations';

-- Check the results
SELECT PermissionId, PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive
FROM Permissions 
WHERE PermissionKey LIKE 'COUNTRY_%' OR PermissionKey LIKE 'AIRPORT_%'
ORDER BY PermissionKey;