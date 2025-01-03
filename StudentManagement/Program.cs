
using StudentManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Data;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity with custom user model and role
builder.Services.AddIdentity<RegistrationModel, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";         // Redirect to Login page
    options.LogoutPath = "/Account/Logout";      // Redirect to Logout page
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect on Access Denied

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always; // Use Secure Cookies
});

// Add session services with configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180); 
    options.Cookie.HttpOnly = true;                // Prevent client-side script access
    options.Cookie.IsEssential = true;             // Ensure the session cookie is always set
});



// Add MVC with controllers and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// Configure EmailSender with settings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Seed roles and initial data
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync(); // Ensure database is updated
    SeedData.SeedGrades(context); // Seed grade data
    await SeedData.SeedRole(scope.ServiceProvider); // Seed roles
}

// Configure middleware for error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Enforce HTTPS
app.UseStaticFiles();      // Serve static files

app.UseRouting();          // Enable routing

app.UseAuthentication();   // Enable authentication
app.UseAuthorization();    // Enable authorization

app.UseSession();          // Enable session handling

app.UseEndpoints(endpoints =>
{
    // Default route to Account/Login
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    // Map Razor Pages (e.g., Identity scaffolding)
    endpoints.MapRazorPages();
});

// Run the application
app.Run();
