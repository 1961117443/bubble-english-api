using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Entitys;

/// <summary>
/// 微信小程序码场景
/// </summary>
[SugarTable("wx_scene")]
[Tenant(ClaimConst.TENANTID)]

public class WechatSceneEntity
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }
}
