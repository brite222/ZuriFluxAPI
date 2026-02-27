using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Repositories;

namespace ZuriFluxAPI.Services
{
    public class CreditService : ICreditService
    {
        private readonly ICreditRepository _creditRepository;
        private readonly IUserRepository _userRepository;

        public CreditService(
            ICreditRepository creditRepository,
            IUserRepository userRepository)
        {
            _creditRepository = creditRepository;
            _userRepository = userRepository;
        }

        public async Task<CreditTransactionResponseDto> AwardCreditsAsync(AwardCreditDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new Exception($"User with ID {dto.UserId} not found.");

            if (dto.Amount <= 0)
                throw new Exception("Award amount must be greater than zero.");

            // Create the transaction record
            var transaction = new CreditTransaction
            {
                UserId = dto.UserId,
                Amount = dto.Amount,          // positive number = earned
                Reason = dto.Reason,
                TransactedAt = DateTime.UtcNow
            };

            await _creditRepository.CreateAsync(transaction);

            // Update user's balance
            user.CreditBalance += dto.Amount;
            await _userRepository.UpdateAsync(user);

            return MapToDto(transaction, user.FullName);
        }

        public async Task<CreditTransactionResponseDto> DeductCreditsAsync(DeductCreditDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new Exception($"User with ID {dto.UserId} not found.");

            if (dto.Amount <= 0)
                throw new Exception("Deduction amount must be greater than zero.");

            // Make sure they have enough balance
            if (user.CreditBalance < dto.Amount)
                throw new Exception($"Insufficient balance. User only has {user.CreditBalance} credits.");

            // Store as negative so transaction history makes sense
            var transaction = new CreditTransaction
            {
                UserId = dto.UserId,
                Amount = -dto.Amount,         // negative number = deducted
                Reason = dto.Reason,
                TransactedAt = DateTime.UtcNow
            };

            await _creditRepository.CreateAsync(transaction);

            // Deduct from user's balance
            user.CreditBalance -= dto.Amount;
            await _userRepository.UpdateAsync(user);

            return MapToDto(transaction, user.FullName);
        }

        public async Task<CreditSummaryDto> GetUserCreditSummaryAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"User with ID {userId} not found.");

            var transactions = await _creditRepository.GetByUserIdAsync(userId);
            var transactionList = transactions.ToList();

            // Calculate totals from transaction history
            var totalEarned = transactionList
                .Where(t => t.Amount > 0)
                .Sum(t => t.Amount);

            var totalDeducted = transactionList
                .Where(t => t.Amount < 0)
                .Sum(t => Math.Abs(t.Amount));  // show as positive number

            // Only return the 10 most recent transactions in the summary
            var recentTransactions = transactionList
                .Take(10)
                .Select(t => MapToDto(t, user.FullName))
                .ToList();

            return new CreditSummaryDto
            {
                UserId = user.Id,
                UserName = user.FullName,
                CurrentBalance = user.CreditBalance,
                TotalEarned = totalEarned,
                TotalDeducted = totalDeducted,
                RecentTransactions = recentTransactions
            };
        }

        public async Task<IEnumerable<CreditTransactionResponseDto>> GetAllTransactionsAsync()
        {
            var transactions = await _creditRepository.GetAllAsync();
            return transactions.Select(t => MapToDto(t, t.User?.FullName));
        }

        public async Task<IEnumerable<CreditTransactionResponseDto>> GetUserTransactionsAsync(int userId)
        {
            var transactions = await _creditRepository.GetByUserIdAsync(userId);
            return transactions.Select(t => MapToDto(t, t.User?.FullName));
        }

        private CreditTransactionResponseDto MapToDto(CreditTransaction t, string userName) =>
            new CreditTransactionResponseDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserName = userName,
                Amount = t.Amount,
                Reason = t.Reason,
                TransactedAt = t.TransactedAt
            };

        public async Task<PagedResult<CreditTransactionResponseDto>> GetPagedAsync(CreditFilterParams filters)
        {
            var pagedTransactions = await _creditRepository.GetPagedAsync(filters);

            return new PagedResult<CreditTransactionResponseDto>
            {
                Data = pagedTransactions.Data.Select(t => MapToDto(t, t.User?.FullName)),
                PageNumber = pagedTransactions.PageNumber,
                PageSize = pagedTransactions.PageSize,
                TotalRecords = pagedTransactions.TotalRecords,
                TotalPages = pagedTransactions.TotalPages,
                HasNextPage = pagedTransactions.HasNextPage,
                HasPreviousPage = pagedTransactions.HasPreviousPage
            };
        }
    }
}
