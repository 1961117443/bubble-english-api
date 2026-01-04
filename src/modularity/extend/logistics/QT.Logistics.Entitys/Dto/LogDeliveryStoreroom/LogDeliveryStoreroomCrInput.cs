
namespace QT.Logistics.Entitys.Dto.LogDeliveryStoreroom;

/// <summary>
/// 配送点仓库修改输入参数.
/// </summary>
public class LogDeliveryStoreroomCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    public string pId { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 类型（仓库0，库区1，货柜2，柜层3）.
    /// </summary>
    public string category { get; set; }

}