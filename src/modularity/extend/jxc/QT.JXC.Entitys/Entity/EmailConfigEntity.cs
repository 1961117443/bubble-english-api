using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 邮件配置
/// </summary>
[SugarTable("EXT_EMAILCONFIG")]
[Tenant(ClaimConst.TENANTID)]
public class EmailConfigEntity : CEntityBase
{
    /// <summary>
    /// POP3服务.
    /// </summary>
    [SugarColumn(ColumnName = "F_POP3HOST")]
    public string? POP3Host { get; set; }

    /// <summary>
    /// POP3端口.
    /// </summary>
    [SugarColumn(ColumnName = "F_POP3PORT")]
    public int? POP3Port { get; set; }

    /// <summary>
    /// SMTP服务.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPHOST")]
    public string? SMTPHost { get; set; }

    /// <summary>
    /// SMTP端口.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMTPPORT")]
    public int? SMTPPort { get; set; }

    /// <summary>
    /// 账户.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNT")]
    public string? Account { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_PASSWORD")]
    public string? Password { get; set; }

    /// <summary>
    /// SSL登录.
    /// </summary>
    [SugarColumn(ColumnName = "F_SSL")]
    public int? Ssl { get; set; }

    /// <summary>
    /// 发件人名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDERNAME")]
    public string? SenderName { get; set; }

    /// <summary>
    /// 我的文件夹.
    /// </summary>
    [SugarColumn(ColumnName = "F_FOLDERJSON")]
    public string? FolderJson { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENABLEDMARK")]
    public int? EnabledMark { get; set; }
}
