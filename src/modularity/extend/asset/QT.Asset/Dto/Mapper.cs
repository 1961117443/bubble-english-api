using Mapster;
using QT.Asset.Dto.AssetWarehouse;
using QT.Asset.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Asset.Dto;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<AssetWarehouseEntity, AssetWarehouseTreeListOutput>()
            .Map(dest => dest.parentId, src => src.PId)
            .Map(dest => dest.fullName, src => src.Name);
    }
}
