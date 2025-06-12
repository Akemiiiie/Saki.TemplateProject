using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Panda.DynamicWebApi;
using Saki.RepositoryTemplate.Base;
using Saki.TemplateWebProject.v1.Startups;
using Saki.TemplateWebProject.v2.Data;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 读取配置文件

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json", true, true);
IConfiguration configRoot = configBuilder.Build();
configRoot.GetSection("ConnectionStrings").Get<BaseDbConfig>();

// EF数据库上下文配置（需要升级到EF9.0否则无法正常使用EF，以及数据初始化功能，且会在初始化数据连接时报错）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

// Autofac自动注入(此处会注入数据库上下文的数据库连接)
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

// 添加MVC中间件,不添加会导致无法正常访问接口
builder.Services.AddMvc();
// 动态api
builder.Services.AddDynamicWebApi();
// 添加swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Saki's Project Template",
        Version = "v1",
        Description = "这是一个用于构建工程的.netCore mvc项目模板",
        Contact = new OpenApiContact { Name = "Saki'CoreTemplate", Email = "2567241787@qq.com" }
    });
    options.DocInclusionPredicate((name, api) => api.HttpMethod != null);
    // 可选：为XML注释添加支持
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
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

app.UseHttpsRedirection();
app.UseStaticFiles();

// 启用swagger
app.UseSwagger();
// 启用中间件为Swagger UI提供服务
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "我的MVC应用API V1");
    // 可选：设置默认展开的API
    c.DocExpansion(DocExpansion.None);
});

app.UseRouting();
app.UseAuthorization();
// 配置路由
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages()
    .WithStaticAssets();
app.Run();
