namespace SqlSugar;

/// <summary>
/// 实体新增时触发 
/// </summary>
public interface ISqlSugarInsertByObject
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityInfo"></param>
    void Execute(DataFilterModel entityInfo);
}
