using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.RepositoryTemplate.Interface
{   
    /// <summary>
    /// 租户Id
    /// </summary>
    public interface ITenantEntity
    {
        long? TenantId { get; set; }
    }
}
