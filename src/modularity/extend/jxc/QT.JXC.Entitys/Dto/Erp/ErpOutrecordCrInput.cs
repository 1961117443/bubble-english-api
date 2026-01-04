namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品出库记录修改输入参数.
/// </summary>
public class ErpOutrecordCrInput
{
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
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 扣减的库存记录.
    /// </summary>
    public List<ErpStorerecordInput> storeDetailList { get; set; }
}