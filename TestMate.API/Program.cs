using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TestMate.API.JWTAuthentication;
using TestMate.API.Profiles;
using TestMate.API.Services;
using TestMate.API.Settings;
using Serilog.AspNetCore;
using Serilog;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Database Settings Reference
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("TestFrameworkDatabase"));

//Services
builder.Services.AddSingleton<DevicesService>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<DevelopersService>();
builder.Services.AddSingleton<TestRequestsService>();
builder.Services.AddSingleton<JWTAuthenticationService>();


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


builder.Services.AddAutoMapper(typeof(TestRequestProfile));

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null); //property names in the web API's serialized JSON response match their corresponding property names in the CLR object type. For example, the Book class's Author property serializes as Author instead of author


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "issuer",
            ValidAudience = "audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisisalongkeyforjwtauthentication"))
        };
        //TODO: Update the above to get values of issuer, audience and secret key from ICONFIGURATION
    });


//Adding scopes to implement [Authorize]
//builder.Services.AddScoped<JWTAuthenticationService>();
//builder.Services.AddScoped<JWTTokenValidationMiddleware>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<JWTTokenValidationMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();