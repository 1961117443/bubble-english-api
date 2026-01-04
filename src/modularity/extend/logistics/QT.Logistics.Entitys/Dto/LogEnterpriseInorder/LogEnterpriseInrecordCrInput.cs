
namespace QT.Logistics.Entitys.Dto.LogEnterpriseInrecord;

/// <summary>
/// 商家商品入库明细修改输入参数.
/// </summary>
public class LogEnterpriseInrecordCrInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    public decimal inNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}