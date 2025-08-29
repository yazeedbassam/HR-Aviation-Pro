USE [HR-Aviation]
GO

-- إضافة فئة "Roles" إذا لم تكن موجودة
IF NOT EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Roles')
BEGIN
    INSERT INTO ConfigurationCategories (CategoryName, Description, IsActive)
    VALUES ('Roles', 'User Roles for Permission System', 1)
    PRINT 'تم إضافة فئة Roles'
END
ELSE
BEGIN
    PRINT 'فئة Roles موجودة بالفعل'
END

-- الحصول على CategoryId للـ Roles
DECLARE @RolesCategoryId INT
SELECT @RolesCategoryId = CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'

-- إضافة الأدوار الأساسية إذا لم تكن موجودة
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryId = @RolesCategoryId AND ValueKey = 'ADMIN')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive)
    VALUES (@RolesCategoryId, 'ADMIN', 'Administrator', 1)
    PRINT 'تم إضافة دور Administrator'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryId = @RolesCategoryId AND ValueKey = 'SUPERVISOR')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive)
    VALUES (@RolesCategoryId, 'SUPERVISOR', 'Supervisor', 1)
    PRINT 'تم إضافة دور Supervisor'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryId = @RolesCategoryId AND ValueKey = 'STAFF')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive)
    VALUES (@RolesCategoryId, 'STAFF', 'Staff Member', 1)
    PRINT 'تم إضافة دور Staff Member'
END

-- عرض النتائج
SELECT 'ConfigurationCategories' as TableName, CategoryId, CategoryName, Description, IsActive
FROM ConfigurationCategories 
WHERE CategoryName = 'Roles'

SELECT 'ConfigurationValues' as TableName, ValueId, ValueKey, ValueText, IsActive
FROM ConfigurationValues 
WHERE CategoryId = @RolesCategoryId

PRINT 'تم إصلاح بيانات الأدوار بنجاح!'
GO 
