using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcContract;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Contracts;
using QT.JsonSerialization;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才合同管理.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcContract", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcContractService : IJzrcContractService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcContractEntity> _repository;

    /// <summary>
    /// 单据规则服务.
    /// </summary>
    private readonly IBillRule _billRullService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcContractService"/>类型的新实例.
    /// </summary>
    public JzrcContractService(
        ISqlSugarRepository<JzrcContractEntity> jzrcContractRepository,
        IBillRule billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcContractRepository;
        _billRullService = billRullService;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才合同管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcContractInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才合同管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcContractListQueryInput input)
    {
        List<DateTime> querySignTime = input.signTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startSignTime = querySignTime?.First();
        DateTime? endSignTime = querySignTime?.Last();
        var data = await _repository.Context.Queryable<JzrcContractEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.talentId), it => it.TalentId.Contains(input.talentId))
            .WhereIF(!string.IsNullOrEmpty(input.companyId), it => it.CompanyId.Contains(input.companyId))
            .WhereIF(querySignTime != null, it => SqlFunc.Between(it.SignTime, startSignTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endSignTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.TalentId.Contains(input.keyword)
                || it.CompanyId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcContractListOutput
            {
                id = it.Id,
                no = it.No,
                talentId = it.TalentId,
                companyId = it.CompanyId,
                certificateId = it.CertificateId,
                jobId = it.JobId,
                signTime = it.SignTime,
                amount = it.Amount,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcContractListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才合同管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcContractCrInput input)
    {
        var entity = input.Adapt<JzrcContractEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("hetongbianhao");

        await SetProperty(entity);

        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才合同管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcContractUpInput input)
    {
        var entity = input.Adapt<JzrcContractEntity>();
        await SetProperty(entity);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才合同管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcContractEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    #region Private Method

    [NonAction]
    public async Task SetProperty(JzrcContractEntity entity)
    {
        entity.TalentProperty = string.Empty;
        entity.CompanyProperty = string.Empty;
        entity.JobProperty = string.Empty;
        entity.CertificateProperty = string.Empty;
        if (entity.TalentId.IsNotEmptyOrNull())
        {
            var record = await _repository.Context.Queryable<JzrcTalentEntity>().InSingleAsync(entity.TalentId);
            if (record!=null)
            {
                entity.TalentProperty = JSON.Serialize(record);
            }
        }

        if (entity.CompanyId.IsNotEmptyOrNull())
        {
            var record = await _repository.Context.Queryable<JzrcCompanyEntity>().InSingleAsync(entity.CompanyId);
            if (record != null)
            {
                entity.CompanyProperty = JSON.Serialize(record);
            }
        }

        if (entity.JobId.IsNotEmptyOrNull())
        {
            var record = await _repository.Context.Queryable<JzrcCompanyJobEntity>().InSingleAsync(entity.JobId);
            if (record != null)
            {
                entity.JobProperty = JSON.Serialize(record);
            }
        }

        if (entity.CertificateId.IsNotEmptyOrNull())
        {
            var record = await _repository.Context.Queryable<JzrcTalentCertificateEntity>().InSingleAsync(entity.CertificateId);
            if (record != null)
            {
                entity.CertificateProperty = JSON.Serialize(record);
            }
        }
    } 
    #endregion
}