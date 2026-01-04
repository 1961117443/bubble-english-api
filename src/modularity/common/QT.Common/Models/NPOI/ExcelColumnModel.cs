using NPOI.SS.UserModel;
using QT.DependencyInjection;
using System.Drawing;

namespace QT.Common.Models.NPOI;

/// <summary>
/// Excel导出列名
/// </summary>
[SuppressSniffer]
public class ExcelColumnModel
{
    /// <summary>
    /// 列名.
    /// </summary>
    public string? Column { get; set; }

    /// <summary>
    /// Excel列名.
    /// </summary>
    public string? ExcelColumn { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    public int Width { get; set; } = 10;

    /// <summary>
    /// 前景色.
    /// </summary>
    public Color ForeColor { get; set; }

    /// <summary>
    /// 背景色.
    /// </summary>
    public Color Background { get; set; }

    /// <summary>
    /// 字体.
    /// </summary>
    public string? Font { get; set; }

    /// <summary>
    /// 字号.
    /// </summary>
    public short Point { get; set; }

    /// <summary>
    /// 对齐方式
    /// left 左
    /// center 中间
    /// right 右
    /// fill 填充
    /// justify 两端对齐
    /// centerselection 跨行居中
    /// distributed.
    /// </summary>
    public string? Alignment { get; set; }

    /// <summary>
    /// 是否为图片类型
    /// </summary>
    public bool isPicture { get; set; }
}