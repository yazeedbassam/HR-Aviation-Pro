-- =====================================================
-- إضافة الأعمدة المالية إلى جدول controllers
-- =====================================================

PRINT 'بدء إضافة الأعمدة المالية...';

-- إضافة عمود الراتب الحالي
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'current_salary')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [current_salary] [decimal](18,2) NULL;
    PRINT '✓ تم إضافة عمود current_salary';
END
ELSE
BEGIN
    PRINT '✓ عمود current_salary موجود بالفعل';
END

-- إضافة عمود نسبة الزيادة السنوية
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'annual_increase_percentage')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [annual_increase_percentage] [decimal](5,2) NULL;
    PRINT '✓ تم إضافة عمود annual_increase_percentage';
END
ELSE
BEGIN
    PRINT '✓ عمود annual_increase_percentage موجود بالفعل';
END

-- إضافة عمود الراتب بعد الزيادة السنوية
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'salary_after_annual_increase')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [salary_after_annual_increase] [decimal](18,2) NULL;
    PRINT '✓ تم إضافة عمود salary_after_annual_increase';
END
ELSE
BEGIN
    PRINT '✓ عمود salary_after_annual_increase موجود بالفعل';
END

-- إضافة عمود رقم الحساب البنكي
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'bank_account_number')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [bank_account_number] [varchar](50) NULL;
    PRINT '✓ تم إضافة عمود bank_account_number';
END
ELSE
BEGIN
    PRINT '✓ عمود bank_account_number موجود بالفعل';
END

-- إضافة عمود اسم البنك
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'bank_name')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [bank_name] [varchar](100) NULL;
    PRINT '✓ تم إضافة عمود bank_name';
END
ELSE
BEGIN
    PRINT '✓ عمود bank_name موجود بالفعل';
END

-- إضافة عمود الرقم الضريبي
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'tax_id')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [tax_id] [varchar](50) NULL;
    PRINT '✓ تم إضافة عمود tax_id';
END
ELSE
BEGIN
    PRINT '✓ عمود tax_id موجود بالفعل';
END

-- إضافة عمود رقم التأمين
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'insurance_number')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [insurance_number] [varchar](50) NULL;
    PRINT '✓ تم إضافة عمود insurance_number';
END
ELSE
BEGIN
    PRINT '✓ عمود insurance_number موجود بالفعل';
END

-- إضافة عمود تاريخ الميلاد
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'date_of_birth')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [date_of_birth] [date] NULL;
    PRINT '✓ تم إضافة عمود date_of_birth';
END
ELSE
BEGIN
    PRINT '✓ عمود date_of_birth موجود بالفعل';
END

-- إضافة عمود الحالة الاجتماعية
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'marital_status')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [marital_status] [varchar](20) NULL;
    PRINT '✓ تم إضافة عمود marital_status';
END
ELSE
BEGIN
    PRINT '✓ عمود marital_status موجود بالفعل';
END

-- إضافة عمود المستوى التعليمي
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'education_level')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [education_level] [varchar](50) NULL;
    PRINT '✓ تم إضافة عمود education_level';
END
ELSE
BEGIN
    PRINT '✓ عمود education_level موجود بالفعل';
END

-- إضافة عمود الحالة الوظيفية
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'employment_status')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [employment_status] [varchar](20) NULL;
    PRINT '✓ تم إضافة عمود employment_status';
END
ELSE
BEGIN
    PRINT '✓ عمود employment_status موجود بالفعل';
END

-- إضافة عمود جهة الاتصال في الطوارئ
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'emergency_contact')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [emergency_contact] [varchar](50) NULL;
    PRINT '✓ تم إضافة عمود emergency_contact';
END
ELSE
BEGIN
    PRINT '✓ عمود emergency_contact موجود بالفعل';
END

-- إضافة عمود العنوان
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'address')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [address] [varchar](200) NULL;
    PRINT '✓ تم إضافة عمود address';
END
ELSE
BEGIN
    PRINT '✓ عمود address موجود بالفعل';
END

-- إضافة عمود تاريخ التعيين
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('controllers') AND name = 'hire_date')
BEGIN
    ALTER TABLE [dbo].[controllers] ADD [hire_date] [date] NULL;
    PRINT '✓ تم إضافة عمود hire_date';
END
ELSE
BEGIN
    PRINT '✓ عمود hire_date موجود بالفعل';
END

PRINT 'تم الانتهاء من إضافة جميع الأعمدة المالية!';
PRINT 'الآن يمكنك اختبار صفحة البروفايل مرة أخرى'; 