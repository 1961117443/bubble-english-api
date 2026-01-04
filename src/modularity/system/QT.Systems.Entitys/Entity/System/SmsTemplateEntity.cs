using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 短信模板.
/// </summary>
[SugarTable("BASE_SMS_TEMPLATE")]
[Tenant(ClaimConst.TENANTID)]
public class SmsTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 短信提供商.
    /// </summary>
    [SugarColumn(ColumnName = "F_COMPANY")]
    public int? Company { get; set; }

    /// <summary>
    /// 应用编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPID")]
    public string AppId { get; set; }

    /// <summary>
    /// 签名内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_SIGNCONTENT")]
    public string SignContent { get; set; }

    /// <summary>
    /// 模板编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string TemplateId { get; set; }

    /// <summary>
    /// 模板参数JSON.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEJSON")]
    public string TemplateJson { get; set; }

    /// <summary>
    /// 模板名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 模板编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_EnCode")]
    public string EnCode { get; set; }

    /// <summary>
    /// 域名.
    /// </summary>
    [SugarColumn(ColumnName = "F_Endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    /// 地域.
    /// </summary>
    [SugarColumn(ColumnName = "F_Region")]
    public string Region { get; set; }
}