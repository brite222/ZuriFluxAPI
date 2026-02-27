using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
    }
}
