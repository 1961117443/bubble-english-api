namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 仓库覆盖路线(中间表）输出参数.
/// </summary>
public class ErpStoreDeliveryInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string sid { get; set; }

    public ErpStoreroomListOutput erpStoreroom { get; set; }

    /// <summary>
    /// 路线ID.
    /// </summary>
    public string did { get; set; }

    public ErpStoreDeliveryInfoOutput erpStoreDelivery { get; set; }
}