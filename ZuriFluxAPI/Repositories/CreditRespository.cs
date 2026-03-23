using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Helper;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Data;
namespace ZuriFluxAPI.Repositories
{
    public class CreditRepository : ICreditRepository
    {
        private readonly ZuriFluxDbContext _context;

        public CreditRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CreditTransaction>> GetAllAsync()
        {
            return await _context.CreditTransactions
                .Include(c => c.User)
                .OrderByDescending(c => c.TransactedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditTransaction>> GetByUserIdAsync(int userId)
        {
            return await _context.CreditTransactions
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.TransactedAt)
                .ToListAsync();
        }

        public async Task<CreditTransaction> GetByIdAsync(int id)
        {
            return await _context.CreditTransactions
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CreditTransaction> CreateAsync(CreditTransaction transaction)
        {
            _context.CreditTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PagedResult<CreditTransaction>> GetPagedAsync(CreditFilterParams filters)
        {
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .AsQueryable();

            if (filters.UserId.HasValue)
                query = query.Where(t => t.UserId == filters.UserId.Value);

            if (!string.IsNullOrEmpty(filters.Type))
            {
                if (filters.Type == "earned")
                    query = query.Where(t => t.Amount > 0);
                else if (filters.Type == "deducted")
                    query = query.Where(t => t.Amount < 0);
            }

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.TransactedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.TransactedAt <= filters.ToDate.Value);

            query = query.OrderByDescending(t => t.TransactedAt);

            return await PaginationHelper.CreatePagedResultAsync(
                query, filters.PageNumber, filters.PageSize);
        }
    }
}
