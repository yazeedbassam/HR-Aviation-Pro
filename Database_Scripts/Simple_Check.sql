USE [HR-Aviation]

-- Check if Divisions category exists
SELECT 'Divisions Category' as CheckType, 
       CASE WHEN EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Divisions') 
            THEN 'EXISTS' ELSE 'NOT EXISTS' END as Status

-- Check if any divisions exist
SELECT 'Divisions in ConfigurationValues' as CheckType,
       CASE WHEN EXISTS (SELECT 1 FROM ConfigurationValues cv 
                        JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
                        WHERE cc.CategoryName = 'Divisions') 
            THEN 'EXISTS' ELSE 'NOT EXISTS' END as Status

-- Show all categories
SELECT 'All Categories' as CheckType, CategoryId, CategoryName, DisplayName, IsActive
FROM ConfigurationCategories
ORDER BY CategoryName

-- Show all values for Divisions category
SELECT 'Divisions Values' as CheckType, cv.ValueId, cv.ValueText, cv.ValueKey, cv.IsActive
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Divisions'
ORDER BY cv.ValueText 
