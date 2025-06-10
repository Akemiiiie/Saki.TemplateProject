using Microsoft.AspNetCore.Mvc;
using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.BaseTemplate.BaseControllers
{
    /// <summary>
    /// 动态api扩展基础控制器
    /// </summary>
    [DynamicWebApi]
    public class BaseController:Controller,IDynamicWebApi
    {

    }
}
