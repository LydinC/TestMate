using Serilog;
using TestMate.WEB.Controllers;
using TestMate.WEB.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHandler>();

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

builder.Services.AddHttpClient<DevelopersController>("DevelopersClient",
    client => client.BaseAddress = new Uri(configuration["APIBaseUrl"] + "Developers"))
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<TestRequestsController>("TestRequestsClient",
    client => client.BaseAddress = new Uri(configuration["APIBaseUrl"] + "TestRequests"))
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<TestRequestsController>("TestRunsClient",
    client => client.BaseAddress = new Uri(configuration["APIBaseUrl"] + "TestRuns"))
    .AddHttpMessageHandler<TokenHandler>();

//builder.Services.AddHttpClient<AuthenticationHelper>("AuthenticationClient",
//    client => client.BaseAddress = new Uri(configuration["APIBaseUrl"] + "Authenticate"))
//    .AddHttpMessageHandler<TokenHandler>();

//Logging Configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
});

builder.Services.AddControllers();

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


//Authentication and Authorisation are done at API layer
//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
