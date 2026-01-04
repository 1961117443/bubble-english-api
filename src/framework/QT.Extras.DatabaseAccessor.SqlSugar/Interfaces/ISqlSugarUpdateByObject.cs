namespace SqlSugar;

/// <summary>
/// 实体更新时触发 
/// </summary>
public interface ISqlSugarUpdateByObject
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityInfo"></param>
    void Execute(DataFilterModel entityInfo);
}
