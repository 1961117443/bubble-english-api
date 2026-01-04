using Microsoft.AspNetCore.Http;
using QT.Application.Entitys;
using QT.Application.Entitys.Dto.SmsCustomer;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Core;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using System.Data;

namespace QT.Application;

/// <summary>
/// 业务实现：短信平台-客户信息.
/// </summary>
[ApiDescriptionSettings("乾通ERP.V2", Tag = "客户信息", Name = "Customer", Order = 200)]
[Route("api/extend/sms/[controller]")]
public class SmsCustomerService : QTBaseService<SmsCustomerEntity, SmsCustomerCrInput, SmsCustomerUpInput, SmsCustomerInfoOutput, PageInputBase, SmsCustomerListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<SmsCustomerEntity> _repository;

    public SmsCustomerService(ISqlSugarRepository<SmsCustomerEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<SmsCustomerListOutput>> GetList([FromQuery] PageInputBase input)
    {
        var data = await _repository.Context.Queryable<SmsCustomerEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.CustomerName.Contains(input.keyword) || x.ContactTel.Contains(input.keyword))
          .Select<SmsCustomerListOutput>()
          .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<SmsCustomerListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("list")]
    public Task<List<SmsCustomerListOutput>> GetNoPageList([FromQuery] PageInputBase input)
    {
        return _repository.Context.Queryable<SmsCustomerEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.CustomerName.Contains(input.keyword) || x.ContactTel.Contains(input.keyword))
          .Select<SmsCustomerListOutput>()
          .ToListAsync();
    }
}
