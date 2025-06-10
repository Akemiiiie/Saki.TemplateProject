using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saki.OpenIddictServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// ���� Entity Framework Core ʹ�� Microsoft SQL Server��
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // ���� EfCore ʹ�� Microsoft SQL Server ���ݿ�
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // ע��OpenIddict���������ʵ�弯��
    // ע�⣺�������Ҫ�滻Ĭ�ϵ�OpenIddictʵ�壬��ʹ�÷������ط���
    options.UseOpenIddict();
});

// builder.Services.AddRazorPages();

// ע�� OpenIddict ���������
builder.Services.AddOpenIddict().AddCore(options =>
{
    // ���� OpenIddict ʹ�� Entity Framework Core �洢��ģ�͡�
    // ע�⣺���� ReplaceDefaultEntities() ���滻Ĭ��ʵ�塣
    options.UseEntityFrameworkCore()
        .UseDbContext<ApplicationDbContext>();
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
