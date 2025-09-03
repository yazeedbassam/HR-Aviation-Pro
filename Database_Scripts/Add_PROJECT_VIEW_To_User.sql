-- Add PROJECT_VIEW permission to user yazeed.bassam
USE [HR-Aviation];

-- Get user ID
DECLARE @UserId INT;
SELECT @UserId = UserId FROM Users WHERE Username = 'yazeed.bassam';

-- Check if PROJECT_VIEW menu permission already exists for user
IF NOT EXISTS (SELECT 1 FROM UserMenuPermissions WHERE UserId = @UserId AND MenuKey = 'PROJECT_VIEW')
BEGIN
    -- Insert PROJECT_VIEW menu permission for user
    INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, CreatedAt, UpdatedAt)
    VALUES (@UserId, 'PROJECT_VIEW', 1, GETDATE(), GETDATE());
    
    PRINT 'PROJECT_VIEW menu permission added to user yazeed.bassam successfully';
END
ELSE
BEGIN
    -- Update existing menu permission to visible
    UPDATE UserMenuPermissions 
    SET IsVisible = 1, UpdatedAt = GETDATE()
    WHERE UserId = @UserId AND MenuKey = 'PROJECT_VIEW';
    
    PRINT 'PROJECT_VIEW menu permission updated for user yazeed.bassam';
END

-- Check the result
SELECT ump.UserId, u.Username, ump.MenuKey, ump.IsVisible, ump.CreatedAt
FROM UserMenuPermissions ump
INNER JOIN Users u ON ump.UserId = u.UserId
WHERE u.Username = 'yazeed.bassam' AND ump.MenuKey = 'PROJECT_VIEW';