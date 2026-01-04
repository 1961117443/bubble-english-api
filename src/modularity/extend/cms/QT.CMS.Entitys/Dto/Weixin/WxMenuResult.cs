using Newtonsoft.Json;

namespace QT.CMS.Entitys.Dto.Weixin;

/// <summary>
/// 微信公众号菜单(查询)
/// </summary>
public class WxMenuResult : WxJsonResult
{
    /// <summary>
    /// 菜单是否开启，0代表未开启，1代表开启
    /// </summary>
    [JsonProperty("is_menu_open")]
    public int isMenuOpen { get; set; }

    /// <summary>
    /// 菜单信息
    /// </summary>
    [JsonProperty("selfmenu_info")]
    public WxMenuResultSelfMenuInfo? selfMenuInfo { get; set; }
}

/// <summary>
/// 菜单信息(查询)
/// </summary>
public class WxMenuResultSelfMenuInfo
{
    /// <summary>
    /// 菜单按钮
    /// </summary>
    [JsonProperty("button")]
    public ICollection<WxMenuResultButton> button { get; set; } = new List<WxMenuResultButton>();
}

/// <summary>
/// 菜单按钮(查询)
/// </summary>
public class WxMenuResultButton : WxMenuButtonBase
{
    /// <summary>
    /// 二级菜单
    /// </summary>
    [JsonProperty("sub_button")]
    public WxMenuResultSubButton? subButton { get; set; }
}

/// <summary>
/// 子菜单信息(查询)
/// </summary>
public class WxMenuResultSubButton
{
    /// <summary>
    /// 子菜单列表
    /// </summary>
    [JsonProperty("list")]
    public ICollection<WxMenuButtonBase> list { get; set; } = new List<WxMenuButtonBase>();
}


/// <summary>
/// 微信公众号菜单(编辑)
/// </summary>
public class WxMenuCreate : WxJsonResult
{
    /// <summary>
    /// 菜单列表
    /// </summary>
    [JsonProperty("button")]
    public ICollection<WxMenuCreateButton> button { get; set; } = new List<WxMenuCreateButton>();
}

/// <summary>
/// 菜单按钮(编辑)
/// </summary>
public class WxMenuCreateButton : WxMenuButtonBase
{
    /// <summary>
    /// 子菜单列表
    /// </summary>
    [JsonProperty("sub_button")]
    public ICollection<WxMenuButtonBase> subButton { get; set; } = new List<WxMenuButtonBase>();
}


/// <summary>
/// 菜单基本属性
/// </summary>
public class WxMenuButtonBase
{
    /// <summary>
    /// 菜单的响应动作类型
    /// 公众平台官网上能够设置的菜单类型有view（跳转网页）、text（返回文本，下同）、img、photo、video、voice。使用 API 设置的则有8种
    /// </summary>
    [JsonProperty("type")]
    public string? type { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [JsonProperty("name")]
    public string? name { get; set; }

    /// <summary>
    /// 类型为click、scancode_push、scancode_waitmsg、pic_sysphoto、pic_photo_or_album、 pic_weixin、location_select：保存值到key
    /// </summary>
    [JsonProperty("key")]
    public string? key { get; set; }

    /// <summary>
    /// 类型为view：保存链接到url
    /// </summary>
    [JsonProperty("url")]
    public string? url { get; set; }

    /// <summary>
    /// 小程序的appid（仅认证公众号可配置）
    /// 类型Type为miniprogra(小程序)必填
    /// </summary>
    [JsonProperty("appid")]
    public string? appId { get; set; }

    /// <summary>
    /// 小程序的页面路径
    /// 类型Type为miniprogra(小程序)必填
    /// </summary>
    [JsonProperty("pagepath")]
    public string? pagePath { get; set; }

    /// <summary>
    /// 调用新增永久素材接口返回的合法media_id
    /// media_id类型和view_limited类型必须
    /// </summary>
    [JsonProperty("media_id")]
    public string? mediaId { get; set; }

    /// <summary>
    /// 发布后获得的合法 article_id
    /// article_id类型和article_view_limited类型必须
    /// </summary>
    [JsonProperty("article_id")]
    public string? articleId { get; set; }
}
