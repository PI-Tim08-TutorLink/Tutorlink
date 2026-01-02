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

// Register your services (DI for SOLID setup)
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
builder.Services.AddScoped<ResetPasswordFacade>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
