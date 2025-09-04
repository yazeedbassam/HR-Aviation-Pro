-- =====================================================
-- Supabase Fix Admin Password - إصلاح كلمة مرور Admin
-- =====================================================

-- تحديث كلمة مرور مستخدم admin لتطابق التطبيق
UPDATE "Users" 
SET "PasswordHash" = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'
WHERE "Username" = 'admin';

-- التحقق من التحديث
SELECT 'Admin User Updated' as Status, "Username", "RoleName", "PasswordHash" 
FROM "Users" 
WHERE "Username" = 'admin';

-- رسالة النجاح
DO $$
BEGIN
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Admin Password Fixed Successfully!';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Username: admin';
    RAISE NOTICE 'Password: admin123';
    RAISE NOTICE 'PasswordHash updated to match application';
    RAISE NOTICE '=====================================================';
END $$;