using Consul;
using Saki.BaseTemplate.ConfigerOptions;
using Consul.AspNetCore;

namespace Saki.TemplateWebProject.v2.Startups
{
    /// <summary>
    /// 依赖注入Consul客户端和配置选项
    /// </summary>
    /// <param name="consulClient"></param>
    /// <param name="configuration"></param>
    public class ConsulRegistrationService(IConsulClient consulClient, IConfiguration configuration) : IHostedService
    {
        private readonly IConsulClient _consulClient = consulClient;
        private readonly ConsulOptions _options = configuration.GetSection("Consul").Get<ConsulOptions>();
        private string _serviceId;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _serviceId = $"{_options.ServiceName}-{Guid.NewGuid()}";

            var registration = new AgentServiceRegistration
            {
                ID = _serviceId,
                Name = _options.ServiceName,
                Address = _options.IP,
                Port = _options.Port,
                Check = new AgentServiceCheck
                {
                    HTTP = $"https://{_options.IP}:{_options.Port}/api/health/checkhealth",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30),
                    TLSSkipVerify = true // 确保生效的设置
                },
                Tags = new[] { $"urlprefix-/{_options.ServiceName}" }
            };

            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
        }
    }
}
