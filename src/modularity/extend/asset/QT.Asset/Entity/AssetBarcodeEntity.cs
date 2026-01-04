using QT.Common.Contracts;

namespace QT.Asset.Entity;

using QT.Common.Filter;
using SqlSugar;
using System;
using System.ComponentModel;

/// <summary>
/// 条码实例实体
/// </summary>
[SugarTable("asset_barcodes")]
public class AssetBarcodeEntity : CLDEntityBase
{
    /// <summary>
    /// 规则ID
    /// </summary>
    [SugarColumn(ColumnName = "rule_id", Length = 50, IsNullable = false)]
    public string RuleId { get; set; }

    /// <summary>
    /// 条码编号
    /// </summary>
    [SugarColumn(ColumnName = "barcode_number", Length = 100, IsNullable = false)]
    public string BarcodeNumber { get; set; }

    /// <summary>
    /// 二维码图片路径
    /// </summary>
    [SugarColumn(ColumnName = "barcode_image_url", Length = 255, IsNullable = true)]
    public string BarcodeImageUrl { get; set; }

    /// <summary>
    /// 绑定类型：Asset/Warehouse
    /// </summary>
    [SugarColumn(ColumnName = "bind_type", Length = 20, IsNullable = true)]
    public string BindType { get; set; }

    /// <summary>
    /// 绑定对象ID
    /// </summary>
    [SugarColumn(ColumnName = "bind_id", Length = 50, IsNullable = true)]
    public string BindId { get; set; }

    /// <summary>
    /// 状态：Unbound/Bound/Disabled
    /// </summary>
    [SugarColumn(ColumnName = "status", Length = 20, IsNullable = true)]
    public AssetBarcodeStatus Status { get; set; }

    /// <summary>
    /// 绑定时间
    /// </summary>
    [SugarColumn(ColumnName = "bind_time", IsNullable = true)]
    public DateTime? BindTime { get; set; }
}


public enum AssetBarcodeStatus
{
    [Description("未绑定"), TagStyle("primary")] Unbound=0,
    [Description("已绑定"), TagStyle("success")] Bound=1,
    [Description("禁用"), TagStyle("danger")] Disabled=99
}