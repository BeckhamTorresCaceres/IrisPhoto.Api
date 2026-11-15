using IrisPhoto_Api.Domain.Entities;
using IrisPhoto_Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static IrisPhoto_Api.Infrastructure.IBaseRepository;

namespace IrisPhoto_Api.Services
{
    public interface IUsersServices : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }

    public class UsersServices : BaseRepository<User>, IUsersServices
    {
        public UsersServices(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}