using System.Linq.Expressions;
using QT.Common.Contracts;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.LinqBuilder;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.VisualDev.Entitys;
using QT.WorkFlow.Entitys.Dto.FlowBefore;
using QT.WorkFlow.Entitys.Dto.FlowLaunch;
using QT.WorkFlow.Entitys.Dto.FlowMonitor;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Entitys.Model;
using QT.WorkFlow.Interfaces.Repository;
using SqlSugar;

namespace QT.WorkFlow.Repository;

/// <summary>
/// 流程任务数据处理.
/// </summary>
public class FlowTaskRepository : IFlowTaskRepository, ITransient
{
    private readonly ISqlSugarRepository<FlowTaskEntity> _flowTaskRepository;
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="flowTaskRepository"></param>
    /// <param name="userManager"></param>
    /// <param name="context"></param>
    public FlowTaskRepository(
        ISqlSugarRepository<FlowTaskEntity> flowTaskRepository,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _flowTaskRepository = flowTaskRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region 流程列表

    /// <summary>
    /// 列表（流程监控）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetMonitorList(FlowMonitorListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowMonitorListOutput>();
        if (!input.startTime.IsNullOrEmpty() && !input.endTime.IsNullOrEmpty())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (!input.creatorUserId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.creatorUserId == input.creatorUserId);
        if (!input.flowCategory.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (!input.creatorUserId.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.creatorUserId.Contains(input.creatorUserId));
        if (!input.flowId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (!input.status.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.status == input.status);
        if (!input.keyword.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));
        var list = await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowEngineEntity, UserEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.FlowId == b.Id, JoinType.Left, a.CreatorUserId == c.Id)).Where(a => a.Status > 0 && a.DeleteMark == null).Select((a, b, c) => new FlowMonitorListOutput()
        {
            completion = a.Completion,
            creatorTime = a.CreatorTime,
            creatorUserId = a.CreatorUserId,
            description = a.Description,
            enCode = a.EnCode,
            flowCategory = a.FlowCategory,
            flowCode = a.FlowCode,
            flowId = a.FlowId,
            flowName = b.FullName,
            formUrgent = a.FlowUrgent,
            formData = b.FormTemplateJson,
            formType = b.FormType,
            fullName = a.FullName,
            id = a.Id,
            processId = a.ProcessId,
            startTime = a.StartTime,
            thisStep = a.ThisStep,
            userName = SqlFunc.MergeString(c.RealName, "/", c.Account),
            status = a.Status,
            sortCode = a.SortCode
        }).MergeTable().Where(whereLambda).OrderBy(a => a.sortCode)
           .OrderBy(a => a.creatorTime, OrderByType.Desc)
           .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowMonitorListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表（我发起的）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetLaunchList(FlowLaunchListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowLaunchListOutput>();
        whereLambda = whereLambda.And(x => x.creatorUserId == _userManager.UserId);
        if (!input.startTime.IsNullOrEmpty() && !input.endTime.IsNullOrEmpty())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (!input.flowCategory.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (!input.flowId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (!input.status.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.status == input.status);
        if (!input.keyword.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));
        var list = await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowEngineEntity, UserEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.FlowId == b.Id, JoinType.Left, a.CreatorUserId == c.Id)).Where((a, b) => a.DeleteMark == null).Select((a, b, c) => new FlowLaunchListOutput()
        {
            completion = a.Completion,
            creatorTime = a.CreatorTime,
            creatorUserId = a.CreatorUserId,
            endTime = a.EndTime,
            description = a.Description,
            enCode = a.EnCode,
            flowCategory = a.FlowCategory,
            flowCode = a.FlowCode,
            flowId = a.FlowId,
            flowName = b.FullName,
            formData = b.FormTemplateJson,
            formType = b.FormType,
            fullName = a.FullName,
            id = a.Id,
            startTime = a.StartTime,
            thisStep = a.ThisStep,
            status = a.Status
        }).MergeTable().Where(whereLambda).OrderBy(a => a.status).OrderBy(a => a.startTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowLaunchListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表（待我审批）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetWaitList(FlowBeforeListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowBeforeListOutput>();
        if (input.startTime.IsNotEmptyOrNull() && input.endTime.IsNotEmptyOrNull())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (input.flowCategory.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (input.flowId.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (input.keyword.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));
        if (input.creatorUserId.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.creatorUserId.Contains(input.creatorUserId));

        // 经办审核
        var list1 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, UserEntity, FlowEngineEntity>(
            (a, b, c, d) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.CreatorUserId == c.Id, JoinType.Left, a.FlowId == d.Id))
            .Where((a, b) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
            && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null) && b.HandleId == _userManager.UserId)
            .Select((a, b, c, d) => new FlowBeforeListOutput()
            {
                enCode = a.EnCode,
                creatorUserId = a.CreatorUserId,
                creatorTime = SqlFunc.IsNullOrEmpty(SqlFunc.ToString(b.Description)) ? b.CreatorTime : SqlFunc.ToDate(SqlFunc.ToString(b.Description)),
                thisStep = a.ThisStep,
                thisStepId = b.TaskNodeId,
                flowCategory = a.FlowCategory,
                fullName = a.FullName,
                flowName = a.FlowName,
                status = a.Status,
                id = b.Id,
                userName = SqlFunc.MergeString(c.RealName, "/", c.Account),
                description = SqlFunc.ToString(a.Description),
                flowCode = a.FlowCode,
                flowId = a.FlowId,
                processId = a.ProcessId,
                formType = d.FormType,
                flowUrgent = a.FlowUrgent,
                startTime = a.CreatorTime,
                completion = a.Completion,
                nodeName = b.NodeName
            });

        // 委托审核
        var list2 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, FlowDelegateEntity, UserEntity, UserEntity, FlowEngineEntity>(
            (a, b, c, d, e, f) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.FlowId == c.FlowId
        && c.EndTime > DateTime.Now, JoinType.Left, c.CreatorUserId == d.Id, JoinType.Left, a.CreatorUserId == e.Id, JoinType.Left, a.FlowId == f.Id))
            .Where((a, b, c) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
            && b.HandleId == c.CreatorUserId && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null)
            && c.ToUserId == _userManager.UserId && c.DeleteMark == null && c.EnabledMark == 1 && c.EndTime > DateTime.Now && c.StartTime < DateTime.Now)
            .Select((a, b, c, d, e, f) => new FlowBeforeListOutput()
            {
                enCode = a.EnCode,
                creatorUserId = a.CreatorUserId,
                creatorTime = SqlFunc.IsNullOrEmpty(SqlFunc.ToString(b.Description)) ? b.CreatorTime : SqlFunc.ToDate(SqlFunc.ToString(b.Description)),
                thisStep = a.ThisStep,
                thisStepId = b.TaskNodeId,
                flowCategory = a.FlowCategory,
                fullName = SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                flowName = a.FlowName,
                status = a.Status,
                id = b.Id,
                userName = SqlFunc.MergeString(e.RealName, "/", e.Account),
                description = SqlFunc.ToString(a.Description),
                flowCode = a.FlowCode,
                flowId = a.FlowId,
                processId = a.ProcessId,
                formType = f.FormType,
                flowUrgent = a.FlowUrgent,
                startTime = a.CreatorTime,
                completion = a.Completion,
                nodeName = b.NodeName
            });
        var output = await _flowTaskRepository.Context.UnionAll(list1, list2).Where(whereLambda).MergeTable().OrderBy(x => x.creatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(output);
    }

    /// <summary>
    /// 列表（批量审批）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetBatchWaitList(FlowBeforeListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowBeforeListOutput>();
        if (input.startTime.IsNotEmptyOrNull() && input.endTime.IsNotEmptyOrNull())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (!input.flowCategory.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (!input.flowId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (!input.keyword.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));
        if (!input.creatorUserId.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.creatorUserId.Contains(input.creatorUserId));
        if (!input.nodeCode.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.nodeCode.Contains(input.nodeCode));
        //经办审核
        var list1 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, UserEntity, FlowEngineEntity, FlowTaskNodeEntity>((a, b, c, d, e) =>
         new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.CreatorUserId == c.Id, JoinType.Left, a.FlowId == d.Id, JoinType.Left, b.TaskNodeId == e.Id))
            .Where((a, b, c) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
            && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null)
            && b.HandleId == _userManager.UserId && a.IsBatch == 1)
            .Select((a, b, c, d, e) => new FlowBeforeListOutput()
            {
                enCode = a.EnCode,
                creatorUserId = a.CreatorUserId,
                creatorTime = SqlFunc.IsNullOrEmpty(SqlFunc.ToString(b.Description)) ? b.CreatorTime : SqlFunc.ToDate(SqlFunc.ToString(b.Description)),
                thisStep = a.ThisStep,
                thisStepId = b.TaskNodeId,
                flowCategory = a.FlowCategory,
                fullName = a.FullName,
                flowName = a.FlowName,
                status = a.Status,
                id = b.Id,
                userName = SqlFunc.MergeString(c.RealName, "/", c.Account),
                description = SqlFunc.ToString(a.Description),
                flowCode = a.FlowCode,
                flowId = a.FlowId,
                processId = a.ProcessId,
                formType = d.FormType,
                flowUrgent = a.FlowUrgent,
                startTime = a.CreatorTime,
                completion = a.Completion,
                nodeName = b.NodeName,
                approversProperties = e.NodePropertyJson,
                flowVersion = SqlFunc.MergeString("v", a.FlowVersion),
                nodeCode = b.NodeCode
            });
        //委托审核
        var list2 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, FlowDelegateEntity, UserEntity, UserEntity, FlowEngineEntity, FlowTaskNodeEntity>((a, b, c, d, e, f, g) =>
         new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.FlowId == c.FlowId
         && c.EndTime > DateTime.Now, JoinType.Left, c.CreatorUserId == d.Id, JoinType.Left,
         a.CreatorUserId == e.Id, JoinType.Left, a.FlowId == f.Id, JoinType.Left, b.TaskNodeId == g.Id))
            .Where((a, b, c) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0" && a.IsBatch == 1
            && b.HandleId == c.CreatorUserId && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null)
            && c.ToUserId == _userManager.UserId && c.DeleteMark == null && c.EnabledMark == 1 && c.EndTime > DateTime.Now && c.StartTime < DateTime.Now).Select((a, b, c, d, e, f, g) => new FlowBeforeListOutput()
            {
                enCode = a.EnCode,
                creatorUserId = a.CreatorUserId,
                creatorTime = SqlFunc.IsNullOrEmpty(SqlFunc.ToString(b.Description)) ? b.CreatorTime : SqlFunc.ToDate(SqlFunc.ToString(b.Description)),
                thisStep = a.ThisStep,
                thisStepId = b.TaskNodeId,
                flowCategory = a.FlowCategory,
                fullName = SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                flowName = a.FlowName,
                status = a.Status,
                id = b.Id,
                userName = SqlFunc.MergeString(e.RealName, "/", e.Account),
                description = SqlFunc.ToString(a.Description),
                flowCode = a.FlowCode,
                flowId = a.FlowId,
                processId = a.ProcessId,
                formType = f.FormType,
                flowUrgent = a.FlowUrgent,
                startTime = a.CreatorTime,
                completion = a.Completion,
                nodeName = b.NodeName,
                approversProperties = g.NodePropertyJson,
                flowVersion = SqlFunc.MergeString("v", a.FlowVersion),
                nodeCode = b.NodeCode
            });
        var output = await _flowTaskRepository.Context.UnionAll(list1, list2).Where(whereLambda).MergeTable().OrderBy(x => x.creatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(output);
    }

    /// <summary>
    /// 列表（我已审批）.
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    public async Task<dynamic> GetTrialList(FlowBeforeListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowBeforeListOutput>();
        if (!input.startTime.IsNullOrEmpty() && !input.endTime.IsNullOrEmpty())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (!input.flowCategory.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (!input.flowId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (!input.creatorUserId.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.creatorUserId.Contains(input.creatorUserId));
        if (!input.keyword.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));

        var list = await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorRecordEntity, FlowTaskOperatorEntity,
            UserEntity, UserEntity, FlowEngineEntity>((a, b, c, d, e, f) =>
            new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, b.TaskOperatorId == c.Id,
            JoinType.Left, c.HandleId == d.Id, JoinType.Left, a.CreatorUserId == e.Id, JoinType.Left, a.FlowId == f.Id))
            .Where((a, b) => b.HandleStatus < 2 && b.TaskOperatorId != null && b.HandleId == _userManager.UserId)
            .Select((a, b, c, d, e, f) => new FlowBeforeListOutput()
            {
                enCode = a.EnCode,
                creatorUserId = a.CreatorUserId,
                creatorTime = b.HandleTime,
                thisStep = a.ThisStep,
                thisStepId = b.TaskNodeId,
                flowCategory = a.FlowCategory,
                fullName = b.HandleId == c.HandleId || c.Id == null ? a.FullName : SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                flowName = a.FlowName,
                status = b.HandleStatus,
                id = b.Id,
                userName = SqlFunc.MergeString(e.RealName, "/", e.Account),
                description = a.Description,
                flowCode = a.FlowCode,
                flowId = a.FlowId,
                processId = a.ProcessId,
                formType = f.FormType,
                flowUrgent = a.FlowUrgent,
                startTime = a.CreatorTime,
            }).MergeTable().Where(whereLambda).OrderBy(a => a.creatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表（抄送我的）.
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    public async Task<dynamic> GetCirculateList(FlowBeforeListQuery input)
    {
        var whereLambda = LinqExpression.And<FlowBeforeListOutput>();
        if (!input.startTime.IsNullOrEmpty() && !input.endTime.IsNullOrEmpty())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.creatorTime, startTime, endTime));
        }
        if (!input.flowCategory.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowCategory == input.flowCategory);
        if (!input.flowId.IsNullOrEmpty())
            whereLambda = whereLambda.And(x => x.flowId == input.flowId);
        if (!input.creatorUserId.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.creatorUserId.Contains(input.creatorUserId));
        if (!input.keyword.IsNullOrEmpty())
            whereLambda = whereLambda.And(m => m.enCode.Contains(input.keyword) || m.fullName.Contains(input.keyword));
        var list = await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskCirculateEntity, UserEntity, FlowEngineEntity>((a, b, c, d) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.CreatorUserId == c.Id, JoinType.Left, a.FlowId == d.Id)).Where((a, b) => b.ObjectId == _userManager.UserId).Select((a, b, c, d) => new FlowBeforeListOutput()
        {
            enCode = a.EnCode,
            creatorUserId = a.CreatorUserId,
            creatorTime = b.CreatorTime,
            thisStep = a.ThisStep,
            thisStepId = b.TaskNodeId,
            flowCategory = a.FlowCategory,
            fullName = a.FullName,
            flowName = a.FlowName,
            status = a.Status,
            id = b.Id,
            userName = SqlFunc.MergeString(c.RealName, "/", c.Account),
            description = a.Description,
            flowCode = a.FlowCode,
            flowId = a.FlowId,
            processId = a.ProcessId,
            formType = d.FormType,
            flowUrgent = a.FlowUrgent,
            startTime = a.CreatorTime,
        }).MergeTable().Where(whereLambda).OrderBy(x => x.creatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 批量流程列表.
    /// </summary>
    /// <returns></returns>
    public async Task<dynamic> BatchFlowSelector()
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity>(
            (a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId))
            .Where((a, b) => a.DeleteMark == null && a.IsBatch == 1 && b.HandleId == _userManager.UserId && b.Completion == 0 && a.Status == 1 && b.State == "0")
            .GroupBy((a, b) => new { a.FlowId })
            .Select((a, b) => new
            {
                id = a.FlowId,
                fullName = SqlFunc.MergeString(SqlFunc.Subqueryable<FlowEngineEntity>().Where(x => x.Id == a.FlowId).Select(x => x.FullName), "(", SqlFunc.AggregateCount(b.Id).ToString(), ")"),
                count = SqlFunc.AggregateCount(b.Id)
            }).MergeTable().OrderBy(x => x.count, OrderByType.Desc).ToListAsync();
    }

    /// <summary>
    /// 根据分类获取审批意见.
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="category"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<List<FlowBeforeRecordListModel>> GetRecordListByCategory(string taskId, string category, string type = "0")
    {
        var recordList = new List<FlowBeforeRecordListModel>();
        switch (category)
        {
            case "1":
                return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity, UserEntity, OrganizeEntity>((a, b, c) =>
         new JoinQueryInfos(JoinType.Left, a.HandleId == b.Id, JoinType.Left, SqlFunc.ToString(b.OrganizeId) == c.Id)).Where(a => a.TaskId == taskId)
         .WhereIF(type == "1", (a) => a.HandleStatus == 0 || a.HandleStatus == 1).Select((a, b, c) =>
                       new FlowBeforeRecordListModel()
                       {
                           id = a.Id,
                           handleId = a.Id,
                           handleOpinion = a.HandleOpinion,
                           handleStatus = a.HandleStatus,
                           handleTime = a.HandleTime,
                           userName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                           category = c.Id,
                           categoryName = c.FullName,
                           operatorId = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.OperatorId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                       }).ToListAsync();
            case "2":
                return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity, UserEntity, UserRelationEntity, RoleEntity>((a, b, c, d) =>
          new JoinQueryInfos(JoinType.Left, a.HandleId == b.Id, JoinType.Left, b.Id == c.UserId, JoinType.Left, c.ObjectId == d.Id))
            .Where((a, b, c) => a.TaskId == taskId && c.ObjectType == "Role").WhereIF(type == "1", (a) => a.HandleStatus == 0 || a.HandleStatus == 1)
            .Select((a, b, c, d) => new FlowBeforeRecordListModel()
            {
                id = a.Id,
                handleId = a.Id,
                handleOpinion = a.HandleOpinion,
                handleStatus = a.HandleStatus,
                handleTime = a.HandleTime,
                userName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                category = d.Id,
                categoryName = d.FullName,
                operatorId = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.OperatorId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
            }).ToListAsync();
            case "3":
                return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity, UserEntity, UserRelationEntity, PositionEntity>((a, b, c, d) =>
          new JoinQueryInfos(JoinType.Left, a.HandleId == b.Id, JoinType.Left, b.Id == c.UserId, JoinType.Left, c.ObjectId == d.Id))
             .Where((a, b, c) => a.TaskId == taskId && c.ObjectType == "Position").WhereIF(type == "1", (a) => a.HandleStatus == 0 || a.HandleStatus == 1)
             .Select((a, b, c, d) => new FlowBeforeRecordListModel()
             {
                 id = a.Id,
                 handleId = a.Id,
                 handleOpinion = a.HandleOpinion,
                 handleStatus = a.HandleStatus,
                 handleTime = a.HandleTime,
                 userName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                 category = d.Id,
                 categoryName = d.FullName,
                 operatorId = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.OperatorId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
             }).ToListAsync();
        }

        return recordList;
    }
    #endregion

    #region 其他模块流程列表

    /// <summary>
    /// 门户列表（待我审批）.
    /// </summary>
    /// <returns></returns>
    public async Task<List<FlowTaskEntity>> GetWaitList()
    {
        // 经办审核
        var list1 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId))
            .Where((a, b) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
            && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null) && b.HandleId == _userManager.UserId)
            .Select((a, b) => new FlowTaskEntity()
            {
                Id = b.Id,
                ParentId = a.ParentId,
                ProcessId = a.ProcessId,
                EnCode = a.EnCode,
                FullName = a.FullName,
                FlowUrgent = a.FlowUrgent,
                FlowId = a.FlowId,
                FlowCode = a.FlowCode,
                FlowName = a.FlowName,
                FlowCategory = a.FlowCategory,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ThisStep = a.ThisStep,
                ThisStepId = b.TaskNodeId,
                Status = a.Status,
                Completion = a.Completion,
                CreatorTime = b.CreatorTime,
                CreatorUserId = a.CreatorUserId,
                LastModifyTime = a.LastModifyTime,
                LastModifyUserId = a.LastModifyUserId
            });

        // 委托审核
        var list2 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, FlowDelegateEntity, UserEntity>((a, b, c, d) =>
            new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.FlowId == c.FlowId &&
            c.EndTime > DateTime.Now, JoinType.Left, c.CreatorUserId == d.Id)).Where((a, b, c) =>
            a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
            && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null)
            && b.HandleId == c.CreatorUserId && c.ToUserId == _userManager.UserId && c.DeleteMark == null && c.EndTime > DateTime.Now && c.StartTime < DateTime.Now)
            .Select((a, b, c, d) => new FlowTaskEntity()
            {
                Id = b.Id,
                ParentId = a.ParentId,
                ProcessId = a.ProcessId,
                EnCode = a.EnCode,
                FullName = SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                FlowUrgent = a.FlowUrgent,
                FlowId = a.FlowId,
                FlowName = a.FlowName,
                FlowCode = a.FlowCode,
                FlowCategory = a.FlowCategory,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ThisStep = a.ThisStep,
                ThisStepId = b.TaskNodeId,
                Status = a.Status,
                Completion = a.Completion,
                CreatorTime = b.CreatorTime,
                CreatorUserId = a.CreatorUserId,
                LastModifyTime = a.LastModifyTime,
                LastModifyUserId = a.LastModifyUserId
            });
        return await _flowTaskRepository.Context.UnionAll(list1, list2).MergeTable().ClearFilter<IDeleteTime>().ToListAsync();
    }

    /// <summary>
    /// 门户列表（待我审批）.
    /// </summary>
    /// <returns></returns>
    public async Task<dynamic> GetPortalWaitList()
    {
        // 经办审核
        var list1 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, FlowEngineEntity>((a, b, c) =>
            new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.FlowId == c.Id)).Where((a, b) => a.Status == 1 && a.DeleteMark == null
            && b.Completion == 0 && b.State == "0" && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null) && b.HandleId == _userManager.UserId)
            .Select((a, b, c) => new PortalWaitListModel()
            {
                id = b.Id,
                fullName = a.FullName,
                enCode = c.EnCode,
                flowId = c.Id,
                formType = c.FormType,
                status = a.Status,
                processId = a.Id,
                taskNodeId = b.TaskNodeId,
                taskOperatorId = b.Id,
                creatorTime = b.CreatorTime,
                type = 2
            });

        // 委托审核
        var list2 = _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorEntity, FlowDelegateEntity, UserEntity, FlowEngineEntity>((a, b, c, d, e) =>
            new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, a.FlowId == c.FlowId &&
            c.EndTime > DateTime.Now, JoinType.Left, c.CreatorUserId == d.Id, JoinType.Left, a.FlowId == e.Id))
            .Where((a, b, c) => a.Status == 1 && a.DeleteMark == null && b.Completion == 0 && b.State == "0"
             && (SqlFunc.ToDate(SqlFunc.ToString(b.Description)) < DateTime.Now || b.Description == null)
             && b.HandleId == c.CreatorUserId && c.ToUserId == _userManager.UserId && c.DeleteMark == null && c.EndTime > DateTime.Now && c.StartTime < DateTime.Now).Select((a, b, c, d, e) => new PortalWaitListModel()
             {
                 id = b.Id,
                 fullName = SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                 enCode = e.EnCode,
                 flowId = e.Id,
                 formType = e.FormType,
                 status = a.Status,
                 processId = a.Id,
                 taskNodeId = b.TaskNodeId,
                 taskOperatorId = b.Id,
                 creatorTime = b.CreatorTime,
                 type = 2
             });
        return await _flowTaskRepository.Context.UnionAll(list1, list2).MergeTable().ToListAsync();
    }

    /// <summary>
    /// 列表（我已审批）.
    /// </summary>
    /// <returns></returns>
    public async Task<List<FlowTaskEntity>> GetTrialList()
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskEntity, FlowTaskOperatorRecordEntity, FlowTaskOperatorEntity, UserEntity>(
            (a, b, c, d) => new JoinQueryInfos(JoinType.Left, a.Id == b.TaskId, JoinType.Left, b.TaskOperatorId == c.Id, JoinType.Left, c.HandleId == d.Id))
            .Where((a, b, c) => b.HandleStatus < 2 && b.TaskOperatorId != null && b.HandleId == _userManager.UserId)
            .Select((a, b, c, d) => new FlowTaskEntity()
            {
                Id = b.Id,
                ParentId = a.ParentId,
                ProcessId = a.ProcessId,
                EnCode = a.EnCode,
                FullName = b.HandleId == c.HandleId || c.Id == null ? a.FullName : SqlFunc.MergeString(a.FullName, "(", d.RealName, "的委托)"),
                FlowUrgent = a.FlowUrgent,
                FlowId = a.FlowId,
                FlowCode = a.FlowCode,
                FlowName = a.FlowName,
                FlowCategory = a.FlowCategory,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ThisStep = b.NodeName,
                ThisStepId = c.TaskNodeId,
                Status = b.HandleStatus,
                Completion = a.Completion,
                CreatorTime = b.HandleTime,
                CreatorUserId = a.CreatorUserId,
                LastModifyTime = a.LastModifyTime,
                LastModifyUserId = a.LastModifyUserId,
                DeleteTime = a.DeleteTime
            }).ToListAsync();
    }
    #endregion

    #region other

    /// <summary>
    /// 流程信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FlowEngineEntity> GetEngineInfo(string id)
    {
        return await _flowTaskRepository.Context.Queryable<FlowEngineEntity>().FirstAsync(x => x.DeleteMark == null && x.Id == id);
    }

    /// <summary>
    /// 任务信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public FlowEngineEntity GetEngineFirstOrDefault(string id)
    {
        return _flowTaskRepository.Context.Queryable<FlowEngineEntity>().First(x => x.DeleteMark == null && x.Id == id);
    }

    /// <summary>
    /// 获取指定用户被委托人.
    /// </summary>
    /// <param name="userIds">指定用户.</param>
    /// <param name="flowId">流程id.</param>
    /// <returns></returns>
    public async Task<List<string>> GetDelegateUserIds(List<string> userIds, string flowId)
    {
        return await _flowTaskRepository.Context.Queryable<FlowDelegateEntity>().Where(a => userIds.Contains(a.CreatorUserId) && a.FlowId == flowId && a.EndTime > DateTime.Now && a.DeleteMark == null).Select(a => a.ToUserId).ToListAsync();
    }

    /// <summary>
    /// 获取指定用户被委托人.
    /// </summary>
    /// <param name="userId">指定用户.</param>
    /// <param name="flowId">流程id.</param>
    /// <returns></returns>
    public List<string> GetToUserId(string userId, string flowId)
    {
        return _flowTaskRepository.Context.Queryable<FlowDelegateEntity>().Where(a => a.CreatorUserId == userId && a.FlowId == flowId && a.EndTime > DateTime.Now && a.DeleteMark == null).Select(a => a.ToUserId).ToList();
    }

    /// <summary>
    /// 获取功能开发.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <returns></returns>
    public async Task<VisualDevEntity> GetVisualDevInfo(string flowId)
    {
        return await _flowTaskRepository.Context.Queryable<VisualDevEntity>().FirstAsync(a => a.Id == flowId && a.DeleteMark == null);
    }

    /// <summary>
    /// 获取数据连接.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns></returns>
    public async Task<DbLinkEntity> GetLinkInfo(string id)
    {
        return await _flowTaskRepository.Context.Queryable<DbLinkEntity>().FirstAsync(a => a.Id == id && a.DeleteMark == null);
    }
    #endregion

    #region FlowTask

    /// <summary>
    /// 任务列表.
    /// </summary>
    /// <returns></returns>
    public async Task<List<FlowTaskEntity>> GetTaskList()
    {
        return await _flowTaskRepository.Entities.ToListAsync();
    }

    /// <summary>
    /// 任务列表.
    /// </summary>
    /// <param name="flowId">引擎id.</param>
    /// <returns></returns>
    public async Task<List<FlowTaskEntity>> GetTaskList(string flowId)
    {
        return await _flowTaskRepository.ToListAsync(x => x.DeleteMark == null && x.FlowId == flowId);
    }

    /// <summary>
    /// 任务列表.
    /// </summary>
    /// <param name="expression">条件.</param>
    /// <returns></returns>
    public async Task<List<FlowTaskEntity>> GetTaskList(Expression<Func<FlowTaskEntity, bool>> expression)
    {
        return await _flowTaskRepository.ToListAsync(expression);
    }

    /// <summary>
    /// 任务信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FlowTaskEntity> GetTaskInfo(string id)
    {
        return await _flowTaskRepository.FirstOrDefaultAsync(x => x.DeleteMark == null && x.Id == id);
    }

    /// <summary>
    /// 任务信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public FlowTaskEntity GetTaskFirstOrDefault(string id)
    {
        return _flowTaskRepository.FirstOrDefault(x => x.DeleteMark == null && x.Id == id);
    }

    /// <summary>
    /// 是否存在任务.
    /// </summary>
    /// <param name="expression">条件.</param>
    /// <returns></returns>
    public async Task<bool> AnyFlowTask(Expression<Func<FlowTaskEntity, bool>> expression)
    {
        return await _flowTaskRepository.AnyAsync(expression);
    }

    /// <summary>
    /// 任务删除.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<int> DeleteTask(FlowTaskEntity entity)
    {
        entity.DeleteTime = DateTime.Now;
        entity.DeleteMark = 1;
        entity.DeleteUserId = _userManager.UserId;
        await _flowTaskRepository.Context.Deleteable<FlowTaskNodeEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        await _flowTaskRepository.Context.Deleteable<FlowTaskOperatorEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        await _flowTaskRepository.Context.Deleteable<FlowTaskOperatorRecordEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        await _flowTaskRepository.Context.Deleteable<FlowTaskCirculateEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        await _flowTaskRepository.Context.Deleteable<FlowCandidatesEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        await _flowTaskRepository.Context.Deleteable<FlowCommentEntity>(x => entity.Id == x.TaskId).ExecuteCommandAsync();
        return await _flowTaskRepository.Context.Updateable(entity).UpdateColumns(it => new { it.DeleteTime, it.DeleteMark, it.DeleteUserId }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 任务删除, 非异步.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public int DeleteTaskNoAwait(FlowTaskEntity entity)
    {
        entity.DeleteTime = DateTime.Now;
        entity.DeleteMark = 1;
        entity.DeleteUserId = _userManager.UserId;
        _flowTaskRepository.Context.Deleteable<FlowTaskNodeEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        _flowTaskRepository.Context.Deleteable<FlowTaskOperatorEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        _flowTaskRepository.Context.Deleteable<FlowTaskOperatorRecordEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        _flowTaskRepository.Context.Deleteable<FlowTaskCirculateEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        _flowTaskRepository.Context.Deleteable<FlowCandidatesEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        _flowTaskRepository.Context.Deleteable<FlowCommentEntity>(x => entity.Id == x.TaskId).ExecuteCommand();
        return _flowTaskRepository.Context.Updateable(entity).UpdateColumns(it => new { it.DeleteTime, it.DeleteMark, it.DeleteUserId }).ExecuteCommand();
    }

    /// <summary>
    /// 任务创建.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<FlowTaskEntity> CreateTask(FlowTaskEntity entity)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskEntity>().AsInsertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 任务更新.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTask(FlowTaskEntity entity)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskEntity>().AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 打回流程删除所有相关数据.
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="isClearRecord">是否清除记录.</param>
    /// <returns></returns>
    public async Task DeleteFlowTaskAllData(string taskId, bool isClearRecord = true)
    {
        try
        {
            _db.BeginTran();
            await _flowTaskRepository.Context.Updateable<FlowTaskNodeEntity>().SetColumns(x => x.State == "-2").Where(x => x.TaskId == taskId).ExecuteCommandAsync();
            await _flowTaskRepository.Context.Updateable<FlowTaskOperatorEntity>().SetColumns(x => x.State == "-1").Where(x => x.TaskId == taskId).ExecuteCommandAsync();
            if (isClearRecord)
                await _flowTaskRepository.Context.Updateable<FlowTaskOperatorRecordEntity>().SetColumns(x => x.Status == -1).Where(x => x.TaskId == taskId).ExecuteCommandAsync();
            await _flowTaskRepository.Context.Deleteable<FlowCandidatesEntity>(x => x.TaskId == taskId).ExecuteCommandAsync();
            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
        }
    }

    /// <summary>
    /// 打回流程删除所有相关数据.
    /// </summary>
    /// <param name="taskIds">任务di数组.</param>
    /// <param name="isClearRecord">是否清除记录.</param>
    /// <returns></returns>
    public async Task DeleteFlowTaskAllData(List<string> taskIds, bool isClearRecord = true)
    {
        try
        {
            _db.BeginTran();
            await _flowTaskRepository.Context.Updateable<FlowTaskNodeEntity>().SetColumns(x => x.State == "-2").Where(x => taskIds.Contains(x.TaskId)).ExecuteCommandAsync();
            await _flowTaskRepository.Context.Updateable<FlowTaskOperatorEntity>().SetColumns(x => x.State == "-1").Where(x => taskIds.Contains(x.TaskId)).ExecuteCommandAsync();
            if (isClearRecord)
                await _flowTaskRepository.Context.Updateable<FlowTaskOperatorRecordEntity>().SetColumns(x => x.Status == -1).Where(x => taskIds.Contains(x.TaskId)).ExecuteCommandAsync();
            await _flowTaskRepository.Context.Deleteable<FlowCandidatesEntity>(x => taskIds.Contains(x.TaskId)).ExecuteCommandAsync();
            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
        }
    }
    #endregion

    #region FlowTaskNode

    /// <summary>
    /// 节点列表.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskNodeEntity>> GetTaskNodeList(string taskId)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskNodeEntity>().Where(x => x.TaskId == taskId).OrderBy(x => x.SortCode).ToListAsync();
    }

    /// <summary>
    /// 节点列表.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="orderByExpression"></param>
    /// <param name="orderByType"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskNodeEntity>> GetTaskNodeList(Expression<Func<FlowTaskNodeEntity, bool>> expression, Expression<Func<FlowTaskNodeEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskNodeEntity>().Where(expression).OrderByIF(orderByExpression.IsNotEmptyOrNull(), orderByExpression, orderByType).ToListAsync();
    }

    /// <summary>
    /// 节点信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FlowTaskNodeEntity> GetTaskNodeInfo(string id)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskNodeEntity>().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 节点信息.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public async Task<FlowTaskNodeEntity> GetTaskNodeInfo(Expression<Func<FlowTaskNodeEntity, bool>> expression)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskNodeEntity>().FirstAsync(expression);
    }

    /// <summary>
    /// 节点创建.
    /// </summary>
    /// <param name="entitys"></param>
    /// <returns></returns>
    public async Task<bool> CreateTaskNode(List<FlowTaskNodeEntity> entitys)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskNodeEntity>().InsertRangeAsync(entitys);
    }

    /// <summary>
    /// 节点更新.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTaskNode(FlowTaskNodeEntity entity)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskNodeEntity>().UpdateAsync(entity);
    }

    /// <summary>
    /// 节点更新.
    /// </summary>
    /// <param name="entitys"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTaskNode(List<FlowTaskNodeEntity> entitys)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskNodeEntity>().UpdateRangeAsync(entitys);
    }
    #endregion

    #region FlowTaskOperator

    /// <summary>
    /// 经办列表.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorEntity>> GetTaskOperatorList(string taskId)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorEntity>().Where(x => x.TaskId == taskId).OrderBy(x => x.CreatorTime).ToListAsync();
    }

    /// <summary>
    /// 经办列表.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="orderByExpression"></param>
    /// <param name="orderByType"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorEntity>> GetTaskOperatorList(Expression<Func<FlowTaskOperatorEntity, bool>> expression, Expression<Func<FlowTaskOperatorEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorEntity>().Where(expression).OrderByIF(orderByExpression.IsNotEmptyOrNull(), orderByExpression, orderByType).ToListAsync();
    }

    /// <summary>
    /// 经办信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FlowTaskOperatorEntity> GetTaskOperatorInfo(string id)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorEntity>().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 经办信息.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public async Task<FlowTaskOperatorEntity> GetTaskOperatorInfo(Expression<Func<FlowTaskOperatorEntity, bool>> expression)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorEntity>().FirstAsync(expression);
    }

    /// <summary>
    /// 经办删除.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<int> DeleteTaskOperator(List<string> ids)
    {
        return await _flowTaskRepository.Context.Updateable<FlowTaskOperatorEntity>().SetColumns(x => x.State == "-1").Where(x => ids.Contains(x.Id)).ExecuteCommandAsync();
    }

    /// <summary>
    /// 经办创建.
    /// </summary>
    /// <param name="entitys"></param>
    /// <returns></returns>
    public async Task<bool> CreateTaskOperator(List<FlowTaskOperatorEntity> entitys)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskOperatorEntity>().InsertRangeAsync(entitys);
    }

    /// <summary>
    /// 经办创建.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> CreateTaskOperator(FlowTaskOperatorEntity entity)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskOperatorEntity>().InsertAsync(entity);
    }

    /// <summary>
    /// 经办更新.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTaskOperator(FlowTaskOperatorEntity entity)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskOperatorEntity>().UpdateAsync(entity);
    }

    /// <summary>
    /// 经办更新.
    /// </summary>
    /// <param name="entitys"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTaskOperator(List<FlowTaskOperatorEntity> entitys)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskOperatorEntity>().UpdateRangeAsync(entitys);
    }
    #endregion

    #region FlowTaskOperatorRecord

    /// <summary>
    /// 经办记录列表.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorRecordEntity>> GetTaskOperatorRecordList(string taskId)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity>().Where(x => x.TaskId == taskId).OrderBy(o => o.HandleTime).ToListAsync();
    }

    /// <summary>
    /// 经办记录列表.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="orderByExpression"></param>
    /// <param name="orderByType"></param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorRecordEntity>> GetTaskOperatorRecordList(Expression<Func<FlowTaskOperatorRecordEntity, bool>> expression, Expression<Func<FlowTaskOperatorRecordEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity>().Where(expression).OrderByIF(orderByExpression.IsNotEmptyOrNull(),orderByExpression, orderByType).ToListAsync();
    }

    /// <summary>
    /// 经办记录信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FlowTaskOperatorRecordEntity> GetTaskOperatorRecordInfo(string id)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity>().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 经办记录信息.
    /// </summary>
    /// <param name="expression">条件.</param>
    /// <returns></returns>
    public async Task<FlowTaskOperatorRecordEntity> GetTaskOperatorRecordInfo(Expression<Func<FlowTaskOperatorRecordEntity, bool>> expression)
    {
        return await _flowTaskRepository.Context.Queryable<FlowTaskOperatorRecordEntity>().FirstAsync(expression);
    }

    /// <summary>
    /// 经办记录创建.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> CreateTaskOperatorRecord(FlowTaskOperatorRecordEntity entity)
    {
        entity.Id = SnowflakeIdHelper.NextId();
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskOperatorRecordEntity>().InsertAsync(entity);
    }

    /// <summary>
    /// 经办记录作废.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task DeleteTaskOperatorRecord(List<string> ids)
    {
        await _flowTaskRepository.Context.Updateable<FlowTaskOperatorRecordEntity>().SetColumns(it => it.Status == -1).Where(x => ids.Contains(x.Id)).ExecuteCommandAsync();
    }
    #endregion

    #region FlowTaskCirculate

    /// <summary>
    /// 传阅创建.
    /// </summary>
    /// <param name="entitys"></param>
    /// <returns></returns>
    public async Task<bool> CreateTaskCirculate(List<FlowTaskCirculateEntity> entitys)
    {
        return await _flowTaskRepository.Context.GetSimpleClient<FlowTaskCirculateEntity>().InsertRangeAsync(entitys);
    }
    #endregion

    #region FlowTaskCandidates

    /// <summary>
    /// 候选人创建.
    /// </summary>
    /// <param name="entitys"></param>
    public void CreateFlowCandidates(List<FlowCandidatesEntity> entitys)
    {
        _flowTaskRepository.Context.GetSimpleClient<FlowCandidatesEntity>().InsertRange(entitys);
    }

    /// <summary>
    /// 候选人删除.
    /// </summary>
    /// <param name="expression"></param>
    public void DeleteFlowCandidates(Expression<Func<FlowCandidatesEntity, bool>> expression)
    {
        _flowTaskRepository.Context.Deleteable(expression).ExecuteCommand();
    }

    /// <summary>
    /// 候选人获取.
    /// </summary>
    /// <param name="nodeId"></param>
    public List<string> GetFlowCandidates(string nodeId)
    {
        var flowCandidates = new List<string>();
        var candidateUserIdList = _flowTaskRepository.Context.GetSimpleClient<FlowCandidatesEntity>().GetList(x => x.TaskNodeId == nodeId).Select(x => x.Candidates).ToList();
        foreach (var item in candidateUserIdList)
        {
            flowCandidates = flowCandidates.Union(item.Split(",").ToList()).Distinct().ToList();
        }

        return flowCandidates;
    }
    #endregion
}

