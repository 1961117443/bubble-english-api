using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetBarcode;

/// <summary>
/// 条码规则更新输入模型
/// </summary>
[SuppressSniffer]
public class AssetBarcodeRuleCrInput
{
    /// <summary>
    /// 规则名称
    /// </summary>
    [Required(ErrorMessage = "规则名称不能为空")]
    [StringLength(100, ErrorMessage = "规则名称长度不能超过100")]
    public string ruleName { get; set; }

    /// <summary>
    /// 条码类型：Asset/Warehouse
    /// </summary>
    [Required(ErrorMessage = "条码类型不能为空")]
    [StringLength(20, ErrorMessage = "条码类型长度不能超过20")]
    public string barcodeType { get; set; }

    /// <summary>
    /// 编码前缀
    /// </summary>
    [StringLength(20, ErrorMessage = "编码前缀长度不能超过20")]
    public string prefix { get; set; } = string.Empty;

    /// <summary>
    /// 补零位数
    /// </summary>
    [Range(1, 10, ErrorMessage = "补零位数必须在1-10范围内")]
    public int zeroPadding { get; set; } = 5;

    /// <summary>
    /// 二维码样式设置
    /// </summary>
    public string style { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    public string dateFormat { get; set; }
}



[SuppressSniffer]
public class AssetBarcodeRuleInfoOutput : AssetBarcodeRuleCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}



/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetBarcodeRuleListOutput : AssetBarcodeRuleInfoOutput
{
    /// <summary>
    /// 当前流水号
    /// </summary>
    public int? lastSerialNumber { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetBarcodeRuleListPageInput : PageInputBase
{
}

/// <summary>
/// 条码规则更新输入模型
/// </summary>
[SuppressSniffer]
public class AssetBarcodeRuleUpInput : AssetBarcodeRuleCrInput
{
    /// <summary>
    /// 规则ID
    /// </summary>
    [Required(ErrorMessage = "规则ID不能为空")]
    public string id { get; set; }
}
