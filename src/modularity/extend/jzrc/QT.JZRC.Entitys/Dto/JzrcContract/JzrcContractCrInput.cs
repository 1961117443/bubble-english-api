
namespace QT.JZRC.Entitys.Dto.JzrcContract;

/// <summary>
/// 建筑人才合同管理修改输入参数.
/// </summary>
public class JzrcContractCrInput
{
    /// <summary>
    /// 合同编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    public string certificateId { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    public string jobId { get; set; }

    /// <summary>
    /// 签订时间.
    /// </summary>
    public DateTime? signTime { get; set; }

    /// <summary>
    /// 合同金额.
    /// </summary>
    public decimal amount { get; set; }

}