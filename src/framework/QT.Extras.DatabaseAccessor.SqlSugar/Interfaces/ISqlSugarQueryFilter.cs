namespace SqlSugar;

/// <summary>
/// 需要注册为单例
/// </summary>
public interface ISqlSugarQueryFilter
{
    /// <summary>
    /// 处理过滤器
    /// </summary>
    /// <param name="provider"></param>
    void Execute(ISqlSugarClient provider);
}