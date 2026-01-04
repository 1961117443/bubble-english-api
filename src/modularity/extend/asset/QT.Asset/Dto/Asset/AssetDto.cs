using QT.Asset.Entity;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.Asset;

/// <summary>
/// 输入
/// </summary>
[SuppressSniffer]
public class AssetCrInput
{
    /// <summary>
    /// 资产编号
    /// </summary>
    //[Required(ErrorMessage = "资产编号不能为空")]
    [StringLength(50, ErrorMessage = "资产编号长度不能超过50")]
    public string assetCode { get; set; }

    /// <summary>
    /// 资产名称
    /// </summary>
    [Required(ErrorMessage = "资产名称不能为空")]
    [StringLength(100, ErrorMessage = "资产名称长度不能超过100")]
    public string name { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [StringLength(50, ErrorMessage = "分类ID长度不能超过50")]
    public string categoryId { get; set; }

    /// <summary>
    /// 存放位置
    /// </summary>
    [StringLength(255, ErrorMessage = "存放位置长度不能超过255")]
    public string location { get; set; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    [StringLength(50, ErrorMessage = "仓库ID长度不能超过50")]
    public string warehouseId { get; set; }

    /// <summary>
    /// 责任人ID
    /// </summary>
    [StringLength(50, ErrorMessage = "责任人ID长度不能超过50")]
    public string dutyUserId { get; set; }

    /// <summary>
    /// 使用人id
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 状态：闲置0/在用1/维修中2/报废3
    /// </summary>
    [Range(0, 3, ErrorMessage = "状态值必须在0-3范围内")]
    public int status { get; set; } = 0;

    /// <summary>
    /// 条码
    /// </summary>
    public string? barcode { get; set; }

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public List<AssetAttributeValueDto> assetFields { get; set; }

    /// <summary>
    /// 购置时间
    /// </summary>
    public DateTime? purchaseDate { get; set; }

    /// <summary>
    /// 部门id
    /// </summary>
    public string? deptId { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public string? remark { get; set; }

    /// <summary>
    /// 图片附件
    /// </summary>
    public string attachmentJson { get; set; }
}



[SuppressSniffer]
public class AssetInfoOutput : AssetCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    public string dutyUserName { get; set; }
    public string userName { get; set; }
    public string deptName { get; set; }
    public string warehouseName { get; set; }

    public string barcode { get; set; }
}



/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetListOutput : AssetInfoOutput
{
    /// <summary>
    /// 分类
    /// </summary>
    public string categoryName { get; set; }

    /// <summary>
    /// 责任人
    /// </summary>
    public string dutyUserName { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetListPageInput : PageInputBase
{
    public string code { get; set; }

    public string  name { get; set; }
    public string categoryId { get; set; }

    public AssetStatus? status { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetUpInput : AssetCrInput
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [Required(ErrorMessage = "资产ID不能为空")]
    public string id { get; set; }
}
