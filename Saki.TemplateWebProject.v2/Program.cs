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
// ��ȡ�����ļ�

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json", true, true);
IConfiguration configRoot = configBuilder.Build();
configRoot.GetSection("ConnectionStrings").Get<BaseDbConfig>();

// EF���ݿ����������ã���Ҫ������EF9.0�����޷�����ʹ��EF���Լ����ݳ�ʼ�����ܣ��һ��ڳ�ʼ����������ʱ����
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

// Autofac�Զ�ע��(�˴���ע�����ݿ������ĵ����ݿ�����)
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

// ���MVC�м��,����ӻᵼ���޷��������ʽӿ�
builder.Services.AddMvc();
// ��̬api
builder.Services.AddDynamicWebApi();
// ���swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Saki's Project Template",
        Version = "v1",
        Description = "����һ�����ڹ������̵�.netCore mvc��Ŀģ��",
        Contact = new OpenApiContact { Name = "Saki'CoreTemplate", Email = "2567241787@qq.com" }
    });
    options.DocInclusionPredicate((name, api) => api.HttpMethod != null);
    // ��ѡ��ΪXMLע�����֧��
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

// ����swagger
app.UseSwagger();
// �����м��ΪSwagger UI�ṩ����
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "�ҵ�MVCӦ��API V1");
    // ��ѡ������Ĭ��չ����API
    c.DocExpansion(DocExpansion.None);
});

app.UseRouting();
app.UseAuthorization();
// ����·��
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages()
    .WithStaticAssets();
app.Run();
