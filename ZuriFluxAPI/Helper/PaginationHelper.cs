using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Helper
{
    public static class PaginationHelper
    {
        public static async Task<PagedResult<T>> CreatePagedResultAsync<T>(
            IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}
