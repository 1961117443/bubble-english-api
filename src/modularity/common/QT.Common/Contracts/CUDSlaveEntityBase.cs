using SqlSugar;

namespace QT.Common.Contracts;

/// <summary>
/// 创更删从表实体基类
/// </summary>
public abstract class CUDSlaveEntityBase : CUDEntityBase, ISlaveEntityBase<string, string>
{
    /// <summary>
    /// 父类id.
    /// </summary>
    [SugarColumn(ColumnName = "FId", ColumnDescription = "父类id")]
    public virtual string FId { get; set; }
}