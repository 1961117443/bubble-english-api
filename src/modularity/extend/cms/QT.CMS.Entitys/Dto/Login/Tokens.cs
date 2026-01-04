using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Login;

/// <summary>
/// 通讯密钥Token
/// </summary>
public class Tokens
{
    public Tokens() { }
    public Tokens(string accessToken, string refreshToken)
    {
        this.accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        this.refreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
    }
    /// <summary>
    /// 正式使用的Token
    /// </summary>
    public string? accessToken { get; set; }
    /// <summary>
    /// 用于刷新的Token
    /// </summary>
    public string? refreshToken { get; set; }
}
