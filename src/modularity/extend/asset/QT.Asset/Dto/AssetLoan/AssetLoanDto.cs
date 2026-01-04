using QT.Asset.Dto.Asset;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetLoan;

/// <summary>
/// 资产领用创建输入模型
/// </summary>
[SuppressSniffer]
public class AssetLoanCrInput
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [Required(ErrorMessage = "资产ID不能为空")]
    public string assetId { get; set; }

    /// <summary>
    /// 使用人ID
    /// </summary>
    [Required(ErrorMessage = "使用人ID不能为空")]
    public string userId { get; set; }

    /// <summary>
    /// 领用时间
    /// </summary>
    [Required(ErrorMessage = "领用时间不能为空")]
    public DateTime loanTime { get; set; }
}

/// <summary>
/// 资产归还输入模型
/// </summary>
[SuppressSniffer]
public class AssetLoanReturnInput
{
    /// <summary>
    /// 领用记录ID
    /// </summary>
    [Required(ErrorMessage = "记录ID不能为空")]
    public string id { get; set; }

    /// <summary>
    /// 归还时间
    /// </summary>
    [Required(ErrorMessage = "归还时间不能为空")]
    public DateTime returnTime { get; set; }
}



[SuppressSniffer]
public class AssetLoanInfoOutput : AssetLoanCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 资产信息
    /// </summary>
    public AssetInfoOutput assetInfo { get; set; }
}



/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetLoanListOutput : AssetLoanInfoOutput
{
    public string assetCode { get; set; }

    public string assetName { get; set; }

    public string userName { get; set; }
    public string barcode { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetLoanListPageInput : PageInputBase
{
    public string assetId { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetLoanUpInput : AssetLoanCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
