using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcCompanyJob;

/// <summary>
/// 企业招聘列表查询输入
/// </summary>
public class JzrcCompanyJobListQueryInput : PageInputBase
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
    /// 职位名称.
    /// </summary>
    public string jobTitle { get; set; }

    /// <summary>
    /// 人才类型.
    /// </summary>
    public string candidateType { get; set; }

    /// <summary>
    /// 需求时间起.
    /// </summary>
    public string requiredStart { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 证书类型.
    /// </summary>
    public string certificateCategoryId { get; set; }

}
