-- Quick Fix for All Users - Complete Solution
USE [HR-Aviation];

-- Step 1: Add missing columns to Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastPermissionUpdate')
BEGIN
    ALTER TABLE Users ADD LastPermissionUpdate datetime NULL;
    PRINT 'Added LastPermissionUpdate column';
END

-- Step 2: Fix yazeed.bassam permissions
DECLARE @UserId INT = (SELECT UserId FROM Users WHERE Username = 'yazeed.bassam');

IF @UserId IS NOT NULL
BEGIN
    -- Clear existing permissions
    DELETE FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');
    
    -- Add Country permissions
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
    VALUES 
        (@UserId, 1, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Export', 1, 'All', GETDATE(), GETDATE());
    
    -- Add Airport permissions
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
    VALUES 
        (@UserId, 1, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Export', 1, 'All', GETDATE(), GETDATE());
    
    -- Update timestamp
    UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;
    
    PRINT 'Fixed permissions for yazeed.bassam';
END
ELSE
BEGIN
    PRINT 'User yazeed.bassam not found';
END

-- Step 3: Create simple stored procedure for future use
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'FixUserPermissions')
    DROP PROCEDURE FixUserPermissions
GO

CREATE PROCEDURE FixUserPermissions
    @Username NVARCHAR(50)
AS
BEGIN
    DECLARE @UserId INT = (SELECT UserId FROM Users WHERE Username = @Username);
    
    IF @UserId IS NULL
    BEGIN
        PRINT 'User not found: ' + @Username;
        RETURN;
    END
    
    -- Clear and add permissions
    DELETE FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');
    
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
    VALUES 
        (@UserId, 1, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Export', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Export', 1, 'All', GETDATE(), GETDATE());
    
    UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;
    PRINT 'Fixed permissions for: ' + @Username;
END
GO

PRINT 'SUCCESS: All fixes applied successfully!';
PRINT 'To fix any user in the future, run: EXEC FixUserPermissions ''username'''; 