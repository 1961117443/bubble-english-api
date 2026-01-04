using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 楼栋表
/// </summary>
[SugarTable("prm_building")]
public class BuildingEntity : CLDEntityBase
{
    /// <summary>
    /// 苑区ID
    /// </summary>
    [SugarColumn(ColumnName = "community_id")]
    public string CommunityId { get; set; }

    /// <summary>
    /// 楼栋编码
    /// </summary>
    [SugarColumn(ColumnName = "code")]
    public string Code { get; set; }

    /// <summary>
    /// 总楼层数
    /// </summary>
    [SugarColumn(ColumnName = "total_floors")]
    public int? TotalFloors { get; set; }

    /// <summary>
    /// 楼栋类型
    /// </summary>
    [SugarColumn(ColumnName = "building_type")]
    public BuildingType BuildingType { get; set; } = BuildingType.住宅;

    /// <summary>
    /// 竣工日期
    /// </summary>
    [SugarColumn(ColumnName = "completion_date")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// 附件JSON
    /// </summary>
    [SugarColumn]
    public string AttachmentJson { get; set; }
}

//  
public enum BuildingType { 住宅 = 1, 商业 = 2, 厂房 = 3, 混合 = 4 }
