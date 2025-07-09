using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.ModelTemplate.Bases
{
    /// <summary>
    /// 实体基类
    /// </summary>
    internal class BaseEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "User_Id")]
        public Guid Id { get; set; }
    }
}
