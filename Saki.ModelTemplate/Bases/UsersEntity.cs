using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.ModelTemplate.Bases
{
    /// <summary>
    /// 实体层
    /// </summary>
    [SugarTable("sys_Users")]
    public class UsersEntity: Entity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "User_Id")]
        public override Guid Id { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        [SugarColumn(ColumnName = "User_Name")]
        public string Name { get; set; }

        /// <summary>
        /// 用户性别
        /// </summary>
        [SugarColumn(ColumnName = "User_Sex")]
        public int? UserSex { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [SugarColumn(ColumnName = "IdCard")]
        public string IdCard { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        [SugarColumn(ColumnName = "Age")]
        public int Age { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [SugarColumn(ColumnName = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [SugarColumn(ColumnName = "Address")]
        public string Address { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [SugarColumn(ColumnName = "Email")]
        public string Email { get; set; }
    }
}
