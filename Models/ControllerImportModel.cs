using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    /// <summary>
    /// نموذج استيراد المراقبين من ملف Excel
    /// </summary>
    public class ControllerImportModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "اسم المستخدم يجب أن يحتوي على أحرف وأرقام ونقاط وشرطات فقط")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم/القطاع مطلوب")]
        [Display(Name = "القسم/القطاع")]
        public string Division { get; set; } = string.Empty;

        [Required(ErrorMessage = "الهيكل التنظيمي مطلوب")]
        [Display(Name = "الهيكل التنظيمي")]
        public string OrganizationalStructure { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز المطار الدولي مطلوب")]
        [Display(Name = "رمز المطار الدولي")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "رمز المطار يجب أن يكون 3-4 أحرف")]
        public string ICAO { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "كلمة المرور المخصصة")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "كلمة المرور يجب أن تكون 3-100 حرف")]
        public string? CustomPassword { get; set; }

        [Display(Name = "المستوى التعليمي")]
        public string? EducationLevel { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "الحالة الاجتماعية")]
        public string? MaritalStatus { get; set; }

        [Display(Name = "العنوان")]
        [StringLength(500, ErrorMessage = "العنوان يجب أن يكون أقل من 500 حرف")]
        public string? Address { get; set; }

        [Display(Name = "تاريخ التعيين")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        [Display(Name = "حالة التوظيف")]
        public string? EmploymentStatus { get; set; }

        [Display(Name = "القسم الحالي")]
        public string? CurrentDepartment { get; set; }

        [Display(Name = "يحتاج رخصة")]
        public bool NeedLicense { get; set; } = true;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        // حقول إضافية للتعامل مع Excel
        [Display(Name = "رقم الصف في Excel")]
        public int ExcelRowNumber { get; set; }

        [Display(Name = "أخطاء التحقق")]
        public List<string> ValidationErrors { get; set; } = new List<string>();

        [Display(Name = "صالح للاستيراد")]
        public bool IsValid => !ValidationErrors.Any();

        /// <summary>
        /// التحقق من صحة البيانات
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            // التحقق من التاريخ
            if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.Now)
            {
                ValidationErrors.Add("تاريخ الميلاد لا يمكن أن يكون في المستقبل");
            }

            if (HireDate.HasValue && HireDate.Value > DateTime.Now)
            {
                ValidationErrors.Add("تاريخ التعيين لا يمكن أن يكون في المستقبل");
            }

            // التحقق من طول الحقول
            if (FullName.Length > 255)
            {
                ValidationErrors.Add("الاسم الكامل يجب أن يكون أقل من 255 حرف");
            }

            if (Username.Length > 100)
            {
                ValidationErrors.Add("اسم المستخدم يجب أن يكون أقل من 100 حرف");
            }

            // التحقق من صيغة ICAO
            if (!string.IsNullOrEmpty(ICAO) && !System.Text.RegularExpressions.Regex.IsMatch(ICAO, @"^[A-Z]{3,4}$"))
            {
                ValidationErrors.Add("رمز المطار الدولي يجب أن يكون 3-4 أحرف كبيرة");
            }
        }

        /// <summary>
        /// الحصول على كلمة المرور (افتراضية أو مخصصة)
        /// </summary>
        public string GetPassword()
        {
            return string.IsNullOrWhiteSpace(CustomPassword) ? "123" : CustomPassword;
        }

        /// <summary>
        /// معالجة التواريخ من Excel
        /// </summary>
        public static DateTime? ParseExcelDate(object? excelValue)
        {
            if (excelValue == null || excelValue == DBNull.Value)
                return null;

            if (excelValue is DateTime dateTime)
                return dateTime;

            if (excelValue is string dateString)
            {
                if (DateTime.TryParse(dateString, out DateTime parsedDate))
                    return parsedDate;
                
                // Try different date formats
                string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy" };
                if (DateTime.TryParseExact(dateString, formats, null, System.Globalization.DateTimeStyles.None, out DateTime exactDate))
                    return exactDate;
            }

            if (excelValue is double excelDate)
            {
                // Excel stores dates as numbers (days since 1900-01-01)
                try
                {
                    return DateTime.FromOADate(excelDate);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// نموذج نتيجة الاستيراد
    /// </summary>
    public class ControllerImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ControllerImportModel> ValidControllers { get; set; } = new List<ControllerImportModel>();
        public List<ControllerImportModel> InvalidControllers { get; set; } = new List<ControllerImportModel>();
    }
} 