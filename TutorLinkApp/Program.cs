using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<TutorLinkContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services (DI for SOLID setup)
builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<IAdminStatsService, AdminStatsService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
//builder.Services.AddScoped<IAdminUserCreationService, AdminUserCreationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITutorService, TutorService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAdminUserCreationService, AdminUserCreationService>();


// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
