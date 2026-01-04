using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 销售订单
/// </summary>
[SugarTable("crm_order")]
public class CrmOrderEntity : CUDEntityBase
{

    /// <summary>
    /// 订单编号
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 订单日期
    /// </summary>
    [SugarColumn(ColumnName = "OrderDate")]
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// 业务员id
    /// </summary>
    [SugarColumn(ColumnName = "UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 合同金额
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }


    /// <summary>
    /// 附件
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }


}
