﻿using MongoDB.Driver;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.InvoiceService;
using WebApp.Services.OrganizationService;
using WebApp.Services.RegionService;
using WebApp.Services.RestService;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;

namespace WebApp.Register;

public static class DependencyRegister
{
    public static void AddMongoServices(this IServiceCollection s, IConfiguration c)
    {
        var mongoDbSettings = c.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
        s.AddSingleton(mongoDbSettings);
        s.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(mongoDbSettings.ConnectionString));

        s.AddScoped<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>()
                                                               .GetDatabase(mongoDbSettings.DatabaseName));
        s.AddScoped<IInvoiceMongoRepository, InvoiceMongoRepository>();
        s.AddScoped<IUserMongoRepository, UserMongoRepository>();
    }

    public static void AddAppServices(this IServiceCollection s)
    {
        s.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
        s.AddScoped<IRestAppService, RestAppService>();
        
        s.AddTransient<IUserAppService, UserAppAppService>();
        s.AddTransient<IRoleAppService, RoleAppService>();
        s.AddTransient<IPermissionAppService, PermissionAppService>();
        s.AddTransient<IOrganizationAppService, OrganizationAppService>();
        s.AddTransient<IInvoiceAppService, InvoiceAppService>();
        s.AddTransient<IRegionAppService, RegionAppService>();
        s.AddTransient<IRiskCompanyAppService, RiskCompanyAppService>();
        s.AddTransient<IBalanceSheetAppService, BalanceSheetAppService>();
    }
}