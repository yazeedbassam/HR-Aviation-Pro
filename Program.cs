using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Services;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// تعيين البيئة بشكل صريح
builder.Environment.EnvironmentName = "Development";

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// تسجيل معلومات البيئة والتكوين
Console.WriteLine($"🔧 Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"🔧 Content Root: {builder.Environment.ContentRootPath}");
Console.WriteLine($"🔧 Application Name: {builder.Environment.ApplicationName}");

// Add health checks
builder.Services.AddHealthChecks();

// Configure Data Protection for production
if (!builder.Environment.IsDevelopment())
{
    // Use file system for key storage in production
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
        .SetApplicationName("HR-Aviation");
}

// builder.Services.AddSingleton<MySqlDb>();
builder.Services.AddSingleton<IPasswordHasher<ControllerUser>, PasswordHasher<ControllerUser>>();
builder.Services.AddSingleton<SqlServerDb>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<ControllerUser>>();
    var logger = serviceProvider.GetRequiredService<ILogger<SqlServerDb>>();
    return new SqlServerDb(configuration, passwordHasher, logger);
});

// تعليق خدمة الإيميلات الأوتوماتيكية
// builder.Services.AddHostedService<LicenseExpiryNotificationService>();
builder.Services.AddScoped<LicenseExpiryNotificationService>();

// إضافة خدمة مسح الكاش التلقائي
builder.Services.AddHostedService<AutoCacheClearService>();

// Add Configuration Service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Add Cache Service
builder.Services.AddScoped<ICacheService, CacheService>();

// Add Email Service
builder.Services.Configure<EmailConfiguration>(options =>
{
    // إعدادات مباشرة بدلاً من قراءة من ملف التكوين
    options.SmtpServer = "smtp-relay.brevo.com";
    options.SmtpPort = 587;
    options.Username = "8e2caf001@smtp-brevo.com";
    options.Password = "3HzgVG7nwKMxqcA2";
    options.FromEmail = "yazeedbassam1987@gmail.com";
    options.FromName = "HR Aviation System";
    options.EnableSsl = true;
    options.Timeout = 30000;
    options.MaxRetries = 3;
    options.RetryDelay = 1000;
    options.EnableLogging = true;
    
    // تسجيل الإعدادات النهائية
    Console.WriteLine($"🔧 Final Email Configuration:");
    Console.WriteLine($"   SMTP Server: {options.SmtpServer}");
    Console.WriteLine($"   SMTP Port: {options.SmtpPort}");
    Console.WriteLine($"   Username: {options.Username}");
    Console.WriteLine($"   From Email: {options.FromEmail}");
    Console.WriteLine($"   From Name: {options.FromName}");
    Console.WriteLine($"   SSL: {options.EnableSsl}");
    Console.WriteLine($"   Timeout: {options.Timeout}ms");
});

builder.Services.AddScoped<IEmailService, EmailService>();

// Add Logger Service
builder.Services.AddScoped<ILoggerService, LoggerService>();

// Add Permission Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAdvancedPermissionService, AdvancedPermissionService>();
builder.Services.AddScoped<IAdvancedPermissionManagerService, AdvancedPermissionManagerService>();

// Add License Notification Service
builder.Services.AddScoped<ILicenseNotificationService, LicenseNotificationService>();

// Add Smart Database Service
builder.Services.AddScoped<IDatabaseService, SmartDatabaseService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    opt.AddPolicy("RequireController", p => p.RequireRole("Controller", "Admin"));
});

builder.Services.AddControllersWithViews();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure application to handle startup gracefully
app.Lifetime.ApplicationStarted.Register(() => {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 Application has started successfully!");
    logger.LogInformation("🔍 Health check endpoints are ready!");
    logger.LogInformation("📍 Available endpoints: /, /health, /ping, /ready");
});

// Enhanced database connection testing
app.Lifetime.ApplicationStarted.Register(async () => {
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("🔍 Testing SQL Server database connection...");
        var isAvailable = db.IsDatabaseAvailable();
        
        if (isAvailable)
        {
            logger.LogInformation("✅ Database connection successful!");
            
            // Test basic operations
            try
            {
                // Check which table exists and count users
                var usersExists = db.ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Users'");
                var controllersExists = db.ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Controllers'");
                
                if (Convert.ToInt32(usersExists) > 0)
                {
                    var userCount = db.ExecuteScalar("SELECT COUNT(*) FROM Users");
                    logger.LogInformation("📊 Database contains {UserCount} users (from Users table)", userCount);
                }
                else if (Convert.ToInt32(controllersExists) > 0)
                {
                    var userCount = db.ExecuteScalar("SELECT COUNT(*) FROM Controllers");
                    logger.LogInformation("📊 Database contains {UserCount} users (from Controllers table)", userCount);
                }
                else
                {
                    logger.LogInformation("📊 No user tables found in database");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("⚠️ Database query test failed: {Message}", ex.Message);
            }
        }
        else
        {
            logger.LogWarning("❌ Database connection failed!");
            logger.LogWarning("⚠️ Application will run with limited functionality");
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError("❌ Database service initialization failed: {Message}", ex.Message);
        logger.LogWarning("⚠️ Application will run with limited functionality");
    }
});

// Enhanced environment variable logging
app.Lifetime.ApplicationStarted.Register(() => {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🌍 Environment: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("🔧 ASPNETCORE_ENVIRONMENT: {ASPNETCORE_ENVIRONMENT}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT SET");
    logger.LogInformation("🔧 ASPNETCORE_URLS: {ASPNETCORE_URLS}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "NOT SET");
    logger.LogInformation("🔧 PORT: {PORT}", Environment.GetEnvironmentVariable("PORT") ?? "NOT SET");
    
    // Database environment variables
    var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
    
    logger.LogInformation("🗄️ DB_SERVER: {DB_SERVER}", dbServer ?? "NOT SET");
    logger.LogInformation("🗄️ DB_NAME: {DB_NAME}", dbName ?? "NOT SET");
    logger.LogInformation("🗄️ DB_USER: {DB_USER}", dbUser ?? "NOT SET");
    logger.LogInformation("🗄️ DB_PASSWORD: {DB_PASSWORD}", (dbPassword != null ? "SET" : "NOT SET"));
    logger.LogInformation("🗄️ DB_PORT: {DB_PORT}", dbPort ?? "NOT SET");
    
    // Check if we're in production and database is not configured
    if (app.Environment.IsProduction())
    {
        if (string.IsNullOrEmpty(dbServer) || string.IsNullOrEmpty(dbName) || 
            string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword))
        {
            logger.LogWarning("⚠️ WARNING: Database environment variables are not set in production!");
            logger.LogWarning("⚠️ The application will not be able to connect to the database.");
            logger.LogWarning("⚠️ Please configure the following environment variables in Railway:");
            logger.LogWarning("   - DB_SERVER (e.g., your-sql-server.database.windows.net)");
            logger.LogWarning("   - DB_NAME (e.g., hr_aviation_db)");
            logger.LogWarning("   - DB_USER (e.g., your_username)");
            logger.LogWarning("   - DB_PASSWORD (e.g., your_password)");
            logger.LogWarning("   - DB_PORT (e.g., 1433)");
            logger.LogWarning("⚠️ You can set these in Railway Dashboard > Your Project > Variables");
        }
        else
        {
            logger.LogInformation("✅ Database environment variables are properly configured!");
        }
    }
});

// Seed Admin user (only in development or if database is available)
app.Lifetime.ApplicationStarted.Register(async () => {
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Check if database is available
        if (db.IsDatabaseAvailable())
        {
            if (app.Environment.IsDevelopment())
            {
                if (db.GetUserByUsername("admin") == null)
                {
                    db.CreateUser("admin", "123", "Admin");
                    logger.LogInformation("👤 Admin user created successfully");
                }
                else
                {
                    logger.LogInformation("👤 Admin user already exists");
                }
            }
        }
        else
        {
            logger.LogWarning("⚠️ Cannot seed admin user - database not available");
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("⚠️ Could not seed admin user: {Message}", ex.Message);
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
    // Disable detailed error pages in production
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
}

// Only use HTTPS redirection in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Add Session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// إضافة Middleware لمسح الكاش تلقائياً
app.UseMiddleware<AutoCacheClearMiddleware>();

// Map health check endpoints FIRST - before any other routing
app.MapGet("/ping", () => Results.Text("pong", "text/plain"));
app.MapGet("/ready", () => Results.Text("ready", "text/plain"));
app.MapGet("/health", () => {
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
        var dbAvailable = db.IsDatabaseAvailable();
        
        return Results.Json(new { 
            status = dbAvailable ? "healthy" : "degraded",
            database = dbAvailable ? "connected" : "disconnected",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        });
    }
    catch
    {
        return Results.Json(new { 
            status = "unhealthy",
            database = "error",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        });
    }
});
app.MapGet("/", () => Results.Redirect("/Home/Index"));

// Map controllers AFTER health check endpoints
app.MapControllers();

// Add MVC routing for traditional controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

