using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Building;

/// <summary>
/// 楼栋创建输入
/// </summary>
[SuppressSniffer]
public class BuildingCrInput
{
    /// <summary>
    /// 苑区ID
    /// </summary>
    [Required]
    public string communityId { get; set; }

    /// <summary>
    /// 楼栋编号
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "楼栋编号不能超过20字符")]
    public string code { get; set; }

    /// <summary>
    /// 总楼层数
    /// </summary>
    public int? totalFloors { get; set; }

    /// <summary>
    /// 楼栋类型
    /// </summary>
    public int buildingType { get; set; }

    /// <summary>
    /// 竣工日期
    /// </summary>
    public DateTime? completionDate { get; set; }


    /// <summary>
    /// 附件图片
    /// </summary>
    public string attachmentJson { get; set; }
}
