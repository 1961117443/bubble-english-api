using QT.Common.Models;
using QT.Logistics.Entitys.Dto.LogEnterpriseProductmodel;
using System.ComponentModel.DataAnnotations;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseProduct;

/// <summary>
/// 商家商品信息修改输入参数.
/// </summary>
public class LogEnterpriseProductCrInput
{
    /// <summary>
    /// 分类ID.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    public string producer { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public int? sort { get; set; }

    /// <summary>
    /// 介绍.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    public string storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public bool state { get; set; }

    /// <summary>
    /// 商家商品规格.
    /// </summary>
    public List<LogEnterpriseProductmodelCrInput> logEnterpriseProductmodelList { get; set; }

    /// <summary>
    /// 简图.
    /// </summary>
    public List<FileControlsModel> imageUrl { get; set; }

}