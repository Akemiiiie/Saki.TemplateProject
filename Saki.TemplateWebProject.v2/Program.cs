using Autofac;
using Autofac.Extensions.DependencyInjection;
using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
using Consul;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Panda.DynamicWebApi;
using Saki.BaseTemplate.ConfigerOptions;
using Saki.MiniProfilerOption;
using Saki.RepositoryTemplate.DBClients;
using Saki.TemplateWebProject.v2.Startups;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ��ȡ��������������
builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().AddNamespace("Saki_DevDept1.globalsetting"); // ˽�������ռ�;
// ��ȡ�����������
builder.Configuration.GetSection("ConnectionStrings").Get<BaseDbConfig>();

// EF���ݿ����������ã���Ҫ������EF9.0�����޷�����ʹ��EF���Լ����ݳ�ʼ�����ܣ��һ��ڳ�ʼ����������ʱ����
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<EFDbContext>();

// Autofac�Զ�ע��(�˴���ע�����ݿ������ĵ����ݿ�����)
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

// ���MVC�м��,����ӻᵼ���޷��������ʽӿ�
builder.Services.AddRazorPages();
builder.Services.AddMvc();
// ��̬api
builder.Services.AddDynamicWebApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // ����jwt��Ȩ������
        options.SetIssuer("https://localhost:7001/"); // Auth server URL
        options.AddAudiences("service-worker"); // Must match the audience in the auth server
        // ���ÿͻ����Լ��ͻ�����Կ
        options.UseIntrospection().SetClientId("service-worker").SetClientSecret("388D45FA-B36B-4988-BA59-B187D329C207");

        // options.AddEncryptionCertificate();
        // ���öԳƼ�����Կ
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="))); // Use secure key management in production!

        // Register the System.Net.Http integration for remote validation/introspection.
        options.UseSystemNetHttp();
        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

// ��Consulע����ע��
builder.Services.ConsulRegister(builder.Configuration);

// builder.Services.Configure<ConsulOptions>(builder.Configuration.GetSection("Consul"));

// builder.Services.AddSingleton<IConsulClient>(_ =>
//     new ConsulClient(cfg =>
//     {
//         var opt = _.GetRequiredService<IOptions<ConsulOptions>>().Value;
//         cfg.Address = new Uri($"http://{opt.ConsulIP}:{opt.ConsulPort}");
//     })
// );
// builder.Services.AddHostedService<ConsulRegistrationService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


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
    options.AddSecurityDefinition(
        "oauth",
        new OpenApiSecurityScheme
        {
            Flows = new OpenApiOAuthFlows
            {
                // ֤���ȡ��ַ
                ClientCredentials = new OpenApiOAuthFlow
                {
                    Scopes = new Dictionary<string, string>
                    {
                        ["api"] = "api scope description"
                    },
                    TokenUrl = new Uri("https://localhost:7001/connect/token"),
                },
            },
            In = ParameterLocation.Header,
            Name = HeaderNames.Authorization,
            Type = SecuritySchemeType.OAuth2
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "oauth" },
                },
                new[] { "api" }
            }
        }
    );
    options.DocInclusionPredicate((name, api) => api.HttpMethod != null);
    // ��ѡ��ΪXMLע�����֧��
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// ��ӷ������м��
builder.Services.AddMiniProfiler(ProfilerDefaultOption.GetProfilerDefaultOption)
    // AddEntityFramework��Ҫ���EntityFrameworkCore���ɵ�SQL
    .AddEntityFramework();


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

// ���÷��������
app.UseMiniProfiler();

// �����м��ΪSwagger UI�ṩ����
app.UseSwagger().UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// ����·��
app.MapControllers();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
