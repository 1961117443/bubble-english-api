namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 课程内容（文本/音频/视频/单词）
#endregion

[SugarTable("learning_record", TableDescription = "学习记录")]
public class BubbleLearningRecordEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "孩子ID")]
    public long ChildId { get; set; }

    [SugarColumn(ColumnDescription = "课程ID")]
    public long CourseId { get; set; }

    [SugarColumn(ColumnDescription = "课时ID")]
    public long LessonId { get; set; }

    [SugarColumn(ColumnDescription = "学习时长（秒）")]
    public int Duration { get; set; }

    [SugarColumn(ColumnDescription = "学习日期")]
    public DateTime LearnTime { get; set; }
}
#endregion
