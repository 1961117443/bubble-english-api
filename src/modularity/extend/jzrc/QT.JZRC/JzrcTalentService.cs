using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys;
using QT.JZRC.Entitys.Dto.AppService;
using QT.JZRC.Entitys.Dto.AppService.Login;
using QT.JZRC.Entitys.Dto.JzrcCompany;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys.Dto.JzrcContract;
using QT.JZRC.Entitys.Dto.JzrcTalent;
using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;
using QT.JZRC.Entitys.Dto.JzrcTalentDemand;
using QT.JZRC.Entitys.Dto.JzrcTalentJob;
using QT.JZRC.Interfaces;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using SqlSugar;
using System.Data;

namespace QT.JZRC;

/// <summary>
/// 业务实现：人才信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "JzrcTalent", Order = 200)]
[Route("api/JZRC/[controller]")]
[Injection(Named = nameof(AppLoginUserRole.Talent))]
public class JzrcTalentService : IJzrcTalentService, IDynamicApiController, ITransient, IJzrcAppLogin
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcTalentEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcTalentService"/>类型的新实例.
    /// </summary>
    public JzrcTalentService(
        ISqlSugarRepository<JzrcTalentEntity> jzrcTalentRepository,
        ISqlSugarClient context,
        IUserManager userManager, IFileManager fileManager)
    {
        _repository = jzrcTalentRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 获取人才信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcTalentInfoOutput>();

        var jzrcTalentCertificateList = await _repository.Context.Queryable<JzrcTalentCertificateEntity>().Where(w => w.TalentId == output.id).ToListAsync();
        output.jzrcTalentCertificateList = jzrcTalentCertificateList.Adapt<List<JzrcTalentCertificateInfoOutput>>();

        output.jzrcTalentCommunicationList = await _repository.Context.Queryable<JzrcTalentCommunicationEntity>().Where(w => w.TalentId == output.id)
            .OrderByDescending(it=>it.CommunicationTime)
            .Select(it => new JzrcTalentCommunicationInfoOutput
            {
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.ManagerId).Select(ddd => ddd.RealName),
            },true)
            .ToListAsync();
        //output.jzrcTalentCommunicationList = jzrcTalentCommunicationList.Adapt<List<JzrcTalentCommunicationInfoOutput>>();

        output.jzrcTalentContractList = await _repository.Context.Queryable<JzrcContractEntity>()
            .Where(it => it.TalentId == id)
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
            }).ToListAsync();

        output.jzrcTalentJobCompanys = await _repository.Context.Queryable<JzrcCompanyJobTalentEntity>().Where(it => it.TalentId == id)
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

        var jzrcTalentHandoverList = await _repository.Context.Queryable<JzrcTalentHandoverEntity>().Where(w => w.TalentId == output.id).ToListAsync();
        output.jzrcTalentHandoverList = jzrcTalentHandoverList.Adapt<List<JzrcTalentHandoverInfoOutput>>();


        output.jzrcTalentDemandList = await _repository.Context.Queryable<JzrcTalentDemandEntity>()
            .Where(it => it.TalentId == id)
            .Select(it => new JzrcTalentDemandListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                content = it.Content,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd => ddd.Id == it.TalentId).Select(ddd => ddd.Name),
                creatorTime = it.CreatorTime
            }).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取人才信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcTalentListQueryInput input)
    {
        var regions = input.region?.Split(",", true) ?? new string[0];
        var data = await _repository.Context.Queryable<JzrcTalentEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(regions.IsAny(), it => regions.Contains(it.Region))
            .WhereIF(!string.IsNullOrEmpty(input.mobilePhone), it => it.MobilePhone.Contains(input.mobilePhone))
            .WhereIF(input.signed == 2, it => (it.ManagerId ?? "") == "")
            .WhereIF(input.signed == 1, it=> SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd=>ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Talent).Any())
            .WhereIF(input.signed == 0, it => SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd => ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Talent).NotAny())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Region.Contains(input.keyword)
                || it.RegistrationCategory.Contains(input.keyword)
                )
            .WhereIF(input.managerId.IsNotEmptyOrNull(),it=>it.ManagerId == input.managerId)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentListOutput
            {
                id = it.Id,
                name = it.Name,
                gender = it.Gender,
                region = SqlFunc.Subqueryable<ProvinceEntity>().Where(ddd=>ddd.Id == it.Region).Select(ddd=>ddd.FullName),
                registrationCategory = it.RegistrationCategory,
                major1 = it.Major1,
                major2 = it.Major2,
                major3 = it.Major3,
                salaryRequirement = it.SalaryRequirement,
                managerIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd=>ddd.Id == it.ManagerId).Select(ddd=>ddd.RealName),
                signed = SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd=>ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Talent).Any(),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTalentListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建人才信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcTalentCrInput input)
    {
        var entity = input.Adapt<JzrcTalentEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        // 手机号码是否存在
        if (!string.IsNullOrEmpty(entity.MobilePhone) && await _repository.Context.Queryable<JzrcTalentEntity>().AnyAsync(x => x.MobilePhone == entity.MobilePhone))
        {
            throw Oops.Oh("手机号码已存在，不允许重复添加！");
        }
        // 身份证号码是否存在
        if (!string.IsNullOrEmpty(entity.IdCard) && await _repository.Context.Queryable<JzrcTalentEntity>().AnyAsync(x => x.IdCard == entity.IdCard))
        {
            throw Oops.Oh("身份证号码已存在，不允许重复添加！");
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<JzrcTalentEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var jzrcTalentCertificateEntityList = input.jzrcTalentCertificateList.Adapt<List<JzrcTalentCertificateEntity>>();
            if(jzrcTalentCertificateEntityList != null)
            {
                foreach (var item in jzrcTalentCertificateEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.TalentId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcTalentCertificateEntity>(jzrcTalentCertificateEntityList).ExecuteCommandAsync();
            }

            var jzrcTalentCommunicationEntityList = input.jzrcTalentCommunicationList.Adapt<List<JzrcTalentCommunicationEntity>>();
            if(jzrcTalentCommunicationEntityList != null)
            {
                foreach (var item in jzrcTalentCommunicationEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.TalentId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcTalentCommunicationEntity>(jzrcTalentCommunicationEntityList).ExecuteCommandAsync();
            }

            var jzrcTalentHandoverEntityList = input.jzrcTalentHandoverList.Adapt<List<JzrcTalentHandoverEntity>>();
            if (jzrcTalentHandoverEntityList != null)
            {
                foreach (var item in jzrcTalentHandoverEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.TalentId = newEntity.Id;
                }

                await _repository.Context.Insertable<JzrcTalentHandoverEntity>(jzrcTalentHandoverEntityList).ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 获取人才信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] JzrcTalentListQueryInput input)
    {
        return await _repository.Context.Queryable<JzrcTalentEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.region), it => it.Region.Contains(input.region))
            .WhereIF(!string.IsNullOrEmpty(input.mobilePhone), it => it.MobilePhone.Contains(input.mobilePhone))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Region.Contains(input.keyword)
                || it.RegistrationCategory.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentListOutput
            {
                id = it.Id,
                name = it.Name,
                gender = it.Gender,
                region = it.Region,
                registrationCategory = it.RegistrationCategory,
                major1 = it.Major1,
                major2 = it.Major2,
                major3 = it.Major3,
                salaryRequirement = it.SalaryRequirement,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出人才信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] JzrcTalentListQueryInput input)
    {
        var exportData = new List<JzrcTalentListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<JzrcTalentListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"姓名\",\"field\":\"name\"},{\"value\":\"性别\",\"field\":\"gender\"},{\"value\":\"区域\",\"field\":\"region\"},{\"value\":\"注册类别\",\"field\":\"registrationCategory\"},{\"value\":\"薪资要求\",\"field\":\"salaryRequirement\"},{\"value\":\"专业1\",\"field\":\"major1\"},{\"value\":\"专业2\",\"field\":\"major2\"},{\"value\":\"专业3\",\"field\":\"major3\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "人才信息管理.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<JzrcTalentListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 批量删除人才信息.
    /// </summary>
    /// <param name="ids">主键数组.</param>
    /// <returns></returns>
    [HttpPost("batchRemove")]
    public async Task BatchRemove([FromBody] List<string> ids)
    {
        var entitys = await _repository.Context.Queryable<JzrcTalentEntity>()
            .In(it => it.Id, ids)
            .Where(it=> SqlFunc.Subqueryable<JzrcMemberEntity>().Where(ddd=>ddd.RelationId == it.Id && ddd.Role == AppLoginUserRole.Talent).NotAny()) // 过滤掉已入驻的数据
            .ToListAsync();
        if (entitys.Count > 0)
        {
            try
            {
                // 开启事务
                _db.BeginTran();

                foreach (var entity in entitys)
                {
                    _repository.Context.Tracking(entity);
                    entity.Delete();
                }
                await _repository.Context.Updateable(entitys).ExecuteCommandAsync();

                //// 批量删除人才信息
                //await _repository.Context.Deleteable<JzrcTalentEntity>().In(it => it.Id,ids).ExecuteCommandAsync();

                //// 清空人才证书表数据
                //await _repository.Context.Deleteable<JzrcTalentCertificateEntity>().In(u => u.TalentId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                //// 清空人才沟通记录表数据
                //await _repository.Context.Deleteable<JzrcTalentCommunicationEntity>().In(u => u.TalentId, entitys.Select(s => s.Id).ToArray()).ExecuteCommandAsync();

                // 关闭事务
                _db.CommitTran();
            }
            catch (Exception)
            {
                // 回滚事务
                _db.RollbackTran();
                throw Oops.Oh(ErrorCode.COM1002);
            }
        }
    }

    /// <summary>
    /// 更新人才信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcTalentUpInput input)
    {
        var entity = input.Adapt<JzrcTalentEntity>();

        // 手机号码是否存在
        if (!string.IsNullOrEmpty(entity.MobilePhone) && await _repository.Context.Queryable<JzrcTalentEntity>().AnyAsync(x => x.Id != entity.Id && x.MobilePhone == entity.MobilePhone))
        {
            throw Oops.Oh("手机号码已存在，不允许重复添加！");
        }
        // 身份证号码是否存在
        if (!string.IsNullOrEmpty(entity.IdCard) && await _repository.Context.Queryable<JzrcTalentEntity>().AnyAsync(x => x.Id != entity.Id && x.IdCard == entity.IdCard))
        {
            throw Oops.Oh("身份证号码已存在，不允许重复添加！");
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<JzrcTalentEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 人才证书原有数据
            await _repository.Context.CUDSaveAsnyc<JzrcTalentCertificateEntity>(it => it.TalentId == entity.Id, input.jzrcTalentCertificateList, it => it.TalentId = entity.Id);

            //// 新增人才证书新数据
            //var jzrcTalentCertificateEntityList = input.jzrcTalentCertificateList.Adapt<List<JzrcTalentCertificateEntity>>();
            //if(jzrcTalentCertificateEntityList != null)
            //{
            //    foreach (var item in jzrcTalentCertificateEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.TalentId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcTalentCertificateEntity>(jzrcTalentCertificateEntityList).ExecuteCommandAsync();
            //}

            // 人才沟通记录原有数据
            await _repository.Context.CUDSaveAsnyc<JzrcTalentCommunicationEntity>(it => it.TalentId == entity.Id, input.jzrcTalentCommunicationList, it => it.TalentId = entity.Id);

            // 人才交接记录原有数据
            await _repository.Context.CUDSaveAsnyc<JzrcTalentHandoverEntity>(it => it.TalentId == entity.Id, input.jzrcTalentHandoverList, it => it.TalentId = entity.Id);

            //// 新增人才沟通记录新数据
            //var jzrcTalentCommunicationEntityList = input.jzrcTalentCommunicationList.Adapt<List<JzrcTalentCommunicationEntity>>();
            //if(jzrcTalentCommunicationEntityList != null)
            //{
            //    foreach (var item in jzrcTalentCommunicationEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.TalentId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<JzrcTalentCommunicationEntity>(jzrcTalentCommunicationEntityList).ExecuteCommandAsync();
            //}

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 删除人才信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCompanyEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        //如果已入驻，不允许删除
        if (await _repository.Context.Queryable<JzrcMemberEntity>().Where(it => it.RelationId == entity.Id && it.Role == AppLoginUserRole.Talent).AnyAsync())
        {
            throw Oops.Oh("该企业已入驻，不允许删除！");
        }

        _repository.Context.Tracking(entity);
        entity.Delete();
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();

        //if (!await _repository.Context.Queryable<JzrcTalentEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<JzrcTalentEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //        // 清空人才证书表数据
        //    await _repository.Context.Deleteable<JzrcTalentCertificateEntity>().Where(it => it.TalentId.Equals(entity.Id)).ExecuteCommandAsync();

        //        // 清空人才沟通记录表数据
        //    await _repository.Context.Deleteable<JzrcTalentCommunicationEntity>().Where(it => it.TalentId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1002);
        //}
    }

    /// <summary>
    /// 分配客户经理
    /// </summary>
    /// <returns></returns>
    [HttpPut("Actions/Allot")]
    public async Task<int> AllotManager([FromBody] JzrcCompanyAllotManagerInput input)
    {
        if (input.radio == 1 && !input.ids.IsAny())
        {
            throw Oops.Oh("请选择数据！");
        }
        List<string> regions = new List<string>();
        if (input.radio == 2)
        {
            if (string.IsNullOrEmpty(input.province))
            {
                throw Oops.Oh("请选择区域！");
            }
            if (input.number < 0)
            {
                throw Oops.Oh("数量不能小于等于0！");
            }
            regions.Add(input.province); 
            var arr = input.province.Split(',').ToArray();
            regions.AddRange(arr);
            var list1= await _repository.Context.Queryable<ProvinceEntity>().In(it => it.Id, arr).Select(it => it.FullName).ToListAsync();
            regions.AddRange(list1);
        }
        int num = input.radio == 1 ? input.ids.Length : Math.Min(input.number, 1000);

        // 使用分布式锁
        using (var l = RedisHelper.Lock($"JzrcTalentService:AllotManager:{_userManager.TenantId}", 5))
        {
            var qur = input.radio == 1 ? _repository.Where(x => input.ids.Contains(x.Id)) : _repository.Where(x => regions.Contains(x.Region) && string.IsNullOrEmpty(x.ManagerId));
            var list = await qur.Take(num).ToListAsync();

            list.ForEach(x =>
            {
                x.ManagerId = input.managerId;
            });

            var result = await _repository.Context.Updateable<JzrcTalentEntity>(list)
                .UpdateColumns(nameof(JzrcTalentEntity.ManagerId))
                .ExecuteCommandAsync();

            return result;
        }

    }

    /// <summary>
    /// 添加沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Communication")]
    public async Task JzrcTalentCommunication(string id, [FromBody] JzrcTalentCommunicationCrInput input)
    {
        var company = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        // 当前用户非客户经理，禁止添加
        if (company.ManagerId != _userManager.UserId)
        {
            throw Oops.Oh(ErrorCode.D1013);
        }

        var entity = input.Adapt<JzrcTalentCommunicationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.ManagerId = _userManager.UserId;
        entity.TalentId = id;
        if (!entity.CommunicationTime.HasValue)
        {
            entity.CommunicationTime = DateTime.Now;
        }

        await _repository.Context.Insertable<JzrcTalentCommunicationEntity>(entity).ExecuteCommandAsync();
    }


    /// <summary>
    /// 获取人才的沟通记录
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Communication")]
    public async Task<List<JzrcTalentCommunicationInfoOutput>> JzrcTalentCommunicationList(string id)
    {
        var jzrcTalentCommunicationList = await _repository.Context.Queryable<JzrcTalentCommunicationEntity>().Where(w => w.TalentId == id).OrderByDescending(it => it.CommunicationTime).ToListAsync();
        return jzrcTalentCommunicationList.Adapt<List<JzrcTalentCommunicationInfoOutput>>();
    }

    /// <summary>
    /// 获取人才的证书列表
    /// </summary>
    /// <param name="talentId"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/CertificateList")]
    public async Task<List<JzrcTalentCertificateListOutput>> GetTalentCertificateList(string id)
    {
        var data = await _repository.Context.Queryable<JzrcTalentCertificateEntity>()
            .Where(it => it.TalentId == id)
            .OrderByDescending(it => it.CreatorTime)
            .Select(it => new JzrcTalentCertificateListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                certificateName = it.CertificateName,
                level = it.Level,
                acquisitionTime = it.AcquisitionTime,
                issuingOrganization = it.IssuingOrganization,
                validityPeriod = it.ValidityPeriod,
                remark = it.Remark,
                region = it.Region,
                categoryId = it.CategoryId,
                certificateNo = it.CertificateNo
            }).ToListAsync();

        return data;
    }

    #region 导入数据
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        var dataList = new List<JzrcTalentImportDataTemplate>() { new JzrcTalentImportDataTemplate() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("人才信息导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<JzrcTalentImportDataTemplate>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<JzrcTalentImportDataTemplate>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("Actions/Import")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> ImportData(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);

        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<JzrcTalentImportDataInput>();

        string? filePath = FileVariable.TemporaryFilePath;
        string? savePath = Path.Combine(filePath, _fileName);

        // 得到数据
        var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
        if (excelData != null)
        {
            foreach (DataColumn item in excelData.Columns)
            {
                var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(key))
                {
                    item.ColumnName = key;
                    //excelData.Columns[item.ToString()].ColumnName = ;
                }
            }

        }

        var list = _repository.Context.Utilities.DataTableToList<JzrcTalentImportDataInput>(excelData);
        var provinceList = await _repository.Context.Queryable<ProvinceEntity>().Select(m => new ProvinceEntity {
            Id = m.Id,
            FullName = m.FullName
        }).Where(m => m.ParentId == "-1" && m.DeleteMark == null && m.EnabledMark == 1).ToListAsync();
        //检查企业名称是否为空
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.name))
            {
                throw Oops.Oh("名称不能为空！");
            }
            if (string.IsNullOrWhiteSpace(item.mobilePhone))
            {
                throw Oops.Oh("手机号码不能为空！");
            }

            //if (!DateTime.TryParse(item.approvalDate, out var dt))
            //{
            //    item.approvalDate = null;
            //}
            //if (!DateTime.TryParse(item.establishmentDate, out var dt1))
            //{
            //    item.establishmentDate = null;
            //}

            // 区域转换
            item.region = provinceList.Find(w => w.FullName == item.region)?.Id ?? "";
        }
        var g = list.Where(x => x.mobilePhone.IsNotEmptyOrNull()).GroupBy(x => x.mobilePhone)
            .Select(x => new { key = x.Key, count = x.Count() }).Where(x=>x.count>1).ToList();
        if (g.Any())
        {
            throw Oops.Oh($"[{string.Join(",", g.Select(x => x.key))}]手机号码重复，请检查后再导入！");
        }



        //转化为实体 ErpProductEntity\ErpProductmodelEntity 
        var insertEntities = list.Where(x => string.IsNullOrEmpty(x.id)).Adapt<List<JzrcTalentEntity>>();
        var updateEntities = list.Where(x => !string.IsNullOrEmpty(x.id)).Adapt<List<JzrcTalentEntity>>();

        if (updateEntities.Any())
        {
            await updateEntities.PagingLoop(async (items, index) =>
            {
                var tempIdList = items.Select(x => x.Id).ToList();

                var dbEntitys = await _repository.Where(x => tempIdList.Contains(x.Id)).ToListAsync();



                for (int i = items.Count - 1; i > -1; i--)
                {
                    var item = items[i];
                    var entity = dbEntitys.Find(x => x.Id == item.Id);
                    if (entity == null)
                    {
                        insertEntities.Add(entity);
                    }
                    else
                    {
                        _repository.Context.Tracking(item);
                        item.Adapt(entity);
                        items.RemoveAt(i);
                    }
                }
                if (items.IsAny())
                {
                    await _repository.Context.Updateable(items).ExecuteCommandAsync();
                }

            });
        }

        //更新数据库
        if (insertEntities.Any())
        {
            insertEntities.ForEach(x => x.Id ??= SnowflakeIdHelper.NextId());
            await _repository.Context.Insertable(insertEntities).ExecuteCommandAsync();
        }

        return new { np = insertEntities.Count, up = updateEntities.Count };
    }

    #endregion

    #region client 前台服务
    ///// <summary>
    ///// 前台登录
    ///// </summary>
    ///// <param name="loginInput"></param>
    ///// <returns></returns>
    ///// <exception cref="NotImplementedException"></exception>
    //public async Task<AppLoginUser?> GetByAccount(string account)
    //{
    //    var entity = await _repository.SingleAsync(x => x.MobilePhone == account);
    //    if (entity == null)
    //    {
    //        return null;
    //    }

    //    AppLoginUser appLoginUser = new AppLoginUser
    //    {
    //        Id = entity.Id,
    //        Account = entity.MobilePhone,
    //        RealName = entity.Name,
    //        Role = AppLoginUserRole.Talent
    //    };

    //    return appLoginUser;
    //}

    /// <summary>
    /// 创建或者获取人才信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<AppLoginUser?> GetOrCreateAsync(AppLoginCrInput input)
    {
        if (input.phone.IsNullOrEmpty())
        {
            throw Oops.Oh("手机号码不能为空！");
        }
        if (input.name.IsNullOrEmpty())
        {
            throw Oops.Oh("姓名不能为空！");
        }
        // 分布式锁
        using (var l = RedisHelper.Lock($"AppLoginUser:{input.phone}", 5))
        {
            var entity = await _repository.SingleAsync(x => x.MobilePhone == input.phone);
            if (entity == null)
            {
                entity = new JzrcTalentEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MobilePhone = input.phone,
                    Name = input.name
                };
                await _repository.Context.Insertable<JzrcTalentEntity>(entity).IgnoreColumns(true).ExecuteCommandAsync();
            }

            return new AppLoginUser
            {
                Id = entity.Id,
                Account = entity.MobilePhone,
                RealName = entity.Name,
                Role = AppLoginUserRole.Talent
            };
        }
    }
    #endregion
}