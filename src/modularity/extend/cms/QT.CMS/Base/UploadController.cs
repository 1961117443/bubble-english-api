using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Interfaces;
using QT.Common.Const;
using QT.FriendlyException;

namespace QT.CMS;

/// <summary>
/// 文件上传
/// </summary>
[Route("api/cms/[controller]")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "upload", Name = "upload", Order = 200)]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IFileService _fileService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public UploadController(IWebHostEnvironment hostEnvironment, IFileService fileService)
    {
        _hostEnvironment = hostEnvironment;
        _fileService = fileService;
    }

    /// <summary>
    /// 文件上传
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpLoadFile([FromForm] IFormCollection formCollection)
    {
        //检查是否有文件上传
        if (formCollection == null && formCollection?.Files == null)
        {
            throw Oops.Oh("请选择要上传文件");
        }

        //获取GET参数
        bool isWater = false;//默认不打水印
        bool isThumbnail = false;//默认不生成缩略图
        if (Request.Query["water"] == "1")
        {
            isWater = true;
        }
        if (Request.Query["thumb"] == "1")
        {
            isThumbnail = true;
        }

        List<FileDto> listFileDto = new List<FileDto>();
        //循环遍历要上传的文件
        foreach (IFormFile file in formCollection.Files)
        {
            var filePath = file.FileName;//获取文件名
            if (filePath.IndexOf(".") == -1 && file.ContentType.LastIndexOf("/") != -1)
            {
                filePath += "." + file.ContentType[(file.ContentType.LastIndexOf("/") + 1)..];
            }
            var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] byteData = ms.ToArray();//转换成二进制
            listFileDto.Add(await _fileService.SaveAsync(byteData, filePath, isThumbnail, isWater));

        }
        //返回文件上传地址
        return Ok(listFileDto);
    }
}
