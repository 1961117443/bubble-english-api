using QT.Logistics.Entitys.Dto.LogOrder;

namespace QT.Logistics.Entitys.Dto.LogOrderAccount;

public class LogOrderAccountListQueryInput: LogOrderListQueryInput
{
    /// <summary>
    /// 已分账=1，未分账=0
    /// </summary>
    public int? accountStatus { get; set; }

    /// <summary>
    /// 数据范围 scope=point只看当前用户绑定的配送点
    /// </summary>
    public string scope { get; set; }
}
