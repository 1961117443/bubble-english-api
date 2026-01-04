using QT.Common.Filter;

namespace QT.SDMS.Entitys.Dto.Customer;

public class CustomerListQueryInput:PageInputBase
{
    ///// <summary>
    ///// 订单日期范围
    ///// </summary>
    //public string orderDate { get; set; }

    /// <summary>
    /// 认证状态
    /// </summary>
    public int? status { get; set; }


    /// <summary>
    /// 
    /// </summary>
    public string managerId { get; set; }

    public string name { get; set; }

    public string contactName { get; set; }

    public string contactPhone { get; set; }
    public string email { get; set; }
}
