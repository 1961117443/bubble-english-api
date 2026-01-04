using QT.DependencyInjection;
using SqlSugar;

namespace QT.Common.Contracts;

/// <summary>
/// 实体类基类.
/// </summary>
[SuppressSniffer]
public abstract class EntityBase<TKey> : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 获取或设置 编号.
    /// 虚属性
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", IsPrimaryKey = true)]
    public virtual TKey Id { get; set; }
}