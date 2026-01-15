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
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// HSTS Configuration
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    // Force correct content type for HTML responses
    if (!context.Response.Headers.ContainsKey("Content-Type"))
    {
        context.Response.ContentType = "text/html; charset=utf-8";
    }

    // Prevent MIME sniffing
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");

    // Optional but recommended
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});

// Error handling and HSTS
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();