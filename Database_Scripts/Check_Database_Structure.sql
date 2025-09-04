-- =====================================================
-- سكريبت للتحقق من هيكل قاعدة البيانات
-- Database Structure Check Script
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: التحقق من الجداول والحقول الموجودة في قاعدة البيانات
-- =====================================================

-- 1. عرض جميع الجداول
SELECT 
    table_name as "Table Name",
    table_type as "Type"
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- 2. التحقق من جدول Users
SELECT 
    column_name as "Column Name",
    data_type as "Data Type",
    is_nullable as "Nullable",
    column_default as "Default Value"
FROM information_schema.columns 
WHERE table_name = 'Users' 
AND table_schema = 'public'
ORDER BY ordinal_position;

-- 3. التحقق من الجداول المتعلقة بالصلاحيات
SELECT 
    table_name as "Permission Tables"
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND (table_name LIKE '%Permission%' OR table_name LIKE '%User%')
ORDER BY table_name;

-- 4. التحقق من وجود المستخدم admin
SELECT 
    "Username",
    "RoleName",
    "IsActive",
    "CreatedDate"
FROM "Users" 
WHERE "Username" = 'admin';

-- 5. عرض عدد المستخدمين
SELECT 
    COUNT(*) as "Total Users"
FROM "Users";

-- 6. عرض معلومات قاعدة البيانات
SELECT 
    current_database() as "Database Name",
    current_user as "Current User",
    version() as "PostgreSQL Version";