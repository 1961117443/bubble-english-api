namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

[SugarTable("bubble_user", TableDescription = "系统用户（家长端+后台管理）")]
public class BubbleUserEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "用户ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "微信 OpenId")]
    public string OpenId { get; set; }

    [SugarColumn(ColumnDescription = "昵称")]
    public string Nickname { get; set; }

    [SugarColumn(ColumnDescription = "头像地址")]
    public string AvatarUrl { get; set; }

    [SugarColumn(ColumnDescription = "手机号码")]
    public string Phone { get; set; }

    [SugarColumn(ColumnDescription = "是否后台管理员 0否 1是")]
    public int IsAdmin { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
