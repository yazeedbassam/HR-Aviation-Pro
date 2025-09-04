-- =====================================================
-- Create Useful Functions for HR Aviation System
-- =====================================================
-- سكريبت لإنشاء Functions مفيدة للنظام
-- يجب تنفيذه في Supabase SQL Editor

-- إنشاء دالة للتحقق من صلاحية المستخدم
CREATE OR REPLACE FUNCTION CheckUserPermission(
    p_username VARCHAR(50),
    p_permission_name VARCHAR(100)
) RETURNS BOOLEAN AS $$
DECLARE
    has_permission BOOLEAN := FALSE;
BEGIN
    SELECT EXISTS(
        SELECT 1 
        FROM Users u
        JOIN UserPermissions up ON u.UserId = up.UserId
        JOIN Permissions p ON up.PermissionId = p.PermissionId
        WHERE u.Username = p_username 
        AND p.PermissionName = p_permission_name
        AND u.IsActive = TRUE
        AND p.IsActive = TRUE
    ) INTO has_permission;
    
    RETURN has_permission;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للتحقق من صلاحية المستخدم في قسم معين
CREATE OR REPLACE FUNCTION CheckUserDepartmentPermission(
    p_username VARCHAR(50),
    p_department_name VARCHAR(100),
    p_permission_type VARCHAR(20) -- 'View', 'Edit', 'Delete', 'Create'
) RETURNS BOOLEAN AS $$
DECLARE
    has_permission BOOLEAN := FALSE;
BEGIN
    SELECT EXISTS(
        SELECT 1 
        FROM Users u
        JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId
        JOIN Departments d ON udp.DepartmentId = d.DepartmentId
        WHERE u.Username = p_username 
        AND d.DepartmentName = p_department_name
        AND u.IsActive = TRUE
        AND d.IsActive = TRUE
        AND (
            (p_permission_type = 'View' AND udp.CanView = TRUE) OR
            (p_permission_type = 'Edit' AND udp.CanEdit = TRUE) OR
            (p_permission_type = 'Delete' AND udp.CanDelete = TRUE) OR
            (p_permission_type = 'Create' AND udp.CanCreate = TRUE)
        )
    ) INTO has_permission;
    
    RETURN has_permission;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتسجيل دخول المستخدم
CREATE OR REPLACE FUNCTION LogUserLogin(
    p_username VARCHAR(50)
) RETURNS BOOLEAN AS $$
DECLARE
    user_exists BOOLEAN := FALSE;
BEGIN
    -- التحقق من وجود المستخدم
    SELECT EXISTS(
        SELECT 1 FROM Users WHERE Username = p_username AND IsActive = TRUE
    ) INTO user_exists;
    
    IF user_exists THEN
        -- تحديث تاريخ آخر تسجيل دخول
        UPDATE Users 
        SET LastLoginDate = CURRENT_TIMESTAMP 
        WHERE Username = p_username;
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتغيير كلمة مرور المستخدم
CREATE OR REPLACE FUNCTION ChangeUserPassword(
    p_username VARCHAR(50),
    p_old_password VARCHAR(255),
    p_new_password VARCHAR(255)
) RETURNS BOOLEAN AS $$
DECLARE
    password_matches BOOLEAN := FALSE;
BEGIN
    -- التحقق من كلمة المرور القديمة
    SELECT (Password = p_old_password) INTO password_matches
    FROM Users 
    WHERE Username = p_username AND IsActive = TRUE;
    
    IF password_matches THEN
        -- تحديث كلمة المرور
        UPDATE Users 
        SET Password = p_new_password 
        WHERE Username = p_username;
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة لتسجيل الأنشطة
CREATE OR REPLACE FUNCTION LogUserActivity(
    p_user_id INTEGER,
    p_username VARCHAR(50),
    p_action VARCHAR(100),
    p_entity_type VARCHAR(50) DEFAULT NULL,
    p_entity_id VARCHAR(50) DEFAULT NULL,
    p_details TEXT DEFAULT NULL,
    p_ip_address INET DEFAULT NULL,
    p_user_agent TEXT DEFAULT NULL,
    p_is_successful BOOLEAN DEFAULT TRUE,
    p_error_message TEXT DEFAULT NULL
) RETURNS INTEGER AS $$
DECLARE
    log_id INTEGER;
BEGIN
    INSERT INTO ActivityLog (
        UserId, Username, Action, EntityType, EntityId, 
        Details, IpAddress, UserAgent, IsSuccessful, ErrorMessage
    ) VALUES (
        p_user_id, p_username, p_action, p_entity_type, p_entity_id,
        p_details, p_ip_address, p_user_agent, p_is_successful, p_error_message
    ) RETURNING LogId INTO log_id;
    
    RETURN log_id;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على صلاحيات المستخدم
CREATE OR REPLACE FUNCTION GetUserPermissions(
    p_username VARCHAR(50)
) RETURNS TABLE(
    PermissionName VARCHAR(100),
    Description TEXT,
    Category VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.PermissionName,
        p.Description,
        p.Category
    FROM Users u
    JOIN UserPermissions up ON u.UserId = up.UserId
    JOIN Permissions p ON up.PermissionId = p.PermissionId
    WHERE u.Username = p_username 
    AND u.IsActive = TRUE
    AND p.IsActive = TRUE
    ORDER BY p.Category, p.PermissionName;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على أقسام المستخدم
CREATE OR REPLACE FUNCTION GetUserDepartments(
    p_username VARCHAR(50)
) RETURNS TABLE(
    DepartmentName VARCHAR(100),
    Description TEXT,
    CanView BOOLEAN,
    CanEdit BOOLEAN,
    CanDelete BOOLEAN,
    CanCreate BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d.DepartmentName,
        d.Description,
        udp.CanView,
        udp.CanEdit,
        udp.CanDelete,
        udp.CanCreate
    FROM Users u
    JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId
    JOIN Departments d ON udp.DepartmentId = d.DepartmentId
    WHERE u.Username = p_username 
    AND u.IsActive = TRUE
    AND d.IsActive = TRUE
    ORDER BY d.DepartmentName;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على إحصائيات النظام
CREATE OR REPLACE FUNCTION GetSystemStatistics()
RETURNS TABLE(
    StatName VARCHAR(100),
    StatValue BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 'Active Users'::VARCHAR(100), COUNT(*)::BIGINT FROM Users WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Employees'::VARCHAR(100), COUNT(*)::BIGINT FROM Employees WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Controllers'::VARCHAR(100), COUNT(*)::BIGINT FROM Controllers WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Valid Certificates'::VARCHAR(100), COUNT(*)::BIGINT FROM Certificates WHERE IsValid = TRUE
    UNION ALL
    SELECT 'Valid Licenses'::VARCHAR(100), COUNT(*)::BIGINT FROM Licenses WHERE IsValid = TRUE
    UNION ALL
    SELECT 'Active Projects'::VARCHAR(100), COUNT(*)::BIGINT FROM Projects WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Active Observations'::VARCHAR(100), COUNT(*)::BIGINT FROM Observations WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Total Permissions'::VARCHAR(100), COUNT(*)::BIGINT FROM Permissions WHERE IsActive = TRUE
    UNION ALL
    SELECT 'Total Departments'::VARCHAR(100), COUNT(*)::BIGINT FROM Departments WHERE IsActive = TRUE;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على الشهادات والتراخيص المنتهية الصلاحية قريباً
CREATE OR REPLACE FUNCTION GetExpiringSoon(
    p_days INTEGER DEFAULT 30
) RETURNS TABLE(
    Type VARCHAR(20),
    Id INTEGER,
    Number VARCHAR(50),
    Title VARCHAR(200),
    ExpiryDate DATE,
    DaysUntilExpiry INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        'Certificate'::VARCHAR(20),
        CertificateId,
        CertificateNumber,
        CertificateTitle,
        ExpiryDate,
        (ExpiryDate - CURRENT_DATE)::INTEGER
    FROM Certificates
    WHERE IsValid = TRUE 
    AND ExpiryDate <= CURRENT_DATE + (p_days || ' days')::INTERVAL
    AND ExpiryDate > CURRENT_DATE
    
    UNION ALL
    
    SELECT 
        'License'::VARCHAR(20),
        LicenseId,
        LicenseNumber,
        LicenseTitle,
        ExpiryDate,
        (ExpiryDate - CURRENT_DATE)::INTEGER
    FROM Licenses
    WHERE IsValid = TRUE 
    AND ExpiryDate <= CURRENT_DATE + (p_days || ' days')::INTERVAL
    AND ExpiryDate > CURRENT_DATE
    
    ORDER BY ExpiryDate;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للحصول على الشهادات والتراخيص المنتهية الصلاحية
CREATE OR REPLACE FUNCTION GetExpired()
RETURNS TABLE(
    Type VARCHAR(20),
    Id INTEGER,
    Number VARCHAR(50),
    Title VARCHAR(200),
    ExpiryDate DATE,
    DaysOverdue INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        'Certificate'::VARCHAR(20),
        CertificateId,
        CertificateNumber,
        CertificateTitle,
        ExpiryDate,
        (CURRENT_DATE - ExpiryDate)::INTEGER
    FROM Certificates
    WHERE IsValid = TRUE 
    AND ExpiryDate < CURRENT_DATE
    
    UNION ALL
    
    SELECT 
        'License'::VARCHAR(20),
        LicenseId,
        LicenseNumber,
        LicenseTitle,
        ExpiryDate,
        (CURRENT_DATE - ExpiryDate)::INTEGER
    FROM Licenses
    WHERE IsValid = TRUE 
    AND ExpiryDate < CURRENT_DATE
    
    ORDER BY ExpiryDate;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للبحث في المستخدمين
CREATE OR REPLACE FUNCTION SearchUsers(
    p_search_term VARCHAR(100)
) RETURNS TABLE(
    UserId INTEGER,
    Username VARCHAR(50),
    FullName VARCHAR(100),
    Email VARCHAR(100),
    Role VARCHAR(50),
    IsActive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.UserId,
        u.Username,
        u.FullName,
        u.Email,
        u.Role,
        u.IsActive
    FROM Users u
    WHERE (
        u.Username ILIKE '%' || p_search_term || '%' OR
        u.FullName ILIKE '%' || p_search_term || '%' OR
        u.Email ILIKE '%' || p_search_term || '%' OR
        u.Role ILIKE '%' || p_search_term || '%'
    )
    ORDER BY u.FullName;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للبحث في الموظفين
CREATE OR REPLACE FUNCTION SearchEmployees(
    p_search_term VARCHAR(100)
) RETURNS TABLE(
    EmployeeId INTEGER,
    EmployeeNumber VARCHAR(20),
    FullName VARCHAR(100),
    Email VARCHAR(100),
    Department VARCHAR(100),
    Position VARCHAR(100),
    IsActive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        e.EmployeeId,
        e.EmployeeNumber,
        e.FullName,
        e.Email,
        e.Department,
        e.Position,
        e.IsActive
    FROM Employees e
    WHERE (
        e.EmployeeNumber ILIKE '%' || p_search_term || '%' OR
        e.FullName ILIKE '%' || p_search_term || '%' OR
        e.Email ILIKE '%' || p_search_term || '%' OR
        e.Department ILIKE '%' || p_search_term || '%' OR
        e.Position ILIKE '%' || p_search_term || '%'
    )
    ORDER BY e.FullName;
END;
$$ LANGUAGE plpgsql;

-- إنشاء دالة للبحث في المراقبين
CREATE OR REPLACE FUNCTION SearchControllers(
    p_search_term VARCHAR(100)
) RETURNS TABLE(
    ControllerId INTEGER,
    ControllerNumber VARCHAR(20),
    FullName VARCHAR(100),
    Email VARCHAR(100),
    Department VARCHAR(100),
    Position VARCHAR(100),
    IsActive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        c.ControllerId,
        c.ControllerNumber,
        c.FullName,
        c.Email,
        c.Department,
        c.Position,
        c.IsActive
    FROM Controllers c
    WHERE (
        c.ControllerNumber ILIKE '%' || p_search_term || '%' OR
        c.FullName ILIKE '%' || p_search_term || '%' OR
        c.Email ILIKE '%' || p_search_term || '%' OR
        c.Department ILIKE '%' || p_search_term || '%' OR
        c.Position ILIKE '%' || p_search_term || '%'
    )
    ORDER BY c.FullName;
END;
$$ LANGUAGE plpgsql;

-- عرض معلومات الـ Functions المُنشأة
SELECT 'Functions Created Successfully' as Status;

SELECT 
    'Created Functions' as Info,
    routine_name,
    routine_type,
    data_type
FROM information_schema.routines 
WHERE routine_schema = 'public'
AND routine_name IN ('CheckUserPermission', 'CheckUserDepartmentPermission', 'LogUserLogin',
                     'ChangeUserPassword', 'LogUserActivity', 'GetUserPermissions',
                     'GetUserDepartments', 'GetSystemStatistics', 'GetExpiringSoon',
                     'GetExpired', 'SearchUsers', 'SearchEmployees', 'SearchControllers')
ORDER BY routine_name;

-- رسالة النجاح
SELECT '✅ Functions created successfully!' as Message
UNION ALL
SELECT '🔧 You can now use these functions for system operations'
UNION ALL
SELECT '🔍 Functions provide easy access to common operations'
UNION ALL
SELECT '📊 Use GetSystemStatistics() for dashboard data'
UNION ALL
SELECT '⚠️  Use GetExpiringSoon() to check expiring certificates and licenses';