using QT.Iot.Application.Dto.CrmOrderCommission;

namespace QT.Iot.Application.Dto.CrmOrder;

public class CrmOrderOutput: CrmOrderUpInput
{
    /// <summary>
    /// 佣金明细
    /// </summary>
    public List<CrmOrderCommissionOutput> crmOrderCommissions { get; set; }
}