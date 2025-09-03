-- Add PROJECT_VIEW permission to the database
USE [HR-Aviation];

-- Check if PROJECT_VIEW permission already exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'PROJECT_VIEW')
BEGIN
    -- Insert PROJECT_VIEW permission
    INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('PROJECT_VIEW', 'Project View', 'View Training & Development Projects', 'Training & Development', 1, GETDATE(), GETDATE());
    
    PRINT 'PROJECT_VIEW permission added successfully';
END
ELSE
BEGIN
    PRINT 'PROJECT_VIEW permission already exists';
END

-- Check the result
SELECT PermissionId, PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt
FROM Permissions 
WHERE PermissionKey = 'PROJECT_VIEW';