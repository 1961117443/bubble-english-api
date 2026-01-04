using QT.Asset.Entity;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetBarcode;

/// <summary>
/// 条码实例创建输入模型
/// </summary>
[SuppressSniffer]
public class AssetBarcodeCrInput
{
    /// <summary>
    /// 规则ID
    /// </summary>
    [Required(ErrorMessage = "规则ID不能为空")]
    public string ruleId { get; set; }

    /// <summary>
    /// 条码编号
    /// </summary>
    [Required(ErrorMessage = "条码编号不能为空")]
    [StringLength(100, ErrorMessage = "条码编号长度不能超过100")]
    public string barcodeNumber { get; set; }
}

/// <summary>
/// 条码实例绑定输入模型
/// </summary>
[SuppressSniffer]
public class AssetBarcodeBindInput
{
    /// <summary>
    /// 条码ID
    /// </summary>
    [Required(ErrorMessage = "条码不能为空")]
    public string barcodeNumber { get; set; }

    /// <summary>
    /// 绑定类型：Asset/Warehouse
    /// </summary>
    [Required(ErrorMessage = "绑定类型不能为空")]
    [StringLength(20, ErrorMessage = "绑定类型长度不能超过20")]
    public string bindType { get; set; }

    /// <summary>
    /// 绑定对象ID
    /// </summary>
    [Required(ErrorMessage = "绑定对象ID不能为空")]
    [StringLength(50, ErrorMessage = "绑定对象ID长度不能超过50")]
    public string bindId { get; set; }
}

/// <summary>
/// 条码实例输出模型
/// </summary>
[SuppressSniffer]
public class AssetBarcodeDto
{
    /// <summary>
    /// 条码ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 规则ID
    /// </summary>
    public string ruleId { get; set; }

    /// <summary>
    /// 条码编号
    /// </summary>
    public string barcodeNumber { get; set; }

    /// <summary>
    /// 二维码图片路径
    /// </summary>
    public string barcodeImageUrl { get; set; }

    /// <summary>
    /// 绑定类型
    /// </summary>
    public string bindType { get; set; }

    /// <summary>
    /// 绑定对象ID
    /// </summary>
    public string bindId { get; set; }

    /// <summary>
    /// 状态：Unbound/Bound/Disabled
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 绑定时间
    /// </summary>
    public DateTime? bindTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 条码规则
    /// </summary>
    public string ruleName { get; set; }
}


/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetBarcodeListPageInput : PageInputBase
{
    public AssetBarcodeStatus? status { get; set; }
}
