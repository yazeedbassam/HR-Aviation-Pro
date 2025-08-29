using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Microsoft.Data.SqlClient; // تأكد من استخدام هذا الـ namespace
using System.Data;             // مطلوب لـ DataTable
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SqlServerDb>();
builder.Services.AddSingleton<IPasswordHasher<ControllerUser>, PasswordHasher<ControllerUser>>();
builder.Services.AddHostedService<LicenseExpiryNotificationService>();
builder.Services.AddScoped<LicenseExpiryNotificationService>();

// Add Configuration Service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Add Permission Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAdvancedPermissionService, AdvancedPermissionService>();

// Add License Notification Service
builder.Services.AddScoped<ILicenseNotificationService, LicenseNotificationService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // مثال لمدة انتهاء الصلاحية
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthorization(opt => {
    opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();
    opt.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    opt.AddPolicy("RequireController", p => p.RequireRole("Controller", "Admin"));
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2) Seed Admin user
using (var scope = app.Services.CreateScope())
{
    // Database service is already configured
    var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();

    if (db.GetUserByUsername("admin") == null)
    {
        db.CreateUser("admin", "123", "Admin");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

