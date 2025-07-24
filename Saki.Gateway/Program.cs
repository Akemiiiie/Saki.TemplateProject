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
// 读取配置文件
//builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().AddNamespace("Ocelot.Setting"); // 私有命名空间;
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
      //        Title = "网关API Swagger文档",
      //        Version = "v1",
      //        Description = "这是网关中心的Swagger服务，仅用于提供部分内部测试接口.",
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
// 4. 添加内存缓存（推荐）
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
