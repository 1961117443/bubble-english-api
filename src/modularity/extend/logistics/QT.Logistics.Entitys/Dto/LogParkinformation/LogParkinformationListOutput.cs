namespace QT.Logistics.Entitys.Dto.LogParkinformation;

/// <summary>
/// 物流园信息输入参数.
/// </summary>
public class LogParkinformationListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 物流园名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 物流园地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 物流园简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 物流园电话.
    /// </summary>
    public string phone { get; set; }

}