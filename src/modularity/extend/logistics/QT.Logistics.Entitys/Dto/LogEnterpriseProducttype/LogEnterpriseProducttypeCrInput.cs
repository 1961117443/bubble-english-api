
namespace QT.Logistics.Entitys.Dto.LogEnterpriseProducttype;

/// <summary>
/// 商品分类修改输入参数.
/// </summary>
public class LogEnterpriseProducttypeCrInput
{
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