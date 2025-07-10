using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Saki.BaseTemplate.Enums.SakiEnums;

namespace Saki.ModelTemplate.Bases
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class BaseEntity<TKey> where TKey : struct
    {
        public BaseEntity()
        {
            
        }

        /// <summary>
        /// 包含Id的构造
        /// </summary>
        /// <param name="id"></param>
        public BaseEntity(TKey id)
        {
            this.Id = id;
        }

        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Id")]
        public virtual TKey Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public virtual DateTimeOffset? CreatedTime { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [SugarColumn(ColumnDescription = "软删除标识")]
        public virtual BooleanEnum? IsDeleted { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "更新时间", IsNullable = true)]
        public virtual DateTimeOffset? UpdatedTime { get; set; }
    }

    /// <summary>
    /// 实体
    /// </summary>
    public abstract class Entity : BaseEntity<Guid>
    {

    }
}
