using QT.DependencyInjection;
using QT.JXC.Entitys.Dto;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.ErpOutorderDb;

public class ErpOutorderDbImportData: BaseImportDataInput
{
    /// <summary>
    /// 调出公司.
    /// </summary>
    [Display(Name = "调出公司")]
    [Required(ErrorMessage = "调出公司必填")]
    public string Oid1Name { get; set; }

    /// <summary>
    /// 调入公司.
    /// </summary>
    [Display(Name = "调入公司")]
    [Required(ErrorMessage = "调入公司必填")]
    public string Oid2Name { get; set; }


    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    [Required(ErrorMessage = "商品名称必填")]
    public string ProductName { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    [Display(Name = "规格")]
    [Required(ErrorMessage = "规格必填")]
    public string MidName { get; set; }
    /// <summary>
    /// 单位.
    /// </summary>
    [Display(Name = "单位")]
    //[Required(ErrorMessage = "单位必填")]
    public string ProductUnit { get; set; }
    /// <summary>
    /// 数量.
    /// </summary>
    [Display(Name = "数量")]
    public string Num { get; set; }
    /// <summary>
    /// 单价.
    /// </summary>
    [Display(Name = "单价")]
    public string Price { get; set; }
    /// <summary>
    /// 金额.
    /// </summary>
    [Display(Name = "金额")]
    public string Amount { get; set; }

    public string oid1 { get; set; }
    public string oid2 { get; set; }
    public string mid { get; set; }
}

public class ErpOutorderDbImportErrorData : ErpOutorderDbImportData
{
    /// <summary>
    /// 错误信息
    /// </summary>
    [Display(Name = "错误信息")]
    public override string ErrorMessage { get; set; }
}

/// <summary>
/// 调出数据导入 输入.
/// </summary>
[SuppressSniffer]
public class ErpOutorderDbImportDataInput
{
    /// <summary>
    /// 导入的数据列表.
    /// </summary>
    public List<ErpOutorderDbImportData> list { get; set; }
}

/// <summary>
/// 调出 导出 结果 输出.
/// </summary>
[SuppressSniffer]
public class ErpOutorderDbImportResultOutput
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
    public List<ErpOutorderDbImportData> failResult { get; set; }

    /// <summary>
    /// 失败结果集合文件地址
    /// </summary>
    public string failFileUrl { get; set; }
}
