//namespace QTMall.Core.API.Controllers
//{
//    /// <summary>
//    /// 管理日志
//    /// </summary>
//    [Route("admin/manager/log")]
//    [ApiController]
//    public class ManagerLogController : ControllerBase
//    {
//        private readonly IManagerLogService _managerLogService;
//        private readonly IMapper _mapper;

//        /// <summary>
//        /// 依赖注入
//        /// </summary>
//        public ManagerLogController(IManagerLogService managerLogService, IMapper mapper)
//        {
//            _managerLogService = managerLogService;
//            _mapper = mapper;
//        }

//        #region 管理员调用接口==========================
//        /// <summary>
//        /// 获取分页列表
//        /// 示例：/admin/manager/log?pageSize=10&pageIndex=1
//        /// </summary>
//        [HttpGet]
//        [Authorize]
//        [AuthorizeFilter("ManagerLog", ActionType.View)]
//        public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
//        {
//            //检测参数是否合法
//            if (searchParam.OrderBy!=null
//                && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<ManagerLogDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的排序参数"));
//            }
//            if (!searchParam.Fields.IsPropertyExists<ManagerLogDto>())
//            {
//                return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
//            }

//            //获取数据列表
//            var list = await _managerLogService.QueryPageAsync<ManagerLog>(
//                pageParam.PageSize,
//                pageParam.PageIndex,
//                x => !searchParam.Keyword.IsNotNullOrEmpty()
//                    || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))
//                    || (x.Method != null && x.Method.Contains(searchParam.Keyword)),
//                searchParam.OrderBy ?? "-Id");

//            //x-pagination
//            var paginationMetadata = new
//            {
//                totalCount = list.TotalCount,
//                pageSize = list.PageSize,
//                pageIndex = list.PageIndex,
//                totalPages = list.TotalPages
//            };
//            Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

//            //映射成DTO
//            var resultDto = _mapper.Map<IEnumerable<ManagerLogDto>>(list).ShapeData(searchParam.Fields);
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 批量删除记录
//        /// 示例：/admin/manager/log
//        /// </summary>
//        [HttpDelete]
//        [Authorize]
//        [AuthorizeFilter("ManagerLog", ActionType.Delete)]
//        public async Task<IActionResult> Delete()
//        {
//            //计算7天前的日期
//            var lastDate = DateTime.Now.AddDays(-7);
//            //删除7天之后的日志
//            await _managerLogService.DeleteAsync<ManagerLog>(x => DateTime.Compare(x.AddTime, lastDate) <= 0);
//            return NoContent();
//        }
//        #endregion
//    }
//}
