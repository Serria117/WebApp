using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApp;
using WebApp.Core.Data;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;

// Declare variables.
var builder = WebApplication.CreateBuilder(args);
var service = builder.Services;
var config = builder.Configuration;
var jwtKey = config["JwtSettings:SecretKey"];

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
service.AddDbContext<AppDbContext>(op =>
{
    op.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

service.AddAuthentication(options =>
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
    //ops.OperationFilter<AddAuthorizationHeaderOperationFilter>();
});

/* Add mapper services */
service.AddAutoMapper(
    typeof(UserMapper),
    typeof(RoleMapper)
);

/* Add application services */
service.AddHttpContextAccessor();
service.AddSingleton<JwtService>();
service.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
service.AddScoped<IUserService, UserAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

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