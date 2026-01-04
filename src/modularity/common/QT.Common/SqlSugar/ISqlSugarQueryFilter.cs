using QT.Common.Contracts;
using QT.DependencyInjection;
using SqlSugar;

namespace QT.Common;

/// <summary>
/// 通用的过滤器
/// </summary>
public class CommonQueryFilter : ISqlSugarQueryFilter, ISingleton
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    public void Execute(ISqlSugarClient provider)
    {
        provider.QueryFilter.AddTableFilter<IDeleteTime>(it => it.DeleteTime == null);
    }
}