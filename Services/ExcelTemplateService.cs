using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    /// <summary>
    /// خدمة إنشاء قوالب Excel
    /// </summary>
    public class ExcelTemplateService
    {
        /// <summary>
        /// إنشاء قالب Excel لاستيراد المراقبين
        /// </summary>
        public byte[] GenerateControllersImportTemplate()
        {
            // تعيين ترخيص EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // إنشاء ورقة التعليمات
                var instructionsSheet = package.Workbook.Worksheets.Add("تعليمات الاستيراد");
                CreateInstructionsSheet(instructionsSheet);

                // إنشاء ورقة القالب
                var templateSheet = package.Workbook.Worksheets.Add("Template Data");
                CreateTemplateSheet(templateSheet);

                // إنشاء ورقة البيانات المطلوبة
                var requiredDataSheet = package.Workbook.Worksheets.Add("البيانات المطلوبة");
                CreateRequiredDataSheet(requiredDataSheet);

                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// إنشاء ورقة التعليمات
        /// </summary>
        private void CreateInstructionsSheet(ExcelWorksheet worksheet)
        {
            // عنوان الصفحة
            worksheet.Cells["A1"].Value = "دليل استيراد المراقبين - Controllers Import Guide";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.DarkBlue);
            worksheet.Cells["A1:H1"].Merge = true;

            // التعليمات الأساسية
            var instructions = new[]
            {
                "كيفية استخدام هذا القالب:",
                "",
                "1. لا تغير أسماء الأعمدة في الصف الأول",
                "2. املأ البيانات في الصفوف التالية",
                "3. تأكد من صحة البيانات قبل الحفظ",
                "4. احفظ الملف بصيغة .xlsx",
                "5. ارفع الملف في النظام",
                "",
                "الحقول المطلوبة (Required):",
                "- FullName: الاسم الكامل للمراقب",
                "- Username: اسم المستخدم (يجب أن يكون فريد)",
                "- JobTitle: المسمى الوظيفي",
                "- Division: القسم/القطاع",
                "- OrganizationalStructure: الهيكل التنظيمي",
                "- ICAO: رمز المطار الدولي",
                "",
                "الحقول الاختيارية (Optional):",
                "- Email: البريد الإلكتروني",
                "- PhoneNumber: رقم الهاتف",
                "- CustomPassword: كلمة المرور (اتركها فارغة لاستخدام \"123\" كافتراضي)",
                "- EducationLevel: المستوى التعليمي",
                "- DateOfBirth: تاريخ الميلاد (YYYY-MM-DD)",
                "- MaritalStatus: الحالة الاجتماعية",
                "- Address: العنوان",
                "- HireDate: تاريخ التعيين (YYYY-MM-DD)",
                "- EmploymentStatus: حالة التوظيف",
                "- CurrentDepartment: القسم الحالي",
                "- NeedLicense: يحتاج رخصة (TRUE/FALSE)",
                "- IsActive: نشط (TRUE/FALSE)",
                "",
                "ملاحظات مهمة:",
                "- Username يجب أن يكون فريد في النظام",
                "- تاريخ الميلاد والتعيين بصيغة YYYY-MM-DD",
                "- القيم المنطقية (TRUE/FALSE) للحقول المناسبة",
                "- لا تترك صفوف فارغة بين البيانات"
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                worksheet.Cells[$"A{i + 3}"].Value = instructions[i];
                if (i < 2) // العنوان
                {
                    worksheet.Cells[$"A{i + 3}"].Style.Font.Bold = true;
                    worksheet.Cells[$"A{i + 3}"].Style.Font.Size = 14;
                }
            }

            // تنسيق الأعمدة
            worksheet.Columns[1].Width = 60;
        }

        /// <summary>
        /// إنشاء ورقة القالب
        /// </summary>
        private void CreateTemplateSheet(ExcelWorksheet worksheet)
        {
            // أسماء الأعمدة
            var headers = new[]
            {
                "FullName",
                "Username", 
                "JobTitle",
                "Division",
                "OrganizationalStructure",
                "ICAO",
                "Email",
                "PhoneNumber",
                "CustomPassword",
                "EducationLevel",
                "DateOfBirth",
                "MaritalStatus",
                "Address",
                "HireDate",
                "EmploymentStatus",
                "CurrentDepartment",
                "NeedLicense",
                "IsActive"
            };

            // إضافة العناوين
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // إضافة بيانات مثال
            var sampleData = new[]
            {
                "John Doe",
                "john.doe",
                "Air Traffic Controller",
                "ANSS",
                "GACA",
                "OERK",
                "john@example.com",
                "+966501234567",
                "", // كلمة مرور فارغة = "123" افتراضية
                "Master's Degree",
                "1990-01-01",
                "Married",
                "Riyadh, Saudi Arabia",
                "2023-01-01",
                "Active",
                "ATC Department",
                "TRUE",
                "TRUE"
            };

            // إضافة البيانات في الصف الثاني
            for (int i = 0; i < sampleData.Length; i++)
            {
                var cell = worksheet.Cells[2, i + 1];
                cell.Value = sampleData[i];
                cell.Style.Font.Color.SetColor(Color.Gray);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // إضافة تعليق توضيحي
            worksheet.Cells["A3"].Value = "ملاحظة: الصف الثاني يحتوي على بيانات مثال. احذفه واملأ بياناتك الخاصة.";
            worksheet.Cells["A3"].Style.Font.Italic = true;
            worksheet.Cells["A3"].Style.Font.Color.SetColor(Color.Red);
            worksheet.Cells["A3:R3"].Merge = true;

            // تنسيق الأعمدة
            for (int i = 1; i <= headers.Length; i++)
            {
                worksheet.Columns[i].Width = 20;
            }

            // تنسيق خاص للأعمدة
            worksheet.Columns[1].Width = 25; // FullName
            worksheet.Columns[2].Width = 20; // Username
            worksheet.Columns[5].Width = 30; // OrganizationalStructure
            worksheet.Columns[6].Width = 10; // ICAO
            worksheet.Columns[7].Width = 30; // Email
            worksheet.Columns[13].Width = 40; // Address
        }

        /// <summary>
        /// إنشاء ورقة البيانات المطلوبة
        /// </summary>
        private void CreateRequiredDataSheet(ExcelWorksheet worksheet)
        {
            // عنوان الصفحة
            worksheet.Cells["A1"].Value = "البيانات المطلوبة للاستيراد";
            worksheet.Cells["A1"].Style.Font.Size = 14;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.DarkGreen);
            worksheet.Cells["A1:C1"].Merge = true;

            // قائمة القيم المتاحة
            var data = new[]
            {
                new { Category = "JobTitle", Values = "Air Traffic Controller, Section Head, Staff, Supervisor" },
                new { Category = "Division", Values = "ANSS, CNS, AIS, AFTN, Administration, Safety, Quality" },
                new { Category = "OrganizationalStructure", Values = "GACA, Jordan, UAE, Kuwait, Oman, Bahrain, Qatar" },
                new { Category = "ICAO", Values = "OERK, OJAI, OMDB, OKBK, OOMS, OBBI, OTBD" },
                new { Category = "EducationLevel", Values = "High School, Bachelor's Degree, Master's Degree, PhD" },
                new { Category = "MaritalStatus", Values = "Single, Married, Divorced, Widowed" },
                new { Category = "EmploymentStatus", Values = "Active, Suspended, Terminated, Retired" },
                new { Category = "CurrentDepartment", Values = "ATC Department, CNS Department, AIS Department, AFTN Department" },
                new { Category = "NeedLicense", Values = "TRUE, FALSE" },
                new { Category = "IsActive", Values = "TRUE, FALSE" }
            };

            // إضافة البيانات
            worksheet.Cells["A3"].Value = "الفئة";
            worksheet.Cells["B3"].Value = "القيم المتاحة";
            worksheet.Cells["A3:B3"].Style.Font.Bold = true;
            worksheet.Cells["A3:B3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A3:B3"].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            for (int i = 0; i < data.Length; i++)
            {
                worksheet.Cells[$"A{i + 4}"].Value = data[i].Category;
                worksheet.Cells[$"B{i + 4}"].Value = data[i].Values;
                worksheet.Cells[$"A{i + 4}:B{i + 4}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // تنسيق الأعمدة
            worksheet.Columns[1].Width = 25;
            worksheet.Columns[2].Width = 60;
        }
    }
} 