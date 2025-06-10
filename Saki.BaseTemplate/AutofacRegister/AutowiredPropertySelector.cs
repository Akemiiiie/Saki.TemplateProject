using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saki.BaseTemplate.AutofacRegister
{
    /// <summary>
    /// 新增依赖注入特性类
    /// </summary>
    public class AutowiredPropertySelector:IPropertySelector
    {
        public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            //判断属性的特性是否包含自定义的属性,标记有返回true
            return propertyInfo.CustomAttributes.Any(s => s.AttributeType == typeof(AutowiredPropertyAttribute));
        }
    }
}
