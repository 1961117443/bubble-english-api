using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseProducttype;

/// <summary>
/// 商品分类输入参数.
/// </summary>
public class LogEnterpriseProducttypeListOutput
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


    /// <summary>
    /// 上级分类.
    /// </summary>
    public string fidName { get; set; }

}

/// <summary>
/// 树形下拉模型
/// </summary>
[SuppressSniffer]
public class LogEnterpriseProducttypeTreeListOutput : TreeModel
{
    public string fullName { get; set; }
}