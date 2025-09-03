-- =====================================================
-- APPLY AUTO PERMISSIONS TO EXISTING USERS
-- =====================================================
-- This script applies the new auto-permission system to all existing users
-- who don't have organizational permissions yet

USE [HR-Aviation];
GO

PRINT '=== APPLYING AUTO PERMISSIONS TO EXISTING USERS ===';

-- Get all users who don't have organizational permissions
DECLARE @UsersWithoutOrgPermissions TABLE (
    UserId INT,
    Username NVARCHAR(50),
    RoleName NVARCHAR(50)
);

INSERT INTO @UsersWithoutOrgPermissions (UserId, Username, RoleName)
SELECT u.UserId, u.Username, u.RoleName
FROM users u
WHERE u.UserId NOT IN (
    SELECT DISTINCT UserId 
    FROM UserOrganizationalPermissions 
    WHERE IsActive = 1
);

-- Show users that will be processed
SELECT 'Users to process:' as Info, COUNT(*) as Count FROM @UsersWithoutOrgPermissions;

SELECT 'User Details:' as Info, UserId, Username, RoleName FROM @UsersWithoutOrgPermissions;

-- Apply auto permissions to each user
DECLARE @UserId INT, @Username NVARCHAR(50), @RoleName NVARCHAR(50);
DECLARE user_cursor CURSOR FOR 
SELECT UserId, Username, RoleName FROM @UsersWithoutOrgPermissions;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId, @Username, @RoleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Processing user: ' + @Username + ' (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ', Role: ' + @RoleName + ')';
    
    -- Call the stored procedure for each user
    EXEC AutoAssignPermissionsForNewUser @UserId, @RoleName;
    
    FETCH NEXT FROM user_cursor INTO @UserId, @Username, @RoleName;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT '=== VERIFICATION ===';

-- Check final results for all users
SELECT 
    u.UserId,
    u.Username,
    u.RoleName,
    ISNULL(menu_count.Count, 0) as MenuPermissions,
    ISNULL(op_count.Count, 0) as OperationPermissions,
    ISNULL(org_count.Count, 0) as OrganizationalPermissions
FROM users u
LEFT JOIN (
    SELECT UserId, COUNT(*) as Count 
    FROM UserMenuPermissions 
    WHERE IsActive = 1 
    GROUP BY UserId
) menu_count ON u.UserId = menu_count.UserId
LEFT JOIN (
    SELECT UserId, COUNT(*) as Count 
    FROM UserOperationPermissions 
    WHERE IsActive = 1 
    GROUP BY UserId
) op_count ON u.UserId = op_count.UserId
LEFT JOIN (
    SELECT UserId, COUNT(*) as Count 
    FROM UserOrganizationalPermissions 
    WHERE IsActive = 1 
    GROUP BY UserId
) org_count ON u.UserId = org_count.UserId
ORDER BY u.UserId;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'All existing users now have appropriate permissions based on their roles!';