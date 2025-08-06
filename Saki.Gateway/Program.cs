using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);
// 读取配置文件
//builder.Configuration.AddApollo(builder.Configuration.GetSection("Apollo")).AddDefault().AddNamespace("Ocelot.Setting"); // 私有命名空间;
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// Add services to the container.
builder.Services.AddOcelot(builder.Configuration).AddConsul();
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
  }
);

//builder.Services.AddEndpointsApiExplorer();
// 4. 添加内存缓存（推荐）
//builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();
app.UsePathBase("/gateway");
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
    opt.DownstreamSwaggerEndPointBasePath = "/gateway/swagger/docs";
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.DownstreamSwaggerHeaders = new[]
    {
        new KeyValuePair<string, string>("Key", "Value")
    };
}, 
action => {
    
}).UseOcelot().Wait();
app.Run();
