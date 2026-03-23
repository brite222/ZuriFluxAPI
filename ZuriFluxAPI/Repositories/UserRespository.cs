using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ZuriFluxDbContext _context;

        public UserRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
            public async Task<User> GetByReferralCodeAsync(string referralCode)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.ReferralCode == referralCode);
            }
    }
}
