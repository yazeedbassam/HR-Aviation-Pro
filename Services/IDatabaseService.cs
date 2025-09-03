using System.Data;

namespace WebApplication1.Services
{
    /// <summary>
    /// واجهة خدمة قاعدة البيانات الذكية
    /// تدعم SQL Server المحلي و Supabase
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// الحصول على اتصال قاعدة البيانات المناسبة
        /// </summary>
        /// <returns>اتصال قاعدة البيانات</returns>
        IDbConnection GetConnection();

        /// <summary>
        /// الحصول على نوع قاعدة البيانات الحالية
        /// </summary>
        /// <returns>نوع قاعدة البيانات (SqlServer أو Supabase)</returns>
        string GetCurrentDatabase();

        /// <summary>
        /// التحقق من توفر قاعدة البيانات
        /// </summary>
        /// <returns>صحيح إذا كانت قاعدة البيانات متاحة</returns>
        Task<bool> IsDatabaseAvailableAsync();

        /// <summary>
        /// التحقق من توفر قاعدة البيانات بناءً على النوع
        /// </summary>
        /// <param name="databaseType">نوع قاعدة البيانات</param>
        /// <returns>صحيح إذا كانت قاعدة البيانات متاحة</returns>
        Task<bool> IsDatabaseAvailableAsync(string databaseType);

        /// <summary>
        /// الحصول على معلومات قاعدة البيانات
        /// </summary>
        /// <returns>معلومات قاعدة البيانات</returns>
        Task<DatabaseInfo> GetDatabaseInfoAsync();

        /// <summary>
        /// تبديل قاعدة البيانات يدوياً
        /// </summary>
        /// <param name="databaseType">نوع قاعدة البيانات المطلوب</param>
        void SwitchDatabase(string databaseType);

        /// <summary>
        /// تبديل تلقائي لقاعدة البيانات عند الفشل
        /// </summary>
        /// <returns>صحيح إذا تم التبديل بنجاح</returns>
        Task<bool> TryAutoSwitchOnFailureAsync();
    }

    /// <summary>
    /// معلومات قاعدة البيانات
    /// </summary>
    public class DatabaseInfo
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public string Version { get; set; } = string.Empty;
    }
} 