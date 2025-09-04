-- =====================================================
-- سكريبت منح جميع الصلاحيات للمستخدم admin
-- Grant All Permissions to Admin User Script
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: منح جميع الصلاحيات المطلوبة للمستخدم admin في قاعدة البيانات
-- =====================================================

-- التحقق من وجود المستخدم admin
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM controller_users WHERE username = 'admin') THEN
        RAISE NOTICE 'User admin does not exist. Creating admin user...';
        
        -- إنشاء المستخدم admin إذا لم يكن موجوداً
        INSERT INTO controller_users (username, password_hash, role, is_active, created_at, updated_at)
        VALUES ('admin', '123', 'Admin', true, NOW(), NOW());
        
        RAISE NOTICE 'Admin user created successfully.';
    ELSE
        RAISE NOTICE 'Admin user already exists.';
    END IF;
END $$;

-- الحصول على ID المستخدم admin
DO $$
DECLARE
    admin_user_id INTEGER;
BEGIN
    -- الحصول على ID المستخدم admin
    SELECT id INTO admin_user_id FROM controller_users WHERE username = 'admin';
    
    IF admin_user_id IS NULL THEN
        RAISE EXCEPTION 'Admin user not found!';
    END IF;
    
    RAISE NOTICE 'Admin user ID: %', admin_user_id;
    
    -- =====================================================
    -- 1. منح صلاحيات إدارة المستخدمين
    -- =====================================================
    
    -- حذف الصلاحيات الموجودة للمستخدم admin
    DELETE FROM user_permissions WHERE user_id = admin_user_id;
    
    -- منح جميع الصلاحيات المتاحة
    INSERT INTO user_permissions (user_id, permission_key, is_granted, created_at, updated_at)
    SELECT 
        admin_user_id,
        p.permission_key,
        true,
        NOW(),
        NOW()
    FROM permissions p
    WHERE NOT EXISTS (
        SELECT 1 FROM user_permissions up 
        WHERE up.user_id = admin_user_id 
        AND up.permission_key = p.permission_key
    );
    
    RAISE NOTICE 'User permissions granted successfully.';
    
    -- =====================================================
    -- 2. منح صلاحيات إدارة الأقسام
    -- =====================================================
    
    -- حذف صلاحيات الأقسام الموجودة
    DELETE FROM user_department_permissions WHERE user_id = admin_user_id;
    
    -- منح صلاحيات لجميع الأقسام
    INSERT INTO user_department_permissions (user_id, department_id, can_view, can_edit, can_delete, can_manage_users, created_at, updated_at)
    SELECT 
        admin_user_id,
        d.id,
        true,  -- can_view
        true,  -- can_edit
        true,  -- can_delete
        true,  -- can_manage_users
        NOW(),
        NOW()
    FROM departments d
    WHERE NOT EXISTS (
        SELECT 1 FROM user_department_permissions udp 
        WHERE udp.user_id = admin_user_id 
        AND udp.department_id = d.id
    );
    
    RAISE NOTICE 'Department permissions granted successfully.';
    
    -- =====================================================
    -- 3. منح صلاحيات إدارة الشهادات
    -- =====================================================
    
    -- حذف صلاحيات الشهادات الموجودة
    DELETE FROM user_certificate_permissions WHERE user_id = admin_user_id;
    
    -- منح صلاحيات لجميع أنواع الشهادات
    INSERT INTO user_certificate_permissions (user_id, certificate_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
    VALUES 
        (admin_user_id, 'AFTN', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'AIS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'CNS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'ATFM', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Meteorology', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Airport', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Ground', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Maintenance', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Security', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'General', true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'Certificate permissions granted successfully.';
    
    -- =====================================================
    -- 4. منح صلاحيات إدارة الرخص
    -- =====================================================
    
    -- حذف صلاحيات الرخص الموجودة
    DELETE FROM user_license_permissions WHERE user_id = admin_user_id;
    
    -- منح صلاحيات لجميع أنواع الرخص
    INSERT INTO user_license_permissions (user_id, license_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
    VALUES 
        (admin_user_id, 'AFTN', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'AIS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'CNS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'ATFM', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Meteorology', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Airport', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Ground', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Maintenance', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Security', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'General', true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'License permissions granted successfully.';
    
    -- =====================================================
    -- 5. منح صلاحيات إدارة المراقبة
    -- =====================================================
    
    -- حذف صلاحيات المراقبة الموجودة
    DELETE FROM user_observation_permissions WHERE user_id = admin_user_id;
    
    -- منح صلاحيات لجميع أنواع المراقبة
    INSERT INTO user_observation_permissions (user_id, observation_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
    VALUES 
        (admin_user_id, 'AFTN', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'AIS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'CNS', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'ATFM', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Meteorology', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Airport', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Ground', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Maintenance', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'Security', true, true, true, true, NOW(), NOW()),
        (admin_user_id, 'General', true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'Observation permissions granted successfully.';
    
    -- =====================================================
    -- 6. منح صلاحيات إدارة الموظفين
    -- =====================================================
    
    -- حذف صلاحيات الموظفين الموجودة
    DELETE FROM user_employee_permissions WHERE user_id = admin_user_id;
    
    -- منح صلاحيات إدارة جميع الموظفين
    INSERT INTO user_employee_permissions (user_id, can_view_all, can_edit_all, can_delete_all, can_manage_salary, can_manage_contract, created_at, updated_at)
    VALUES (admin_user_id, true, true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'Employee permissions granted successfully.';
    
    -- =====================================================
    -- 7. منح صلاحيات إدارة النظام
    -- =====================================================
    
    -- حذف صلاحيات النظام الموجودة
    DELETE FROM user_system_permissions WHERE user_id = admin_user_id;
    
    -- منح جميع صلاحيات النظام
    INSERT INTO user_system_permissions (user_id, can_manage_users, can_manage_roles, can_manage_permissions, can_view_logs, can_manage_config, can_backup_restore, created_at, updated_at)
    VALUES (admin_user_id, true, true, true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'System permissions granted successfully.';
    
    -- =====================================================
    -- 8. منح صلاحيات إدارة التقارير
    -- =====================================================
    
    -- حذف صلاحيات التقارير الموجودة
    DELETE FROM user_report_permissions WHERE user_id = admin_user_id;
    
    -- منح جميع صلاحيات التقارير
    INSERT INTO user_report_permissions (user_id, can_view_all_reports, can_export_reports, can_schedule_reports, can_manage_report_templates, created_at, updated_at)
    VALUES (admin_user_id, true, true, true, true, NOW(), NOW());
    
    RAISE NOTICE 'Report permissions granted successfully.';
    
    -- =====================================================
    -- 9. تحديث دور المستخدم إلى Super Admin
    -- =====================================================
    
    UPDATE controller_users 
    SET 
        role = 'SuperAdmin',
        is_active = true,
        updated_at = NOW()
    WHERE id = admin_user_id;
    
    RAISE NOTICE 'User role updated to SuperAdmin.';
    
    -- =====================================================
    -- 10. إنشاء جلسة نشطة للمستخدم
    -- =====================================================
    
    -- حذف الجلسات القديمة
    DELETE FROM user_sessions WHERE user_id = admin_user_id;
    
    -- إنشاء جلسة جديدة
    INSERT INTO user_sessions (user_id, session_token, expires_at, created_at, updated_at)
    VALUES (admin_user_id, 'admin-super-session-' || EXTRACT(EPOCH FROM NOW()), NOW() + INTERVAL '24 hours', NOW(), NOW());
    
    RAISE NOTICE 'Active session created for admin user.';
    
    RAISE NOTICE '=====================================================';
    RAISE NOTICE '✅ ALL PERMISSIONS GRANTED SUCCESSFULLY TO ADMIN USER';
    RAISE NOTICE '=====================================================';
    RAISE NOTICE 'Admin user now has:';
    RAISE NOTICE '- All system permissions';
    RAISE NOTICE '- All department permissions';
    RAISE NOTICE '- All certificate permissions';
    RAISE NOTICE '- All license permissions';
    RAISE NOTICE '- All observation permissions';
    RAISE NOTICE '- All employee permissions';
    RAISE NOTICE '- All report permissions';
    RAISE NOTICE '- SuperAdmin role';
    RAISE NOTICE '- Active session';
    RAISE NOTICE '=====================================================';
    
END $$;

-- التحقق النهائي من الصلاحيات
SELECT 
    'Admin User Info' as info_type,
    cu.username,
    cu.role,
    cu.is_active,
    COUNT(up.permission_key) as total_permissions
FROM controller_users cu
LEFT JOIN user_permissions up ON cu.id = up.user_id
WHERE cu.username = 'admin'
GROUP BY cu.id, cu.username, cu.role, cu.is_active

UNION ALL

SELECT 
    'Department Permissions' as info_type,
    'Total Departments' as username,
    COUNT(*)::text as role,
    'true' as is_active,
    COUNT(*) as total_permissions
FROM user_department_permissions udp
JOIN controller_users cu ON udp.user_id = cu.id
WHERE cu.username = 'admin'

UNION ALL

SELECT 
    'Certificate Permissions' as info_type,
    'Total Certificate Types' as username,
    COUNT(*)::text as role,
    'true' as is_active,
    COUNT(*) as total_permissions
FROM user_certificate_permissions ucp
JOIN controller_users cu ON ucp.user_id = cu.id
WHERE cu.username = 'admin'

UNION ALL

SELECT 
    'License Permissions' as info_type,
    'Total License Types' as username,
    COUNT(*)::text as role,
    'true' as is_active,
    COUNT(*) as total_permissions
FROM user_license_permissions ulp
JOIN controller_users cu ON ulp.user_id = cu.id
WHERE cu.username = 'admin';

-- رسالة النجاح النهائية
DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '🎉 SCRIPT EXECUTED SUCCESSFULLY!';
    RAISE NOTICE 'Admin user now has full system access.';
    RAISE NOTICE 'You can now log in with:';
    RAISE NOTICE 'Username: admin';
    RAISE NOTICE 'Password: 123';
    RAISE NOTICE '';
END $$;