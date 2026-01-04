using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcCertificateOut;

/// <summary>
/// 建筑人才档案寄件列表查询输入
/// </summary>
public class JzrcCertificateOutListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 寄件时间.
    /// </summary>
    public string outTime { get; set; }

    /// <summary>
    /// 快递单号.
    /// </summary>
    public string expressNo { get; set; }

}