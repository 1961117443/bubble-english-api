
namespace QT.Logistics.Entitys.Dto.LogEnterpriseStore;

/// <summary>
/// 入驻商铺输出参数.
/// </summary>
public class LogEnterpriseStoreInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    public string eId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    public string storeNumber { get; set; }

    /// <summary>
    /// 商铺位置.
    /// </summary>
    public string storeLocation { get; set; }

    /// <summary>
    /// 商铺面积.
    /// </summary>
    public string storeArea { get; set; }

    /// <summary>
    /// 商铺租金.
    /// </summary>
    public string storeRent { get; set; }

    /// <summary>
    /// 起租时间.
    /// </summary>
    public DateTime? leaseStartTime { get; set; }

    /// <summary>
    /// 合同期限.
    /// </summary>
    public DateTime? contractDuration { get; set; }

}