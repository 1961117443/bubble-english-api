
namespace QT.Emp.Entitys.Dto.BaseModuleremind;

/// <summary>
/// 模块提醒输出参数.
/// </summary>
public class BaseModuleremindInfoOutput
{
    /// <summary>
    /// 主键_id.
    /// </summary>
    public string id { get; set; }

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
    public int? enabledMark { get; set; }

    /// <summary>
    /// sql语句.
    /// </summary>
    public string sqlTemplate { get; set; }

    /// <summary>
    /// 功能主键.
    /// </summary>
    public string moduleId { get; set; }

}