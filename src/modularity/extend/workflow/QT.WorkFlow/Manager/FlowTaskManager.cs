using System.Text;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Dtos.VisualDev;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.RemoteRequest.Extensions;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Entitys;
using QT.VisualDev.Interfaces;
using QT.WorkFlow.Entitys.Dto.FlowBefore;
using QT.WorkFlow.Entitys.Dto.FlowTask;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Entitys.Enum;
using QT.WorkFlow.Entitys.Model;
using QT.WorkFlow.Entitys.Model.Conifg;
using QT.WorkFlow.Entitys.Model.Item;
using QT.WorkFlow.Entitys.Model.Properties;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using QT.WorkFlow.Interfaces.Service;
using Mapster;
using Newtonsoft.Json.Linq;
using SqlSugar;
using QT.Common.Const;
using Senparc.NeuChar.Helpers;

namespace QT.WorkFlow.Manager;

public class FlowTaskManager : IFlowTaskManager, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IUsersService _usersService;
    private readonly IBillRullService _billRullService;
    private readonly IOrganizeService _organizeService;
    private readonly IUserRelationService _userRelationService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IDataInterfaceService _dataInterfaceService;
    private readonly IRunService _runService;
    private readonly IUserManager _userManager;
    private readonly IDataBaseManager _dataBaseManager;
    private readonly ITenant _db;
    private string globalTaskId = string.Empty;
    private string globalTaskNodeId = string.Empty;
    private FlowTaskEntity globalTaskEntity = new FlowTaskEntity();

    public FlowTaskManager(
        IFlowTaskRepository flowTaskRepository,
        IUsersService usersService,
        IBillRullService billRullService,
        IOrganizeService organizeService,
        IUserRelationService userRelationService,
        IMessageTemplateService messageTemplateService,
        IDataInterfaceService dataInterfaceService,
        IRunService runService,
        IUserManager userManager,
        IDataBaseManager dataBaseManager,
        ISqlSugarClient context)
    {
        _flowTaskRepository = flowTaskRepository;
        _usersService = usersService;
        _billRullService = billRullService;
        _organizeService = organizeService;
        _userRelationService = userRelationService;
        _messageTemplateService = messageTemplateService;
        _dataInterfaceService = dataInterfaceService;
        _runService = runService;
        _userManager = userManager;
        _dataBaseManager = dataBaseManager;
        _db = context.AsTenant();
    }

    #region PublicMethod

    /// <summary>
    /// 获取任务详情.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <param name="taskNodeId">节点id.</param>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    public async Task<FlowBeforeInfoOutput> GetFlowBeforeInfo(string id, string taskNodeId, string taskOperatorId = null)
    {
        try
        {
            var output = new FlowBeforeInfoOutput();
            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(id);
            if (flowTaskEntity.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.WF0026);
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            var flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(x => x.TaskId == flowTaskEntity.Id && "0".Equals(x.State), x => x.SortCode);
            var flowTaskNodeList = flowTaskNodeEntityList.Adapt<List<FlowTaskNodeModel>>().OrderBy(x => x.sortCode).ToList();
            var flowTaskOperatorList = await _flowTaskRepository.GetTaskOperatorList(x => x.TaskId == flowTaskEntity.Id && "0".Equals(x.State));
            var flowTaskOperatorRecordList = (await _flowTaskRepository.GetTaskOperatorRecordList(flowTaskEntity.Id)).Adapt<List<FlowTaskOperatorRecordModel>>();
            var colorFlag = true;
            // 统一查找用户
            var flowTaskOperatorRecordUserIdList = flowTaskOperatorRecordList.SelectMany(x => new[] { x.handleId, x.operatorId }).Where(x => x.IsNotEmptyOrNull()).ToList();
            var flowTaskOperatorRecordUserList = await _usersService.GetUserListByExp(u => flowTaskOperatorRecordUserIdList.Contains(u.Id) && u.DeleteMark == null);
            foreach (var item in flowTaskOperatorRecordList)
            {
                //item.userName = await _usersService.GetUserName(item.handleId);
                //item.operatorId = await _usersService.GetUserName(item.operatorId);

                item.userName = _usersService.GetUserName(flowTaskOperatorRecordUserList.Find(x => x.Id == item.handleId));

                item.operatorId = _usersService.GetUserName(flowTaskOperatorRecordUserList.Find(x => x.Id == item.operatorId));
            }

            foreach (var item in flowTaskNodeList)
            {
                if (colorFlag || item.completion == 1)
                {
                    item.type = "0";
                }

                if (flowTaskEntity.ThisStepId.Contains(item.nodeCode))
                {
                    item.type = "1";
                    colorFlag = false;
                }

                if (FlowTaskNodeTypeEnum.end.ParseToString().Equals(flowTaskEntity.ThisStepId))
                {
                    item.type = "0";
                }

                item.userName = await GetApproverUserName(item, flowTaskEntity, flowTaskEntity.FlowFormContentJson, flowTaskNodeEntityList);
            }

            var thisNode = await _flowTaskRepository.GetTaskNodeInfo(x => x.TaskId == flowTaskEntity.Id && x.Id == taskNodeId);
            if (thisNode.IsNotEmptyOrNull())
            {
                var thisNodeProperties = thisNode.NodePropertyJson.ToObject<ApproversProperties>();
                output.approversProperties = thisNodeProperties;
                output.formOperates = thisNodeProperties.formOperates.Adapt<List<FormOperatesModel>>();
            }

            output.flowFormInfo = flowTaskEntity.FlowForm;
            output.flowTaskInfo = flowTaskEntity.Adapt<FlowTaskModel>();
            output.flowTaskInfo.appFormUrl = flowEngineEntity.AppFormUrl;
            output.flowTaskInfo.formUrl = flowEngineEntity.FormUrl;
            output.flowTaskInfo.type = flowEngineEntity.Type;
            output.flowTaskNodeList = flowTaskNodeList;
            output.flowTaskOperatorList = flowTaskOperatorList.Adapt<List<FlowTaskOperatorModel>>();
            output.flowTaskOperatorRecordList = flowTaskOperatorRecordList;
            if (taskOperatorId.IsNotEmptyOrNull())
            {
                var flowTaskOperator = flowTaskOperatorList.Find(x => x.Id == taskOperatorId);
                if (flowTaskOperator.IsNotEmptyOrNull() && flowTaskOperator.DraftData.IsNotEmptyOrNull())
                {
                    output.draftData = flowTaskOperator.DraftData.ToObject();
                }
            }

            return output;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 详情查看表单数据根据控件转换.
    /// </summary>
    /// <param name="entity">任务实例.</param>
    /// <returns></returns>
    public async Task<FlowTaskInfoOutput> GetFlowDynamicDataManage(FlowTaskEntity entity)
    {
        try
        {
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(entity.FlowId);
            var flowEngineTablesModelList = flowEngineEntity.Tables.ToObject<List<DbTableRelationModel>>();
            FlowTaskInfoOutput output = entity.Adapt<FlowTaskInfoOutput>();
            var visualDevEntity = flowEngineEntity.Adapt<VisualDevEntity>();
            visualDevEntity.FormData = flowEngineEntity.FormTemplateJson;
            if (flowEngineTablesModelList.Count > 0)
            {
                output.data = (await _runService.GetHaveTableInfo(entity.Id, visualDevEntity)).ToJsonString();
            }
            else
            {
                //真实表单数据
                Dictionary<string, object> formDataDic = entity.FlowFormContentJson.ToObject<Dictionary<string, object>>();
                output.data = (await _runService.GetIsNoTableInfo(visualDevEntity, entity.FlowFormContentJson)).ToJsonString();
            }
            return output;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="id">任务主键id（通过空值判断是修改还是新增）.</param>
    /// <param name="flowId">引擎id.</param>
    /// <param name="processId">关联id.</param>
    /// <param name="flowTitle">任务名.</param>
    /// <param name="flowUrgent">紧急程度（自定义默认为1）.</param>
    /// <param name="billNo">单据规则.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="status">状态 1:保存，0提交.</param>
    /// <param name="approvaUpType">审批修改权限1：可写，0：可读.</param>
    /// <param name="isSysTable">true：系统表单，false：自定义表单.</param>
    /// <param name="parentId">任务父id.</param>
    /// <param name="crUser">子流程发起人.</param>
    /// <param name="isDev">是否功能设计.</param>
    /// <param name="isAsync">是否异步.</param>
    /// <returns></returns>
    public async Task<FlowTaskEntity> Save(string id, string flowId, string processId, string flowTitle, int? flowUrgent, string billNo, object formData,
        int status, int? approvaUpType = 0, bool isSysTable = true, string parentId = "0", string crUser = null, bool isDev = false, bool isAsync = false)
    {
        try
        {
            var flowTaskEntity = new FlowTaskEntity();
            if (!isSysTable)
            {
                var flowTaskEntityDynamic = await FlowDynamicDataManage(id, flowId, processId, flowTitle, flowUrgent, billNo, formData, crUser, isDev);
                processId = flowTaskEntityDynamic.ProcessId;
                flowTitle = flowTaskEntityDynamic.FlowName;
                flowUrgent = flowTaskEntityDynamic.FlowUrgent;
                billNo = flowTaskEntityDynamic.EnCode;
                formData = flowTaskEntityDynamic.FlowFormContentJson.ToObject();
            }

            if (id.IsEmpty())
            {
                FlowEngineEntity flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);
                flowTaskEntity.Id = processId;
                flowTaskEntity.ProcessId = processId;
                flowTaskEntity.EnCode = billNo;
                flowTaskEntity.FullName = parentId.Equals("0") ? flowTitle : flowTitle + "(子流程)";
                flowTaskEntity.FlowUrgent = flowUrgent;
                flowTaskEntity.FlowId = flowEngineEntity.Id;
                flowTaskEntity.FlowCode = flowEngineEntity.EnCode;
                flowTaskEntity.FlowName = flowEngineEntity.FullName;
                flowTaskEntity.FlowType = flowEngineEntity.Type;
                flowTaskEntity.FlowCategory = flowEngineEntity.Category;
                flowTaskEntity.FlowForm = flowEngineEntity.FormTemplateJson;
                flowTaskEntity.FlowFormContentJson = formData.IsNullOrEmpty() ? string.Empty : formData.ToJsonString();
                flowTaskEntity.FlowTemplateJson = flowEngineEntity.FlowTemplateJson;
                flowTaskEntity.FlowVersion = flowEngineEntity.Version;
                flowTaskEntity.Status = FlowTaskStatusEnum.Draft.ParseToInt();
                flowTaskEntity.Completion = 0;
                flowTaskEntity.ThisStep = "开始";
                flowTaskEntity.CreatorTime = DateTime.Now;
                flowTaskEntity.CreatorUserId = crUser.IsEmpty() ? _userManager.UserId : crUser;
                flowTaskEntity.ParentId = parentId;
                flowTaskEntity.IsAsync = isAsync ? 1 : 0;
                if (status == 0)
                {
                    flowTaskEntity.Status = FlowTaskStatusEnum.Handle.ParseToInt();
                    flowTaskEntity.EnabledMark = FlowTaskStatusEnum.Handle.ParseToInt();
                    flowTaskEntity.StartTime = DateTime.Now;
                    flowTaskEntity.CreatorTime = DateTime.Now;
                }

                await _flowTaskRepository.CreateTask(flowTaskEntity);
            }
            else
            {
                flowTaskEntity = await _flowTaskRepository.GetTaskInfo(id);
                if (flowTaskEntity.Status == FlowTaskStatusEnum.Handle.ParseToInt() && approvaUpType == 0)
                    throw Oops.Oh(ErrorCode.WF0031);
                if (status == 0)
                {
                    flowTaskEntity.Status = FlowTaskStatusEnum.Handle.ParseToInt();
                    flowTaskEntity.StartTime = DateTime.Now;
                    flowTaskEntity.LastModifyTime = DateTime.Now;
                    flowTaskEntity.LastModifyUserId = _userManager.UserId;
                }

                if (approvaUpType == 0)
                {
                    flowTaskEntity.FullName = parentId.Equals("0") ? flowTitle : flowTitle + "(子流程)";
                    flowTaskEntity.FlowUrgent = flowUrgent;
                }

                if (formData != null)
                {
                    flowTaskEntity.FlowFormContentJson = formData.ToJsonString();
                }

                await _flowTaskRepository.UpdateTask(flowTaskEntity);
            }

            return flowTaskEntity;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 提交.
    /// </summary>
    /// <param name="id">主键id（通过空值判断是修改还是新增）.</param>
    /// <param name="flowId">引擎id.</param>
    /// <param name="processId">关联id.</param>
    /// <param name="flowTitle">任务名.</param>
    /// <param name="flowUrgent">紧急程度（自定义默认为1）.</param>
    /// <param name="billNo">单据规则.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="status">状态 1:保存，0提交.</param>
    /// <param name="approvaUpType">审批修改权限1：可写，0：可读.</param>
    /// <param name="isSysTable">true：系统表单，false：自定义表单.</param>
    /// <param name="isDev">是否功能设计.</param>
    /// <param name="candidateList">候选人.</param>
    /// <returns></returns>
    public async Task<bool> Submit(string id, string flowId, string processId, string flowTitle, int? flowUrgent, string billNo, object formData, int status, int? approvaUpType = 0, bool isSysTable = true, bool isDev = false, Dictionary<string, List<string>> candidateList = null)
    {
        try
        {
            _db.BeginTran();

            // 流程引擎
            FlowEngineEntity flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);

            // 流程任务
            FlowTaskEntity flowTaskEntity = await this.Save(id, flowId, processId, flowTitle, flowUrgent, billNo, formData, status, approvaUpType, isSysTable, "0", null, isDev);

            // 流程节点
            List<FlowTaskNodeEntity> flowTaskNodeEntityList = ParsingTemplateGetNodeList(flowEngineEntity, formData.ToJsonString(), flowTaskEntity.Id);
            SaveNodeCandidates(flowTaskNodeEntityList, candidateList, "0");
            await _flowTaskRepository.CreateTaskNode(flowTaskNodeEntityList);

            List<FlowTaskOperatorEntity> flowTaskOperatorEntityList = new List<FlowTaskOperatorEntity>();

            // 开始节点
            var startTaskNodeEntity = flowTaskNodeEntityList.Find(m => FlowTaskNodeTypeEnum.start.ParseToString().Equals(m.NodeType));
            var startApproversProperties = startTaskNodeEntity.NodePropertyJson.ToObject<StartProperties>();
            flowTaskEntity.IsBatch = startApproversProperties.isBatchApproval ? 1 : 0;
            var nextTaskNodeIdList = startTaskNodeEntity.NodeNext.Split(",");
            if (FlowTaskNodeTypeEnum.end.ParseToString().Equals(nextTaskNodeIdList.FirstOrDefault()))
            {
                flowTaskEntity.Status = FlowTaskStatusEnum.Adopt.ParseToInt();
                flowTaskEntity.Completion = 100;
                flowTaskEntity.EndTime = DateTime.Now;
                flowTaskEntity.ThisStepId = FlowTaskNodeTypeEnum.end.ParseToString();
                flowTaskEntity.ThisStep = "结束";
                await _flowTaskRepository.UpdateTask(flowTaskEntity);

                // 子流程结束回到主流程下一节点
                if (flowTaskEntity.ParentId != "0" && flowTaskEntity.IsAsync == 0)
                {
                    await InsertSubFlowNextNode(flowTaskEntity);
                }
            }
            else
            {
                #region 流程经办

                // 任务流程当前节点名
                var ThisStepList = new List<string>();

                // 任务流程当前完成度
                var CompletionList = new List<int>();
                var isAsync = false;
                foreach (var item in nextTaskNodeIdList)
                {
                    var nextTaskNodeEntity = flowTaskNodeEntityList.Find(m => m.NodeCode.Equals(item));
                    var approverPropertiers = nextTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
                    if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(nextTaskNodeEntity.NodeType))
                    {
                        var childTaskPro = nextTaskNodeEntity.NodePropertyJson.ToObject<ChildTaskProperties>();
                        var childTaskCrUserList = await GetFlowUserId(flowTaskEntity.CreatorUserId, childTaskPro.initiateType, childTaskPro.managerLevel,
                formData.ToJsonString(), childTaskPro.formField, childTaskPro.formFieldType, flowTaskNodeEntityList, childTaskPro.nodeId,
                nextTaskNodeEntity, childTaskPro.getUserUrl, childTaskPro.initiator,
                childTaskPro.initiateRole, childTaskPro.initiatePos);
                        var childFormData = await GetSubFlowFormData(childTaskPro, formData.ToJsonString());
                        childTaskPro.childTaskId = await CreateSubProcesses(childTaskPro, childFormData, flowTaskEntity.Id, childTaskCrUserList);
                        childTaskPro.formData = formData.ToJsonString();
                        nextTaskNodeEntity.NodePropertyJson = childTaskPro.ToJsonString();

                        // 将子流程id保存到主流程的子流程节点属性上
                        nextTaskNodeEntity.Completion = childTaskPro.isAsync ? 1 : 0;
                        await _flowTaskRepository.UpdateTaskNode(nextTaskNodeEntity);
                        await Alerts(childTaskPro.launchMsgConfig, childTaskCrUserList, formData.ToJsonString());
                        if (childTaskPro.isAsync)
                        {
                            isAsync = true;
                            flowTaskNodeEntityList.Remove(flowTaskNodeEntityList.Find(m => m.NodeCode.Equals(item)));
                            flowTaskNodeEntityList.Add(nextTaskNodeEntity);
                            flowTaskNodeEntityList = flowTaskNodeEntityList.FindAll(x => x.State == "0");
                            await CreateNextFlowTaskOperator(flowTaskNodeEntityList, nextTaskNodeEntity,
                                nextTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>(), new List<FlowTaskOperatorEntity>(),
                                1, flowTaskEntity, string.Empty, flowTaskOperatorEntityList, formData.ToJsonString(),
                                new FlowHandleModel(), flowEngineEntity.FormType.ParseToInt());
                        }
                    }
                    else
                    {
                        await AddFlowTaskOperatorEntityByAssigneeType(flowTaskOperatorEntityList, flowTaskNodeEntityList, startTaskNodeEntity, nextTaskNodeEntity, flowTaskEntity.CreatorUserId, formData.ToJsonString(), 0);
                    }

                    ThisStepList.Add(nextTaskNodeEntity.NodeName);
                    CompletionList.Add(approverPropertiers.progress.ParseToInt());
                }

                if (!isAsync)
                {
                    await _flowTaskRepository.CreateTaskOperator(flowTaskOperatorEntityList);
                }
                #endregion

                #region 更新流程任务
                if (isAsync)
                {
                    await _flowTaskRepository.UpdateTask(flowTaskEntity);
                }
                else
                {
                    flowTaskEntity.ThisStepId = startTaskNodeEntity.NodeNext;
                    flowTaskEntity.ThisStep = string.Join(",", ThisStepList);
                    flowTaskEntity.Completion = CompletionList.Min();
                    await _flowTaskRepository.UpdateTask(flowTaskEntity);
                }
                #endregion
            }

            #region 流程经办记录
            FlowTaskOperatorRecordEntity flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 2;
            flowTaskOperatorRecordEntity.NodeName = "开始";
            flowTaskOperatorRecordEntity.TaskId = flowTaskEntity.Id;
            flowTaskOperatorRecordEntity.Status = 0;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion

            _db.CommitTran();

            #region 开始事件
            globalTaskId = startTaskNodeEntity.TaskId;
            globalTaskNodeId = startTaskNodeEntity.Id;
            await RequestEvents(startApproversProperties.initFuncConfig, formData.ToJsonString());
            #endregion

            #region 消息提醒

            // 审批消息
            var messageDic = GroupByOperator(flowTaskOperatorEntityList);
            var bodyDic = new Dictionary<string, object>();
            foreach (var item in messageDic.Keys)
            {
                var userList = messageDic[item].Select(x => x.HandleId).ToList();

                // 委托人
                var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
                userList = userList.Union(delegateUserIds).ToList();
                bodyDic = GetMesBodyText(flowEngineEntity, item, userList, messageDic[item], 2);
                await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
                await Alerts(startApproversProperties.waitMsgConfig, userList, formData.ToJsonString());
            }

            // 结束消息
            if (flowTaskEntity.Status == FlowTaskStatusEnum.Adopt.ParseToInt())
            {
                #region 结束事件
                await RequestEvents(startApproversProperties.endFuncConfig, formData.ToJsonString());
                #endregion

                bodyDic = GetMesBodyText(flowEngineEntity, string.Empty, new List<string>() { flowTaskEntity.CreatorUserId }, null, 1);
                await StationLetterMsg(flowTaskEntity.FullName, new List<string>() { flowTaskEntity.CreatorUserId }, 5, bodyDic);
                await Alerts(startApproversProperties.endMsgConfig, new List<string>() { flowTaskEntity.CreatorUserId }, formData.ToJsonString());
            }
            #endregion
            return true;
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            return false;
        }
    }

    /// <summary>
    /// 审批(同意).
    /// </summary>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="formType">表单类型.</param>
    /// <returns></returns>
    public async Task Audit(FlowTaskEntity flowTaskEntity, FlowTaskOperatorEntity flowTaskOperatorEntity, FlowHandleModel flowHandleModel, int formType)
    {
        //流程所有节点
        List<FlowTaskNodeEntity> flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(x => x.TaskId == flowTaskEntity.Id && x.State == "0");
        var candidates = SaveNodeCandidates(flowTaskNodeEntityList, flowHandleModel.candidateList, flowTaskOperatorEntity.Id);
        try
        {
            _db.BeginTran();

            // 当前节点
            FlowTaskNodeEntity flowTaskNodeEntity = flowTaskNodeEntityList.Find(m => m.Id == flowTaskOperatorEntity.TaskNodeId);

            // 当前节点属性
            ApproversProperties approversProperties = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();

            // 当前节点所有审批人
            var thisFlowTaskOperatorEntityList = await _flowTaskRepository.GetTaskOperatorList(x => x.TaskId == flowTaskNodeEntity.TaskId && x.TaskNodeId == flowTaskNodeEntity.Id && x.State == "0");
            // 下一节点流程经办
            List<FlowTaskOperatorEntity> flowTaskOperatorEntityList = new List<FlowTaskOperatorEntity>();

            // 流程抄送
            List<FlowTaskCirculateEntity> flowTaskCirculateEntityList = new List<FlowTaskCirculateEntity>();

            // 表单数据
            var formData = formType == 2 ? flowHandleModel.formData.ToJsonString().ToObject<JObject>()["data"].ToString() : flowHandleModel.formData.ToJsonString();

            if (flowTaskOperatorEntity.Id.IsNotEmptyOrNull())
            {
                #region 更新当前经办数据
                await UpdateFlowTaskOperator(flowTaskOperatorEntity, thisFlowTaskOperatorEntityList, approversProperties, 1, flowHandleModel.freeApproverUserId);
                #endregion

                #region 更新当前抄送
                GetflowTaskCirculateEntityList(approversProperties, flowTaskOperatorEntity, flowTaskCirculateEntityList, flowHandleModel.copyIds);
                await _flowTaskRepository.CreateTaskCirculate(flowTaskCirculateEntityList);
                #endregion
            }

            #region 更新经办记录
            await CreateOperatorRecode(flowTaskOperatorEntity, flowHandleModel, 1);
            #endregion

            #region 更新下一节点经办
            var freeApproverOperatorEntity = new FlowTaskOperatorEntity();
            if (flowHandleModel.freeApproverUserId.IsNotEmptyOrNull())
            {
                // 加签审批人
                freeApproverOperatorEntity.Id = SnowflakeIdHelper.NextId();
                freeApproverOperatorEntity.ParentId = flowTaskOperatorEntity.Id;
                freeApproverOperatorEntity.HandleType = "6";
                freeApproverOperatorEntity.HandleId = flowHandleModel.freeApproverUserId;
                freeApproverOperatorEntity.NodeCode = flowTaskOperatorEntity.NodeCode;
                freeApproverOperatorEntity.NodeName = flowTaskOperatorEntity.NodeName;
                freeApproverOperatorEntity.Description = flowTaskOperatorEntity.Description;
                freeApproverOperatorEntity.CreatorTime = DateTime.Now;
                freeApproverOperatorEntity.TaskNodeId = flowTaskOperatorEntity.TaskNodeId;
                freeApproverOperatorEntity.TaskId = flowTaskOperatorEntity.TaskId;
                freeApproverOperatorEntity.Type = flowTaskOperatorEntity.Type;
                freeApproverOperatorEntity.State = flowTaskOperatorEntity.State;
                freeApproverOperatorEntity.Completion = 0;
                await _flowTaskRepository.CreateTaskOperator(freeApproverOperatorEntity);
                // 当前审批人state改为1
                flowTaskOperatorEntity.State = "1";
                await _flowTaskRepository.UpdateTaskOperator(flowTaskOperatorEntity);

                #region 流转记录
                var flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
                flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
                flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
                flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
                flowTaskOperatorRecordEntity.HandleStatus = 6;
                flowTaskOperatorRecordEntity.NodeName = flowTaskOperatorEntity.NodeName;
                flowTaskOperatorRecordEntity.TaskId = flowTaskOperatorEntity.TaskId;
                flowTaskOperatorRecordEntity.TaskNodeId = flowTaskOperatorEntity.TaskNodeId;
                flowTaskOperatorRecordEntity.TaskOperatorId = flowTaskOperatorEntity.Id;
                flowTaskOperatorRecordEntity.Status = 0;
                flowTaskOperatorRecordEntity.OperatorId = flowHandleModel.freeApproverUserId;
                await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
                #endregion
            }
            else
            {
                await CreateNextFlowTaskOperator(flowTaskNodeEntityList, flowTaskNodeEntity, approversProperties,
                                        thisFlowTaskOperatorEntityList, 1, flowTaskEntity, flowHandleModel.freeApproverUserId,
                                        flowTaskOperatorEntityList, formData, flowHandleModel, formType);
            }
            #endregion

            #region 更新节点
            await _flowTaskRepository.UpdateTaskNode(flowTaskNodeEntity);
            #endregion

            #region 更新任务
            globalTaskId = flowTaskNodeEntity.TaskId;
            globalTaskNodeId = flowTaskNodeEntity.Id;
            if (flowTaskNodeEntity.Completion > 0)
            {

                if (flowTaskEntity.Status == FlowTaskStatusEnum.Adopt.ParseToInt())
                {
                    #region 子流程结束回到主流程下一节点
                    if (flowTaskEntity.ParentId != "0" && flowTaskEntity.IsAsync == 0)
                    {
                        await InsertSubFlowNextNode(flowTaskEntity);
                    }
                    #endregion
                }
                await _flowTaskRepository.UpdateTask(flowTaskEntity);
            }
            #endregion

            _db.CommitTran();

            #region 消息与事件
            var startApproversProperties = flowTaskNodeEntityList.Find(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType)).NodePropertyJson.ToObject<StartProperties>();
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            var bodyDic = new Dictionary<string, object>();
            //加签
            if (flowHandleModel.freeApproverUserId.IsNotEmptyOrNull())
            {
                bodyDic = GetMesBodyText(flowEngineEntity, freeApproverOperatorEntity.TaskNodeId, new List<string>() { flowHandleModel.freeApproverUserId }, new List<FlowTaskOperatorEntity>() { freeApproverOperatorEntity }, 2);
                await StationLetterMsg(flowTaskEntity.FullName, new List<string>() { flowHandleModel.freeApproverUserId }, 0, bodyDic);
                await Alerts(startApproversProperties.waitMsgConfig, new List<string>() { flowHandleModel.freeApproverUserId }, formData);
            }

            //抄送
            var userIdList = flowTaskCirculateEntityList.Select(x => x.ObjectId).ToList();
            if (userIdList.Count > 0)
            {
                bodyDic = GetMesBodyText(flowEngineEntity, flowTaskNodeEntity.Id, userIdList, null, 3, flowTaskOperatorEntity.Id);
                await StationLetterMsg(flowTaskEntity.FullName, userIdList, 3, bodyDic);
                approversProperties.copyMsgConfig = approversProperties.copyMsgConfig.on == 2 ? startApproversProperties.copyMsgConfig : approversProperties.copyMsgConfig;
                await Alerts(approversProperties.copyMsgConfig, userIdList, formData);
            }

            if (flowTaskNodeEntity.Completion > 0)
            {
                #region 审批事件
                await RequestEvents(approversProperties.approveFuncConfig, formData);
                #endregion

                #region 消息提醒
                var messageDic = GroupByOperator(flowTaskOperatorEntityList);
                //审批
                foreach (var item in messageDic.Keys)
                {
                    var userList = messageDic[item].Select(x => x.HandleId).ToList();
                    //委托人
                    var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
                    userList = userList.Union(delegateUserIds).ToList();
                    bodyDic = GetMesBodyText(flowEngineEntity, item, userList, messageDic[item], 2);
                    await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
                    await StationLetterMsg(flowTaskEntity.FullName, userList, 1, bodyDic);
                    await Alerts(startApproversProperties.waitMsgConfig, userList, formData);
                    if (approversProperties.approveMsgConfig.IsNotEmptyOrNull())
                    {
                        approversProperties.approveMsgConfig = approversProperties.approveMsgConfig.on == 2 ? startApproversProperties.approveMsgConfig : approversProperties.approveMsgConfig;
                    }
                    await Alerts(approversProperties.approveMsgConfig, userList, formData);
                }
                #endregion

                if (flowTaskEntity.Status == FlowTaskStatusEnum.Adopt.ParseToInt())
                {
                    #region 结束事件
                    await RequestEvents(startApproversProperties.endFuncConfig, formData);
                    #endregion
                    //结束
                    bodyDic = GetMesBodyText(flowEngineEntity, flowTaskNodeEntity.Id, new List<string>() { flowTaskEntity.CreatorUserId }, null, 1);
                    await StationLetterMsg(flowTaskEntity.FullName, new List<string>() { flowTaskEntity.CreatorUserId }, 5, bodyDic);
                    await Alerts(startApproversProperties.endMsgConfig, new List<string>() { flowTaskEntity.CreatorUserId }, formData);
                }
            }
            #endregion
        }
        catch (Exception ex)
        {
            var ids = candidates.Select(x => x.Id).ToArray();
            _flowTaskRepository.DeleteFlowCandidates(x => ids.Contains(x.Id));
            _db.RollbackTran();
            throw Oops.Oh(ex.Message);
        }
    }

    /// <summary>
    /// 审批(拒绝).
    /// </summary>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="formType">表单类型.</param>
    /// <returns></returns>
    public async Task Reject(FlowTaskEntity flowTaskEntity, FlowTaskOperatorEntity flowTaskOperatorEntity, FlowHandleModel flowHandleModel, int formType)
    {
        try
        {
            _db.BeginTran();
            //流程所有节点
            List<FlowTaskNodeEntity> flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(x => x.State == "0" && x.TaskId == flowTaskEntity.Id);
            //当前节点
            FlowTaskNodeEntity flowTaskNodeEntity = flowTaskNodeEntityList.Find(m => m.Id == flowTaskOperatorEntity.TaskNodeId);
            //当前节点属性
            ApproversProperties approversProperties = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
            //当前节点所有审批人
            var thisFlowTaskOperatorEntityList = await _flowTaskRepository.GetTaskOperatorList(x => x.TaskNodeId == flowTaskNodeEntity.Id && x.State == "0" && x.TaskId == flowTaskNodeEntity.TaskId);
            //表单数据
            var formData = flowTaskEntity.FlowFormContentJson;
            //驳回节点流程经办
            List<FlowTaskOperatorEntity> flowTaskOperatorEntityList = new List<FlowTaskOperatorEntity>();
            #region 更新当前经办数据
            await UpdateFlowTaskOperator(flowTaskOperatorEntity, thisFlowTaskOperatorEntityList, approversProperties, 0, flowHandleModel.freeApproverUserId);
            #endregion

            #region 自定义抄送
            var flowTaskCirculateEntityList = new List<FlowTaskCirculateEntity>();
            GetflowTaskCirculateEntityList(approversProperties, flowTaskOperatorEntity, flowTaskCirculateEntityList, flowHandleModel.copyIds, 0);
            await _flowTaskRepository.CreateTaskCirculate(flowTaskCirculateEntityList);
            #endregion

            #region 更新驳回经办
            await CreateNextFlowTaskOperator(flowTaskNodeEntityList, flowTaskNodeEntity, approversProperties,
                thisFlowTaskOperatorEntityList, 0, flowTaskEntity, flowHandleModel.freeApproverUserId,
                flowTaskOperatorEntityList, formData, flowHandleModel, formType);
            #endregion

            #region 更新流程任务
            if (flowTaskEntity.Status == FlowTaskStatusEnum.Reject.ParseToInt())
            {
                await _flowTaskRepository.UpdateTask(flowTaskEntity);
                await _flowTaskRepository.DeleteFlowTaskAllData(flowTaskEntity.Id);
            }
            else
            {
                await _flowTaskRepository.UpdateTask(flowTaskEntity);
                await _flowTaskRepository.CreateTaskOperator(flowTaskOperatorEntityList);
            }
            #endregion

            #region 更新经办记录
            await CreateOperatorRecode(flowTaskOperatorEntity, flowHandleModel, 0);
            #endregion

            _db.CommitTran();

            #region 消息与事件
            globalTaskId = flowTaskNodeEntity.TaskId;
            globalTaskNodeId = flowTaskNodeEntity.Id;
            await RequestEvents(approversProperties.rejectFuncConfig, formData);

            var startApproversProperties = flowTaskNodeEntityList.Find(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType)).NodePropertyJson.ToObject<StartProperties>();
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            var bodyDic = new Dictionary<string, object>();
            if (flowTaskOperatorEntityList.Count > 0)
            {
                #region 审批事件
                await RequestEvents(approversProperties.approveFuncConfig, formData);
                #endregion

                #region 消息提醒

                var messageDic = GroupByOperator(flowTaskOperatorEntityList);
                //审批
                foreach (var item in messageDic.Keys)
                {
                    var userList = messageDic[item].Select(x => x.HandleId).ToList();
                    //委托人
                    var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
                    userList = userList.Union(delegateUserIds).ToList();
                    bodyDic = GetMesBodyText(flowEngineEntity, item, userList, messageDic[item], 2);
                    await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
                    await StationLetterMsg(flowTaskEntity.FullName, userList, 2, bodyDic);
                    await Alerts(startApproversProperties.waitMsgConfig, userList, formData);
                    if (approversProperties.rejectMsgConfig.IsNotEmptyOrNull())
                    {
                        approversProperties.rejectMsgConfig = approversProperties.rejectMsgConfig.on == 2 ? startApproversProperties.rejectMsgConfig : approversProperties.rejectMsgConfig;
                    }
                    await Alerts(approversProperties.rejectMsgConfig, userList, formData);
                }

                #endregion
            }
            //抄送
            var userIdList = flowTaskCirculateEntityList.Select(x => x.ObjectId).ToList();
            if (userIdList.Count > 0)
            {
                bodyDic = GetMesBodyText(flowEngineEntity, flowTaskNodeEntity.Id, userIdList, null, 3, flowTaskOperatorEntity.Id);
                await StationLetterMsg(flowTaskEntity.FullName, userIdList, 3, bodyDic);
                approversProperties.copyMsgConfig = approversProperties.copyMsgConfig.on == 2 ? startApproversProperties.copyMsgConfig : approversProperties.copyMsgConfig;
                await Alerts(approversProperties.copyMsgConfig, userIdList, formData);
            }

            if (flowTaskEntity.Status == FlowTaskStatusEnum.Reject.ParseToInt())
            {
                bodyDic = GetMesBodyText(flowEngineEntity, flowTaskNodeEntity.Id, new List<string> { flowTaskEntity.CreatorUserId }, null, 2, flowTaskOperatorEntity.Id);
                await StationLetterMsg(flowTaskEntity.FullName, new List<string> { flowTaskEntity.CreatorUserId }, 2, bodyDic);
                if (approversProperties.rejectMsgConfig.IsNotEmptyOrNull())
                {
                    approversProperties.rejectMsgConfig = approversProperties.rejectMsgConfig.on == 2 ? startApproversProperties.rejectMsgConfig : approversProperties.rejectMsgConfig;
                }
                await Alerts(approversProperties.rejectMsgConfig, new List<string> { flowTaskEntity.CreatorUserId }, formData);
            }
            #endregion
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
        }
    }

    /// <summary>
    /// 审批(撤回).
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">撤回参数.</param>
    public async Task Recall(string id, FlowHandleModel flowHandleModel)
    {
        try
        {
            _db.BeginTran();
            //撤回经办记录
            var flowTaskOperatorRecordEntity = await _flowTaskRepository.GetTaskOperatorRecordInfo(id);
            if (flowTaskOperatorRecordEntity.Status == -1)
                throw Oops.Oh(ErrorCode.WF0011);
            //撤回经办
            var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(flowTaskOperatorRecordEntity.TaskOperatorId);
            //撤回节点
            var flowTaskNodeEntity = await _flowTaskRepository.GetTaskNodeInfo(flowTaskOperatorRecordEntity.TaskNodeId);
            //撤回任务
            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorRecordEntity.TaskId);
            //所有节点
            var flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(x => x.TaskId == flowTaskOperatorRecordEntity.TaskId && x.State == "0");
            //所有经办
            var flowTaskOperatorEntityList = await _flowTaskRepository.GetTaskOperatorList(x => x.TaskId == flowTaskOperatorRecordEntity.TaskId && x.State == "0");
            //撤回节点属性
            var recallNodeProperties = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
            #region 撤回判断
            //拒绝不撤回
            if (flowTaskOperatorEntity.HandleStatus == 0)
                throw Oops.Oh(ErrorCode.WF0010);
            //任务待审状态才能撤回
            if (!(flowTaskEntity.EnabledMark == 1 && flowTaskEntity.Status == 1))
                throw Oops.Oh(ErrorCode.WF0011);
            //撤回节点下一节点已操作
            var recallNextOperatorList = flowTaskOperatorEntityList.FindAll(x => flowTaskNodeEntity.NodeNext.Contains(x.NodeCode));
            if (recallNextOperatorList.FindAll(x => x.Completion == 1 && x.HandleStatus == 1).Count > 0)
                throw Oops.Oh(ErrorCode.WF0011);
            #endregion

            #region 经办修改
            var delOperatorRecordIds = new List<string>();
            //加签人
            var upOperatorList = await GetOperator(flowTaskOperatorEntity.Id, new List<FlowTaskOperatorEntity>());

            flowTaskOperatorEntity.HandleStatus = null;
            flowTaskOperatorEntity.HandleTime = null;
            flowTaskOperatorEntity.Completion = 0;
            flowTaskOperatorEntity.State = "0";
            upOperatorList.Add(flowTaskOperatorEntity);

            foreach (var item in upOperatorList)
            {
                var operatorRecord = await _flowTaskRepository.GetTaskOperatorRecordInfo(x => x.TaskId == item.TaskId && x.TaskNodeId == item.TaskNodeId && x.TaskOperatorId == item.Id && x.Status != -1 && x.HandleStatus < 2);
                if (operatorRecord.IsNotEmptyOrNull())
                {
                    delOperatorRecordIds.Add(operatorRecord.Id);
                }
            }
            //撤回节点是否完成
            if (flowTaskNodeEntity.Completion == 1)
            {
                //撤回节点下一节点经办删除
                await _flowTaskRepository.DeleteTaskOperator(recallNextOperatorList.Select(x => x.Id).ToList());
                //或签经办全部撤回，会签撤回未处理的经办
                //撤回节点未审批的经办
                var notHanleOperatorList = flowTaskOperatorEntityList.FindAll(x => x.TaskNodeId == flowTaskOperatorRecordEntity.TaskNodeId && x.HandleStatus == null
                 && x.HandleTime == null);
                foreach (var item in notHanleOperatorList)
                {
                    item.Completion = 0;
                }
                upOperatorList = upOperatorList.Union(notHanleOperatorList).ToList();

                #region 更新撤回节点
                flowTaskNodeEntity.Completion = 0;
                await _flowTaskRepository.UpdateTaskNode(flowTaskNodeEntity);
                #endregion

                #region 更新任务流程
                flowTaskEntity.ThisStepId = GetRecallThisStepId(new List<FlowTaskNodeEntity>() { flowTaskNodeEntity }, flowTaskEntity.ThisStepId);
                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                flowTaskEntity.Completion = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>().progress.ParseToInt();
                flowTaskEntity.Status = FlowTaskStatusEnum.Handle.ParseToInt();
                await _flowTaskRepository.UpdateTask(flowTaskEntity);
                #endregion
            }

            foreach (var item in flowTaskNodeEntityList)
            {
                if (flowTaskNodeEntity.NodeNext.Contains(item.NodeCode))
                {
                    _flowTaskRepository.DeleteFlowCandidates(x => x.TaskNodeId == item.Id && x.HandleId == _userManager.UserId && x.TaskOperatorId == flowTaskOperatorEntity.Id);
                }
            }

            await _flowTaskRepository.UpdateTaskOperator(upOperatorList);
            #endregion

            #region 删除经办记录
            delOperatorRecordIds.Add(flowTaskOperatorRecordEntity.Id);
            await _flowTaskRepository.DeleteTaskOperatorRecord(delOperatorRecordIds);
            #endregion

            #region 撤回记录
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 3;
            flowTaskOperatorRecordEntity.NodeName = flowTaskNodeEntity.NodeName;
            flowTaskOperatorRecordEntity.TaskId = flowTaskEntity.Id;
            flowTaskOperatorRecordEntity.TaskNodeId = flowTaskOperatorRecordEntity.TaskNodeId;
            flowTaskOperatorRecordEntity.TaskOperatorId = flowTaskOperatorRecordEntity.Id;
            flowTaskOperatorRecordEntity.Status = 0;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion

            _db.CommitTran();
            #region 撤回事件
            globalTaskId = flowTaskNodeEntity.TaskId;
            globalTaskNodeId = flowTaskNodeEntity.Id;
            await RequestEvents(recallNodeProperties.recallFuncConfig, flowTaskEntity.FlowFormContentJson);
            #endregion
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw Oops.Oh(ex.Message);
        }
    }

    /// <summary>
    /// 流程撤回.
    /// </summary>
    /// <param name="flowTaskEntity">流程实例.</param>
    /// <param name="flowHandleModel">流程经办.</param>
    public async Task Revoke(FlowTaskEntity flowTaskEntity, string flowHandleModel)
    {
        try
        {
            _db.BeginTran();
            var starProperty = (await _flowTaskRepository.GetTaskNodeInfo(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType) && x.State == "0" && x.TaskId == flowTaskEntity.Id)).NodePropertyJson?.ToObject<StartProperties>();
            #region 撤回数据
            await _flowTaskRepository.DeleteFlowTaskAllData(flowTaskEntity.Id, false);
            #endregion

            #region 更新实例
            flowTaskEntity.ThisStepId = string.Empty;
            flowTaskEntity.ThisStep = "开始";
            flowTaskEntity.Completion = 0;
            flowTaskEntity.Status = FlowTaskStatusEnum.Revoke.ParseToInt();
            flowTaskEntity.StartTime = null;
            flowTaskEntity.EndTime = null;
            await _flowTaskRepository.UpdateTask(flowTaskEntity);
            #endregion

            #region 撤回记录
            FlowTaskOperatorRecordEntity flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
            flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel;
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 3;
            flowTaskOperatorRecordEntity.NodeName = "开始";
            flowTaskOperatorRecordEntity.TaskId = flowTaskEntity.Id;
            flowTaskOperatorRecordEntity.Status = 0;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion

            #region 撤回子流程任务
            var childTask = await _flowTaskRepository.GetTaskList(x => flowTaskEntity.Id == x.ParentId && x.DeleteMark == null);
            foreach (var item in childTask)
            {
                if (item.Status == 1)
                {
                    await this.Revoke(item, flowHandleModel);
                }
                await _flowTaskRepository.DeleteTask(item);
            }
            #endregion

            _db.CommitTran();

            #region 撤回事件
            globalTaskId = flowTaskEntity.Id;
            globalTaskNodeId = string.Empty;
            await RequestEvents(starProperty.flowRecallFuncConfig, flowTaskEntity.FlowFormContentJson);
            #endregion
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
        }
    }

    /// <summary>
    /// 终止.
    /// </summary>
    /// <param name="flowTaskEntity">流程实例.</param>
    /// <param name="flowHandleModel">流程经办.</param>
    public async Task Cancel(FlowTaskEntity flowTaskEntity, FlowHandleModel flowHandleModel)
    {
        try
        {
            _db.BeginTran();
            #region 更新实例
            flowTaskEntity.Status = FlowTaskStatusEnum.Cancel.ParseToInt();
            flowTaskEntity.EndTime = DateTime.Now;
            await _flowTaskRepository.UpdateTask(flowTaskEntity);
            #endregion

            #region 作废记录
            FlowTaskOperatorRecordEntity flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
            flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 4;
            flowTaskOperatorRecordEntity.NodeName = flowTaskEntity.ThisStep;
            flowTaskOperatorRecordEntity.TaskId = flowTaskEntity.Id;
            flowTaskOperatorRecordEntity.Status = 0;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion
            _db.CommitTran();

        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 指派.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <param name="flowHandleModel">指派参数.</param>
    /// <returns></returns>
    public async Task Assigned(string id, FlowHandleModel flowHandleModel)
    {
        try
        {
            _db.BeginTran();
            var flowOperatorEntityList = await _flowTaskRepository.GetTaskOperatorList(x => x.State == "0" && x.NodeCode == flowHandleModel.nodeCode && x.TaskId == id);
            await _flowTaskRepository.DeleteTaskOperator(flowOperatorEntityList.Select(x => x.Id).ToList());
            var entity = new FlowTaskOperatorEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                HandleId = flowHandleModel.freeApproverUserId,
                HandleType = flowOperatorEntityList.FirstOrDefault().HandleType,
                NodeCode = flowOperatorEntityList.FirstOrDefault().NodeCode,
                NodeName = flowOperatorEntityList.FirstOrDefault().NodeName,
                CreatorTime = DateTime.Now,
                TaskId = flowOperatorEntityList.FirstOrDefault().TaskId,
                TaskNodeId = flowOperatorEntityList.FirstOrDefault().TaskNodeId,
                Type = flowOperatorEntityList.FirstOrDefault().Type,
                Completion = 0,
                State = "0"
            };
            var isOk = await _flowTaskRepository.CreateTaskOperator(entity);
            if (!isOk)
                throw Oops.Oh(ErrorCode.WF0008);

            #region 流转记录
            var flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
            flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 5;
            flowTaskOperatorRecordEntity.NodeName = entity.NodeName;
            flowTaskOperatorRecordEntity.TaskId = entity.TaskId;
            flowTaskOperatorRecordEntity.Status = 0;
            flowTaskOperatorRecordEntity.OperatorId = flowHandleModel.freeApproverUserId;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion
            _db.CommitTran();

            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(entity.TaskId);
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            globalTaskId = flowTaskEntity.Id;
            globalTaskNodeId = entity.TaskNodeId;
            var startApproversProperties = (await _flowTaskRepository.GetTaskNodeInfo(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType) && x.TaskId == entity.TaskId)).NodePropertyJson.ToObject<StartProperties>();
            var userList = new List<string>() { flowHandleModel.freeApproverUserId };
            //委托人
            var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
            userList = userList.Union(delegateUserIds).ToList();
            var bodyDic = GetMesBodyText(flowEngineEntity, entity.TaskNodeId, userList, new List<FlowTaskOperatorEntity>() { entity }, 2);
            await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
            await Alerts(startApproversProperties.waitMsgConfig, userList, flowTaskEntity.FlowFormContentJson);
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 转办.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">转办参数.</param>
    /// <returns></returns>
    public async Task Transfer(string id, FlowHandleModel flowHandleModel)
    {
        try
        {
            _db.BeginTran();
            var flowOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(id);
            if (flowOperatorEntity == null)
                throw Oops.Oh(ErrorCode.COM1005);
            flowOperatorEntity.HandleId = flowHandleModel.freeApproverUserId;
            var isOk = await _flowTaskRepository.UpdateTaskOperator(flowOperatorEntity);
            if (!isOk)
                throw Oops.Oh(ErrorCode.WF0007);

            #region 流转记录
            var flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
            flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
            flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
            flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
            flowTaskOperatorRecordEntity.HandleStatus = 7;
            flowTaskOperatorRecordEntity.NodeName = flowOperatorEntity.NodeName;
            flowTaskOperatorRecordEntity.TaskId = flowOperatorEntity.TaskId;
            flowTaskOperatorRecordEntity.Status = 0;
            flowTaskOperatorRecordEntity.OperatorId = flowHandleModel.freeApproverUserId;
            await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
            #endregion
            _db.CommitTran();
            globalTaskId = flowOperatorEntity.TaskId;
            globalTaskNodeId = flowOperatorEntity.TaskNodeId;
            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowOperatorEntity.TaskId);
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            var startApproversProperties = (await _flowTaskRepository.GetTaskNodeInfo(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType) && x.TaskId == flowOperatorEntity.TaskId)).NodePropertyJson.ToObject<StartProperties>();
            var userList = new List<string>() { flowHandleModel.freeApproverUserId };
            //委托人
            var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
            userList = userList.Union(delegateUserIds).ToList();
            var bodyDic = GetMesBodyText(flowEngineEntity, flowOperatorEntity.TaskNodeId, userList, new List<FlowTaskOperatorEntity>() { flowOperatorEntity }, 2);
            await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
            await Alerts(startApproversProperties.waitMsgConfig, userList, flowTaskEntity.FlowFormContentJson);
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 催办.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <returns></returns>
    public async Task Press(string id)
    {
        try
        {
            _db.BeginTran();
            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(id);
            var flowTaskOperatorEntityList = await _flowTaskRepository.GetTaskOperatorList(x => x.TaskId == id && x.Completion == 0 && x.State == "0");
            if (flowTaskOperatorEntityList.Any(x => x.HandleId.IsNullOrEmpty()))
                throw Oops.Oh(ErrorCode.WF0009);
            _db.CommitTran();

            globalTaskId = flowTaskEntity.Id;
            var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            var bodyDic = new Dictionary<string, object>();
            var messageDic = GroupByOperator(flowTaskOperatorEntityList);
            var startApproversProperties = (await _flowTaskRepository.GetTaskNodeInfo(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType) && x.TaskId == flowTaskEntity.Id)).NodePropertyJson.ToObject<StartProperties>();
            foreach (var item in messageDic.Keys)
            {
                var node = await _flowTaskRepository.GetTaskNodeInfo(item);
                globalTaskNodeId = node.Id;
                var nodeProperties = node.NodePropertyJson.ToObject<ApproversProperties>();
                var userList = messageDic[item].Select(x => x.HandleId).ToList();
                //委托人
                var delegateUserIds = await _flowTaskRepository.GetDelegateUserIds(userList, flowEngineEntity.Id);
                userList = userList.Union(delegateUserIds).ToList();
                bodyDic = GetMesBodyText(flowEngineEntity, node.Id, userList, messageDic[item], 2);
                await StationLetterMsg(flowTaskEntity.FullName, userList, 0, bodyDic);
                await Alerts(startApproversProperties.waitMsgConfig, userList, flowTaskEntity.FlowFormContentJson);
            }
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 审批事前操作.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    public async Task ApproveBefore(FlowEngineEntity flowEngineEntity, FlowTaskEntity flowTaskEntity, FlowHandleModel flowHandleModel)
    {
        try
        {
            if (flowEngineEntity.FormType == 2)
            {
                var data = (flowHandleModel.formData.ToObject<JObject>())["data"].ToString().ToObject<JObject>();
                var devData = (flowHandleModel.formData.ToObject<JObject>())["data"].ToString();
                var devEntity = await _flowTaskRepository.GetVisualDevInfo(flowEngineEntity.Id);
                var upInput = new VisualDevModelDataUpInput() { id = flowTaskEntity.Id, data = devData, status = 1 };
                await Save(flowTaskEntity.Id, flowTaskEntity.FlowId, flowTaskEntity.ProcessId, flowTaskEntity.FullName, flowTaskEntity.FlowUrgent, flowTaskEntity.EnCode, devData.ToObject(), 1, 1, false, "0", null, devEntity.IsNotEmptyOrNull());
                if (devEntity.IsNotEmptyOrNull())
                {
                    await _runService.Update(flowTaskEntity.Id, devEntity, upInput);
                }
            }
            else
            {
                flowTaskEntity.FlowFormContentJson = flowHandleModel.formData.ToJsonString();
                await _flowTaskRepository.UpdateTask(flowTaskEntity);
                GetSysTableFromService(flowHandleModel.enCode, flowHandleModel.formData, flowTaskEntity.Id, 0);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 获取候选人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="type">0:候选节点编码，1：候选人.</param>
    /// <returns></returns>
    public async Task<dynamic> GetCandidateModelList(string id, FlowHandleModel flowHandleModel, int type = 0)
    {
        var output = new List<FlowTaskCandidateModel>();
        //所有节点
        List<FlowTaskNodeEntity> flowTaskNodeEntityList = new List<FlowTaskNodeEntity>();
        //下个节点集合
        List<FlowTaskNodeEntity> nextNodeEntityList = new List<FlowTaskNodeEntity>();
        //指定下个节点
        FlowTaskNodeEntity nextNodeEntity = new FlowTaskNodeEntity();
        var jobj = flowHandleModel.formData.ToJsonString().ToObject<JObject>();
        if (!jobj.ContainsKey("flowId"))
        {
            return output;
        }
        var flowId = jobj["flowId"].ToString();
        var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);
        var formData = flowEngineEntity.FormType == 2 ? jobj["data"].ToString().ToObject() : flowHandleModel.formData;
        if (id == "0")
        {
            //所有节点
            flowTaskNodeEntityList = ParsingTemplateGetNodeList(flowEngineEntity, formData.ToJsonString(), string.Empty);
            var startTaskNodeEntity = flowTaskNodeEntityList.Find(m => FlowTaskNodeTypeEnum.start.ParseToString().Equals(m.NodeType));
            nextNodeEntityList = flowTaskNodeEntityList.FindAll(m => startTaskNodeEntity.NodeNext.Contains(m.NodeCode));
        }
        else
        {
            var flowTaskOperator = await _flowTaskRepository.GetTaskOperatorInfo(id);
            if (flowTaskOperator.ParentId.IsNotEmptyOrNull() && type == 0)
            {
                return output;
            }
            var flowTaskNodeEntity = await _flowTaskRepository.GetTaskNodeInfo(flowTaskOperator.TaskNodeId);
            flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(x => x.State == "0" && x.TaskId == flowTaskOperator.TaskId);
            nextNodeEntityList = flowTaskNodeEntityList.FindAll(m => flowTaskNodeEntity.NodeNext.Contains(m.NodeCode));
        }
        nextNodeEntity = flowTaskNodeEntityList.Find(x => x.NodeCode.Equals(flowHandleModel.nodeCode));
        if (type == 1)
        {
            return await GetCandidateItems(nextNodeEntity, flowHandleModel);
        }
        await GetCandidates(output, nextNodeEntityList, flowTaskNodeEntityList);
        return output;
    }

    /// <summary>
    /// 批量审批节点列表.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <returns></returns>
    public async Task<dynamic> NodeSelector(string flowId)
    {
        var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);
        var taskNodeList = ParsingTemplateGetNodeList(flowEngineEntity, null, string.Empty);
        return taskNodeList.FindAll(x => FlowTaskNodeTypeEnum.approver.ParseToString().Equals(x.NodeType)).Select(x => new { id = x.NodeCode, fullName = x.NodePropertyJson.ToObject<ApproversProperties>().title }).ToList();
    }

    /// <summary>
    /// 获取批量审批候选人.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <param name="flowTaskOperatorId">经办id.</param>
    /// <returns></returns>
    public async Task<dynamic> GetBatchCandidate(string flowId, string flowTaskOperatorId)
    {
        //所有节点
        var flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);
        var taskNodeList = ParsingTemplateGetNodeList(flowEngineEntity, null, string.Empty);
        var flowTaskOperator = await _flowTaskRepository.GetTaskOperatorInfo(flowTaskOperatorId);
        // 当前经办节点实例
        var node = await _flowTaskRepository.GetTaskNodeInfo(flowTaskOperator.TaskNodeId);
        var ids = node.NodeNext.Split(",").ToList();
        // 判断当前节点下节点是否属于条件之下
        var flag1 = taskNodeList.Any(x => FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.NodeType) && ids.Intersect(x.NodeNext.Split(",").ToList()).ToList().Count > 0);
        // 判断当前节点下节点是否包含候选人节点
        var flag2 = taskNodeList.Any(x => ids.Contains(x.NodeCode) && FlowTaskNodeTypeEnum.approver.ParseToString().Equals(x.NodeType) && x.NodePropertyJson.ToObject<ApproversProperties>().assigneeType == 7);
        if (flag1 && flag2)
        {
            throw Oops.Oh(ErrorCode.WF0022);
        }
        var model = new FlowHandleModel
        {
            nodeCode = flowTaskOperator.NodeCode,
            formData = new { flowId = flowId, data = "{}", id = flowTaskOperator.TaskId }
        };
        return await GetCandidateModelList(flowTaskOperatorId, model);
    }

    /// <summary>
    /// 审批根据条件变更节点.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <returns></returns>
    public async Task AdjustNodeByCon(FlowEngineEntity flowEngineEntity, object formData, FlowTaskOperatorEntity flowTaskOperatorEntity)
    {
        var taskNodeList = GetTaskNodeModelList(flowEngineEntity, flowTaskOperatorEntity.TaskId);

        // 判断当前审批节点下节点是否是条件
        if (taskNodeList.Any(x => x.upNodeId == flowTaskOperatorEntity.NodeCode && FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type)))
        {
            var data = flowEngineEntity.FormType == 2 ? formData.ToJsonString().ToObject<JObject>()["data"].ToString() : formData.ToJsonString();
            DeleteConditionTaskNodeModel(taskNodeList, data, flowTaskOperatorEntity.TaskId);
            var flowNodeList = new List<FlowTaskNodeEntity>();
            foreach (var item in taskNodeList)
            {
                var flowTaskNodeEntity = new FlowTaskNodeEntity();
                flowTaskNodeEntity.Id = SnowflakeIdHelper.NextId();
                flowTaskNodeEntity.CreatorTime = DateTime.Now;
                flowTaskNodeEntity.TaskId = item.taskId;
                flowTaskNodeEntity.NodeCode = item.nodeId;
                flowTaskNodeEntity.NodeType = item.type;
                flowTaskNodeEntity.Completion = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? 1 : 0;
                flowTaskNodeEntity.NodeName = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? "开始" : item.propertyJson.title;
                flowTaskNodeEntity.NodeUp = !FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.type) ? null : item.propertyJson.rejectStep;
                flowTaskNodeEntity.NodeNext = item.nextNodeId;
                flowTaskNodeEntity.NodePropertyJson = JsonHelper.ToJsonString(item.propertyJson);
                flowTaskNodeEntity.State = "-2";
                flowNodeList.Add(flowTaskNodeEntity);
            }
            DeleteTimerTaskNode(flowNodeList);

            var nodeList = await _flowTaskRepository.GetTaskNodeList(flowTaskOperatorEntity.TaskId);
            foreach (var item in nodeList)
            {
                var node = flowNodeList.FirstOrDefault(x => x.NodeCode == item.NodeCode);
                item.NodeNext = node.NodeNext;
                item.SortCode = node.SortCode;
                item.State = node.State;
            }
            await _flowTaskRepository.UpdateTaskNode(nodeList);
        }
    }

    /// <summary>
    /// 判断驳回节点是否存在子流程.
    /// </summary>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <returns></returns>
    public async Task<bool> IsSubFlowUpNode(FlowTaskOperatorEntity flowTaskOperatorEntity)
    {
        var nodeList = await _flowTaskRepository.GetTaskNodeList(flowTaskOperatorEntity.TaskId);
        var nodeInfo = await _flowTaskRepository.GetTaskNodeInfo(flowTaskOperatorEntity.TaskNodeId);
        if (nodeInfo.NodeUp == "0")
        {
            return false;
        }
        else
        {
            var rejectNodeList = GetRejectFlowTaskOperatorEntity(nodeList, nodeInfo, nodeInfo.NodePropertyJson.ToObject<ApproversProperties>());
            return rejectNodeList.Any(x => x.NodeType.Equals("subFlow"));
        }
    }

    /// <summary>
    /// 获取批量任务的表单数据.
    /// </summary>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    public async Task<object> GetBatchOperationData(string taskOperatorId)
    {
        var taskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(taskOperatorId);
        var taskEntity = await _flowTaskRepository.GetTaskInfo(taskOperatorEntity.TaskId);
        var flowEngine = await _flowTaskRepository.GetEngineInfo(taskEntity.FlowId);
        if (flowEngine.FormType == 1)
        {
            return taskEntity.FlowFormContentJson.ToObject();
        }
        else
        {
            return new { flowId = taskEntity.Id, data = taskEntity.FlowFormContentJson, id = taskEntity.Id };
        }
    }

    /// <summary>
    /// 详情操作验证.
    /// </summary>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    public async Task Validation(string taskOperatorId)
    {
        var taskOperator = await _flowTaskRepository.GetTaskOperatorInfo(taskOperatorId);
        if (taskOperator.IsNullOrEmpty() || taskOperator.Completion != 0 || taskOperator.State == "-1")
            throw Oops.Oh(ErrorCode.WF0030);
        var task = await _flowTaskRepository.GetTaskInfo(taskOperator.TaskId);
        if (task.IsNullOrEmpty() || task.Status != 1 || task.DeleteMark.IsNotEmptyOrNull())
            throw Oops.Oh(ErrorCode.WF0030);
        if (taskOperator.HandleId != _userManager.UserId)
        {
            var toUserId = _flowTaskRepository.GetToUserId(taskOperator.HandleId, task.FlowId);
            if (!toUserId.Contains(_userManager.UserId))
                throw Oops.Oh(ErrorCode.WF0030);
        }
    }
    #endregion

    #region PrivateMethod
    #region 流程模板解析
    /// <summary>
    /// 递归获取流程模板数组.
    /// </summary>
    /// <param name="template">流程模板.</param>
    /// <param name="templateList">流程模板数组.</param>
    private void GetFlowTemplateList(FlowTemplateJsonModel template, List<FlowTemplateJsonModel> templateList)
    {
        if (template.IsNotEmptyOrNull())
        {
            var haschildNode = template.childNode.IsNotEmptyOrNull();
            var hasconditionNodes = template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0;

            templateList.Add(template);

            if (hasconditionNodes)
            {
                foreach (var conditionNode in template.conditionNodes)
                {
                    GetFlowTemplateList(conditionNode, templateList);
                }
            }

            if (haschildNode)
            {
                GetFlowTemplateList(template.childNode, templateList);
            }
        }
    }

    /// <summary>
    /// 递归获取流程模板最外层childNode中所有nodeid.
    /// </summary>
    /// <param name="template">流程模板实例.</param>
    /// <param name="childNodeIdList">子节点id.</param>
    private void GetChildNodeIdList(FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        if (template.IsNotEmptyOrNull() && template.childNode.IsNotEmptyOrNull())
        {
            childNodeIdList.Add(template.childNode.nodeId);
            GetChildNodeIdList(template.childNode, childNodeIdList);
        }
    }

    /// <summary>
    /// 递归审批模板获取所有节点.
    /// </summary>
    /// <param name="template">当前审批流程json.</param>
    /// <param name="nodeList">流程节点数组.</param>
    /// <param name="templateList">流程模板数组.</param>
    private void GetFlowTemplateAll(FlowTemplateJsonModel template, List<TaskNodeModel> nodeList, List<FlowTemplateJsonModel> templateList, List<string> childNodeIdList, string taskId = "")
    {
        try
        {
            if (template.IsNotEmptyOrNull())
            {
                var taskNodeModel = template.Adapt<TaskNodeModel>();
                taskNodeModel.taskId = taskId;
                taskNodeModel.propertyJson = GetPropertyByType(template.type, template.properties);
                var haschildNode = template.childNode.IsNotEmptyOrNull();
                var hasconditionNodes = template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0;
                List<string> nextNodeIdList = new List<string> { string.Empty };
                if (templateList.Count > 1)
                {
                    nextNodeIdList = GetNextNodeIdList(templateList, template, childNodeIdList);
                }
                taskNodeModel.nextNodeId = string.Join(',', nextNodeIdList.ToArray());
                nodeList.Add(taskNodeModel);

                if (hasconditionNodes)
                {
                    foreach (var conditionNode in template.conditionNodes)
                    {
                        GetFlowTemplateAll(conditionNode, nodeList, templateList, childNodeIdList, taskId);
                    }
                }

                if (haschildNode)
                {
                    taskNodeModel.childNodeId = template.childNode.nodeId;
                    GetFlowTemplateAll(template.childNode, nodeList, templateList, childNodeIdList, taskId);
                }
            }
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    /// <summary>
    /// 根据类型获取不同属性对象.
    /// </summary>
    /// <param name="type">属性类型.</param>
    /// <param name="jd">数据.</param>
    /// <returns></returns>
    private dynamic GetPropertyByType(string type, JObject jd)
    {
        switch (type)
        {
            case "approver":
                return jd.ToObject<ApproversProperties>();
            case "timer":
                return jd.ToObject<TimerProperties>();
            case "start":
                return jd.ToObject<StartProperties>();
            case "condition":
                return jd.ToObject<ConditionProperties>();
            case "subFlow":
                return jd.ToObject<ChildTaskProperties>();
            default:
                return jd;
        }
    }

    /// <summary>
    /// 获取当前模板的下一节点
    /// 下一节点数据来源：conditionNodes和childnode (conditionNodes优先级大于childnode)
    /// conditionNodes非空：下一节点则为conditionNodes数组中所有nodeID
    /// conditionNodes非空childNode非空：下一节点则为childNode的nodeId
    /// conditionNodes空childNode空则为最终节点(两种情况：当前模板属于conditionNodes的最终节点或childNode的最终节点)
    /// conditionNodes的最终节点:下一节点为与conditionNodes同级的childNode的nodeid,没有则继续递归，直到最外层的childNode
    /// childNode的最终节点直接为"".
    /// </summary>
    /// <param name="templateList">模板数组</param>
    /// <param name="template">当前模板</param>
    /// <param name="childNodeIdList">最外层childnode的nodeid集合</param>
    /// <returns></returns>
    private List<string> GetNextNodeIdList(List<FlowTemplateJsonModel> templateList, FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        List<string> nextNodeIdList = new List<string>();
        if (template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0)
        {
            nextNodeIdList = template.conditionNodes.Select(x => x.nodeId).ToList();
        }
        else
        {
            if (template.childNode.IsNotEmptyOrNull())
            {
                nextNodeIdList.Add(template.childNode.nodeId);
            }
            else
            {
                //判断是否是最外层的节点
                if (childNodeIdList.Contains(template.nodeId))
                {
                    nextNodeIdList.Add(string.Empty);
                }
                else
                {
                    //conditionNodes中最终节点
                    nextNodeIdList.Add(GetChildId(templateList, template, childNodeIdList));
                }
            }
        }
        return nextNodeIdList;
    }

    /// <summary>
    /// 递归获取conditionNodes最终节点下一节点.
    /// </summary>
    /// <param name="templateList">流程模板数组.</param>
    /// <param name="template">当前模板.</param>
    /// <param name="childNodeIdList">最外层childNode的节点数据.</param>
    /// <returns></returns>
    private string GetChildId(List<FlowTemplateJsonModel> templateList, FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        var prevModel = new FlowTemplateJsonModel();
        if (template.prevId.IsNotEmptyOrNull())
        {
            prevModel = templateList.Find(x => x.nodeId.Equals(template.prevId));
            if (prevModel.childNode.IsNotEmptyOrNull() && prevModel.childNode.nodeId != template.nodeId)
            {
                return prevModel.childNode.nodeId;
            }
            if (childNodeIdList.Contains(prevModel.nodeId))
            {
                return prevModel.childNode.IsNullOrEmpty() ? string.Empty : prevModel.childNode.nodeId;
            }
            else
            {
                return GetChildId(templateList, prevModel, childNodeIdList);
            }
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 删除条件节点
    /// 将条件的上非条件的节点的nextnode替换成当前条件的nextnode.
    /// </summary>
    /// <param name="taskNodeModelList">所有节点数据.</param>
    /// <param name="formDataJson">填写表单数据.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    private void DeleteConditionTaskNodeModel(List<TaskNodeModel> taskNodeModelList, string formDataJson, string taskId)
    {
        var conditionTaskNodeModelList = taskNodeModelList.FindAll(x => FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type));
        var dic = new Dictionary<string, List<TaskNodeModel>>();
        foreach (var item in conditionTaskNodeModelList.GroupBy(x => x.upNodeId))
        {
            dic.Add(item.Key, taskNodeModelList.FindAll(x => x.upNodeId == item.Key && FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type)));
        }
        //条件的默认情况判断（同层条件的父节点是一样的，只要非默认的匹配成功则不需要走默认的）
        var isDefault = new List<string>();
        foreach (var nodeId in dic.Keys)
        {
            foreach (var item in dic[nodeId])
            {
                //条件节点的父节点且为非条件的节点
                var upTaskNodeModel = taskNodeModelList.Find(x => x.nodeId == nodeId);
                if (FlowTaskNodeTypeEnum.condition.ParseToString().Equals(upTaskNodeModel.type))
                {
                    upTaskNodeModel = GetUpTaskNodeModelIsNotCondition(taskNodeModelList, upTaskNodeModel);
                }
                if (!item.propertyJson.isDefault && ConditionNodeJudge(formDataJson, item.propertyJson, taskId))
                {
                    upTaskNodeModel.nextNodeId = item.nextNodeId;
                    isDefault.Add(item.upNodeId);
                    break;
                }
                else
                {
                    if (!isDefault.Contains(item.upNodeId) && item.propertyJson.isDefault)
                    {
                        upTaskNodeModel.nextNodeId = item.nextNodeId;
                    }
                    else
                    {
                        upTaskNodeModel.nextNodeId = upTaskNodeModel.childNodeId.IsNotEmptyOrNull() ? upTaskNodeModel.childNodeId : FlowTaskNodeTypeEnum.end.ParseToString();
                    }
                }
            }
        }

        if (formDataJson.IsNotEmptyOrNull())
        {
            taskNodeModelList.RemoveAll(x => FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type));
        }
    }

    /// <summary>
    /// 向上递获取非条件的节点.
    /// </summary>
    /// <param name="taskNodeModelList">所有节点数据.</param>
    /// <param name="taskNodeModel">当前节点.</param>
    /// <returns></returns>
    private TaskNodeModel GetUpTaskNodeModelIsNotCondition(List<TaskNodeModel> taskNodeModelList, TaskNodeModel taskNodeModel)
    {
        var preTaskNodeModel = taskNodeModelList.Find(x => x.nodeId == taskNodeModel.upNodeId);
        if (FlowTaskNodeTypeEnum.condition.ParseToString().Equals(preTaskNodeModel.type))
        {
            return GetUpTaskNodeModelIsNotCondition(taskNodeModelList, preTaskNodeModel);
        }
        else
        {
            return preTaskNodeModel;
        }
    }

    /// <summary>
    /// 条件判断.
    /// </summary>
    /// <param name="formDataJson">表单填写数据.</param>
    /// <param name="conditionPropertie">条件属性.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    private bool ConditionNodeJudge(string formDataJson, ConditionProperties conditionPropertie, string taskId)
    {
        try
        {
            bool flag = false;
            StringBuilder expression = new StringBuilder();
            expression.AppendFormat("select * from base_user where  ");
            var formData = formDataJson.ToObject<JObject>();
            int i = 0;
            foreach (ConditionsItem flowNodeWhereModel in conditionPropertie.conditions)
            {
                var logic = flowNodeWhereModel.logic;
                var formValue = " ";
                if (formData.ContainsKey(flowNodeWhereModel.field))
                {
                    if (formData[flowNodeWhereModel.field] is JArray)
                    {
                        try
                        {
                            var jar = formData[flowNodeWhereModel.field].ToObject<List<string>>();
                            if (jar.Count > 0)
                            {
                                formValue = string.Join(",", jar);
                            }
                        }
                        catch (Exception)
                        {
                            var arr = formData[flowNodeWhereModel.field].ToObject<List<List<string>>>();
                            formValue = string.Join(",", arr.Select(x => string.Join(",", x)).ToList());
                        }
                    }
                    else
                    {
                        formValue = formData[flowNodeWhereModel.field].ToString();
                        SysWidgetFormValue(taskId, flowNodeWhereModel.qtKey, ref formValue);
                    }
                }
                var symbol = flowNodeWhereModel.symbol.Equals("==") ? "=" : flowNodeWhereModel.symbol;
                var value = " ";
                //数组类型控件
                var qtKeyList = new List<string>() { "cascader", "comSelect", "address", "currOrganize" };
                if (qtKeyList.Contains(flowNodeWhereModel.qtKey) && flowNodeWhereModel.filedValue.Count > 0)
                {
                    if (flowNodeWhereModel.qtKey.Equals("currOrganize"))
                    {
                        value = flowNodeWhereModel.filedValue[flowNodeWhereModel.filedValue.Count - 1];
                    }
                    else
                    {
                        value = string.Join(",", flowNodeWhereModel.filedValue);
                    }
                }
                else
                {
                    value = flowNodeWhereModel.filedValue.ToString();
                }

                if ("currentUser".Equals(value))
                {
                    value = _userManager.UserId;
                }

                if ("time".Equals(flowNodeWhereModel.qtKey))
                {
                    formValue = formValue.Replace(":", string.Empty);
                    value = value.Replace(":", string.Empty);
                }

                if (symbol.Equals("=") || symbol.Equals("<>"))
                {
                    expression.AppendFormat("('{0}'{1}'{2}')", formValue, symbol, value);
                }
                else if (symbol.Equals("like"))
                {
                    if (formValue.Length > value.Length)
                    {
                        expression.AppendFormat("('{0}' {1} '%{2}%')", formValue, symbol, value);
                    }
                    else
                    {
                        expression.AppendFormat("('{0}' {1} '%{2}%')", value, symbol, formValue);
                    }

                }
                else if (symbol.Equals("notLike"))
                {
                    if (formValue.Length > value.Length)
                    {
                        expression.AppendFormat("('{0}' {1} '%{2}%')", formValue, "not like", value);
                    }
                    else
                    {
                        expression.AppendFormat("('{0}' {1} '%{2}%')", value, "not like", formValue);
                    }

                }
                else
                {
                    expression.AppendFormat("({0}{1}{2})", formValue, symbol, value);
                }

                if (logic.IsNotEmptyOrNull() && i != conditionPropertie.conditions.Count - 1)
                {
                    expression.Append(" " + logic.Replace("&&", " and ").Replace("||", " or ") + " ");
                }

                i++;
            }

            flag = _dataBaseManager.WhereDynamicFilter(null, expression.ToString());
            return flag;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// 系统控件条件匹配数据转换.
    /// </summary>
    /// <param name="taskId">任务id</param>
    /// <param name="qtKey">条件匹配字段类型</param>
    /// <param name="formValue">条件匹配值</param>
    private void SysWidgetFormValue(string taskId, string qtKey, ref string formValue)
    {
        var taskEntity = _flowTaskRepository.GetTaskFirstOrDefault(taskId);
        if (taskEntity.IsNotEmptyOrNull())
        {
            var creatorUser = _usersService.GetInfoByUserId(taskEntity.CreatorUserId);
            switch (qtKey)
            {
                case "createUser":
                    formValue = taskEntity.CreatorUserId;
                    break;
                case "modifyUser":
                    if (taskEntity.LastModifyUserId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.UserId;
                    }

                    break;
                case "currOrganize":
                    if (creatorUser.OrganizeId.IsNotEmptyOrNull())
                    {
                        formValue = creatorUser.OrganizeId;
                    }

                    break;
                case "createTime":
                    formValue = ((DateTime)taskEntity.CreatorTime).ParseToUnixTime().ToString();
                    break;
                case "modifyTime":
                    if (taskEntity.LastModifyTime.IsNotEmptyOrNull())
                    {
                        formValue = DateTime.Now.ParseToUnixTime().ToString();
                    }

                    break;
                case "currPosition":
                    if (creatorUser.PositionId.IsNotEmptyOrNull())
                    {
                        formValue = creatorUser.PositionId;
                    }

                    break;
            }
        }
        else
        {
            switch (qtKey)
            {
                case "createUser":
                    formValue = _userManager.UserId;
                    break;
                case "modifyUser":
                    formValue = " ";
                    break;
                case "currOrganize":
                    if (_userManager.User.OrganizeId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.User.OrganizeId;
                    }

                    break;
                case "createTime":
                    formValue = DateTime.Now.ParseToUnixTime().ToString();
                    break;
                case "modifyTime":
                    formValue = "0";
                    break;
                case "currPosition":
                    if (_userManager.User.PositionId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.User.PositionId;
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// 删除定时器.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点</param>
    private void DeleteTimerTaskNode(List<FlowTaskNodeEntity> flowTaskNodeEntityList)
    {
        foreach (var item in flowTaskNodeEntityList)
        {
            if ("timer".Equals(item.NodeType))
            {
                // 下一节点为Timer类型节点的节点集合
                var taskNodeList = flowTaskNodeEntityList.FindAll(x => x.NodeNext.Contains(item.NodeCode));

                // Timer类型节点的下节点集合
                var nextTaskNodeList = flowTaskNodeEntityList.FindAll(x => item.NodeNext.Contains(x.NodeCode));

                // 保存定时器节点的上节点编码到属性中
                var timerProperties = item.NodePropertyJson.ToObject<TimerProperties>();
                timerProperties.upNodeCode = string.Join(",", taskNodeList.Select(x => x.NodeCode).ToArray());
                item.NodePropertyJson = timerProperties.ToJsonString();

                // 上节点替换NodeNext
                foreach (var taskNode in taskNodeList)
                {
                    var flowTaskNodeEntity = flowTaskNodeEntityList.Where(x => x.NodeCode == taskNode.NodeCode).FirstOrDefault();
                    flowTaskNodeEntity.NodeNext = item.NodeNext;
                }

                // 下节点添加定时器属性
                nextTaskNodeList.ForEach(nextNode =>
                {
                    var flowTaskNodeEntity = flowTaskNodeEntityList.Where(x => x.NodeCode == nextNode.NodeCode).FirstOrDefault();
                    if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(flowTaskNodeEntity.NodeType))
                    {
                        var properties = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
                        properties.timerList.Add(item.NodePropertyJson.ToObject<TimerProperties>());
                        flowTaskNodeEntity.NodePropertyJson = JsonHelper.ToJsonString(properties);
                    }
                });
            }
        }

        flowTaskNodeEntityList.RemoveAll(x => FlowTaskNodeTypeEnum.timer.ParseToString().Equals(x.NodeType));
        UpdateNodeSort(flowTaskNodeEntityList);
    }
    #endregion

    #region 审批人员

    /// <summary>
    /// 获取流程人员id.
    /// </summary>
    /// <param name="userId">发起人id.</param>
    /// <param name="type">流程人员类型.</param>
    /// <param name="managerLevel">主管层级.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="formField">变量控件.</param>
    /// <param name="formFieldType">变量控件类型.</param>
    /// <param name="flowTaskNodeEntities">任务节点list.</param>
    /// <param name="linkNodeCode">环节节点编码.</param>
    /// <param name="nextFlowTaskNodeEntity">下一节点实体.</param>
    /// <param name="url">接口地址.</param>
    /// <param name="assign">指定用户.</param>
    /// <param name="assignRole">指定角色.</param>
    /// <param name="assignPos">指定岗位.</param>
    /// <returns></returns>
    public async Task<List<string>> GetFlowUserId(
        string userId,
        int type,
        int managerLevel,
        string formData,
        string formField,
        int formFieldType,
        List<FlowTaskNodeEntity> flowTaskNodeEntities,
        string linkNodeCode,
        FlowTaskNodeEntity nextFlowTaskNodeEntity,
        string url,
        List<string> assign,
        List<string> assignRole,
        List<string> assignPos)
    {
        var userIdList = new List<string>();

        // 发起者本人.
        var userEntity = _usersService.GetInfoByUserId(userId);
        switch (type)
        {
            // 发起者主管
            case (int)FlowTaskOperatorEnum.LaunchCharge:
                var crDirector = await GetManagerByLevel(userEntity.ManagerId, managerLevel);
                if (crDirector.IsNullOrEmpty())
                    crDirector = CommonConst.SUPPER_ADMIN_ACCOUNT;
                userIdList.Add(crDirector);
                break;

            // 发起者本人
            case (int)FlowTaskOperatorEnum.InitiatorMe:
                userIdList.Add(userEntity.Id);
                break;

            // 部门主管
            case (int)FlowTaskOperatorEnum.DepartmentCharge:
                var organizeEntity = await _organizeService.GetInfoById(userEntity.OrganizeId);
                if (organizeEntity.ManagerId.IsNullOrEmpty())
                    organizeEntity.ManagerId = CommonConst.SUPPER_ADMIN_ACCOUNT;
                userIdList.Add(organizeEntity.ManagerId);
                break;

            // 表单变量
            case (int)FlowTaskOperatorEnum.VariableApprover:
                var jd = formData.ToObject<JObject>();
                var fieldValueList = new List<string>();
                if (jd.ContainsKey(formField))
                {
                    if (jd[formField] is JArray)
                    {
                        fieldValueList = jd[formField].ToObject<List<string>>();
                    }
                    else
                    {
                        fieldValueList = jd[formField].ToString().Split(",").ToList();
                    }
                }

                if (formFieldType == 2)
                {
                    // 获取指定部门下所有用户id
                    fieldValueList = _userRelationService.GetUserId("Organize", fieldValueList);
                }

                // 获取全部用户id
                var userList1 = await _usersService.GetUserListByExp(x => x.DeleteMark == null, u => new UserEntity() { Id = u.Id });

                // 利用list交集方法过滤非用户数据
                userIdList = userList1.Select(x => x.Id).Intersect(fieldValueList).ToList();
                if (userIdList.Count == 0)
                    userIdList.Add(CommonConst.SUPPER_ADMIN_ACCOUNT);
                break;

            // 环节(提交时下个节点是环节就跳过，审批则看环节节点是否是当前节点的上级)
            case (int)FlowTaskOperatorEnum.LinkApprover:
                if (flowTaskNodeEntities.Any(x => x.NodeCode.Equals(linkNodeCode) && x.SortCode < nextFlowTaskNodeEntity.SortCode))
                {
                    // 环节节点所有经办人(过滤掉加签人)
                    userIdList = (await _flowTaskRepository.GetTaskOperatorRecordList(x =>
                        x.TaskId == nextFlowTaskNodeEntity.TaskId && !SqlFunc.IsNullOrEmpty(x.NodeCode)
                        && x.NodeCode.Equals(linkNodeCode) && x.HandleStatus == 1 && x.Status == 0))
                        .Where(x => HasFreeApprover(x.TaskOperatorId).Result).Select(x => x.HandleId).Distinct().ToList();
                }

                break;

            // 接口(接口结构为{"code":200,"data":{"handleId":"admin"},"msg":""})
            case (int)FlowTaskOperatorEnum.ServiceApprover:
                try
                {
                    var data = await url.SetHeaders(new { Authorization = _userManager.ToKen }).GetAsStringAsync();
                    var result = data.ToObject<RESTfulResult<object>>();
                    if (result.IsNotEmptyOrNull())
                    {
                        var resultJobj = result.data.ToObject<JObject>();
                        if (result.code == 200)
                        {
                            var handleId = resultJobj["handleId"].IsNotEmptyOrNull() ? resultJobj["handleId"].ToString() : CommonConst.SUPPER_ADMIN_ACCOUNT;
                            var handleIdList = handleId.Split(",").ToList();
                            var userList2 = await _usersService.GetUserListByExp(x => x.DeleteMark == null, u => new UserEntity() { Id = u.Id });

                            // 利用list交集方法过滤非用户数据
                            userIdList = userList2.Select(x => x.Id).Intersect(handleIdList).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    userIdList.Add(CommonConst.SUPPER_ADMIN_ACCOUNT);
                    break;
                }

                break;

            // 候选人
            case (int)FlowTaskOperatorEnum.CandidateApprover:
                userIdList = _flowTaskRepository.GetFlowCandidates(nextFlowTaskNodeEntity.Id);
                break;
            default:
                userIdList = GetUserDefined(assign, assignRole, assignPos).ToList();
                if (userIdList.Count == 0)
                {
                    userIdList.Add(CommonConst.SUPPER_ADMIN_ACCOUNT);
                }
                break;
        }
        return userIdList.Distinct().ToList();

    }

    /// <summary>
    /// 根据类型获取审批人.
    /// </summary>
    /// <param name="flowTaskOperatorEntityList">审批人集合.</param>
    /// <param name="flowTaskNodeEntitieList">所有节点.</param>
    /// <param name="flowTaskNodeEntity">下个审批节点数据.</param>
    /// <param name="nextFlowTaskNodeEntity">当前审批节点数据.</param>
    /// <param name="creatorUserId">发起人.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="type">操作标识（0：提交，1：审批）.</param>
    /// <returns></returns>
    private async Task AddFlowTaskOperatorEntityByAssigneeType(
        List<FlowTaskOperatorEntity> flowTaskOperatorEntityList,
        List<FlowTaskNodeEntity> flowTaskNodeEntitieList,
        FlowTaskNodeEntity flowTaskNodeEntity,
        FlowTaskNodeEntity nextFlowTaskNodeEntity,
        string creatorUserId,
        string formData,
        int type = 1)
    {
        try
        {
            if (!FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(nextFlowTaskNodeEntity.NodeType) && !FlowTaskNodeTypeEnum.end.ParseToString().Equals(nextFlowTaskNodeEntity.NodeCode))
            {
                var approverPropertiers = nextFlowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
                // 创建人
                var userId = type == 0 ? _userManager.UserId : creatorUserId;
                var handleIds = await GetFlowUserId(userId, approverPropertiers.assigneeType, approverPropertiers.managerLevel, formData, approverPropertiers.formField,
                    approverPropertiers.formFieldType, flowTaskNodeEntitieList, approverPropertiers.nodeId, nextFlowTaskNodeEntity, approverPropertiers.getUserUrl,
                    approverPropertiers.approvers, approverPropertiers.approverRole, approverPropertiers.approverPos);
                foreach (var item in handleIds)
                {
                    FlowTaskOperatorEntity flowTaskOperatorEntity = new FlowTaskOperatorEntity();
                    flowTaskOperatorEntity.Id = SnowflakeIdHelper.NextId();
                    flowTaskOperatorEntity.HandleType = approverPropertiers.assigneeType.ToString();
                    flowTaskOperatorEntity.NodeCode = nextFlowTaskNodeEntity.NodeCode;
                    flowTaskOperatorEntity.NodeName = nextFlowTaskNodeEntity.NodeName;
                    flowTaskOperatorEntity.TaskNodeId = nextFlowTaskNodeEntity.Id;
                    flowTaskOperatorEntity.TaskId = nextFlowTaskNodeEntity.TaskId;
                    flowTaskOperatorEntity.CreatorTime = DateTime.Now;
                    flowTaskOperatorEntity.Completion = 0;
                    flowTaskOperatorEntity.State = "0";
                    flowTaskOperatorEntity.Description = GetTimerDate(approverPropertiers, flowTaskNodeEntity.NodeCode);
                    flowTaskOperatorEntity.Type = approverPropertiers.assigneeType.ToString();
                    flowTaskOperatorEntity.HandleId = item;
                    flowTaskOperatorEntityList.Add(flowTaskOperatorEntity);
                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.WF0013);
        }
    }

    /// <summary>
    /// 判断经办记录人是否加签且加签是否完成.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <returns></returns>
    private async Task<bool> HasFreeApprover(string id)
    {
        var entityList = await GetOperator(id, new List<FlowTaskOperatorEntity>());
        if (entityList.Count == 0)
        {
            return true;
        }
        else
        {
            return !entityList.Any(x => x.HandleStatus.IsEmpty() || x.HandleStatus == 0);
        }
    }

    /// <summary>
    /// 获取抄送人.
    /// </summary>
    /// <param name="approverPropertiers">节点属性.</param>
    /// <param name="flowTaskOperatorEntity">经办.</param>
    /// <param name="flowTaskCirculateEntityList">抄送list.</param>
    /// <param name="copyIds">自定义抄送.</param>
    /// <param name="hanlderState">审批状态.</param>
    private void GetflowTaskCirculateEntityList(ApproversProperties approverPropertiers, FlowTaskOperatorEntity flowTaskOperatorEntity, List<FlowTaskCirculateEntity> flowTaskCirculateEntityList, string copyIds, int hanlderState = 1)
    {
        var circulateUserList = copyIds.IsNotEmptyOrNull() ? copyIds.Split(",").ToList() : new List<string>();
        #region 抄送人
        if (hanlderState == 1)
        {
            var userList = GetUserDefined(approverPropertiers.circulateUser, approverPropertiers.circulateRole, approverPropertiers.circulatePosition);
            circulateUserList = circulateUserList.Union(userList).ToList();
        }

        foreach (var item in circulateUserList.Distinct())
        {
            flowTaskCirculateEntityList.Add(new FlowTaskCirculateEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                ObjectType = flowTaskOperatorEntity.HandleType,
                ObjectId = item,
                NodeCode = flowTaskOperatorEntity.NodeCode,
                NodeName = flowTaskOperatorEntity.NodeName,
                TaskNodeId = flowTaskOperatorEntity.TaskNodeId,
                TaskId = flowTaskOperatorEntity.TaskId,
                CreatorTime = DateTime.Now,
            });
        }
        #endregion
    }

    /// <summary>
    /// 获取自定义人员.
    /// </summary>
    /// <param name="userList">指定人.</param>
    /// <param name="roleList">指定角色.</param>
    /// <param name="posList">指定岗位.</param>
    /// <returns></returns>
    private List<string> GetUserDefined(List<string> userList, List<string> roleList, List<string> posList)
    {
        if (posList.IsNotEmptyOrNull() && posList.Count > 0)
        {
            foreach (var item in posList)
            {
                var userPosition = _userRelationService.GetUserId("Position", item);
                userList = userList.Union(userPosition).ToList();
            }
        }
        if (roleList.IsNotEmptyOrNull() && roleList.Count > 0)
        {
            foreach (var item in roleList)
            {
                var userRole = _userRelationService.GetUserId("Role", item);
                userList = userList.Union(userRole).ToList();
            }
        }
        return userList;
    }

    /// <summary>
    /// 获取自定义人员名称.
    /// </summary>
    /// <param name="userList">指定人.</param>
    /// <param name="roleList">指定角色.</param>
    /// <param name="posList">指定岗位.</param>
    /// <param name="userNameList">用户名.</param>
    /// <returns></returns>
    private async Task GetUserNameDefined(List<string> userList, List<string> roleList, List<string> posList, List<string> userNameList)
    {
        userList = GetUserDefined(userList, roleList, posList).Distinct().ToList();
        if (userList.Count == 0)
        {
            userList.Add(CommonConst.SUPPER_ADMIN_ACCOUNT);
        }
        //List<string> userNameList1 = new List<string>();
        var users = await _usersService.GetUserListByExp(x => userList.Contains(x.Id),x=> new UserEntity { Id = x.Id,RealName=x.RealName,Account=x.Account});
        foreach (var item in users)
        {
            var name = _usersService.GetUserName(item);
            if (name.IsNotEmptyOrNull())
                userNameList.Add(name);
        }
        //foreach (var item in userList)
        //{
        //    var name = await _usersService.GetUserName(item);
        //    if (name.IsNotEmptyOrNull())
        //        userNameList.Add(name);
        //}
    }

    /// <summary>
    /// 获取候选人节点信息.
    /// </summary>
    /// <param name="flowTaskCandidateModels">返回参数.</param>
    /// <param name="nextNodeEntities">下一节点集合.</param>
    /// <param name="nodeEntities">所有节点.</param>
    /// <returns></returns>
    private async Task GetCandidates(List<FlowTaskCandidateModel> flowTaskCandidateModels, List<FlowTaskNodeEntity> nextNodeEntities, List<FlowTaskNodeEntity> nodeEntities)
    {
        foreach (var item in nextNodeEntities)
        {
            if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.NodeType))
            {
                var candidateItem = new FlowTaskCandidateModel();
                var approverPropertiers = item.NodePropertyJson.ToObject<ApproversProperties>();
                if (approverPropertiers.assigneeType == 7)
                {
                    candidateItem.nodeId = item.NodeCode;
                    candidateItem.nodeName = item.NodeName;
                    flowTaskCandidateModels.Add(candidateItem);
                }
            }
            else if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(item.NodeType))
            {
                var subFlowNextNodes = nodeEntities.FindAll(m => item.NodeNext.Contains(m.NodeCode));
                await GetCandidates(flowTaskCandidateModels, subFlowNextNodes, nodeEntities);
            }
        }
    }

    /// <summary>
    /// 候选人员列表.
    /// </summary>
    /// <param name="nextNodeEntity"></param>
    /// <param name="flowHandleModel"></param>
    /// <returns></returns>
    private async Task<dynamic> GetCandidateItems(FlowTaskNodeEntity nextNodeEntity, FlowHandleModel flowHandleModel)
    {
        var approverPropertiers = nextNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
        var objIds = approverPropertiers.approverPos.Union(approverPropertiers.approverRole).ToList();
        return await _userRelationService.GetUserPage(approverPropertiers.approvers, objIds, flowHandleModel);
    }

    /// <summary>
    /// 获取级别主管
    /// </summary>
    /// <param name="managerId">主管id</param>
    /// <param name="level">级别</param>
    /// <returns></returns>
    private async Task<string> GetManagerByLevel(string managerId, int level)
    {
        --level;
        if (level == 0)
        {
            return managerId;
        }
        else
        {
            var manager = await _usersService.GetInfoByUserIdAsync(managerId);
            return manager.IsNullOrEmpty() ? string.Empty : await GetManagerByLevel(manager.ManagerId, level);
        }
    }

    /// <summary>
    /// 获取审批人名称.
    /// </summary>
    /// <param name="flowTaskNodeModel">当前节点.</param>
    /// <param name="flowTaskEntity">任务.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="flowTaskNodeEntities">所有节点.</param>
    /// <returns></returns>
    private async Task<string> GetApproverUserName(FlowTaskNodeModel flowTaskNodeModel, FlowTaskEntity flowTaskEntity, string formData, List<FlowTaskNodeEntity> flowTaskNodeEntities)
    {
        var userNameList = new List<string>();
        var creatorUser = await _usersService.GetInfoByUserIdAsync(flowTaskEntity.CreatorUserId);
        //var userName = await _usersService.GetUserName(creatorUser.Id);
        var userName = _usersService.GetUserName(creatorUser);
        if (flowTaskNodeModel.nodeType.Equals(FlowTaskNodeTypeEnum.start.ParseToString()))
        {
            var startProperties = flowTaskNodeModel.nodePropertyJson.ToObject<StartProperties>();
            if (startProperties.initiator.Count > 0 || startProperties.initiateRole.Count > 0 || startProperties.initiatePos.Count > 0)
            {
                await GetUserNameDefined(startProperties.initiator, startProperties.initiateRole, startProperties.initiatePos, userNameList);
            }
            else
            {
                userNameList.Add(userName);
            }
        }
        else if (flowTaskNodeModel.nodeType.Equals(FlowTaskNodeTypeEnum.subFlow.ParseToString()))
        {
            var subFlowProperties = flowTaskNodeModel.nodePropertyJson.ToObject<ChildTaskProperties>();
            var userIdList = await GetFlowUserId(creatorUser.Id, subFlowProperties.initiateType, subFlowProperties.managerLevel,
                formData, subFlowProperties.formField, subFlowProperties.formFieldType, flowTaskNodeEntities, subFlowProperties.nodeId,
                flowTaskNodeModel.Adapt<FlowTaskNodeEntity>(), subFlowProperties.getUserUrl, subFlowProperties.initiator,
                subFlowProperties.initiateRole, subFlowProperties.initiatePos);
            await GetUserNameDefined(userIdList, null, null, userNameList);
        }
        else
        {
            var approverProperties = flowTaskNodeModel.nodePropertyJson.ToObject<ApproversProperties>();
            var userIdList = await GetFlowUserId(creatorUser.Id, approverProperties.assigneeType, approverProperties.managerLevel,
                formData, approverProperties.formField, approverProperties.formFieldType, flowTaskNodeEntities, approverProperties.nodeId,
                flowTaskNodeModel.Adapt<FlowTaskNodeEntity>(), approverProperties.getUserUrl, approverProperties.approvers,
                approverProperties.approverRole, approverProperties.approverPos);
            await GetUserNameDefined(userIdList, null, null, userNameList);
        }

        return string.Join(",", userNameList.Distinct());
    }

    /// <summary>
    /// 获取定时器节点定时结束时间.
    /// </summary>
    /// <param name="approverPropertiers">定时器节点属性.</param>
    /// <param name="nodeCode">定时器节点编码.</param>
    /// <returns></returns>
    private string GetTimerDate(ApproversProperties approverPropertiers, string nodeCode)
    {
        var nowTime = DateTime.Now;
        if (approverPropertiers.timerList.Count > 0)
        {
            string upNodeStr = string.Join(",", approverPropertiers.timerList.Select(x => x.upNodeCode).ToArray());
            if (upNodeStr.Contains(nodeCode))
            {
                foreach (var item in approverPropertiers.timerList)
                {
                    var result = DateTime.Now.AddDays(item.day).AddHours(item.hour).AddMinutes(item.minute).AddSeconds(item.second);
                    if (result > nowTime)
                    {
                        nowTime = result;
                    }
                }

                return nowTime.ToString();
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region 节点处理

    /// <summary>
    /// 判断分流节点是否完成
    /// (因为分流节点最终只能是一个 所以只需判断下一节点中的其中一个的上节点完成情况).
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="nextNodeCode">下一个节点编码.</param>
    /// <param name="flowTaskNodeEntity">当前节点.</param>
    /// <returns></returns>
    private bool IsShuntNodeCompletion(List<FlowTaskNodeEntity> flowTaskNodeEntityList, string nextNodeCode, FlowTaskNodeEntity flowTaskNodeEntity)
    {
        var shuntNodeCodeList = flowTaskNodeEntityList.FindAll(x => x.NodeNext.IsNotEmptyOrNull() &&
        x.NodeCode != flowTaskNodeEntity.NodeCode && x.NodeNext.Contains(nextNodeCode) && x.Completion == 0);
        return shuntNodeCodeList.Count == 0;
    }

    /// <summary>
    /// 替换审批同意任务当前节点.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="nextNodeCodeList">替换数据.</param>
    /// <param name="thisStepId">源数据.</param>
    /// <returns></returns>
    private string GetThisStepId(List<FlowTaskNodeEntity> flowTaskNodeEntityList, List<string> nextNodeCodeList, string thisStepId)
    {
        var replaceNodeCodeList = new List<string>();
        nextNodeCodeList.ForEach(item =>
        {
            var nodeCode = new List<string>();
            var nodeEntityList = flowTaskNodeEntityList.FindAll(x => x.NodeNext.IsNotEmptyOrNull() && x.NodeNext.Contains(item));
            nodeCode = nodeEntityList.Select(x => x.NodeCode).ToList();
            replaceNodeCodeList = replaceNodeCodeList.Union(nodeCode).ToList();
        });
        var thisNodeList = new List<string>();
        if (thisStepId.IsNotEmptyOrNull())
        {
            thisNodeList = thisStepId.Split(",").ToList();
        }
        //去除当前审批节点并添加下个节点
        var list = thisNodeList.Except(replaceNodeCodeList).Union(nextNodeCodeList);
        return string.Join(",", list.ToArray());
    }

    /// <summary>
    /// 替换审批撤回当前任务节点.
    /// </summary>
    /// <param name="nextNodeCodeList">下一节点编码.</param>
    /// <param name="thisStepId">当前待处理节点.</param>
    /// <returns></returns>
    private string GetRecallThisStepId(List<FlowTaskNodeEntity> nextNodeCodeList, string thisStepId)
    {
        var replaceNodeCodeList = new List<string>();
        foreach (var item in nextNodeCodeList)
        {
            var nodeCode = item.NodeNext.Split(",").ToList();
            replaceNodeCodeList = replaceNodeCodeList.Union(nodeCode).ToList();
        }

        var thisNodeList = new List<string>();
        if (thisStepId.IsNotEmptyOrNull())
        {
            thisNodeList = thisStepId.Split(",").ToList();
        }
        //去除当前审批节点并添加下个节点
        var list = thisNodeList.Except(replaceNodeCodeList).Union(nextNodeCodeList.Select(x => x.NodeCode));
        return string.Join(",", list.ToArray());
    }

    /// <summary>
    /// 驳回替换任务当前节点.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">驳回节点下所有节点.</param>
    /// <param name="upNodeCodes">当前节点.</param>
    /// <param name="thisStepId">当前待处理节点.</param>
    /// <returns></returns>
    private string GetRejectThisStepId(List<FlowTaskNodeEntity> flowTaskNodeEntityList, List<string> upNodeCodes, string thisStepId)
    {
        // 驳回节点下所有节点编码
        var removeNodeCodeList = flowTaskNodeEntityList.Select(x => x.NodeCode).ToList();
        var ids = thisStepId.Split(",").ToList();
        var thisNodes = ids.Except(removeNodeCodeList).Union(upNodeCodes).ToList();
        return string.Join(",", thisNodes);
    }

    /// <summary>
    /// 根据当前节点编码获取节点名称.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="thisStepId">当前待处理节点.</param>
    /// <returns></returns>
    private string GetThisStep(List<FlowTaskNodeEntity> flowTaskNodeEntityList, string thisStepId)
    {
        var ids = thisStepId.Split(",").ToList();
        var nextNodeNameList = new List<string>();
        foreach (var item in ids)
        {
            var name = flowTaskNodeEntityList.Find(x => x.NodeCode.Equals(item)).NodeName;
            nextNodeNameList.Add(name);
        }
        return string.Join(",", nextNodeNameList);
    }

    /// <summary>
    /// 获取驳回节点.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="flowTaskNodeEntity">当前节点.</param>
    /// <param name="approversProperties">当前节点属性.</param>
    /// <returns></returns>
    private List<FlowTaskNodeEntity> GetRejectFlowTaskOperatorEntity(List<FlowTaskNodeEntity> flowTaskNodeEntityList, FlowTaskNodeEntity flowTaskNodeEntity, ApproversProperties approversProperties)
    {
        //驳回节点
        var upflowTaskNodeEntityList = new List<FlowTaskNodeEntity>();
        if (flowTaskNodeEntity.NodeUp == "1")
        {
            upflowTaskNodeEntityList = flowTaskNodeEntityList.FindAll(x => x.NodeNext.IsNotEmptyOrNull() && x.NodeNext.Contains(flowTaskNodeEntity.NodeCode));
        }
        else
        {
            var upflowTaskNodeEntity = flowTaskNodeEntityList.Find(x => x.NodeCode == approversProperties.rejectStep);
            upflowTaskNodeEntityList = flowTaskNodeEntityList.FindAll(x => x.SortCode == upflowTaskNodeEntity.SortCode);
        }
        return upflowTaskNodeEntityList;
    }

    /// <summary>
    /// 修改节点完成状态.
    /// </summary>
    /// <param name="taskNodeList">修改节点.</param>
    /// <param name="state">状态.</param>
    /// <returns></returns>
    private async Task RejectUpdateTaskNode(List<FlowTaskNodeEntity> taskNodeList, int state)
    {
        foreach (var item in taskNodeList)
        {
            item.Completion = state;
            await _flowTaskRepository.UpdateTaskNode(item);
        }
    }

    /// <summary>
    /// 处理并保存节点.
    /// </summary>
    /// <param name="entitys">节点list.</param>
    private void UpdateNodeSort(List<FlowTaskNodeEntity> entitys)
    {
        var startNodes = entitys.FindAll(x => x.NodeType.Equals("start"));
        if (startNodes.Count > 0)
        {
            var startNode = startNodes[0].NodeCode;
            long num = 0L;
            long maxNum = 0L;
            var max = new List<long>();
            var _treeList = new List<FlowTaskNodeEntity>();
            NodeList(entitys, startNode, _treeList, num, max);
            max.Sort();
            if (max.Count > 0)
            {
                maxNum = max[max.Count - 1];
            }
            var nodeNext = "end";
            foreach (var item in entitys)
            {
                var type = item.NodeType;
                var node = _treeList.Find(x => x.NodeCode.Equals(item.NodeCode));
                if (item.NodeNext.IsEmpty())
                {
                    item.NodeNext = nodeNext;
                }
                if (node.IsNotEmptyOrNull())
                {
                    item.SortCode = node.SortCode;
                    item.State = "0";
                    if (item.NodeNext.IsEmpty())
                    {
                        item.NodeNext = nodeNext;
                    }
                }
            }
            entitys.Add(new FlowTaskNodeEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                NodeCode = nodeNext,
                NodeName = "结束",
                Completion = 0,
                CreatorTime = DateTime.Now,
                SortCode = maxNum + 1,
                TaskId = _treeList[0].TaskId,
                NodePropertyJson = startNodes[0].NodePropertyJson,
                NodeType = "endround",
                State = "0"
            });
        }
    }

    /// <summary>
    /// 递归获取经过的节点.
    /// </summary>
    /// <param name="dataAll"></param>
    /// <param name="nodeCode"></param>
    /// <param name="_treeList"></param>
    /// <param name="num"></param>
    /// <param name="max"></param>
    private void NodeList(List<FlowTaskNodeEntity> dataAll, string nodeCode, List<FlowTaskNodeEntity> _treeList, long num, List<long> max)
    {
        num++;
        max.Add(num);
        foreach (var item in dataAll)
        {
            if (item.NodeCode.Contains(nodeCode))
            {
                item.SortCode = num;
                item.State = "0";
                _treeList.Add(item);
                foreach (var nodeNext in item.NodeNext.Split(","))
                {
                    long nums = _treeList.FindAll(x => x.NodeCode.Equals(nodeNext)).Count;
                    if (nodeNext.IsNotEmptyOrNull() && nums == 0)
                    {
                        NodeList(dataAll, nodeNext, _treeList, num, max);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 递归节点下所有节点.
    /// </summary>
    /// <param name="flowTaskNodeList">所有节点.</param>
    /// <param name="nodeNext">下一节点.</param>
    /// <param name="flowTaskNodeEntities">指定节点下所有节点.</param>
    private async Task GetAllNextNode(List<FlowTaskNodeEntity> flowTaskNodeList, string nodeNext, List<FlowTaskNodeEntity> flowTaskNodeEntities)
    {
        var nextNodes = nodeNext.Split(",").ToList();
        var flowTaskNodeEntityList = flowTaskNodeList.FindAll(x => nextNodes.Contains(x.NodeCode));
        flowTaskNodeEntities.AddRange(flowTaskNodeEntityList);
        foreach (var item in flowTaskNodeEntityList)
        {
            if (!FlowTaskNodeTypeEnum.end.ParseToString().Equals(item.NodeCode))
            {
                await GetAllNextNode(flowTaskNodeList, item.NodeNext, flowTaskNodeEntities);
            }
        }
    }

    /// <summary>
    /// 根据表单数据解析模板获取流程节点.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    public List<FlowTaskNodeEntity> ParsingTemplateGetNodeList(FlowEngineEntity flowEngineEntity, string formData, string taskId)
    {
        var flowTaskNodeEntityList = new List<FlowTaskNodeEntity>();
        var taskNodeList = GetTaskNodeModelList(flowEngineEntity, taskId);
        DeleteConditionTaskNodeModel(taskNodeList, formData, taskId);
        foreach (var item in taskNodeList)
        {
            var flowTaskNodeEntity = new FlowTaskNodeEntity();
            flowTaskNodeEntity.Id = SnowflakeIdHelper.NextId();
            flowTaskNodeEntity.CreatorTime = DateTime.Now;
            flowTaskNodeEntity.TaskId = item.taskId;
            flowTaskNodeEntity.NodeCode = item.nodeId;
            flowTaskNodeEntity.NodeType = item.type;
            flowTaskNodeEntity.Completion = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? 1 : 0;
            flowTaskNodeEntity.NodeName = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? "开始" : item.propertyJson.title;
            flowTaskNodeEntity.NodeUp = !FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.type) ? null : item.propertyJson.rejectStep;
            flowTaskNodeEntity.NodeNext = item.nextNodeId;
            flowTaskNodeEntity.NodePropertyJson = JsonHelper.ToJsonString(item.propertyJson);
            flowTaskNodeEntity.State = "-2";
            flowTaskNodeEntityList.Add(flowTaskNodeEntity);
        }

        DeleteTimerTaskNode(flowTaskNodeEntityList);
        return flowTaskNodeEntityList;
    }

    /// <summary>
    /// 解析流程.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    private List<TaskNodeModel> GetTaskNodeModelList(FlowEngineEntity flowEngineEntity, string taskId)
    {
        var taskNodeList = new List<TaskNodeModel>();
        var flowTemplateJsonModel = flowEngineEntity.FlowTemplateJson.ToObject<FlowTemplateJsonModel>();
        #region 流程模板所有节点
        var flowTemplateJsonModelList = new List<FlowTemplateJsonModel>();
        var childNodeIdList = new List<string>();
        GetChildNodeIdList(flowTemplateJsonModel, childNodeIdList);
        GetFlowTemplateList(flowTemplateJsonModel, flowTemplateJsonModelList);
        #endregion
        GetFlowTemplateAll(flowTemplateJsonModel, taskNodeList, flowTemplateJsonModelList, childNodeIdList, taskId);
        return taskNodeList;
    }
    #endregion

    #region 经办处理
    /// <summary>
    /// 根据不同节点类型修改经办数据.
    /// </summary>
    /// <param name="flowTaskOperatorEntity">当前经办.</param>
    /// <param name="thisFlowTaskOperatorEntityList">当前节点所有经办.</param>
    /// <param name="aspproversProperties">当前节点属性.</param>
    /// <param name="handleStatus">审批类型（0：拒绝，1：同意）.</param>
    /// <param name="freeApprover">加签人.</param>
    /// <returns></returns>
    private async Task UpdateFlowTaskOperator(FlowTaskOperatorEntity flowTaskOperatorEntity,
        List<FlowTaskOperatorEntity> thisFlowTaskOperatorEntityList, ApproversProperties aspproversProperties,
        int handleStatus, string freeApprover)
    {
        if (aspproversProperties.counterSign == 0)
        {
            if (freeApprover.IsNullOrEmpty())
            {
                //未审批经办
                var notCompletion = GetNotCompletion(thisFlowTaskOperatorEntityList);
                await _flowTaskRepository.UpdateTaskOperator(notCompletion);
            }
        }
        else
        {
            if (IsAchievebilProportion(thisFlowTaskOperatorEntityList, handleStatus, (int)aspproversProperties.countersignRatio, freeApprover.IsEmpty()))
            {
                //未审批经办
                var notCompletion = GetNotCompletion(thisFlowTaskOperatorEntityList);
                await _flowTaskRepository.UpdateTaskOperator(notCompletion);
            }
        }
        await UpdateThisOperator(flowTaskOperatorEntity, handleStatus);
    }

    /// <summary>
    /// 根据当前审批节点插入下一节点经办.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="flowTaskNodeEntity">当前节点.</param>
    /// <param name="approversProperties">当前节点属性.</param>
    /// <param name="thisFlowTaskOperatorEntityList">当前节点所有经办.</param>
    /// <param name="handleStatus">审批状态.</param>
    /// <param name="flowTaskEntity">流程任务.</param>
    /// <param name="freeApproverUserId">加签人.</param>
    /// <param name="nextFlowTaskOperatorEntityList">经办数据.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="flowHandleModel">审批详情.</param>
    /// <param name="formType">单据类型.</param>
    /// <returns></returns>
    private async Task CreateNextFlowTaskOperator(List<FlowTaskNodeEntity> flowTaskNodeEntityList, FlowTaskNodeEntity flowTaskNodeEntity,
        ApproversProperties approversProperties, List<FlowTaskOperatorEntity> thisFlowTaskOperatorEntityList, int handleStatus, FlowTaskEntity flowTaskEntity, string freeApproverUserId,
        List<FlowTaskOperatorEntity> nextFlowTaskOperatorEntityList, string formData, FlowHandleModel flowHandleModel, int formType)
    {
        try
        {
            var nextNodeCodeList = new List<string>();
            var nextNodeCompletion = new List<int>();
            var isInsert = false;
            //下个节点集合
            List<FlowTaskNodeEntity> nextNodeEntity = flowTaskNodeEntityList.FindAll(m => flowTaskNodeEntity.NodeNext.Contains(m.NodeCode));
            if (handleStatus == 0)
            {
                if (approversProperties.counterSign == 0)
                {
                    await GetNextOperatorByNo(flowTaskNodeEntity, flowTaskEntity, flowTaskNodeEntityList, approversProperties, nextFlowTaskOperatorEntityList, formData);
                }
                else
                {
                    if (IsAchievebilProportion(thisFlowTaskOperatorEntityList, handleStatus, (int)approversProperties.countersignRatio, freeApproverUserId.IsEmpty()))
                    {
                        await GetNextOperatorByNo(flowTaskNodeEntity, flowTaskEntity, flowTaskNodeEntityList, approversProperties, nextFlowTaskOperatorEntityList, formData);
                    }
                }
            }
            else
            {
                foreach (var item in nextNodeEntity)
                {
                    if (approversProperties.counterSign == 0)
                    {
                        isInsert = true;
                        await GetNextOperatorByYes(flowTaskNodeEntity, item, nextNodeCodeList, nextNodeCompletion, nextFlowTaskOperatorEntityList, flowTaskNodeEntityList, flowTaskEntity, formData);
                    }
                    else
                    {
                        if (IsAchievebilProportion(thisFlowTaskOperatorEntityList, handleStatus, (int)approversProperties.countersignRatio, freeApproverUserId.IsEmpty()))
                        {
                            isInsert = true;
                            await GetNextOperatorByYes(flowTaskNodeEntity, item, nextNodeCodeList, nextNodeCompletion, nextFlowTaskOperatorEntityList, flowTaskNodeEntityList, flowTaskEntity, formData);
                        }
                    }

                }
                #region 判断是否插入下个节点数据
                //下一节点是分流必定有审批人
                if (nextNodeEntity.Count > 1)
                {
                    flowTaskEntity.ThisStepId = GetThisStepId(flowTaskNodeEntityList, nextNodeCodeList, flowTaskEntity.ThisStepId);
                    flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                    flowTaskEntity.Completion = nextNodeCompletion.Count == 0 ? flowTaskEntity.Completion : nextNodeCompletion.Min();
                    await _flowTaskRepository.CreateTaskOperator(nextFlowTaskOperatorEntityList);
                }
                else
                {
                    //判断当前节点在不在分流当中且是否为分流的最后审批节点
                    var nextNode = nextNodeEntity.FirstOrDefault();
                    var isShuntNodeCompletion = IsShuntNodeCompletion(flowTaskNodeEntityList, nextNode.NodeCode, flowTaskNodeEntity);
                    if (nextFlowTaskOperatorEntityList.Count > 0)
                    {
                        if (isShuntNodeCompletion)
                        {
                            flowTaskEntity.ThisStepId = GetThisStepId(flowTaskNodeEntityList, nextNodeCodeList, flowTaskEntity.ThisStepId);
                            flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                            flowTaskEntity.Completion = nextNodeCompletion.Count == 0 ? flowTaskEntity.Completion : nextNodeCompletion.Min();
                            await _flowTaskRepository.CreateTaskOperator(nextFlowTaskOperatorEntityList);
                        }
                        else
                        {
                            if (FlowTaskNodeTypeEnum.end.ParseToString().Equals(nextNode.NodeCode))
                            {
                                flowTaskEntity.Status = FlowTaskStatusEnum.Adopt.ParseToInt();
                                flowTaskEntity.Completion = 100;
                                flowTaskEntity.EndTime = DateTime.Now;
                                flowTaskEntity.ThisStepId = FlowTaskNodeTypeEnum.end.ParseToString();
                                flowTaskEntity.ThisStep = "结束";
                            }
                            else
                            {
                                var thisStepIds = flowTaskEntity.ThisStepId.Split(",").ToList();
                                thisStepIds.Remove(flowTaskNodeEntity.NodeCode);

                                flowTaskEntity.ThisStepId = string.Join(",", thisStepIds.ToArray());
                                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                            }
                        }

                    }
                    else
                    {
                        //下一节点没有审批人(1.当前会签节点没结束，2.结束节点，3.子流程)
                        if (isShuntNodeCompletion)
                        {
                            var isLastEndNode = flowTaskNodeEntityList.FindAll(x =>
                        x.NodeNext.IsNotEmptyOrNull() && x.NodeNext.Equals(FlowTaskNodeTypeEnum.end.ParseToString())
                        && !x.NodeCode.Equals(flowTaskNodeEntity.NodeCode) && x.Completion == 0).Count == 0;
                            //下一节点是子流程
                            if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(nextNode.NodeType) && isInsert)
                            {
                                flowTaskEntity.ThisStepId = GetThisStepId(flowTaskNodeEntityList, nextNodeCodeList, flowTaskEntity.ThisStepId);
                                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                                flowTaskEntity.Completion = nextNodeCompletion.Count == 0 ? flowTaskEntity.Completion : nextNodeCompletion.Min();
                                var childTaskPro = nextNode.NodePropertyJson.ToObject<ChildTaskProperties>();
                                var childTaskCrUserList = await GetFlowUserId(flowTaskEntity.CreatorUserId, childTaskPro.initiateType, childTaskPro.managerLevel,
                formData, childTaskPro.formField, childTaskPro.formFieldType, flowTaskNodeEntityList, childTaskPro.nodeId,
                nextNode, childTaskPro.getUserUrl, childTaskPro.initiator,
                childTaskPro.initiateRole, childTaskPro.initiatePos);
                                var childFLowEngine = await _flowTaskRepository.GetEngineInfo(childTaskPro.flowId);
                                var isSysTable = childFLowEngine.FormType == 1;
                                var childFormData = await GetSubFlowFormData(childTaskPro, formData);
                                childTaskPro.childTaskId = await CreateSubProcesses(childTaskPro, childFormData, flowTaskEntity.Id, childTaskCrUserList);
                                childTaskPro.formData = formData;
                                nextNode.NodePropertyJson = childTaskPro.ToJsonString();
                                //将子流程id保存到主流程的子流程节点属性上
                                nextNode.Completion = childTaskPro.isAsync ? 1 : 0;
                                await _flowTaskRepository.UpdateTaskNode(nextNode);
                                await Alerts(childTaskPro.launchMsgConfig, childTaskCrUserList, formData);
                                if (childTaskPro.isAsync)
                                {
                                    flowTaskNodeEntityList.Remove(nextNodeEntity.FirstOrDefault());
                                    flowTaskNodeEntityList.Add(nextNode);
                                    await CreateNextFlowTaskOperator(flowTaskNodeEntityList, nextNode,
                                        nextNode.NodePropertyJson.ToObject<ApproversProperties>(), new List<FlowTaskOperatorEntity>(),
                                        handleStatus, flowTaskEntity, freeApproverUserId, nextFlowTaskOperatorEntityList, formData,
                                        flowHandleModel, formType);
                                }
                            }
                            else if (FlowTaskNodeTypeEnum.end.ParseToString().Equals(nextNode.NodeCode) && isLastEndNode && isInsert)
                            {
                                flowTaskEntity.Status = FlowTaskStatusEnum.Adopt.ParseToInt();
                                flowTaskEntity.Completion = 100;
                                flowTaskEntity.EndTime = DateTime.Now;
                                flowTaskEntity.ThisStepId = "end";
                                flowTaskEntity.ThisStep = "结束";
                            }
                            else
                            {
                                flowTaskEntity.ThisStepId = GetThisStepId(flowTaskNodeEntityList, nextNodeCodeList, flowTaskEntity.ThisStepId);
                                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                                flowTaskEntity.Completion = nextNodeCompletion.Count == 0 ? flowTaskEntity.Completion : nextNodeCompletion.Min();
                            }
                        }
                        else
                        {
                            if (FlowTaskNodeTypeEnum.end.ParseToString().Equals(nextNode.NodeCode) && isShuntNodeCompletion && isInsert)
                            {
                                flowTaskEntity.Status = FlowTaskStatusEnum.Adopt.ParseToInt();
                                flowTaskEntity.Completion = 100;
                                flowTaskEntity.EndTime = DateTime.Now;
                                flowTaskEntity.ThisStepId = "end";
                                flowTaskEntity.ThisStep = "结束";
                            }
                            else
                            {
                                var thisStepIds = flowTaskEntity.ThisStepId.Split(",").ToList();
                                thisStepIds.Remove(flowTaskNodeEntity.NodeCode);

                                flowTaskEntity.ThisStepId = string.Join(",", thisStepIds.ToArray());
                                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                            }
                        }
                    }
                }
                #endregion
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 修改当前经办以及所属委托经办.
    /// </summary>
    /// <param name="entity">经办实体.</param>
    /// <param name="handleStatus">审批意见.</param>
    /// <returns></returns>
    private async Task UpdateThisOperator(FlowTaskOperatorEntity entity, int handleStatus)
    {
        entity.HandleStatus = handleStatus;
        entity.Completion = 1;
        entity.HandleTime = DateTime.Now;
        await _flowTaskRepository.UpdateTaskOperator(entity);
    }

    /// <summary>
    /// 获取未审经办并修改完成状态.
    /// </summary>
    /// <param name="thisFlowTaskOperatorEntityList">当前节点所有经办.</param>
    /// <returns></returns>
    private List<FlowTaskOperatorEntity> GetNotCompletion(List<FlowTaskOperatorEntity> thisFlowTaskOperatorEntityList)
    {
        var notCompletion = thisFlowTaskOperatorEntityList.FindAll(x => x.Completion == 0);
        notCompletion.ForEach(item =>
        {
            item.Completion = 1;
        });
        return notCompletion;
    }

    /// <summary>
    /// 获取驳回节点经办.
    /// </summary>
    /// <param name="flowTaskNodeEntity">当前节点.</param>
    /// <param name="flowTaskEntity">当前任务.</param>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="approversProperties">当前节点属性.</param>
    /// <param name="nextFlowTaskOperatorEntityList">下个节点存储list.</param>
    /// <param name="formData">表单数据.</param>
    /// <returns></returns>
    private async Task GetNextOperatorByNo(FlowTaskNodeEntity flowTaskNodeEntity, FlowTaskEntity flowTaskEntity, List<FlowTaskNodeEntity> flowTaskNodeEntityList, ApproversProperties approversProperties, List<FlowTaskOperatorEntity> nextFlowTaskOperatorEntityList, string formData)
    {
        if (flowTaskNodeEntity.NodeUp == "0")
        {
            flowTaskEntity.ThisStepId = flowTaskNodeEntityList.Find(x => FlowTaskNodeTypeEnum.start.ParseToString().Equals(x.NodeType)).NodeCode;
            flowTaskEntity.ThisStep = "开始";
            flowTaskEntity.Completion = 0;
            flowTaskEntity.Status = FlowTaskStatusEnum.Reject.ParseToInt();
        }
        else
        {
            // 驳回节点(合流驳回多个节点)
            var upflowTaskNodeEntityList = GetRejectFlowTaskOperatorEntity(flowTaskNodeEntityList, flowTaskNodeEntity, approversProperties);
            if (upflowTaskNodeEntityList.Count == 0)
            {
                throw Oops.Oh(ErrorCode.WF0032);
            }
            foreach (var item in upflowTaskNodeEntityList)
            {
                await AddFlowTaskOperatorEntityByAssigneeType(nextFlowTaskOperatorEntityList, flowTaskNodeEntityList, flowTaskNodeEntity, item, flowTaskEntity.CreatorUserId, formData);
            }
            // 驳回节点下所有节点.
            var rejectNodeNextAllList = new List<FlowTaskNodeEntity>();
            flowTaskEntity.Completion = upflowTaskNodeEntityList.Select(x => x.NodePropertyJson.ToObject<ApproversProperties>().progress.ParseToInt()).ToList().Min();
            await GetAllNextNode(flowTaskNodeEntityList, upflowTaskNodeEntityList.FirstOrDefault().NodeNext, rejectNodeNextAllList);
            if (flowTaskNodeEntity.NodeUp == "1")
            {
                // 驳回上级节点
                flowTaskEntity.ThisStepId = GetRejectThisStepId(rejectNodeNextAllList, upflowTaskNodeEntityList.Select(x => x.NodeCode).ToList(), flowTaskEntity.ThisStepId);
                flowTaskEntity.ThisStep = GetThisStep(flowTaskNodeEntityList, flowTaskEntity.ThisStepId);
                if (flowTaskEntity.ThisStep.Equals("开始"))
                {
                    flowTaskEntity.Completion = 0;
                    flowTaskEntity.Status = FlowTaskStatusEnum.Reject.ParseToInt();
                    nextFlowTaskOperatorEntityList.Clear();
                }
            }
            else
            {
                // 驳回任意节点
                flowTaskEntity.ThisStep = string.Join(",", upflowTaskNodeEntityList.Select(x => x.NodeName).ToArray());
                flowTaskEntity.ThisStepId = string.Join(",", upflowTaskNodeEntityList.Select(x => x.NodeCode).ToArray());
            }

            // 驳回节点下所有节点的候选人
            var candidateDelNodeIds = rejectNodeNextAllList.Select(x => x.Id).ToList();
            _flowTaskRepository.DeleteFlowCandidates(x => candidateDelNodeIds.Contains(x.TaskNodeId));
            await RejectUpdateTaskNode(upflowTaskNodeEntityList.Union(rejectNodeNextAllList).ToList(), 0);
            //删除驳回节点下所有经办
            var rejectList = (await _flowTaskRepository.GetTaskOperatorList(x => x.TaskId == flowTaskEntity.Id && candidateDelNodeIds.Contains(x.TaskNodeId))).OrderBy(x => x.HandleTime).Select(x => x.Id).ToList();
            await _flowTaskRepository.DeleteTaskOperator(rejectList);
            //删除驳回节点经办记录
            var rejectRecodeList = (await _flowTaskRepository.GetTaskOperatorRecordList(x => x.TaskId == flowTaskEntity.Id && candidateDelNodeIds.Contains(x.TaskNodeId))).OrderBy(x => x.HandleTime).Select(x => x.Id).ToList();
            await _flowTaskRepository.DeleteTaskOperatorRecord(rejectRecodeList);
        }
    }

    /// <summary>
    /// 获取同意节点经办.
    /// </summary>
    /// <param name="flowTaskNodeEntity">当前节点.</param>
    /// <param name="nextNode">下个节点.</param>
    /// <param name="nextNodeCodeList">下个节点编码list.</param>
    /// <param name="nextNodeCompletion">下个节点完成度list.</param>
    /// <param name="nextFlowTaskOperatorEntityList">下个节点经办list.</param>
    /// <param name="flowTaskNodeEntityList">所有节点.</param>
    /// <param name="flowTaskEntity">当前任务.</param>
    /// <param name="formData">表单数据.</param>
    /// <returns></returns>
    private async Task GetNextOperatorByYes(FlowTaskNodeEntity flowTaskNodeEntity, FlowTaskNodeEntity nextNode, List<string> nextNodeCodeList, List<int> nextNodeCompletion, List<FlowTaskOperatorEntity> nextFlowTaskOperatorEntityList, List<FlowTaskNodeEntity> flowTaskNodeEntityList, FlowTaskEntity flowTaskEntity, string formData)
    {
        flowTaskNodeEntity.Completion = 1;
        nextNodeCodeList.Add(nextNode.NodeCode);
        nextNodeCompletion.Add(nextNode.NodePropertyJson.ToObject<ApproversProperties>().progress.ParseToInt());
        await AddFlowTaskOperatorEntityByAssigneeType(nextFlowTaskOperatorEntityList, flowTaskNodeEntityList, flowTaskNodeEntity, nextNode, flowTaskEntity.CreatorUserId, formData);
    }

    /// <summary>
    /// 递归获取加签人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowTaskOperatorEntities">所有经办.</param>
    /// <returns></returns>
    private async Task<List<FlowTaskOperatorEntity>> GetOperator(string id, List<FlowTaskOperatorEntity> flowTaskOperatorEntities)
    {
        var childEntity = await _flowTaskRepository.GetTaskOperatorInfo(x => x.ParentId == id && !x.State.Equals("-1"));
        if (childEntity.IsNotEmptyOrNull())
        {
            childEntity.State = "-1";
            flowTaskOperatorEntities.Add(childEntity);
            return await GetOperator(childEntity.Id, flowTaskOperatorEntities);
        }
        else
        {
            return flowTaskOperatorEntities;
        }
    }

    /// <summary>
    /// 对审批人节点分组.
    /// </summary>
    /// <param name="flowTaskOperatorEntities">所有经办.</param>
    /// <returns></returns>
    private Dictionary<string, List<FlowTaskOperatorEntity>> GroupByOperator(List<FlowTaskOperatorEntity> flowTaskOperatorEntities)
    {
        var dic = new Dictionary<string, List<FlowTaskOperatorEntity>>();
        foreach (var item in flowTaskOperatorEntities.GroupBy(x => x.TaskNodeId))
        {
            dic.Add(item.Key, flowTaskOperatorEntities.FindAll(x => x.TaskNodeId == item.Key));
        }
        return dic;
    }

    /// <summary>
    /// 保存当前未完成节点下个候选人节点的候选人.
    /// </summary>
    /// <param name="nodeEntityList">所有节点.</param>
    /// <param name="candidateList">候选人.</param>
    /// <param name="taskOperatorId">0:发起节点，其他：审批节点.</param>
    private List<FlowCandidatesEntity> SaveNodeCandidates(List<FlowTaskNodeEntity> nodeEntityList, Dictionary<string, List<string>> candidateList, string taskOperatorId)
    {
        var flowCandidateList = new List<FlowCandidatesEntity>();
        if (candidateList.IsNotEmptyOrNull())
        {
            foreach (var item in candidateList.Keys)
            {
                var node = nodeEntityList.Find(x => x.NodeCode == item);
                if (node != null)
                {
                    flowCandidateList.Add(new FlowCandidatesEntity()
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        TaskId = node.TaskId,
                        TaskNodeId = node.Id,
                        HandleId = _userManager.UserId,
                        Account = _userManager.Account,
                        Candidates = string.Join(",", candidateList[item]),
                        TaskOperatorId = taskOperatorId
                    });
                }
                var dic = new Dictionary<string, List<string>>();
                if (node.Candidates.IsNotEmptyOrNull())
                {
                    dic = node.Candidates.ToObject<Dictionary<string, List<string>>>();
                    dic[_userManager.Account] = candidateList[item];
                }
                else
                {
                    dic[_userManager.Account] = candidateList[item];
                }
                node.Candidates = dic.ToJsonString();
            }
            _flowTaskRepository.CreateFlowCandidates(flowCandidateList);
        }
        return flowCandidateList;
    }
    #endregion

    #region 子流程处理
    /// <summary>
    /// 创建子流程任务.
    /// </summary>
    /// <param name="childTaskPro">子流程节点属性.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="parentId">子任务父id.</param>
    /// <param name="childTaskCrUsers">子任务创建人.</param>
    private async Task<List<string>> CreateSubProcesses(ChildTaskProperties childTaskPro, object formData, string parentId, List<string> childTaskCrUsers)
    {
        var childFLowEngine = await _flowTaskRepository.GetEngineInfo(childTaskPro.flowId);
        var isSysTable = childFLowEngine.FormType == 1;
        var childTaskId = new List<string>();
        var bodyDic = new Dictionary<string, object>();
        foreach (var item in childTaskCrUsers)
        {
            var prossId = isSysTable ? SnowflakeIdHelper.NextId() : null;
            var title = isSysTable ? _usersService.GetUserName(item, false) + "的" + childFLowEngine.FullName : null;
            var childTaskEntity = await this.Save(null, childTaskPro.flowId, prossId, title, 0, null, formData, 1, 0, isSysTable, parentId, item, false, childTaskPro.isAsync);
            childTaskId.Add(childTaskEntity.Id);
            globalTaskEntity = childTaskEntity;
            if (isSysTable)
            {
                GetSysTableFromService(childFLowEngine.EnCode, formData, childTaskEntity.Id, 1);
            }

            bodyDic.Add(item, new
            {
                enCode = childFLowEngine.EnCode,
                flowId = childFLowEngine.Id,
                formType = childFLowEngine.FormType,
                status = 0,
                processId = childTaskEntity.Id,
                taskNodeId = string.Empty,
                taskOperatorId = string.Empty,
                type = 1
            });
            await StationLetterMsg(childTaskEntity.FullName, new List<string>() { item }, 4, bodyDic);
        }
        return childTaskId;
    }

    /// <summary>
    /// 获取子流程继承父流程的表单数据.
    /// </summary>
    /// <param name="childTaskProperties">子流程属性</param>
    /// <param name="formData">表单数据.</param>
    /// <returns></returns>
    private async Task<object> GetSubFlowFormData(ChildTaskProperties childTaskProperties, string formData)
    {
        var childFlowEngine = _flowTaskRepository.GetEngineFirstOrDefault(childTaskProperties.flowId);
        var parentDic = formData.ToObject().ToObject<Dictionary<string, object>>();
        var isSysTable = childFlowEngine.FormType == 1;
        var childDic = new Dictionary<string, object>();
        if (isSysTable)
        {
            childDic["flowId"] = childTaskProperties.flowId;
        }
        else
        {
            // 解析表单模板
            var formTemplateBase = new TemplateParsingBase(childFlowEngine.FormTemplateJson, childFlowEngine.Tables, true);
            foreach (var item in formTemplateBase.SingleFormData)
            {
                childDic[item.__vModel__] = string.Empty;
            }
        }

        foreach (var item in childTaskProperties.assignList)
        {
            childDic[item.childField] = parentDic.ContainsKey(item.parentField) ? parentDic[item.parentField] : null;
        }

        return childDic;
    }

    /// <summary>
    /// 插入子流程.
    /// </summary>
    /// <param name="childFlowTaskEntity">子流程.</param>
    /// <returns></returns>
    private async Task InsertSubFlowNextNode(FlowTaskEntity childFlowTaskEntity)
    {
        try
        {
            //所有子流程(不包括当前流程)
            var childFlowTaskAll = (await _flowTaskRepository.GetTaskList()).FindAll(x => x.ParentId == childFlowTaskEntity.ParentId && x.Id != childFlowTaskEntity.Id && x.IsAsync == 0);
            //已完成的子流程
            var completeChildFlow = childFlowTaskAll.FindAll(x => x.Status == FlowTaskStatusEnum.Adopt.ParseToInt());
            //父流程
            var parentFlowTask = await _flowTaskRepository.GetTaskInfo(childFlowTaskEntity.ParentId);
            //子流程所属父流程节点
            var parentSubFlowNode = (await _flowTaskRepository.GetTaskNodeList(x => x.TaskId == parentFlowTask.Id && FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(x.NodeType) && x.Completion == 0)).Find(x => x.NodePropertyJson.ToObject<ChildTaskProperties>().childTaskId.Contains(childFlowTaskEntity.Id));
            //判断该父流程子流程节点下的子流程是否完成
            var list = parentSubFlowNode.NodePropertyJson.ToObject<ChildTaskProperties>().childTaskId;
            if (!childFlowTaskAll.Any(x => x.Status != FlowTaskStatusEnum.Adopt.ParseToInt() && list.Contains(x.Id)))
            {
                parentSubFlowNode.Completion = 1;
                await _flowTaskRepository.UpdateTaskNode(parentSubFlowNode);
            }
            if (childFlowTaskAll.Count == completeChildFlow.Count)
            {
                var subFlowOperator = parentSubFlowNode.Adapt<FlowTaskOperatorEntity>();
                subFlowOperator.Id = null;
                subFlowOperator.TaskNodeId = parentSubFlowNode.Id;
                var formType = (await _flowTaskRepository.GetEngineInfo(parentFlowTask.FlowId)).FormType;
                var childData = parentSubFlowNode.NodePropertyJson.ToObject<ChildTaskProperties>().formData;
                var handleModel = new FlowHandleModel();
                if (formType == 2)
                {
                    var dic = new Dictionary<string, object>();
                    dic.Add("data", childData);
                    handleModel.formData = dic;
                }
                else
                {
                    handleModel.formData = childData.ToObject();
                }
                await this.Audit(parentFlowTask, subFlowOperator, handleModel, (int)formType);
            }
        }
        catch (Exception ex)
        {

            throw;
        }
    }
    #endregion

    #region 其他

    /// <summary>
    /// 自定义表单数据处理(保存或提交).
    /// </summary>
    /// <param name="id">主键id（通过空值判断是修改还是新增）.</param>
    /// <param name="flowId">流程id.</param>
    /// <param name="processId">实例id.</param>
    /// <param name="flowTitle">流程任务名.</param>
    /// <param name="flowUrgent">紧急程度（自定义默认为1）.</param>
    /// <param name="billNo">单据规则.</param>
    /// <param name="formData">表单填写的数据.</param>
    /// <param name="crUser">子流程发起人.</param>
    /// <param name="isDev">是否功能设计.</param>
    /// <returns></returns>
    private async Task<FlowTaskEntity> FlowDynamicDataManage(string id, string flowId, string processId, string flowTitle, int? flowUrgent, string billNo, object formData, string crUser, bool isDev)
    {
        try
        {
            FlowEngineEntity flowEngineEntity = await _flowTaskRepository.GetEngineInfo(flowId);
            billNo = "单据规则不存在";
            var userName = crUser.IsNotEmptyOrNull() ? await _usersService.GetUserName(crUser, false) : _userManager.User.RealName;
            flowTitle = string.Format("{0}的{1}", userName, flowEngineEntity.FullName);
            processId = processId.IsNullOrEmpty() ? SnowflakeIdHelper.NextId() : processId;
            flowUrgent = 1;

            // 解析表单模板
            var formTemplateBase = new TemplateParsingBase(flowEngineEntity.FormTemplateJson, flowEngineEntity.Tables, true);

            // 待保存表单数据
            Dictionary<string, object> formDataDic = formData.ToObject<Dictionary<string, object>>();

            // 有表无表
            bool isTable = flowEngineEntity.Tables.Equals("[]");

            // 新增或修改
            bool type = id.IsEmpty();
            if (!type)
            {
                var entity = await _flowTaskRepository.GetTaskInfo(id);
                processId = id;
                flowTitle = entity.FullName;
                billNo = entity.EnCode;
                flowUrgent = entity.FlowUrgent;
            }
            #region 待保存表单数据
            VisualDevEntity visualdevEntity = new VisualDevEntity() { Id = processId, FormData = flowEngineEntity.FormTemplateJson, Tables = flowEngineEntity.Tables, DbLinkId = flowEngineEntity.DbLinkId };
            VisualDevModelDataCrInput visualdevModelDataCrForm = new VisualDevModelDataCrInput() { data = formData.ToJsonString() };
            var dbLink = await _flowTaskRepository.GetLinkInfo(flowEngineEntity.DbLinkId) ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);

            #endregion
            if (!isTable)
            {
                var sql = new List<string>();
                sql = type ? await _runService.CreateHaveTableSql(visualdevEntity, visualdevModelDataCrForm, processId) : await _runService.UpdateHaveTableSql(visualdevEntity, visualdevModelDataCrForm.Adapt<VisualDevModelDataUpInput>(), id);
                if (!isDev)
                {
                    foreach (var item in sql)
                    {
                        await _dataBaseManager.ExecuteSql(dbLink, item);
                    }
                }
            }
            else
            {
                // 获取旧数据
                if (!type)
                {
                    var oldEntity = await _flowTaskRepository.GetTaskInfo(id);
                    var oldAllDataMap = oldEntity.FlowFormContentJson.ToObject<Dictionary<string, object>>();

                    // 当前组织和当前岗位不做修改
                    foreach (var item in formTemplateBase.SingleFormData)
                    {
                        if (item.__config__.qtKey == "currOrganize" || item.__config__.qtKey == "currPosition")
                        {
                            // 当前组织和当前岗位不做修改
                            formDataDic[item.__vModel__] = oldAllDataMap[item.__vModel__];
                        }
                    }
                }

                // 无表处理后待保存数据
                formData = await _runService.GenerateFeilds(formTemplateBase.SingleFormData.ToJsonString(), formDataDic, type);

                // 修改时单据规则不做修改
                if (formTemplateBase.SingleFormData.Any(x => "billRule".Equals(x.__config__.qtKey)) && type)
                {
                    string ruleKey = formTemplateBase.SingleFormData.Where(x => "billRule".Equals(x.__config__.qtKey)).FirstOrDefault().__config__.rule;
                    billNo = await _billRullService.GetBillNumber(ruleKey, true);
                }
            }

            var flowTaskEntity = new FlowTaskEntity();
            flowTaskEntity.Id = id;
            flowTaskEntity.FlowId = flowId;
            flowTaskEntity.ProcessId = processId;
            flowTaskEntity.FlowName = flowTitle;
            flowTaskEntity.FlowUrgent = flowUrgent;
            flowTaskEntity.EnCode = billNo;
            flowTaskEntity.FlowFormContentJson = formData.ToJsonString();
            return flowTaskEntity;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 添加经办记录.
    /// </summary>
    /// <param name="flowTaskOperatorEntity">当前经办.</param>
    /// <param name="flowHandleModel">审批数据.</param>
    /// <param name="hanldState">审批状态.</param>
    /// <returns></returns>
    private async Task CreateOperatorRecode(FlowTaskOperatorEntity flowTaskOperatorEntity, FlowHandleModel flowHandleModel, int hanldState)
    {
        FlowTaskOperatorRecordEntity flowTaskOperatorRecordEntity = new FlowTaskOperatorRecordEntity();
        flowTaskOperatorRecordEntity.HandleOpinion = flowHandleModel.handleOpinion;
        flowTaskOperatorRecordEntity.HandleId = _userManager.UserId;
        flowTaskOperatorRecordEntity.HandleTime = DateTime.Now;
        flowTaskOperatorRecordEntity.HandleStatus = hanldState;
        flowTaskOperatorRecordEntity.NodeCode = flowTaskOperatorEntity.NodeCode;
        flowTaskOperatorRecordEntity.NodeName = flowTaskOperatorEntity.NodeName;
        flowTaskOperatorRecordEntity.TaskOperatorId = flowTaskOperatorEntity.Id;
        flowTaskOperatorRecordEntity.TaskNodeId = flowTaskOperatorEntity.TaskNodeId;
        flowTaskOperatorRecordEntity.TaskId = flowTaskOperatorEntity.TaskId;
        flowTaskOperatorRecordEntity.SignImg = flowHandleModel.signImg;
        flowTaskOperatorRecordEntity.Status = flowTaskOperatorEntity.ParentId.IsNotEmptyOrNull() ? 1 : 0;
        await _flowTaskRepository.CreateTaskOperatorRecord(flowTaskOperatorRecordEntity);
    }

    /// <summary>
    /// 判断会签人数是否达到会签比例.
    /// </summary>
    /// <param name="thisFlowTaskOperatorEntityList">当前节点所有审批人(已剔除加签人).</param>
    /// <param name="handStatus">审批状态.</param>
    /// <param name="index">比例.</param>
    /// <param name="hasFreeApprover">是否有加签(true：没有，flase：有).</param>
    /// <returns></returns>
    private bool IsAchievebilProportion(List<FlowTaskOperatorEntity> thisFlowTaskOperatorEntityList, int handStatus, int index, bool hasFreeApprover)
    {
        if (handStatus == 0)
            index = 100 - index;
        //完成人数（加上当前审批人）
        var comIndex = thisFlowTaskOperatorEntityList.FindAll(x => x.HandleStatus == handStatus && x.Completion == 1 && x.State == "0").Count.ParseToDouble();
        if (hasFreeApprover)
        {
            ++comIndex;
        }
        //完成比例
        var comProportion = (comIndex / thisFlowTaskOperatorEntityList.Count.ParseToDouble() * 100).ParseToInt();
        return comProportion >= index;
    }

    /// <summary>
    /// 事件请求.
    /// </summary>
    /// <param name="funcConfig">事件配置.</param>
    /// <param name="formdata">表单数据.</param>
    /// <returns></returns>
    private async Task RequestEvents(FuncConfig funcConfig, string formdata)
    {
        if (funcConfig.IsNotEmptyOrNull() && funcConfig.on && funcConfig.interfaceId.IsNotEmptyOrNull())
        {
            var parameters = GetMsgContent(funcConfig.templateJson, formdata, new MessageTemplateEntity());
            await _dataInterfaceService.GetResponseByType(funcConfig.interfaceId, 3, _userManager.TenantId, null, parameters);
        }
    }

    #region 消息推送
    /// <summary>
    /// 站内消息推送.
    /// </summary>
    /// <param name="fullName">任务名.</param>
    /// <param name="users">通知人员.</param>
    /// <param name="msgType">消息类型【0.审批，1.同意，2.拒绝，3.抄送，4.子流程，5.结束】.</param>
    /// <param name="pairs">详情跳转json.</param>
    /// <returns></returns>
    private async Task StationLetterMsg(string fullName, List<string> users, int msgType, Dictionary<string, object> pairs)
    {
        var msgTemplateEntity = new MessageTemplateEntity();
        switch (msgType)
        {
            case 1:
                msgTemplateEntity.Title = fullName + "已被【同意】";
                break;
            case 2:
                msgTemplateEntity.Title = fullName + "已被【拒绝】";
                break;
            case 3:
                msgTemplateEntity.Title = fullName + "已被【抄送】";
                break;
            case 4:
                msgTemplateEntity.Title = fullName + "请发起【子流程】";
                break;
            case 5:
                msgTemplateEntity.Title = fullName + "已【结束】";
                break;
            default:
                msgTemplateEntity.Title = fullName;
                break;
        }
        await _messageTemplateService.SendNodeMessage(new List<string>() { "1" }, msgTemplateEntity, users, null, pairs);
    }

    /// <summary>
    /// 通过消息模板获取消息通知.
    /// </summary>
    /// <param name="msgConfig">消息配置.</param>
    /// <param name="users">通知人员.</param>
    /// <param name="formdata">表单数据.</param>
    private async Task Alerts(MsgConfig msgConfig, List<string> users, string formdata)
    {
        if (msgConfig.IsNotEmptyOrNull() && msgConfig.on != 0 && msgConfig.msgId.IsNotEmptyOrNull())
        {
            var msgTemplateEntity = await _messageTemplateService.GetInfo(msgConfig.msgId);
            var typeList = new List<string>();
            var parameters = GetMsgContent(msgConfig.templateJson, formdata, msgTemplateEntity);
            if (msgTemplateEntity.IsEmail == 1)
                typeList.Add("2");
            if (msgTemplateEntity.IsSms == 1)
                typeList.Add("3");
            if (msgTemplateEntity.IsDingTalk == 1)
                typeList.Add("4");
            if (msgTemplateEntity.IsWeCom == 1)
                typeList.Add("5");
            await _messageTemplateService.SendNodeMessage(typeList, msgTemplateEntity, users, parameters, null);
        }
    }

    /// <summary>
    /// 获取消息模板内容.
    /// </summary>
    /// <param name="templateJsonItems">消息模板json.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="messageTemplateEntity">消息模板.</param>
    private Dictionary<string, string> GetMsgContent(List<TemplateJsonItem> templateJsonItems, string formData, MessageTemplateEntity messageTemplateEntity)
    {
        var jObj = formData.ToObject<JObject>();
        var dic = new Dictionary<string, string>();
        var taskEntity = _flowTaskRepository.GetTaskFirstOrDefault(globalTaskId);
        if (taskEntity.IsNullOrEmpty())
        {
            taskEntity = globalTaskEntity;
        }
        foreach (var item in templateJsonItems)
        {
            var value = string.Empty;
            if (item.relationField.Equals("qtFlowOperatorUserId"))
            {
                value = _userManager.UserId;
            }
            else if (item.relationField.Equals("qtTaskId"))
            {
                value = globalTaskId;
            }
            else if (item.relationField.Equals("qtTaskNodeId"))
            {
                value = globalTaskNodeId;
            }
            else if (item.relationField.Equals("qtTaskFullName"))
            {
                value = taskEntity.FullName;
            }
            else if (item.relationField.Equals("qtLaunchUserId"))
            {
                value = taskEntity.CreatorUserId;
            }
            else if (item.relationField.Equals("qtLaunchUserName"))
            {
                value = _usersService.GetInfoByUserId(taskEntity.CreatorUserId).RealName;
            }
            else if (item.relationField.Equals("qtFlowOperatorUserName"))
            {
                value = _userManager.User.RealName;
            }
            else if (item.relationField.Equals("qtFlowId"))
            {
                value = taskEntity.FlowId;
            }
            else if (item.relationField.Equals("qtFlowFullName"))
            {
                value = taskEntity.FlowName;
            }
            else
            {
                if (item.isSubTable)
                {
                    var fields = item.relationField.Split("-").ToList();
                    // 子表键值
                    var tableField = fields[0];
                    // 子表字段键值
                    var keyField = fields[1];
                    if (jObj.ContainsKey(tableField) && jObj[tableField] is JArray)
                    {
                        var jar = jObj[tableField] as JArray;

                        value = jar.Where(x => x.ToObject<JObject>().ContainsKey(keyField)).Select(x => x.ToObject<JObject>()[keyField]).ToJsonString();
                    }
                }
                else
                {
                    value = jObj.ContainsKey(item.relationField) ? jObj[item.relationField].ToString() : string.Empty;
                }
            }
            messageTemplateEntity.Title = messageTemplateEntity.Title.Replace("{" + item.field + "}", value);
            messageTemplateEntity.Content = messageTemplateEntity.Content.Replace("{" + item.field + "}", value);
            dic.Add(item.field, value);
        }
        return dic;
    }

    /// <summary>
    /// 组装消息跳转详情参数.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例</param>
    /// <param name="taskNodeId">节点id.</param>
    /// <param name="userList">通知人员.</param>
    /// <param name="flowTaskOperatorEntities">经办实例.</param>
    /// <param name="type">1:发起，2：待办，3：抄送</param>
    /// <param name="taskOperatorId"></param>
    /// <returns></returns>
    private Dictionary<string, object> GetMesBodyText(FlowEngineEntity flowEngineEntity, string taskNodeId, List<string> userList, List<FlowTaskOperatorEntity> flowTaskOperatorEntities, int type, string taskOperatorId = "")
    {
        var dic = new Dictionary<string, object>();
        if (flowTaskOperatorEntities.IsNotEmptyOrNull() && flowTaskOperatorEntities.Count > 0)
        {
            foreach (var item in flowTaskOperatorEntities)
            {
                var value = new
                {
                    enCode = flowEngineEntity.EnCode,
                    flowId = flowEngineEntity.Id,
                    formType = flowEngineEntity.FormType,
                    status = type == 1 ? 0 : 1,
                    processId = item.TaskId,
                    taskNodeId = item.TaskNodeId,
                    taskOperatorId = item.Id,
                    type = type
                };
                dic.Add(item.HandleId, value);
                var toUserId = _flowTaskRepository.GetToUserId(item.HandleId, flowEngineEntity.Id);
                toUserId.ForEach(u => dic[u] = value);
            }
        }
        else
        {
            var value = new
            {
                enCode = flowEngineEntity.EnCode,
                flowId = flowEngineEntity.Id,
                formType = flowEngineEntity.FormType,
                status = type == 1 ? 0 : 1,
                processId = globalTaskId,
                taskNodeId = taskNodeId,
                taskOperatorId = taskOperatorId,
                type = type
            };
            userList.ForEach(u => dic.Add(u, value));
        }
        return dic;
    }
    #endregion
    #endregion

    #region 系统表单
    /// <summary>
    /// 系统表单操作
    /// </summary>
    /// <param name="enCode"></param>
    /// <param name="data"></param>
    /// <param name="id"></param>
    /// <param name="type"></param>
    private void GetSysTableFromService(string enCode, object data, string id, int type)
    {
        Scoped.Create((_, scope) =>
        {
            switch (enCode.ToLower())
            {
                case "salesorder":
                    var SalesOrder = App.GetService<ISalesOrderService>();
                    SalesOrder.Save(id, data, type);
                    break;
                case "leaveapply":
                    var LeaveApply = App.GetService<ILeaveApplyService>();
                    LeaveApply.Save(id, data, type);
                    break;
            }
        });
    }
    #endregion
    #endregion
}