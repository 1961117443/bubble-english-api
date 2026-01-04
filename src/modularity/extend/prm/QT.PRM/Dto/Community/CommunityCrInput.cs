using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Community;

/// <summary>
/// 苑区创建输入
/// </summary>
[SuppressSniffer]
public class CommunityCrInput
{
    /// <summary>
    /// 苑区名称
    /// </summary>
    [Required(ErrorMessage = "苑区名称不能为空")]
    [StringLength(100, ErrorMessage = "名称长度不能超过100字符")]
    public string name { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string city { get; set; }

    /// <summary>
    /// 区县
    /// </summary>
    public string district { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    public string addressDetail { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [Range(-180, 180, ErrorMessage = "经度范围无效")]
    public decimal? longitude { get; set; }

    /// <summary>
    /// 纬度
    /// </summary>
    [Range(-90, 90, ErrorMessage = "纬度范围无效")]
    public decimal? latitude { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public string attachmentJson { get; set; }
}
