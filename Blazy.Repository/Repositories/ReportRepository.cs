using Blazy.Data;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Repository.Repositories;

/// <summary>
/// Repository implementation for Report operations
/// </summary>
public class ReportRepository : Repository<Blazy.Core.Entities.Report>, Interfaces.IReportRepository
{
    public ReportRepository(BlazyDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Report> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.TargetPost)
            .Include(r => r.TargetComment)
            .Include(r => r.TargetUser)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var reports = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reports, totalCount);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Report> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Where(r => !r.IsReviewed)
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.TargetPost)
            .Include(r => r.TargetComment)
            .Include(r => r.TargetUser)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var reports = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reports, totalCount);
    }

    public async Task<IEnumerable<Blazy.Core.Entities.Report>> GetReportsByReporterAsync(int reporterId)
    {
        return await _dbSet
            .Where(r => r.ReporterId == reporterId)
            .Include(r => r.TargetPost)
            .Include(r => r.TargetComment)
            .Include(r => r.TargetUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasReportedAsync(int reporterId, string contentType, int? targetPostId, int? targetCommentId, int? targetUserId)
    {
        return await _dbSet.AnyAsync(r =>
            r.ReporterId == reporterId &&
            r.ContentType == contentType &&
            r.TargetPostId == targetPostId &&
            r.TargetCommentId == targetCommentId &&
            r.TargetUserId == targetUserId);
    }

    public async Task<DateTime?> GetLastReportTimeAsync(int reporterId)
    {
        return await _dbSet
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (DateTime?)r.CreatedAt)
            .FirstOrDefaultAsync();
    }
}