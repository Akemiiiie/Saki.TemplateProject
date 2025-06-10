using Saki.BaseTemplate.AutofacRegister;
using Saki.BaseTemplate.Utils;
using Saki.ModelTemplate.Bases;
using Saki.ModelTemplate.Dtos;

namespace Saki.InterfaceTemplate.Users
{
    // 接口层-继承自动依赖注入接口
    public interface IUsersServiceInterface: ITransitDenpendency
    {
        /// <summary>
        /// 根据Id获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<UsersEntity> GetUser(string id);

        /// <summary>
        /// 通用列表查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetUserList(UsersDto query);

        /// <summary>
        /// 通用分页列表查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetUserPageList(UsersDto query, PageParam page);

        /// <summary>
        /// 跨库查询测试
        /// </summary>
        /// <returns></returns>
        public Task<List<UsersEntity>> GetJoinList();
    }
}
