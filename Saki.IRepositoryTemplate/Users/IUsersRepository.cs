using Saki.BaseTemplate.Utils;
using Saki.IRepositoryTemplate.Base;
using Saki.ModelTemplate.Bases;
using Saki.ModelTemplate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.IRepositoryTemplate.Users
{
    /// <summary>
    /// 仓储接口
    /// 可以在原有仓储基类上增加自定义扩展接口
    /// </summary>
    public interface IUsersRepository: IBaseRepository<UsersEntity>
    {
        /// <summary>
        /// 多条件查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetList(UsersDto query);


        /// <summary>
        /// 多条件分页查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetPageList(UsersDto query, PageParam page);

        /// <summary>
        /// 跨库查询测试
        /// </summary>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetJoinList();

    }
}
