using AutoMapper;
using CarRentalApp.DTO.Lookup;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using CarRentalApp.Services.Customers;

namespace CarRentalApp.Services.Lookup
{
    public class LookupService : ILookupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LookupService> _logger;

        public LookupService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LookupService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryReadOnlyDTO>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryReadOnlyDTO>>(categories);
        }

        public async Task<IEnumerable<LocationReadOnlyDTO>> GetAllLocationsAsync()
        {
            var locations = await _unitOfWork.LocationRepository.GetAllAsync();
            return _mapper.Map<List<LocationReadOnlyDTO>>(locations);
        }
    }
}
