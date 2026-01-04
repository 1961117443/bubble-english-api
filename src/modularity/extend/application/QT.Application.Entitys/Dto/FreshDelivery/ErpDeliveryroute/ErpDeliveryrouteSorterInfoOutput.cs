namespace QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryrouteSorter;

/// <summary>
/// 线路分拣员（中间表）输出参数.
/// </summary>
public class ErpDeliveryrouteSorterInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string did { get; set; }

    /// <summary>
    /// 分拣员ID.
    /// </summary>
    public string sid { get; set; }

    ///// <summary>
    ///// 分拣员
    ///// </summary>
    //public ErpSorterListOutput erpSorter { get; set; }
}