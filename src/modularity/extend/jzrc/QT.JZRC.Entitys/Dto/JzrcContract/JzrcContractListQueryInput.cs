using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcContract;

/// <summary>
/// 建筑人才合同管理列表查询输入
/// </summary>
public class JzrcContractListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

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
    /// 签订时间.
    /// </summary>
    public string signTime { get; set; }

}