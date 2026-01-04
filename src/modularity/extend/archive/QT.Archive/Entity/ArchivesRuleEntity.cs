using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Archive.Entity;

/// <summary>
/// 档案馆信息
/// </summary>
[SugarTable("ext_archives_rule")]
public class ArchivesRuleEntity : CLDEntityBase
{
    /// <summary>
    /// 编号
    /// </summary>
    [SugarColumn(ColumnName = "F_Code")]
    public string Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 属性配置
    /// </summary>
    [SugarColumn(ColumnName = "F_PropertyJson")]
    public string PropertyJson { get; set; }
}