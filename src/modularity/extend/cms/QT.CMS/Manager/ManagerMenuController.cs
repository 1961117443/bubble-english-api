//using AutoMapper;
//using QTMall.Core.API.Filters;
//using QTMall.Core.Common.Emum;
//using QTMall.Core.Common.Extensions;
//using QTMall.Core.Common.Helper;
//using QTMall.Core.IServices;
//using QTMall.Core.Model.Models;
//using QTMall.Core.Model.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
//using Microsoft.AspNetCore.Mvc;

//namespace QTMall.Core.API.Controllers
//{
//    /// <summary>
//    /// 管理菜单
//    /// </summary>
//    [Route("admin/manager/menu")]
//    [ApiController]
//    public class ManagerMenuController : ControllerBase
//    {
//        private readonly IManagerMenuService _managerMenuService;
//        private readonly IUserService _userService;
//        private readonly IMapper _mapper;

//        /// <summary>
//        /// 依赖注入
//        /// </summary>
//        public ManagerMenuController(IManagerMenuService managerMenuService, IUserService userService, IMapper mapper)
//        {
//            _managerMenuService = managerMenuService;
//            _userService = userService;
//            _mapper = mapper;
//        }

//        #region 管理员调用接口==========================
//        /// <summary>
//        /// 获取导航树目录列表
//        /// 示例：/admin/manager/menu
//        /// </summary>
//        [HttpGet]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.View)]
//        public async Task<IActionResult> GetList([FromQuery] BaseParameter param)
//        {
//            //检测参数是否合法
//            if (!param.Fields.IsPropertyExists<ManagerMenuDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }
//            //如果有查询关健字
//            var parentId = 0; //父节点ID
//            if (param.Keyword.IsNotNullOrWhiteSpace())
//            {
//                var model = await _managerMenuService.QueryAsync<ManagerMenu>(x => (x.Title != null && x.Title.Contains(param.Keyword)));
//                if (model == null)
//                {
//                    return NotFound(ResponseMessage.Error("暂无查询记录"));
//                }
//                parentId = model.Id;
//            }
//            //获取数据库列表
//            var resultFrom = await _managerMenuService.QueryListAsync(parentId);
//            //使用AutoMapper转换成ViewModel
//            //根据字段进行塑形
//            var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
//            //返回成功200
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 根据ID获取导航
//        /// 示例：/admin/manager/menu/1
//        /// </summary>
//        [HttpGet("{navId}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.View)]
//        public async Task<IActionResult> GetById([FromRoute] int navId, [FromQuery] BaseParameter param)
//        {
//            //检测参数是否合法
//            if (!param.Fields.IsPropertyExists<ManagerMenuDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }
//            //查询数据库获取实体
//            var model = await _managerMenuService.QueryAsync<ManagerMenu>(x => x.Id == navId, WriteRoRead.Write);
//            if (model == null)
//            {
//                return BadRequest(ResponseMessage.Error($"数据{navId}不存在或已删除"));
//            }
//            //使用AutoMapper转换成ViewModel
//            //根据字段进行塑形
//            var result = _mapper.Map<ManagerMenuDto>(model).ShapeData(param.Fields);
//            return Ok(result);
//        }

//        /// <summary>
//        /// 添加一条记录
//        /// 示例：/admin/manager/menu
//        /// </summary>
//        [HttpPost]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.Add)]
//        public async Task<IActionResult> Add([FromBody] ManagerMenuEditDto modelDto)
//        {
//            //检查菜单名称是否重复
//            if (await _managerMenuService.ExistsAsync<ManagerMenu>(x => x.Name != null && x.Name.ToLower().Equals(modelDto.Name!.ToLower())))
//            {
//                return BadRequest(new ResponseMessage(ErrorCode.RepeatField, $"菜单名称[{modelDto.Name}]已存在，请更换后重试"));
//            }
//            //映射成实体
//            var model = _mapper.Map<ManagerMenu>(modelDto);
//            //获取当前用户名
//            model.AddBy = await _userService.GetUserNameAsync();
//            model.AddTime = DateTime.Now;
//            //写入数据库
//            var sourceModel = await _managerMenuService.AddAsync(model);
//            //映射成DTO再返回，否则出错
//            var result = _mapper.Map<ManagerMenuDto>(sourceModel);
//            return Ok(result);
//        }

//        /// <summary>
//        /// 修改一条记录
//        /// 示例：/admin/manager/menu/1
//        /// </summary>
//        [HttpPut("{navId}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.Edit)]
//        public async Task<IActionResult> Update([FromRoute] int navId, [FromBody] ManagerMenuEditDto modelDto)
//        {
//            //查找菜单信息
//            var model = await _managerMenuService.QueryAsync<ManagerMenu>(x => x.Id == navId);
//            if (model == null)
//            {
//                return BadRequest(ResponseMessage.Error($"数据不存在或已删除"));
//            }
//            //检查站点名称是否重复
//            if (model.Name?.ToLower() != modelDto.Name?.ToLower()
//                && await _managerMenuService.ExistsAsync<Sites>(x => x.Name!.ToLower() == modelDto.Name!.ToLower()))
//            {
//                return BadRequest(new ResponseMessage(ErrorCode.RepeatField, $"菜单名称[{modelDto.Name}]已存在"));
//            }
//            //更新操作AutoMapper替我们完成，只需要调用保存即可
//            _mapper.Map(modelDto, model);
//            var result = await _managerMenuService.SaveAsync();
//            return NoContent();
//        }

//        /// <summary>
//        /// 局部更新一条记录
//        /// 示例：/admin/manager/menu/1
//        /// Body：[{"op":"replace","path":"/title","value":"new title"}]
//        /// </summary>
//        [HttpPatch("{navId}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.Edit)]
//        public async Task<IActionResult> Update([FromRoute] int navId, [FromBody] JsonPatchDocument<ManagerMenuEditDto> patchDocument)
//        {
//            var model = await _managerMenuService.QueryAsync<ManagerMenu>(x => x.Id == navId);
//            if (model == null)
//            {
//                return BadRequest(ResponseMessage.Error($"数据[{navId}]不存在或已删除"));
//            }

//            var modelToPatch = _mapper.Map<ManagerMenuEditDto>(model);
//            patchDocument.ApplyTo(modelToPatch, ModelState);
//            //验证数据是否合法
//            if (!TryValidateModel(modelToPatch))
//            {
//                return ValidationProblem(ModelState);
//            }
//            //更新操作AutoMapper替我们完成，只需要调用保存即可
//            _mapper.Map(modelToPatch, model);
//            await _managerMenuService.SaveAsync();

//            return NoContent();
//        }

//        /// <summary>
//        /// 删除一条记录
//        /// 示例：/admin/manager/menu/1
//        /// </summary>
//        [HttpDelete("{navId}")]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.Delete)]
//        public async Task<IActionResult> Delete([FromRoute] int navId)
//        {
//            if (!await _managerMenuService.ExistsAsync<ManagerMenu>(x => x.Id == navId))
//            {
//                return BadRequest(ResponseMessage.Error($"数据[{navId}]不存在或已删除"));
//            }
//            var result = await _managerMenuService.DeleteAsync(x => x.Id == navId);

//            return NoContent();
//        }

//        /// <summary>
//        /// 批量删除记录(级联数据)
//        /// 示例：/admin/manager/menu?ids=1,2,3
//        /// </summary>
//        [HttpDelete]
//        [Authorize]
//        [AuthorizeFilter("ManagerMenu", ActionType.Delete)]
//        public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
//        {
//            if (Ids == null)
//            {
//                return BadRequest(ResponseMessage.Error("传输参数不可为空"));
//            }
//            //将ID列表转换成IEnumerable
//            var areaIds = Ids.ToIEnumerable<int>();
//            if (areaIds == null)
//            {
//                return BadRequest(ResponseMessage.Error("传输参数不符合规范"));
//            }
//            //执行批量删除操作
//            await _managerMenuService.DeleteAsync(x => areaIds.Contains(x.Id));

//            return NoContent();
//        }
//        #endregion

//        #region 当前账户调用接口========================
//        /// <summary>
//        /// 获取管理员导航树目录列表
//        /// 示例：/account/manager/menu
//        /// </summary>
//        [HttpGet("/account/manager/menu")]
//        [Authorize]
//        public async Task<IActionResult> GetNavList([FromQuery] BaseParameter param)
//        {
//            //检测参数是否合法
//            if (!param.Fields.IsPropertyExists<ManagerMenuDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }
//            //获取数据库列表
//            var resultFrom = await _managerMenuService.QueryListAsync();
//            //使用AutoMapper转换成ViewModel
//            //根据字段进行塑形
//            var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
//            //返回成功200
//            return Ok(resultDto);
//        }
//        #endregion
//    }
//}