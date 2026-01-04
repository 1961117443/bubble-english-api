using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;
using QT.Archive.Dto.Document;
using QT.Archive.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;
using System.Collections;

namespace QT.Archive;

/// <summary>
/// 档案管理
/// </summary>
[ApiDescriptionSettings("档案管理", Tag = "档案管理", Name = "Document", Order = 601)]
[Route("api/extend/archive/Document")]
public class DocumentService : QTBaseService<ArchivesDocumentEntity, DocumentCrInput, DocumentUpInput, DocumentInfoOutput, DocumentListPageInput, DocumentListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ArchivesDocumentEntity> _repository;

    /// <summary>
    /// 初始化档案馆管理服务实例
    /// </summary>
    /// <param name="repository">档案馆实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public DocumentService(ISqlSugarRepository<ArchivesDocumentEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    protected async override Task<SqlSugarPagedList<DocumentListOutput>> GetPageList([FromQuery] DocumentListPageInput input)
    {
        return await _repository.Context.Queryable<ArchivesDocumentEntity>()
            .WhereIF(input.aid.IsNotEmptyOrNull(),x=>x.Aid == input.aid)
            .Select<DocumentListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    /// <summary>
    /// 下载文件.
    /// </summary>
    /// <param name="id">主键值.</param>
    [HttpPost("Download/{id}")]
    public async Task<dynamic> Download(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.D8000);
        var fileName = _userManager.UserId + "|" + entity.FilePath + "|document";
        return new
        {
            name = entity.FullName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }


    /// <summary>
    /// 批量创建压缩图
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Thumbnail")]
    [NonUnify]
    public async Task<IActionResult> MakeThumbnail(string id,[FromQuery]int width, [FromQuery] int height, [FromQuery] string mode, [FromServices]IFileManager fileManager)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        //如果是图片，检查是否需要生成缩略图，是则生成
        if (!string.IsNullOrEmpty(entity.FilePath) && IsImage(entity.FileExtension) && width > 0 && height > 0)
        {
            var fs = await fileManager.DownloadFileByType(entity.FilePath, entity.FullName);

            
            using var stream = fs.FileStream;
            var fileLength = stream.Length;
            var bytes = new byte[fileLength];

            stream.Read(bytes, 0, (int)fileLength); 
            var thumbData = ImageHelper.MakeThumbnail(bytes, entity.FilePath, width, height, mode);

            return new FileContentResult(thumbData, "image/jpeg") { FileDownloadName = $"{Path.GetFileNameWithoutExtension(entity.FullName).ToUrlEncode()}_{width}x{height}.{entity.FileExtension}" };
        }

        throw Oops.Oh(ErrorCode.D8000);
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
}



