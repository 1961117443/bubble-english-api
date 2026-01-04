namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;

/// <summary>
/// 订单备注记录修改输入参数.
/// </summary>
public class ErpOrderremarksCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 备注内容.
    /// </summary>
    public string remark { get; set; }

}