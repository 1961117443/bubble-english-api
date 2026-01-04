using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 消息模板



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE_TEMPLATE")]
[Tenant(ClaimConst.TENANTID)]
public class MessageTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 分类（数据字典）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY")]
    public string Category { get; set; }

    /// <summary>
    /// 模板名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 模板编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 是否站内信.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISSTATIONLETTER")]
    public int? IsStationLetter { get; set; }

    /// <summary>
    /// 是否邮箱.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISEMAIL")]
    public int? IsEmail { get; set; }

    /// <summary>
    /// 是否企业微信.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISWECOM")]
    public int? IsWeCom { get; set; }

    /// <summary>
    /// 是否钉钉.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISDINGTALK")]
    public int? IsDingTalk { get; set; }

    /// <summary>
    /// 是否短信.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISSMS")]
    public int? IsSms { get; set; }

    /// <summary>
    /// 短信模板ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_SMSID")]
    public string SmsId { get; set; }

    /// <summary>
    /// 模板参数JSON.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEJSON")]
    public string TemplateJson { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string Content { get; set; } = string.Empty;
}