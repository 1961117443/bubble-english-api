using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Archives;

public class ArchivesListPageInput: PageInputBase
{
    /// <summary>
    /// 档案位置
    /// </summary>
    public string bid { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    public string establishmentDate { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string fileName { get; set; }


    /// <summary>
    /// 标签
    /// </summary>
    public string label { get; set; }
}