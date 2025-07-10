using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Saki.ModelTemplate.Bases;

namespace Saki.RepositoryTemplate.DBClients
{
    /// <summary>
    /// openIddict 数据库上下文
    /// </summary>
    public class EFDbContext : DbContext
    {
        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="options"></param>
        public EFDbContext(DbContextOptions<EFDbContext> options)
            : base(options)
        {
        }

        public DbSet<OpenidUser> Users { get; set; }
    }
}
