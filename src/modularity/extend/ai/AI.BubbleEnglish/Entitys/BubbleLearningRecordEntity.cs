namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 课程内容（文本/音频/视频/单词）
#endregion

[SugarTable("learning_record", TableDescription = "学习记录")]
public class BubbleLearningRecordEntity
{
    /// <summary>
    /// 家长账号ID（base_user.id）
    /// </summary>
    [SugarColumn(ColumnDescription = "家长账号ID(base_user.id)")]
    public long ParentId { get; set; }

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "孩子ID")]
    public long ChildId { get; set; }

    [SugarColumn(ColumnDescription = "课程ID")]
    public long CourseId { get; set; }

    [SugarColumn(ColumnDescription = "课时ID", IsNullable = true)]
    public long? LessonId { get; set; }

    [SugarColumn(ColumnDescription = "学习时长（秒）")]
    public int Duration { get; set; }

    [SugarColumn(ColumnDescription = "本次学习单词数量", IsNullable = true)]
    public int? WordCount { get; set; }

    [SugarColumn(ColumnDescription = "本次学习句子数量", IsNullable = true)]
    public int? SentenceCount { get; set; }

    [SugarColumn(ColumnDescription = "学习模式(guest/normal)", IsNullable = true, Length = 16)]
    public string? Mode { get; set; }

    [SugarColumn(ColumnDescription = "原始上报JSON", IsNullable = true, ColumnDataType = "longtext")]
    public string? ReportJson { get; set; }

    [SugarColumn(ColumnDescription = "学习日期")]
    public DateTime LearnTime { get; set; }
}
#endregion
