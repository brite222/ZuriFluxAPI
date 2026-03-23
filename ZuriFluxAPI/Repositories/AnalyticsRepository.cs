using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Data;

namespace ZuriFluxAPI.Repositories

{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ZuriFluxDbContext _context;

        public AnalyticsRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var totalBins = await _context.Bins.CountAsync();
            var binsNeedingPickup = await _context.Bins.CountAsync(b => b.NeedsPickup);
            var avgFillLevel = await _context.Bins.AverageAsync(b => (double)b.FillLevel);

            var collectionsToday = await _context.WasteCollections
                .CountAsync(w => w.CollectedAt >= todayStart);

            var collectionsThisMonth = await _context.WasteCollections
                .CountAsync(w => w.CollectedAt >= monthStart);

            var totalUsers = await _context.Users
                .CountAsync(u => u.Role == "citizen");

            var totalCreditsAwarded = await _context.CreditTransactions
                .Where(t => t.Amount > 0)
                .SumAsync(t => t.Amount);

            return new DashboardSummaryDto
            {
                TotalBins = totalBins,
                BinsNeedingPickup = binsNeedingPickup,
                BinsOperational = totalBins - binsNeedingPickup,
                AverageFillLevel = Math.Round(avgFillLevel, 1),
                TotalCollectionsToday = collectionsToday,
                TotalCollectionsThisMonth = collectionsThisMonth,
                TotalActiveUsers = totalUsers,
                TotalCreditsAwarded = totalCreditsAwarded,
                GeneratedAt = now
            };
        }

        public async Task<BinAnalyticsDto> GetBinAnalyticsAsync()
        {
            var bins = await _context.Bins
                .Include(b => b.SensorReadings)
                .ToListAsync();

            var fullBins = bins.Count(b => b.FillLevel >= 80);
            var moderateBins = bins.Count(b => b.FillLevel >= 40 && b.FillLevel < 80);
            var emptyBins = bins.Count(b => b.FillLevel < 40);

            // Hotspots = bins with the most sensor readings (most active/problematic)
            var hotspots = bins
                .OrderByDescending(b => b.SensorReadings.Count)
                .Take(5)
                .Select(b => new BinHotspotDto
                {
                    BinId = b.Id,
                    Location = b.Location,
                    FillLevel = b.FillLevel,
                    TotalReadings = b.SensorReadings.Count
                })
                .ToList();

            return new BinAnalyticsDto
            {
                TotalBins = bins.Count,
                FullBins = fullBins,
                ModerateBins = moderateBins,
                EmptyBins = emptyBins,
                Hotspots = hotspots
            };
        }

        public async Task<CollectionAnalyticsDto> GetCollectionAnalyticsAsync()
        {
            var collections = await _context.WasteCollections
                .Include(w => w.Collector)
                .ToListAsync();

            var total = collections.Count;
            var completed = collections.Count(c => c.Status == "completed");
            var failed = collections.Count(c => c.Status == "failed");
            var completionRate = total > 0
                ? Math.Round((double)completed / total * 100, 1)
                : 0;

            // Group by collector and count their pickups
            var topCollectors = collections
                .Where(c => c.Status == "completed")
                .GroupBy(c => new { c.CollectorId, c.Collector?.FullName })
                .Select(g => new TopCollectorDto
                {
                    CollectorId = g.Key.CollectorId,
                    CollectorName = g.Key.FullName,
                    TotalPickups = g.Count(),
                    CreditsEarned = g.Count() * 10   // 10 credits per pickup
                })
                .OrderByDescending(c => c.TotalPickups)
                .Take(5)
                .ToList();

            return new CollectionAnalyticsDto
            {
                TotalCollections = total,
                CompletedCollections = completed,
                FailedCollections = failed,
                CompletionRate = completionRate,
                TopCollectors = topCollectors
            };
        }

        public async Task<CreditAnalyticsDto> GetCreditAnalyticsAsync()
        {
            var transactions = await _context.CreditTransactions
                .Include(t => t.User)
                .ToListAsync();

            var totalAwarded = transactions
                .Where(t => t.Amount > 0)
                .Sum(t => t.Amount);

            var totalDeducted = transactions
                .Where(t => t.Amount < 0)
                .Sum(t => Math.Abs(t.Amount));

            // Top citizens by current credit balance
            var topCitizens = await _context.Users
                .Where(u => u.Role == "citizen" && u.CreditBalance > 0)
                .OrderByDescending(u => u.CreditBalance)
                .Take(5)
                .Select(u => new TopCitizenDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    CreditBalance = u.CreditBalance,
                    TotalTransactions = _context.CreditTransactions
                        .Count(t => t.UserId == u.Id)
                })
                .ToListAsync();

            var usersWithCredits = await _context.Users
                .CountAsync(u => u.CreditBalance > 0);

            return new CreditAnalyticsDto
            {
                TotalCreditsAwarded = totalAwarded,
                TotalCreditsDeducted = totalDeducted,
                TotalCreditsInCirculation = totalAwarded - totalDeducted,
                TotalUsersWithCredits = usersWithCredits,
                TopCitizens = topCitizens
            };

        }
        public async Task<LeaderboardDto> GetLeaderboardAsync(int topCount = 10)
        {
            // Top citizens by credit balance
            var topCitizens = await _context.Users
                .Where(u => u.Role == "citizen")
                .OrderByDescending(u => u.CreditBalance)
                .Take(topCount)
                .ToListAsync();

            // Top collectors by completed pickups
            var topCollectors = await _context.Users
                .Where(u => u.Role == "collector")
                .Select(u => new
                {
                    User = u,
                    TotalPickups = _context.WasteCollections
                        .Count(wc => wc.CollectorId == u.Id
                            && wc.Status == "completed"),
                    ScheduledPickups = _context.CollectionSchedules
                        .Count(cs => cs.AssignedCollectorId == u.Id
                            && cs.Status == "completed"),
                    TotalCreditsEarned = _context.CreditTransactions
                        .Where(ct => ct.UserId == u.Id && ct.Amount > 0)
                        .Sum(ct => (int?)ct.Amount) ?? 0
                })
                .OrderByDescending(c => c.TotalPickups)
                .Take(topCount)
                .ToListAsync();

            // Build citizen leaderboard with rank
            var citizenLeaderboard = topCitizens
                .Select((u, index) => new CitizenLeaderboardDto
                {
                    Rank = index + 1,
                    UserId = u.Id,
                    FullName = u.FullName,
                    CreditBalance = u.CreditBalance,
                    TotalReferrals = u.TotalReferrals,
                    MemberSince = u.CreatedAt
                })
                .ToList();

            // Build collector leaderboard with rank
            var collectorLeaderboard = topCollectors
                .Select((c, index) => new CollectorLeaderboardDto
                {
                    Rank = index + 1,
                    UserId = c.User.Id,
                    FullName = c.User.FullName,
                    TotalPickups = c.TotalPickups,
                    ScheduledPickups = c.ScheduledPickups,
                    TotalCreditsEarned = c.TotalCreditsEarned
                })
                .ToList();

            return new LeaderboardDto
            {
                TopCitizens = citizenLeaderboard,
                TopCollectors = collectorLeaderboard,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
