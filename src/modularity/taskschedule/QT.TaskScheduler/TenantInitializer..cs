using Microsoft.Extensions.DependencyInjection;
using QT.Common.Cache;
using QT.Common.Contracts;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.TaskScheduler.Entitys.Entity;
using QT.TaskScheduler.Interfaces.TaskScheduler;
using SqlSugar;

namespace QT.TaskScheduler;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">租户标识</typeparam>
public class TenantInitializer<T> : ITenantInitializer<T>,ISingleton
{
    static TenantInitializer()
    {
        //NPOI.SS.Formula.Functions.T

        //Scoped.Create((factory, scope) =>
        //{
        //    var tenantId = TenantCacheFactory.GetTenantId<T>();
        //    //Console.WriteLine("租户初始化：{0}", tenantId);
        //    var tenant = scope.ServiceProvider.GetService<ISqlSugarTenant>();
        //    var taskService = scope.ServiceProvider.GetService<ITimeTaskService>();
        //    if (tenant!=null && tenant.IsLoggedIn)
        //    {
        //        using (var db = scope.ServiceProvider.GetService<ISqlSugarClient>())
        //        {
        //            db.AsTenant().ChangeDatabase(tenantId);
        //            var list = db.Queryable<TimeTaskEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToList();
        //            if (list.IsAny())
        //            {
        //                foreach (TimeTaskEntity item in list)
        //                {
        //                    taskService.AddTimerJob(item);
        //                }
        //            }
        //        }
        //    }
           
        //});
    }
}
