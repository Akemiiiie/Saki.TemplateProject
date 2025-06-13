using Autofac.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saki.OpenIddictServer.Startups;
using Saki.RepositoryTemplate.Base;
using Saki.RepositoryTemplate.DBClients;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// EF���ݿ����������ã���Ҫ������EF9.0�����޷�����ʹ��EF���Լ����ݳ�ʼ�����ܣ��һ��ڳ�ʼ����������ʱ����
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(BaseDbConfig.DefaultConnection));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<EFDbContext>();

// ������̨�������ڼ��������Ƿ�Ϸ�
builder.Services.AddHostedService<HostedService>();

// ���� Entity Framework Core ʹ�� Microsoft SQL Server��
builder.Services.AddDbContext<EFDbContext>(options =>
{
    // ���� EfCore ʹ�� Microsoft SQL Server ���ݿ�
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), builder =>
    {
        builder.MigrationsAssembly("Saki.OpenIddictServer");
    });

    // ע��OpenIddict���������ʵ�弯��
    // ע�⣺�������Ҫ�滻Ĭ�ϵ�OpenIddictʵ�壬��ʹ�÷������ط���
    options.UseOpenIddict();
});

// ע�� OpenIddict ���������
builder.Services.AddOpenIddict().AddCore(options =>
{
    // ���� OpenIddict ʹ�� Entity Framework Core �洢��ģ�͡�
    // ע�⣺���� ReplaceDefaultEntities() ���滻Ĭ��ʵ�塣
    options.UseEntityFrameworkCore()
        .UseDbContext<EFDbContext>();
});


builder.Services.AddOpenIddict()

    // ע�� OpenIddict ���������
    .AddServer(options =>
    {
        // ���� token �˵�
        options.SetTokenEndpointUris("connect/token");

        // ���ÿͻ���ƾ����
        options.AllowClientCredentialsFlow();

        // ע��ǩ���ͼ���ƾ֤
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // ע�� ASP.NET Core ������������ ASP.NET Core ѡ��
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    });

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

// // ����·��
// app.MapControllerRoute(
//     "default",
//     "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages()
//     .WithStaticAssets();

app.Run();