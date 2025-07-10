using Consul;
using Consul.AspNetCore;
using Saki.BaseTemplate.ConfigerOptions;

namespace Saki.TemplateWebProject.v2.Startups
{
    public static class ConsulRegisterModule
    {
        public static IServiceCollection ConsulRegister(this IServiceCollection services, IConfiguration configuration)
        { 
            // 配置consul服务注册信息
            var consulOptions = configuration.GetSection("Consul").Get<ConsulOptions>();
            services.AddConsul(options => options.Address = new Uri($"http://{consulOptions.ConsulIP}:{consulOptions.ConsulPort}"));

            // 启用心跳检查
            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔，或者称为心跳间隔
                HTTP = $"http://{consulOptions.IP}:{consulOptions.Port}/api/health/checkhealth",//健康检查地址
                Timeout = TimeSpan.FromSeconds(10),
            };

            // 在consul中注册服务
            services.AddConsulServiceRegistration(options =>
            {
                options.Checks = new[] { httpCheck };
                options.ID = Guid.NewGuid().ToString();
                options.Name = consulOptions.ServiceName;
                options.Address = consulOptions.IP;
                options.Port = consulOptions.Port;
                options.Meta = new Dictionary<string, string>() { { "Weight", consulOptions.Weight.HasValue ? consulOptions.Weight.Value.ToString() : "1" } };
                options.Tags = new[] { $"urlprefix-/{consulOptions.ServiceName}" }; //添加
            });

            return services;
        }
    }
}
