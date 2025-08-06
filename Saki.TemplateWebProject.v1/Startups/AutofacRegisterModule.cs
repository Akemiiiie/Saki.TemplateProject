using Autofac;
using Microsoft.AspNetCore.Mvc;
using Saki.AutoFac.AutofacRegister;
using Saki.DomainTemplate.Users;
using Saki.InterfaceTemplate.Users;
using Saki.RepositoryTemplate.Base;

namespace Saki.TemplateWebProject.v1.Startups;

internal class AutofacRegisterModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //获取所有控制器类型并使用属性注入
        var controllersTypeAssembly = typeof(Program).Assembly.GetExportedTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type)).ToArray();
        builder.RegisterTypes(controllersTypeAssembly).PropertiesAutowired(new AutowiredPropertySelector());
        //批量自动注入,把需要注入层的程序集传参数,注入Service层的类
        builder.BatchAutowired(typeof(UsersService).Assembly);
        builder.BatchAutowired(typeof(IUsersServiceInterface).Assembly);
        builder.BatchAutowired(typeof(BaseRepository<>).Assembly);
        //builder.BatchAutowired(typeof(UsersEntity).Assembly);
        // builder.BatchAutowired(typeof(BaseDbConfig).Assembly);
        //注入其它层的containerBuilder.BatchAutowired(typeof(其它层的任务一个类).Assembly);
    }
}