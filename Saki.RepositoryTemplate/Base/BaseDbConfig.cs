namespace Saki.RepositoryTemplate.Base;

public class BaseDbConfig
{
    /// <summary>
    ///     主库配置
    /// </summary>
    public static string ConnectionString { get; set; }

    /// <summary>
    ///     从库配置
    /// </summary>
    public static string SlaveConnectionConfig { get; set; }
}