-- =====================================================
-- CHECK REQUIRED TABLES FOR PERMISSIONS FIX
-- =====================================================
-- This script checks if all required tables exist before running the fix scripts

PRINT 'Checking required tables for permissions fix...'

-- Check if UserMenuPermissions table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserMenuPermissions]') AND type in (N'U'))
BEGIN
    PRINT '✓ UserMenuPermissions table exists'
END
ELSE
BEGIN
    PRINT '✗ UserMenuPermissions table does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if UserDepartmentPermissions table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDepartmentPermissions]') AND type in (N'U'))
BEGIN
    PRINT '✓ UserDepartmentPermissions table exists'
END
ELSE
BEGIN
    PRINT '✗ UserDepartmentPermissions table does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if UserOperationPermissions table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationPermissions]') AND type in (N'U'))
BEGIN
    PRINT '✓ UserOperationPermissions table exists'
END
ELSE
BEGIN
    PRINT '✗ UserOperationPermissions table does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if Permissions table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    PRINT '✓ Permissions table exists'
END
ELSE
BEGIN
    PRINT '✗ Permissions table does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if users table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type in (N'U'))
BEGIN
    PRINT '✓ users table exists'
END
ELSE
BEGIN
    PRINT '✗ users table does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if CanUserViewMenu procedure exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
BEGIN
    PRINT '✓ CanUserViewMenu procedure exists'
END
ELSE
BEGIN
    PRINT '✗ CanUserViewMenu procedure does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

-- Check if CheckUserPermission procedure exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckUserPermission]') AND type in (N'P', N'PC'))
BEGIN
    PRINT '✓ CheckUserPermission procedure exists'
END
ELSE
BEGIN
    PRINT '✗ CheckUserPermission procedure does NOT exist - Please run Advanced_Permission_System_New.sql first'
END

PRINT 'Table check completed!'