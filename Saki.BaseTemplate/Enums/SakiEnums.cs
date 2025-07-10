using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.BaseTemplate.Enums
{
    public class SakiEnums
    {
        public enum BooleanEnum
        {
            /// <summary>
            /// 否
            /// </summary>
            [Description("否")]
            NO = 0,

            /// <summary>
            /// 是
            /// </summary>
            [Description("是")]
            YES = 1
        }
    }
}
