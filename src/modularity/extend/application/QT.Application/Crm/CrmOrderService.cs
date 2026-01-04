using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core;
using QT.DynamicApiController;
using SqlSugar;
using QT.Iot.Application.Entity;
using QT.Iot.Application.Dto.CrmOrder;
using QT.Common.Extension;
using QT.Systems.Interfaces.System;
using QT.Common.Security;
using QT.Systems.Entitys.Permission;
using QT.Common.Core.Security;
using NPOI.SS.Formula.Functions;
using QT.FriendlyException;
using QT.Iot.Application.Dto.CrmMarketer;
using System.Runtime.CompilerServices;
using QT.UnifyResult;
using System.ComponentModel.DataAnnotations;
using QT.Iot.Application.Dto.CrmOrderCommission;
using QT.Common.Enum;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：销售订单.
/// </summary>
[ApiDescriptionSettings("营销管理", Tag = "销售订单", Name = "CrmOrder", Order = 300)]
[Route("api/iot/crm/order")]
public class CrmOrderService : QTBaseService<CrmOrderEntity, CrmOrderCrInput, CrmOrderUpInput, CrmOrderOutput, CrmOrderListQueryInput, CrmOrderListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CrmOrderService(ISqlSugarRepository<CrmOrderEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(CrmOrderCrInput input, CrmOrderEntity entity)
    {
        entity.No = await _billRullService.GetBillNumber("IotCrmOrder");
        await base.BeforeCreate(input, entity);
    }

    protected override async Task AfterCreate(CrmOrderEntity entity)
    {
        await this.CalcCommission(entity);
        await this.CalcBusinessCount(entity);
    }

    public override async Task<CrmOrderOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        data.crmOrderCommissions = await _repository.Context.Queryable<CrmOrderCommissionEntity>()
            .Where(x => x.FId == id)
            .Select<CrmOrderCommissionOutput>((x) => new CrmOrderCommissionOutput
            {
                userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.UserId).Select(ddd => ddd.RealName),
                balanceIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.BalanceUserId).Select(ddd => ddd.RealName)
            }, true).ToListAsync();
        return data;
    }

    protected override async Task<SqlSugarPagedList<CrmOrderListOutput>> GetPageList([FromQuery] CrmOrderListQueryInput input)
    {
        List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CrmOrderEntity>()
            .LeftJoin<UserEntity>((it,a)=>it.UserId == a.Id)
            .LeftJoin<UserEntity>((it, a,b) => a.Sid == b.Id)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
            .WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CrmOrderListOutput>((it,a,b) => new CrmOrderListOutput
            {
                userIdName = a.RealName,
                parentUserName = b.RealName,
                grandfatherUserName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == b.Sid).Select(ddd => ddd.RealName)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    protected override async Task AfterDelete(CrmOrderEntity entity)
    {
        await base.AfterDelete(entity);

        // 删除订单分成
        await _repository.Context.Deleteable<CrmOrderCommissionEntity>().Where(x => x.FId == entity.Id).ExecuteCommandAsync();

        await this.CalcBusinessCount(entity);
    }

    protected override async Task AfterUpdate(CrmOrderUpInput input, CrmOrderEntity entity)
    {
        // 重新计算分成
        var list = await _repository.Context.Queryable<CrmOrderCommissionEntity>().Where(x => x.FId == entity.Id).ToListAsync();

        foreach (var item in list)
        {
            _repository.Context.Tracking(item);
            item.Amount = Math.Round(entity.Amount * item.Proportion / 100, 2);
        }
        await _repository.Context.Updateable<CrmOrderCommissionEntity>(list).ExecuteCommandAsync();
    }

    /// <summary>
    /// 计算每月分红
    /// </summary>
    /// <returns></returns>
    [HttpGet("bonus/mon")]
    public async Task<List<CrmMarketerBounsListOutput>> CalcMonBonus([FromQuery,Required]string mon)
    {
        if (!DateTime.TryParse($"{mon}-01", out DateTime stDate))
        {
            throw Oops.Oh("月份格式不正确[yyyy-MM]");
        }
        var endDate = stDate.AddMonths(1).AddDays(-1);
        // 查询所有的营销人员
        var crmMarketerList = await _repository.Context.Queryable<UserEntity>()
             .LeftJoin<CrmMarketerEntity>((b,a) => a.UserId == b.Id)
             .Select(( b, a) => new CrmMarketerBounsListOutput
             {
                 id = b.Id,
                 fullName = b.RealName,
                 parentId = b.Sid, // a.ManagerId,
                 level = a.Level
             })
             .ToListAsync();

        // 查询所有的订单
        var orderList = await _repository.AsQueryable().Select(it => new CrmOrderEntity
        {
            OrderDate = it.OrderDate,
            UserId = it.UserId,
            Amount = it.Amount
        }).ToListAsync();

        decimal allTolAmount = 0; // 公司总业绩
        decimal allMonAmount = 0; // 公司当月业绩

        foreach (var item in orderList)
        {
            if (item.OrderDate >= stDate && item.OrderDate <= endDate)
            {
                allMonAmount += item.Amount;
            }
            allTolAmount += item.Amount;
        }


        //A.当月1 %: 主管和部门经理，小部门业务总和大于等于30 % (小部门指除了第一外以外的所有部门)
        //B.当月1 %: 部门经理，公司总业绩 >= 500万，当月小部门业务总和 >= 30 %
        //C.当月1 %: 部门经理，公司总业绩 >=1500万，当前小部门业务总和 >=30 %


        foreach (var market in crmMarketerList)
        {
            // 统计个人数据
            foreach (var order in orderList.Where(x => x.UserId == market.id))
            {
                market.personAmount += order.Amount;
                if (order.OrderDate >= stDate && order.OrderDate <= endDate)
                {
                    market.monPersonAmount += order.Amount;
                }
            }


            // 统计他的部门数据  一级部门
            var firstChildList = crmMarketerList.Where(x => x.parentId == market.id);

            if (firstChildList.IsAny())
            {
                 //统计部门数据 部门id，累计业绩，当月业绩
                List<(string, decimal, decimal)> deptList = new List<(string, decimal, decimal)>();

                foreach (var firstChild in firstChildList)
                {
                    var idList = GetAllChild(firstChild.id, crmMarketerList);
                    idList.Add(firstChild.id);

                    decimal total = 0;
                    decimal monTotal = 0;
                    foreach (var order in orderList.Where(x => idList.Contains(x.UserId)))
                    {
                        total += order.Amount;
                        if (order.OrderDate>= stDate && order.OrderDate <=endDate)
                        {
                            monTotal += order.Amount;
                        }
                    }
                    deptList.Add((firstChild.id, total, monTotal));
                }

                // 部门总业绩
                market.deptAmount = deptList.Sum(x => x.Item2); 
                // 部门当月业绩
                market.monDeptAmount = deptList.Sum(x => x.Item3);

                // 小部门业绩
                market.minDeptAmount= market.deptAmount - deptList.Max(x => x.Item2);
                // 当月小部门业绩
                market.monMinDeptAmount = market.monDeptAmount - deptList.Max(x => x.Item3);
            }
        }

        //A.当月1 %: 主管和部门经理，小部门业务总和大于等于30 % (小部门指除了第一外以外的所有部门)
        //B.当月1 %: 部门经理，公司总业绩 >= 500万，当月小部门业务总和 >= 30 %
        //C.当月1 %: 部门经理，公司总业绩 >=1500万，当前小部门业务总和 >=30 %
        decimal level1 = 5000000m;
        decimal level2 = 15000000m;
        List<(string, decimal)> bonus = new List<(string, decimal)>();
        foreach (var item in crmMarketerList.Where(x=>x.level == MarketLevel.BusinessManager || x.level == MarketLevel.DivisionManager))
        {
            if (item.minDeptAmount>= (0.3m * item.deptAmount))
            {
                bonus.Add((item.id, allMonAmount * 0.01m));// A
            }

            if (item.level == MarketLevel.DivisionManager)
            {
                if (allTolAmount >= level1 && item.monMinDeptAmount >= (0.3m * item.monDeptAmount))
                {
                    bonus.Add((item.id, allMonAmount * 0.01m));// B
                }
                if (allTolAmount >= level2 && item.minDeptAmount >= (0.3m * item.deptAmount))
                {
                    bonus.Add((item.id, allMonAmount * 0.01m));// C
                }
            }            
        }
       
        var result = bonus.GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Sum(a => a.Item2));

        crmMarketerList.ForEach(x =>
        {
            if (result.ContainsKey(x.id))
            {
                x.bonus = result[x.id];
            }
        });


        //crmMarketerList= crmMarketerList.TreeWhere(x => x.bonus > 0, x => x.id, x => x.parentId);


        UnifyContext.Fill(new
        {
            allTolAmount,
            allMonAmount,
            allMonBonus = result.Sum(x=>x.Value),
        });

        return crmMarketerList.ToTree();
    }


    #region Private Methods

    /// <summary>
    /// 计算订单分成
    /// </summary>
    /// <returns></returns>
    private async Task CalcCommission(CrmOrderEntity entity)
    {
        if (entity.UserId.IsNullOrEmpty())
        {
            throw Oops.Oh("订单缺少业务员!");
        }
        var markers = await _repository.Context.Queryable<UserEntity>()
            .LeftJoin<UserEntity>((a, b) => a.Sid == b.Id)
            .LeftJoin<UserEntity>((a, b, c) => b.Sid == c.Id)
            .Where(a => a.Id == entity.UserId)
            .Select((a, b, c) => new
            {
                userId = a.Id,
                parentId = b.Id,
                grandpaId = c.Id
            })
            .FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);
        if (markers == null)
        {
            throw Oops.Oh("当前下单员非营销人员！");
        }

        // 10% 业务员 ,
        // 1%  下级
        // 0.50 % 孙级产生业务
        List<CrmOrderCommissionEntity> crmOrderCommissionEntities = new List<CrmOrderCommissionEntity>
        {
            new CrmOrderCommissionEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                FId = entity.Id,
                Amount = Math.Round(entity.Amount * 0.1m, 2),
                Proportion = 10,
                UserId = markers.userId,
                No = $"{entity.No}-01"
            }
        };

        // 判断上级 = 1%
        if (markers.parentId.IsNotEmptyOrNull())
        {
            crmOrderCommissionEntities.Add(new CrmOrderCommissionEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                FId = entity.Id,
                Amount = Math.Round(entity.Amount * 0.01m, 2),
                Proportion = 1,
                UserId = markers.parentId,
                No = $"{entity.No}-02"
            });
        }

        // 判断上上级 = 0.5%
        if (markers.grandpaId.IsNotEmptyOrNull())
        {
            crmOrderCommissionEntities.Add(new CrmOrderCommissionEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                FId = entity.Id,
                Amount = Math.Round(entity.Amount * 0.005m, 2),
                Proportion = 0.5m,
                UserId = markers.grandpaId,
                No = $"{entity.No}-03"
            });
        }

        if (crmOrderCommissionEntities.IsAny())
        {
            await _repository.Context.Insertable<CrmOrderCommissionEntity>(crmOrderCommissionEntities).ExecuteCommandAsync();
        }

    }

    /// <summary>
    /// 计算订单量
    /// </summary>
    /// <returns></returns>
    private async Task CalcBusinessCount(CrmOrderEntity entity)
    {
        var count = await _repository.Context.Queryable<CrmOrderEntity>().Where(x => x.UserId == entity.UserId).CountAsync();

        var crmMarketerEntity = await _repository.Context.Queryable<CrmMarketerEntity>().FirstAsync(x => x.UserId == entity.UserId);

        if (crmMarketerEntity!=null)
        {
            crmMarketerEntity.BusinessCount = count;
            await _repository.Context.Updateable<CrmMarketerEntity>(crmMarketerEntity).UpdateColumns(x => x.BusinessCount).ExecuteCommandAsync();
        }
        else
        {
            crmMarketerEntity = new CrmMarketerEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                UserId = entity.UserId,
                BusinessCount = count,
                ManagerId = await _repository.Context.Queryable<UserEntity>().Where(x => x.Id == entity.UserId).Select(x => x.Sid).FirstAsync()
            };
            await _repository.Context.Insertable<CrmMarketerEntity>(crmMarketerEntity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取所有的下级
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="users"></param>
    /// <returns></returns>
    private List<string> GetAllChild(string userId, List<CrmMarketerBounsListOutput> users)
    {
        List<string> list = new List<string>();

        Action<string> fetch = null;
        fetch = id =>
        {
            foreach (var item in users.Where(x => x.parentId == id))
            {
                list.Add(item.id);
                fetch!(item.id);
            }
        };

        fetch(userId);
        return list;
    }
    #endregion
}