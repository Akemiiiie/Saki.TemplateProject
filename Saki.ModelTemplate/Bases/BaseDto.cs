using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.ModelTemplate.Bases
{
    public class BaseDto
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
