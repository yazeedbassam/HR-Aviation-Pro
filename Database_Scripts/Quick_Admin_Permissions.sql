-- =====================================================
-- سكريبت سريع لمنح صلاحيات admin
-- Quick Admin Permissions Script
-- =====================================================
-- تاريخ الإنشاء: 2025-01-09
-- الغرض: سكريبت مبسط وسريع لمنح جميع الصلاحيات للمستخدم admin
-- =====================================================

-- 1. التأكد من وجود المستخدم admin
INSERT INTO controller_users (username, password_hash, role, is_active, created_at, updated_at)
VALUES ('admin', '123', 'SuperAdmin', true, NOW(), NOW())
ON CONFLICT (username) DO UPDATE SET
    role = 'SuperAdmin',
    is_active = true,
    updated_at = NOW();

-- 2. منح جميع الصلاحيات الأساسية
INSERT INTO user_permissions (user_id, permission_key, is_granted, created_at, updated_at)
SELECT 
    cu.id,
    p.permission_key,
    true,
    NOW(),
    NOW()
FROM controller_users cu
CROSS JOIN permissions p
WHERE cu.username = 'admin'
ON CONFLICT (user_id, permission_key) DO UPDATE SET
    is_granted = true,
    updated_at = NOW();

-- 3. منح صلاحيات جميع الأقسام
INSERT INTO user_department_permissions (user_id, department_id, can_view, can_edit, can_delete, can_manage_users, created_at, updated_at)
SELECT 
    cu.id,
    d.id,
    true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
CROSS JOIN departments d
WHERE cu.username = 'admin'
ON CONFLICT (user_id, department_id) DO UPDATE SET
    can_view = true,
    can_edit = true,
    can_delete = true,
    can_manage_users = true,
    updated_at = NOW();

-- 4. منح صلاحيات جميع أنواع الشهادات
INSERT INTO user_certificate_permissions (user_id, certificate_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
SELECT 
    cu.id,
    unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
    true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id, certificate_type) DO UPDATE SET
    can_view = true,
    can_edit = true,
    can_delete = true,
    can_approve = true,
    updated_at = NOW();

-- 5. منح صلاحيات جميع أنواع الرخص
INSERT INTO user_license_permissions (user_id, license_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
SELECT 
    cu.id,
    unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
    true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id, license_type) DO UPDATE SET
    can_view = true,
    can_edit = true,
    can_delete = true,
    can_approve = true,
    updated_at = NOW();

-- 6. منح صلاحيات جميع أنواع المراقبة
INSERT INTO user_observation_permissions (user_id, observation_type, can_view, can_edit, can_delete, can_approve, created_at, updated_at)
SELECT 
    cu.id,
    unnest(ARRAY['AFTN', 'AIS', 'CNS', 'ATFM', 'Meteorology', 'Airport', 'Ground', 'Maintenance', 'Security', 'General']),
    true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id, observation_type) DO UPDATE SET
    can_view = true,
    can_edit = true,
    can_delete = true,
    can_approve = true,
    updated_at = NOW();

-- 7. منح صلاحيات إدارة الموظفين
INSERT INTO user_employee_permissions (user_id, can_view_all, can_edit_all, can_delete_all, can_manage_salary, can_manage_contract, created_at, updated_at)
SELECT 
    cu.id,
    true, true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id) DO UPDATE SET
    can_view_all = true,
    can_edit_all = true,
    can_delete_all = true,
    can_manage_salary = true,
    can_manage_contract = true,
    updated_at = NOW();

-- 8. منح صلاحيات النظام
INSERT INTO user_system_permissions (user_id, can_manage_users, can_manage_roles, can_manage_permissions, can_view_logs, can_manage_config, can_backup_restore, created_at, updated_at)
SELECT 
    cu.id,
    true, true, true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id) DO UPDATE SET
    can_manage_users = true,
    can_manage_roles = true,
    can_manage_permissions = true,
    can_view_logs = true,
    can_manage_config = true,
    can_backup_restore = true,
    updated_at = NOW();

-- 9. منح صلاحيات التقارير
INSERT INTO user_report_permissions (user_id, can_view_all_reports, can_export_reports, can_schedule_reports, can_manage_report_templates, created_at, updated_at)
SELECT 
    cu.id,
    true, true, true, true,
    NOW(),
    NOW()
FROM controller_users cu
WHERE cu.username = 'admin'
ON CONFLICT (user_id) DO UPDATE SET
    can_view_all_reports = true,
    can_export_reports = true,
    can_schedule_reports = true,
    can_manage_report_templates = true,
    updated_at = NOW();

-- 10. التحقق من النتيجة
SELECT 
    'SUCCESS' as status,
    cu.username,
    cu.role,
    cu.is_active,
    COUNT(up.permission_key) as permissions_count
FROM controller_users cu
LEFT JOIN user_permissions up ON cu.id = up.user_id AND up.is_granted = true
WHERE cu.username = 'admin'
GROUP BY cu.id, cu.username, cu.role, cu.is_active;