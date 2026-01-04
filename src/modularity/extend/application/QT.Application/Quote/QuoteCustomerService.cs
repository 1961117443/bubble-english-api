using QT.Common.Core;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.QuoteCustomer;
using QT.UnifyResult;

namespace QT.Extend;

/// <summary>
    /// 客户管理
    /// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "Extend", Name = "QuoteCustomer", Order = 600)]
[Route("api/extend/Quote/customer")]
public class QuoteCustomerService : QTBaseService<QuoteCustomerEntity,QuoteCustomerCrInput,QuoteCustomerUpInput,QuoteCustomerInfoOutput,QuoteCustomerListPageInput,QuoteCustomerListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<QuoteCustomerEntity> _repository;

    public QuoteCustomerService(ISqlSugarRepository<QuoteCustomerEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    protected override async Task BeforeCreate(QuoteCustomerCrInput input, QuoteCustomerEntity entity)
    {
        await base.BeforeCreate(input, entity);

        if (await _repository.Context.Queryable<QuoteCustomerEntity>().AnyAsync(x=>x.Name == entity.Name && x.Admintel == entity.Admintel))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        UnifyContext.Fill(new
        {
            id = entity.Id,
        });
    }

    public override async Task Update(string id, [FromBody] QuoteCustomerUpInput input)
    {
        if (await _repository.Context.Queryable<QuoteCustomerEntity>().AnyAsync(x => x.Name == input.name && x.Admintel == input.admintel && x.Id!=id))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        await base.Update(id, input);
    }


    [HttpGet("")]
    public override async Task<PageResult<QuoteCustomerListOutput>> GetList([FromQuery] QuoteCustomerListPageInput input)
    {
        var data = await _repository.Context.Queryable<QuoteCustomerEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(),x=>x.Name.Contains(input.keyword) || x.No.Contains(input.keyword) || x.Admintel.Contains(input.keyword))
             .Select<QuoteCustomerListOutput>()
             .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<QuoteCustomerListOutput>.SqlSugarPagedList(data);
    }
}
