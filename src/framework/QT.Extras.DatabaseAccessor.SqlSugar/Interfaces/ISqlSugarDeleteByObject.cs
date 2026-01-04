namespace SqlSugar;

/// <summary>
/// 实体删除时触发 
/// </summary>
public interface ISqlSugarDeleteByObject
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityInfo"></param>
    void Execute(DataFilterModel entityInfo);
}