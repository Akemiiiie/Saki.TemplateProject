using Saki.BaseTemplate.Utils;
using Saki.IRepositoryTemplate.Users;
using Saki.ModelTemplate.Bases;
using Saki.ModelTemplate.Dtos;
using Saki.RepositoryTemplate.Base;
using Saki.RepositoryTemplate.DBClients;
using SqlSugar;

namespace Saki.RepositoryTemplate.Users;

public class UsersRepository : BaseRepository<UsersEntity>, IUsersRepository
{
    private SqlSugarClient newDb;
    public UsersRepository()
    {
        newDb = new SqlSugarClient(new ConnectionConfig()
        {
            // 主库配置
            ConnectionString = "server=.;database=Saki_MiniWorkSlave;uid=sa;pwd=lei005917.;TrustServerCertificate=true",
            DbType = DbType.SqlServer,
            InitKeyType = InitKeyType.Attribute, //从特性读取主键和自增列信息
            IsAutoCloseConnection = true, //开启自动释放模式
        });
    }

    /// <summary>
    ///     多条件查询
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<List<UsersEntity>> GetList(UsersDto query)
    {
        var res = await Db.Queryable<UsersEntity>().Where(t => t.Id.Equals(query.Id)).ToListAsync();
        return res;
    }

    /// <summary>
    ///     多条件分页查询
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<List<UsersEntity>> GetPageList(UsersDto query, PageParam page)
    {
        var res = await Db.Queryable<UsersEntity>().Where(t => t.Id.Equals(query.Id))
            .ToPageListAsync(page.PageIndex, page.PageSize);
        return res;
    }

    /// <summary>
    /// 跨库查询示例
    /// </summary>
    /// <returns></returns>
    public Task<List<UsersEntity>> GetJoinList()
    {
        var datas = Db.Queryable<UsersEntity>().InnerJoin<UserTypeEntity>(
                (user, userType) => user.Id == userType.UserId,
                "Saki_MiniWorkSlave.dbo.UserType")
            .Where((user, userType)=>userType.UserId == "1")
            .Select((user, userType) => new UsersEntity()
            {
                Id = user.Id
            })
            .ToListAsync();
       return datas;
    }
}