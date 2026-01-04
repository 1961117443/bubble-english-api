using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.Team;
using QT.Systems.Entitys.Permission;

namespace QT.Extend
{
    /// <summary>
    /// 项目计划
    /// </summary>
    [ApiDescriptionSettings(Tag = "Extend", Name = "ProjectGantt", Order = 600)]
    [Route("api/extend/[controller]")]
    public class ProjectGanttService : IDynamicApiController, ITransient
    {
        private readonly ISqlSugarRepository<ProjectGanttEntity> _projectGanttRepository;
        private readonly IUserManager _userManager;

        public ProjectGanttService(ISqlSugarRepository<ProjectGanttEntity> projectGanttRepository,IUserManager userManager)
        {
            _projectGanttRepository = projectGanttRepository;
            _userManager = userManager;
        }

        #region GET

        /// <summary>
        /// 项目列表.
        /// </summary>
        /// <param name="input">请求参数</param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<dynamic> GetList([FromQuery] KeywordInput input, [FromQuery]string userId)
        {
            var data = await _projectGanttRepository.AsQueryable().Where(x => x.Type == 1 && x.DeleteMark == null)
                .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.FullName.Contains(input.keyword))
                .WhereIF(userId.IsNotEmptyOrNull(), x => QTSqlFunc.FIND_IN_SET(userId, x.ManagerIds) || QTSqlFunc.FIND_IN_SET(userId, x.MemberIds))
                .OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime, OrderByType.Desc)
                .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc).ToListAsync();
            var output = data.Adapt<List<ProjectGanttListOutput>>();
            await GetManagersInfo(output);
            return new { list = output };
        }

        /// <summary>
        /// 任务列表
        /// </summary>
        /// <param name="projectId">项目Id</param>
        /// <param name="input">请求参数</param>
        /// <returns></returns>
        [HttpGet("{projectId}/Task")]
        public async Task<dynamic> GetTaskList([FromQuery] KeywordInput input, string projectId)
        {
            var data = await _projectGanttRepository.AsQueryable()
                .Where(x => x.Type == 2 && x.ProjectId == projectId && x.DeleteMark == null)
                .OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime, OrderByType.Desc)
                .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc).ToListAsync();
            data.Add(await _projectGanttRepository.FirstOrDefaultAsync(x => x.Id == projectId));
            if (!string.IsNullOrEmpty(input.keyword))
            {
                data = data.TreeWhere(t => t.FullName.Contains(input.keyword), t => t.Id, t => t.ParentId);
            }
            var output = data.Adapt<List<ProjectGanttTaskListOutput>>();
            return new { list = output.ToTree() };
        }

        /// <summary>
        /// 任务树形
        /// </summary>
        /// <param name="projectId">项目Id</param>
        /// <returns></returns>
        [HttpGet("{projectId}/Task/Selector/{id}")]
        public async Task<dynamic> GetTaskTreeView(string projectId, string id)
        {
            var data = (await _projectGanttRepository.AsQueryable().Where(x => x.Type == 2 && x.ProjectId == projectId && x.DeleteMark == null).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToListAsync());
            data.Add(await _projectGanttRepository.FirstOrDefaultAsync(x => x.Id == projectId));
            if (!id.Equals("0"))
            {
                data.RemoveAll(x => x.Id == id);
            }
            var output = data.Adapt<List<ProjectGanttTaskTreeViewOutput>>();
            return new { list = output.ToTree() };
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<dynamic> GetInfo(string id)
        {
            var data = (await _projectGanttRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<ProjectGanttInfoOutput>();
            return data;
        }

        /// <summary>
        /// 项目任务信息
        /// </summary>
        /// <param name="taskId">主键值</param>
        /// <returns></returns>
        [HttpGet("Task/{taskId}")]
        public async Task<dynamic> GetTaskInfo(string taskId)
        {
            var data = (await _projectGanttRepository.FirstOrDefaultAsync(x => x.Id == taskId && x.DeleteMark == null)).Adapt<ProjectGanttTaskInfoOutput>();
            return data;
        }

        /// <summary>
        /// 根据组员获取所有分组.
        /// </summary>
        /// <returns></returns>
        [HttpGet("member/{userId}")]
        public async Task<List<ProjectGanttListOutput>> GetListByMember(string userId)
        {
            var list = await _projectGanttRepository.Context.Queryable<ProjectGanttEntity>()
                .Where(a => a.Type == 1)
                .WhereIF(!_userManager.IsAdministrator, a => QTSqlFunc.FIND_IN_SET(userId, a.ManagerIds) || QTSqlFunc.FIND_IN_SET(userId, a.MemberIds))
                .Where(a => a.DeleteMark == null)
                .OrderBy(a => a.SortCode)
                .OrderBy(a => a.CreatorTime, OrderByType.Desc)
                .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
                .Select((a) => new ProjectGanttListOutput
                {
                    id = a.Id,
                    fullName = a.FullName,
                    state = a.State,
                    schedule = a.Schedule,
                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// 组员下拉选项，根据负责人和参与人分组.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Actions/{id}/member/Selector")]
        public async Task<List<TeamSelectorOutput>> GetMemberSelector(string id)
        {
            var entity = await _projectGanttRepository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

            List<TeamSelectorOutput> teamSelectorOutputs = new List<TeamSelectorOutput>();

            if (entity.ManagerIds.IsNotEmptyOrNull())
            {
                var managerIds = entity.ManagerIds.Split(',').ToList();
                var list = await _projectGanttRepository.Context.Queryable<UserEntity>().Where(x => managerIds.Contains(x.Id))
                    .Select(x => new TeamSelectorOutput
                    {
                        id = x.Id,
                        fullName = x.RealName
                    })
                    .ToListAsync();

                if (list.IsAny())
                {
                    teamSelectorOutputs.Add(new TeamSelectorOutput
                    {
                        fullName = "负责人",
                        children = list.ToList<object>() // .Select(x => (object)x)
                    });
                }
            }

            if (entity.MemberIds.IsNotEmptyOrNull())
            {
                var memberIds = entity.MemberIds.Split(',').ToList();
                var list = await _projectGanttRepository.Context.Queryable<UserEntity>().Where(x => memberIds.Contains(x.Id))
                    .Select(x => new TeamSelectorOutput
                    {
                        id = x.Id,
                        fullName = x.RealName
                    })
                    .ToListAsync();

                if (list.IsAny())
                {
                    teamSelectorOutputs.Add(new TeamSelectorOutput
                    {
                        fullName = "参与人",
                        children = list.ToList<object>() // .Select(x => (object)x)
                    });
                }
            }

            return teamSelectorOutputs;
        }
        #endregion

        #region POST

        /// <summary>
        /// 删除.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            if (await _projectGanttRepository.AnyAsync(x => x.ParentId != id && x.DeleteMark == null))
            {
                var entity = await _projectGanttRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
                if (entity != null)
                {
                    int isOk = await _projectGanttRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
                    if (isOk < 1)
                        throw Oops.Oh(ErrorCode.COM1002);
                }
                else
                {
                    throw Oops.Oh(ErrorCode.COM1005);
                }
            }
            else
            {
                throw Oops.Oh(ErrorCode.D1007);
            }
        }

        /// <summary>
        /// 创建.
        /// </summary>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task Create([FromBody] ProjectGanttCrInput input)
        {
            if (await _projectGanttRepository.AnyAsync(x => x.EnCode == input.enCode && x.DeleteMark == null) || await _projectGanttRepository.AnyAsync(x => x.FullName == input.fullName && x.DeleteMark == null))
                throw Oops.Oh(ErrorCode.COM1004);
            var entity = input.Adapt<ProjectGanttEntity>();
            entity.Type = 1;
            entity.ParentId = "0";
            var isOk = await _projectGanttRepository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);
        }

        /// <summary>
        /// 编辑.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Update(string id, [FromBody] ProjectGanttUpInput input)
        {
            if (await _projectGanttRepository.AnyAsync(x => x.Id != id && x.EnCode == input.enCode && x.DeleteMark == null) || await _projectGanttRepository.AnyAsync(x => x.Id != id && x.FullName == input.fullName && x.DeleteMark == null))
                throw Oops.Oh(ErrorCode.COM1004);
            var entity = input.Adapt<ProjectGanttEntity>();
            var isOk = await _projectGanttRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1001);
        }

        /// <summary>
        /// 创建.
        /// </summary>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPost("Task")]
        public async Task CreateTask([FromBody] ProjectGanttTaskCrInput input)
        {
            var entity = input.Adapt<ProjectGanttEntity>();
            entity.Type = 2;
            var isOk = await _projectGanttRepository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);
        }

        /// <summary>
        /// 编辑.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="input">实体对象</param>
        /// <returns></returns>
        [HttpPut("Task/{id}")]
        public async Task UpdateTask(string id, [FromBody] ProjectGanttTaskUpInput input)
        {
            var entity = input.Adapt<ProjectGanttEntity>();
            var isOk = await _projectGanttRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1001);
        }
        #endregion

        #region PrivateMethod

        /// <summary>
        /// 项目参与人员.
        /// </summary>
        /// <param name="outputList"></param>
        /// <returns></returns>
        private async Task GetManagersInfo(List<ProjectGanttListOutput> outputList)
        {
            var userIds = outputList.SelectMany(x =>
                {
                    List<string> strings = new List<string>();
                    if (x.managerIds.IsNotEmptyOrNull())
                    {
                        strings.AddRange(x.managerIds.Split(","));
                    }
                    if (x.memberIds.IsNotEmptyOrNull())
                    {
                        strings.AddRange(x.memberIds.Split(","));
                    }
                    return strings;
                })
                .Where(x => x.IsNotEmptyOrNull()).Distinct().ToList();
            if (userIds.IsAny())
            {
                var userList = await _projectGanttRepository.Context.Queryable<UserEntity>()
                    .Where(x => userIds.Contains(x.Id) && x.DeleteMark == null)
                    .Select(x=> new UserEntity
                    {
                        Id = x.Id,
                        RealName = x.RealName,
                        Account = x.Account,
                        HeadIcon =x.HeadIcon,
                    })
                    .ToListAsync();
                foreach (var output in outputList)
                {
                    if (output.managerIds.IsNotEmptyOrNull())
                    {
                        foreach (var id in output.managerIds.Split(","))
                        {
                            var managerInfo = new ManagersInfo();
                            var userInfo = userList.Find(x => x.Id == id);

                            if (userInfo != null)
                            {
                                userInfo.Adapt(managerInfo);
                                output.managersInfo.Add(managerInfo);
                            }
                        }
                    }
                    

                    if (output.memberIds.IsNotEmptyOrNull())
                    {
                        foreach (var id in output.memberIds.Split(","))
                        {
                            var managerInfo = new ManagersInfo();
                            var userInfo = userList.Find(x => x.Id == id);
                            if (userInfo != null)
                            {
                                userInfo.Adapt(managerInfo);
                                output.membersInfo.Add(managerInfo);
                            }
                        }
                    }
                    
                }
            }
           
        }
        #endregion
    }
}
