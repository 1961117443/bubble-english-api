using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System.ComponentModel;

/// <summary>
/// 资产报废记录实体
/// </summary>
[SugarTable("asset_scraps")]
public class AssetScrapEntity : CLDEntityBase
{
    /// <summary>
    /// 报废资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 报废原因说明
    /// </summary>
    [SugarColumn(ColumnName = "scrap_reason", Length = 255, IsNullable = true)]
    public string ScrapReason { get; set; }

    /// <summary>
    /// 处置方式：报废/遗失/销毁/捐赠/变卖
    /// </summary>
    [SugarColumn(ColumnName = "scrap_type", Length = 20, IsNullable = true, DefaultValue = "报废")]
    public AssetScrapType? ScrapType { get; set; }

    /// <summary>
    /// 状态：已报废/待审批/驳回等
    /// </summary>
    [SugarColumn(ColumnName = "scrap_status", Length = 20, IsNullable = true, DefaultValue = "已报废")]
    public string ScrapStatus { get; set; }

    /// <summary>
    /// 实际报废时间
    /// </summary>
    [SugarColumn(ColumnName = "scrap_time", IsNullable = true, DefaultValue = "CURRENT_TIMESTAMP")]
    public DateTime ScrapTime { get; set; }

    /// <summary>
    /// 报废凭证文件（如销毁证明）
    /// </summary>
    [SugarColumn(ColumnName = "attachment_url", Length = 500, IsNullable = true)]
    public string AttachmentUrl { get; set; }
}

public enum AssetScrapType
{
    /// <summary>
    /// 报废
    /// </summary>
    [Description("报废")] Scrap = 1, // 报废

    /// <summary>
    /// 遗失
    /// </summary>
    [Description("遗失")] Lost = 2, // 遗失

    /// <summary>
    /// 销毁
    /// </summary>
    [Description("销毁")] Destroyed = 3, // 销毁

    /// <summary>
    /// 捐赠
    /// </summary>
    [Description("捐赠")] Donation = 4, // 捐赠

    /// <summary>
    /// 变卖
    /// </summary>
    [Description("变卖")] Sold = 5 // 变卖
}