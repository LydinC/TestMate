using System.Net.Http;
using TestMate.WEB.Services;
using TestMate.WEB.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IUsersService, UsersService>(c =>
c.BaseAddress = new Uri("https://localhost:7112/api/users"));
builder.Services.AddHttpClient<IDevelopersService, DevelopersService>(c =>
c.BaseAddress = new Uri("https://localhost:7112/api/developers"));
builder.Services.AddHttpClient<ITestRequestsService, TestRequestsService>(c =>
c.BaseAddress = new Uri("https://localhost:7112/api/testrequests"));

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
