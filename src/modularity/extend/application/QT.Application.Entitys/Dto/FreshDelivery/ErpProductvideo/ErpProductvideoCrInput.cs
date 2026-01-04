
namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductvideo;

/// <summary>
/// 商品视频修改输入参数.
/// </summary>
public class ErpProductvideoCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 视频文件地址
    /// </summary>
    public string video { get; set; }  

    /// <summary>
    /// 备注（文件id）
    /// </summary>
    public string remark { get; set; }
}