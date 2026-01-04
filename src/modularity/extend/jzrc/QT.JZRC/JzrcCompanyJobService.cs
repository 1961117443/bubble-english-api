using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;
using Org.BouncyCastle.Crypto;

namespace QT.JZRC;

/// <summary>
/// 业务实现：企业招聘.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcCompanyJob", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCompanyJobService : IJzrcCompanyJobService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCompanyJobEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCompanyJobService"/>类型的新实例.
    /// </summary>
    public JzrcCompanyJobService(
        ISqlSugarRepository<JzrcCompanyJobEntity> jzrcCompanyJobRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcCompanyJobRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取企业招聘.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCompanyJobInfoOutput>();

        data.jzrcCompanyJobTalents = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => it.JobId == data.id)
            .OrderByDescending(it => it.Amount)
            .OrderBy(it => it.CreatorTime)
            .Select(it => new JzrcCompanyJobTalentInfo
            {
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                status = it.Status,
                creatorTime = it.CreatorTime,
                amount = it.Amount
            })
            .ToListAsync();

        return data;
    }

    /// <summary>
    /// 获取企业招聘列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCompanyJobListQueryInput input)
    {
        List<DateTime> queryRequiredStart = input.requiredStart?.Split(',').ToObject<List<DateTime>>();
        DateTime? startRequiredStart = queryRequiredStart?.First();
        DateTime? endRequiredStart = queryRequiredStart?.Last();
        var region = input.region?.Split(',').ToList().Last();
        var data = await _repository.Context.Queryable<JzrcCompanyJobEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.jobTitle), it => it.JobTitle.Contains(input.jobTitle))
            .WhereIF(!string.IsNullOrEmpty(input.candidateType), it => it.CandidateType.Equals(input.candidateType))
            .WhereIF(queryRequiredStart != null, it => SqlFunc.Between(it.RequiredStart, startRequiredStart.ParseToDateTime("yyyy-MM-dd 00:00:00"), endRequiredStart.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.region), it => it.Region.Contains(region))
            .WhereIF(!string.IsNullOrEmpty(input.certificateCategoryId), it => it.CertificateCategoryId.Equals(input.certificateCategoryId))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.JobTitle.Contains(input.keyword)
                || it.CandidateType.Contains(input.keyword)
                || it.Region.Contains(input.keyword)
                || it.CertificateCategoryId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCompanyJobListOutput
            {
                id = it.Id,
                companyId = it.CompanyId,
                jobTitle = it.JobTitle,
                candidateType = it.CandidateType,
                number = it.Number,
                jobSalary = it.JobSalary,
                requiredStart = it.RequiredStart,
                requiredEnd = it.RequiredEnd,
                region = it.Region,
                certificateCategoryId = it.CertificateCategoryId,
                certificateLevel = it.CertificateLevel,
                status = it.Status ?? 0,
                price = it.Price,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd=>ddd.Id == it.CompanyId).Select(ddd=>ddd.CompanyName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        var ids = data.list.Select(it => it.id).ToArray();
        var list = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => ids.Contains(it.JobId)).Select(it => new JzrcCompanyJobTalentEntity
        {
            JobId = it.JobId,
            Status = it.Status
        }).ToListAsync();

        foreach (var item in data.list)
        {
            item.postNum = list.Where(a => a.JobId == item.id).Count();
            item.signNum = list.Where(a => a.JobId == item.id && a.Status == 1).Count();
        }

        return PageResult<JzrcCompanyJobListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建企业招聘.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCompanyJobCrInput input)
    {
        var entity = input.Adapt<JzrcCompanyJobEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.CreatorUserName = _userManager.RealName;
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新企业招聘.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCompanyJobUpInput input)
    {
        var entity = input.Adapt<JzrcCompanyJobEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除企业招聘.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCompanyJobEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //var isOk = await _repository.Context.Deleteable<JzrcCompanyJobEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    #region 发布、撤回招聘信息
    /// <summary>
    /// 发布企业招聘.
    /// 更新招聘状态，扣减保证金
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/Publish/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Publish(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCompanyJobEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.AuditTime.HasValue)
        {
            throw Oops.Oh("职位已发布！");
        }


        var jzrcMemberEntity = await _repository.Context.Queryable<JzrcMemberEntity>().Where(it => it.RelationId == entity.CompanyId && it.Role == Entitys.Dto.AppService.AppLoginUserRole.Company).FirstAsync();
        if (jzrcMemberEntity == null)
        {
            throw Oops.Oh("该企业未注册会员！");
        }


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
            Remark = $"发布岗位[{entity.Id}]，扣除保证金",
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
    /// 取消发布企业招聘.
    /// 更新招聘状态，退回保证金
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Actions/NoPublish/{id}")]
    [SqlSugarUnitOfWork]
    public async Task NoPublish(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCompanyJobEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (!entity.AuditTime.HasValue)
        {
            throw Oops.Oh("职位未发布！");
        }

        // 判断是否有人报名，有人报名了取消的话，不退保证金


        var jzrcMemberEntity = await _repository.Context.Queryable<JzrcMemberEntity>().Where(it => it.RelationId == entity.CompanyId && it.Role == Entitys.Dto.AppService.AppLoginUserRole.Company).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);


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
            Remark = $"取消岗位[{entity.Id}]，退回保证金",
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
        var data = await _repository.Context.Queryable<JzrcCompanyJobEntity>()
            .Where(it => it.Status == 1)
            .Where(it => SqlFunc.Between(today, it.RequiredStart, it.RequiredEnd))
            //.Where(it => SqlFunc.GreaterThan(it.RequiredStart, today) && SqlFunc.LessThan(it.RequiredEnd, today))
            .WhereIF(!string.IsNullOrEmpty(input.region) && input.region != "0", it => it.Region.Equals(input.region))
            .WhereIF(!string.IsNullOrEmpty(input.certificateCategoryId), it => it.CertificateCategoryId.Equals(input.certificateCategoryId))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ClientJzrcCompanyJobListOutput
            {
                id = it.Id,
                companyId = it.CompanyId,
                jobTitle = it.JobTitle,
                candidateType = it.CandidateType,
                number = it.Number,
                jobSalary = it.JobSalary,
                requiredStart = it.RequiredStart,
                requiredEnd = it.RequiredEnd,
                region = it.Region,
                certificateCategoryId = it.CertificateCategoryId,
                certificateLevel = it.CertificateLevel,
                performanceSituation = it.PerformanceSituation ?? "-",
                socialSecurityStatus = it.SocialSecurityStatus ?? "-",
                price = it.Price,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ClientJzrcCompanyJobListOutput>.SqlSugarPageResult(data);
    } 
    #endregion
}