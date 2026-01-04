//using AutoMapper;
//using QTMall.Core.API.Filters;
//using QTMall.Core.Common.Emum;
//using QTMall.Core.Common.Extensions;
//using QTMall.Core.Common.Helper;
//using QTMall.Core.IServices;
//using QTMall.Core.Model.Models;
//using QTMall.Core.Model.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace QTMall.Core.API.Controllers
//{
//    /// <summary>
//    /// 管理角色
//    /// </summary>
//    [Route("admin/manager/role")]
//    [ApiController]
//    public class ManagerRoleController : ControllerBase
//    {
//        private readonly IManagerRoleService _managerRoleService;
//        private readonly IUserService _userService;
//        private readonly IManagerMenuService _navigationService;
//        private readonly IMapper _mapper;

//        /// <summary>
//        /// 依赖注入
//        /// </summary>
//        public ManagerRoleController(IManagerRoleService managerRoleService, IUserService userService,
//            IManagerMenuService navigationService, IMapper mapper)
//        {
//            _managerRoleService = managerRoleService;
//            _userService = userService;
//            _navigationService = navigationService;
//            _mapper = mapper;
//        }

//        #region 管理员调用接口==========================
//        /// <summary>
//        /// 根据ID获取数据
//        /// 示例：/admin/manager/role/menu
//        /// </summary>
//        [HttpGet("menu")]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.View)]
//        public async Task<IActionResult> GetNavigation()
//        {
//            //查询数据库获取实体
//            var result = await _navigationService.QueryListByRoleIdAsync(0, WriteRoRead.Write);
//            if (result == null)
//            {
//                return BadRequest(ResponseMessage.Error("菜单导航不存在或已删除"));
//            }
//            return Ok(result);
//        }

//        /// <summary>
//        /// 根据ID获取数据
//        /// 示例：/admin/manager/role/1
//        /// </summary>
//        [HttpGet("{id}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.View)]
//        public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
//        {
//            //检测参数是否合法
//            if (!param.Fields.IsPropertyExists<ManagerRolesDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }
//            //检查当前用户是否超级管理员
//            bool isSuperAdmin = await _userService.IsSuperAdminAsync();
//            //查询数据库获取实体
//            var model = await _managerRoleService.QueryAsync(x => x.Id == id && (isSuperAdmin || x.RoleType == 1), WriteRoRead.Write);
//            if (model == null)
//            {
//                return BadRequest(ResponseMessage.Error($"数据[{id}]不存在或已删除"));
//            }
//            //根据字段进行塑形
//            var result = model.ShapeData(param.Fields);
//            return Ok(result);
//        }

//        /// <summary>
//        /// 获取指定数量列表
//        /// 示例：/admin/manager/role/view/10
//        /// </summary>
//        [HttpGet("view/{top}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.View)]
//        public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
//        {
//            //检测参数是否合法
//            if (searchParam.OrderBy != null
//                && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ManagerRolesDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的排序参数"));
//            }
//            if (!searchParam.Fields.IsPropertyExists<ManagerRolesDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }

//            //检查当前用户是否超级管理员
//            bool isSuperAdmin = await _userService.IsSuperAdminAsync();
//            //获取数据库列表
//            var resultFrom = await _managerRoleService.QueryListAsync(top,
//                x => (isSuperAdmin || x.RoleType == 1) && x.RoleType > 0
//                && (!searchParam.Keyword.IsNotNullOrEmpty() || x.Name.Contains(searchParam.Keyword) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
//                searchParam.OrderBy ?? "Id");

//            //映射成DTO，根据字段进行塑形
//            var resultDto = _mapper.Map<IEnumerable<ManagerRolesDto>>(resultFrom).ShapeData(searchParam.Fields);
//            //返回成功200
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 获取分页列表
//        /// 示例：/admin/manager/role?pageSize=10&pageIndex=1
//        /// </summary>
//        [HttpGet]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.View)]
//        public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
//        {
//            //检测参数是否合法
//            if (searchParam.OrderBy != null
//                && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ManagerRolesDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的排序参数"));
//            }
//            if (!searchParam.Fields.IsPropertyExists<ManagerRolesDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }

//            //检查当前用户是否超级管理员
//            bool isSuperAdmin = await _userService.IsSuperAdminAsync();
//            //获取数据列表
//            var list = await _managerRoleService.QueryPageAsync(
//                pageParam.PageSize,
//                pageParam.PageIndex,
//                x => (isSuperAdmin || x.RoleType == 1) && x.RoleType > 0
//                && (!searchParam.Keyword.IsNotNullOrEmpty() || x.Name.Contains(searchParam.Keyword) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
//                searchParam.OrderBy ?? "Id");

//            //x-pagination
//            var paginationMetadata = new
//            {
//                totalCount = list.TotalCount,
//                pageSize = list.PageSize,
//                pageIndex = list.PageIndex,
//                totalPages = list.TotalPages
//            };
//            Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

//            //映射成DTO，根据字段进行塑形
//            var resultDto = _mapper.Map<IEnumerable<ManagerRolesDto>>(list).ShapeData(searchParam.Fields);
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 添加一条记录
//        /// 示例：/admin/manager/role
//        /// </summary>
//        [HttpPost]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.Add)]
//        public async Task<IActionResult> Add([FromBody] ManagerRolesEditDto modelDto)
//        {
//            var result = await _managerRoleService.AddAsync(modelDto);
//            return NoContent();
//        }

//        /// <summary>
//        /// 修改一条记录
//        /// 示例：/admin/manager/role/1
//        /// </summary>
//        [HttpPut("{id}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.Edit)]
//        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ManagerRolesEditDto modelDto)
//        {
//            var result = await _managerRoleService.UpdateAsync(id, modelDto);
//            return NoContent();
//        }

//        /// <summary>
//        /// 删除一条记录
//        /// 示例：/admin/manager/role/1
//        /// </summary>
//        [HttpDelete("{id}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.Delete)]
//        public async Task<IActionResult> Delete([FromRoute] int id)
//        {
//            if (!await _managerRoleService.ExistsAsync<ApplicationRole>(x => x.Id == id))
//            {
//                return BadRequest(ResponseMessage.Error($"数据[{id}]不存在或已删除"));
//            }
//            var result = await _managerRoleService.DeleteAsync(x => x.Id == id);

//            return NoContent();
//        }

//        /// <summary>
//        /// 批量删除记录
//        /// 示例：/admin/manager/role?ids=1,2,3
//        /// </summary>
//        [HttpDelete]
//        [Authorize]
//        [AuthorizeFilter("ManagerRole", ActionType.Delete)]
//        public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
//        {
//            if (Ids == null)
//            {
//                return BadRequest(ResponseMessage.Error("传输参数不可为空"));
//            }
//            //将ID列表转换成IEnumerable
//            var arrIds = Ids.ToIEnumerable<int>();
//            if (arrIds == null)
//            {
//                return BadRequest(ResponseMessage.Error("传输参数不符合规范"));
//            }
//            //执行批量删除操作
//            await _managerRoleService.DeleteAsync(x => arrIds.Contains(x.Id));

//            return NoContent();
//        }
//        #endregion
//    }
//}