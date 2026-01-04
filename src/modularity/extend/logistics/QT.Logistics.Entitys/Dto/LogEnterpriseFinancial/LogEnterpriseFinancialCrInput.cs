
namespace QT.Logistics.Entitys.Dto.LogEnterpriseFinancial;

/// <summary>
/// 缴费记录修改输入参数.
/// </summary>
public class LogEnterpriseFinancialCrInput
{
    /// <summary>
    /// 商家id.
    /// </summary>
    public string eId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    public string storeNumber { get; set; }

    /// <summary>
    /// 缴费金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 缴费方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 缴费流水号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}