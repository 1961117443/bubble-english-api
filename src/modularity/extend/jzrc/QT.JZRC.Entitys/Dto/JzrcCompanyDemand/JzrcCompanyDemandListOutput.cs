namespace QT.JZRC.Entitys.Dto.JzrcCompanyDemand;

/// <summary>
/// 建筑企业需求输入参数.
/// </summary>
public class JzrcCompanyDemandListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 需求内容.
    /// </summary>
    public string content { get; set; }


    public string companyIdName { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }
}