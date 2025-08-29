using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add health checks
builder.Services.AddHealthChecks();

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
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    opt.AddPolicy("RequireController", p => p.RequireRole("Controller", "Admin"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure application to handle startup gracefully
app.Lifetime.ApplicationStarted.Register(() => {
    Console.WriteLine("Application has started successfully!");
    Console.WriteLine("Health check endpoints are ready!");
    Console.WriteLine("Available endpoints: /, /health, /ping, /ready");
});

// Test database connection on startup (only in development)
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(async () => {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
            var connection = db.GetConnection();
            await connection.OpenAsync();
            Console.WriteLine("Database connection successful!");
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Database connection failed: {ex.Message}");
            Console.WriteLine("Application will continue without database functionality");
        }
    });
}

// Log environment variables for debugging
app.Lifetime.ApplicationStarted.Register(() => {
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine($"DB_SERVER: {Environment.GetEnvironmentVariable("DB_SERVER") ?? "NOT SET"}");
    Console.WriteLine($"DB_NAME: {Environment.GetEnvironmentVariable("DB_NAME") ?? "NOT SET"}");
    Console.WriteLine($"DB_USER: {Environment.GetEnvironmentVariable("DB_USER") ?? "NOT SET"}");
    var passwordStatus = Environment.GetEnvironmentVariable("DB_PASSWORD") != null ? "SET" : "NOT SET";
    Console.WriteLine($"DB_PASSWORD: {passwordStatus}");
    Console.WriteLine($"DB_PORT: {Environment.GetEnvironmentVariable("DB_PORT") ?? "NOT SET"}");
    
    // Check if we're in production and database is not configured
    if (app.Environment.IsProduction())
    {
        var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
        if (string.IsNullOrEmpty(dbServer))
        {
            Console.WriteLine("⚠️ WARNING: Database environment variables are not set in production!");
            Console.WriteLine("⚠️ The application will not be able to connect to the database.");
            Console.WriteLine("⚠️ Please configure the following environment variables in Railway:");
            Console.WriteLine("   - DB_SERVER");
            Console.WriteLine("   - DB_NAME");
            Console.WriteLine("   - DB_USER");
            Console.WriteLine("   - DB_PASSWORD");
            Console.WriteLine("   - DB_PORT");
        }
    }
});

// Seed Admin user (only in development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<SqlServerDb>();
            if (db.GetUserByUsername("admin") == null)
            {
                db.CreateUser("admin", "123", "Admin");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not seed admin user: {ex.Message}");
        }
    }
}

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
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoints FIRST - before any other routing
app.MapGet("/ping", () => Results.Text("pong", "text/plain"));
app.MapGet("/ready", () => Results.Text("ready", "text/plain"));
app.MapGet("/health", () => Results.Json(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/", () => Results.Text("AVIATION HR PRO - System is running!", "text/plain"));

// Map controllers AFTER health check endpoints
app.MapControllers();

// Add MVC routing for traditional controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

