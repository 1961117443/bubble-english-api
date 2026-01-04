using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcTalentJob;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.System;
using Yitter.IdGenerator;
using NPOI.POIFS.Crypt.Dsig;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才求职信息.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcTalentJob", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcTalentJobService : IJzrcTalentJobService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcTalentJobEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcTalentJobService"/>类型的新实例.
    /// </summary>
    public JzrcTalentJobService(
        ISqlSugarRepository<JzrcTalentJobEntity> jzrcTalentJobRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcTalentJobRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才求职信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcTalentJobInfoOutput>();
        data.jzrcTalentJobCompanys = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => it.JobId == data.id)
        .OrderByDescending(it => it.Amount)
            .OrderBy(it => it.CreatorTime)
            .Select(it => new JzrcTalentJobCompanyInfo
            {
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
                status = it.Status,
                creatorTime = it.CreatorTime,
                amount = it.Amount
            })
            .ToListAsync();
        return data;
    }

    /// <summary>
    /// 获取建筑人才求职信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcTalentJobListQueryInput input)
    {
        List<DateTime> queryRequiredStart = input.requiredStart?.Split(',').ToObject<List<DateTime>>();
        DateTime? startRequiredStart = queryRequiredStart?.First();
        DateTime? endRequiredStart = queryRequiredStart?.Last();
        List<DateTime> queryRequiredEnd = input.requiredEnd?.Split(',').ToObject<List<DateTime>>();
        DateTime? startRequiredEnd = queryRequiredEnd?.First();
        DateTime? endRequiredEnd = queryRequiredEnd?.Last();
        var data = await _repository.Context.Queryable<JzrcTalentJobEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.region), it => it.Region.Equals(input.region))
            .WhereIF(!string.IsNullOrEmpty(input.candidateType), it => it.CandidateType.Equals(input.candidateType))
            .WhereIF(queryRequiredStart != null, it => SqlFunc.Between(it.RequiredStart, startRequiredStart.ParseToDateTime("yyyy-MM-dd 00:00:00"), endRequiredStart.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(queryRequiredEnd != null, it => SqlFunc.Between(it.RequiredEnd, startRequiredEnd.ParseToDateTime("yyyy-MM-dd 00:00:00"), endRequiredEnd.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Region.Contains(input.keyword)
                || it.CandidateType.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentJobListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                region = it.Region,
                candidateType = it.CandidateType,
                jobSalary = it.JobSalary,
                socialSecurityStatus = it.SocialSecurityStatus,
                performanceSituation = it.PerformanceSituation,
                certificateId = it.CertificateId,
                requiredStart = it.RequiredStart,
                requiredEnd = it.RequiredEnd,
                price = it.Price,
                status = it.Status ?? 0,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd=>ddd.Id == it.TalentId).Select(ddd=>ddd.Name),
                regionName = SqlFunc.Subqueryable<ProvinceEntity>().Where(ddd=>ddd.Id == it.Region).Select(ddd=>ddd.FullName),
                certificateCategoryId = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd=>ddd.Id == it.CertificateId).Select(ddd=>ddd.CategoryId),
                certificateIdName = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.CertificateName),
                level = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.Level)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTalentJobListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才求职信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcTalentJobCrInput input)
    {
        var entity = input.Adapt<JzrcTalentJobEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才求职信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcTalentJobUpInput input)
    {
        var entity = input.Adapt<JzrcTalentJobEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才求职信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcTalentJobEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    #region 发布、撤回求职信息
    /// <summary>
    /// 发布求职信息.
    /// 更新招聘状态，扣减保证金
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/Publish/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Publish(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcTalentJobEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.AuditTime.HasValue)
        {
            throw Oops.Oh("职位已发布！");
        }


        var jzrcMemberEntity = await _repository.Context.Queryable<JzrcMemberEntity>().Where(it => it.RelationId == entity.TalentId && it.Role == Entitys.Dto.AppService.AppLoginUserRole.Talent).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);


        _repository.Context.Tracking(entity);
        _repository.Context.Tracking(jzrcMemberEntity);

        //1、计算所需保证金，
        var margin = 100; // 假设保证金固定是100元
        //2、扣减保证金，不足的话报错，扣减余额
        if (jzrcMemberEntity.Amount - margin < 0)
        {
            throw Oops.Oh("账户余额不足，请充值后再进行操作！");
        }
        jzrcMemberEntity.Amount -= margin;
        jzrcMemberEntity.Margin += margin;
        var isOk = await _repository.Context.Updateable<JzrcMemberEntity>(jzrcMemberEntity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //3、记录余额变动记录
        JzrcMemberAmountLogEntity jzrcMemberAmountLogEntity = new JzrcMemberAmountLogEntity
        {
            Id = YitIdHelper.NextId(),
            Amount = margin * (-1),
            AddTime = DateTime.Now,
            Remark = $"发布求职信息[{entity.Id}]，扣除保证金",
            UserId = jzrcMemberEntity.Id,
            Category = (int)JzrcAmountLogEnum.Margin
        };
        isOk = await _repository.Context.Insertable<JzrcMemberAmountLogEntity>(jzrcMemberAmountLogEntity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //4、更新岗位状态
        entity.AuditTime = DateTime.Now;
        entity.AuditUserId = _userManager.UserId;
        entity.Status = 1;
        entity.AuditUserName = _userManager.RealName;
        isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

    }

    /// <summary>
    /// 撤回求职信息.
    /// 更新求职状态，退回保证金
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/NoPublish/{id}")]
    [SqlSugarUnitOfWork]
    public async Task NoPublish(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcTalentJobEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (!entity.AuditTime.HasValue)
        {
            throw Oops.Oh("职位未发布！");
        }

        // 判断是否有人报名，有人报名了取消的话，不退保证金


        var jzrcMemberEntity = await _repository.Context.Queryable<JzrcMemberEntity>().Where(it => it.RelationId == entity.TalentId && it.Role == Entitys.Dto.AppService.AppLoginUserRole.Talent).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);


        _repository.Context.Tracking(entity);
        _repository.Context.Tracking(jzrcMemberEntity);

        //1、计算所需保证金
        var margin = 100; // 假设保证金固定是100元
        //2、扣减保证金，不足的话报错，扣减余额
        if (jzrcMemberEntity.Margin - margin < 0)
        {
            throw Oops.Oh("保证金不足，请联系管理员！");
        }
        jzrcMemberEntity.Amount += margin;
        jzrcMemberEntity.Margin -= margin;
        var isOk = await _repository.Context.Updateable<JzrcMemberEntity>(jzrcMemberEntity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //3、记录余额变动记录
        JzrcMemberAmountLogEntity jzrcMemberAmountLogEntity = new JzrcMemberAmountLogEntity
        {
            Id = YitIdHelper.NextId(),
            Amount = margin,
            AddTime = DateTime.Now,
            Remark = $"撤回求职信息[{entity.Id}]，退回保证金",
            UserId = jzrcMemberEntity.Id,
            Category = (int)JzrcAmountLogEnum.Margin
        };
        isOk = await _repository.Context.Insertable<JzrcMemberAmountLogEntity>(jzrcMemberAmountLogEntity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //4、更新岗位状态
        entity.AuditTime = null;
        entity.AuditUserId = string.Empty;
        entity.AuditUserName = string.Empty;
        entity.Status = 0;
        isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    #region 前台服务==============================================================================
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <returns></returns>
    [HttpGet("client/list")]
    public async Task<dynamic> ClientGetList([FromQuery] ClientJzrcCompanyJobListQueryInput input)
    {
        var today = DateTime.Now.Date;
        var data = await _repository.Context.Queryable<JzrcTalentJobEntity>()
            .Where(it => it.Status == 1)
            .Where(it => SqlFunc.Between(today, it.RequiredStart, it.RequiredEnd))
            //.Where(it => SqlFunc.GreaterThan(it.RequiredStart, today) && SqlFunc.LessThan(it.RequiredEnd, today))
            .WhereIF(!string.IsNullOrEmpty(input.region) && input.region != "0", it => it.Region.Equals(input.region))
            .WhereIF(!string.IsNullOrEmpty(input.certificateCategoryId), it => SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(x=>x.Id == it.CertificateId && x.CategoryId == input.certificateCategoryId).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentJobListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                region = it.Region,
                candidateType = it.CandidateType,
                jobSalary = it.JobSalary,
                socialSecurityStatus = it.SocialSecurityStatus,
                performanceSituation = it.PerformanceSituation,
                certificateId = it.CertificateId,
                requiredStart = it.RequiredStart,
                requiredEnd = it.RequiredEnd,
                price = it.Price,
                status = it.Status ?? 0,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                regionName = SqlFunc.Subqueryable<ProvinceEntity>().Where(ddd => ddd.Id == it.Region).Select(ddd => ddd.FullName),
                certificateCategoryId = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.CategoryId),
                certificateIdName = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.CertificateName),
                level = SqlFunc.Subqueryable<JzrcTalentCertificateEntity>().Where(ddd => ddd.Id == it.CertificateId).Select(ddd => ddd.Level)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTalentJobListOutput>.SqlSugarPageResult(data);
    }
    #endregion
}