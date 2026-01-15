using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Email;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;
using ILogger = TutorLinkApp.Services.Interfaces.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TutorLinkContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// User-related
builder.Services.AddSingleton<ILogger>(AppLogger.GetInstance());
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISessionManager, SessionManager>();

// Admin-related
builder.Services.AddScoped<IAdminStatsService, AdminStatsService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAdminUserCreationService, AdminUserCreationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Tutor-related
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<TutorService>();
builder.Services.AddScoped<ITutorService>(provider =>
new LoggingTutorServiceDecorator(
provider.GetRequiredService<TutorService>(),
provider.GetRequiredService<ILogger>()
)
);

// Email-related
builder.Services.AddScoped<FakeEmailSender>();
builder.Services.AddScoped<IEmailSender>(sp => EmailSenderFactory.Create(sp));
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IResetPasswordFacade, ResetPasswordFacade>();

builder.Services.AddDistributedMemoryCache();

// Session with security
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// HSTS Configuration
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Error handling and HSTS
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.Use(async (context, next) =>
{
    // HSTS Header - dodaj OVDJE
    context.Response.Headers["Strict-Transport-Security"] =
        "max-age=31536000; includeSubDomains; preload";

    // CSP Header
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net; " +
        "style-src 'self'; " +  // Bez unsafe-inline
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    // Cache Control
    if (context.Request.Path.StartsWithSegments("/css") ||
        context.Request.Path.StartsWithSegments("/js") ||
        context.Request.Path.StartsWithSegments("/lib"))
    {
        context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
    }
    else
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }

    // MIME sniffing prevention
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    await next();
});

app.UseCookiePolicy();
app.UseHttpsRedirection();
app.UseStaticFiles();  // UKLONI OnPrepareResponse - nije potreban više
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();