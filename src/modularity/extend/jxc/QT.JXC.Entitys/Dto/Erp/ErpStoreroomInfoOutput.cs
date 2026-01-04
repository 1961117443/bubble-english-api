namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 仓库信息输出参数.
/// </summary>
public class ErpStoreroomInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 父级仓库ID.
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
    /// 仓库地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 联系电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string admintel { get; set; }

    /// <summary>
    /// 库区信息.
    /// </summary>
    public List<ErpStoreareaInfoOutput> erpStoreareaList { get; set; }

    /// <summary>
    /// 仓库覆盖路线(中间表）.
    /// </summary>
    public List<ErpStoreDeliveryInfoOutput> erpStoreDeliveryList { get; set; }

    /// <summary>
    /// 关联公司集合.
    /// </summary>
    public List<string> erpStoreCompanyList { get; set; }

}