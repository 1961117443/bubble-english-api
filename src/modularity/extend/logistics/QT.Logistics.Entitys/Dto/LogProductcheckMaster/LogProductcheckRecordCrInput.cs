
namespace QT.Logistics.Entitys.Dto.LogProductcheckRecord;

/// <summary>
/// 盘点记录修改输入参数.
/// </summary>
public class LogProductcheckRecordCrInput
{
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string gid { get; set; }

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