using QT.Application.Entitys;
using QT.Application.Entitys.Dto.SmsCustomerGroup;
using QT.DependencyInjection;
using QT.DynamicApiController;
using Yitter.IdGenerator;

namespace QT.Application;

/// <summary>
/// 业务实现：短信平台-客户分组.
/// </summary>
[ApiDescriptionSettings("乾通ERP.V2", Tag = "客户分组", Name = "CustomerGroup", Order = 200)]
[Route("api/extend/sms/[controller]")]
public class SmsCustomerGroupService : QTBaseService<SmsCustomerGroupEntity, SmsCustomerGroupCrInput, SmsCustomerGroupUpInput, SmsCustomerGroupInfoOutput, PageInputBase, SmsCustomerGroupListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<SmsCustomerGroupEntity> _repository;

    public SmsCustomerGroupService(ISqlSugarRepository<SmsCustomerGroupEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<SmsCustomerGroupInfoOutput> GetInfo(string id)
    {
        var output = await base.GetInfo(id);

        output.smsCustomerGroupDetails = await _repository.Context.Queryable<SmsCustomerGroupDetailEntity>()
            .InnerJoin<SmsCustomerEntity>((x, y) => x.CId == y.Id)
            .Where((x, y) => x.GId == id).Select((x, y) => new SmsCustomerGroupDetailInfoOutput
            {
                id = SqlFunc.ToString(x.Id),
                cid = x.CId,
                gid = x.GId,
                customerName = y.CustomerName
            }).ToListAsync();
        return output;
    }

    protected override async Task BeforeCreate(SmsCustomerGroupCrInput input, SmsCustomerGroupEntity entity)
    {
        await base.BeforeCreate(input, entity);

        if (input.smsCustomerGroupDetails.IsAny())
        {
            var smsCustomerGroupDetails = input.smsCustomerGroupDetails.Select(x=> new SmsCustomerGroupDetailEntity
            {
                Id = YitIdHelper.NextId(),
                GId = entity.Id,
                CId = x.cid
            }).ToList();

            await _repository.Context.Insertable<SmsCustomerGroupDetailEntity>(smsCustomerGroupDetails).ExecuteCommandAsync();
        }
    }


    protected override async Task BeforeUpdate(SmsCustomerGroupUpInput input, SmsCustomerGroupEntity entity)
    {
        await base.BeforeUpdate(input, entity);
        await _repository.Context.Deleteable<SmsCustomerGroupDetailEntity>().Where(x => x.GId == entity.Id).ExecuteCommandAsync();
        if (input.smsCustomerGroupDetails.IsAny())
        {
            var smsCustomerGroupDetails = input.smsCustomerGroupDetails.Select(x => new SmsCustomerGroupDetailEntity
            {
                Id = x.id.IsNullOrEmpty() ? YitIdHelper.NextId() : long.Parse(x.id),
                GId = entity.Id,
                CId = x.cid
            }).ToList();
            await _repository.Context.Insertable<SmsCustomerGroupDetailEntity>(smsCustomerGroupDetails).ExecuteCommandAsync();
        }
    }

    public override async Task Delete(string id)
    {
        _repository.Context.Deleteable<SmsCustomerGroupEntity>(id).AddQueue();
        _repository.Context.Deleteable<SmsCustomerGroupDetailEntity>(x=>x.GId == id).AddQueue();
        await _repository.Context.SaveQueuesAsync();
    }
}