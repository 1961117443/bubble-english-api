using QT.Common.Filter;
using QT.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Asset.Dto.AssetAttributeDefinition;

/// <summary>
/// 资产属性定义输出模型
/// </summary>
[SuppressSniffer]
public class AssetAttributeDefinitionDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [Required(ErrorMessage = "分类ID不能为空")]
    [StringLength(50, ErrorMessage = "分类ID长度不能超过50")]
    public string categoryId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Display(Name = "字段名")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string title { get; set; }

    /// <summary>
    /// 控件类型
    /// </summary>
    [Display(Name = "控件类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? controlType { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    [Display(Name = "选项列表")]
    public string? itemOption { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [Display(Name = "默认值")]
    [StringLength(512)]
    public string? defaultValue { get; set; }

    /// <summary>
    /// 是否密码框
    /// </summary>
    [Display(Name = "是否密码框")]
    [Range(0, 9)]
    public byte isPassword { get; set; } = 0;

    /// <summary>
    /// 是否必填0非必填1必填
    /// </summary>
    [Display(Name = "是否必填")]
    [Range(0, 9)]
    public byte isRequired { get; set; } = 0;

    /// <summary>
    /// 编辑器0标准型1简洁型
    /// </summary>
    [Display(Name = "编辑器")]
    [Range(0, 9)]
    public byte editorType { get; set; } = 0;

    /// <summary>
    /// 验证提示信息
    /// </summary>
    [Display(Name = "验证提示信息")]
    [StringLength(255)]
    public string? validTipMsg { get; set; }

    /// <summary>
    /// 验证失败提示信息
    /// </summary>
    [Display(Name = "验证失败提示信息")]
    [StringLength(255)]
    public string? validErrorMsg { get; set; }

    /// <summary>
    /// 验证正则表达式
    /// </summary>
    [Display(Name = "验证正则表达式")]
    [StringLength(255)]
    public string? validPattern { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 多选项
    /// </summary>
    public object? options { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    public object? fieldValue { get; set; }
}