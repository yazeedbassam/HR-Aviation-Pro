-- =============================================
-- Create UserActivityLogs Table
-- جدول لتتبع أنشطة المستخدمين في النظام
-- =============================================

-- Check if table exists and drop it
IF OBJECT_ID('UserActivityLogs', 'U') IS NOT NULL
    DROP TABLE UserActivityLogs;
GO

-- Create UserActivityLogs table
CREATE TABLE UserActivityLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Create, Update, Delete, Login, Logout, etc.
    EntityType NVARCHAR(50) NOT NULL, -- Employee, Controller, License, Certificate, etc.
    EntityId NVARCHAR(50) NULL, -- ID of the affected entity
    Details NVARCHAR(MAX) NULL, -- Additional details about the action
    IpAddress NVARCHAR(45) NULL, -- IPv4 or IPv6 address
    UserAgent NVARCHAR(500) NULL, -- Browser/Client information
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    IsSuccessful BIT DEFAULT 1, -- Whether the action was successful
    ErrorMessage NVARCHAR(MAX) NULL -- Error message if action failed
);
GO

-- Create indexes for better performance
CREATE INDEX IX_UserActivityLogs_UserId ON UserActivityLogs(UserId);
CREATE INDEX IX_UserActivityLogs_Timestamp ON UserActivityLogs(Timestamp);
CREATE INDEX IX_UserActivityLogs_Action ON UserActivityLogs(Action);
CREATE INDEX IX_UserActivityLogs_EntityType ON UserActivityLogs(EntityType);
CREATE INDEX IX_UserActivityLogs_EntityId ON UserActivityLogs(EntityId);
GO

-- Create a view for easier querying
CREATE VIEW vw_UserActivityLogs AS
SELECT 
    Id,
    UserId,
    UserName,
    Action,
    EntityType,
    EntityId,
    Details,
    IpAddress,
    UserAgent,
    Timestamp,
    IsSuccessful,
    ErrorMessage,
    -- Add computed columns for better readability
    CASE 
        WHEN Action = 'Create' THEN 'إنشاء'
        WHEN Action = 'Update' THEN 'تحديث'
        WHEN Action = 'Delete' THEN 'حذف'
        WHEN Action = 'Login' THEN 'تسجيل دخول'
        WHEN Action = 'Logout' THEN 'تسجيل خروج'
        WHEN Action = 'View' THEN 'عرض'
        WHEN Action = 'Export' THEN 'تصدير'
        WHEN Action = 'Import' THEN 'استيراد'
        ELSE Action
    END AS ActionArabic,
    CASE 
        WHEN EntityType = 'Employee' THEN 'موظف'
        WHEN EntityType = 'Controller' THEN 'مراقب'
        WHEN EntityType = 'License' THEN 'رخصة'
        WHEN EntityType = 'Certificate' THEN 'شهادة'
        WHEN EntityType = 'Observation' THEN 'ملاحظة'
        WHEN EntityType = 'Project' THEN 'مشروع'
        WHEN EntityType = 'Airport' THEN 'مطار'
        WHEN EntityType = 'Country' THEN 'دولة'
        ELSE EntityType
    END AS EntityTypeArabic
FROM UserActivityLogs;
GO

-- Insert sample data for testing
INSERT INTO UserActivityLogs (UserId, UserName, Action, EntityType, EntityId, Details, IpAddress, UserAgent)
VALUES 
(1, 'admin', 'Login', 'System', NULL, 'User logged in successfully', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'),
(1, 'admin', 'Create', 'Employee', '1001', 'Created new employee: John Doe', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'),
(1, 'admin', 'Update', 'License', '2001', 'Updated license expiry date for Controller ID: 150', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36');
GO

-- Create stored procedure for inserting logs
CREATE PROCEDURE sp_InsertUserActivityLog
    @UserId INT,
    @UserName NVARCHAR(100),
    @Action NVARCHAR(50),
    @EntityType NVARCHAR(50),
    @EntityId NVARCHAR(50) = NULL,
    @Details NVARCHAR(MAX) = NULL,
    @IpAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @IsSuccessful BIT = 1,
    @ErrorMessage NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO UserActivityLogs (
        UserId, UserName, Action, EntityType, EntityId, 
        Details, IpAddress, UserAgent, IsSuccessful, ErrorMessage
    )
    VALUES (
        @UserId, @UserName, @Action, @EntityType, @EntityId,
        @Details, @IpAddress, @UserAgent, @IsSuccessful, @ErrorMessage
    );
    
    -- Return the ID of the inserted log
    SELECT SCOPE_IDENTITY() AS LogId;
END
GO

-- Create stored procedure for querying logs with filters
CREATE PROCEDURE sp_GetUserActivityLogs
    @UserId INT = NULL,
    @Action NVARCHAR(50) = NULL,
    @EntityType NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageSize INT = 50,
    @PageNumber INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM UserActivityLogs
    WHERE (@UserId IS NULL OR UserId = @UserId)
        AND (@Action IS NULL OR Action = @Action)
        AND (@EntityType IS NULL OR EntityType = @EntityType)
        AND (@StartDate IS NULL OR Timestamp >= @StartDate)
        AND (@EndDate IS NULL OR Timestamp <= @EndDate);
    
    -- Get paginated results
    SELECT 
        Id, UserId, UserName, Action, EntityType, EntityId,
        Details, IpAddress, UserAgent, Timestamp, IsSuccessful, ErrorMessage
    FROM UserActivityLogs
    WHERE (@UserId IS NULL OR UserId = @UserId)
        AND (@Action IS NULL OR Action = @Action)
        AND (@EntityType IS NULL OR EntityType = @EntityType)
        AND (@StartDate IS NULL OR Timestamp >= @StartDate)
        AND (@EndDate IS NULL OR Timestamp <= @EndDate)
    ORDER BY Timestamp DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Create function to get user activity summary
CREATE FUNCTION fn_GetUserActivitySummary(@UserId INT, @Days INT = 30)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        Action,
        EntityType,
        COUNT(*) AS ActionCount,
        MAX(Timestamp) AS LastAction
    FROM UserActivityLogs
    WHERE UserId = @UserId
        AND Timestamp >= DATEADD(day, -@Days, GETUTCDATE())
    GROUP BY Action, EntityType
);
GO

PRINT 'UserActivityLogs table and related objects created successfully!';
PRINT 'Sample data inserted for testing.';
PRINT 'Stored procedures and functions created for easy logging and querying.';
GO 