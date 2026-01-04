
namespace QT.Application.Entitys.Dto.FreshDelivery.ErpStorearea;

/// <summary>
/// 库区信息输出参数.
/// </summary>
public class ErpStoreareaInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 分类名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 库区编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 库区备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 仓库id
    /// </summary>
    public string sid { get; set; }

}