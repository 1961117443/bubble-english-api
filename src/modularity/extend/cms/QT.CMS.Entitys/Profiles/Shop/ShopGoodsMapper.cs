using Mapster;
using QT.CMS.Entitys.Dto.Shop;
using QT.Common.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.CMS.Entitys.Profiles.Shop;

public class ShopGoodsMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //商品主从，将源数据映射到DTO
        config.ForType<ShopGoods, ShopGoodsDto>()
            .Map(
                dest => dest.categoryTitle,
                src => src.CategoryRelations.IsAny() ? string.Join(",", src.CategoryRelations.Select(x => x.ShopCategory != null ? x.ShopCategory.Title : "").ToArray()):""
            )
            .Map(
                dest => dest.labelTitle,
                src => src.LabelRelations.IsAny()? string.Join(",", src.LabelRelations.Select(x => x.ShopLabel != null ? x.ShopLabel.Title : "").ToArray()) :""
            )
                .Map(
                dest => dest.brandTitle,
                src => src.Brand != null ? src.Brand.Title : ""
            ).Map(dest => dest.categoryRelations, src => src.CategoryRelations ?? new List<ShopCategoryRelation>())
            .Map(dest => dest.labelRelations, src => src.LabelRelations ?? new List<ShopLabelRelation>())
            .Map(dest => dest.fieldValues, src => src.FieldValues ?? new List<ShopGoodsFieldValue>())
            .Map(dest => dest.goodsSpecs, src => src.GoodsSpecs ?? new List<ShopGoodsSpec>())
            .Map(dest => dest.goodsAlbums, src => src.GoodsAlbums ?? new List<ShopGoodsAlbum>())
            .Map(dest => dest.goodsProducts, src => src.GoodsProducts ?? new List<ShopGoodsProduct>());

        config.ForType<ShopGoods, ShopGoodsClientDto>()
           .Map(
               dest => dest.categoryTitle,
               src => src.CategoryRelations.IsAny() ? string.Join(",", src.CategoryRelations.Select(x => x.ShopCategory != null ? x.ShopCategory.Title : "").ToArray()) :""
           )
           .Map(
               dest => dest.labelTitle,
               src => src.LabelRelations.IsAny() ? string.Join(",", src.LabelRelations.Select(x => x.ShopLabel != null ? x.ShopLabel.Title : "").ToArray()) :""
           )
           .Map(
               dest => dest.brandTitle,
               src => src.Brand != null ? src.Brand.Title : ""
           ).Map(dest => dest.categoryRelations, src => src.CategoryRelations ?? new List<ShopCategoryRelation>())
            .Map(dest => dest.labelRelations, src => src.LabelRelations ?? new List<ShopLabelRelation>())
            .Map(dest => dest.fieldValues, src => src.FieldValues ?? new List<ShopGoodsFieldValue>())
            .Map(dest => dest.goodsSpecs, src => src.GoodsSpecs ?? new List<ShopGoodsSpec>())
            .Map(dest => dest.goodsAlbums, src => src.GoodsAlbums ?? new List<ShopGoodsAlbum>())
            .Map(dest => dest.goodsProducts, src => src.GoodsProducts ?? new List<ShopGoodsProduct>());
    }
}
