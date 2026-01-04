using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QT.Common.Configuration;
using QT.Common.Core.Captcha.General;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Options;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logging.Attributes;
using QT.RemoteRequest.Extensions;
using QT.Systems.Interfaces.Common;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace QT.Systems.Common;

/// <summary>
/// 业务实现：通用控制器.
/// </summary>
[ApiDescriptionSettings(Tag = "Common", Name = "File", Order = 161)]
[Route("api/[controller]")]
[AllowAnonymous]
[IgnoreLog]
public class FileService : IFileService, IDynamicApiController, ITransient
{
    private readonly AppOptions _appOptions;

    /// <summary>
    /// 验证码处理程序.
    /// </summary>
    private readonly IGeneralCaptcha _captchaHandler;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个<see cref="FileService"/>类型的新实例.
    /// </summary>
    public FileService(
        IOptions<AppOptions> appOptions,
        IGeneralCaptcha captchaHandler,
        IUserManager userManager,
        IFileManager fileManager)
    {
        _appOptions = appOptions.Value;
        _captchaHandler = captchaHandler;
        _userManager = userManager;
        _fileManager = fileManager;
    }

    #region GET

    /// <summary>
    /// 上传文件预览 (doc/docx/xls/xlsx/ppt/pptx/pdf).
    /// </summary>
    /// <returns></returns>
    [HttpGet("Uploader/Preview")]
    public async Task<dynamic> Preview(string fileName)
    {
        string[]? typeList = new string[] { "doc", "docx", "xls", "xlsx", "ppt", "pptx", "pdf" };
        string? type = fileName.Split('.').LastOrDefault();
        if (typeList.Contains(type))
        {
            if (fileName.IsNotEmptyOrNull())
            {
                string previewUrl = string.Empty;
                switch (_appOptions.PreviewType)
                {
                    case PreviewType.kkfile:
                        previewUrl = KKFileUploaderPreview(fileName);
                        break;
                    case PreviewType.yozo:
                        previewUrl = await YoZoUploaderPreview(fileName, 5, 1);
                        break;
                }

                return previewUrl;
            }
            else
            {
                throw Oops.Oh(ErrorCode.D8000);
            }
        }
        else
        {
            throw Oops.Oh(ErrorCode.D1802);
        }
    }

    /// <summary>
    /// 生成图片链接.
    /// </summary>
    /// <param name="type">图片类型.</param>
    /// <param name="fileName">注意 后缀名前端故意把 .替换@ .</param>
    /// <returns></returns>
    [HttpGet("Image/{type}/{fileName}")]
    public async Task<IActionResult> GetImg(string type, string fileName)
    {
        string? filePath = Path.Combine(GetPathByType(type), fileName.Replace("@", "."));
        return await _fileManager.DownloadFileByType(filePath, fileName);
    }

    /// <summary>
    /// 生成大屏图片链接.
    /// </summary>
    /// <param name="type">图片类型.</param>
    /// <param name="fileName">注意 后缀名前端故意把 .替换@ .</param>
    /// <returns></returns>
    [HttpGet("VisusalImg/{type}/{fileName}")]
    public async Task<IActionResult> GetScreenImg(string type, string fileName)
    {
        string filePath = Path.Combine(GetPathByType(type), type, fileName.Replace("@", "."));
        return await _fileManager.DownloadFileByType(filePath, fileName);
    }

    /// <summary>
    /// 获取图形验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    [HttpGet("ImageCode/{timestamp}")]
    [NonUnify]
    public async Task<IActionResult> GetCode(string timestamp)
    {
        return new FileContentResult(await _captchaHandler.CreateCaptchaImage(timestamp, 114, 32), "image/jpeg");
    }

    /// <summary>
    /// 下载.
    /// </summary>
    /// <param name="fileName"></param>
    [HttpGet("down/{fileName}")]
    public async Task FileDown(string fileName)
    {
        string? systemFilePath = Path.Combine(FileVariable.SystemFilePath , fileName);
        var fileStreamResult = await _fileManager.DownloadFileByType(systemFilePath, fileName);
        byte[] bytes = new byte[fileStreamResult.FileStream.Length];

        fileStreamResult.FileStream.Read(bytes, 0, bytes.Length);

        fileStreamResult.FileStream.Close();
        var httpContext = App.HttpContext;
        httpContext.Response.ContentType = "application/octet-stream";
        httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
        httpContext.Response.Headers.Add("Content-Length", bytes.Length.ToString());
        httpContext.Response.Body.WriteAsync(bytes);
        httpContext.Response.Body.Flush();
        httpContext.Response.Body.Close();
    }

    #region 下载附件

    /// <summary>
    /// 获取下载文件链接.
    /// </summary>
    /// <param name="type">图片类型.</param>
    /// <param name="fileName">文件名称.</param>
    /// <returns></returns>
    [HttpGet("Download/{type}/{fileName}")]
    public dynamic DownloadUrl(string type, string fileName)
    {
        string? url = string.Format("{0}|{1}|{2}|{3}", _userManager.UserId, fileName, type, _userManager.TenantId);
        string? encryptStr = DESCEncryption.Encrypt(url, "QT");
        return new { name = fileName, url = string.Format("/api/file/Download?encryption={0}", encryptStr) };
    }

    /// <summary>
    /// 下载文件链接.
    /// </summary>
    [HttpGet("Download")]
    //[ResponseCache(Duration = int.MaxValue)]
    public async Task<dynamic> DownloadFile([FromQuery] string encryption)
    {
        string decryptStr = DESCEncryption.Decrypt(encryption, "QT");
        List<string> paramsList = decryptStr.Split("|").ToList();
        if (paramsList.Count > 0)
        {
            string fileName = paramsList.Count > 1 ? paramsList[1] : string.Empty;
            string type = paramsList.Count > 2 ? paramsList[2] : string.Empty;
            string tenantId = paramsList.Count > 3 ? paramsList[3] : string.Empty;
            if (tenantId.IsNotEmptyOrNull())
            {
                TenantScoped.LoginScoped(tenantId, App.HttpContext?.RequestServices);
            }
            string filePath = Path.Combine(GetPathByType(type), fileName.Replace("@", "."));
            string fileDownloadName = fileName.Replace(GetPathByType(type), string.Empty);

            if (App.HttpContext != null && App.HttpContext.Request.Query.TryGetValue("name",out var name) && name.IsNotEmptyOrNull())
            {
                fileDownloadName = name!;
            }
            return await _fileManager.DownloadFileByType(filePath, fileDownloadName);
        }
        else
        {
            throw Oops.Oh(ErrorCode.D8000);
        }
    }

    /// <summary>
    /// 下载文件链接.
    /// </summary>
    [HttpGet("Download/{encryption}")]
    [ResponseCache(Duration = int.MaxValue)]
    public async Task<dynamic> DownloadFileCache([FromRoute] string encryption)
    {
        return await DownloadFile(encryption);
    }

    /// <summary>
    /// App启动信息.
    /// </summary>
    [HttpGet("AppStartInfo/{appName}")]
    public async Task<dynamic> AppStartInfo(string appName)
    {
        return new { appVersion = KeyVariable.AppVersion, appUpdateContent = KeyVariable.AppUpdateContent };
    }

    #endregion

    /// <summary>
    /// 分片上传获取.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("chunk")]
    public async Task<dynamic> CheckChunk([FromQuery] ChunkModel input)
    {
        try
        {
            if (!AllowFileType(input.extension, input.extension))
                throw Oops.Oh(ErrorCode.D1800);
            string path = GetPathByType(string.Empty);
            string filePath = Path.Combine(path, $"{input.identifier}{MD5Encryption.Encrypt(input.fileName)}");
            var chunkFiles = await _fileManager.GetObjList(filePath);
            List<int> existsChunk = chunkFiles.FindAll(x => !x.FileType.Equals("tmp"))
                .Select(x => x.FileName.Replace(input.identifier + "-", string.Empty).ParseToInt()).ToList();

            return new { chunkNumbers = existsChunk, merge = existsChunk.Count == input.totalChunks };
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    #endregion

    #region POST

    /// <summary>
    /// 上传文件/图片.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Uploader/{type}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> Uploader(string type, IFormFile file)
    {
        string? fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!AllowFileType(fileType, type))
            throw Oops.Oh(ErrorCode.D1800);
        string filePath = GetPathByType(type);
        string fileName = string.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMdd"), SnowflakeIdHelper.NextId(), Path.GetExtension(file.FileName));
        var stream = file.OpenReadStream();
        var flag = await _fileManager.UploadFileByType(stream, filePath, fileName);
        return new { name = fileName, url = flag.Item2.IsNullOrEmpty() ? string.Format("/api/File/Image/{0}/{1}", type, fileName) : flag.Item2 };
    }

    /// <summary>
    /// 上传图片.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Uploader/userAvatar")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> UploadImage(IFormFile file)
    {
        string? ImgType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!this.AllowImageType(ImgType))
            throw Oops.Oh(ErrorCode.D5013);
        string? filePath = FileVariable.UserAvatarFilePath;
        string? fileName = string.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMdd"), SnowflakeIdHelper.NextId(), Path.GetExtension(file.FileName));
        var stream = file.OpenReadStream();
        var flag =  await _fileManager.UploadFileByType(stream, filePath, fileName);
        return new { name = fileName, url = flag.Item2 ?? string.Format("/api/file/Image/userAvatar/{0}", fileName) };
    }

    /// <summary>
    /// 分片上传附件.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("chunk")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> UploadChunk([FromForm] ChunkModel input)
    {
        if (!AllowFileType(input.extension, input.extension))
            throw Oops.Oh(ErrorCode.D1800);
        return await _fileManager.UploadChunk(input);
    }

    /// <summary>
    /// 分片组装.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("merge")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> Merge([FromForm] ChunkModel input)
    {
        return await _fileManager.Merge(input);
    }
    #endregion

    #region PublicMethod

    #region 多种存储文件

    /// <summary>
    /// 根据存储类型上传文件.
    /// </summary>
    /// <param name="uploadFilePath">上传文件地址.</param>
    /// <param name="directoryPath">保存文件夹.</param>
    /// <param name="fileName">新文件名.</param>
    /// <returns></returns>
    [NonAction]
    public async Task UploadFileByType(string uploadFilePath, string directoryPath, string fileName)
    {
        FileStream? file = new FileStream(uploadFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await _fileManager.UploadFileByType(file, directoryPath, fileName);
    }
    #endregion

    /// <summary>
    /// 根据类型获取文件存储路径.
    /// </summary>
    /// <param name="type">文件类型.</param>
    /// <returns></returns>
    [NonAction]
    public string GetPathByType(string type)
    {
        return _fileManager.GetPathByType(type);
    }

    #region kkfile 文件预览

    /// <summary>
    /// KKFile 文件预览.
    /// </summary>
    /// <param name="fileName">文件名称.</param>
    /// <returns></returns>
    public string KKFileUploaderPreview(string fileName)
    {
        var domain = App.Configuration["QT_APP:Domain"];
        var filePath = (domain + "/api/File/down/" + fileName).ToBase64String();

        var kkFileDoMain = App.Configuration["QT_APP:KKFileDomain"];
        var kkurl = kkFileDoMain + "/onlinePreview?url=";

        return kkurl + filePath;
    }

    #endregion

    #region YoZo 生成 sign 方法

    /// <summary>
    /// 调用YoZo 文件预览.
    /// </summary>
    /// <param name="fileName">文件名.</param>
    /// <param name="maxNumber">最多请求次数.</param>
    /// <param name="number">当前请求次数.</param>
    /// <returns></returns>
    public async Task<string> YoZoUploaderPreview(string fileName, int maxNumber, int number)
    {
        string domain = _appOptions.YOZO.Domain;
        string uploadAPI = _appOptions.YOZO.UploadAPI;
        string downloadAPI = _appOptions.YOZO.DownloadAPI;
        string yozoAppId = _appOptions.YOZO.AppId;
        string yozoAppKey = _appOptions.YOZO.AppKey;
        string outputFilePath = string.Format("{0}/api/File/Image/annex/{1}", domain, fileName);

        Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
        dic.Add("fileUrl", new string[] { outputFilePath });
        dic.Add("appId", new string[] { yozoAppId });
        string? sign = generateSign(yozoAppKey, dic);
        uploadAPI = string.Format(uploadAPI, outputFilePath, yozoAppId, sign);
        string? resStr = await uploadAPI.PostAsStringAsync();
        if (resStr.IsNotEmptyOrNull())
        {
            Dictionary<string, object>? result = resStr.ToObject<Dictionary<string, object>>();
            if (result.ContainsKey("data"))
            {
                Dictionary<string, object>? data = result["data"].ToObject<Dictionary<string, object>>();
                if (data != null)
                {
                    string? fileVersionId = data.ContainsKey("fileVersionId") ? data["fileVersionId"].ToString() : string.Empty;

                    #region 生成签名sign

                    dic = new Dictionary<string, string[]>();
                    dic.Add("fileVersionId", new string[] { fileVersionId });
                    dic.Add("appId", new string[] { yozoAppId });
                    sign = generateSign(yozoAppKey, dic);

                    #endregion

                    return string.Format(downloadAPI, fileVersionId, yozoAppId, sign);
                }
                else
                {
                    return await YoZoUploaderPreview(fileName, maxNumber, number + 1);
                }
            }
            else
            {
                if (number >= maxNumber) return string.Empty;
                else return await YoZoUploaderPreview(fileName, maxNumber, number + 1);
            }
        }
        else
        {
            if (number >= maxNumber) return string.Empty;
            else return await YoZoUploaderPreview(fileName, maxNumber, number + 1);
        }
    }

    #endregion

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 生成令牌.
    /// </summary>
    /// <param name="secret">签名.</param>
    /// <param name="paramMap">参数集合.</param>
    /// <returns></returns>
    private string generateSign(string secret, Dictionary<string, string[]> paramMap)
    {
        string fullParamStr = uniqSortParams(paramMap);
        return HmacSHA256(fullParamStr, secret);
    }

    /// <summary>
    /// uniq类型参数.
    /// </summary>
    /// <param name="paramMap"></param>
    /// <returns></returns>
    private string uniqSortParams(Dictionary<string, string[]> paramMap)
    {
        paramMap.Remove("sign");
        paramMap = paramMap.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        StringBuilder strB = new StringBuilder();
        foreach (KeyValuePair<string, string[]> kvp in paramMap)
        {
            string key = kvp.Key;
            string[] value = kvp.Value;
            if (value.Length > 0)
            {
                Array.Sort(value);
                foreach (string temp in value)
                {
                    strB.Append(key).Append("=").Append(temp);
                }
            }
            else
            {
                strB.Append(key).Append("=");
            }

        }

        return strB.ToString();
    }

    /// <summary>
    /// 加密.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private string HmacSHA256(string data, string key)
    {
        string signRet = string.Empty;
        using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(data));
            signRet = hash.ToHexString();
        }

        return signRet;
    }

    /// <summary>
    /// 允许文件类型.
    /// </summary>
    /// <param name="fileExtension">文件后缀名.</param>
    /// <param name="type">文件类型.</param>
    /// <returns></returns>
    private bool AllowFileType(string fileExtension, string type)
    {
        List<string>? allowExtension = KeyVariable.AllowUploadFileType;
        if (type.Equals("weixin"))
            allowExtension = KeyVariable.WeChatUploadFileType;
        return allowExtension.Any(a => a == fileExtension.ToLower());
    }

    /// <summary>
    /// 允许文件类型.
    /// </summary>
    /// <param name="fileExtension">文件后缀名.</param>
    /// <returns></returns>
    private bool AllowImageType(string fileExtension)
    {
        return KeyVariable.AllowImageType.Any(a => a == fileExtension.ToLower());
    }

    #endregion
}