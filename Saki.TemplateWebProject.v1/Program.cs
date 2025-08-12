using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Panda.DynamicWebApi;
using Saki.BaseTemplate.ConfigerOptions;
using Saki.TemplateWebProject.v1.Startups;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json", true, true);
IConfiguration configRoot = configBuilder.Build();
configRoot.GetSection("BaseDBConfig").Get<BaseDbConfig>();

// 自动注册
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

builder.Services.AddMvc();
builder.Services.AddDynamicWebApi();
// Add services to the container.
// builder.Services.AddRazorPages();
// builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Saki's Project Template",
        Version = "v1",
        Description = "这是一个基于.NET Core MVC架构的项目模板",
        Contact = new OpenApiContact { Name = "Saki'CoreTemplate", Email = "2567241787@qq.com" }
    });
    options.DocInclusionPredicate((name, api) => api.HttpMethod != null);
    // 为XML注释提供支持
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// 配置中间件为Swagger JSON提供路由
app.UseSwagger();

// 配置中间件为Swagger UI提供服务
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "我的MVC应用API V1");
    // 为选择性展开API提供支持
    c.DocExpansion(DocExpansion.None);
});

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();