﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.AutoFac.AutofacRegister
{
    [AttributeUsage(AttributeTargets.Property)]
    //为了支持属性注入，只能打到属性上
    public class AutowiredPropertyAttribute : Attribute
    {

    }
}
