using Aspose.Words.Saving;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.QuoteOrder;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using Aspose.Words;
using Aspose.Words.Fields;
using Aspose.Words.Tables;

namespace QT.Extend;

/// <summary>
    /// 报价模块
    /// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "报价模块", Name = "QuoteOrder", Order = 600)]
[Route("api/extend/Quote/order")]
public class QuoteOrderService : QTBaseService<QuoteOrderEntity, QuoteOrderCrInput, QuoteOrderUpInput, QuoteOrderInfoOutput, QuoteOrderListPageInput, QuoteOrderListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<QuoteOrderEntity> _repository;
    private readonly IBillRullService _billRullService;

    public QuoteOrderService(ISqlSugarRepository<QuoteOrderEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _repository = repository;
        _billRullService = billRullService;
    }

    public override async Task<QuoteOrderInfoOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);
        var user = await _repository.Context.Queryable<UserEntity>().Where(x => x.Id == data.creatorUserId).Select(x => new UserEntity
        {
            RealName = x.RealName,
            MobilePhone = x.MobilePhone
        }).FirstAsync();
        if (user != null)
        {
            data.quoterCn = user.RealName + (user.MobilePhone.IsNotEmptyOrNull() ? "/" + user.MobilePhone : "");
        }
        data.quoteRecordList = await _repository.Context.Queryable<QuoteRecordEntity>().Where(x => x.Fid == id).Select<QuoteRecordInfoOutput>().ToListAsync();

        return data;
    }

    protected override async Task BeforeCreate(QuoteOrderCrInput input, QuoteOrderEntity entity)
    {
        await base.BeforeCreate(input, entity);
        entity.No = await _billRullService.GetBillNumber("QuoteOrder");

        if (input.quoteRecordList.IsAny())
        {
            var quoteRecordList = input.quoteRecordList.Adapt<List<QuoteRecordEntity>>();

            quoteRecordList.ForEach(x =>
            {
                x.Id = SnowflakeIdHelper.NextId();
                x.Fid = entity.Id;
            });

            await _repository.Context.Insertable<QuoteRecordEntity>(quoteRecordList).ExecuteCommandAsync();
        }
    }

    public override async Task<PageResult<QuoteOrderListOutput>> GetList([FromQuery] QuoteOrderListPageInput input)
    {
        var data = await _repository.Context.Queryable<QuoteOrderEntity>()
            .LeftJoin<QuoteCustomerEntity>((x, c) => x.Cid == c.Id)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), (x, c) => x.No.Contains(input.keyword) || c.Admintel.Contains(input.keyword) || c.Name.Contains(input.keyword))
            .WhereIF(input.minViewCount.HasValue && input.minViewCount>0,(x,c)=> x.ViewCount> input.minViewCount)
             .OrderByDescending((x, c) => x.BillDate)
             .Select<QuoteOrderListOutput>((x, c) => new QuoteOrderListOutput
             {
                 id = x.Id,
                 billDate = x.BillDate,
                 cid = x.Cid,
                 no = x.No,
                 remark = x.Remark,
                 cidName = c.Name,
                 cidAdmintel = c.Admintel,
                 viewCount = x.ViewCount,
                 lastViewTime = SqlFunc.Subqueryable<QuoteOrderShareLogEntity>().Where(ddd=>ddd.Fid == x.Id).OrderByDesc(ddd=>ddd.CreatorTime).Select(ddd=>ddd.CreatorTime)
             }, true)
             .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<QuoteOrderListOutput>.SqlSugarPagedList(data);
    }


    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public override async Task Update(string id, [FromBody] QuoteOrderUpInput input)
    {
        await base.Update(id, input);


        _repository.Context.Deleteable<QuoteRecordEntity>().Where(x => x.Fid == id).AddQueue();
        if (input.quoteRecordList.IsAny())
        {
            var quoteRecordList = input.quoteRecordList.Adapt<List<QuoteRecordEntity>>();

            quoteRecordList.ForEach(x =>
            {
                x.Id ??= SnowflakeIdHelper.NextId();
                x.Fid = id;
            });

            _repository.Context.Insertable<QuoteRecordEntity>(quoteRecordList).AddQueue();
        }

        await _repository.Context.SaveQueuesAsync();
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

    //[HttpGet("Actions/{id}/CreatePdf")]
    //[AllowAnonymous]
    //[NonUnify]
    //public IActionResult CreatePdf(string id)
    //{
    //    string templatePath = @"D:\文档\20250401金鼎正消防改造预算.docx";
    //    string outputPath = @$"D:\文档\新建文件夹\{Guid.NewGuid()}.pdf";
    //    // 数据字典：表单域名称 -> 填充值
    //    var fieldData = new Dictionary<string, string>
    //    {
    //        { "项目名称", "保税区消防整改技术咨询服务" },
    //        { "委托单位", "贵阳金鼎正物业管理有限公司" },
    //        { "预算单位", "贵州习升厚发科技有限公司" },
    //        { "签约日期","2025  年  04  月  01 日" },
    //        { "方案名称","贵阳金鼎正物业管理有限公司消防整改预算方案" },
    //        { "工程地点","贵州省贵阳市白云区都拉乡贵阳综合保税区" },
    //        { "整改内容","11个消控室合并，联动控制。\r\ntest" }
    //    };

    //    // 加载 Word 文档
    //    Document doc = new Document(templatePath);

    //    // 填充文本表单域
    //    foreach (FormField formField in doc.Range.FormFields)
    //    {
    //        if (formField.Type == FieldType.FieldFormTextInput)
    //        {
    //            if (fieldData.ContainsKey(formField.Name))
    //            {
    //                formField.Result = fieldData[formField.Name];
    //            }
    //        }
    //    }

    //    foreach (Table table in doc.GetChildNodes(NodeType.Table,true))
    //    {
    //        if (table.Rows.Count <=1)
    //        {
    //            continue;
    //        }
    //        // 模板行
    //        Row templateRow = (Row)table.Rows[table.Rows.Count - 1].Clone(true);

    //        foreach (Cell cell in templateRow.Cells)
    //        {

    //        }
    //    }

    //    // 配置 PDF 输出参数（可选）
    //    PdfSaveOptions pdfOptions = new PdfSaveOptions
    //    {
    //        // 嵌入所有字体（避免字体缺失）
    //        //EmbedFullFonts = true,
    //        // 图片压缩质量（0-100，100 为无损）
    //        ImageCompression = PdfImageCompression.Auto,
    //        JpegQuality = 100,
    //        // 设置 PDF 标准（如 PDF/A）
    //        //Compliance = PdfCompliance.Pdf17
    //    };

    //    // 保存为 PDF
    //    doc.Save(outputPath, pdfOptions);
    //    Console.WriteLine("表单填充并导出为 PDF 完成！");

    //    return new NoContentResult();
    //    //var generator = new DocumentGenerator();
    //    //var template = JsonConvert.DeserializeObject<Template>(json);
    //    //var buffer =  generator.GenerateDocument(template, new Dictionary<string, object>());
    //    //var memory = new MemoryStream(buffer);
    //    //return File((memory, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "example.docx");
    //    //return new FileStreamResult(memory, "application/octet-stream") { FileDownloadName = "example.docx" };
    //}
     

}