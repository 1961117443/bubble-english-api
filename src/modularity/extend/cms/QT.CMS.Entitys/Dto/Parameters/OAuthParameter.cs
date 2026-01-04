namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 第三方授权查询参数
/// </summary>
public class OAuthParameter : BaseParameter
{
    /// <summary>
    /// 接口类型
    /// 示例：web(网站)|mp(小程序)|app
    /// </summary>
    public string? Types { get; set; }
}
