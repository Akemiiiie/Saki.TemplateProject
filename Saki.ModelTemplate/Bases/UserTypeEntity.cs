using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.ModelTemplate.Bases
{
    [SugarTable("UserType")]
    public class UserTypeEntity: Entity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(ColumnName = "User_Id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }
    }
}
