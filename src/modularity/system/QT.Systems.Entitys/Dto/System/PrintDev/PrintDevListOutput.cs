using System.Text.Json.Serialization;
using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.PrintDev;

/// <summary>
/// 打印模板列表输出.
/// </summary>
[SuppressSniffer]
public class PrintDevListOutput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 创建人.
    /// </summary>
    public string creatorUser { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 修改人.
    /// </summary>
    public string lastModifyUser { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    public DateTime? lastModifyTime { get; set; }

    /// <summary>
    /// 状态(0-关闭，1-开启).
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 流程分类(数据字典-工作流-流程分类).
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 流程分类(数据字典-工作流-流程分类).
    /// </summary>
    public long? sortCode { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 绑定模块.
    /// </summary>
    public string[] moduleName { get; set; }
}