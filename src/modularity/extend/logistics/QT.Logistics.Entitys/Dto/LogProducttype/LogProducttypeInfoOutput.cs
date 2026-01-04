
namespace QT.Logistics.Entitys.Dto.LogProducttype;

/// <summary>
/// 商品分类输出参数.
/// </summary>
public class LogProducttypeInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 父级ID.
    /// </summary>
    public string fid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}