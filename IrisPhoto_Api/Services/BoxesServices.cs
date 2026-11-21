using IrisPhoto_Api.Domain.Entities;
using IrisPhoto_Api.Infrastructure;
using static IrisPhoto_Api.Infrastructure.IBaseRepository;

namespace IrisPhoto_Api.Services
{
    public interface IBoxesServices : IBaseRepository<Box>
    {

    }
    public class BoxesServices : BaseRepository<Box>, IBoxesServices
    {
        private readonly IAppConfigService _configService;

        public BoxesServices(AppDbContext context, IAppConfigService configService)
            : base(context)
        {
            _configService = configService;
        }
    }
}
