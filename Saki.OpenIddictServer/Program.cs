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
// EF���ݿ����������ã���Ҫ������EF9.0�����޷�����ʹ��EF���Լ����ݳ�ʼ�����ܣ��һ��ڳ�ʼ����������ʱ����
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));
//
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<EFDbContext>();

// ������̨�������ڼ��������Ƿ�Ϸ�
builder.Services.AddHostedService<HostedService>();

// ���� Entity Framework Core ʹ�� Microsoft SQL Server��
builder.Services.AddDbContext<EFDbContext>(options =>
{
    // ���� EfCore ʹ�� Microsoft SQL Server ���ݿ�
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        builder => { builder.MigrationsAssembly("Saki.OpenIddictServer"); });

    // ע��OpenIddict���������ʵ�弯��
    // ע�⣺�������Ҫ�滻Ĭ�ϵ�OpenIddictʵ�壬��ʹ�÷������ط���
    options.UseOpenIddict();
});
//
// var encryptionCert = new X509Certificate2(
//     "encryption.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable); // ����Ȩ�޿�ѡ
// var signingCert = new X509Certificate2(
//     "signing.pfx",
//     "005917",
//     X509KeyStorageFlags.MachineKeySet |
//     X509KeyStorageFlags.PersistKeySet |
//     X509KeyStorageFlags.Exportable);

builder.Services.AddOpenIddict().AddCore(options =>
    {
        // ���� OpenIddict ʹ�� Entity Framework Core �洢��ģ�͡�
        // ע�⣺���� ReplaceDefaultEntities() ���滻Ĭ��ʵ�塣
        options.UseEntityFrameworkCore()
            .UseDbContext<EFDbContext>();
        options.UseQuartz();
    })
    // ע�� OpenIddict ���������
    .AddServer(options =>
    {
        // ���ƶ˵�
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token");
        options.RegisterScopes("service-worker"); // ��������Դ��������Audienceһ��
        // options.SetLogoutEndpointUris("/connect/logout"); // �ǳ��˵�
        // options.AcceptAnonymousClients();              // �������ͻ���
        options.AllowPasswordFlow()                    // ����������Ȩ����
            .AllowClientCredentialsFlow();              // ���ÿͻ���ƾ����
        // options.AcceptAnonymousClients();              // �������ͻ���

        // ע��ǩ���ͼ���֤��
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        // options.AddEncryptionCertificate(encryptionCert);
        // options.AddSigningCertificate(signingCert);

        // ע�� ASP.NET Core ������������ ASP.NET Core ѡ��
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
        // ��ʹ��֤���JWT���м��ܣ����ڲ���ʱʹ�ã����ڽ���jwt�鿴��Ӧ���ݽ��е��ԣ�
        //options.DisableAccessTokenEncryption();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
            // ��������Ȩ�˵���ʹ���Զ����߼�
            .EnableAuthorizationEndpointPassthrough()
            // ���������ƶ˵���ʹ���Զ����߼�
            .EnableTokenEndpointPassthrough()
            // ����UseStatusCodePages
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        // �ӱ��� OpenIddict ������ʵ����������
        options.UseLocalServer();
        // ע��host����
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
        // �������ã��������ɾ�����Ҫ��������
        policy.WithOrigins("https://localhost:7002", "https://localhost:7003", "https://oidcdebugger.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(); // Optional: For API testing

builder.Services.AddTransient<AuthorizationService>(); // Register custom service
builder.Services.AddTransient<ClientsSeeder>();      // Register seeder

// ���MVC�м��,����ӻᵼ���޷��������ʽӿ�
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

// // ����·��
// app.MapControllerRoute(
//     "default",
//     "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages()
//     .WithStaticAssets();

app.Run();