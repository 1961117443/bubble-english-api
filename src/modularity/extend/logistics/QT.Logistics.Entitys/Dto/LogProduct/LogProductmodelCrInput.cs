
namespace QT.Logistics.Entitys.Dto.LogProductmodel;

/// <summary>
/// 商家商品规格修改输入参数.
/// </summary>
public class LogProductmodelCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 规格.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 成本价.
    /// </summary>
    public decimal costPrice { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 条形码.
    /// </summary>
    public string barCode { get; set; }

    /// <summary>
    /// 起售数.
    /// </summary>
    public decimal minNum { get; set; }

    /// <summary>
    /// 限购数.
    /// </summary>
    public decimal maxNum { get; set; }

}