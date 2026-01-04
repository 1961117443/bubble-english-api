namespace QT.Logistics.Entitys.Dto.LogDeliveryStoreroom;

/// <summary>
/// 配送点仓库输入参数.
/// </summary>
public class LogDeliveryStoreroomListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 上级仓库.
    /// </summary>
    public string pidName { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    public int category { get; set; }
}