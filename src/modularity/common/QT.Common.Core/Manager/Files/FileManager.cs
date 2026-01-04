using System.Text;
using QT.Common.Configuration;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Options;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.RemoteRequest.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnceMi.AspNetCore.OSS;
using QT.Common.Core.Manager.Tenant;
using QT.UnifyResult;
using Serilog;

namespace QT.Common.Core.Manager.Files
{
    /// <summary>
    /// 文件管理.
    /// </summary>
    public class FileManager : IFileManager, IScoped
    {
        private readonly IOSSServiceFactory _oSSServiceFactory;
        private readonly IUserManager _userManager;

        public FileManager(
            IUserManager userManager,
            IOSSServiceFactory oSSServiceFactory)
        {
            _userManager = userManager;
            _oSSServiceFactory = oSSServiceFactory;
        }

        #region OSS

        /// <summary>
        /// 根据存储类型上传文件.
        /// </summary>
        /// <param name="stream">文件流.</param>
        /// <param name="directoryPath">保存文件夹.</param>
        /// <param name="fileName">新文件名.</param>
        /// <returns></returns>
        public async Task<(bool,string)> UploadFileByType(Stream stream, string directoryPath, string fileName)
        {
            try
            {
                bool flag = true;
                var bucketName = KeyVariable.BucketName; // 桶名
                var fileStoreType = KeyVariable.FileStoreType; // 文件存储类型
                var uploadPath = string.Empty; // 上传路径
                string url = string.Empty;
                switch (fileStoreType)
                {
                    case OSSProviderType.Invalid:
                        uploadPath = Path.Combine(directoryPath, fileName);
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);
                        if (!File.Exists(uploadPath))
                        {
                            using (var streamLocal = File.Create(uploadPath))
                            {
                                await stream.CopyToAsync(streamLocal);
                            }
                        }                        

                        break;
                    default:
                        uploadPath = string.Format("{0}/{1}", directoryPath, fileName);
                        if (KeyVariable.MultiTenancy)
                        {
                            uploadPath = $"{TenantScoped.TenantId}/{uploadPath}";
                        }
                        flag = await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).PutObjectAsync(bucketName, uploadPath, stream);
                        if (flag)
                        {
                            url = $"/{bucketName}/{uploadPath}";
                        }
                        
                        break;
                }
                return (flag, url);
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D8001);
            }
        }

        /// <summary>
        /// 根据存储类型下载文件.
        /// </summary>
        /// <param name="filePath">文件路径.</param>
        /// <param name="fileDownLoadName">文件下载名.</param>
        /// <returns></returns>
        public async Task<FileStreamResult> DownloadFileByType(string filePath, string fileDownLoadName)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        return new FileStreamResult(new FileStream(filePath, FileMode.Open), "application/octet-stream") { FileDownloadName = fileDownLoadName };
                    default:
                        filePath = filePath.Replace(@"\", "/");
                        if (KeyVariable.MultiTenancy)
                        {
                            // 判断是否已经是 bucket开头
                            var fp = filePath.TrimStart('/');
                            if (fp.StartsWith(KeyVariable.BucketName))
                            {
                                filePath= fp.Replace($"{KeyVariable.BucketName}/", "");
                            }
                            else
                            {
                                filePath = $"{TenantScoped.TenantId}/{filePath}";
                            }

                                
                        }
                        var url = await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).PresignedGetObjectAsync(KeyVariable.BucketName, filePath, 86400);
                        var stream = await url.GetAsStreamAsync();
                        return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileDownLoadName };
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                UnifyContext.Fill(e.Message);
                throw Oops.Oh(ErrorCode.D8003);
            }
        }

        /// <summary>
        /// 获取指定文件夹下所有文件.
        /// </summary>
        /// <param name="filePath">文件前缀.</param>
        /// <returns></returns>
        [NonAction]
        public async Task<List<AnnexModel>> GetObjList(string filePath)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        var files = FileHelper.GetAllFiles(filePath);
                        List<AnnexModel> data = new List<AnnexModel>();
                        if (files != null)
                        {
                            for (int i = 0; i < files.Count; i++)
                            {
                                var item = files[i];
                                AnnexModel fileModel = new AnnexModel();
                                fileModel.FileId = i.ToString();
                                fileModel.FileName = item.Name;
                                fileModel.FileType = FileHelper.GetFileType(item);
                                fileModel.FileSize = FileHelper.GetFileSize(item.FullName).ToString();
                                fileModel.FileTime = item.LastWriteTime;
                                data.Add(fileModel);
                            }
                        }

                        return data;
                    default:
                        var bucketName = KeyVariable.BucketName;
                        var list = await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).ListObjectsAsync(bucketName, filePath);
                        return list.Select(x => new AnnexModel()
                        {
                            FileId = x.ETag,
                            FileName = x.Key.Replace(filePath + "/", string.Empty),
                            FileType = x.Key.Substring(x.Key.LastIndexOf(".") + 1),
                            FileSize = x.Size.ToString(),
                            FileTime = x.LastModifiedDateTime.ParseToDateTime(),
                            FileUrl = $"/{bucketName}/{x.Key}",
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D8000);
            }

        }

        /// <summary>
        /// 删除文件.
        /// </summary>
        /// <param name="filePath">文件地址.</param>
        /// <returns></returns>
        [NonAction]
        public async Task DeleteFile(string filePath)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        FileHelper.DeleteFile(filePath);
                        break;
                    default:
                        filePath = filePath.Replace(@"\", "/");
                        var bucketName = KeyVariable.BucketName;
                        await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).RemoveObjectAsync(bucketName, filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D1803);
            }
        }

        /// <summary>
        /// 判断文件是否存在.
        /// </summary>
        /// <param name="filePath">文件路径.</param>
        /// <returns></returns>
        public async Task<bool> ExistsFile(string filePath)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        return FileHelper.Exists(filePath);
                    default:
                        filePath = filePath.Replace(@"\", "/");
                        var bucketName = KeyVariable.BucketName;
                        return await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).ObjectsExistsAsync(bucketName, filePath);
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D8000);
            }
        }

        /// <summary>
        /// 获取指定文件流.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<Stream> GetFileStream(string filePath)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        return FileHelper.FileToStream(filePath);
                    default:
                        filePath = filePath.Replace(@"\", "/");
                        var bucketName = KeyVariable.BucketName;
                        Stream fs = null;
                        await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).GetObjectAsync(bucketName, filePath, (stream) =>
                        {
                            using (fs = new FileStream("1.jpg", FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fs);
                                fs.Close();
                            }
                        });
                        return fs;
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D1804);
            }
        }

        /// <summary>
        /// 剪切文件.
        /// </summary>
        /// <param name="filePath">源文件地址.</param>
        /// <param name="toFilePath">剪切地址.</param>
        /// <returns></returns>
        public async Task MoveFile(string filePath, string toFilePath)
        {
            try
            {
                switch (KeyVariable.FileStoreType)
                {
                    case OSSProviderType.Invalid:
                        FileHelper.MoveFile(filePath, toFilePath);
                        break;
                    default:
                        filePath = filePath.Replace(@"\", "/");
                        var bucketName = KeyVariable.BucketName;
                        await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).CopyObjectAsync(bucketName, filePath, bucketName, toFilePath);
                        await _oSSServiceFactory.Create(KeyVariable.FileStoreType.ToString()).RemoveObjectAsync(bucketName, filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D1804);
            }
        }
        #endregion

        #region 导入导出(json文件)

        /// <summary>
        /// 导出.
        /// </summary>
        /// <param name="jsonStr">json数据.</param>
        /// <param name="name">文件名.</param>
        /// <param name="exportFileType">文件后缀.</param>
        /// <returns></returns>
        public async Task<dynamic> Export(string jsonStr, string name, ExportFileType exportFileType = ExportFileType.json)
        {
            var _filePath = GetPathByType(string.Empty);
            var _fileName = string.Format("{0}{1}.{2}", name, DateTime.Now.ParseToUnixTime(), exportFileType.ToString());
            var byteList = new UTF8Encoding(true).GetBytes(jsonStr.ToCharArray());
            var stream = new MemoryStream(byteList);
            var flag = await UploadFileByType(stream, _filePath, _fileName);
            return new
            {
                name = _fileName,
                url = flag.Item2 ?? string.Format("/api/file/Download?encryption={0}", DESCEncryption.Encrypt(string.Format("{0}|{1}|json", _userManager.UserId, _fileName), "QT"))
            };
        }

        /// <summary>
        /// 导入.
        /// </summary>
        /// <param name="file">文件.</param>
        /// <returns></returns>
        public string Import(IFormFile file)
        {
            var stream = file.OpenReadStream();
            var byteList = new byte[file.Length];
            stream.Read(byteList, 0, (int)file.Length);
            stream.Position = 0;
            var sr = new StreamReader(stream, Encoding.Default);
            var json = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return json;
        }
        #endregion

        #region 分块式上传文件

        /// <summary>
        /// 分片上传附件.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<dynamic> UploadChunk([FromForm] ChunkModel input)
        {
            // 碎片临时文件存储路径
            //string directoryPath = Path.Combine(App.GetConfig<AppOptions>("QT_App", true).SystemPath, "TemporaryFile", input.identifier);
            string directoryPath = Path.Combine(GetPathByType(string.Empty), $"{input.identifier}{MD5Encryption.Encrypt(input.fileName)}");
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // 碎片文件名称
                string chunkFileName = string.Format("{0}{1}{2}", input.identifier, "-", input.chunkNumber);
                string chunkFilePath = Path.Combine(directoryPath, chunkFileName);
                if (!FileHelper.Exists(chunkFilePath))
                {
                    using (var streamLocal = File.Create(chunkFilePath))
                    {
                        await input.file.OpenReadStream().CopyToAsync(streamLocal);
                    }
                }

                return new { merge = input.chunkNumber == input.totalChunks };
            }
            catch (Exception ex)
            {
                FileHelper.DeleteDirectory(directoryPath);
                throw Oops.Oh(ErrorCode.D8001);
            }
        }

        /// <summary>
        /// 分片组装.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<dynamic> Merge([FromForm] ChunkModel input)
        {
            FileStream fs = null;
            try
            {
                // 新文件名称
                var saveFileName = string.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMdd"), SnowflakeIdHelper.NextId(), Path.GetExtension(input.fileName));
                // 碎片临时文件存储路径
                //string directoryPath = Path.Combine(App.GetConfig<AppOptions>("QT_App", true).SystemPath, "TemporaryFile", input.identifier);
                string directoryPath = Path.Combine(GetPathByType(string.Empty), $"{input.identifier}{MD5Encryption.Encrypt(input.fileName)}");
                var chunkFiles = Directory.GetFiles(directoryPath);
                List<byte> byteSource = new List<byte>();
                fs = new FileStream(Path.Combine(directoryPath, input.fileName), FileMode.Create);
                foreach (var part in chunkFiles.OrderBy(x => x.Length).ThenBy(x => x))
                {
                    var bytes = FileHelper.ReadAllBytes(part);
                    fs.Write(bytes, 0, bytes.Length);
                    bytes = null;
                    FileHelper.DeleteFile(part);
                }
                fs.Flush();
                fs.Close();
                Stream stream = new FileStream(Path.Combine(directoryPath, input.fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
                var flag = await UploadFileByType(stream, GetPathByType(input.type), saveFileName);
                if (flag.Item1)
                {
                    stream.Flush();
                    stream.Close();
                    FileHelper.DeleteDirectory(directoryPath);
                }
                return new { name = saveFileName, size= input.fileSize, url = flag.Item2 ?? string.Format("/api/file/Download?encryption={0}", DESCEncryption.Encrypt(string.Format("{0}|{1}|{2}", _userManager.UserId, saveFileName, input.type), "QT")) };
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ErrorCode.D8003);
            }
            finally
            {
                if (fs!=null)
                {
                    fs.Close();
                }
            }
        }
        #endregion

        /// <summary>
        /// 根据类型获取文件存储路径.
        /// </summary>
        /// <param name="type">文件类型.</param>
        /// <returns></returns>
        public string GetPathByType(string type)
        {
            switch (type)
            {
                case "userAvatar":
                    return FileVariable.UserAvatarFilePath;
                case "mail":
                    return FileVariable.EmailFilePath;
                case "IM":
                    return FileVariable.IMContentFilePath;
                case "weixin":
                    return FileVariable.MPMaterialFilePath;
                case "workFlow":
                case "annex":
                case "annexpic":
                    return FileVariable.SystemFilePath;
                case "document":
                    return FileVariable.DocumentFilePath;
                case "preview":
                    return FileVariable.DocumentPreviewFilePath;
                case "screenShot":
                case "banner":
                case "bg":
                case "border":
                case "source":
                    return FileVariable.BiVisualPath;
                case "template":
                    return FileVariable.TemplateFilePath;
                case "codeGenerator":
                    return FileVariable.GenerateCodePath;
                default:
                    return FileVariable.TemporaryFilePath;
            }
        }
    }
}