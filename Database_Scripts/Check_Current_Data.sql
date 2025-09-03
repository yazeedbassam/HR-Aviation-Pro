-- Check current data in the system
USE [HR-Aviation]
GO

-- Check users table
PRINT '=== USERS TABLE ==='
SELECT COUNT(*) as TotalUsers FROM users
SELECT TOP 10 userid, username, rolename FROM users ORDER BY userid

-- Check employees table (if exists)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[employees]') AND type in (N'U'))
BEGIN
    PRINT '=== EMPLOYEES TABLE ==='
    SELECT COUNT(*) as TotalEmployees FROM employees
    SELECT TOP 10 * FROM employees ORDER BY employeeid
END
ELSE
BEGIN
    PRINT '=== EMPLOYEES TABLE DOES NOT EXIST ==='
END

-- Check controllers table (if exists)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[controllers]') AND type in (N'U'))
BEGIN
    PRINT '=== CONTROLLERS TABLE ==='
    SELECT COUNT(*) as TotalControllers FROM controllers
    SELECT TOP 10 * FROM controllers ORDER BY controllerid
END
ELSE
BEGIN
    PRINT '=== CONTROLLERS TABLE DOES NOT EXIST ==='
END

-- Check if there are users with roles that don't match existing data
PRINT '=== USERS BY ROLE ==='
SELECT rolename, COUNT(*) as Count 
FROM users 
GROUP BY rolename 
ORDER BY Count DESC

-- Check for orphaned users (users without corresponding employee/controller records)
PRINT '=== ORPHANED USERS ANALYSIS ==='
SELECT 
    u.userid,
    u.username,
    u.rolename,
    CASE 
        WHEN u.rolename = 'Employee' AND NOT EXISTS (SELECT 1 FROM employees e WHERE e.userid = u.userid) 
        THEN 'Employee without employee record'
        WHEN u.rolename = 'Controller' AND NOT EXISTS (SELECT 1 FROM controllers c WHERE c.userid = u.userid) 
        THEN 'Controller without controller record'
        ELSE 'OK'
    END as Status
FROM users u
WHERE u.rolename IN ('Employee', 'Controller')
ORDER BY u.rolename, u.userid
GO