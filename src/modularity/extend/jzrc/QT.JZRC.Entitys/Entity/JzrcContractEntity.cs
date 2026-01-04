using QT.Common.Const;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑人才合同管理实体.
/// </summary>
[SugarTable("jzrc_contract")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcContractEntity
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
    /// 合同编号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateId")]
    public string CertificateId { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    [SugarColumn(ColumnName = "JobId")]
    public string JobId { get; set; }

    /// <summary>
    /// 签订时间.
    /// </summary>
    [SugarColumn(ColumnName = "SignTime")]
    public DateTime? SignTime { get; set; }

    /// <summary>
    /// 当前的人才信息.
    /// </summary>
    [SugarColumn(ColumnName = "TalentProperty")]
    public string TalentProperty { get; set; }

    /// <summary>
    /// 当前的企业信息.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyProperty")]
    public string CompanyProperty { get; set; }

    /// <summary>
    /// 当前的证书信息.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateProperty")]
    public string CertificateProperty { get; set; }

    /// <summary>
    /// 当前的岗位信息.
    /// </summary>
    [SugarColumn(ColumnName = "JobProperty")]
    public string JobProperty { get; set; }

    /// <summary>
    /// 合同金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

}