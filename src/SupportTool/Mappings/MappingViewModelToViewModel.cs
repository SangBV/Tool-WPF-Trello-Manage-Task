using AutoMapper;
using SupportTool.Model.Trello;
using SupportTool.Model.Trello.Entities;
using SupportTool.ViewModel.Trello;

namespace SupportTool.Mappings
{
    public class MappingViewModelToViewModelProfile : Profile
    {
        public MappingViewModelToViewModelProfile()
        {
            CreateMap<CustomFieldTemplate, CardCustomFieldViewModel>()
                .ForMember(a => a.IdCustomField, b => b.MapFrom(s => s.Id))
                .ForMember(a => a.Value, b => b.Ignore());

            CreateMap<CardCustomFieldViewModel, CustomFieldTemplate>()
                .ForMember(a => a.Id, b => b.MapFrom(s => s.IdCustomField))
                .ForMember(a => a.ValueNumber, b => b.Ignore())
                .ForMember(a => a.ValueText, b => b.Ignore())
                .ForMember(a => a.ValueDate, b => b.Ignore());
        }
    }
}
