using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.SDMS.Entitys.Dto.Customer;
using QT.SDMS.Entitys.Dto.CustomerCommunication;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Crm;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：客户管理.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "客户管理", Name = "Customer", Order = 300)]
[Route("api/sdms/customer")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class CustomerService : QTBaseService<CustomerEntity, CustomerCrInput, CustomerUpInput, CustomerOutput, CustomerListQueryInput, CustomerListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerService(ISqlSugarRepository<CustomerEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task<SqlSugarPagedList<CustomerListOutput>> GetPageList([FromQuery] CustomerListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();

        bool isCurrent = input.managerId.IsNullOrEmpty() && !_userManager.IsAdministrator;

        return await _repository.Context.Queryable<CustomerEntity>()
            .WhereIF(isCurrent, it => it.ManagerId == _userManager.UserId)
            .WhereIF(input.status.HasValue, it=>it.Status == input.status)
            .WhereIF(input.managerId.IsNotEmptyOrNull(),it=>it.ManagerId == input.managerId)
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.contactName), it => it.ContactName.Contains(input.contactName))
            .WhereIF(!string.IsNullOrEmpty(input.contactPhone), it => it.ContactPhone.Contains(input.contactPhone))
            .WhereIF(!string.IsNullOrEmpty(input.email), it => it.Email.Contains(input.email))
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CustomerListOutput>(it => new CustomerListOutput
            {
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName),
                id = it.Id,
                name = it.Name,
                //adminName = it.AdminName,
                //adminTel = it.AdminTel,
                remark = it.Remark,
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }


    #region 沟通记录

    /// <summary>
    /// 添加沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Communication")]
    public async Task AddCommunication(string id, [FromBody] CustomerCommunicationCrInput input)
    {
        var user = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        //// 当前用户非客户经理，禁止添加
        //if (company.ManagerId != _userManager.UserId)
        //{
        //    throw Oops.Oh(ErrorCode.D1013);
        //}

        var entity = input.Adapt<CustomerCommunicationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.ManagerId = _userManager.UserId;
        entity.CustomerId = id;
        if (!entity.CommunicationTime.HasValue)
        {
            entity.CommunicationTime = DateTime.Now;
        }

        await _repository.Context.Insertable<CustomerCommunicationEntity>(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取客户的沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Communication")]
    public async Task<List<CustomerCommunicationInfoOutput>> GetCommunicationList(string id)
    {
        var jzrcCompanyCommunicationList = await _repository.Context.Queryable<CustomerCommunicationEntity>().Where(w => w.CustomerId == id)
            .OrderByDescending(w => w.CommunicationTime).ToListAsync();
        return jzrcCompanyCommunicationList.Adapt<List<CustomerCommunicationInfoOutput>>();
    } 
    #endregion
}