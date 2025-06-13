using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Saki.RepositoryTemplate.DBClients
{
    /// <summary>
    /// openIddict 数据库上下文
    /// </summary>
    public class EFDbContext : IdentityDbContext
    {
        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="options"></param>
        public EFDbContext(DbContextOptions<EFDbContext> options)
            : base(options)
        {
        }
    }
}
