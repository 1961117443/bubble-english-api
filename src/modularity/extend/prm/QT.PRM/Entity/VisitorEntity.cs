using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 访客登记表
/// </summary>
[SugarTable("prm_visitor")]
public class VisitorEntity : CLDEntityBase
{
    /// <summary>
    /// 住户ID
    /// </summary>
    [SugarColumn(ColumnName = "resident_id")]
    public string ResidentId { get; set; }

    /// <summary>
    /// 访客姓名
    /// </summary>
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; }

    /// <summary>
    /// 访客电话
    /// </summary>
    [SugarColumn(ColumnName = "phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    [SugarColumn(ColumnName = "car_plate")]
    public string CarPlate { get; set; }

    /// <summary>
    /// 访问日期
    /// </summary>
    [SugarColumn(ColumnName = "visit_date")]
    public DateTime? VisitDate { get; set; }

    /// <summary>
    /// 访问状态
    /// </summary>
    [SugarColumn]
    public VisitorStatus Status { get; set; }
}

public enum VisitorStatus { 待审核 = 1, 已通过 = 2, 已过期 = 3 }