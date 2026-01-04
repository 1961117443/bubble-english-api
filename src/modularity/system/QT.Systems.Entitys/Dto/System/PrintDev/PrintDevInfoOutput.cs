using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.PrintDev;

/// <summary>
/// 打印模板信息输出.
/// </summary>
[SuppressSniffer]
public class PrintDevInfoOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }

    /// <summary>
    /// 数据连接id.
    /// </summary>
    public string dbLinkId { get; set; }

    /// <summary>
    /// sql模板.
    /// </summary>
    public string sqlTemplate { get; set; }

    /// <summary>
    /// 左侧字段.
    /// </summary>
    public string leftFields { get; set; }

    /// <summary>
    /// 打印模板.
    /// </summary>
    public string printTemplate { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    public string propertyJson { get; set; }
}

/// <summary>
/// 打印模板信息输出.
/// </summary>
[SuppressSniffer]
public class PrintDevInfoSelectorOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public string propertyJson {get;set;}
}