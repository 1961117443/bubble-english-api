using QT.Common.Const;
using QT.DependencyInjection;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.Common.Contracts;

/// <summary>
/// 创实体基类.
/// </summary>
[SuppressSniffer]
public abstract class CEntityBase : EntityBase<string>, ICreatorTime
{
    /// <summary>
    /// 获取或设置 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME", ColumnDescription = "创建时间")]
    public virtual DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 获取或设置 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORUSERID", ColumnDescription = "创建用户")]
    public virtual string CreatorUserId { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Creator()
    {
        var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.CreatorTime = DateTime.Now;
        this.Id = YitIdHelper.NextId().ToString();
        if (!string.IsNullOrEmpty(userId))
        {
            this.CreatorUserId = userId;
        }
    }
}