using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 平台证书实体
/// </summary>
public class WeChatPayCertificateDto
{
    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNo { get; set; }

    /// <summary>
    /// 生效时间
    /// </summary>
    public DateTime EffectiveTime { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 证书
    /// </summary>
    public X509Certificate2? Certificate;
}

/// <summary>
/// 平台证书列表
/// </summary>
public class CertificateList
{
    /// <summary>
    /// 证书列表
    /// </summary>
    [JsonProperty("data")]
    public IList<Certificate> Data { get; set; } = new List<Certificate>();
}

/// <summary>
/// 平台证书信息
/// </summary>
public class Certificate
{
    /// <summary>
    /// 序列号
    /// </summary>
    [JsonProperty("serial_no")]
    public string? SerialNo { get; set; }

    /// <summary>
    /// 生效时间
    /// </summary>
    [JsonProperty("effective_time")]
    public string? EffectiveTime { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    [JsonProperty("expire_time")]
    public string? ExpireTime { get; set; }

    /// <summary>
    /// 加密证书
    /// </summary>
    [JsonProperty("encrypt_certificate")]
    public EncryptCertificate? EncryptCertificate { get; set; }
}

/// <summary>
/// 加密证书信息
/// </summary>
public class EncryptCertificate
{
    /// <summary>
    /// 加密算法类型
    /// </summary>
    [JsonProperty("algorithm")]
    public string? Algorithm { get; set; }

    /// <summary>
    /// 随机串
    /// </summary>
    [JsonProperty("nonce")]
    public string? Nonce { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    [JsonProperty("associated_data")]
    public string? AssociatedData { get; set; }

    /// <summary>
    /// 数据密文
    /// </summary>
    [JsonProperty("ciphertext")]
    public string? Ciphertext { get; set; }
}
