namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 仓库覆盖路线(中间表）修改输入参数.
/// </summary>
public class ErpStoreDeliveryCrInput
{
    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 路线ID.
    /// </summary>
    public string did { get; set; }

}

public class ErpStoreCompanyCrInput
{
    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

}