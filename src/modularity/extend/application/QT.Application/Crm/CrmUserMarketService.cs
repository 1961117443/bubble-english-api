using Microsoft.AspNetCore.Mvc;
using Qiniu.Storage;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.CrmUserMarket;
using QT.Iot.Application.Entity;
using QT.Systems.Entitys.Permission;
using Spire.Presentation;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：营销管理-用户订单（我的部门）.
/// </summary>
[ApiDescriptionSettings("营销管理", Tag = "销售订单", Name = "CrmOrder", Order = 300)]
[Route("api/iot/crm/user/current/market")]
public class CrmUserMarketService : IDynamicApiController
{
    private readonly ISqlSugarRepository<CrmOrderEntity> _repository;
    private readonly IUserManager _userManager;

    public CrmUserMarketService(ISqlSugarRepository<CrmOrderEntity> repository, IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    [HttpGet("dashboard")]
    public async Task<CrmUserMarketOutput> GetInfo()
    {
        var currentId = _userManager.UserId;
        CrmUserMarketOutput CrmUserMarket = new CrmUserMarketOutput()
        {
            crmUserMarketers = new List<CrmUserMarketerOutput>(),
            myOrderList = new List<CrmUserMarketerOrderOutput>(),
            panels = new List<object>()
        };

        // 获取当前用户的所有营销人员
        var users = await _repository.Context.Queryable<UserEntity>()
            .ToChildListAsync(it => it.Sid, currentId);

        //var userList = users;
        //var userList = await _repository.Context.Queryable<UserEntity>().Where(it => users.Any(x => x.UserId == it.Id)).Select(it => new UserEntity
        //{
        //    Id = it.Id,
        //    RealName = it.RealName,
        //}).ToListAsync();

        // 获取所有的订单
        var orderList = await _repository.Context.Queryable<CrmOrderEntity>().Where(it => users.Any(x => x.Id == it.UserId)).ToListAsync();

        // 所有的订单提成
        var orderCommissionEntities = await _repository.Context.Queryable<CrmOrderCommissionEntity>().ToListAsync();

        var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        // 一级下级
        //Dictionary<string, List<string>> childUsers = new Dictionary<string, List<string>>();
        foreach (var item in users.Where(x => x.Sid == currentId))
        {
            var childs = GetAllChild(item.Id, users);

            var grandChilds = users.Where(x => x.Sid == item.Id).Select(x => x.Id).ToList();
            //childUsers.Add(item.UserId, GetAllChild(item.UserId,users)); 

            CrmUserMarketerOutput crmUserMarketerOutput = new CrmUserMarketerOutput()
            {
                id = item.Id,
                userName = users.Find(u=>u.Id == item.Id)?.RealName ?? item.Id,
                childMonAmount = 0,
                childTotalAmount = 0,
                monAmount = 0,// orderList.Where(x => x.UserId == item.UserId && x.OrderDate >= start && x.OrderDate<= end).Sum(x => x.Amount),
                totalAmount = 0, // orderList.Where(x => x.UserId == item.UserId).Sum(x => x.Amount),
                commission = 0,
                monCommission = 0,
                //deptCommission=0,
                //monDeptCommission=0,
                grandAmount = orderList.Where(x => grandChilds.Contains( x.UserId)).Sum(x => x.Amount),
                monGrandAmount = orderList.Where(x => grandChilds.Contains(x.UserId) && x.OrderDate >= start && x.OrderDate <= end).Sum(x => x.Amount)
            };

            #region 计算提成（给部门主管创造的提成）
            // 计算提成（给部门主管创造的提成）
            //var mygrouporders = orderList.Where(x => x.UserId == item.UserId || childs.Contains(x.UserId)).Select(x => x.Id).ToList();
            var qur = from a in orderList
                      join b in orderCommissionEntities on a.Id equals b.FId
                      where b.UserId == currentId && (a.UserId == item.Id || grandChilds.Contains(a.UserId))
                      select new CrmUserMarketerOrderOutput
                      {
                          orderDate = a.OrderDate,
                          no = a.No,
                          amount = a.Amount,
                          remark = a.Remark,
                          commission = b.Amount
                      };


            // 计算我的提成
            foreach (var xitem in qur)
            {
                // 判断是否当月
                if (xitem.orderDate >= start && xitem.orderDate <= end)
                {
                    crmUserMarketerOutput.monCommission += xitem.commission;
                }
                crmUserMarketerOutput.commission += xitem.commission;
            } 
            #endregion

            for (int i = orderList.Count - 1; i >= 0; i--)
            {
                var order = orderList[i];
                bool remove = false;

                // 计算本人业绩
                if (order.UserId == item.Id)
                {
                    remove = true;
                    // 下级的订单
                    crmUserMarketerOutput.totalAmount += order.Amount;

                    // 判断是否当月
                    if (order.OrderDate >= start && order.OrderDate <= end)
                    {
                        crmUserMarketerOutput.monAmount += order.Amount;
                    }
                }

                if (childs.Contains(order.UserId))
                {
                    remove = true;
                    // 孙子的订单
                    crmUserMarketerOutput.childTotalAmount += order.Amount;

                    // 判断是否当月
                    if (order.OrderDate >= start && order.OrderDate <= end)
                    {
                        crmUserMarketerOutput.childMonAmount += order.Amount;
                    }
                }


                if (remove)
                {
                    //orderList.RemoveAt(i); // 删除的话，后面的计算就没用了
                }
            }

            // 计算孙级业绩


            CrmUserMarket.crmUserMarketers.Add(crmUserMarketerOutput);
        }


        //CrmUserMarket.monAmount = orderList.Where(x => x.UserId == currentId && x.OrderDate >= start && x.OrderDate <= end).Sum(x => x.Amount);
        //CrmUserMarket.totalAmount = orderList.Where(x => x.UserId == currentId).Sum(x => x.Amount);
        //CrmUserMarket.deptMonAmount = CrmUserMarket.crmUserMarketers.Sum(x=>x.monAmount + x.childMonAmount);
        //CrmUserMarket.deptTotalAmount = CrmUserMarket.crmUserMarketers.Sum(x => x.totalAmount + x.childTotalAmount);

        // 获取我的订单业绩
        var q = from a in orderList
                join b in orderCommissionEntities on a.Id equals b.FId
                where b.UserId == currentId
                select new CrmUserMarketerOrderOutput
                {
                    orderDate = a.OrderDate,
                    no = a.No,
                    amount = a.Amount,
                    remark = a.Remark,
                    commission = b.Amount,
                    owner = a.UserId
                };
        //CrmUserMarket.myOrderList = q.OrderByDescending(x => x.orderDate).ToList();

        var allChilds = GetAllChild(currentId, users); // 整个部门的人，
        
        // panel 统计
        CrmUserMarket.panels = new List<object>
        {
            new { title="个人当月业绩",num=(float)orderList.Where(x => x.UserId == currentId && x.OrderDate >= start && x.OrderDate <= end).Sum(x => x.Amount)},
            new { title="个人当月提成",num=(float)q.Where(x=> x.orderDate >= start && x.orderDate <= end).Sum(x=>x.commission)},
            new { title="部门当月业绩",num=(float)CrmUserMarket.crmUserMarketers.Sum(x=>x.monAmount + x.childMonAmount)},
            new { title="部门当月提成",num=(float)orderCommissionEntities.Where(x=> allChilds.Contains(x.UserId) && orderList.Any(o=>o.Id==x.FId && o.OrderDate >= start && o.OrderDate <= end)).Sum(x=>x.Amount)},
            new { title="个人业绩累计",num=(float)orderList.Where(x => x.UserId == currentId).Sum(x => x.Amount)},
            new { title="个人提成累计",num=(float)q.Sum(x=>x.commission)},
            new { title="部门业绩累计",num=(float)CrmUserMarket.crmUserMarketers.Sum(x => x.totalAmount + x.childTotalAmount)},
            new { title="部门提成累计",num=(float)orderCommissionEntities.Where(x=> allChilds.Contains(x.UserId)).Sum(x=>x.Amount)}
        };

        // 统计完后，个人业绩只显示自己的订单
        CrmUserMarket.myOrderList = q.Where(x => x.owner == currentId).OrderByDescending(x => x.orderDate).ToList();

        return CrmUserMarket;
    }

    /// <summary>
    /// 获取所有的下级，不包含userId
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="users"></param>
    /// <returns></returns>
    private List<string> GetAllChild(string userId, List<UserEntity> users)
    {
        List<string> list = new List<string>();

        Action<string> fetch = null;
        fetch = id =>
        {
            foreach (var item in users.Where(x => x.Sid == id))
            {
                list.Add(item.Id);
                fetch!(item.Id);
            }
        };

        fetch(userId);
        return list;
    }
}
