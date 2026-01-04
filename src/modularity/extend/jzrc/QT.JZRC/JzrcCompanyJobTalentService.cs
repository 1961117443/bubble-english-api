using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QT.JZRC.Entitys.Dto.JzrcCompanyJobTalent;
using Mapster;
using SqlSugar;
using QT.Common.Extension;
using QT.JZRC.Interfaces;
using QT.Common.Contracts;
using QT.DependencyInjection;

namespace QT.JZRC;

/// <summary>
/// 业务实现：企业人才签约.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcCompanyJobTalent", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCompanyJobTalentService :IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<JzrcCompanyJobTalentEntity> _repository;
    private readonly IJzrcTransactionRuleService _transactionRuleService;
    private readonly IBillRule _billRule;
    private readonly IJzrcContractService _contractService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="transactionRuleService"></param>
    /// <param name="billRule"></param>
    public JzrcCompanyJobTalentService(ISqlSugarRepository<JzrcCompanyJobTalentEntity> repository,
        IJzrcTransactionRuleService transactionRuleService, IBillRule billRule,
        IJzrcContractService contractService)
    {
        _repository = repository;
        _transactionRuleService = transactionRuleService;
        _billRule = billRule;
        _contractService = contractService;
    }

    /// <summary>
    /// 新建企业人才签约关系.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] JzrcCompanyJobTalentCrInput input)
    {
        if (input.amount <=0)
        {
            throw Oops.Oh("金额不能小于0！");
        }
        // 1、判断jobid 和人才是否已经存在，如果存在则删除，
        // 2、计算佣金，插入新的关系
        // 3、如果是签约，创建订单

        #region 1、同一个岗位，同一个人，删除原来的签约记录
        var list = await _repository.Where(it => it.JobId == input.jobId && it.TalentId == input.talentId).ToListAsync();
        // 判断人才是否已签约
        if (list.Any(it => it.Status == 1))
        {
            throw Oops.Oh("人才已签约！");
        }

        // 企业签约人才，判断是否签满
        if (input.category == Entitys.Dto.AppService.AppLoginUserRole.Company)
        {
            var number = await _repository.Context.Queryable<JzrcCompanyJobEntity>().Where(it => it.Id == input.jobId).Select(it => it.Number).FirstAsync() ?? 0;
            var count = await _repository.Where(it => it.JobId == input.jobId && it.Status == 1).CountAsync();
            if (count >= number)
            {
                throw Oops.Oh($"该岗位已签约{count}人！");
            }
        }

        if (list.IsAny())
        {
            _repository.Context.Tracking(list);
            list.ForEach(x => x.Delete());
            await _repository.Context.Updateable(list).ExecuteCommandAsync();
        }
        #endregion

        #region 2、插入新的签约关系
        var rule = await _transactionRuleService.GetDefaultRule();
        var entity = input.Adapt<JzrcCompanyJobTalentEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        // 当前的佣金比例
        entity.CommissionRate = entity.Category == Entitys.Dto.AppService.AppLoginUserRole.Talent ? rule.talentCommission : rule.enterprisesCommission;
        // 计算实际的佣金
        entity.Commission = Math.Round(entity.Amount * entity.CommissionRate * 0.01m, 2);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
        #endregion

        #region 3、如果是签约状态，生成订单 、生成合同
        if (entity.Status == 1)
        {
            #region 创建订单
            var order = new JzrcOrderEntity
            {
                Id = entity.Id,
                Amount = entity.Commission,
                CompanyId = entity.CompanyId,
                TalentId = entity.TalentId,
                OrderNo = await _billRule.GetBillNumber("dingdanbianhao"),
                ProcessingStatus = 0,
                SettlementStatus = 0,
                ManagerId = "",
                CompanyShare = 0,
                PlatformShare = 0,
                TalentShare = 0,
                //ProcessingTime = DateTime.Now,
                //     SettlementTime = DateTime.Now,
            };
            if (entity.Category == Entitys.Dto.AppService.AppLoginUserRole.Talent)
            {
                order.ManagerId = await _repository.Context.Queryable<JzrcTalentEntity>().Where(it => it.Id == order.TalentId).Select(it => it.ManagerId).FirstAsync();
            }
            else
            {
                order.ManagerId = await _repository.Context.Queryable<JzrcCompanyEntity>().Where(it => it.Id == order.CompanyId).Select(it => it.ManagerId).FirstAsync();
            }

            // 计算分成
            order.PlatformShare = order.Amount;

            isOk = await _repository.Context.Insertable(order).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
            #endregion

            #region 生成合同
            JzrcContractEntity jzrcContractEntity = new JzrcContractEntity
            {
                Id = entity.Id,
                Amount = entity.Amount,
                CompanyId = entity.CompanyId,
                TalentId = entity.TalentId,
                CertificateId = "",
                JobId = entity.JobId,
                No = await _billRule.GetBillNumber("hetongbianhao")
            };
            await _contractService.SetProperty(jzrcContractEntity);
            isOk = await _repository.Context.Insertable(jzrcContractEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000); 
            #endregion
        }
        #endregion
    }
}
