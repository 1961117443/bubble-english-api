using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Asset.Dto.Asset;

/// <summary>
/// 资产属性值输出模型
/// </summary>
[SuppressSniffer]
public class AssetAttributeValueDto
{
    /// <summary>
    /// 属性值ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 资产ID
    /// </summary>
    public string assetId { get; set; }

    /// <summary>
    /// 所属字段ID
    /// </summary>
    [Display(Name = "所属字段")]
    public long fieldId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Display(Name = "字段名")]
    [StringLength(128)]
    public string? fieldName { get; set; }

    /// <summary>
    /// 字段值
    /// </summary>
    [Display(Name = "字段值")]
    public string? fieldValue { get; set; }
}

/// <summary>
/// 资产属性值详情模型（包含定义信息）
/// </summary>
[SuppressSniffer]
public class AssetAttributeValueDetailDto
{
    /// <summary>
    /// 属性定义ID
    /// </summary>
    public string attributeId { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string attributeName { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string dataType { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool isRequired { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    public List<string> options { get; set; } = new List<string>();

    /// <summary>
    /// 字段值
    /// </summary>
    public string value { get; set; }
}