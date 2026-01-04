using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

[SuppressSniffer]
public class ErpBuyorderDetailListImportDataInput
{
    /// <summary>
    /// 商品.
    /// </summary>
    [Display(Name = "商品")]
    public string name { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    public string unit { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [Display(Name = "数量")]
    public decimal? num { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }
}

/// <summary>
/// 数据 导出 结果 输出.
/// </summary>
[SuppressSniffer]
public class ImportResultOutput<TS,T>
{
    /// <summary>
    /// 导入成功条数.
    /// </summary>
    public int snum { get; set; }

    /// <summary>
    /// 导入失败条数.
    /// </summary>
    public int fnum { get; set; }

    /// <summary>
    /// 导入结果状态(0：成功，1：失败).
    /// </summary>
    public int resultType { get; set; }

    /// <summary>
    /// 失败结果集合.
    /// </summary>
    public List<T> failResult { get; set; }


    /// <summary>
    /// 失败结果集合.
    /// </summary>
    public List<TS> successResult { get; set; }


    /// <summary>
    /// 失败结果集合文件地址
    /// </summary>
    public string failFileUrl { get; set; }
}

/// <summary>
/// 用户数据导入 输入.
/// </summary>
[SuppressSniffer]
public class ImportDataInput<T>
{
    /// <summary>
    /// 导入的数据列表.
    /// </summary>
    public List<T> list { get; set; }
}