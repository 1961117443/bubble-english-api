using QT.Common.Const;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑人才档案收件实体.
/// </summary>
[SugarTable("jzrc_certificate_in")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCertificateInEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }

    /// <summary>
    /// 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string DeleteUserId { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateId")]
    public string CertificateId { get; set; }

    /// <summary>
    /// 档案位置id.
    /// </summary>
    [SugarColumn(ColumnName = "StoreroomId")]
    public string StoreroomId { get; set; }

    /// <summary>
    /// 收件时间.
    /// </summary>
    [SugarColumn(ColumnName = "InTime")]
    public DateTime? InTime { get; set; }

    /// <summary>
    /// 快递单号.
    /// </summary>
    [SugarColumn(ColumnName = "ExpressNo")]
    public string ExpressNo { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }


    /// <summary>
    /// 经手人.
    /// </summary>
    [SugarColumn(ColumnName = "HandledBy")]
    public string HandledBy { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 类型（0:借入，1借出）.
    /// </summary>
    [SugarColumn(ColumnName = "InoutType")]
    public int InoutType { get; set; }
}