using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 管理日志(显示)
/// </summary>
public class ManagerLogDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    [Display(Name = "请求方法")]
    [StringLength(128)]
    public string? method { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    [Display(Name = "请求路径")]
    [StringLength(512)]
    public string? path { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    [Display(Name = "请求参数")]
    [StringLength(512)]
    public string? query { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    [Display(Name = "响应状态码")]
    [StringLength(128)]
    public string? statusCode { get; set; }

    /// <summary>
    /// 记录时间
    /// </summary>
    [Display(Name = "记录时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}
