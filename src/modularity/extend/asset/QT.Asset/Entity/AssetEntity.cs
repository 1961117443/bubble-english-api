using QT.Common.Contracts;

namespace QT.Asset.Entity;

using Aspose.Cells;
using QT.Common.Filter;
using SqlSugar;
using System.ComponentModel;

/// <summary>
/// 资产信息实体
/// </summary>
[SugarTable("assets")]
public class AssetEntity : CLDEntityBase
{
    /// <summary>
    /// 资产编号
    /// </summary>
    [SugarColumn(ColumnName = "asset_code", Length = 50, IsNullable = false)]
    [Description("资产编号")]
    public string AssetCode { get; set; }

    /// <summary>
    /// 资产名称
    /// </summary>
    [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
    [Description("资产名称")]
    public string Name { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [SugarColumn(ColumnName = "cid", Length = 50, IsNullable = true)]
    [Description("分类")]
    public string CategoryId { get; set; }

    /// <summary>
    /// 存放位置
    /// </summary>
    [SugarColumn(ColumnName = "location", Length = 255, IsNullable = true)]
    [Description("存放位置")]
    public string Location { get; set; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "wid", Length = 50, IsNullable = true)]
    [Description("仓库")]
    public string WarehouseId { get; set; }

    /// <summary>
    /// 责任人ID
    /// </summary>
    [SugarColumn(ColumnName = "duty_user_id", Length = 50, IsNullable = true)]
    [Description("责任人")]
    public string DutyUserId { get; set; }

    /// <summary>
    /// 使用人ID
    /// </summary>
    [SugarColumn(ColumnName = "user_id", Length = 50, IsNullable = true)]
    [Description("使用人")]
    public string UserId { get; set; }

    /// <summary>
    /// 状态：闲置0/在用1/维修中2/报废3
    /// </summary>
    [SugarColumn(ColumnName = "status", IsNullable = true)]
    [Description("状态")]
    public AssetStatus? Status { get; set; }

    /// <summary>
    /// 条码
    /// </summary>
    [SugarColumn(ColumnName = "barcode", IsNullable = true)]
    [Description("条码")]
    public string? Barcode { get; set; }



    /// <summary>
    /// 采购日期
    /// </summary>
    [SugarColumn(ColumnName = "purchase_date", IsNullable = true)]
    [Description("采购日期")]
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// 部门id
    /// </summary>
    [SugarColumn(ColumnName = "dept_id", IsNullable = true)]
    [Description("部门")]
    public string? DeptId { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "remark", IsNullable = true)]
    [Description("备注")]
    public string? Remark { get; set; }


    /// <summary>
    /// 图片集合
    /// </summary>
    [SugarColumn(ColumnName = "attachment_json", IsNullable = true)]
    [Description("图片")]
    public string? AttachmentJson { get; set; }
}



/// <summary>
/// 资产状态
/// </summary>
public enum AssetStatus
{
    /// <summary>
    /// 闲置
    /// </summary>
    [Description("闲置"), TagStyle("info")] Idle = 0,           // 闲置

    /// <summary>
    /// 在用
    /// </summary>
    [Description("在用"), TagStyle("success")] InUse = 1,          // 在用

    /// <summary>
    /// 维修中
    /// </summary>
    [Description("维修中"), TagStyle("warning")] Repairing = 2,      // 维修中

    /// <summary>
    /// 报废
    /// </summary>
    [Description("报废"), TagStyle("danger")] Scrapped = 3,       // 报废

    /// <summary>
    /// 已处置
    /// </summary>
    [Description("已处置"), TagStyle("info")] Disposed = 4,       // 已处置

    /// <summary>
    /// 遗失
    /// </summary>
    [Description("遗失"), TagStyle("info")] Lost = 5,           // 遗失

    /// <summary>
    /// 调拨中
    /// </summary>
    [Description("调拨中"), TagStyle("info")] Transferring = 6,   // 调拨中

    /// <summary>
    /// 盘点中
    /// </summary>
    [Description("盘点中"), TagStyle("info")] Inventorying = 7,   // 盘点中

    ///// <summary>
    ///// 待审批
    ///// </summary>
    //[Description("待审批")] UnderApproval = 8,  // 待审批

    /// <summary>
    /// 禁用
    /// </summary>
    [Description("禁用"), TagStyle("danger")] Disabled = 99        // 禁用
}
