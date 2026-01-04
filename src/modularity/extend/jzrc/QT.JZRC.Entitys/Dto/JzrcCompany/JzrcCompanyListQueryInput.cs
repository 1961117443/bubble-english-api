using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 企业信息列表查询输入
/// </summary>
public class JzrcCompanyListQueryInput : PageInputBase
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
    /// 企业名称.
    /// </summary>
    public string companyName { get; set; }

    /// <summary>
    /// 企业联系人.
    /// </summary>
    public string contactPerson { get; set; }

    /// <summary>
    /// 企业联系电话.
    /// </summary>
    public string contactPhoneNumber { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    public string settlementDate { get; set; }

    /// <summary>
    /// 客户经理id
    /// </summary>
    public string managerId { get; set; }


    /// <summary>
    /// 是否入驻
    /// </summary>
    public int? signed { get; set; }

    /// <summary>
    /// 区域
    /// </summary>
    public string region { get; set; }

}