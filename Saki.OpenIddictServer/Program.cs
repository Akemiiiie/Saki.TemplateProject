using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Saki.OpenIddictServer.Startups;
using Saki.RepositoryTemplate.Base;
using Saki.RepositoryTemplate.DBClients;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// EF数据库上下文配置（需要升级到EF9.0否则无法正常使用EF，以及数据初始化功能，且会在初始化数据连接时报错）
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<EFDbContext>();

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
    })
    // 注册 OpenIddict 服务器组件
    .AddServer(options =>
    {
        options.RegisterScopes("service-worker"); // 必须与资源服务器的Audience一致
        options.SetTokenEndpointUris("/connect/token"); // 令牌端点
        options.AcceptAnonymousClients();              // 允许公共客户端
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
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

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
// app.UseHttpsRedirection();
// app.UseStaticFiles();
//
// app.UseHttpsRedirection();
// app.UseStaticFiles();

app.UseDeveloperExceptionPage();
app.UseForwardedHeaders();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

// // 配置路由
// app.MapControllerRoute(
//     "default",
//     "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages()
//     .WithStaticAssets();

app.Run();