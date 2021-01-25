using AutoMapper;
using SupportTool.Mappings;

namespace SupportTool.Services
{
    public class MappingService
    {
        private static bool _isInitialized;

        public void InitializeMapper()
        {
            if (!_isInitialized)
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfile(new MappingEntityToViewModelProfile());
                    cfg.AddProfile(new MappingViewModelToViewModelProfile());
                });
                _isInitialized = true;
            }
        }
    }
}
