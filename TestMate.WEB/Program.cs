using System.Net.Http;
using TestMate.WEB.Services;
using TestMate.WEB.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddHttpClient<IUsersService, UsersService>(c =>
//c.BaseAddress = new Uri("https://localhost:7112/api/users"));
//builder.Services.AddHttpClient<IDevelopersService, DevelopersService>(c =>
//c.BaseAddress = new Uri("https://localhost:7112/api/developers"));
//builder.Services.AddHttpClient<ITestRequestsService, TestRequestsService>(c =>
//c.BaseAddress = new Uri("https://localhost:7112/api/testrequests"));

builder.Services.AddHttpClient<IUsersService, UsersService>();
builder.Services.AddHttpClient<IDevelopersService, DevelopersService>();
builder.Services.AddHttpClient<ITestRequestsService, TestRequestsService>();

//Logging Configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});
var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}





app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
