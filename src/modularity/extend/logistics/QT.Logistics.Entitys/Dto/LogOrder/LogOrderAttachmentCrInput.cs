using QT.Common.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogOrderAttachment;

public class LogOrderAttachmentCrInput :FileControlsModel
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}
