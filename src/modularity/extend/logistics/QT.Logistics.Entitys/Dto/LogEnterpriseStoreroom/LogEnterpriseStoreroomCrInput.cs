
namespace QT.Logistics.Entitys.Dto.LogEnterpriseStoreroom;

/// <summary>
/// 仓库信息修改输入参数.
/// </summary>
public class LogEnterpriseStoreroomCrInput
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
    /// 简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string adminId { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string adminTel { get; set; }

    /// <summary>
    /// 面积.
    /// </summary>
    public decimal area { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    public string pId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public bool status { get; set; }

    /// <summary>
    /// 类型（仓库0，库区1，货柜2，柜层3）.
    /// </summary>
    public int? category { get; set; }

}