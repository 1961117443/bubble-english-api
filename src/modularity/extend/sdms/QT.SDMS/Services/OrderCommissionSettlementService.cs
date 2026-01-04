using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.SDMS.Entitys.Dto.OrderCommission;
using QT.SDMS.Entitys.Dto.OrderCommissionSettlement;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：充值记录.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "佣金结算", Name = "OrderCommissionSettlement", Order = 300)]
[Route("api/sdms/OrderCommissionSettlement")]
public class OrderCommissionSettlementService : QTBaseService<OrderCommissionSettlementEntity, OrderCommissionSettlementCrInput, OrderCommissionSettlementUpInput, OrderCommissionSettlementOutput, OrderCommissionSettlementListQueryInput, OrderCommissionSettlementListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public OrderCommissionSettlementService(ISqlSugarRepository<OrderCommissionSettlementEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(OrderCommissionSettlementCrInput input, OrderCommissionSettlementEntity entity)
    {
        entity.SettlementNo = await _billRullService.GetBillNumber("SdmsOrderCommissionSettlement");

        // 取出全部待结算佣金
        var ids = input.commissionList.Select(x=>x.id).ToList();
        var records = await _repository.Context.Queryable<OrderCommissionEntity>().Where(x=> ids.Contains(x.Id)).ToListAsync();

        if (records.Count == 0)
            throw Oops.Oh("未找到任何有效的待结算佣金记录");

        // 将佣金记录关联到结算单
        foreach (var rec in records)
        {
            if (rec.Status != SdmsCommissionStatus.Pending)
            {
                throw Oops.Oh( $"佣金记录[{rec.No}]状态非待结算，无法进行结算操作");
            }
            
            _repository.Context.Tracking(rec);
            rec.SettleId = entity.Id;
            rec.Status = SdmsCommissionStatus.Settled;
        }

        await base.BeforeCreate(input, entity);

        await _repository.Context.Updateable<OrderCommissionEntity>(records).ExecuteCommandAsync();
    }
 
    protected override async Task BeforeUpdate(OrderCommissionSettlementUpInput input, OrderCommissionSettlementEntity entity)
    {
        var records = await _repository.Context.Queryable<OrderCommissionEntity>().Where(x => x.SettleId == entity.Id).ToListAsync();
       
        // 1. 判断是否有变化：比较旧的 RecordIds 和新的 RecordIds
        var oldRecordIds = records?.Select(x => x.Id).ToList() ?? new List<string>();
        var newRecordIds = input.commissionList?.Select(x=>x.id).ToList() ?? new List<string>();

        if (!oldRecordIds.SequenceEqual(newRecordIds))
        {
            if (records.IsAny())
            {
                // 1. 先恢复旧记录状态
                foreach (var r in records)
                {
                    _repository.Context.Tracking(r);
                    r.Status = SdmsCommissionStatus.Pending;
                    r.SettleId = "";
                }
                await _repository.Context.Updateable<OrderCommissionEntity>(records).ExecuteCommandAsync();
            }


            // 2. 重新绑定新记录
            var ids = input.commissionList.Select(x => x.id).ToList();
            var newRecords = await _repository.Context.Queryable<OrderCommissionEntity>().Where(x => ids.Contains(x.Id)).ToListAsync();
            if (newRecords.Count == 0)
                throw Oops.Oh("未找到任何有效的待结算佣金记录");
            foreach (var rec in newRecords)
            {
                if (rec.Status != SdmsCommissionStatus.Pending)
                {
                    throw Oops.Oh($"佣金记录[{rec.No}]状态非待结算，无法进行结算操作");
                }

                _repository.Context.Tracking(rec);
                rec.SettleId = entity.Id;
                rec.Status = SdmsCommissionStatus.Settled;
            }

            await _repository.Context.Updateable<OrderCommissionEntity>(newRecords).ExecuteCommandAsync();
        }
        
         
    }

    protected override async Task AfterDelete(OrderCommissionSettlementEntity entity)
    {
        var records = await _repository.Context.Queryable<OrderCommissionEntity>().Where(x => x.SettleId == entity.Id).ToListAsync();

        if (records.IsAny())
        {
            // 1. 先恢复旧记录状态
            foreach (var r in records)
            {
                _repository.Context.Tracking(r);
                r.Status = SdmsCommissionStatus.Pending;
                r.SettleId = "";
            }
            await _repository.Context.Updateable<OrderCommissionEntity>(records).ExecuteCommandAsync();
        }
    }

    public override async Task<OrderCommissionSettlementOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        data.commissionList = await _repository.Context.Queryable<OrderEntity>()
             .InnerJoin<OrderCommissionEntity>((a, b) => a.Id == b.FId)

             .Where((a, b) => b.SettleId == id)
             .Select((a, b) => new OrderCommissionListOutput
             {
                 id = b.Id,
                 amount = b.Amount,
                 orderAmount = a.Amount,
                 no = b.No,
                 orderDate = a.OrderDate,
                 orderNo = a.No,
                 userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == b.UserId).Select(ddd => ddd.RealName),
                 proportion = b.Proportion
             })
             .ToListAsync();

        return data;
    }

    protected override async Task<SqlSugarPagedList<OrderCommissionSettlementListOutput>> GetPageList([FromQuery] OrderCommissionSettlementListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<OrderCommissionSettlementEntity>()
            .WhereIF(input.userId.IsNotEmptyOrNull(), it=>it.UserId == input.userId)
            .Select<OrderCommissionSettlementListOutput>(it => new OrderCommissionSettlementListOutput
            {
                 userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}