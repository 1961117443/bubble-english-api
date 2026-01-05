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

    /// <summary>
    /// 家长账号ID（复用 base_user 主键，来自 IUserManager.UserId）
    /// </summary>
    [SugarColumn(ColumnDescription = "家长账号ID(base_user.id)")]
    public long ParentId { get; set; }

    [SugarColumn(ColumnDescription = "孩子名称")]
    public string Name { get; set; }

    /// <summary>
    /// 出生年月（YYYY-MM），用于动态计算年龄段
    /// </summary>
    [SugarColumn(ColumnDescription = "出生年月(YYYY-MM)", IsNullable = true, Length = 7)]
    public string? BirthYearMonth { get; set; }

    // 兼容旧字段：若你库里已存在 Age 字段，可保留；新逻辑优先使用 BirthYearMonth。
    [SugarColumn(ColumnDescription = "年龄(兼容旧字段)")]
    public int Age { get; set; }

    [SugarColumn(ColumnDescription = "头像")]
    public string Avatar { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
