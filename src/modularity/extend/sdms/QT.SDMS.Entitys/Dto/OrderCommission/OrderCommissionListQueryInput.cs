using QT.SDMS.Entitys.Dto.Order;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.OrderCommission;

public class OrderCommissionListQueryInput : OrderListQueryInput
{
    public SdmsCommissionStatus? status { get; set; }

    /// <summary>
    /// 销售人员
    /// </summary>
    public string userId { get; set; }
}