using Mapster;
using QT.CMS.Entitys.Dto.Article;
using QT.Common.Extension;
using QT.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.CMS.Entitys.Profiles.Article;

public class ArticleMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<Articles, ArticlesDto>()
            .Map(dest => dest.fields, src => GetFields(src.ArticleFields))
            .Map(dest => dest.isComment, src => src.SiteChannel != null && src.SiteChannel.IsComment == 1 ? src.IsComment : 0)
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryRelations.IsAny())
                {
                    dest.categoryTitle = string.Join(",", src.CategoryRelations.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category.Title)).Select(x => x.Category!.Title).ToArray());
                }
                else
                {
                    dest.categoryTitle = "";
                }

                if (src.LabelRelations.IsAny())
                {
                    dest.labelTitle = string.Join(",", src.LabelRelations.Where(x => x.Label != null && !string.IsNullOrEmpty(x.Label.Title)).Select(x => x.Label!.Title).ToArray());
                }
                else
                {
                    dest.labelTitle = "";
                }
            })
            ;

        config.ForType<Articles, ArticlesClientDto>()
            .Map(dest => dest.fields, src => GetFields(src.ArticleFields))
            .Map(dest => dest.isComment, src => src.SiteChannel != null && src.SiteChannel.IsComment == 1 ? src.IsComment : 0)
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryRelations.IsAny())
                {
                    dest.categoryTitle = string.Join(",", src.CategoryRelations.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category.Title)).Select(x => x.Category!.Title).ToArray());
                }
                else
                {
                    dest.categoryTitle = "";
                }

                if (src.LabelRelations.IsAny())
                {
                    dest.labelTitle = string.Join(",", src.LabelRelations.Where(x => x.Label != null && !string.IsNullOrEmpty(x.Label.Title)).Select(x => x.Label!.Title).ToArray());
                }
                else
                {
                    dest.labelTitle = "";
                }
            })
            ;

        #region 文章投稿
        //源数据映射到DTO
        config.ForType<ArticleContribute, ArticleContributeViewDto>()
            .Map(dest => dest.fields, src => string.IsNullOrEmpty(src.FieldMeta)? new List<ArticleContributeFieldValueDto>(): JsonHelper.ToObject<List<ArticleContributeFieldValueDto>>(src.FieldMeta))
            .Map(dest => dest.albums, src => string.IsNullOrEmpty(src.AlbumMeta) ?  new List<ArticleAlbumDto>(): JsonHelper.ToObject<List<ArticleAlbumDto>>(src.AlbumMeta))
            .Map(dest => dest.attachs, src => string.IsNullOrEmpty(src.AttachMeta) ? new List<ArticleAttachDto>(): JsonHelper.ToObject<List<ArticleAttachDto>>(src.AttachMeta));
        //DTO映射到源数据
        config.ForType<ArticleContributeAddDto, ArticleContribute>()
            .Map(desc => desc.FieldMeta, src => src.Fields != null ? src.Fields.ToJsonString() : null)
            .Map(desc => desc.AlbumMeta, src => src.Albums != null ? src.Albums!.ToJsonString() : null)
            .Map(desc => desc.AttachMeta, src => src.Attachs != null ? src.Attachs!.ToJsonString() : null);
        config.ForType<ArticleContributeEditDto, ArticleContribute>()
            .Map(desc => desc.FieldMeta, src => src.Fields != null ? src.Fields.ToJsonString() : null)
            .Map(desc => desc.AlbumMeta, src => src.Albums != null ? src.Albums!.ToJsonString() : null)
            .Map(desc => desc.AttachMeta, src => src.Attachs != null ? src.Attachs!.ToJsonString() : null);
        #endregion
    }

    /// <summary>
    /// 获得扩展字段键值对
    /// </summary>
    /// <param name="articleFields">扩展字段列表</param>
    public static Dictionary<string, string?> GetFields(ICollection<ArticleFieldValue> articleFields)
    {
        Dictionary<string, string?> dic = new();
        if (articleFields.IsAny())
        {
            foreach (var item in articleFields)
            {
                if (item.FieldName != null)
                {
                    dic.Add(item.FieldName, item.FieldValue);
                }
            }
        }
        
        return dic;
    }
}
