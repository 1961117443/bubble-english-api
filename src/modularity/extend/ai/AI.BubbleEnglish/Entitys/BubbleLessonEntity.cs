namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 课程
#endregion

[SugarTable("lesson", TableDescription = "课程节")]
public class BubbleLessonEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "课程节ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "课程ID")]
    public long CourseId { get; set; }

    [SugarColumn(ColumnDescription = "课时名称")]
    public string Title { get; set; }

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; }

    [SugarColumn(ColumnDescription = "是否免费 1免费 0付费")]
    public int IsFree { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
