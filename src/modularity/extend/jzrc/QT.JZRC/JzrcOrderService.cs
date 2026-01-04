using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Contracts;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys;
using QT.JZRC.Entitys.Dto.JzrcOrder;
using QT.JZRC.Interfaces;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才平台订单管理.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcOrder", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcOrderService : IJzrcOrderService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcOrderEntity> _repository;

    /// <summary>
    /// 单据规则服务.
    /// </summary>
    private readonly IBillRule _billRullService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcOrderService"/>类型的新实例.
    /// </summary>
    public JzrcOrderService(
        ISqlSugarRepository<JzrcOrderEntity> jzrcOrderRepository,
        IBillRule billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcOrderRepository;
        _billRullService = billRullService;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才平台订单管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcOrderInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才平台订单管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcOrderListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcOrderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.orderNo), it => it.OrderNo.Contains(input.orderNo))
            .WhereIF(!string.IsNullOrEmpty(input.managerId), it => it.ManagerId.Equals(input.managerId))
            .WhereIF(input.talentIdName.IsNotEmptyOrNull(),it=> SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId && ddd.Name.Contains(input.talentIdName)).Any())
            .WhereIF(input.companyIdName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId && ddd.CompanyName.Contains(input.companyIdName)).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.OrderNo.Contains(input.keyword)
                || it.ManagerId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcOrderListOutput
            {
                id = it.Id,
                orderNo = it.OrderNo,
                talentId = it.TalentId,
                companyId = it.CompanyId,
                managerId = it.ManagerId,
                amount = it.Amount,
                talentShare = it.TalentShare,
                companyShare = it.CompanyShare,
                platformShare = it.PlatformShare,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd=>ddd.Id==it.TalentId).Select(ddd=>ddd.Name),
                companyIdName= SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcOrderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才平台订单管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcOrderCrInput input)
    {
        var entity = input.Adapt<JzrcOrderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.OrderNo = await _billRullService.GetBillNumber("dingdanbianhao");
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才平台订单管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcOrderUpInput input)
    {
        var entity = input.Adapt<JzrcOrderEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才平台订单管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();

        //var isOk = await _repository.Context.Deleteable<JzrcOrderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}