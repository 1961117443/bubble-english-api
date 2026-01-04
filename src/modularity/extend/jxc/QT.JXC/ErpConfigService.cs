using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Model.Organize;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

public class ErpConfigService : IErpConfigService,ITransient
{
    private readonly IUserManager _userManager;
    private readonly ISqlSugarRepository<ErpCustomerEntity> _repository;
    private readonly ISysConfigService _sysConfigService;

    public ErpConfigService(IUserManager userManager,ISqlSugarRepository<ErpCustomerEntity> repository,ISysConfigService sysConfigService)
    {
        _userManager = userManager;
        _repository = repository;
        _sysConfigService = sysConfigService;
    }

     
    public async Task ValidCustomerOrder()
    {
        // 判断是否为客户账号
        if (await _repository.AnyAsync(x => x.Oid == _userManager.CompanyId && x.LoginId == _userManager.Account))
        {
            // 判断当前公司是否允许下单
            var entity = await _repository.Context.Queryable<OrganizeEntity>().SingleAsync(x => x.Id == _userManager.CompanyId);
            if (entity!=null && entity.PropertyJson.IsNotEmptyOrNull())
            {
                var extend = entity.PropertyJson.ToObject<OrganizePropertyModel>();
                if (!extend.enableOrder)
                {
                    throw Oops.Oh("当前时间段禁止下单");
                }
            }

            var info = await _sysConfigService.GetInfo();
            if (info.erpDisEnableCustomerOrder == 1)
            {
                //启用禁用时间
                var now = DateTime.Now;

                var nowWeek = now.DayOfWeek;

                // 根据开始禁止时间段，计算开始时间
                var startWeek = info.erpDisEnableCustomerOrder_StartWeek;
                var endWeek = info.erpDisEnableCustomerOrder_EndWeek;

               

                int daysUntilMonday = (startWeek - (int)now.DayOfWeek + 7) % 7;
                DateTime st = now.AddDays(daysUntilMonday).Date;

                daysUntilMonday = (endWeek - (int)now.DayOfWeek + 7) % 7;

                if (endWeek > 0 && startWeek > endWeek)
                {
                    daysUntilMonday += 7;
                }

                DateTime et = now.AddDays(daysUntilMonday).Date;

                if (info.erpDisEnableCustomerOrder_StartTime > 0)
                {
                    var dt = info.erpDisEnableCustomerOrder_StartTime.Value.TimeStampToDateTime();
                    st = st.AddSeconds(dt.TimeOfDay.TotalSeconds);
                }

                if (info.erpDisEnableCustomerOrder_EndTime > 0)
                {
                    var dt = info.erpDisEnableCustomerOrder_EndTime.Value.TimeStampToDateTime();
                    et = et.AddSeconds(dt.TimeOfDay.TotalSeconds);
                }

                if (now >= st && now<=et)
                {
                    throw Oops.Oh("当前时间段禁止客户下单！");
                }
            }
        }
    }
}
