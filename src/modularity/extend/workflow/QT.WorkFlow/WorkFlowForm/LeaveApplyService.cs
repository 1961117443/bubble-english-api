using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.WorkFlow.Entitys;
using QT.WorkFlow.Entitys.Dto.WorkFlowForm.LeaveApply;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.WorkFlow.WorkFlowForm
{
    /// <summary>
    /// 请假申请
    
    
    
    /// 日 期：2021-06-01 
    /// </summary>
    [ApiDescriptionSettings(Tag = "WorkflowForm", Name = "LeaveApply", Order = 516)]
    [Route("api/workflow/Form/[controller]")]
    public class LeaveApplyService : ILeaveApplyService, IDynamicApiController, ITransient
    {
        private readonly ISqlSugarRepository<LeaveApplyEntity> _sqlSugarRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IFileManager _fileManager;
        private readonly IUserManager _userManager;
        private readonly IFlowTaskManager _flowTaskManager;
        private readonly ITenant _db;

        public LeaveApplyService(
            ISqlSugarRepository<LeaveApplyEntity> sqlSugarRepository,
            ICacheManager cacheManager,
            IFileManager fileManager,
            IUserManager userManager,
            IFlowTaskManager flowTaskManager,
            ISqlSugarClient context)
        {
            _sqlSugarRepository = sqlSugarRepository;
            _cacheManager = cacheManager;
            _fileManager = fileManager;
            _userManager = userManager;
            _flowTaskManager = flowTaskManager;
            _db = context.AsTenant();
        }

        #region GET

        /// <summary>
        /// 信息.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<dynamic> GetInfo(string id)
        {
            return (await _sqlSugarRepository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LeaveApplyInfoOutput>();
        }
        #endregion

        #region POST

        /// <summary>
        /// 保存.
        /// </summary>
        /// <param name="input">表单信息</param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task Save([FromBody] LeaveApplyCrInput input)
        {
            var entity = input.Adapt<LeaveApplyEntity>();
            if (input.status == 1)
            {
                await Save(entity.Id, entity);
            }
            else
            {
                await Submit(entity.Id, entity, input.candidateList);
            }

        }

        /// <summary>
        /// 提交.
        /// </summary>
        /// <param name="id">表单信息</param>
        /// <param name="input">表单信息</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Submit(string id, [FromBody] LeaveApplyUpInput input)
        {
            input.id = id;
            var entity = input.Adapt<LeaveApplyEntity>();
            if (input.status == 1)
            {
                await Save(entity.Id, entity);
            }
            else
            {
                await Submit(entity.Id, entity, input.candidateList);
            }

        }
        #endregion

        #region PrivateMethod

        /// <summary>
        /// 保存.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task Save(string id, LeaveApplyEntity entity, int type = 0)
        {
            try
            {
                _db.BeginTran();
                #region 表单信息
                await HandleForm(id, entity);
                #endregion

                #region 流程信息
                await _flowTaskManager.Save(id, entity.FlowId, entity.Id, entity.FlowTitle, entity.FlowUrgent, entity.BillNo, null, 1, type, true);
                #endregion

                _db.CommitTran();
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 提交.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="entity">实体对象</param>
        private async Task Submit(string id, LeaveApplyEntity entity, Dictionary<string, List<string>> candidateList)
        {
            try
            {
                _db.BeginTran();
                #region 表单信息
                await HandleForm(id, entity);
                #endregion

                #region 流程信息
                await _flowTaskManager.Submit(id, entity.FlowId, entity.Id, entity.FlowTitle, entity.FlowUrgent, entity.BillNo, entity.Adapt<LeaveApplyUpInput>(), 0, 0, true, false, candidateList);
                #endregion

                _db.CommitTran();
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 表单操作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task HandleForm(string id, LeaveApplyEntity entity)
        {
            if (string.IsNullOrEmpty(id))
            {
                entity.Id = YitIdHelper.NextId().ToString();
                await _sqlSugarRepository.InsertAsync(entity);
                _cacheManager.Del(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYBILLRULE, _userManager.TenantId, _userManager.UserId + "WF_LeaveApplyNo"));
            }
            else
            {
                entity.Id = id;
                await _sqlSugarRepository.UpdateAsync(entity);
                foreach (var item in entity.FileJson.ToList<AnnexModel>())
                {
                    if (item.IsNotEmptyOrNull() && item.FileType == "delete")
                    {
                        await _fileManager.DeleteFile(Path.Combine(FileVariable.SystemFilePath, item.FileName));
                    }
                }
            }
        }
        #endregion

        #region PublicMethod

        /// <summary>
        /// 工作流表单操作.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <param name="type">0：事前审批，1：创建子流程</param>
        /// <returns></returns>
        [NonAction]
        public async Task Save(string id, object obj, int type)
        {
            try
            {
                var input = obj.ToObject<LeaveApplyUpInput>();
                var entity = input.Adapt<LeaveApplyEntity>();
                if (type == 0)
                {
                    await this.HandleForm(id, entity);
                }
                else
                {
                    entity.Id = id;
                    await _sqlSugarRepository.InsertAsync(entity);
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }
        #endregion
    }
}
