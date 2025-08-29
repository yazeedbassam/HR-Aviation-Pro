using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess;
using System.Security.Claims;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using WebApplication1.Services;

public class NotificationCountViewComponent : ViewComponent
{
    private readonly SqlServerDb _db;
    private readonly ILicenseNotificationService _licenseNotificationService;

    public NotificationCountViewComponent(SqlServerDb db, ILicenseNotificationService licenseNotificationService)
    {
        _db = db;
        _licenseNotificationService = licenseNotificationService;
    }

    public IViewComponentResult Invoke()
    {
        // استخدام User بدلاً من UserClaimsPrincipal للوصول إلى بيانات المستخدم المصادق عليه
        if (!User.Identity.IsAuthenticated)
        {
            return View((object)0); // إرجاع 0 إذا لم يكن المستخدم مصادقًا
        }

        //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier); // <== تم التعديل هنا

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return View((object)0); // إرجاع 0 إذا لم يتم العثور على معرف المستخدم أو كان غير صالح
        }

        int notificationCount = 0; // تهيئة عدد الإشعارات

        // استخدام SqlConnection و SqlCommand و SqlParameter
        // نفترض أن _db.GetConnection() ترجع SqlConnection
        using (var connection = _db.GetConnection())
        {
            connection.Open();
            // تم تعديل SQL: :userId إلى @userId
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM notifications WHERE userid = @userId AND is_read = 0", connection))
            {
                // تعريف المعامل باستخدام Microsoft.Data.SqlClient.SqlParameter
                cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@userId", SqlDbType.Int) { Value = userId }); // <== تم التعديل

                // استخدام ExecuteScalar لجلب قيمة واحدة (COUNT(*)) بكفاءة أكبر
                object result = cmd.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                {
                    notificationCount = Convert.ToInt32(result);
                }
            }
        }

        // إضافة عداد الإشعارات الجديدة للمراقبين الذين يحتاجون رخص
        int controllersNeedingLicensesCount = _licenseNotificationService.GetControllersNeedingLicensesCount();
        int inactiveControllersCount = _licenseNotificationService.GetInactiveControllersCount();

        // إجمالي عدد الإشعارات
        int totalNotifications = notificationCount + controllersNeedingLicensesCount + inactiveControllersCount;

        return View((object)totalNotifications); // إرجاع الـ View مع عدد الإشعارات
    }
}