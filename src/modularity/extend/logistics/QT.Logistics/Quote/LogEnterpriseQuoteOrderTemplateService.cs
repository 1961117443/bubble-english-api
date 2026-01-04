using Aspose.Words;
using Aspose.Words.Fields;
using Microsoft.AspNetCore.Http;
using QT.Common.Core;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderTemplate;
using QT.UnifyResult;

namespace QT.Logistics;

/// <summary>
    /// 报价单沟通记录
    /// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "报价单模板", Name = "QuoteOrderTemplate", Order = 600)]
[Route("api/Logistics/quote/[controller]")]
public class LogEnterpriseQuoteOrderTemplateService : QTBaseService<LogEnterpriseQuoteOrderTemplateEntity, LogEnterpriseQuoteOrderTemplateCrInput, LogEnterpriseQuoteOrderTemplateUpInput, LogEnterpriseQuoteOrderTemplateInfoOutput, LogEnterpriseQuoteOrderTemplateListPageInput, LogEnterpriseQuoteOrderTemplateListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<LogEnterpriseQuoteOrderTemplateEntity> _repository;

    public LogEnterpriseQuoteOrderTemplateService(ISqlSugarRepository<LogEnterpriseQuoteOrderTemplateEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    protected override async Task<SqlSugarPagedList<LogEnterpriseQuoteOrderTemplateListOutput>> GetPageList([FromQuery] LogEnterpriseQuoteOrderTemplateListPageInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseQuoteOrderTemplateEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(), it=>it.Name.Contains(input.keyword))
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return data.Adapt<SqlSugarPagedList<LogEnterpriseQuoteOrderTemplateListOutput>>();
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public override async Task Delete(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(entity);
        entity.Delete();

        await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// 上传模板文件
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Actions/UploadTemplate")]
    public async Task<LogEnterpriseQuoteOrderTemplateCrInput> UploadTemplate(IFormFile file, [FromServices]IFileManager _fileManager)
    {
        try
        {
            LogEnterpriseQuoteOrderTemplateCrInput op = new LogEnterpriseQuoteOrderTemplateCrInput()
            {
                //name = file.FileName,
                property = new List<LogEnterpriseQuoteOrderTemplatePropertyInfo>()
            };

            if (file != null && file.FileName.IsNotEmptyOrNull())
            {
                op.name = Path.GetFileNameWithoutExtension(file.FileName);
            }

            //  读取模板，获取参数
            using (var stream = file.OpenReadStream())
            {
                // 加载 Word 文档
                Document doc = new Document(stream);

                // 填充文本表单域
                foreach (FormField formField in doc.Range.FormFields)
                {
                    if (formField.Type == FieldType.FieldFormTextInput)
                    {
                        if (!op.property.Any(x=>x.name == formField.Name))
                        {
                            // 如果是 商品信息_ 开头的文本域，则跳过
                            if (formField.Name.StartsWith("商品信息_"))
                            {
                                continue;
                            }
                            op.property.Add(new LogEnterpriseQuoteOrderTemplatePropertyInfo
                            {
                                name = formField.Name
                            });
                        }
                        
                    }
                }

                var _filePath = _fileManager.GetPathByType(string.Empty);
                var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
                //var stream = file.OpenReadStream();
                stream.Seek(0, SeekOrigin.Begin);
                var response = await _fileManager.UploadFileByType(stream, _filePath, _fileName);
                if (response.Item1)
                {
                    op.fileUrl = response.Item2;
                }

                return op;
            }
           
        }
        catch (Exception ex)
        {
            UnifyContext.Fill(ex.Message);
        }


        throw Oops.Oh("上传失败！");
    }
}
