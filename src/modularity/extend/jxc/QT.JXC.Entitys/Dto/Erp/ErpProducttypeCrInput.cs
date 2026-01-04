namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品分类管理修改输入参数.
/// </summary>
public class ErpProducttypeCrInput
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
    /// 分类编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 分类说明.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 序号：排序规则数字越大越靠前。
    /// </summary>
    public int? order { get; set; }
}