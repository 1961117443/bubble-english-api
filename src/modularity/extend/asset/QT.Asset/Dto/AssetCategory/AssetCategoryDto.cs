using QT.Asset.Dto.AssetAttributeDefinition;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetCategory;

/// <summary>
/// 资产分类输入
/// </summary>
[SuppressSniffer]
public class AssetCategoryCrInput
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(100, ErrorMessage = "分类名称长度不能超过100")]
    public string name { get; set; }

    /// <summary>
    /// 父分类ID
    /// </summary>
    [StringLength(50, ErrorMessage = "父分类ID长度不能超过50")]
    public string parentId { get; set; }

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public List<AssetAttributeDefinitionDto> fields { get; set; }
}



[SuppressSniffer]
public class AssetCategoryInfoOutput : AssetCategoryCrInput
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
public class AssetCategoryListOutput : AssetCategoryInfoOutput
{

}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetCategoryListPageInput : PageInputBase
{
    /// <summary>
    /// 分类名称
    /// </summary>
    public string name { get; set; }
}

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AssetCategoryUpInput : AssetCategoryCrInput
{
    /// <summary>
    /// 分类ID
    /// </summary>
    [Required(ErrorMessage = "分类ID不能为空")]
    public string id { get; set; }
}
