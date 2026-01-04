namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 供货商信息输入参数.
/// </summary>
public class ErpSupplierListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string adminTel { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    public DateTime? joinTime { get; set; }

    /// <summary>
    /// 经营周期.
    /// </summary>
    public string workCycle { get; set; }

    /// <summary>
    /// 经营时间.
    /// </summary>
    public string workTime { get; set; }

    /// <summary>
    /// 业务人员.
    /// </summary>
    public string salesman { get; set; }

    /// <summary>
    /// 营业执照
    /// </summary>
    public string businessLicense { get; set; }

    /// <summary>
    /// 生产许可
    /// </summary>
    public string productionLicense { get; set; }

    /// <summary>
    /// 食品经营许可证
    /// </summary>
    public string foodBusinessLicense { get; set; }
}