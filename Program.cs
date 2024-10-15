using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestSharp;
using WebApp;
using WebApp.Authentication;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Mongo;
using WebApp.Register;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.RestService;
using WebApp.SignalrConfig;
using Serilog;

// Declare variables.
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var jwtKey = config["JwtSettings:SecretKey"];

var restSettings = config.GetSection("RestSharp").Get<RestSharpSetting>()!;
var mongoSettings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
var origins = config.GetSection("AllowedOrigins").Get<string[]>() ?? [];

//Config logging
Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .WriteTo.File(
                 path: "logs/log-.txt",        // Log file path with rolling logs
                 rollingInterval: RollingInterval.Day,  // Roll log files daily
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information // Minimum level to log
             )
             .CreateLogger();

// Add services to the container.
services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"));
    // options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
});

builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.ReferenceHandler =
               System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });

services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JwtSettings:Issuer"],
                ValidAudience = config["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                NameClaimType = "name"
            };
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/progressHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

// Custom authorization handlers:
services.AddAuthorization();
services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

services.AddSignalR();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(ops =>
{
    ops.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "App",
        Version = "1.0"
    });
    ops.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT bearer authentication",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    ops.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                /*Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,*/
            },
            Array.Empty<string>()
        }
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    ops.IncludeXmlComments(xmlPath);
});

services.AddSingleton(restSettings);
services.AddSingleton<IRestClient>(new RestClient(new RestClientOptions(restSettings.BaseUrl)));

/* Add mapper services */
/*services.AddAutoMapper(typeof(UserMapper), typeof(RoleMapper),
                       typeof(OrgMapper), typeof(PagedMapper), typeof(RegionMapper));*/

services.AddSingleton<CustomMap>();

services.AddHttpContextAccessor();
services.AddSingleton<JwtService>();

/* Add application services */
services.AddAppServices();
services.AddMongoServices(mongoSettings);

builder.Host.UseSerilog();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    SeedPermissions(context);
}

// Configure the HTTP request pipeline.
app.UseCors(op =>
{
    op.WithOrigins(origins);
    op.AllowAnyMethod();
    op.AllowCredentials();
    op.WithExposedHeaders("X-Filename"); //custom header for client to access
    op.AllowAnyHeader();
    op.Build();
});

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<AppHub>("/progressHub");

app.Run();
return;

void SeedPermissions(AppDbContext context)
{
    var existingPermissions = context.Permissions.Select(p => p.PermissionName).ToHashSet();
    var defaultPermissions = PermissionSeeder.GetDefaultPermissions();

    foreach (var permission in defaultPermissions.Where(permission => !existingPermissions.Contains(permission)))
    {
        context.Permissions.Add(new Permission { PermissionName = permission });
    }

    context.SaveChanges();
}