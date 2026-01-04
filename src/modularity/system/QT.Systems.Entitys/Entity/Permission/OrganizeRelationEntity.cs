using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.Systems.Entitys.Permission;

/// <summary>
/// 用户关系映射.
/// </summary>
[SugarTable("BASE_ORGANIZE_RELATION")]
[Tenant(ClaimConst.TENANTID)]
public class OrganizeRelationEntity : EntityBase<string>
{
    /// <summary>
    /// 获取或设置 组织Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORGANIZE_ID", ColumnDescription = "组织Id")]
    public string OrganizeId { get; set; }

    /// <summary>
    /// 对象类型（角色：Role、岗位：Position）.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECT_TYPE", ColumnDescription = "对象类型（角色：Role、岗位：Position）")]
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