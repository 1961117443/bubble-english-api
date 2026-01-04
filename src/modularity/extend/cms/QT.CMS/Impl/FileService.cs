using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Config;
using QT.CMS.Interfaces;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.JsonSerialization;
using Serilog;
using SqlSugar;
using System.Collections;
using System.Net;

namespace QT.CMS;

/// <summary>
/// 文件上传接口实现
/// </summary>
public class FileService: IFileService,IScoped
{
    private readonly ISqlSugarRepository<SysConfig> _configService;
    private readonly IFileManager _fileManager;

    public FileService(ISqlSugarRepository<SysConfig> configService,IFileManager fileManager)
    {
        _configService = configService;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 通过文件流上传文件方法
    /// </summary>
    /// <param name="byteData">文件字节数组</param>
    /// <param name="fileName">文件名</param>
    /// <param name="isThumbnail">是否生成缩略图</param>
    /// <param name="isWater">是否打水印</param>
    public async Task<FileDto> SaveAsync(byte[] byteData, string fileName, bool isThumbnail, bool isWater)
    {
        var config = new SysConfigDto();
        try
        {
            //取得站点配置信息
            var sysConfig = await _configService.SingleAsync(x => x.Type == ConfigType.SysConfig.ToString());
            if (sysConfig == null)
            {
                throw Oops.Oh("系统配置信息不存在");
            }
            config = JSON.Deserialize<SysConfigDto>(sysConfig.JsonData);
            if (config == null)
            {
                throw Oops.Oh("系统配置信息格式有误");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }

        string fileExt = Path.GetExtension(fileName).Trim('.'); //文件扩展名，不含“.”
        string newFileName = Guid.NewGuid() + "." + fileExt; //随机生成新的文件名
        string newThumbnailFileName = "thumb_" + newFileName; //随机生成缩略图文件名

        string upLoadPath = GetUpLoadPath(config); //本地上传目录相对路径
        string fullUpLoadPath = FileHelper.GetWebPath(upLoadPath); //本地上传目录的物理路径
        string newFilePath = upLoadPath + newFileName; //本地上传后的路径
        string newThumbnailPath = upLoadPath + newThumbnailFileName; //本地上传后的缩略图路径

        byte[]? thumbData = null; //缩略图文件流
        FileDto fileDto = new(); //返回的对象

        //检查文件字节数组是否为NULL
        if (byteData == null)
        {
            throw Oops.Oh("请选择要上传的文件");
        }
        //检查文件扩展名是否合法
        if (!CheckFileExt(config, fileExt))
        {
            throw Oops.Oh($"不允许上传{fileExt}类型的文件");
        }
        //检查文件大小是否合法
        if (!CheckFileSize(config, fileExt, byteData.Length))
        {
            throw Oops.Oh($"文件超过限制的大小");
        }

        //如果是图片，检查图片是否超出最大尺寸，是则裁剪
        if (IsImage(fileExt) && (config.imgMaxHeight > 0 || config.imgMaxWidth > 0))
        {
            byteData = ImageHelper.MakeThumbnail(byteData, fileExt, config.imgMaxWidth, config.imgMaxHeight);
        }
        //如果是图片，检查是否需要生成缩略图，是则生成
        if (IsImage(fileExt) && isThumbnail && config.thumbnailWidth > 0 && config.thumbnailHeight > 0)
        {
            thumbData = ImageHelper.MakeThumbnail(byteData, fileExt, config.thumbnailWidth, config.thumbnailHeight, config.thumbnailMode);
        }
        else
        {
            newThumbnailPath = newFilePath; //不生成缩略图则返回原图
        }
        //如果是图片，检查是否需要打水印
        if (IsWaterMark(config, fileExt) && isWater)
        {
            switch (config.watermarkType)
            {
                case 1:
                    byteData = ImageHelper.LetterWatermark(byteData, fileExt, config.watermarkText, config.watermarkPosition,
                        config.watermarkImgQuality, config.watermarkFont, config.watermarkFontSize);
                    break;
                case 2:
                    byteData = ImageHelper.ImageWatermark(byteData, fileExt,
                        FileHelper.GetWebPath(config.watermarkPic), config.watermarkPosition,
                        config.watermarkImgQuality, config.watermarkTransparency);
                    break;
            }
        }
        using (Stream stream = new MemoryStream(byteData))
        {
            var filePath = _fileManager.GetPathByType(string.Empty);
            var response = await _fileManager.UploadFileByType(stream, filePath, newFileName);

            //保存缩略图文件
            if (thumbData != null)
            {
                using (Stream newThumbnailStream = new MemoryStream(thumbData))
                {
                    var newThumbnailResponse = await _fileManager.UploadFileByType(newThumbnailStream, filePath, newThumbnailFileName);
                    if (isWater && newThumbnailResponse.Item1)
                    {
                        fileDto.thumbPath = new List<string>() { newThumbnailResponse.Item2 }; //如果有缩略图
                    }
                }
                
            }
            //返回上传路径
            if (response.Item1)
            {
                fileDto.filePath = response.Item2;
            }            
        }
        //返回成功信息
        fileDto.fileName = newFileName;
        fileDto.fileSize = byteData.Length;
        fileDto.fileExt = fileExt;
        return fileDto;
    }

    /// <summary>
    /// 裁剪图片并保存
    /// </summary>
    public async Task<FileDto> CropAsync(string fileUri, int maxWidth, int maxHeight, int cropWidth, int cropHeight, int X, int Y)
    {
        FileDto fileDto = new FileDto();//返回信息
        string fileExt = Path.GetExtension(fileUri).Trim('.'); //文件扩展名，不含“.”
        if (string.IsNullOrEmpty(fileExt) || !IsImage(fileExt))
        {
            throw Oops.Oh($"该文件不是图片");
        }

        byte[]? byteData = null;
        //判断是否远程文件
        if (fileUri.ToLower().StartsWith("http://") || fileUri.ToLower().StartsWith("https://"))
        {
            using HttpClient client = new();
            byteData = await client.GetByteArrayAsync(fileUri);
        }
        else //本地源文件
        {
            string fullName = FileHelper.GetWebPath(fileUri);
            if (File.Exists(fullName))
            {
                FileStream fs = File.OpenRead(fullName);
                BinaryReader br = new BinaryReader(fs);
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                byteData = br.ReadBytes((int)br.BaseStream.Length);
                fs.Close();
            }
        }
        if (byteData == null)
        {
            throw Oops.Oh($"无法获取原图片");
        }
        //裁剪后得到文件流
        byteData = ImageHelper.MakeThumbnail(byteData, fileExt, maxWidth, maxHeight, cropWidth, cropHeight, X, Y);
        fileDto = await SaveAsync(byteData, fileUri, false, false);
        await DeleteAsync(fileUri); //删除原图
        return fileDto;
    }

    /// <summary>
    /// 保存远程文件到本地
    /// </summary>
    /// <param name="sourceUri">URI地址</param>
    /// <returns>上传后的路径</returns>
    public async Task<FileDto> RemoteAsync(string sourceUri)
    {
        FileDto fileDto = new();//返回消息

        if (!IsExternalIPAddress(sourceUri))
        {
            throw Oops.Oh($"INVALID_URL");
        }
        using var response = await new HttpClient().GetAsync(sourceUri);
        if (response == null)
        {
            throw Oops.Oh($"抓取文件错误");
        }
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw Oops.Oh($"Url returns{response?.StatusCode}, {response?.RequestMessage}");
        }
        if (response.Content.Headers.ContentType?.MediaType?.IndexOf("image") == -1)
        {
            throw Oops.Oh($"Url is not an image");
        }
        byte[] byteData = await response.Content.ReadAsByteArrayAsync();
        return await SaveAsync(byteData, sourceUri, false, false);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public async Task<bool> DeleteAsync(string fileUri)
    {
        //取得站点配置信息
        var sysConfig = await _configService.SingleAsync(x=>x.Type ==ConfigType.SysConfig.ToString());
        if (sysConfig == null)
        {
            throw Oops.Oh("站点配置信息不存在");
        }
        var config = JSON.Deserialize<SysConfigDto>(sysConfig.JsonData);
        if (config == null)
        {
            throw Oops.Oh("站点配置信息格式有误");
        }
        bool result = false;

        //七牛云存储
        if (config.fileServer.Equals("qiniu"))
        {
            if (!config.kodoBucket.IsNotEmptyOrNull() || !config.kodoDomain.IsNotEmptyOrNull() || !config.kodoAccessKey.IsNotEmptyOrNull() || !config.kodoSecretKey.IsNotEmptyOrNull())
            {
                throw Oops.Oh($"七牛云存储参数未设置");
            }
            //替换成相对路径
            var filePath = fileUri.Replace(config.kodoDomain ?? String.Empty, string.Empty);
            throw new NotImplementedException();
            //result = QiniuHelper.DeleteFile(config.KodoAccessKey, config.KodoSecretKey, config.KodoBucket, filePath.TrimStart('/'));
        }
        //阿里云存储
        else if (config.fileServer.Equals("aliyun"))
        {
            if (!config.ossBucket.IsNotEmptyOrNull() || !config.ossDomain.IsNotEmptyOrNull() || !config.ossEndpoint.IsNotEmptyOrNull() || !config.ossAccessKey.IsNotEmptyOrNull() || !config.ossSecretKey.IsNotEmptyOrNull())
            {
                throw Oops.Oh($"阿里云存储参数未设置");
            }
            //替换成相对路径
            var filePath = fileUri.Replace(config.ossDomain ?? String.Empty, string.Empty);
            throw new NotImplementedException();
            //result = QiniuHelper.DeleteFile(config.KodoAccessKey, config.KodoSecretKey, config.KodoBucket, filePath.TrimStart('/'));
        }
        //本地存储
        else
        {
            result = FileHelper.DeleteFile(FileHelper.GetWebPath(fileUri));
        }
        return false;
    }

    #region 辅助私有方法
    /// <summary>
    /// 返回上传目录相对路径
    /// </summary>
    private string GetUpLoadPath(SysConfigDto config)
    {
        string path = $"/{config.filePath}/";
        switch (config.fileSave)
        {
            case 1: //按年月日每天一个文件夹
                path += DateTime.Now.ToString("yyyyMMdd");
                break;
            default: //按年月/日存入不同的文件夹
                path += DateTime.Now.ToString("yyyyMM") + "/" + DateTime.Now.ToString("dd");
                break;
        }
        return path + "/";
    }

    /// <summary>
    /// 是否需要打水印
    /// </summary>
    private bool IsWaterMark(SysConfigDto config, string fileExt)
    {
        //判断是否开启水印
        if (config.watermarkType > 0)
        {
            //判断是否可以打水印的图片类型
            ArrayList al = new ArrayList();
            al.Add("bmp");
            al.Add("jpeg");
            al.Add("jpg");
            al.Add("png");
            if (al.Contains(fileExt.ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否为图片文件
    /// </summary>
    /// <param name="fileExt">文件扩展名，不含“.”</param>
    private bool IsImage(string fileExt)
    {
        ArrayList al = new ArrayList();
        al.Add("bmp");
        al.Add("jpeg");
        al.Add("jpg");
        al.Add("gif");
        al.Add("png");
        if (al.Contains(fileExt.ToLower()))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查是否为合法的上传文件
    /// </summary>
    private bool CheckFileExt(SysConfigDto config, string fileExt)
    {
        //检查危险文件
        string[] excExt = { "asp", "aspx", "ashx", "asa", "asmx", "asax", "php", "jsp", "htm", "html" };
        for (int i = 0; i < excExt.Length; i++)
        {
            if (excExt[i].ToLower() == fileExt.ToLower())
            {
                return false;
            }
        }
        //检查合法文件
        string[] allowExt = (config.fileExtension + "," + config.videoExtension).Split(',');
        for (int i = 0; i < allowExt.Length; i++)
        {
            if (allowExt[i].ToLower() == fileExt.ToLower())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查文件大小是否合法
    /// </summary>
    /// <param name="fileExt">文件扩展名，不含“.”</param>
    /// <param name="fileSize">文件大小(B)</param>
    private bool CheckFileSize(SysConfigDto config, string fileExt, int fileSize)
    {
        if (config.videoExtension == null)
        {
            return false;
        }
        //将视频扩展名转换成ArrayList
        ArrayList lsVideoExt = new ArrayList(config.videoExtension.ToLower().Split(','));
        //判断是否为图片文件
        if (IsImage(fileExt))
        {
            if (config.imgSize > 0 && fileSize > config.imgSize * 1024)
            {
                return false;
            }
        }
        else if (lsVideoExt.Contains(fileExt.ToLower()))
        {
            if (config.videoSize > 0 && fileSize > config.videoSize * 1024)
            {
                return false;
            }
        }
        else
        {
            if (config.attachSize > 0 && fileSize > config.attachSize * 1024)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查文件地址是否文件服务器地址
    /// </summary>
    /// <param name="url">文件地址</param>
    private bool IsExternalIPAddress(string url)
    {
        var uri = new Uri(url);
        switch (uri.HostNameType)
        {
            case UriHostNameType.Dns:
                var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                {
                    byte[] ipBytes = ipAddress.GetAddressBytes();
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        if (!IsPrivateIP(ipAddress))
                        {
                            return true;
                        }
                    }
                }
                break;

            case UriHostNameType.IPv4:
                return !IsPrivateIP(IPAddress.Parse(uri.DnsSafeHost));
        }
        return false;
    }

    /// <summary>
    /// 检查IP地址是否本地服务器地址
    /// </summary>
    /// <param name="myIPAddress">IP地址</param>
    private bool IsPrivateIP(IPAddress myIPAddress)
    {
        if (IPAddress.IsLoopback(myIPAddress)) return true;
        if (myIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            byte[] ipBytes = myIPAddress.GetAddressBytes();
            // 10.0.0.0/24 
            if (ipBytes[0] == 10)
            {
                return true;
            }
            // 172.16.0.0/16
            else if (ipBytes[0] == 172 && ipBytes[1] == 16)
            {
                return true;
            }
            // 192.168.0.0/16
            else if (ipBytes[0] == 192 && ipBytes[1] == 168)
            {
                return true;
            }
            // 169.254.0.0/16
            else if (ipBytes[0] == 169 && ipBytes[1] == 254)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
