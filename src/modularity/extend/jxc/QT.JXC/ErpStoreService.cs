using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Interfaces;

namespace QT.JXC;

/// <summary>
/// 业务实现：更新库存.
/// </summary>
public class ErpStoreService : ITransient, IErpStoreService
{
    private readonly ISqlSugarRepository<ErpOutdetailRecordEntity> _repository;

    public ErpStoreService(ISqlSugarRepository<ErpOutdetailRecordEntity> repository)
    {
        _repository = repository;
    }

    ///// <summary>
    ///// 增加库存
    ///// 一般是出库单删除的时候会调用
    ///// </summary>
    ///// <returns></returns>
    //public Task Increase(ErpOutdetailRecordUpInput input)
    //{
    //    if (input is null)
    //    {
    //        throw new ArgumentNullException(nameof(input));
    //    }

    //    //1、找出对应的入库记录
    //    //2、更新库存
    //    //3、插入记录表，需要先删除原来的出库关系



    //    return Task.CompletedTask;
    //}

    /// <summary>
    /// 恢复库存
    /// 一般是出库单删除的时候会调用
    /// </summary>
    /// <param name="id">出库明细id</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task Restore(string id)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        //出库明细记录
        var records = await _repository.Where(x => x.OutId == id).ToListAsync();

        //1、找出对应的入库记录
        var inids = records.Select(x => x.InId).ToArray();
        var inList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .ClearFilter<ICompanyEntity>()
            .Where(x => inids.Contains(x.Id))
            .Select(x => new ErpInrecordEntity
            {
                Id = x.Id,
                Num = x.Num,
                Gid = x.Gid,
                Price = x.Price,
                Oid = x.Oid
            })
            .ToListAsync();
     
        if (!inList.Any())
        {
            //throw Oops.Oh("入库记录不存在！");
            return;
        }
        if (inList.GroupBy(x => x.Oid).Count() > 1)
        {
            throw Oops.Oh("数据异常，存在多个公司的数据！");
        }


        //2、更新库存
        foreach (var record in records)
        {
            var entity = inList.Find(a => a.Id == record.InId);
            if (entity == null)
            {
                //throw Oops.Oh("入库记录不存在！");
                continue;
            }
            // 恢复库存
            //_repository.Context.Tracking(entity);
            entity.Num += record.Num;
        }
        //3、更新入库记录， 删除出库明细记录
        //using (var uow = _repository.Context.CreateContext(_repository.Context.Ado.IsNoTran()))
        //{
        //    await _repository.Context.Updateable<ErpInrecordEntity>(inList).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        //    await _repository.DeleteAsync(records.Select(x => x.Id).ToArray());
        //}


        StringBuilder sqlBuilder = new StringBuilder();

        if (inList.IsAny())
        {
            var updateSql = _repository.Context.Updateable<ErpInrecordEntity>(inList).UpdateColumns(x => new { x.Num }).ToSqlString();
            sqlBuilder.AppendLine(updateSql);
            //await _repository.Context.Updateable<ErpInrecordEntity>(inList).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        }
        var deleteArr = records.Select(x => x.Id).ToArray();
        if (deleteArr.IsAny())
        {
            var deleteSql = _repository.Context.Deleteable<ErpOutdetailRecordEntity>(deleteArr).ToSqlString();
            sqlBuilder.AppendLine($"{deleteSql};");
            //await _repository.Context.Deleteable<ErpOutdetailRecordEntity>(deleteArr).ExecuteCommandAsync();
        }

        if (sqlBuilder.Length>0)
        {
            await _repository.Ado.ExecuteCommandAsync(sqlBuilder.ToString());
        }

        if (inids.IsAny())
        {
            var updateInrecordIds = inids;

            // 检查库存数是否一直
            var q1 = _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => updateInrecordIds.Contains(x.InId))
                .GroupBy(x => x.InId)
                .Select(x => new
                {
                    Id = x.InId,
                    Num = SqlFunc.AggregateSum(x.Num)
                });

            // 存在不一致，则更新异常
            var isError = await _repository.Context.Queryable<ErpInrecordEntity>()
                .LeftJoin(q1, (a, b) => a.Id == b.Id)
                .Where((a, b) => updateInrecordIds.Contains(a.Id))
                .Where((a, b) => a.InNum - SqlFunc.IsNull(b.Num, 0.0m) != a.Num)
                .AnyAsync();


            //if (await _repository.Context.Queryable<ErpInrecordEntity>().Where(x=>x.Num < 0 && updateInrecordIds.Contains(x.Id)).AnyAsync())
            if (isError)
            {
                throw Oops.Oh("库存不足！");
            }
        }

        //// 2024.12.12 sxn

        //bool nestTran = false;
        //try
        //{
        //    if (_repository.Context.Ado.IsNoTran())
        //    {
        //        _repository.Context.Ado.BeginTran();
        //        nestTran = true;
        //    }
        //    foreach (var item in inList)
        //    {
        //        await _repository.Context.Updateable<ErpInrecordEntity>(item).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        //    }

        //    await _repository.DeleteAsync(records.Select(x => x.Id).ToArray());
        //    if (nestTran)
        //    {
        //        _repository.Context.Ado.CommitTran();
        //    }
        //}
        //catch (Exception e)
        //{
        //    if (nestTran)
        //    {
        //        _repository.Context.Ado.RollbackTran();
        //    }
        //    throw Oops.Oh($"库存更新失败:{e.Message}");
        //}
    }

    /// <summary>
    /// 减少库存
    /// 一般是出库单新增的时候会调用
    /// 返回本次出库成本
    /// </summary>
    /// <returns></returns>
    public async Task<ErpOutdetailRecordUpOutput> Reduce(ErpOutdetailRecordUpInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        if (input.num <= 0)
        {
            throw Oops.Oh("请输入出库数量！");
        }
        List<ErpOutdetailRecordEntity> erpOutdetailRecords = new List<ErpOutdetailRecordEntity>();
        //1、找出对应的入库记录
        if (input.records == null || !input.records.Any())
        {
            throw Oops.Oh("库存不足！");
        }
        var records = input.records.Select(x => x.id).ToArray();
        var inList = await _repository.Context.Queryable<ErpInrecordEntity>()
           // .ClearFilter<ICompanyEntity>()  // 这里为什么要去掉公司过滤？
            .Where(x => records.Contains(x.Id))
            .Select(x=> new ErpInrecordEntity
            {
                Id = x.Id,
                Num = x.Num,
                Gid = x.Gid,
                Price = x.Price,
                Oid = x.Oid
            })
            .ToListAsync();
        if (!inList.Any())
        {
            throw Oops.Oh("库存不足！");
        }

        if (inList.GroupBy(x=>x.Oid).Count()>1)
        {
            throw Oops.Oh("数据异常，存在多个公司的数据！");
        }

        List<ErpInrecordEntity> updateInrecords = new List<ErpInrecordEntity>();

        // 本次成本
        decimal costAmount = 0;
        //2、更新库存
        foreach (var record in input.records)
        {
            var entity = inList.Find(a => a.Id == record.id);
            if (entity == null || entity.Num <= 0)
            {
                throw Oops.Oh("库存不足！");
            }
            // 扣减库存
            //1、剩余库存数大于等于出库数量 剩余库存数=剩余库存数-出库数量，出库数量=0
            //2、剩余库存数小于出库数量 出库数量= 出库数量-剩余库存数 ，剩余库存数=0
            //本次扣减的数量

            decimal num = entity.Num >= input.num ? input.num : entity.Num;
            entity.Num -= num;
            input.num -= num;


            // 扣减后少于0，提示库存不足
            if (entity.Num < 0)
            {
                throw Oops.Oh("库存不足！");
            }


            updateInrecords.Add(entity);

            erpOutdetailRecords.Add(new ErpOutdetailRecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Gid = entity.Gid,
                InId = entity.Id,
                Num = num,
                OutId = input.id
            });

            // 计算成本
            costAmount += num * entity.Price;


            //标识已扣减完库存
            if (input.num == 0)
            {
                break;
            }
        }

        // 所有入库记录都扣减完，数量不足
        if (input.num > 0)
        {
            throw Oops.Oh("库存不足！");
        }


        //3、更新库存数，插入记录表，需要先删除原来的出库关系
        //using (var uow = _repository.Context.CreateContext(_repository.Context.Ado.IsNoTran()))
        //{
        //    await _repository.Context.Updateable(updateInrecords).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        //    await _repository.DeleteAsync(x => x.OutId == input.id);
        //    await _repository.InsertAsync(erpOutdetailRecords);
        //    uow.Commit();
        //}

        StringBuilder sqlBuilder = new StringBuilder();

        if (updateInrecords.IsAny())
        {
            var updateSql = _repository.Context.Updateable(updateInrecords).UpdateColumns(x => new { x.Num }).ToSqlString();
            sqlBuilder.AppendLine(updateSql);
            //await _repository.Context.Updateable<ErpInrecordEntity>(updateInrecords).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        }

        var deleteSql = _repository.Context.Deleteable<ErpOutdetailRecordEntity>(x => x.OutId == input.id).ToSqlString();
        sqlBuilder.AppendLine($"{deleteSql};");

        //await _repository.Context.Deleteable<ErpOutdetailRecordEntity>(x => x.OutId == input.id).ExecuteCommandAsync();

        if (erpOutdetailRecords.IsAny())
        {
            var insertSql = _repository.Context.Insertable(erpOutdetailRecords).ToSqlString();
            sqlBuilder.AppendLine($"{insertSql};");
            //await _repository.Context.Insertable<ErpOutdetailRecordEntity>(erpOutdetailRecords).ExecuteCommandAsync();
        }

        if (sqlBuilder.Length>0)
        {
            await _repository.Ado.ExecuteCommandAsync(sqlBuilder.ToString());
        }

        if (updateInrecords.IsAny())
        {
            var updateInrecordIds = updateInrecords.Select(x => x.Id).ToList();

            // 检查库存数是否一直
            var q1 = _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => updateInrecordIds.Contains(x.InId))
                .GroupBy(x => x.InId)
                .Select(x => new
                {
                    Id = x.InId,
                    Num = SqlFunc.AggregateSum(x.Num)
                });

            // 存在不一致，则更新异常
            var isError = await  _repository.Context.Queryable<ErpInrecordEntity>()
                .LeftJoin(q1, (a, b) => a.Id == b.Id)
                .Where((a, b) => updateInrecordIds.Contains(a.Id))
                .Where((a, b) => a.InNum - SqlFunc.IsNull(b.Num, 0.0m) != a.Num)
                .AnyAsync();


            //if (await _repository.Context.Queryable<ErpInrecordEntity>().Where(x=>x.Num < 0 && updateInrecordIds.Contains(x.Id)).AnyAsync())
            if(isError)
            {
                throw Oops.Oh("库存不足！");
            }
        }

        ////// 2024.12.12 sxn
        //bool nestTran = false;
        //try
        //{
        //    if (_repository.Context.Ado.IsNoTran())
        //    {
        //        _repository.Context.Ado.BeginTran();
        //        nestTran = true;
        //    }
        //    foreach (var item in updateInrecords)
        //    {
        //        await _repository.Context.Updateable<ErpInrecordEntity>(item).UpdateColumns(x => new { x.Num }).ExecuteCommandAsync();
        //    }
            
        //    await _repository.DeleteAsync(x => x.OutId == input.id);
        //    await _repository.InsertAsync(erpOutdetailRecords);

        //    if (nestTran)
        //    {
        //        _repository.Context.Ado.CommitTran();
        //    }
        //}
        //catch (Exception ex)
        //{
        //    if (nestTran)
        //    {
        //        _repository.Context.Ado.RollbackTran();
        //    }
        //    throw Oops.Oh($"库存更新失败:{ex.Message}");
        //}

        return new ErpOutdetailRecordUpOutput { CostAmount = costAmount };
    }

}
