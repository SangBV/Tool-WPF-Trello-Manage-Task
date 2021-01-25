using AutoMapper;
using SupportTool.Model.Trello.Entities;
using SupportTool.ViewModel.Trello;

namespace SupportTool.Mappings
{
    public class MappingEntityToViewModelProfile : Profile
    { 
        public MappingEntityToViewModelProfile()
        {
            CreateMap<ListCard, ListCardViewModel>();
            CreateMap<Card, CardViewModel>();
            CreateMap<CardBadges, CardBadgesViewModel>();
            CreateMap<CardCustomField, CardCustomFieldViewModel>();

            CreateMap<CustomFieldValue, CustomFieldValueViewModel>();
        }
    }
}
