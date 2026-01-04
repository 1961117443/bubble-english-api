using Mapster;
using NPOI.SS.Formula.Functions;
using QT.Common.Contracts;
using QT.Common.Extension;
using System.Linq.Expressions;
using Yitter.IdGenerator;

namespace SqlSugar;

/// <summary>
/// SqlSugar扩展类
/// </summary>
public static class SqlSugarExtensions
{
    /// <summary>
    /// 对比数据库数据进行增删改，删除是逻辑删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name = "V" ></typeparam>
    /// <param name="db"></param>
    /// <param name="condition"></param>
    /// <param name="detailList"></param>
    /// <param name="onAdd">新增数据事件</param>
    /// <param name="onUpdate">更新数据事件</param>
    /// <param name="onDelete">删除数据事件</param>
    /// <returns></returns>
    public static async Task CUDSaveAsnyc<T, V>(this SqlSugarClient db, Expression<Func<T, bool>> condition, IEnumerable<V> detailList, Action<T>? onAdd = null, Action<T>? onUpdate = null, Action<T>? onDelete = null,bool enableDiffLog=false) where T : CUDEntityBase, new()
    {
        // 
        var dbEntityList = await db.Queryable<T>().Where(condition).ToListAsync();

        List<T> insertList = new List<T>();
        List<T> updateList = new List<T>();
        List<T> nochangeList = new List<T>(); // 没有变化的实体

        if (detailList.IsAny())
        {
            foreach (var item in detailList)
            {
                if (item == null)
                {
                    continue;
                }
                var entity = item.Adapt<T>();
                var dbEntity = dbEntityList.Find(it => it.Id == entity.Id);
                // 更新数据
                if (dbEntity != null)
                {
                    db.Context.Tracking(dbEntity);

                    item.Adapt(dbEntity, item.GetType(), typeof(T));

                    var changes = db.GetChanges(dbEntity);
                    if (changes != null && changes.Count == 0)
                    {
                        nochangeList.Add(dbEntity);
                    }
                    else
                    {
                        onUpdate?.Invoke(dbEntity);
                        updateList.Add(dbEntity);
                    }
                }
                else
                {
                    entity.Id = YitIdHelper.NextId().ToString();
                    onAdd?.Invoke(entity);
                    insertList.Add(entity);
                }
            }
        }

        if (nochangeList.IsAny())
        {
            dbEntityList = dbEntityList.Except(nochangeList).ToList();
        }
        bool isNoTran = db.Ado.IsNoTran();
        using (var uow = db.CreateContext(isNoTran))
        {
            if (updateList.IsAny())
            {
                dbEntityList = dbEntityList.Except(updateList).ToList();
                await db.Context.Updateable<T>(updateList).EnableDiffLogEventIF(enableDiffLog).ExecuteCommandAsync();
            }
            if (insertList.IsAny())
            {
                dbEntityList = dbEntityList.Except(insertList).ToList();
                await db.Context.Insertable<T>(insertList).EnableDiffLogEventIF(enableDiffLog).ExecuteCommandAsync();
            }
            if (dbEntityList.IsAny())
            {
                foreach (var item in dbEntityList)
                {
                    if (onDelete != null)
                    {
                        onDelete(item);
                    }
                    item.Delete();
                    await db.Context.Updateable<T>(item)
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
                    .EnableDiffLogEventIF(enableDiffLog).ExecuteCommandAsync();
                }
                //var updater = db.Context.Updateable<T>(dbEntityList)                    
                //    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId });
                //await updater.EnableDiffLogEvent().ExecuteCommandAsync();
            }

            if (isNoTran)
            {
                uow.Commit();
            }
        }
    }

    /// <summary>
    /// 对比数据库数据进行增删改，删除是逻辑删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name = "V" ></typeparam>
    /// <param name="db"></param>
    /// <param name="condition"></param>
    /// <param name="detailList"></param>
    /// <param name="onAdd">新增数据事件</param>
    /// <param name="onUpdate">更新数据事件</param>
    /// <param name="onDelete">删除数据事件</param>
    /// <returns></returns>
    public static async Task CUDSaveAsnyc<T>(this SqlSugarClient db, Expression<Func<T, bool>> condition, IEnumerable<object> detailList, Action<T>? onAdd = null, Action<T>? onUpdate = null, Action<T>? onDelete = null) where T : CUDEntityBase, new()
    {
        await CUDSaveAsnyc<T, object>(db, condition, detailList, onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// 对比数据库数据进行增删改，删除是硬删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name = "V" ></typeparam>
    /// <param name="db"></param>
    /// <param name="condition"></param>
    /// <param name="detailList"></param>
    /// <param name="onAdd">新增数据事件</param>
    /// <param name="onUpdate">更新数据事件</param>
    /// <param name="onDelete">删除数据事件</param>
    /// <returns></returns>
    public static async Task CUDSaveHardAsnyc<T, V>(this SqlSugarClient db, Expression<Func<T, bool>> condition, List<V> detailList, Action<T>? onAdd = null, Action<T>? onUpdate = null, Action<T>? onDelete = null) where T : EntityBase<string>, new()
    {
        // 清空订单物品明细原有数据
        var dbEntityList = await db.Queryable<T>().Where(condition).ToListAsync();

        List<T> insertList = new List<T>();
        List<T> updateList = new List<T>();

        foreach (var item in detailList)
        {
            if (item == null)
            {
                continue;
            }
            var entity = item.Adapt<T>();
            var dbEntity = dbEntityList.Find(it => it.Id == entity.Id);
            // 更新数据
            if (dbEntity != null)
            {
                db.Context.Tracking(dbEntity);
                item.Adapt(dbEntity);
                onUpdate?.Invoke(dbEntity);
                updateList.Add(dbEntity);
            }
            else
            {
                entity.Id = YitIdHelper.NextId().ToString();
                onAdd?.Invoke(entity);
                insertList.Add(entity);
            }
        }
        bool isNoTran = db.Ado.IsNoTran();
        using (var uow = db.CreateContext(isNoTran))
        {
            if (updateList.IsAny())
            {
                dbEntityList = dbEntityList.Except(updateList).ToList();
                await db.Context.Updateable<T>(updateList).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (insertList.IsAny())
            {
                dbEntityList = dbEntityList.Except(insertList).ToList();
                await db.Context.Insertable<T>(insertList).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (dbEntityList.IsAny())
            {
                if (onDelete != null)
                {
                    dbEntityList.ForEach(it => onDelete(it));
                }
                await db.Context.Deleteable<T>(dbEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
            }

            if (isNoTran)
            {
                uow.Commit();
            }
        }

    }


    /// <summary>
    /// take  默认1000
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> AutoTake<T>(this ISugarQueryable<T> query,int top)
    {
        if (top == 0)
        {
            top = 1000;
        }
        return query.Take(top);
    }

    /// <summary>
    /// 只能保存，实体有变动才更新到数据库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="updater"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> AutoUpdateAsync<T>(this ISqlSugarRepository<T> rep, T entity) where T : class, new()
    {
        var changes = rep.Context.GetChanges(entity);
        if (changes == null || changes.Count>0)
        {
            return rep.UpdateAsync(entity);
        }
        return Task.FromResult(0);
    }

    /// <summary>
    /// 只能保存，实体有变动才更新到数据库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="updater"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> AutoUpdateAsync<T>(this SqlSugarClient context, T entity) where T : class, new()
    {
        var changes = context.GetChanges(entity);
        if (changes == null || changes.Count > 0)
        {
            return context.Updateable<T>(entity).ExecuteCommandAsync();
        }
        return Task.FromResult(0);
    }


    /// <summary>
    /// 返回实体（如果有自增会返回到实体里面，不支批量自增，不支持默认值）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rep"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<T> InsertReturnEntityAsync<T>(this ISqlSugarRepository<T> rep, T entity) where T : class, new()
    {
         return rep.Context.Insertable(entity).ExecuteReturnEntityAsync();
    }
}
