using Consul;
using Consul.AspNetCore;
using Saki.BaseTemplate.ConfigerOptions;

namespace Saki.TemplateWebProject.v3.Startups
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
                HTTP = $"https://{consulOptions.IP}:{consulOptions.Port}/api/health/CheckHealth",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30),
                TLSSkipVerify = true // 是否进行证书验证
            };

            // 在consul中注册服务
            services.AddConsulServiceRegistration(options =>
            {
                options.ID = Guid.NewGuid().ToString();
                options.Name = consulOptions.ServiceName;
                options.Address = consulOptions.IP;
                options.Port = consulOptions.Port;
                options.Check = httpCheck;
                options.Tags = new[] { $"urlprefix-/{consulOptions.ServiceName}" };
            });

            return services;
        }
    }
}
