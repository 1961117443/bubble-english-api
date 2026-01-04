using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace QT.Systems.Entitys.Entity.System;
/// <summary>
/// 模块关系映射.
/// </summary>
[SugarTable("BASE_MODULE_RELATION")]
[Tenant(ClaimConst.TENANTID)]
public class ModuleRelationEntity :EntityBase<string>
{
    /// <summary>
    /// 获取或设置 模块Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULE_ID", ColumnDescription = "模块Id")]
    public string ModuleId { get; set; }

    /// <summary>
    /// 对象类型（打印报表：Print）.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECT_TYPE", ColumnDescription = "对象类型（打印报表：Print）")]
    public string ObjectType { get; set; }

    /// <summary>
    /// 获取或设置 对象主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECT_ID", ColumnDescription = "对象主键")]
    public string ObjectId { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORT_CODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 获取或设置 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATOR_TIME", ColumnDescription = "创建时间")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 获取或设置 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATOR_USER_ID", ColumnDescription = "创建用户")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Creator()
    {
        var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        CreatorTime = DateTime.Now;
        Id = YitIdHelper.NextId().ToString();
        if (!string.IsNullOrEmpty(userId))
        {
            CreatorUserId = userId;
        }
    }
}
