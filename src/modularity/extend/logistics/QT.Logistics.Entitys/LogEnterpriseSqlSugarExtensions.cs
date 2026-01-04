using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.OAuth.Dto;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.Logistics.Entitys;

public class LogEnterpriseSqlSugarExtensions : ISqlSugarQueryFilter, ISingleton , ISqlSugarInsertByObject
{
    private const string NOT_EXISTS = "NOTEXISTS_";

    public void Execute(ISqlSugarClient provider)
    {
        provider.QueryFilter.AddTableFilterIF<ILogEnterpriseEntity>(CheckLogEnterprise(),it => it.EId == GetCurrentEnterpriseId());
    }

    /// <summary>
    /// 判断是否要加商家过滤条件
    /// </summary>
    /// <returns></returns>
    static bool CheckLogEnterprise()
    {
        string? menuId = App.HttpContext?.Request.Headers["qt-model"];
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? "";
        string? origin = App.HttpContext?.Request.Headers["qt-origin"];
        //var isAdmin = App.User?.FindFirst(ClaimConst.CLAINMADMINISTRATOR)?.Value ?? "";
        //if (isAdmin == "1")
        //{
        //    return false;
        //}
        if (!string.IsNullOrEmpty(menuId) && !string.IsNullOrEmpty(userId))
        {
            var cache = App.GetService<ICacheManager>();
            var type = origin == "app" ? "app" : "web";
            var loginUser = cache.Get<CurrentUserOutput>(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, userId, $"auth_{type}"));
            if (loginUser!=null && loginUser.permissionList.IsAny() && loginUser.permissionList.Any(it => it.modelId == menuId && it.resource.Any(x=>x.fullName == "全部数据")))
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// 获取当前用户关联的商家id
    /// </summary>
    /// <returns></returns>
    static string GetCurrentEnterpriseId()
    {
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            throw Oops.Oh("用户未登录！");
        }
        var rep = App.GetService<ISqlSugarRepository<LogEnterpriseEntity>>();
        var logEnterpriseEntityList = rep
             .Where(it => !string.IsNullOrEmpty(it.AdminId))
             .WithCache(nameof(LogEnterpriseEntity))
             .ToList();

     

        //UserManager

        var eid = logEnterpriseEntityList.Find(x => x.AdminId == userId)?.Id;

        if (string.IsNullOrEmpty(eid))
        {
            var userRelationEntityList = rep.Context.Queryable<UserRelationEntity>().Where(x => x.ObjectType == "LogEnterprise")
              .WithCache(nameof(UserRelationEntity), 60 * 60)
              .ToList();

            var users = userRelationEntityList.Where(x => x.UserId == userId && logEnterpriseEntityList.Any(z=>z.Id == x.ObjectId)).ToList();

            if (users.Any())
            {
                if (users.Count == 1)
                {
                    // 唯一绑定
                    eid = users.First().ObjectId;
                }
                else
                {
                    var list = logEnterpriseEntityList.Where(x => users.Any(z => z.ObjectId == x.Id)).Select(x => x.Name);
                    throw Oops.Oh($"当前用户禁止绑定多个商家！{string.Join(",", list)}");
                }
            }
        }


        if (string.IsNullOrEmpty(eid))
        {
            eid = $"{NOT_EXISTS}{userId}";
        }
        return eid;
    }

    public void Execute(DataFilterModel entityInfo)
    {
        if (entityInfo.PropertyName == nameof(ILogEnterpriseEntity.EId) && entityInfo.EntityValue is ILogEnterpriseEntity entity && string.IsNullOrEmpty(entity.EId))
        {
            var eid = GetCurrentEnterpriseId();
            if (eid.StartsWith(NOT_EXISTS))
            {
                throw Oops.Oh("当前用户未绑定商家！");
            }
            entity.EId = eid;
        }
    }
}
