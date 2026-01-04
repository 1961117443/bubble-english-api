using Newtonsoft.Json.Linq;
using QT.Application.Entitys.Enum;
using QT.Common.Core;
using QT.Common.Enum;
using QT.DataValidation;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectLog;
using QT.Extend.Entitys.Dto.TeamLog;
using QT.Extend.Entitys.Dto.WorkOrder;
using QT.Extend.Entitys.Dto.WorkOrderLog;
using QT.Systems.Common;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Extend;

/// <summary>
    /// 工作日志
    /// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "WorkOrder", Order = 600)]
[Route("api/extend/[controller]")]
public class WorkOrderService : QTBaseService<WorkOrderEntity, WorkOrderCrInput, WorkOrderUpInput, WorkOrderInfoOutput, WorkOrderListPageInput, WorkOrderListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<WorkOrderEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;

    public WorkOrderService(ISqlSugarRepository<WorkOrderEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _billRullService = billRullService;
    }


    #region GET
    public override async Task<WorkOrderInfoOutput> GetInfo(string id)
    {
        var entity = (await _repository.Where(x => x.Id == id)
            .Select(x => new WorkOrderInfoOutput
            {
                assignUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.AssignUserId).Select(ddd => ddd.RealName),
                creatorUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreatorUserId).Select(ddd => ddd.RealName),
                tidName = SqlFunc.Subqueryable<TeamEntity>().Where(ddd => ddd.Id == x.Tid).Select(ddd => ddd.FullName),
                pidName = SqlFunc.Subqueryable<ProjectGanttEntity>().Where(ddd => ddd.Id == x.Pid).Select(ddd => ddd.FullName)
            }, true)
            .FirstAsync()) ?? throw Oops.Oh(ErrorCode.COM1005);
        return entity;
    }

    protected override async Task BeforeCreate(WorkOrderCrInput input, WorkOrderEntity entity)
    {
        switch (input.category)
        {
            case Application.Entitys.Enum.WorkOrderCategory.Personal:
                if (entity.AssignUserId.IsNullOrEmpty())
                {
                    throw Oops.Oh("请选择处理人！");
                }
                entity.Tid = "";
                entity.Pid = "";
                break;
            case Application.Entitys.Enum.WorkOrderCategory.Team:
                if (entity.Tid.IsNullOrEmpty())
                {
                    throw Oops.Oh("请选择团队！");
                }
                entity.AssignUserId = "";
                entity.Pid = "";
                break;
            case Application.Entitys.Enum.WorkOrderCategory.Project:
                if (entity.Pid.IsNullOrEmpty())
                {
                    throw Oops.Oh("请选择项目！");
                }
                entity.Tid = "";
                entity.AssignUserId = "";
                break;
            default:
                break;
        }
        entity.Create();

        if (entity.Status == 0)
        {
            // 草稿状态，不占用流水号

        }
        else
        {
            entity.No = await _billRullService.GetBillNumber("WorkOrder");
        }
    }

    protected override async Task BeforeUpdate(WorkOrderUpInput input, WorkOrderEntity entity)
    {
        if (input.status == 1 && entity.No.IsNullOrEmpty())
        {
            // 草稿提交，生成工单号
            entity.No = await _billRullService.GetBillNumber("WorkOrder");
        }
    }

    public override async Task<PageResult<WorkOrderListOutput>> GetList([FromQuery] WorkOrderListPageInput input)
    {
        var data = await _repository.Context.Queryable<WorkOrderEntity>()
            .WhereIF(input.activeTab == "inBox", x => x.AssignUserId == _userManager.UserId)
            .WhereIF(input.activeTab == "sent", x => x.CreatorUserId == _userManager.UserId && x.Status == 1)
            .WhereIF(input.activeTab == "draf", x => x.CreatorUserId == _userManager.UserId && x.Status == 0)
            .WhereIF(input.enabledMark >= 0, x => x.EnabledMark == input.enabledMark)
            .WhereIF(input.creatorUserId.IsNotEmptyOrNull(), x => x.CreatorUserId == input.creatorUserId)
            .WhereIF(input.assignUserId.IsNotEmptyOrNull(), x => x.AssignUserId == input.assignUserId)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.Content.Contains(input.keyword) || x.No.Contains(input.keyword))
            //.OrderByDescending(x => x.CreatorTime)
            .Select<WorkOrderListOutput>(x => new WorkOrderListOutput
            {
                unread = SqlFunc.Subqueryable<WorkOrderLogEntity>().Where(ddd=>ddd.Wid == x.Id && ddd.ReceiveUserId == _userManager.UserId && ddd.Read == null).Count(),
                lastReplayUserId = SqlFunc.Subqueryable<WorkOrderLogEntity>().Where(ddd => ddd.Wid == x.Id).OrderByDesc(ddd=>ddd.CreatorTime).Select(ddd=>ddd.CreatorUserId),
                lastReplayTime = SqlFunc.Subqueryable<WorkOrderLogEntity>().Where(ddd => ddd.Wid == x.Id).OrderByDesc(ddd => ddd.CreatorTime).Select(ddd => ddd.CreatorTime)
            }, true)
            .MergeTable()
            .OrderByIF(input.activeTab == "inBox",x=>x.lastReplayTime, OrderByType.Desc)
            .OrderByDescending(z=> new { z.unread,z.creatorTime})
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<WorkOrderListOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 团队工单列表（整个团队）
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Team/{tid}/List")]
    public async Task<PageResult<WorkOrderListOutput>> GetTeamList(string tid, [FromQuery] WorkOrderListPageInput input)
    {
        var data = await _repository.Context.Queryable<WorkOrderEntity>()
            .Where(x => x.Category == (int)WorkOrderCategory.Team && x.Tid == tid)
            .WhereIF(input.enabledMark >= 0, x => x.EnabledMark == input.enabledMark)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.Content.Contains(input.keyword) || x.No.Contains(input.keyword))
            .WhereIF(input.date.IsNotEmptyOrNull(), m => SqlFunc.Between(m.CreatorTime, input.date, $"{input.date} 23:59:59"))
            .OrderByDescending(x => x.CreatorTime)
            .Select<WorkOrderListOutput>(x => new WorkOrderListOutput
            {
                assignUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.AssignUserId).Select(ddd => ddd.RealName),
                creatorUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreatorUserId).Select(ddd => ddd.RealName)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<WorkOrderListOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 项目工单列表（整个项目）
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Project/{pid}/List")]
    public async Task<PageResult<WorkOrderListOutput>> GetProjectList(string pid, [FromQuery] WorkOrderListPageInput input)
    {
        var data = await _repository.Context.Queryable<WorkOrderEntity>()
            .Where(x => x.Category == (int)WorkOrderCategory.Project && x.Pid == pid)
            .WhereIF(input.enabledMark >= 0, x => x.EnabledMark == input.enabledMark)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.Content.Contains(input.keyword) || x.No.Contains(input.keyword))
            .WhereIF(input.date.IsNotEmptyOrNull(), m => SqlFunc.Between(m.CreatorTime, input.date, $"{input.date} 23:59:59"))
            .OrderByDescending(x => x.CreatorTime)
            .Select<WorkOrderListOutput>(x => new WorkOrderListOutput
            {
                assignUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.AssignUserId).Select(ddd => ddd.RealName),
                creatorUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreatorUserId).Select(ddd => ddd.RealName)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<WorkOrderListOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 按日期汇总团队工单数据
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("Actions/{teamId}/TeamSumByDay")]
    public async Task<List<TeamLogSumOutput>> TeamSumByDay(string teamId)
    {
        var list = await _repository.AsQueryable().Where(x => x.Category == (int)WorkOrderCategory.Team && x.Tid == teamId && x.DeleteMark == null)

            .Select(x => new TeamLogSumOutput
            {
                date = x.CreatorTime.Value.ToString("yyyy-MM-dd"),
                enable = x.EnabledMark == 1 ? 1 : 0
            })
            .MergeTable()
            .GroupBy(x => x.date)
            .Select(x => new TeamLogSumOutput
            {
                date = x.date,
                count = SqlFunc.AggregateCount(x.date),
                enable = SqlFunc.AggregateSum(x.enable)
            })
            .OrderByDescending(x => x.date)
            .ToListAsync();

        return list;
    }

    /// <summary>
    /// 按日期汇总项目工单数据
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("Actions/{projectId}/ProjectSumByDay")]
    public async Task<List<ProjectLogSumOutput>> ProjectSumByDay(string projectId)
    {
        var list = await _repository.AsQueryable().Where(x => x.Category == (int)WorkOrderCategory.Project && x.Pid == projectId && x.DeleteMark == null)

            .Select(x => new ProjectLogSumOutput
            {
                date = x.CreatorTime.Value.ToString("yyyy-MM-dd"),
                enable = x.EnabledMark == 1? 1:0
            })
            .MergeTable()
            .GroupBy(x => x.date)
            .Select(x => new ProjectLogSumOutput
            {
                date = x.date,
                count = SqlFunc.AggregateCount(x.date),
                enable = SqlFunc.AggregateSum(x.enable)
            })
            .OrderByDescending(x => x.date)
            .ToListAsync();

        return list;
    }

    /// <summary>
    /// 工单对当前用户拥有的权限
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Permission")]
    public async Task<WorkOrderCurrentUserAuth> GetCurrentAuth(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        WorkOrderCurrentUserAuth workOrderCurrentUserAuth = new WorkOrderCurrentUserAuth()
        {
            //回复权限：提交人或者指派人 == 当前用户
            replayAuth = entity.EnabledMark == 1 && (entity.CreatorUserId == _userManager.UserId || entity.AssignUserId == _userManager.UserId),
            //结束权限：提交人或者指派人 == 当前用户
            endAuth = entity.EnabledMark == 1 && (entity.CreatorUserId == _userManager.UserId || entity.AssignUserId == _userManager.UserId),
            //指派权限：1、个人工单：提交人 == 当前用户；2、团队工单：当前用户 = 团队管理员；3、项目工单：当前用户 = 项目管理员
            assignAuth = false
        };
        if (entity.AssignUserId.IsNullOrEmpty())
        {
            if (entity.Category == (int)WorkOrderCategory.Personal)
            {
                // 个人工单：提交人 == 当前用户
                workOrderCurrentUserAuth.assignAuth = entity.EnabledMark == 1 && (entity.CreatorUserId == _userManager.UserId);
            }
            if (entity.Category == (int)WorkOrderCategory.Team)
            {
                // 团队工单：当前用户 = 团队管理员
                if (entity.Tid.IsNotEmptyOrNull())
                {
                    var managerIds = await _repository.Context.Queryable<TeamEntity>().Where(x => x.Id == entity.Tid).Select(x => x.ManagerIds).FirstAsync();
                    if (managerIds.IsNotEmptyOrNull() && managerIds.Split(',').Contains(_userManager.UserId))
                    {
                        workOrderCurrentUserAuth.assignAuth = true;
                    }
                }
            }

            if (entity.Category == (int)WorkOrderCategory.Project)
            {
                //项目工单：当前用户 = 项目管理员
                if (entity.Pid.IsNotEmptyOrNull())
                {
                    var managerIds = await _repository.Context.Queryable<ProjectGanttEntity>().Where(x => x.Id == entity.Pid).Select(x => x.ManagerIds).FirstAsync();
                    if (managerIds.IsNotEmptyOrNull() && managerIds.Split(',').Contains(_userManager.UserId))
                    {
                        workOrderCurrentUserAuth.assignAuth = true;
                    }
                }
            }
        }
        

        return workOrderCurrentUserAuth;
    }
    #endregion


    /// <summary>
    /// 删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public override async Task Delete(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(entity);
        entity.Delete();

        await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// 关单.
    /// </summary>
    /// <returns></returns>
    [HttpPut("Actions/{id}/End")]
    public async Task End(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (entity.EnabledMark == 0)
        {
            throw Oops.Oh("工单已关闭!");
        }
        var auth = await GetCurrentAuth(id);
        if (!auth.endAuth)
        {
            throw Oops.Oh("您没有权限关闭该工单！");
        }
        //if (entity.CreatorUserId != _userManager.UserId)
        //{
        //    throw Oops.Oh("您没有权限关闭该工单！");
        //}

        _repository.Context.Tracking(entity);
        entity.EnabledMark = 0;

        var flag = await _repository.UpdateAsync(entity);

        if (flag > 0)
        {
            // 写入日志
            WorkOrderLogEntity workOrderLogEntity = new WorkOrderLogEntity
            {
                Wid = entity.Id,
                Content = $"关闭工单",
            };
            await _repository.Context.Insertable<WorkOrderLogEntity>(workOrderLogEntity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 指派.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Assign/{uid}")]
    [SqlSugarUnitOfWork]
    public async Task Assign(string id, string uid)
    {
        // 判断用户是否存在
        var user = await _repository.Context.Queryable<UserEntity>().Where(x => x.Id == uid).FirstAsync() ?? throw Oops.Oh("用户不存在！");
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.EnabledMark == 0)
        {
            throw Oops.Oh("工单已关闭!");
        }

        _repository.Context.Tracking(entity);
        entity.AssignUserId = uid;

        var flag = await _repository.AutoUpdateAsync(entity);
        if (flag > 0)
        {
            // 写入日志
            WorkOrderLogEntity workOrderLogEntity = new WorkOrderLogEntity
            {
                Wid = entity.Id,
                Content = $"工单指派给 {user.RealName}",
                ReceiveUserId = user.Id
            };
            await _repository.Context.Insertable<WorkOrderLogEntity>(workOrderLogEntity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        }

    }


    /// <summary>
    /// 设置已读邮件.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("Actions/{id}/Read")]
    public async Task<int> ReceiveRead(string id)
    {
        return await _repository.Context.Updateable<WorkOrderLogEntity>().SetColumns(it => new WorkOrderLogEntity()
        {
            Read = 1,
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Wid.Equals(id) && it.ReceiveUserId == _userManager.UserId && it.Read == null).ExecuteCommandAsync();
    }

    /// <summary>
    /// 发送短信通知
    /// </summary>
    /// <param name="id">工单id</param>
    /// <param name="input">{phone="1231231231"}</param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Send")]
    public async Task<string> SendMessage(string id, [FromBody] WorkOrderSendInput input)
    {
        if (!input.userList.IsAny())
        {
            throw Oops.Oh("请选择用户");
        }
        var entity = await _repository.Context.Queryable<WorkOrderEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (input == null)
        {
            throw Oops.Oh("缺少参数[phone]");
        }

        // 获取用户手机号码
        var phones = await _repository.Context.Queryable<UserEntity>().Where(x => input.userList.Contains(x.Id)).Select(x => new UserEntity
        {
            Id = x.Id,
            RealName = x.RealName,
            MobilePhone = x.MobilePhone
        }).ToListAsync();

        if (!phones.IsAny())
        {
            throw Oops.Oh("用户未绑定手机号码！");
        }

        //var phones = input.Value<string>("phone");
        //if (string.IsNullOrEmpty(phones))
        //{
        //    throw Oops.Oh("手机号码[phone]不能为空！");
        //}
        List<string> phoneList = new List<string>();
        foreach (var p in phones)
        {
            if (p.MobilePhone.IsNullOrEmpty())
            {
                throw Oops.Oh($"【{p.RealName}】未绑定手机号码！");
            }
            if (!p.MobilePhone.TryValidate(ValidationTypes.PhoneNumber).IsValid)
            {
                throw Oops.Oh($"【{p.RealName}】手机号码格式不正确！");
            }
            phoneList.Add(p.MobilePhone);
        }
        return await SmsHelper.SendByCode("workorder.sms", string.Join(",", phoneList), new { code = entity.No });
    }
}
