
namespace QT.Emp.Entitys.Dto.BaseModuleremind;

/// <summary>
/// 模块提醒修改输入参数.
/// </summary>
public class BaseModuleremindCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    public long sortCode { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    public bool enabledMark { get; set; }

    /// <summary>
    /// sql语句.
    /// </summary>
    public string sqlTemplate { get; set; }

    /// <summary>
    /// 功能主键.
    /// </summary>
    public string moduleId { get; set; }

}