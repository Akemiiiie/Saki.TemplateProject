using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saki.OpenIddictServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// 配置 Entity Framework Core 使用 Microsoft SQL Server。
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // 配置 EfCore 使用 Microsoft SQL Server 数据库
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // 注册OpenIddict所需的数据实体集。
    // 注意：如果你需要替换默认的OpenIddict实体，请使用泛型重载方法
    options.UseOpenIddict();
});

// builder.Services.AddRazorPages();

// 注册 OpenIddict 核心组件。
builder.Services.AddOpenIddict().AddCore(options =>
{
    // 配置 OpenIddict 使用 Entity Framework Core 存储和模型。
    // 注意：调用 ReplaceDefaultEntities() 来替换默认实体。
    options.UseEntityFrameworkCore()
        .UseDbContext<ApplicationDbContext>();
});
builder.Services.AddOpenIddict()

    // 注册 OpenIddict 服务器组件
    .AddServer(options =>
    {
        // 启用 token 端点
        options.SetTokenEndpointUris("connect/token");

        // 启用客户端凭据流
        options.AllowClientCredentialsFlow();

        // 注册签名和加密凭证
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // 注册 ASP.NET Core 主机并且配置 ASP.NET Core 选项
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseDeveloperExceptionPage();
//
// app.UseHttpsRedirection();
// app.UseStaticFiles();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

app.Run();
