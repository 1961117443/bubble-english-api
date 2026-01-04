using QT.Common.Const;
using QT.DependencyInjection;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.Common.Contracts;

/// <summary>
/// 创更删实体基类.
/// 带上EnabledMark字段
/// </summary>
[SuppressSniffer]
public abstract class CLDEntityBase : CUDEntityBase, ICreatorTime, IDeleteTime
{
    /// <summary>
    /// 获取或设置 启用标识.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENABLEDMARK", ColumnDescription = "启用标识")]
    public virtual int? EnabledMark { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Creator()
    {
        var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.CreatorTime = DateTime.Now;
        this.Id = YitIdHelper.NextId().ToString();
        this.EnabledMark = this.EnabledMark == null ? 1 : this.EnabledMark;
        if (!string.IsNullOrEmpty(userId))
        {
            this.CreatorUserId = userId;
        }
    }

    /// <summary>
    /// 创建.
    /// </summary>
    public override void Create()
    {
        base.Create();
        this.EnabledMark = this.EnabledMark == null ? 1 : this.EnabledMark;
    }
}