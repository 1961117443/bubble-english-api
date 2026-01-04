using System;
using System.Collections.Generic;
using System.Text;

namespace QT.JZRC.Entitys.Dto.AppService.Login;

/// <summary>
/// 通讯密钥Token
/// </summary>
public class Tokens
{
    public Tokens() { }
    public Tokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
    }
    /// <summary>
    /// 正式使用的Token
    /// </summary>
    public string? AccessToken { get; set; }
    /// <summary>
    /// 用于刷新的Token
    /// </summary>
    public string? RefreshToken { get; set; }
}
