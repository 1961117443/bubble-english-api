using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Visitor;

/// <summary>
/// 访客登记创建输入
/// </summary>
[SuppressSniffer]
public class VisitorCrInput
{
    /// <summary>
    /// 住户ID
    /// </summary>
    [Required(ErrorMessage = "住户ID不能为空")]
    public string residentId { get; set; }

    /// <summary>
    /// 访客姓名
    /// </summary>
    [Required(ErrorMessage = "访客姓名不能为空")]
    [StringLength(100, ErrorMessage = "访客姓名不能超过100字符")]
    public string name { get; set; }

    /// <summary>
    /// 访客电话
    /// </summary>
    [Required(ErrorMessage = "访客电话不能为空")]
    [Phone(ErrorMessage = "访客电话格式错误")]
    public string phone { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    [StringLength(20, ErrorMessage = "车牌号不能超过20字符")]
    public string carPlate { get; set; }

    /// <summary>
    /// 访问日期
    /// </summary>
    [Required(ErrorMessage = "访问日期不能为空")]
    public DateTime? visitDate { get; set; }

    /// <summary>
    /// 访问状态
    /// </summary>
    [Required(ErrorMessage = "访问状态不能为空")]
    public VisitorStatus status { get; set; } =  VisitorStatus.待审核;
}
