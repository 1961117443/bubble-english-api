using Microsoft.Extensions.Caching.Memory;
using NPOI.POIFS.FileSystem;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.SqlSugar;

public class QTErpSqlSugarExtensions : ISqlSugarQueryFilter, ISingleton,ISqlSugarInsertByObject
{
    public void Execute(ISqlSugarClient provider)
    {
        provider.QueryFilter.AddTableFilter<ICompanyEntity>(it => GetData().Contains(it.Oid));

        // 全局添加公司过滤
        provider.QueryFilter.AddTableFilterIF<ICompanyEntity>(CompanyFilterCheck(provider), x => x.Oid == GetCompanyId());
    }

    /// <summary>
    /// 获取当前用户所在的公司
    /// </summary>
    /// <returns></returns>
    static string[] GetData()
    {
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            throw Oops.Oh("用户未登录！");
        }
        var cache = App.GetService<ICacheManager>();
        if (!cache.Exists($"u:{userId}:organize"))
        {
            var list = App.GetService<IUsersService>()?.GetRelationOrganizeList(userId).GetAwaiter().GetResult();
            var result = list?.Select(x => x.Id).ToArray() ?? new string[0];

            cache.Set($"u:{userId}:organize", result, TimeSpan.FromMinutes(10));

            //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            //return list?.Select(x => x.Id).ToArray() ?? new string[0];
        }
        return cache.Get<string[]>($"u:{userId}:organize");

        //return cache.GetOrCreate($"u:{userId}:organize", entry =>
        // {
        //     var list = new List<OrganizeEntity>(); // App.GetService<IUsersService>()?.GetRelationOrganizeList(userId).ConfigureAwait(true).GetAwaiter().GetResult();

        //     entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

        //     return list?.Select(x => x.Id).ToArray() ?? new string[0];
        // });
    }

    /// <summary>
    /// 判断是否需要添加公司过滤
    /// </summary>
    /// <returns></returns>
    private bool CompanyFilterCheck(ISqlSugarClient db)
    {
        if (App.User == null) return false;
        if (App.HttpContext == null) return false;
        

        //var userManager = App.GetService<IUserManager>();
        //var info = userManager.GetUserInfo().GetAwaiter().GetResult();
        //Console.WriteLine("CompanyFilterCheck...................1");
        // 集团账号不过滤
        if (!App.HttpContext.Items.TryGetValue(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, out var companyAccount))
        {
            if (KeyVariable.MultiTenancy)
            {
                var tenantId = App.User.FindFirst(ClaimConst.TENANTID)?.Value ?? "";
                if (string.IsNullOrEmpty(tenantId) || !TenantManager.Login(tenantId, db))
                {
                    return true;
                }
                db = db.AsTenant().GetConnection(tenantId);
            }
            var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? "";
            //var org = db.Queryable<OrganizeEntity>()
            //    .Where(x => SqlFunc.Subqueryable<UserEntity>().Where(d => d.OrganizeId == x.Id && d.Id == userId).Any())
            //    .Select(x=>new OrganizeEntity
            //    {
            //        Id = x.Id,
            //        EnCode = x.EnCode
            //    })
            //    .First();
            var org = App.GetService<ICacheManager>()?.GetOrCreate($"{CommonConst.CACHEKEYUSER}{userId}:OrganizeEntity", (entry) =>
            {
                return db.Queryable<OrganizeEntity>().Where(x => SqlFunc.Subqueryable<UserEntity>()
                .Where(d => d.OrganizeId == x.Id && d.Id == userId).Any())
                    .Select(x => new OrganizeEntity
                    {
                        Id = x.Id,
                        EnCode = x.EnCode
                    }).First();
            });

            //var  db.Queryable<UserEntity>().Where(x => x.Id == userId).Select(x => x.OrganizeId).First();
            //var service = App.GetService<ISqlSugarRepository<UserEntity>>();
            ////Console.WriteLine("CompanyFilterCheck...................2");
            //string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value, "company");
            //var org = App.GetService<ICacheManager>().Get<OrganizeEntity>(cacheKey);
            if (org != null)
            {
                companyAccount = org.EnCode != "JT";
                App.HttpContext.Items.TryAdd(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, companyAccount);
                App.HttpContext.Items.TryAdd(ClaimConst.CLAINMCOMPANYID, org.Id);
            }
        };
        return !string.IsNullOrEmpty(companyAccount?.ToString()) && bool.TryParse(companyAccount?.ToString(), out bool result) && result;
        //return App.User.FindFirst(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT)?.Value != "1";
    }

    /// <summary>
    /// 获取公司id
    /// </summary>
    /// <returns></returns>
    private string? GetCompanyId()
    {
        if (!App.HttpContext.Items.TryGetValue(ClaimConst.CLAINMCOMPANYID, out var companyId))
        {
            var repository = App.GetService<ISqlSugarRepository<OrganizeEntity>>();
            var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? "";
            var org = repository.Where(x => SqlFunc.Subqueryable<UserEntity>().Where(d => d.OrganizeId == x.Id && d.Id == userId).Any()).First();
            //string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value, "company");
            //var org = App.GetService<ICacheManager>().Get<OrganizeEntity>(cacheKey);
            if (org != null)
            {
                companyId = org.Id;
                App.HttpContext.Items.TryAdd(ClaimConst.CLAINMCOMPANYID, org.Id);
            }
        }
        return companyId?.ToString() ?? string.Empty;
    }

    public void Execute(DataFilterModel entityInfo)
    {
        if (entityInfo.PropertyName == nameof(ICompanyEntity.Oid) && entityInfo.EntityValue is ICompanyEntity companyEntity && string.IsNullOrEmpty(companyEntity.Oid))
        {
            companyEntity.Oid = GetCompanyId();

            if (string.IsNullOrEmpty(companyEntity.Oid))
            {
                throw Oops.Oh("缺少组织，不能新增！");
            }
            //entityInfo.SetValue(GetCompanyId());
        }
    }
}


public class UserEntityChangedSqlSugarExtensions : ISqlSugarInsertByObject, ISqlSugarUpdateByObject, ISqlSugarDeleteByObject, ISingleton
{
    public void Execute(DataFilterModel entityInfo)
    {
        // 删除缓存
        if (entityInfo.PropertyName == nameof(UserEntity.OrganizeId) && entityInfo.EntityValue is UserEntity entity)
        {
            App.GetService<ICacheManager>()?.Del($"{CommonConst.CACHEKEYUSER}{entity.Id}:OrganizeEntity");
        }
    }
}