-- =====================================================
-- SIMPLE FIX FOR EXISTING USERS
-- =====================================================
-- This script simply applies organizational permissions to existing users
-- who don't have them yet

USE [HR-Aviation];
GO

PRINT '=== APPLYING ORGANIZATIONAL PERMISSIONS TO EXISTING USERS ===';

-- Get users who don't have organizational permissions
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

-- Apply organizational permissions to each user
DECLARE @UserId INT, @Username NVARCHAR(50), @RoleName NVARCHAR(50);
DECLARE user_cursor CURSOR FOR 
SELECT UserId, Username, RoleName FROM @UsersWithoutOrgPermissions;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId, @Username, @RoleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Processing user: ' + @Username + ' (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ', Role: ' + @RoleName + ')';
    
    -- Countries - All users can view, only Admin can edit/delete/create
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        @UserId, 
        'Country', 
        CountryId, 
        CountryName, 
        1,  -- All can view countries
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can edit
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can delete
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can create
        1, 
        GETDATE(), 
        GETDATE()
    FROM Countries
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions 
        WHERE UserId = @UserId AND PermissionType = 'Country' AND EntityId = Countries.CountryId
    );
    
    -- Airports - All users can view, only Admin can edit/delete/create
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        @UserId, 
        'Airport', 
        AirportId, 
        AirportName, 
        1,  -- All can view airports
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can edit
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can delete
        CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can create
        1, 
        GETDATE(), 
        GETDATE()
    FROM Airports
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions 
        WHERE UserId = @UserId AND PermissionType = 'Airport' AND EntityId = Airports.AirportId
    );
    
    PRINT 'Applied organizational permissions to user: ' + @Username;
    
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
    ISNULL(org_count.Count, 0) as OrganizationalPermissions
FROM users u
LEFT JOIN (
    SELECT UserId, COUNT(*) as Count 
    FROM UserOrganizationalPermissions 
    WHERE IsActive = 1 
    GROUP BY UserId
) org_count ON u.UserId = org_count.UserId
ORDER BY u.UserId;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'All existing users now have organizational permissions!';