//add a using statement for currency
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;

//Done: Update these namespaces to match your project name 
//Be sure to remove the []
using ChenKennethHW5.DAL;
using ChenKennethHW5.Models;

//create a web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Done: Add database on Azure so you have a connection string
//Done: Add a connection string here once you have created it on Azure
String connectionString = "Server=tcp:sp25kennethchen.database.windows.net,1433;Initial Catalog=ChenKenneth_HW5;Persist Security Info=False;User ID=MISAdmin;Password=Password123;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

//NOTE: This tells your application how to get a connection to the database
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

//NOTE: You need this line for including Identity in your project
builder.Services.AddDefaultIdentity<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});


//build the app
var app = builder.Build();

//These lines allow you to see more detailed error messages
app.UseDeveloperExceptionPage();
app.UseStatusCodePages();

//This line allows you to use static pages like style sheets and images
app.UseStaticFiles();

//This marks the position in the middleware pipeline where a routing decision
//is made for a URL.
app.UseRouting();

//TODO: (HW4 & Beyond) Once you have added Identity into your project, you will 
//need to uncomment these lines
app.UseAuthentication();
app.UseAuthorization();


//This allows the data annotations for currency to work on Macs
app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
    CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

    await next.Invoke();
});


//This method maps the controllers and their actions to a patter for
//requests that's known as the default route. This route identifies
//the Home controller as the default controller and the Index() action
//method as the default action. The default route also identifies a 
//third segment of the URL that's a parameter named id.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();