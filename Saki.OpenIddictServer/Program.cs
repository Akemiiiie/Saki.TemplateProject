using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Saki.OpenIddictServer.Authorization;
using Saki.OpenIddictServer.Startups;
using Saki.RepositoryTemplate.Base;
using Saki.RepositoryTemplate.DBClients;
using Quartz;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// EF数据库上下文配置（需要升级到EF9.0否则无法正常使用EF，以及数据初始化功能，且会在初始化数据连接时报错）
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));
//
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<EFDbContext>();

// 启动后台任务用于监听请求是否合法
builder.Services.AddHostedService<HostedService>();

// 配置 Entity Framework Core 使用 Microsoft SQL Server。
builder.Services.AddDbContext<EFDbContext>(options =>
{
    // 配置 EfCore 使用 Microsoft SQL Server 数据库
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        builder => { builder.MigrationsAssembly("Saki.OpenIddictServer"); });

    // 注册OpenIddict所需的数据实体集。
    // 注意：如果你需要替换默认的OpenIddict实体，请使用泛型重载方法
    options.UseOpenIddict();
});
//
// var encryptionCert = new X509Certificate2(
//     "encryption.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable); // 导出权限可选
// var signingCert = new X509Certificate2(
//     "signing.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable);

builder.Services.AddOpenIddict().AddCore(options =>
    {
        // 配置 OpenIddict 使用 Entity Framework Core 存储和模型。
        // 注意：调用 ReplaceDefaultEntities() 来替换默认实体。
        options.UseEntityFrameworkCore()
            .UseDbContext<EFDbContext>();
        options.UseQuartz();
    })
    // 注册 OpenIddict 服务器组件
    .AddServer(options =>
    {
        // 令牌端点
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token");
        options.RegisterScopes("service-worker"); // 必须与资源服务器的Audience一致
        // options.SetLogoutEndpointUris("/connect/logout"); // 登出端点
        // options.AcceptAnonymousClients();              // 允许公共客户端
        options.AllowPasswordFlow()                    // 允许密码授权流程
            .AllowClientCredentialsFlow();              // 启用客户端凭据流
        // options.AcceptAnonymousClients();              // 允许公共客户端

        // 注册签名和加密证书
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        // options.AddEncryptionCertificate(encryptionCert);
        // options.AddSigningCertificate(signingCert);

        // 注册 ASP.NET Core 主机并且配置 ASP.NET Core 选项
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
        // 不使用证书对JWT进行加密（仅在测试时使用，用于解密jwt查看对应内容进行调试）
        //options.DisableAccessTokenEncryption();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
            // 允许在授权端点中使用自定义逻辑
            .EnableAuthorizationEndpointPassthrough()
            // 允许在令牌端点中使用自定义逻辑
            .EnableTokenEndpointPassthrough()
            // 集成UseStatusCodePages
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        // 从本地 OpenIddict 服务器实例导入配置
        options.UseLocalServer();
        // 注册host主机
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
        // 跨域配置，根据生成具体需要进行配置
        policy.WithOrigins("https://localhost:7002", "https://localhost:7003", "https://oidcdebugger.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(); // Optional: For API testing

builder.Services.AddTransient<AuthorizationService>(); // Register custom service
builder.Services.AddTransient<ClientsSeeder>();      // Register seeder

// 添加MVC中间件,不添加会导致无法正常访问接口
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

// // 配置路由
// app.MapControllerRoute(
//     "default",
//     "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages()
//     .WithStaticAssets();

app.Run();