using Com.Ctrip.Framework.Apollo;
using Microsoft.OpenApi.Models;
using Saki.TemplateWebProject.v3.Startups;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().
    AddNamespace("Web2Api"); // ˽�������ռ�;
// ��Consulע����ע��
builder.Services.ConsulRegister(builder.Configuration);
// Add services to the container.
builder.Services.AddMvc();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo() { 
        Title = "Saki's Project Template web2", 
        Version = "v1" ,
        Description = "����һ�����ڹ������̵�.netCore mvc��Ŀģ��",
        Contact = new OpenApiContact { Name = "Saki'CoreTemplate", Email = "2567241787@qq.com" }
    });
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
app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

app.Run();
