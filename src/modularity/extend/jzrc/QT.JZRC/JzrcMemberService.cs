using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.ClayObject;
using QT.Common.Configuration;
using QT.Common.Models.NPOI;
using QT.DataEncryption;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcMember;
using QT.JZRC.Entitys.Dto.JzrcMemberAmountLog;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑平台会员信息.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcMember", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcMemberService : IJzrcMemberService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcMemberEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly Func<string, ITransient, object> _resolveNamed;

    /// <summary>
    /// 初始化一个<see cref="JzrcMemberService"/>类型的新实例.
    /// </summary>
    public JzrcMemberService(
        ISqlSugarRepository<JzrcMemberEntity> jzrcMemberRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        Func<string, ITransient, object> resolveNamed)
    {
        _repository = jzrcMemberRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _resolveNamed = resolveNamed;
    }

    /// <summary>
    /// 获取建筑平台会员信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcMemberInfoOutput>();

        var jzrcMemberAmountLogList = await _repository.Context.Queryable<JzrcMemberAmountLogEntity>().Where(w => w.UserId == output.id).ToListAsync();
        output.jzrcMemberAmountLogList = jzrcMemberAmountLogList.Adapt<List<JzrcMemberAmountLogInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取建筑平台会员信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcMemberListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcMemberEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.account), it => it.Account.Contains(input.account))
            .WhereIF(!string.IsNullOrEmpty(input.nickName), it => it.NickName.Contains(input.nickName))
            .WhereIF(!string.IsNullOrEmpty(input.role), it => it.Role.Equals(input.role))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Account.Contains(input.keyword)
                || it.NickName.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcMemberListOutput
            {
                id = it.Id,
                account = it.Account,
                nickName = it.Role == Entitys.Dto.AppService.AppLoginUserRole.Talent ? SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd=>ddd.Id== it.RelationId).Select(ddd=>ddd.Name)
                : SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.RelationId).Select(ddd => ddd.CompanyName),
                role = it.Role,
                relationId = it.RelationId,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcMemberListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑平台会员信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcMemberCrInput input)
    {
        var entity = input.Adapt<JzrcMemberEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        // 判断 账号+类型是否存在
        if (await _repository.AnyAsync(it=>it.Account == entity.Account && it.Role == entity.Role))
        {
            throw Oops.Oh("账号已存在！");
        }

        // 新建的时候，密码默认后手机号码后四位
        entity.Secretkey = Guid.NewGuid().ToString();
        var password = $"abc{input.account.Substring(input.account.Length - 4)}";
        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(MD5Encryption.Encrypt(password) + entity.Secretkey);
        entity.Password = encryptPasswod;

        if (entity.RelationId.IsNullOrEmpty())
        {
            // 绑定人才表或者企业表
            var appLoginService = _resolveNamed(entity.Role.ToString(), default) as IJzrcAppLogin;
            var loginInfo = await appLoginService.GetOrCreateAsync(new Entitys.Dto.AppService.AppLoginCrInput
            {
                phone = entity.Account,
                name = entity.Account
            });
            entity.RelationId = loginInfo.Id;
        }

        // 判断是否已经绑定
        if (entity.RelationId.IsNotEmptyOrNull())
        {
            if (await _repository.AnyAsync(it => it.RelationId == entity.RelationId))
            {
                throw Oops.Oh("请勿重复绑定关联信息！");
            }
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<JzrcMemberEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

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
    /// 获取建筑平台会员信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] JzrcMemberListQueryInput input)
    {
        return await _repository.Context.Queryable<JzrcMemberEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.account), it => it.Account.Contains(input.account))
            .WhereIF(!string.IsNullOrEmpty(input.nickName), it => it.NickName.Contains(input.nickName))
            .WhereIF(!string.IsNullOrEmpty(input.role), it => it.Role.Equals(input.role))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Account.Contains(input.keyword)
                || it.NickName.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcMemberListOutput
            {
                id = it.Id,
                account = it.Account,
                nickName = it.NickName,
                role = it.Role,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出建筑平台会员信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] JzrcMemberListQueryInput input)
    {
        var exportData = new List<JzrcMemberListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<JzrcMemberListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"手机号码\",\"field\":\"account\"},{\"value\":\"呢称\",\"field\":\"nickName\"},{\"value\":\"账号类型\",\"field\":\"role\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "会员管理.xls";
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
        ExcelExportHelper<JzrcMemberListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 更新建筑平台会员信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcMemberUpInput input)
    {
        var entity = input.Adapt<JzrcMemberEntity>();

        // 判断 账号+类型是否存在
        if (await _repository.AnyAsync(it => it.Id!=entity.Id && it.Account == entity.Account && it.Role == entity.Role))
        {
            throw Oops.Oh("账号已存在！");
        }

        // 判断是否已经绑定
        if (entity.RelationId.IsNotEmptyOrNull())
        {
            if (await _repository.AnyAsync(it => it.RelationId == entity.RelationId && it.Id !=entity.Id))
            {
                throw Oops.Oh("请勿重复绑定关联信息！");
            }
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<JzrcMemberEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

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
    /// 删除建筑平台会员信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcMemberEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();

        //if (!await _repository.Context.Queryable<JzrcMemberEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<JzrcMemberEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //        // 清空会员余额记录表数据
        //    await _repository.Context.Deleteable<JzrcMemberAmountLogEntity>().Where(it => it.UserId.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 充值
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/recharge")]
    [SqlSugarUnitOfWork]
    public async Task Recharge(string id, [FromBody] JzrcMemberRechargeInput input )
    {
        var member = await _repository.Context.Queryable<JzrcMemberEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(member);
        member.Amount += input.amount;
        int ok= await _repository.Context.Updateable(member).ExecuteCommandAsync();
        if (ok<=0)
        {
            throw Oops.Oh(ErrorCode.COM1002);
        }
        JzrcMemberAmountLogEntity jzrcMemberAmountLogEntity = new JzrcMemberAmountLogEntity
        {
            Id = YitIdHelper.NextId(),
            Amount = input.amount,
            AddTime = DateTime.Now,
            Remark = $"充值余额",
            UserId = id,
            Category = (int)JzrcAmountLogEnum.Recharge
        };
        await _repository.Context.Insertable<JzrcMemberAmountLogEntity>(jzrcMemberAmountLogEntity).ExecuteCommandAsync();
    }
}