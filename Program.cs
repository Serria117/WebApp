using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using WebApp;
using WebApp.Authentication;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService;
using WebApp.Services.UserService;

// Declare variables.
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var jwtKey = config["JwtSettings:SecretKey"];
var mongoDbSettings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;


string[] origins =
[
    "http://localhost:8888",
    "http://localhost:8080",
    "http://localhost:4200",
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://127.0.0.1:4200",
    "http://14.225.19.135:4200",
    "http://14.225.19.135:5173"
];

// Add services to the container.
services.AddDbContext<AppDbContext>(op =>
{
    op.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"));
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
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidAudience = config["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            NameClaimType = "name"
        };
    });

// Custom authorization handlers:
services.AddAuthorization();
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
    ops.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
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
            new string[]{}
        }
    });
});

services.AddSingleton(mongoDbSettings);
services.AddSingleton<IMongoClient, MongoClient>(sp =>
        new MongoClient(mongoDbSettings?.ConnectionString));

services.AddScoped<IMongoDatabase>(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings?.DatabaseName));

/* Add mapper services */
services.AddAutoMapper(
    typeof(UserMapper),
    typeof(RoleMapper),
    typeof(OrgMapper)
    );

/* Add application services */
services.AddHttpContextAccessor();
services.AddSingleton<JwtService>();
services.AddScoped<IMongoRepository, MongoRepository>();
services.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
services.AddTransient<IUserService, UserAppService>();
services.AddTransient<IRoleAppService, RoleAppService>();
services.AddTransient<IPermissionAppService, PermissionAppService>();
services.AddTransient<IOrganizationAppService, OrganizationAppService>();

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