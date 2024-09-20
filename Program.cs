using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RestSharp;
using WebApp;
using WebApp.Authentication;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.InvoiceService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService;
using WebApp.Services.RestService;
using WebApp.Services.UserService;
using WebApp.SignalrConfig;

// Declare variables.
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var jwtKey = config["JwtSettings:SecretKey"];
var mongoDbSettings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
var restSettings = config.GetSection("RestSharp").Get<RestSharpSetting>()!;

string[] origins =
[
    "http://localhost:8888",
    "http://localhost:8080",
    "http://localhost:4200",
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://127.0.0.1:4200",
    "http://14.225.19.135:8888",
    "http://14.225.19.135:5173",
    "http://localhost:24894"
];

// Add services to the container.
services.AddDbContext<AppDbContext>(op =>
{
    op.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"));
    //op.AddInterceptors(new AuditableEntityInterceptor());
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

services.AddSignalR();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ops =>
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
});

services.AddSingleton(mongoDbSettings);
services.AddSingleton<IMongoClient, MongoClient>(_ =>
        new MongoClient(mongoDbSettings.ConnectionString));

services.AddScoped<IMongoDatabase>(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));

services.AddSingleton(restSettings);
services.AddSingleton<IRestClient>(new RestClient(new RestClientOptions(restSettings.BaseUrl)));

/* Add mapper services */
services.AddAutoMapper(typeof(UserMapper), typeof(RoleMapper), 
    typeof(OrgMapper), typeof(PagedMapper));

/* Add application services */
services.AddHttpContextAccessor();
services.AddSingleton<JwtService>();
services.AddScoped<IMongoRepository, MongoRepository>();
services.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
services.AddTransient<IUserService, UserAppService>();
services.AddTransient<IRoleAppService, RoleAppService>();
services.AddTransient<IPermissionAppService, PermissionAppService>();
services.AddTransient<IOrganizationAppService, OrganizationAppService>();
services.AddScoped<IRestAppService, RestAppService>();
services.AddTransient<IInvoiceAppService, InvoiceAppService>();

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
    op.AllowAnyHeader();
    op.AllowAnyMethod();
    op.AllowCredentials();
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