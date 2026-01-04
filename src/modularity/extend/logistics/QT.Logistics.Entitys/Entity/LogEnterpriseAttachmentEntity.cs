using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 入驻商家附件实体.
/// </summary>
[SugarTable("log_enterprise_attachment")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseAttachmentEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "EId")]
    public string EId { get; set; }

    /// <summary>
    /// 附件名称.
    /// </summary>
    [SugarColumn(ColumnName = "AttachmentName")]
    public string AttachmentName { get; set; }

    /// <summary>
    /// 附件路径.
    /// </summary>
    [SugarColumn(ColumnName = "AttachmentPath")]
    public string AttachmentPath { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    [SugarColumn(ColumnName = "FileSize")]
    public string FileSize { get; set; }

    /// <summary>
    /// 上传时间.
    /// </summary>
    [SugarColumn(ColumnName = "UploadTime")]
    public DateTime? UploadTime { get; set; }

}