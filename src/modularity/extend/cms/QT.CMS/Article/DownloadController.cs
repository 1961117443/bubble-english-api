using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 用户下载数据
/// </summary>
[Route("[controller]")]
[ApiController]
public class DownloadController : ControllerBase
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IFileManager _fileManager;
    private readonly ISqlSugarRepository<Articles> _articleService;
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _clientFactory;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public DownloadController(ISqlSugarRepository<Articles> articlesService, IUserService userService,  IHttpClientFactory clientFactory,
        IWebHostEnvironment hostEnvironment,IFileManager fileManager)
    {
        _articleService = articlesService;
        _userService = userService;
        _clientFactory = clientFactory;
        _hostEnvironment = hostEnvironment;
        _fileManager = fileManager;
    }

    #region 前台调用接口============================
    [HttpGet("/client/download/{id}")]
    [NonUnify]
    [AllowAnonymous]
    public async Task<IActionResult> Download([FromRoute] long id)
    {
        //获得下载ID
        if (id == 0)
        {
            throw Oops.Oh("出错了，文件参数传值不正确！");
        }
        var attachModel = await _articleService.Context.Queryable<ArticleAttach>().SingleAsync(t => t.Id == id);
        if (attachModel == null || attachModel.FilePath == null)
        {
            throw Oops.Oh("出错了，文件不存在或已删除！");
        }
        UserEntity? userModel = new UserEntity { Id = "" };
        // 判断是否需要登录
        if (attachModel.IsMember == 1)
        {
            //获取登录用户
            userModel = await _userService.GetUserAsync();
            if (userModel == null)
            {
                throw Oops.Oh("下载失败，请登录后下载");
            }
        }
      

       
        attachModel.DownCount++;
        //检查文件本地还是远程
        if (attachModel.FilePath.ToLower().StartsWith("http://") || attachModel.FilePath.ToLower().StartsWith("https://"))
        {
            var client = _clientFactory.CreateClient();
            var responseStream = await client.GetStreamAsync(attachModel.FilePath);
            if (responseStream == null)
            {
                throw Oops.Oh("出错了，文件不存在或已删除！");
            }
            byte[] byteData = FileHelper.ConvertStreamToByteBuffer(responseStream);
            //更新下载数量
            await _articleService.Context.Updateable<ArticleAttach>(attachModel).UpdateColumns(nameof(ArticleAttach.DownCount)).ExecuteCommandAsync();
            //修改下载数量
            attachModel.DownCount = attachModel.DownCount + 1;
            //添加下载日志
            MemberAttachLog attachLog = new()
            {
                AddTime = DateTime.Now,
                AttachId = id,
                FileName = attachModel.FileName,
                UserId = userModel.Id
            };
            await _articleService.Context.Insertable<MemberAttachLog>(attachLog).ExecuteCommandAsync();
            return File(byteData, "application/octet-stream", attachModel.FileName);
        }
        else
        {
            var fs = await _fileManager.DownloadFileByType(attachModel.FilePath, attachModel.FileName);
            if (fs != null)
            {
                //更新下载数量
                await _articleService.Context.Updateable<ArticleAttach>(attachModel).UpdateColumns(nameof(ArticleAttach.DownCount)).ExecuteCommandAsync();
                //修改下载数量
                attachModel.DownCount = attachModel.DownCount + 1;
                //添加下载日志
                MemberAttachLog attachLog = new()
                {
                    AddTime = DateTime.Now,
                    AttachId = id,
                    FileName = attachModel.FileName,
                    UserId = userModel.Id
                };
                await _articleService.Context.Insertable<MemberAttachLog>(attachLog).ExecuteCommandAsync();
                return File(fs.FileStream, "application/octet-stream", attachModel.FileName);
            }
            else
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                //获取物理路径
                string fullFileName = (webRootPath + attachModel.FilePath).Replace("\\", @"/");
                //检测文件是否存在
                if (!System.IO.File.Exists(fullFileName))
                {
                    throw Oops.Oh("出错了，文件不存在或已删除！");
                }
                //更新下载数量
                await _articleService.Context.Updateable<ArticleAttach>(attachModel).UpdateColumns(nameof(ArticleAttach.DownCount)).ExecuteCommandAsync();
                //修改下载数量
                attachModel.DownCount = attachModel.DownCount + 1;
                //添加下载日志
                MemberAttachLog attachLog = new()
                {
                    AddTime = DateTime.Now,
                    AttachId = id,
                    FileName = attachModel.FileName,
                    UserId = userModel.Id
                };
                await _articleService.Context.Insertable<MemberAttachLog>(attachLog).ExecuteCommandAsync();
                return File(attachModel.FilePath, "application/octet-stream", attachModel.FileName);
            }
        }
    }
    #endregion
}
