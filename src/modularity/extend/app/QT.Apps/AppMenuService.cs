using QT.Apps.Entitys.Dto;
using QT.Apps.Interfaces;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using QT.Localization;

namespace QT.Apps;

/// <summary>
/// App菜单
/// </summary>
[ApiDescriptionSettings(Tag = "App", Name = "Menu", Order = 800)]
[Route("api/App/[controller]")]
public class AppMenuService : IDynamicApiController, ITransient
{
    /// <summary>
    /// App常用数据.
    /// </summary>
    private readonly IAppDataService _appDataService;

    /// <summary>
    /// 初始化一个<see cref="AppMenuService"/>类型的新实例.
    /// </summary>
    public AppMenuService(IAppDataService appDataService)
    {
        _appDataService = appDataService;
    }

    #region Get

    /// <summary>
    /// 获取菜单列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList(string keyword)
    {
        List<AppMenuListOutput>? list = (await _appDataService.GetAppMenuList(keyword)).Adapt<List<AppMenuListOutput>>();
        list = list.ToTree("-1");

        foreach (var item in list)
        {
            if (item.hasChildren && item.children != null)
            {
                item.children = item.children.OrderBy(x => ((AppMenuListOutput)x).sortCode).ToList();
            }
        }
        return new { list};
        //return new { list = list.ToTree("-1") };
    }

    #endregion
}