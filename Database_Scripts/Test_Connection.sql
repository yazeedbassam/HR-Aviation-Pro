-- =====================================================
-- Test Supabase Connection Script
-- =====================================================
-- سكريبت لاختبار الاتصال بقاعدة البيانات Supabase
-- يجب تنفيذه في Supabase SQL Editor

-- اختبار الاتصال الأساسي
SELECT 'Connection Test Started' as Status, NOW() as Timestamp;

-- اختبار إنشاء جدول مؤقت
CREATE TEMP TABLE IF NOT EXISTS test_table (
    id SERIAL PRIMARY KEY,
    test_data VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- إدراج بيانات اختبار
INSERT INTO test_table (test_data) VALUES ('Test Data 1'), ('Test Data 2'), ('Test Data 3');

-- اختبار القراءة
SELECT 'Data Insertion Test' as Test, COUNT(*) as RecordCount FROM test_table;

-- اختبار التحديث
UPDATE test_table SET test_data = 'Updated Test Data' WHERE id = 1;

-- اختبار الحذف
DELETE FROM test_table WHERE id = 3;

-- اختبار النتيجة النهائية
SELECT 'Final Test Results' as Test, COUNT(*) as RemainingRecords FROM test_table;

-- اختبار الصلاحيات
SELECT 'Permission Test' as Test, 
       current_user as CurrentUser,
       session_user as SessionUser,
       current_database() as CurrentDatabase,
       version() as PostgreSQLVersion;

-- اختبار الجداول الموجودة
SELECT 'Existing Tables' as Test, 
       table_name,
       table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- اختبار الوظائف المتاحة
SELECT 'Available Functions' as Test,
       routine_name,
       routine_type
FROM information_schema.routines 
WHERE routine_schema = 'public'
ORDER BY routine_name;

-- اختبار الفهارس
SELECT 'Available Indexes' as Test,
       indexname,
       tablename,
       indexdef
FROM pg_indexes 
WHERE schemaname = 'public'
ORDER BY tablename, indexname;

-- اختبار Views
SELECT 'Available Views' as Test,
       viewname,
       definition
FROM pg_views 
WHERE schemaname = 'public'
ORDER BY viewname;

-- اختبار الاتصال مع معلومات النظام
SELECT 'System Information' as Test,
       inet_server_addr() as ServerAddress,
       inet_server_port() as ServerPort,
       current_setting('server_version') as ServerVersion,
       current_setting('max_connections') as MaxConnections,
       current_setting('shared_buffers') as SharedBuffers;

-- اختبار الأداء
SELECT 'Performance Test' as Test,
       pg_database_size(current_database()) as DatabaseSize,
       (SELECT COUNT(*) FROM pg_stat_activity) as ActiveConnections,
       (SELECT COUNT(*) FROM pg_stat_activity WHERE state = 'active') as ActiveQueries;

-- تنظيف البيانات المؤقتة
DROP TABLE IF EXISTS test_table;

-- رسالة النجاح
SELECT '✅ Connection Test Completed Successfully!' as Status, NOW() as Timestamp;