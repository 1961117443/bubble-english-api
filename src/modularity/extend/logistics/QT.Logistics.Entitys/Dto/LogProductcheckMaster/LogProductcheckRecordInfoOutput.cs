
namespace QT.Logistics.Entitys.Dto.LogProductcheckRecord;

/// <summary>
/// 盘点记录输出参数.
/// </summary>
public class LogProductcheckRecordInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    public string gidName { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    public decimal systemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    public decimal realNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    public decimal loseNum { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}