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
// 获取配置中心配置信息
builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().AddNamespace("Web1Api"); // 私有命名空间;
// 获取数据库连接配置
builder.Configuration.GetSection("ConnectionStrings").Get<BaseDbConfig>();

// EF数据库连接配置，需要说明的是由于EF9.0开始无法使用EF自带数据初始化功能，故而在初始化的时候需要手动配置
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<EFDbContext>();

// Autofac自动注册(此处注册了数据库访问的数据库仓储)
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

// 添加MVC中间件,防止接口无法被动态访问
builder.Services.AddRazorPages();
builder.Services.AddMvc();
// 动态api
builder.Services.AddDynamicWebApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // 设置jwt授权验证中心
        options.SetIssuer("https://localhost:7001/"); // Auth server URL
        options.AddAudiences("service-worker"); // Must match the audience in the auth server
        // 设置客户端验证客户端密钥
        options.UseIntrospection().SetClientId("service-worker").SetClientSecret("388D45FA-B36B-4988-BA59-B187D329C207");

        // options.AddEncryptionCertificate();
        // 设置对称加密密钥
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="))); // Use secure key management in production!

        // Register the System.Net.Http integration for remote validation/introspection.
        options.UseSystemNetHttp();
        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

// 注册Consul注册服务
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
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


// 添加swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Saki's Project Template web1",
        Version = "v1",
        Description = "这是一个简单的.netCore mvc项目模板",
        Contact = new OpenApiContact { Name = "Saki'CoreTemplate", Email = "2567241787@qq.com" }
    });
    options.AddSecurityDefinition(
        "oauth",
        new OpenApiSecurityScheme
        {
            Flows = new OpenApiOAuthFlows
            {
                // 授权获取地址
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
    // 选择为XML注释提供支持
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// 添加性能分析中间件
builder.Services.AddMiniProfiler(ProfilerDefaultOption.GetProfilerDefaultOption)
    // AddEntityFramework需要配置EntityFrameworkCore的SQL
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

// 使用性能分析中间件
app.UseMiniProfiler();

// 添加中间件为Swagger UI提供服务
app.UseSwagger().UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// 添加路由
app.MapControllers();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
