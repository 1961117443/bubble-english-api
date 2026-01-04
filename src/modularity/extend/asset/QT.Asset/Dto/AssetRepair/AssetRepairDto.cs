using QT.Asset.Dto.Asset;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetCategory;

/// <summary>
/// 资产维修创建输入模型
/// </summary>
[SuppressSniffer]
public class AssetRepairCrInput
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [Required(ErrorMessage = "资产ID不能为空")]
    public string assetId { get; set; }

    /// <summary>
    /// 报修人ID
    /// </summary>
    public string reportUserId { get; set; }

    /// <summary>
    /// 故障描述
    /// </summary>
    [Required(ErrorMessage = "故障描述不能为空")]
    public string faultDesc { get; set; }

    /// <summary>
    /// 报修时间
    /// </summary>
    [Required(ErrorMessage = "报修时间不能为空")]
    public DateTime? reportTime { get; set; }
}

/// <summary>
/// 资产维修完成输入模型
/// </summary>
[SuppressSniffer]
public class AssetRepairCompleteInput
{
    /// <summary>
    /// 维修记录ID
    /// </summary>
    [Required(ErrorMessage = "记录ID不能为空")]
    public string id { get; set; }

    /// <summary>
    /// 维修人ID
    /// </summary>
    [Required(ErrorMessage = "维修人ID不能为空")]
    public string repairUserId { get; set; }

    /// <summary>
    /// 维修费用
    /// </summary>
    public decimal? repairCost { get; set; }

    /// <summary>
    /// 维修备注
    /// </summary>
    public string remark { get; set; }
}


[SuppressSniffer]
public class AssetRepairInfoOutput : AssetRepairCrInput
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
public class AssetRepairListOutput : AssetRepairInfoOutput
{
    public string assetCode { get; set; }

    public string assetName { get; set; }

    /// <summary>
    /// 报修人
    /// </summary>
    public string reportUserName { get; set; }

    /// <summary>
    /// 条形码
    /// </summary>
    public string barcode { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetRepairListPageInput : PageInputBase
{

    public string assetId { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetRepairUpInput : AssetRepairCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
