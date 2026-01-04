using QT.Application.Entitys.Enum.FreshDelivery;
using QT.Common.Const;
using QT.Common.Contracts;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 订单信息实体.
/// </summary>
[SugarTable("erp_order")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOrderEntity : ICompanyEntity,IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreateUid")]
    public string CreateUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreateTime")]
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Cid")]
    public string Cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_Posttime")]
    public DateTime? Posttime { get; set; }

    /// <summary>
    /// 配送金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount1")]
    public decimal Amount1 { get; set; }

    /// <summary>
    /// 复核金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount2")]
    public decimal Amount2 { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 配送费.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryFee")]
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// 送货人.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryManId")]
    public string DeliveryManId { get; set; }

    /// <summary>
    /// 送货车辆.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryCar")]
    public string DeliveryCar { get; set; }

    /// <summary>
    /// 配送凭据.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryProof")]
    public string DeliveryProof { get; set; }

    /// <summary>
    /// 配送时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryTime")]
    public DateTime? DeliveryTime { get; set; }

    /// <summary>
    /// 送达凭据.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryToProof")]
    public string DeliveryToProof { get; set; }

    /// <summary>
    /// 送达时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeliveryToTime")]
    public DateTime? DeliveryToTime { get; set; }

    /// <summary>
    /// 收货确认状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReceiveState")]
    public string ReceiveState { get; set; }

    /// <summary>
    /// 收货确认时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReceiveTime")]
    public DateTime? ReceiveTime { get; set; }

    /// <summary>
    /// 结算方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckOutType")]
    public string CheckOutType { get; set; }

    /// <summary>
    /// 对帐单开具日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_BillDate")]
    public DateTime? BillDate { get; set; }

    /// <summary>
    /// 发票开具日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_InvoiceDate")]
    public DateTime? InvoiceDate { get; set; }

    /// <summary>
    /// 收款日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReceiptsDate")]
    public DateTime? ReceiptsDate { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_State",ColumnDataType = "int")]
    public OrderStateEnum? State { get; set; }

    /// <summary>
    /// 出库单打印状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintState")]
    public string PrintState { get; set; }

    /// <summary>
    /// 出库单打印时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintTime")]
    public string PrintTime { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }


    /// <summary>
    /// 餐别.
    /// </summary>
    [SugarColumn(ColumnName = "F_DiningType")]
    public string DiningType { get; set; }

    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime? DeleteTime { get; set; }

    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string DeleteUserId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}