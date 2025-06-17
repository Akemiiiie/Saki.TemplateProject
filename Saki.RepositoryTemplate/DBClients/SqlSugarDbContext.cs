using System.Reflection;
using Saki.RepositoryTemplate.Base;
using SqlSugar;
using StackExchange.Profiling;

namespace Saki.RepositoryTemplate.DBClients;

public class SqlSugarDbContext<T> where T : class, new()
{
    /// <summary>
    /// 注入配置文件
    /// </summary>

    //注意：不能写成静态的，用来处理事务多表查询和复杂的操作
    public SqlSugarClient Db; 

    public SqlSugarDbContext()
    {
        Db = new SqlSugarClient(new ConnectionConfig
        {
            // 主库配置
            ConnectionString = BaseDbConfig.DefaultConnection,
            DbType = DbType.SqlServer,
            InitKeyType = InitKeyType.Attribute, //从特性读取主键和自增列信息
            IsAutoCloseConnection = true, //开启自动释放模式
            // 从库配置
            // SlaveConnectionConfigs = new List<SlaveConnectionConfig>() {
            //      new() { ConnectionString = BaseDbConfig.SlaveConnectionConfig }
            // },

            ConfigureExternalServices = new ConfigureExternalServices
            {
                //注意:  这儿AOP设置不能少
                EntityService = (c, p) =>
                {
                    /***高版C#写法
                     * 支持string?和string，实体字段没有加?代表建表时该字段不能为null
                     * ***/
                    if (p.IsPrimarykey == false &&
                        new NullabilityInfoContext().Create(c).WriteState is NullabilityState.Nullable)
                        p.IsNullable = true;
                }
            }
        });

        //调式代码 用来打印SQL 
        Db.Aop.OnLogExecuting = (sql, pars) =>
        {
            MiniProfiler.Current.
                CustomTiming($"ConnId:[{Db.ContextID}] SQL：",
                    "【SQL语句】：" + UtilMethods.GetNativeSql(sql, pars));
            Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));
            //Console.WriteLine(sql + "\r\n" +
            //Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            Console.WriteLine();
        };
    }
}