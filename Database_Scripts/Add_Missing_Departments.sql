-- =====================================================
-- Add Missing Departments to Divisions Category
-- =====================================================

USE [HR-Aviation]
GO

-- Check if Divisions category exists, if not create it
IF NOT EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Divisions')
BEGIN
    INSERT INTO ConfigurationCategories (CategoryName, DisplayName, IsActive) 
    VALUES ('Divisions', 'Departments and Divisions', 1)
    PRINT 'Divisions category created'
END

-- Get Divisions category ID
DECLARE @DivisionsCategoryId int
SELECT @DivisionsCategoryId = CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Divisions'

-- Add missing departments to Divisions category
-- Check and add Administration
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'Administration')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'ADMINISTRATION', 'Administration', 1)
    PRINT 'Administration department added to Divisions'
END

-- Check and add Queen
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'Queen')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'QUEEN', 'Queen', 1)
    PRINT 'Queen department added to Divisions'
END

-- Check and add Aqaba
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'Aqaba')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'AQABA', 'Aqaba', 1)
    PRINT 'Aqaba department added to Divisions'
END

-- Check and add Amman
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'Amman')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'AMMAN', 'Amman', 1)
    PRINT 'Amman department added to Divisions'
END

-- Check and add TACC
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'TACC')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'TACC', 'TACC', 1)
    PRINT 'TACC department added to Divisions'
END

-- Check and add CARC
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues cv 
               INNER JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
               WHERE cc.CategoryName = 'Divisions' AND cv.ValueText = 'CARC')
BEGIN
    INSERT INTO ConfigurationValues (CategoryId, ValueKey, ValueText, IsActive) 
    VALUES (@DivisionsCategoryId, 'CARC', 'CARC', 1)
    PRINT 'CARC department added to Divisions'
END

-- Show all departments in Divisions category
SELECT 'All Divisions' as Category, cv.ValueId, cv.ValueText, cv.ValueKey, cv.IsActive
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Divisions'
ORDER BY cv.ValueText

PRINT 'All missing departments have been added to Divisions category!'
GO 
