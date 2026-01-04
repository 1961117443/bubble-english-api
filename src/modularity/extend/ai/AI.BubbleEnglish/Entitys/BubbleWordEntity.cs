namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
#region 课程节（Lesson）
#endregion

[SugarTable("word", TableDescription = "单词库")]
public class BubbleWordEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "单词ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "单词文本")]
    public string Text { get; set; }

    [SugarColumn(ColumnDescription = "音标")]
    public string Phonetic { get; set; }

    [SugarColumn(ColumnDescription = "释义")]
    public string Meaning { get; set; }

    [SugarColumn(ColumnDescription = "音频路径")]
    public string AudioUrl { get; set; }

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; }
}
#endregion
