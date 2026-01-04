using QT.Asset.Dto.Asset;
using QT.Common.Filter;
using QT.DependencyInjection;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetScrap;

/// <summary>
/// 资产报废输入
/// </summary>
[SuppressSniffer]
public class AssetScrapCrInput
{
    /// <summary>
    /// 报废资产ID
    /// </summary>
    [Required]
    public string assetId { get; set; }

    /// <summary>
    /// 报废原因说明
    /// </summary>
    public string scrapReason { get; set; }

    /// <summary>
    /// 处置方式：报废/遗失/销毁/捐赠/变卖
    /// </summary>
    public int? scrapType { get; set; }


    /// <summary>
    /// 实际报废时间
    /// </summary>
    public DateTime? scrapTime { get; set; }

    /// <summary>
    /// 报废凭证文件（如销毁证明）
    /// </summary>
    public string attachmentUrl { get; set; }
}



[SuppressSniffer]
public class AssetScrapInfoOutput : AssetScrapCrInput
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
public class AssetScrapListOutput : AssetScrapInfoOutput
{
    public string assetCode { get; set; }

    public string assetName { get; set; }

    /// <summary>
    /// 条形码
    /// </summary>
    public string barcode { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetScrapListPageInput : PageInputBase
{
    public string assetId { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetScrapUpInput : AssetScrapCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
