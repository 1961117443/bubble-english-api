using QT.JXC.Entitys.Dto.Erp;

namespace QT.JXC.Entitys.Dto.Erp.OrderFj;

/// <summary>
/// 订单信息修改输入参数.
/// </summary>
public class ErpOrderFjCrInput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCrInput> erpOrderdetailList { get; set; }

    /// <summary>
    /// 订单处理记录表.
    /// </summary>
    public List<ErpOrderoperaterecordCrInput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksCrInput> erpOrderremarksList { get; set; }

}