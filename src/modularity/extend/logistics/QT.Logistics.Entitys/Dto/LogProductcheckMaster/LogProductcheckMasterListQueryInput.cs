using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogProductcheckMaster;

/// <summary>
/// 盘点记录主表列表查询输入
/// </summary>
public class LogProductcheckMasterListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    public string checkTime { get; set; }

    /// <summary>
    /// 盘点单号.
    /// </summary>
    public string no { get; set; }

}