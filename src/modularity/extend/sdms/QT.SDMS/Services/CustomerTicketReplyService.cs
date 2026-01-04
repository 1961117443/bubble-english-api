using Mapster;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using QT.Common.Core;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.SDMS.Entitys.Dto.CustomerTicketReply;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：客户咨询.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "客户咨询回复", Name = "CustomerTicketReply", Order = 300)]
[Route("api/sdms/CustomerTicketReply")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class CustomerTicketReplyService : QTBaseService<CustomerTicketReplyEntity, CustomerTicketReplyCrInput, CustomerTicketReplyUpInput, CustomerTicketReplyOutput, CustomerTicketReplyListQueryInput, CustomerTicketReplyListOutput>, IDynamicApiController
{

    public CustomerTicketReplyService(ISqlSugarRepository<CustomerTicketReplyEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task BeforeCreate(CustomerTicketReplyCrInput input, CustomerTicketReplyEntity entity)
    {
        entity.ReplyById = _userManager.UserId;
        entity.ReplyBy = _userManager.RealName;
        await base.BeforeCreate(input, entity);
    }

    [HttpGet("Actions/{id}/List")]
    public async Task<List<CustomerTicketReplyListOutput>> GetList(string id)
    {
        var data = await _repository.Context.Queryable<CustomerTicketReplyEntity>()
            .Where(x => x.TicketId == id)
            .OrderBy(x => x.CreatorTime)
            .Select<CustomerTicketReplyListOutput>(x => new CustomerTicketReplyListOutput
            {
                ticketTime= x.CreatorTime,
                customerIdName = SqlFunc.Subqueryable<CustomerTicketEntity>().InnerJoin<CustomerEntity>((t, c) => t.CustomerId == c.Id).Where((t, c) => t.Id == x.TicketId).Select((t, c) => c.Name)
            }, true)
            .ToListAsync();



        foreach (var item in data)
        {
            item.managerInfo = new ManagersInfo
            {
                account = item.replyBy,
                firstName = item.replyBy.IsNotEmptyOrNull() ? item.replyBy.Substring(0, 1) : "",
                headIcon = ""
            };
        }
        return data;
    }
}