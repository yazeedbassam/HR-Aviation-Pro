using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{


    // نموذج الفئات
    public class ConfigurationCategory
    {
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Property
        public virtual ICollection<ConfigurationValue> Values { get; set; } = new List<ConfigurationValue>();
    }

    // نموذج القيم
    public class ConfigurationValue
    {
        public int ValueId { get; set; }
        
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ValueKey { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ValueText { get; set; }
        
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Property
        public virtual ConfigurationCategory Category { get; set; }
    }

    // نموذج سجل التغييرات
    public class ConfigurationLog
    {
        public int LogId { get; set; }
        
        public int? ValueId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; }
        
        [StringLength(500)]
        public string? OldValue { get; set; }
        
        [StringLength(500)]
        public string? NewValue { get; set; }
        
        [StringLength(100)]
        public string? ChangedBy { get; set; }
        
        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }

    // نموذج ViewModel للقوائم المنسدلة
    public class ConfigurationSelectListItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    // نموذج ViewModel لإدارة الإعدادات
    public class ConfigurationManagementViewModel
    {
        public List<ConfigurationCategory> Categories { get; set; } = new List<ConfigurationCategory>();
        public ConfigurationCategory? SelectedCategory { get; set; }
        public List<ConfigurationValue> Values { get; set; } = new List<ConfigurationValue>();
    }

    // نموذج ViewModel لإضافة/تعديل قيمة
    public class ConfigurationValueViewModel
    {
        public int ValueId { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Value Key")]
        public string ValueKey { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Display Text")]
        public string ValueText { get; set; }
        
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
        
        [Display(Name = "Active")]
        public string IsActiveString { get; set; } = "true";
        
        // للعرض فقط
        public string? CategoryName { get; set; }
        public string? CategoryDisplayName { get; set; }
    }

    // نموذج ViewModel لإضافة فئة جديدة
    public class ConfigurationCategoryViewModel
    {
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Active")]
        public string IsActiveString { get; set; } = "true";
        
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
    }

    // نموذج ViewModel للبحث والتصفية
    public class ConfigurationSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    // نموذج ViewModel للتصدير
    public class ConfigurationExportViewModel
    {
        public string? CategoryName { get; set; }
        public string? ExportFormat { get; set; } // PDF, Excel, CSV
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    // نموذج ViewModel للإحصائيات
    public class ConfigurationStatisticsViewModel
    {
        public int TotalCategories { get; set; }
        public int TotalValues { get; set; }
        public int ActiveCategories { get; set; }
        public int ActiveValues { get; set; }
        public int RecentChanges { get; set; }
        public Dictionary<string, int> ValuesPerCategory { get; set; } = new Dictionary<string, int>();
        public List<ConfigurationLog> RecentLogs { get; set; } = new List<ConfigurationLog>();
        
        // خصائص محسوبة للنسب المئوية
        public double ActiveCategoriesPercentage => TotalCategories > 0 ? (double)ActiveCategories / TotalCategories * 100 : 0;
        public double ActiveValuesPercentage => TotalValues > 0 ? (double)ActiveValues / TotalValues * 100 : 0;
    }
} 