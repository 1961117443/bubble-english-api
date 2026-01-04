
namespace QT.Logistics.Entitys.Dto.LogEnterpriseOutrecord;

/// <summary>
/// 商家商品出库明细输出参数.
/// </summary>
public class LogEnterpriseOutrecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 总价.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    public string gidName { get; set; }
    public string productName { get; set; }
    public string unit { get; set; }
}