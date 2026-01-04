using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Config;
using QT.Common.Const;
using QT.Common.Security;
using QT.DataValidation;
using QT.DynamicApiController;
using QT.JsonSerialization;
using QT.Systems.Common;
using Serilog;

namespace QT.CMS;

/// <summary>
/// 站点接口
/// </summary>
//[Route("admin/site")]
[Route("api/cms/admin/[controller]")]
//[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS)]
[AllowAnonymous]
public class ContactController : IDynamicApiController
{
    private readonly ISqlSugarRepository<Contacts> _repository;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 构造函数依赖注入
    /// </summary>
    public ContactController(ISqlSugarRepository<Contacts> repository,ICacheManager cacheManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 获取咨询列表
    /// 示例：/admin/site?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] ContactParameter searchParam/*, [FromQuery] PageInputBase pageParam*/)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<ContactsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ContactsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _repository.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.AddBy.Contains(searchParam.keyword) || x.Phone.Contains(searchParam.keyword) || x.Subject.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "AddTime desc")
           .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _siteService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "SortId,Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        //映射成DTO
        //var dto = list.Adapt<SqlSugarPagedList<SitesDto>>();
        //var resultDto = dto.ShapeData(searchParam.Fields);
        var resultDto = PageResult<ContactsDto>.SqlSugarPageResult(list);
        //var x = PageResult<SitesDto>.SqlSugarPageResult(dto);
        return (resultDto);
    }

    #region 前台调用接口============================
    /// <summary>
    /// 提交信息联系客服
    /// 示例：/client/contact
    /// </summary>
    [HttpPost("/client/contact/{id}")]
    [AllowAnonymous]
    public async Task Create([FromRoute] int id, [FromBody] ContactsEditDto param)
    {
        if (param.addBy.IsNullOrEmpty())
        {
            throw Oops.Oh($"姓名不可为空");
        }
        if (param.phone.IsNullOrEmpty())
        {
            throw Oops.Oh($"手机号码不可为空");
        }

        if (!param.phone.TryValidate(ValidationTypes.PhoneNumber).IsValid)
        {
            throw Oops.Oh("手机号码格式不正确");
        }
        if (param.code.IsNullOrEmpty())
        {
            throw Oops.Oh($"验证码不可为空");
        }

        var codeKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, param.phone);
        string code = await _cacheManager.GetAsync<string>(codeKey);
        if (code!=param.code)
        {
            throw Oops.Oh($"验证码错误！");
        }
        await _cacheManager.DelAsync(codeKey);
        //查询数据库获取实体
        Sites? model = await _repository.Context.Queryable<Sites>().SingleAsync(x => x.Id == id);
        //查询数据库获取实体
        if (model == null)
        {
            throw Oops.Oh($"站点不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var entity = param.Adapt<Contacts>();
        entity.IP = NetHelper.Ip;
        entity.SiteId = model.Id;
       
        var cacheKey = $"{entity.IP}:{entity.Phone}";
        // 5秒内只能提交一次
        if (_cacheManager.SetNx(cacheKey, entity, TimeSpan.FromSeconds(5)))
        {
            var phoneNum = "13521062888";
            //var phoneNum = "13536688942";
            var config = new SysConfigDto();
            try
            {
                //取得站点配置信息
                //var sysConfig = await _repository.Context.Queryable<Entitys.SysConfig>().SingleAsync(x => x.Type == ConfigType.SysConfig.ToString());
                //if (sysConfig == null)
                //{
                //    throw Oops.Oh("系统配置信息不存在");
                //}
                //config = JSON.Deserialize<SysConfigDto>(sysConfig.JsonData);
                //if (config == null)
                //{
                //    throw Oops.Oh("系统配置信息格式有误");
                //}
                //if (config.webTel.IsNotEmptyOrNull())
                //{
                //    phoneNum = config.webTel;
                //}

                // 发送短信
                //var msg = await SmsHelper.SendByCode("cms.consult", phoneNum, new { consignee = param.addBy, phone= param.phone });

                // 特定的验证码通知管理员 验证码格式 月+日+序号
                var num = await _repository.Context.Queryable<Contacts>()
                    .Where(x => x.SiteId == entity.SiteId)
                    .CountAsync();
                var codev = $"{DateTime.Now.ToString("MMdd")}{num + 1}";
                var msg = await SmsHelper.SendByCode("login.verificationcode", phoneNum, new { code = codev });
                if (msg.IsNotEmptyOrNull())
                {
                    entity.IsSend = 1;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
           

            await _repository.InsertAsync(entity);
        }
        else
        {
            throw Oops.Oh("请稍候再试！");
        }
    }

    #endregion

}