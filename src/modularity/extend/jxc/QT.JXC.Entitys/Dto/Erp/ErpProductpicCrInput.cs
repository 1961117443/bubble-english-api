namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 商品图片修改输入参数.
/// </summary>
public class ErpProductpicCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string pic { get; set; }

    /// <summary>
    /// 备注（文件id）
    /// </summary>
    public string remark { get; set; }

}

/// <summary>
/// 商品关联公司输入参数.
/// </summary>
public class ErpProductcompanyCrInput
{
    public string id { get; set; }

    /// <summary>
    /// 公司id
    /// </summary>
    public string oid { get; set; }

}