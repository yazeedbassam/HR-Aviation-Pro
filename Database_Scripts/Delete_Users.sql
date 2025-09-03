-- Delete users from the system
USE [HR-Aviation]
GO

-- ⚠️ WARNING: This will permanently delete users and their data!
-- Make sure to backup your data before running this script

-- First, let's see what users exist
PRINT '=== CURRENT USERS ==='
SELECT userid, username, rolename FROM users ORDER BY userid

-- Delete specific user by ID (replace 1012 with the actual user ID)
-- Example: Delete user with ID 1012
/*
DELETE FROM UserMenuPermissions WHERE UserId = 1012;
DELETE FROM UserOperationPermissions WHERE UserId = 1012;
DELETE FROM UserOrganizationalPermissions WHERE UserId = 1012;
DELETE FROM users WHERE userid = 1012;
PRINT 'User 1012 deleted successfully';
*/

-- Delete specific user by username (replace 'username' with actual username)
-- Example: Delete user with username 'testuser'
/*
DECLARE @UserIdToDelete INT;
SELECT @UserIdToDelete = userid FROM users WHERE username = 'testuser';

IF @UserIdToDelete IS NOT NULL
BEGIN
    DELETE FROM UserMenuPermissions WHERE UserId = @UserIdToDelete;
    DELETE FROM UserOperationPermissions WHERE UserId = @UserIdToDelete;
    DELETE FROM UserOrganizationalPermissions WHERE UserId = @UserIdToDelete;
    DELETE FROM users WHERE userid = @UserIdToDelete;
    PRINT 'User testuser deleted successfully';
END
ELSE
BEGIN
    PRINT 'User testuser not found';
END
*/

-- Delete multiple users by role (replace 'Employee' with desired role)
-- Example: Delete all users with role 'Employee'
/*
DECLARE @RoleToDelete NVARCHAR(50) = 'Employee';
DECLARE @UserId INT;

DECLARE user_cursor CURSOR FOR
SELECT userid FROM users WHERE rolename = @RoleToDelete;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DELETE FROM UserMenuPermissions WHERE UserId = @UserId;
    DELETE FROM UserOperationPermissions WHERE UserId = @UserId;
    DELETE FROM UserOrganizationalPermissions WHERE UserId = @UserId;
    DELETE FROM users WHERE userid = @UserId;
    
    PRINT 'User ' + CAST(@UserId AS NVARCHAR(10)) + ' deleted successfully';
    
    FETCH NEXT FROM user_cursor INTO @UserId;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;
*/

-- Delete all users except Admin (be very careful with this!)
/*
DECLARE @UserId INT;

DECLARE user_cursor CURSOR FOR
SELECT userid FROM users WHERE rolename != 'Admin';

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DELETE FROM UserMenuPermissions WHERE UserId = @UserId;
    DELETE FROM UserOperationPermissions WHERE UserId = @UserId;
    DELETE FROM UserOrganizationalPermissions WHERE UserId = @UserId;
    DELETE FROM users WHERE userid = @UserId;
    
    PRINT 'User ' + CAST(@UserId AS NVARCHAR(10)) + ' deleted successfully';
    
    FETCH NEXT FROM user_cursor INTO @UserId;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT 'All non-admin users deleted successfully';
*/

-- Check remaining users after deletion
PRINT '=== REMAINING USERS ==='
SELECT userid, username, rolename FROM users ORDER BY userid

GO