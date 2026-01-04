namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 孩子档案
#endregion

[SugarTable("course", TableDescription = "课程主表")]
public class BubbleCourseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "课程ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "课程名称")]
    public string Title { get; set; }

    [SugarColumn(ColumnDescription = "课程封面")]
    public string Cover { get; set; }

    [SugarColumn(ColumnDescription = "课程介绍")]
    public string Description { get; set; }

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; }

    [SugarColumn(ColumnDescription = "是否上架 1上架 0下架")]
    public int IsPublish { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
