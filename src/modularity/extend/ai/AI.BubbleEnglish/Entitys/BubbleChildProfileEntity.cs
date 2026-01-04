namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 用户表
#endregion

[SugarTable("child_profile", TableDescription = "孩子信息")]
public class BubbleChildProfileEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "孩子ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "家长用户ID")]
    public long ParentId { get; set; }

    [SugarColumn(ColumnDescription = "孩子名称")]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "年龄")]
    public int Age { get; set; }

    [SugarColumn(ColumnDescription = "头像")]
    public string Avatar { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
