namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 线路分拣员（中间表）修改输入参数.
/// </summary>
public class ErpDeliveryrouteSorterCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 线路ID.
    /// </summary>
    public string did { get; set; }

    /// <summary>
    /// 分拣员ID.
    /// </summary>
    public string sid { get; set; }

}