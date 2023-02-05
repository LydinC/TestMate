using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TestMate.API.Profiles;
using TestMate.API.Services;
using TestMate.API.Settings;

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
builder.Services.AddAutoMapper(typeof(DeveloperProfile));
builder.Services.AddAutoMapper(typeof(DevicesProfile));



builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null); //property names in the web API's serialized JSON response match their corresponding property names in the CLR object type. For example, the Book class's Author property serializes as Author instead of author


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWTAuthentication:Issuer"],
            ValidAudience = configuration["JWTAuthentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTAuthentication:SecretKey"]))
        };
    });


//Not required any more as JWT Authentication Default bearer is being used as defined above
//builder.Services.AddScoped<JWTAuthenticationService>();
//builder.Services.AddScoped<JWTTokenValidationMiddleware>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "TestMate.API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<JWTTokenValidationMiddleware>();

//TODO: Switched off for now until certificates are settled (using http)
//app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();