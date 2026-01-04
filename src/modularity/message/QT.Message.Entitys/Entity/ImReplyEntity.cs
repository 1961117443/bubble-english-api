using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Message.Entitys;

/// <summary>
/// 聊天会话.
/// </summary>
[SugarTable("BASE_IMREPLY")]
[Tenant(ClaimConst.TENANTID)]
public class ImReplyEntity : EntityBase<string>
{
    /// <summary>
    /// 发送者.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 接收用户.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ReceiveUserId")]
    public string ReceiveUserId { get; set; }

    /// <summary>
    /// 接收用户时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ReceiveTime")]
    public DateTime? ReceiveTime { get; set; }
}