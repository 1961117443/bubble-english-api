
namespace QT.Logistics.Entitys.Dto.LogDeliveryPoint;

/// <summary>
/// 配送点管理修改输入参数.
/// </summary>
public class LogDeliveryPointCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string leader { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 配送点管理员ID.
    /// </summary>
    public string adminId { get; set; }

}