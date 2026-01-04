using SqlSugar;

namespace AI.BubbleEnglish.Entitys;

  
[SugarTable("share_record", TableDescription = "家长分销分享记录")]
public class BubbleShareRecordEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "家长ID")]
    public long UserId { get; set; }

    [SugarColumn(ColumnDescription = "分享内容类型 course/lesson/page")]
    public string ShareType { get; set; }

    [SugarColumn(ColumnDescription = "目标ID")]
    public long TargetId { get; set; }

    [SugarColumn(ColumnDescription = "点击次数")]
    public int ClickCount { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
} 