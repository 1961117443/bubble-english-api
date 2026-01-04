using QT.Common.Contracts;
using SqlSugar;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 售电系统-用电分时数据
/// </summary>
[SugarTable("sdms_customer_electricityusage")]
public class CustomerElectricityusageEntity : CUDEntityBase
{
    /// <summary>
    /// 计量点ID
    /// </summary>
    [SugarColumn(ColumnName = "MeterPointId")]
    public string MeterPointId { get; set; }

    /// <summary>
    /// 数据日期
    /// </summary>
    [SugarColumn(ColumnName = "RecordDate")]
    public DateTime? RecordDate { get; set; }


    /// <summary>
    /// 峰用电量
    /// </summary>
    [SugarColumn(ColumnName = "PeakUsage")]
    public decimal? PeakUsage { get; set; }


    /// <summary>
    /// 谷用电量
    /// </summary>
    [SugarColumn(ColumnName = "ValleyUsage")]
    public decimal? ValleyUsage { get; set; }


    /// <summary>
    /// 平用电量
    /// </summary>
    [SugarColumn(ColumnName = "FlatUsage")]
    public decimal? FlatUsage { get; set; }


    /// <summary>
    /// 尖用电量
    /// </summary>
    [SugarColumn(ColumnName = "SpikeUsage")]
    public decimal? SpikeUsage { get; set; }


    /// <summary>
    /// 合计用电量
    /// </summary>
    [SugarColumn(ColumnName = "TotalUsage")]
    public decimal? TotalUsage { get; set; }

}

