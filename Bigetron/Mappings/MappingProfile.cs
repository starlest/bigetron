namespace Bigetron.Mappings
{
    using System.Linq;
    using AutoMapper;
    using Core.Domain.Articles;
    using ECERP.Core;
    using ViewModels;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Article, ArticleVM>()
                .ForMember(avm => avm.Date, conf => conf.MapFrom(a => a.Date.ToString("dd-MM-yyyy")))
                .ForMember(avm => avm.Author, conf => conf.MapFrom(a => a.Author.UserName));
            CreateMap<IPagedList<Article>, PagedListVM<ArticleVM>>()
                .ForMember(lvm => lvm.Source, conf => conf.MapFrom(l => l.ToList()));
        }
    }
}