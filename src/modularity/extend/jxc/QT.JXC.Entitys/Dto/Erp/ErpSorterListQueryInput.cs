using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 分拣员列表查询输入
/// </summary>
public class ErpSorterListQueryInput : PageInputBase
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
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string mobile { get; set; }

}