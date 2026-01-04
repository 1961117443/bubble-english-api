using Aspose.Words;
using Aspose.Words.Fields;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.ContractTemplate;
using QT.FriendlyException;
using QT.SDMS.Entitys;
using QT.UnifyResult;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
    /// 报价单沟通记录
    /// </summary>
[ApiDescriptionSettings("售电系统", Tag = "合同模板", Name = "ContractTemplate", Order = 600)]
[Route("api/sdms/contract/template")]
public class ContractTemplateService : QTBaseService<ContractTemplateEntity, ContractTemplateCrInput, ContractTemplateUpInput,ContractTemplateInfoOutput,ContractTemplateListPageInput,ContractTemplateListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ContractTemplateEntity> _repository;

    public ContractTemplateService(ISqlSugarRepository<ContractTemplateEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    protected override async Task<SqlSugarPagedList<ContractTemplateListOutput>> GetPageList([FromQuery]ContractTemplateListPageInput input)
    {
        var data = await _repository.Context.Queryable<ContractTemplateEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(), it=>it.Name.Contains(input.keyword))
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return data.Adapt<SqlSugarPagedList<ContractTemplateListOutput>>();
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
    public async Task<ContractTemplateCrInput> UploadTemplate(IFormFile file, [FromServices]IFileManager _fileManager)
    {
        try
        {
            ContractTemplateCrInput op = new ContractTemplateCrInput()
            {
                //name = file.FileName,
                property = new List<ContractTemplatePropertyInfo>()
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
                            op.property.Add(new ContractTemplatePropertyInfo
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
