using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcCompanyJob;

public class ClientJzrcCompanyJobListQueryInput : PageInputBase
{
    /// <summary>
    /// 证书类型.
    /// </summary>
    public string certificateCategoryId { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public string region { get; set; }
}