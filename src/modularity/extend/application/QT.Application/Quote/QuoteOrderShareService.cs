using Aspose.Words;
using Aspose.Words.Fields;
using Aspose.Words.Saving;
using Aspose.Words.Tables;
using Mapster;
using Microsoft.AspNetCore.Routing.Template;
using QT.Common.Configuration;
using QT.Common.Contracts;
using QT.Common.Core;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Core.Service;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.QuoteOrder;
using QT.Extend.Entitys.Dto.QuoteOrderShare;
using QT.Extend.Entitys.Dto.QuoteOrderTemplate;
using QT.Extend.Entitys.Dto.WorkOrderLog;
using QT.Message.Handlers;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using Serilog;
using System.Web;

namespace QT.Extend;

/// <summary>
    /// 报价单沟通记录
    /// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "报价单分享记录", Name = "QuoteOrderShare", Order = 600)]
[Route("api/extend/quote/[controller]")]
public class QuoteOrderShareService : QTBaseService<QuoteOrderShareEntity, QuoteOrderShareCrInput, QuoteOrderShareUpInput, QuoteOrderShareInfoOutput, QuoteOrderShareListPageInput, QuoteOrderShareListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<QuoteOrderShareEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService; 
    private readonly IMHandler _iMHandler;

    public QuoteOrderShareService(ISqlSugarRepository<QuoteOrderShareEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService, IMHandler iMHandler) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _billRullService = billRullService;
        _iMHandler = iMHandler;
    }

    public override async Task Update(string id, [FromBody] QuoteOrderShareUpInput input)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(entity);
        
        input.Adapt(entity);
        //// 判断车牌号码是否存在
        //if (await _repository.AnyAsync(it => it.CarNo == entity.CarNo && it.Id != entity.Id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1004);
        //}
        var isOk = await _repository.AutoUpdateAsync(entity);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    public override async Task<QuoteOrderShareInfoOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        data.webUrl = $"/quote/order/share?id={id}&t={_userManager.TenantId}";
        data.miniUrl = "";
        var dict = new Dictionary<string, string>();
        if (KeyVariable.MultiTenancy)
        {
            dict.Add("tenant", _userManager.TenantId);
        }
        dict.Add("page", "/pages/apply/quote/order/share");
        dict.Add("param", HttpUtility.UrlEncode($"id={data.id}"));

        data.miniUrl = string.Join("&", dict.Select(x => $"{x.Key}={x.Value}"));  // JSON.Serialize(dict);
        return data;
    }

    /// <summary>
    /// 根据报价单获取最后的业务信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Current/{fid}")]
    public async Task<QuoteOrderShareInfoOutput> GetByOrderId(string fid)
    {
        var entity = (await _repository.FirstOrDefaultAsync(x => x.Fid == fid));
        if (entity == null)
        {
            entity=  new QuoteOrderShareEntity
            {
                Id = Guid.NewGuid().ToString(),
                Fid = fid
            };
            await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        }
            
        var data = entity.Adapt<QuoteOrderShareInfoOutput>();

        data.webUrl = $"/quote/order/share?id={data.id}&t={_userManager.TenantId}";
        data.miniUrl = "";

        var dict = new Dictionary<string, string>();
        if (KeyVariable.MultiTenancy)
        {
            dict.Add("tenant", _userManager.TenantId);
        }
        dict.Add("page", "/pages/apply/quote/order/share");
        dict.Add("param", HttpUtility.UrlEncode($"id={data.id}"));

        data.miniUrl = string.Join("&", dict.Select(x => $"{x.Key}={x.Value}"));  // JSON.Serialize(dict);

        return data;
    }


    public override async Task<PageResult<QuoteOrderShareListOutput>> GetList([FromQuery] QuoteOrderShareListPageInput input)
    {
        var data = await _repository.Context.Queryable<QuoteOrderShareEntity>()
            .WhereIF(input.fid.IsNotEmptyOrNull(), x => x.Fid == input.fid)
            .Select<QuoteOrderShareListOutput>(x => new QuoteOrderShareListOutput
            {
                orderNo = SqlFunc.Subqueryable<QuoteOrderEntity>().Where(ddd => ddd.Id == x.Fid).Select(ddd => ddd.No)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<QuoteOrderShareListOutput>.SqlSugarPagedList(data);
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

    protected QuoteOrderShareEntity quoteOrderShareEntity;
    /// <summary>
    /// 判断是否有权限
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/{id}/Permission")]
    [AllowAnonymous]
    public async Task<QuoteOrderShareInfoOutput> ActionPermission(string id)
    {
        QuoteOrderShareInfoOutput quoteOrderShareInfoOutput = new QuoteOrderShareInfoOutput()
        {
            id = id
        };
        quoteOrderShareEntity = (await _repository.FirstOrDefaultAsync(x => x.Id == id)) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (quoteOrderShareEntity.ExpiryTime.HasValue)
        {
            if (DateTime.Now > quoteOrderShareEntity.ExpiryTime)
            {
                throw Oops.Oh("链接已过期，您没有权限查看！");
            }
        }

        if (quoteOrderShareEntity.MaxViewCount > 0)
        {
            if (quoteOrderShareEntity.ViewCount >= quoteOrderShareEntity.MaxViewCount)
            {
                throw Oops.Oh("次数已超，您没有权限查看！");
            }
        }

        if (quoteOrderShareEntity.Password.IsNotEmptyOrNull())
        {
            // 返回md5加密的密码，前端判断
            quoteOrderShareInfoOutput.password = MD5Encryption.Encrypt(quoteOrderShareEntity.Password);
        }

        return quoteOrderShareInfoOutput;
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/{id}/data")]
    [AllowAnonymous]
    public async Task<dynamic> ActionPermission(string id, string password, [FromServices]IFileManager fileManager)
    {
        QuoteOrderShareInfoOutput quoteOrderShareInfoOutput = await ActionPermission(id);

        if (quoteOrderShareInfoOutput.password.IsNotEmptyOrNull())
        {
            if (quoteOrderShareInfoOutput.password != password)
            {
                throw Oops.Oh("密码错误，您没有权限查看！");
            }
        }

        var order = (await _repository.Context.Queryable<QuoteOrderEntity>()
            .Where(x => x.Id == quoteOrderShareEntity.Fid)
            .FirstAsync()) ?? throw Oops.Oh(ErrorCode.COM1005);
        var data = order.Adapt<QuoteOrderInfoOutput>();

        var user = await _repository.Context.Queryable<UserEntity>().Where(x => x.Id == order.CreatorUserId).Select(x => new UserEntity
        {
            RealName = x.RealName,
            MobilePhone = x.MobilePhone
        }).FirstAsync();
        if (user!=null)
        {
            data.quoterCn = user.RealName + (user.MobilePhone.IsNotEmptyOrNull() ? "/" + user.MobilePhone : "");
        }
       
        data.quoteRecordList = await _repository.Context.Queryable<QuoteRecordEntity>().Where(x => x.Fid == order.Id).Select<QuoteRecordInfoOutput>().ToListAsync();

        // 获取客户信息
        var customer = await _repository.Context.Queryable<QuoteCustomerEntity>().Where(x => x.Id == order.Cid).FirstAsync() ?? throw Oops.Oh("客户不存在！");
        var configService = App.GetService<ICoreSysConfigService>();

        var config = configService!=null ? (await configService.GetSysConfig()) : new Systems.Entitys.Dto.SysConfig.SysConfigOutput();

        // 发票类型
        var dataOptions = await App.GetService<IDictionaryDataService>()?.GetList("Quote.InvoiceType");
        if (dataOptions!=null)
        {
            data.invoiceTypeCn = dataOptions.FirstOrDefault(x => x.EnCode == order.InvoiceType)?.FullName ?? data.invoiceType;
        }

        string pdfUrl = string.Empty;
        // 判断是否有报价模板
        if (data.tid != null)
        {
            // 获取模板信息
            var fileUrl = await _repository.Context.Queryable<QuoteOrderTemplateEntity>().Where(x => x.Id == data.tid).Select(x => x.FileUrl).FirstAsync();
            if (fileUrl.IsNotEmptyOrNull())
            {
                try
                {
                    var fieldData = new Dictionary<string, string>();

                    if (data.templateJson.IsNotEmptyOrNull())
                    {
                        foreach (var item in data.templateJson.ToObject<List<QuoteOrderTemplatePropertyInfo>>())
                        {
                            fieldData.Add(item.name, item.value);
                        }
                    }
                    var arr = fileUrl.Split("/", true).ToList();
                    if (arr.Count > 0 && arr[0] == KeyVariable.BucketName)
                    {
                        arr.RemoveAt(0);
                    }
                    if (KeyVariable.MultiTenancy && arr.Count > 0 && arr[0] == TenantScoped.TenantId)
                    {
                        arr.RemoveAt(0);
                    }

                    fileUrl = string.Join("/", arr);

                    //data.templateJson
                    var result = await fileManager.DownloadFileByType(fileUrl, "");
                    result.FileStream.Seek(0, SeekOrigin.Begin);
                    // 加载 Word 文档
                    Document doc = new Document(result.FileStream);
                    fillFormField(fieldData, doc.Range.FormFields);

                    // 填充表格数据
                    // 组装表单数据

                    //Dictionary<string,>

                   
                    foreach (Table table in doc.GetChildNodes(NodeType.Table, true))
                    {
                        if (table.Rows.Count <= 1)
                        {
                            continue;
                        }
                        // 模板行
                        Row templateRow = (Row)table.Rows[table.Rows.Count - 1].Clone(true);

                        // 获取商品名称列的序号
                        int productNameIndex = -1;
                        for (int i = 0; i < templateRow.Range.FormFields.Count; i++)
                        {
                            if (templateRow.Range.FormFields[i].Name == "商品信息_商品名称")
                            {
                                productNameIndex = i;
                                break;
                            }
                        }

                        // 删除模板行
                        table.Rows.RemoveAt(table.Rows.Count - 1);

                        for (int i = 0; i < data.quoteRecordList.Count; i++)
                        {
                            var item = data.quoteRecordList[i];
                            Row newRow = (Row)templateRow.Clone(true);
                            table.Rows.Add(newRow);

                            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
                            {
                                {"商品信息_商品名称",item.name },
                                {"商品信息_商品规格",item.spec },
                                {"商品信息_单位",item.unit },
                                {"商品信息_备注",item.remark },
                            };
                            if (item.num.HasValue)
                            {
                                keyValuePairs.Add("商品信息_数量", ((float)item.num).ToString());
                            }
                            if (item.price.HasValue)
                            {
                                keyValuePairs.Add("商品信息_单价", ((float)item.price).ToString());
                            }
                            if (item.amount.HasValue)
                            {
                                keyValuePairs.Add("商品信息_总价", ((float)item.amount).ToString());
                            }
                            //foreach (Cell cell in newRow.Cells)
                            //{
                            //    fillFormField(keyValuePairs, cell.Range.FormFields);
                            //}
                            fillFormField(keyValuePairs, newRow.Range.FormFields);

                            // 合并相同的商品名称
                            if (productNameIndex>-1 && i >0)
                            {
                                var cell = newRow.Cells[productNameIndex];
                                if (cell != null)
                                {
                                    if (item.name == data.quoteRecordList[i - 1].name)
                                    {

                                        cell.CellFormat.VerticalMerge = CellMerge.Previous;
                                    }
                                    else
                                    {
                                        cell.CellFormat.VerticalMerge = CellMerge.First;
                                    }
                                }
                               
                            }
                        }

                        // 合并表格
                        //templateRow.Cells.ToArray().
                        foreach (Row row in table.Rows)
                        {
                            foreach (Cell cell in row.Cells)
                            {

                            }
                        }

                        {
                            Row newRow = (Row)templateRow.Clone(true);
                            table.Rows.Add(newRow);

                            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
                            {
                                {"商品信息_商品名称","合计" },
                                //{"商品信息_商品规格",item.spec },
                                //{"商品信息_单位",item.unit },
                                //{"商品信息_备注",item.remark },
                                {"商品信息_总价", ((float)data.quoteRecordList.Select(x=> x.amount ?? 0).Sum()).ToString() }
                            };
                            fillFormField(keyValuePairs, newRow.Range.FormFields);
                        }

                    }

                    // 配置 PDF 输出参数（可选）
                    PdfSaveOptions pdfOptions = new PdfSaveOptions
                    {
                        // 嵌入所有字体（避免字体缺失）
                        //EmbedFullFonts = true,
                        // 图片压缩质量（0-100，100 为无损）
                        ImageCompression = PdfImageCompression.Auto,
                        JpegQuality = 100,
                        // 设置 PDF 标准（如 PDF/A）
                        //Compliance = PdfCompliance.Pdf17
                    };

                    // 保存为 PDF
                    // 创建一个MemoryStream对象
                    using (MemoryStream stream = new MemoryStream())
                    {
                        // 保存文档到MemoryStream
                        doc.Save(stream, pdfOptions);

                        var _filePath = fileManager.GetPathByType(string.Empty);
                        var _fileName = $"{SnowflakeIdHelper.NextId()}.pdf";

                        stream.Seek(0, SeekOrigin.Begin);
                        var response = await fileManager.UploadFileByType(stream, _filePath, _fileName);
                        if (response.Item1)
                        {
                            pdfUrl = response.Item2;
                        }
                        //// 如果你需要将MemoryStream转换为字节数组，可以这样做：
                        //byte[] docBytes = stream.ToArray();

                        //// 如果你需要保存到文件系统或其他Stream，可以直接使用stream对象
                        //// 例如，保存到另一个文件：
                        //using (FileStream fileStream = new FileStream("output.docx", FileMode.Create, FileAccess.Write))
                        //{
                        //    stream.Position = 0; // 重置MemoryStream的位置到开始处
                        //    stream.CopyTo(fileStream); // 将MemoryStream的内容复制到FileStream
                        //}
                    }
                }
                catch (Exception ex)
                {
                    UnifyContext.Fill(ex.Message);
                    Log.Information(ex.StackTrace);
                    //Log.Error(ex, "生成报价单失败");
                }
            }
        }

        // 浏览记录
        quoteOrderShareEntity.ViewCount = (quoteOrderShareEntity.ViewCount ?? 0)+1;
        order.ViewCount = (order.ViewCount ?? 0)+1;
        QuoteOrderShareLogEntity quoteOrderShareLogEntity = new QuoteOrderShareLogEntity
        {
            Fid = order.Id,
            Sid = id,
            Ip = NetHelper.Ip,
            CreatorTime = DateTime.Now,
            Id = SnowflakeIdHelper.NextId()
        };

        _repository.Context.Insertable<QuoteOrderShareLogEntity>(quoteOrderShareLogEntity).AddQueue();
        _repository.Context.Updateable<QuoteOrderShareEntity>(quoteOrderShareEntity).UpdateColumns(x => x.ViewCount).AddQueue();
        _repository.Context.Updateable<QuoteOrderEntity>(order).UpdateColumns(x => x.ViewCount).AddQueue();
        await _repository.Context.SaveQueuesAsync();
        return new
        {
            dataForm = data,
            connect = new
            {
                user = customer.Name,
                userPhone = customer.Admintel,
                useremail = "",
                useraddress = customer.Address,
                userPostcode = "",
                supply = config.companyName,
                supplyPhone = config.companyTelePhone,
                supplyemail = config.companyEmail,
                supplyaddress = config.companyAddress,
                suPostcode = ""
            },
            pdfUrl
        };
    }

    /// <summary>
    /// 填充文本域
    /// </summary>
    /// <param name="fieldData"></param>
    /// <param name="doc"></param>
    private void fillFormField(Dictionary<string, string> fieldData, FormFieldCollection fields)
    {
        // 填充文本表单域
        foreach (FormField formField in fields)
        {
            if (formField.Type == FieldType.FieldFormTextInput)
            {
                if (fieldData.ContainsKey(formField.Name) && fieldData[formField.Name].IsNotEmptyOrNull())
                {
                    formField.Result = fieldData[formField.Name];
                }
            }
        }
    }

    /// <summary>
    /// 根据报价单id,获取浏览记录
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    [HttpGet("Actions/{id}/List")]
    public async Task<dynamic> GetQuoteList(string id)
    {
        var data = await _repository.Context.Queryable<QuoteOrderShareLogEntity>()
            .Where(x => x.Fid == id)
            .OrderBy(x => x.CreatorTime)
            .Select(x=> new
            {
                id = x.Id,
                creatorTime = x.CreatorTime,
                ip  =x.Ip
            })
            .ToListAsync();
         
        return data;
    }
}
