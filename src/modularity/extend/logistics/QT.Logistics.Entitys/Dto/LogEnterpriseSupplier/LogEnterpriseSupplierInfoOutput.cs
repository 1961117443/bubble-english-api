
namespace QT.Logistics.Entitys.Dto.LogEnterpriseSupplier;

/// <summary>
/// 供应商输出参数.
/// </summary>
public class LogEnterpriseSupplierInfoOutput
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
    /// 名称首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 联系人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 联系人电话.
    /// </summary>
    public string admintel { get; set; }

}