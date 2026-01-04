using SqlSugar;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;

/// <summary>
/// 订单备注记录输出参数.
/// </summary>
public class ErpOrderremarksInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 备注内容.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    public string creatorUserId { get; set; }

}