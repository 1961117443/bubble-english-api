using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.SDMS.Entitys.Dto.CustomerTicket;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：客户咨询.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "客户咨询", Name = "CustomerTicket", Order = 300)]
[Route("api/sdms/CustomerTicket")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class CustomerTicketService : QTBaseService<CustomerTicketEntity, CustomerTicketCrInput, CustomerTicketUpInput, CustomerTicketOutput, CustomerTicketListQueryInput, CustomerTicketListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerTicketService(ISqlSugarRepository<CustomerTicketEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(CustomerTicketCrInput input, CustomerTicketEntity entity)
    {
        entity.No = await _billRullService.GetBillNumber("SdmsCustomerTicket");
        await base.BeforeCreate(input, entity);
    }

    public override async Task<CustomerTicketOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        if (data.customerId.IsNotEmptyOrNull())
        {
            var c = await _repository.Context.Queryable<CustomerEntity>().Where(x => x.Id == data.customerId).Select(x => new
            {
                customerName = x.Name,
                managerName = SqlFunc.Subqueryable<UserEntity>().Where(ddd=>ddd.Id == x.ManagerId).Select(ddd => ddd.RealName)
            }).FirstAsync();
            data.customerName = c?.customerName;
            data.managerName = c?.managerName;
        }
        
        return data;
    }

    protected override async Task<SqlSugarPagedList<CustomerTicketListOutput>> GetPageList([FromQuery] CustomerTicketListQueryInput input)
    {
        return await _repository.Context.Queryable<CustomerTicketEntity>()
            .WhereIF(!_userManager.IsAdministrator, it => it.ManagerId == _userManager.UserId)
            .WhereIF(input.status.HasValue, it=>it.Status == input.status)
            .WhereIF(input.customerId.IsNotEmptyOrNull(),it=>it.CustomerId == input.customerId)
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CustomerTicketListOutput>(it => new CustomerTicketListOutput
            {
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName),
                customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == it.CustomerId).Select(ddd => ddd.Name),
                id = it.Id,
                ticketTime = it.CreatorTime
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    /// <summary>
    /// 工单对当前用户拥有的权限
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Permission")]
    public async Task<CustomerTicketCurrentUserAuth> GetCurrentAuth(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        CustomerTicketCurrentUserAuth workOrderCurrentUserAuth = new CustomerTicketCurrentUserAuth()
        {
            //回复权限：提交人或者指派人 == 当前用户
            replayAuth = (entity.CreatorUserId == _userManager.UserId || entity.ManagerId == _userManager.UserId),
            //结束权限：提交人或者指派人 == 当前用户
            endAuth = (entity.CreatorUserId == _userManager.UserId || entity.ManagerId == _userManager.UserId),
            //指派权限：1、个人工单：提交人 == 当前用户；2、团队工单：当前用户 = 团队管理员；3、项目工单：当前用户 = 项目管理员
            assignAuth = false
        };
        //if (entity.AssignUserId.IsNullOrEmpty())
        //{
        //    if (entity.Category == (int)WorkOrderCategory.Personal)
        //    {
        //        // 个人工单：提交人 == 当前用户
        //        workOrderCurrentUserAuth.assignAuth = entity.EnabledMark == 1 && (entity.CreatorUserId == _userManager.UserId);
        //    }
        //    if (entity.Category == (int)WorkOrderCategory.Team)
        //    {
        //        // 团队工单：当前用户 = 团队管理员
        //        if (entity.Tid.IsNotEmptyOrNull())
        //        {
        //            var managerIds = await _repository.Context.Queryable<TeamEntity>().Where(x => x.Id == entity.Tid).Select(x => x.ManagerIds).FirstAsync();
        //            if (managerIds.IsNotEmptyOrNull() && managerIds.Split(',').Contains(_userManager.UserId))
        //            {
        //                workOrderCurrentUserAuth.assignAuth = true;
        //            }
        //        }
        //    }

        //    if (entity.Category == (int)WorkOrderCategory.Project)
        //    {
        //        //项目工单：当前用户 = 项目管理员
        //        if (entity.Pid.IsNotEmptyOrNull())
        //        {
        //            var managerIds = await _repository.Context.Queryable<ProjectGanttEntity>().Where(x => x.Id == entity.Pid).Select(x => x.ManagerIds).FirstAsync();
        //            if (managerIds.IsNotEmptyOrNull() && managerIds.Split(',').Contains(_userManager.UserId))
        //            {
        //                workOrderCurrentUserAuth.assignAuth = true;
        //            }
        //        }
        //    }
        //}


        return workOrderCurrentUserAuth;
    }
}