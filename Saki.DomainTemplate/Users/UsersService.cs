using Microsoft.Extensions.Logging;
using Saki.BaseTemplate.Utils;
using Saki.InterfaceTemplate.Users;
using Saki.IRepositoryTemplate.Users;
using Saki.ModelTemplate.Bases;
using Saki.ModelTemplate.Dtos;

namespace Saki.DomainTemplate.Users
{
    /// <summary>
    /// Service层
    /// </summary>
    public class UsersService:IUsersServiceInterface
    {
        public ILogger _logger;
        public IUsersRepository _userRepositort;

        /// <summary>
        /// 依赖注入
        /// </summary>
        public UsersService(IUsersRepository userRepositort,ILogger<UsersService> logger) 
        {
            _userRepositort = userRepositort;
            _logger = logger;
        }

        /// <summary>
        /// 根据Id获取书数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsersEntity> GetUser(string id)
        {
            var item = await _userRepositort.QueryById(id);
            return item;
        }

        /// <summary>
        /// 列表查询
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns></returns>
        public async Task<List<UsersEntity>> GetUserList(UsersDto query)
        {
            var res = await _userRepositort.GetList(query);
            return res;
        }

        /// <summary>
        /// 分页列表查询
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <param name="page">分页条件</param>
        /// <returns></returns>
        public async Task<List<UsersEntity>> GetUserPageList(UsersDto query, PageParam page)
        {
            var res = await _userRepositort.GetPageList(query, page);
            return res;
        }


        public async Task<List<UsersEntity>> GetJoinList()
        {
            var res = await _userRepositort.GetJoinList();
            return res;
        }

    }
}
