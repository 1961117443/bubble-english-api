using QT.Common.Contracts;
using SqlSugar;

namespace QT.Archive.Entity;

/// <summary>
/// 档案信息
/// </summary>
[SugarTable("ext_archives")]
public class ArchivesEntity : CLDEntityBase
{
    /// <summary>
    /// 编号
    /// </summary>
    [SugarColumn(ColumnName = "F_Code")]
    public string Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 档案位置
    /// </summary>
    [SugarColumn(ColumnName = "F_Bid")]
    public string Bid { get; set; }


    /// <summary>
    /// 建立日期
    /// </summary>
    [SugarColumn(ColumnName = "F_EstablishmentDate")]
    public DateTime? EstablishmentDate { get; set; }


    /// <summary>
    /// 销毁日期
    /// </summary>
    [SugarColumn(ColumnName = "F_DestructionDate")]
    public DateTime? DestructionDate { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [SugarColumn(ColumnName = "F_Label")]
    public string Label { get; set; }
}
