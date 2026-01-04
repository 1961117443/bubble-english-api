using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 省市区(显示)
/// </summary>
public class AreasDto : AreasEditDto
{
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 子地区列表
    /// </summary>
    public List<AreasDto> children { get; set; } = new List<AreasDto>();
}

/// <summary>
/// 省市区(编辑)
/// </summary>
public class AreasEditDto
{
    [Display(Name = "父级地区")]
    public int parentId { get; set; } = 0;

    [Display(Name = "地区名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    [Display(Name = "排序数字")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int sortId { get; set; } = 99;
}

/// <summary>
/// 省市区(导入)
/// </summary>
public class AreasImportDto
{
    [Display(Name = "地区名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? name { get; set; }

    /// <summary>
    /// 子地区列表
    /// </summary>
    public List<AreasImportDto> children { get; set; } = new List<AreasImportDto>();
}

/// <summary>
/// 省市区(导入)
/// </summary>
public class AreasImportEditDto
{
    [Display(Name = "地区数据")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? jsonData { get; set; }
}
