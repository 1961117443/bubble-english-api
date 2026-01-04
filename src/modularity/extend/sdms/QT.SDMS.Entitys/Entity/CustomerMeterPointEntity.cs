using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户计量点表
/// </summary>
[SugarTable("sdms_customer_meter_point")]
public class CustomerMeterPointEntity : CUDEntityBase
{
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 电表编号
    /// </summary>
    [SugarColumn(ColumnName = "MeterCode")]
    public string MeterCode { get; set; }

    /// <summary>
    /// 计量点名称
    /// </summary>
    [SugarColumn(ColumnName = "MeterName")]
    public string MeterName { get; set; }

    /// <summary>
    /// 表计倍率
    /// </summary>
    [SugarColumn(ColumnName = "Multiplier")]
    public decimal? Multiplier { get; set; }


    /// <summary>
    /// 安装地址
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string? Address { get; set; }

    /// <summary>
    /// 电压等级
    /// </summary>
    [SugarColumn(ColumnName = "VoltageLevel")]
    public string? VoltageLevel { get; set; }

    /// <summary>
    /// 电表类型（1 智能表、2 普通表…）
    /// </summary>
    [SugarColumn(ColumnName = "MeterType")]
    public MeterType? MeterType { get; set; }

    /// <summary>
    /// 状态（1 启用，2 停用）
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int? Status { get; set; }
}


public enum MeterType:int
{
   [Description("智能电表")] Smart = 1,   // 智能电表
    [Description("普通电表")] Normal = 2   // 普通电表
}