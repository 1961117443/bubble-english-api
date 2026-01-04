using QT.Common.Security;
namespace QT.SDMS.Entitys.Dto.CustomerElectricityusage;

public class CustomerElectricityusageListOutput : CustomerElectricityusageOutput
{

    /// <summary>
    /// 客户id
    /// </summary>
    public string customerId { get; set; }


    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerIdName { get; set; }

    /// <summary>
    /// 计量点
    /// </summary>
    public string meterPointIdCode { get; set; }
}
