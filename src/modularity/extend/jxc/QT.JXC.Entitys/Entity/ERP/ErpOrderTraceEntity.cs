using QT.Common.Const;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 订单溯源信息实体.
/// </summary>
[SugarTable("erp_ordertrace")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOrderTraceEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 溯源编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 订单明细id.
    /// </summary>
    [SugarColumn(ColumnName = "OrderDetailId")]
    public string OrderDetailId { get; set; }

    /// <summary>
    /// 查询次数.
    /// </summary>
    [SugarColumn(ColumnName = "Num")]
    public int Num { get; set; }

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
    /// 首次查询时间.
    /// </summary>
    [SugarColumn(ColumnName = "FirstQueryTime")]
    public DateTime? FirstQueryTime { get; set; }

    /// <summary>
    /// 最后查询时间.
    /// </summary>
    [SugarColumn(ColumnName = "LastQueryTime")]
    public DateTime? LastQueryTime { get; set; }

    /// <summary>
    /// 快照内容.
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }

    /// <summary>
    /// 小程序二维码地址.
    /// </summary>
    [SugarColumn(ColumnName = "QRCodeUrl")]
    public string QRCodeUrl { get; set; }
}