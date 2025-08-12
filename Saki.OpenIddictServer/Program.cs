using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Saki.OpenIddictServer.Authorization;
using Saki.OpenIddictServer.Startups;
using Saki.RepositoryTemplate.DBClients;
using Quartz;
using System.Security.Cryptography.X509Certificates;
using Saki.BaseTemplate.ConfigerOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// EF数据库连接配置，需要说明的是由于EF9.0开始无法使用EF自带数据初始化功能，故而在初始化的时候需要手动配置
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));
//
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<EFDbContext>();

// 后台任务服务用于检查是否合法
builder.Services.AddHostedService<HostedService>();

// 配置 Entity Framework Core 使用 Microsoft SQL Server数据库
builder.Services.AddDbContext<EFDbContext>(options =>
{
    // 配置 EfCore 使用 Microsoft SQL Server 数据库
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        builder => { builder.MigrationsAssembly("Saki.OpenIddictServer"); });

    // 注册OpenIddict所需的实体模型
    // 注意：这些配置需要替换默认的OpenIddict实体
    options.UseOpenIddict();
});
//
// var encryptionCert = new X509Certificate2(
//     "encryption.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable); // 权限可选
// var signingCert = new X509Certificate2(
//     "signing.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable);

builder.Services.AddOpenIddict().AddCore(options =>
    {
        // 配置 OpenIddict 使用 Entity Framework Core 存储模型
        // 注意：这些配置需要使用 ReplaceDefaultEntities() 替换默认实体
        options.UseEntityFrameworkCore()
            .UseDbContext<EFDbContext>();
        options.UseQuartz();
    })
    // 注册 OpenIddict 服务器组件
    .AddServer(options =>
    {
        // 授权端点
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token");
        options.RegisterScopes("service-worker"); // 注册资源范围，作为Audience之一
        // options.SetLogoutEndpointUris("/connect/logout"); // 注销端点
        // options.AcceptAnonymousClients();              // 接受匿名客户端
        options.AllowPasswordFlow()                    // 允许密码授权
            .AllowClientCredentialsFlow();              // 允许客户端凭证授权
        // options.AcceptAnonymousClients();              // 接受匿名客户端

        // 注册加密和签名证书
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        // options.AddEncryptionCertificate(encryptionCert);
        // options.AddSigningCertificate(signingCert);

        // 注册 ASP.NET Core 集成
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
        // 使用JWT作为访问令牌格式，开发时使用，生产时查看对应数据中的字段
        //options.DisableAccessTokenEncryption();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
            // 使用自定义授权端点
            .EnableAuthorizationEndpointPassthrough()
            // 使用自定义令牌端点
            .EnableTokenEndpointPassthrough()
            // 使用UseStatusCodePages
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        // 引用 OpenIddict 服务器组件
        options.UseLocalServer();
        // 注册host
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Authenticate"; // Redirect here if user needs to log in
    options.LogoutPath = "/connect/logout"; // Optional: Centralize logout
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = false;
});

builder.Services.AddAuthorization(options =>
{
    // Optional: Define authorization policies
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Optionally require specific scopes
        // policy.RequireClaim("scope", "api1");
    });
});
builder.Services.AddEndpointsApiExplorer();
// 4. Background Tasks (Quartz.NET for OpenIddict)
builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore(); // Use DB store in production if needed
});
// Register the Quartz.NET hosted service.

builder.Services.AddControllers(); // For AuthorizationController
builder.Services.AddRazorPages(); // Optional: For development
// builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // 允许的来源，添加或删除需要的
        policy.WithOrigins("https://localhost:7002", "https://localhost:7003", "https://oidcdebugger.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(); // Optional: For API testing

builder.Services.AddTransient<AuthorizationService>(); // Register custom service
builder.Services.AddTransient<ClientsSeeder>();      // Register seeder

// 配置MVC模块，否则无法调用接口
// builder.Services.AddRazorPages();
// builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//
app.UseHttpsRedirection();
app.UseStaticFiles();
//
// app.UseHttpsRedirection();
// app.UseStaticFiles();

app.UseDeveloperExceptionPage();
app.UseForwardedHeaders();

app.UseRouting();
app.UseCors(); // Apply CORS policy

app.UseAuthentication(); // Enable Authentication middleware
app.UseAuthorization();  // Enable Authorization middleware

app.MapControllers();
app.MapRazorPages();
app.MapDefaultControllerRoute();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ClientsSeeder>();
    await seeder.SeedAsync();
}

//// 配置路由
// app.MapControllerRoute(
//     "default",
//     "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages()
//     .WithStaticAssets();

app.Run();