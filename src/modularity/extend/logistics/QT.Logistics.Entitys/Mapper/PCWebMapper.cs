using Mapster;
using QT.Common.Models;
using QT.Common.Security;
using QT.Logistics.Entitys.Dto.LogPCWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys;

public class PCWebMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<LogEnterpriseSupplyProductEntity, LogEnterpriseProductWebListOutput>()
         .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        config.ForType<LogEnterpriseEntity, LogEnterpriseWebListOutput>()
         .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());

        config.ForType<LogEnterpriseEntity, LogEnterpriseWebInfoOutput>()
         .Map(dest => dest.imageUrl, src => !string.IsNullOrEmpty(src.ImageUrl) ? src.ImageUrl.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());
        
    }
}
