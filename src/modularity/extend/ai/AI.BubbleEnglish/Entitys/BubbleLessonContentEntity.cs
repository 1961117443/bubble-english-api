namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
#region 单词库
#endregion

[SugarTable("lesson_content", TableDescription = "课程内容列表")]
public class BubbleLessonContentEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "内容ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "课程节ID")]
    public long LessonId { get; set; }

    [SugarColumn(ColumnDescription = "内容类型 text/audio/video/word")]
    public string ContentType { get; set; }

    [SugarColumn(ColumnDescription = "内容文本或媒体地址")]
    public string ContentValue { get; set; }

    [SugarColumn(ColumnDescription = "扩展字段（JSON）")]
    public string Extra { get; set; }

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; }
}
