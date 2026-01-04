namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品图片输出参数.
/// </summary>
public class ErpProductpicInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 文件id
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string pic { get; set; }
}


/// <summary>
/// 商品公司关联输出.
/// </summary>
public class ErpProductcompanyInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }
}