-- Clear Cache for yazeed.bassam - Dynamic Cache Invalidation
USE [HR-Aviation];

-- Get user ID for yazeed.bassam
DECLARE @UserId INT;
SELECT @UserId = UserId FROM Users WHERE Username = 'yazeed.bassam';

IF @UserId IS NULL
BEGIN
    PRINT 'User yazeed.bassam not found!';
    RETURN;
END

PRINT 'Clearing cache for user: yazeed.bassam (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ')';

-- Update user's last permission update timestamp to force cache invalidation
UPDATE Users 
SET LastPermissionUpdate = GETDATE() 
WHERE UserId = @UserId;

-- Insert a cache invalidation record
IF EXISTS (SELECT 1 FROM CacheInvalidationLog WHERE UserId = @UserId)
BEGIN
    UPDATE CacheInvalidationLog 
    SET InvalidatedAt = GETDATE(), 
        Reason = 'Manual cache clear for Country/Airport permissions'
    WHERE UserId = @UserId;
END
ELSE
BEGIN
    INSERT INTO CacheInvalidationLog (UserId, InvalidatedAt, Reason)
    VALUES (@UserId, GETDATE(), 'Manual cache clear for Country/Airport permissions');
END

PRINT 'Cache has been invalidated for yazeed.bassam';
PRINT 'The application will now use fresh permission data on next request.'; 