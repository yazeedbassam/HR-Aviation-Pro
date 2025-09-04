using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess;
using System.Security.Claims;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using WebApplication1.Services;

public class NotificationCountViewComponent : ViewComponent
{
    private readonly SmartDatabaseService _smartDbService;
    private readonly ILicenseNotificationService _licenseNotificationService;

    public NotificationCountViewComponent(SmartDatabaseService smartDbService, ILicenseNotificationService licenseNotificationService)
    {
        _smartDbService = smartDbService;
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

        try
        {
            // استخدام SmartDatabaseService للحصول على عدد الإشعارات
            notificationCount = _smartDbService.GetNotificationCount(userId);
        }
        catch (Exception ex)
        {
            // في حالة فشل الاتصال بقاعدة البيانات، إرجاع 0
            notificationCount = 0;
        }

        // إضافة عداد الإشعارات الجديدة للمراقبين والموظفين الذين يحتاجون رخص
        int controllersNeedingLicensesCount = _licenseNotificationService.GetControllersNeedingLicensesCount();
        int employeesNeedingLicensesCount = 0;
        int inactiveControllersCount = _licenseNotificationService.GetInactiveControllersCount();
        
        try
        {
            // استخدام SmartDatabaseService للحصول على عدد الموظفين الذين يحتاجون رخص
            employeesNeedingLicensesCount = _smartDbService.GetTotalNeedingLicensesCount() - controllersNeedingLicensesCount;
        }
        catch (Exception ex)
        {
            // في حالة فشل الاتصال بقاعدة البيانات، إرجاع 0
            employeesNeedingLicensesCount = 0;
        }

        // إجمالي عدد الإشعارات
        int totalNotifications = notificationCount + controllersNeedingLicensesCount + employeesNeedingLicensesCount + inactiveControllersCount;

        return View((object)totalNotifications); // إرجاع الـ View مع عدد الإشعارات
    }
}