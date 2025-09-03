-- Cleanup duplicate observation permissions for user 1057
-- This script removes duplicate entries and keeps only the latest ones

USE [HR-Aviation];
GO

PRINT '=== Cleaning up duplicate observation permissions ===';
PRINT '';

-- Show current duplicates
PRINT 'Current duplicate permissions for user 1057:';
SELECT 
    EntityType,
    OperationType,
    COUNT(*) as DuplicateCount,
    SUM(CASE WHEN IsAllowed = 1 THEN 1 ELSE 0 END) as AllowedCount,
    SUM(CASE WHEN IsAllowed = 0 THEN 1 ELSE 0 END) as DeniedCount
FROM UserOperationPermissions 
WHERE UserId = 1057 
    AND (EntityType LIKE '%Observation%')
GROUP BY EntityType, OperationType
HAVING COUNT(*) > 1
ORDER BY EntityType, OperationType;
PRINT '';

-- Delete duplicates, keeping only the latest entry for each EntityType + OperationType combination
PRINT 'Removing duplicate entries...';

WITH DuplicatePermissions AS (
    SELECT 
        UserOperationPermissionId,
        ROW_NUMBER() OVER (
            PARTITION BY UserId, EntityType, OperationType 
            ORDER BY UserOperationPermissionId DESC
        ) as RowNum
    FROM UserOperationPermissions 
    WHERE UserId = 1057 
        AND (EntityType LIKE '%Observation%')
)
DELETE FROM UserOperationPermissions 
WHERE UserOperationPermissionId IN (
    SELECT UserOperationPermissionId 
    FROM DuplicatePermissions 
    WHERE RowNum > 1
);

PRINT 'Duplicate entries removed.';
PRINT '';

-- Show remaining permissions
PRINT 'Remaining observation permissions for user 1057:';
SELECT 
    uop.UserOperationPermissionId,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    p.PermissionKey
FROM UserOperationPermissions uop
LEFT JOIN Permissions p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = 1057 
    AND (uop.EntityType LIKE '%Observation%')
ORDER BY uop.EntityType, uop.OperationType;
PRINT '';

-- Test the stored procedure again
PRINT 'Testing stored procedure after cleanup:';
PRINT 'Testing ControllerObservation permissions:';

DECLARE @result1 BIT = 0;
EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'ControllerObservation', @OperationType = 'View', @Scope = 'All', @ScopeId = NULL;
PRINT 'ControllerObservation.View: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'ControllerObservation', @OperationType = 'Add', @Scope = 'All', @ScopeId = NULL;
PRINT 'ControllerObservation.Add: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'ControllerObservation', @OperationType = 'Edit', @Scope = 'All', @ScopeId = NULL;
PRINT 'ControllerObservation.Edit: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'ControllerObservation', @OperationType = 'Delete', @Scope = 'All', @ScopeId = NULL;
PRINT 'ControllerObservation.Delete: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'ControllerObservation', @OperationType = 'Export', @Scope = 'All', @ScopeId = NULL;
PRINT 'ControllerObservation.Export: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;
PRINT '';

PRINT 'Testing EmployeeObservation permissions:';

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'EmployeeObservation', @OperationType = 'View', @Scope = 'All', @ScopeId = NULL;
PRINT 'EmployeeObservation.View: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'EmployeeObservation', @OperationType = 'Add', @Scope = 'All', @ScopeId = NULL;
PRINT 'EmployeeObservation.Add: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'EmployeeObservation', @OperationType = 'Edit', @Scope = 'All', @ScopeId = NULL;
PRINT 'EmployeeObservation.Edit: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'EmployeeObservation', @OperationType = 'Delete', @Scope = 'All', @ScopeId = NULL;
PRINT 'EmployeeObservation.Delete: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;

EXEC @result1 = CanUserPerformOperation @UserId = 1057, @EntityType = 'EmployeeObservation', @OperationType = 'Export', @Scope = 'All', @ScopeId = NULL;
PRINT 'EmployeeObservation.Export: ' + CASE WHEN @result1 = 1 THEN 'ALLOWED' ELSE 'DENIED' END;
PRINT '';

PRINT '=== Cleanup completed ===';