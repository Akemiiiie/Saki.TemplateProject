using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Panda.DynamicWebApi;
using Saki.MiniProfilerOption;
using Saki.RepositoryTemplate.Base;
using Saki.RepositoryTemplate.DBClients;
using Saki.TemplateWebProject.v2.Startups;
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
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<EFDbContext>();

// Autofac自动注入(此处会注入数据库上下文的数据库连接)
builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => { builder.RegisterModule<AutofacRegisterModule>(); });

// 添加MVC中间件,不添加会导致无法正常访问接口
builder.Services.AddRazorPages();
builder.Services.AddMvc();
// 动态api
builder.Services.AddDynamicWebApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // 配置jwt鉴权服务器
        options.SetIssuer("https://localhost:7001/"); // Auth server URL
        options.AddAudiences("service-worker"); // Must match the audience in the auth server
        // 配置客户端以及客户端秘钥
        options.UseIntrospection().SetClientId("service-worker").SetClientSecret("388D45FA-B36B-4988-BA59-B187D329C207");

        // options.AddEncryptionCertificate();
        // 配置对称加密秘钥
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="))); // Use secure key management in production!

        // Register the System.Net.Http integration for remote validation/introspection.
        options.UseSystemNetHttp();
        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


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
    options.AddSecurityDefinition(
        "oauth",
        new OpenApiSecurityScheme
        {
            Flows = new OpenApiOAuthFlows
            {
                // 证书获取地址
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
    // 可选：为XML注释添加支持
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// 添加分析器中间件
builder.Services.AddMiniProfiler(ProfilerDefaultOption.GetProfilerDefaultOption)
    // AddEntityFramework是要监控EntityFrameworkCore生成的SQL
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

// 启用分析器组件
app.UseMiniProfiler();

// 启用swagger
app.UseSwagger();
// 启用中间件为Swagger UI提供服务
app.UseSwaggerUI(c =>
{
    // 需要将文件设置为嵌入资源，并且设置为较新则复制
    c.IndexStream = () => Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("Saki.TemplateWebProject.v2.wwwroot.index.html");
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "我的MVC应用API V1");
    // 可选：设置默认展开的API
    c.DocExpansion(DocExpansion.None);
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// 配置路由
app.MapControllers();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
