namespace QT.Common.Contracts;

/// <summary>
/// 主从实体类接口.
/// </summary>
public interface ISlaveEntityBase<TKey,TDKey> : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 父级id
    /// </summary>
    public TDKey FId { get; set; }
}
