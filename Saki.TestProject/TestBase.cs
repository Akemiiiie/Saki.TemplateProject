using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.TestProject
{
    internal class TestBase
    {
        protected IServiceProvider ServiceProvider { get; }
        public TestBase()
        {
            var services = new ServiceCollection();

            // 注册被测服务
            services.AddTransient<OrderService>();

            // 注册模拟依赖
            services.AddTransient<IPaymentGateway>(_ => Mock.Of<IPaymentGateway>());
            services.AddTransient<IOrderRepository>(_ => Mock.Of<IOrderRepository>());

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
