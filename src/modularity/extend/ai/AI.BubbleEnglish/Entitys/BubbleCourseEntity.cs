namespace AI.BubbleEnglish.Entitys;

using QT.Common.Contracts;
using SqlSugar;
using System;
#region 课程主表
#endregion

[SugarTable("bubble_course", TableDescription = "课程主表")]
public class BubbleCourseEntity : EntityBase<string>
{
    [SugarColumn(IsPrimaryKey = true, ColumnDescription = "课程ID")]
    public override string Id { get; set; }

    [SugarColumn(ColumnDescription = "课程唯一key")]
    public string CourseKey { get; set; }

    [SugarColumn(ColumnDescription = "课程名称")]
    public string Title { get; set; }

    [SugarColumn(ColumnDescription = "课程封面")]
    public string Cover { get; set; }

    [SugarColumn(ColumnDescription = "课程介绍")]
    public string Description { get; set; }

    [SugarColumn(ColumnDescription = "完整 course v2 JSON")]
    public string CourseJson { get; set; }
    

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; }

    [SugarColumn(ColumnDescription = "是否上架 1上架 0下架")]
    public int IsPublish { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
