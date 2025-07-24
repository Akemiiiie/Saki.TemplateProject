using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using MMLib.SwaggerForOcelot.Repositories;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using Saki.Gateway.Interceptor;
using Saki.Gateway.Repository;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
// ��ȡ�����ļ�
//builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().AddNamespace("Ocelot.Setting"); // ˽�������ռ�;
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// Add services to the container.
builder.Services.AddOcelot(builder.Configuration);
//builder.Services.AddSingleton<ISwaggerDownstreamInterceptor, PublishedDownstreamInterceptor>();
//builder.Services.AddSingleton<ISwaggerEndpointConfigurationRepository, DummySwaggerEndpointRepository>();
builder.Services.AddControllers();
//builder.Services.AddMvc();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
//});
builder.Services.AddSwaggerForOcelot(builder.Configuration,
  (options) =>
  {
      options.GenerateDocsForGatewayItSelf = true;
      //options.GenerateDocsDocsForGatewayItSelf(opt =>
      //{
      //    opt.GatewayDocsTitle = "Saki_Gateway API";
      //    opt.GatewayDocsOpenApiInfo = new OpenApiInfo()
      //    {
      //        Title = "����API Swagger�ĵ�",
      //        Version = "v1",
      //        Description = "�����������ĵ�Swagger���񣬽������ṩ�����ڲ����Խӿ�.",
      //        Contact = new OpenApiContact
      //        {
      //            Name = "Saki'Gateway Template",
      //            Email = "2567241787@qq.com",
      //        }
      //    };
      //});
  }
);

//builder.Services.AddEndpointsApiExplorer();
// 4. ����ڴ滺�棨�Ƽ���
//builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();
//app.UsePathBase("/gateway");
app.UseRouting();
app.UseAuthorization();

//app.MapControllers();

app.UseSwagger();
app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});
//app.UseSwaggerUI();
app.UseSwaggerForOcelotUI(opt => 
{
    opt.DownstreamSwaggerHeaders = new[]
    {
        new KeyValuePair<string, string>("Key", "Value")
    };
}).UseOcelot().Wait();

app.Run();
