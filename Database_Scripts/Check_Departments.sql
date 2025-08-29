-- =====================================================
-- Check Departments in ConfigurationValues
-- =====================================================

USE [HR-Aviation]
GO

-- Check ConfigurationCategories
SELECT 'ConfigurationCategories' as TableName, CategoryId, CategoryName, DisplayName, IsActive
FROM ConfigurationCategories
WHERE CategoryName = 'Divisions'
GO

-- Check ConfigurationValues for Divisions
SELECT 'ConfigurationValues for Divisions' as TableName, cv.ValueId, cv.ValueText, cv.ValueKey, cv.IsActive, cc.CategoryName
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Divisions'
GO

-- Check all ConfigurationValues
SELECT 'All ConfigurationValues' as TableName, cv.ValueId, cv.ValueText, cv.ValueKey, cv.IsActive, cc.CategoryName
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
ORDER BY cc.CategoryName, cv.ValueText
GO

-- Check if Divisions category exists
IF EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Divisions')
    PRINT 'Divisions category exists'
ELSE
    PRINT 'Divisions category does NOT exist'

-- Check if any divisions exist
IF EXISTS (SELECT 1 FROM ConfigurationValues cv 
           JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
           WHERE cc.CategoryName = 'Divisions')
    PRINT 'Divisions exist in ConfigurationValues'
ELSE
    PRINT 'No divisions found in ConfigurationValues'
GO 
