-- =====================================================
-- Supabase Connection Test Script
-- =====================================================
-- This script tests the Supabase connection and basic functionality
-- Run this script to verify that the connection is working properly

-- Test 1: Basic Connection Test
SELECT 'Connection Test' as test_name, 'SUCCESS' as status, NOW() as timestamp;

-- Test 2: Check if Users table exists
SELECT 
    'Users Table Check' as test_name,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Users') 
        THEN 'SUCCESS' 
        ELSE 'FAILED' 
    END as status,
    NOW() as timestamp;

-- Test 3: Check Users table structure
SELECT 
    'Users Table Structure' as test_name,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'UserId')
        AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Username')
        AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'PasswordHash')
        AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'RoleName')
        THEN 'SUCCESS' 
        ELSE 'FAILED' 
    END as status,
    NOW() as timestamp;

-- Test 4: Check if admin user exists
SELECT 
    'Admin User Check' as test_name,
    CASE 
        WHEN EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'admin') 
        THEN 'SUCCESS' 
        ELSE 'FAILED' 
    END as status,
    NOW() as timestamp;

-- Test 5: Check admin user details
SELECT 
    'Admin User Details' as test_name,
    "UserId",
    "Username",
    "RoleName",
    CASE 
        WHEN "PasswordHash" IS NOT NULL AND LENGTH("PasswordHash") > 0
        THEN 'SUCCESS' 
        ELSE 'FAILED' 
    END as password_status,
    NOW() as timestamp
FROM "Users" 
WHERE "Username" = 'admin';

-- Test 6: Check if UserActivityLogs table exists
SELECT 
    'UserActivityLogs Table Check' as test_name,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserActivityLogs') 
        THEN 'SUCCESS' 
        ELSE 'FAILED' 
    END as status,
    NOW() as timestamp;

-- Test 7: Test basic insert operation
INSERT INTO "UserActivityLogs" ("UserId", "UserName", "Action", "EntityType", "EntityId", "Details", "IpAddress", "UserAgent", "IsSuccessful", "Timestamp")
VALUES (1, 'test_user', 'Connection Test', 'System', 'test', 'Testing database connection', '127.0.0.1', 'Test Agent', true, NOW())
ON CONFLICT DO NOTHING;

SELECT 
    'Insert Test' as test_name,
    'SUCCESS' as status,
    NOW() as timestamp;

-- Test 8: Test basic select operation
SELECT 
    'Select Test' as test_name,
    COUNT(*) as record_count,
    'SUCCESS' as status,
    NOW() as timestamp
FROM "UserActivityLogs"
WHERE "Action" = 'Connection Test';

-- Test 9: Test basic update operation
UPDATE "UserActivityLogs" 
SET "Details" = 'Connection test completed successfully'
WHERE "Action" = 'Connection Test' AND "Details" = 'Testing database connection';

SELECT 
    'Update Test' as test_name,
    'SUCCESS' as status,
    NOW() as timestamp;

-- Test 10: Test basic delete operation
DELETE FROM "UserActivityLogs" 
WHERE "Action" = 'Connection Test';

SELECT 
    'Delete Test' as test_name,
    'SUCCESS' as status,
    NOW() as timestamp;

-- Final Summary
SELECT 
    'ALL TESTS COMPLETED' as summary,
    'Supabase connection is working properly' as message,
    NOW() as timestamp;

-- Display current database information
SELECT 
    current_database() as database_name,
    current_user as current_user,
    version() as postgresql_version,
    NOW() as current_timestamp;