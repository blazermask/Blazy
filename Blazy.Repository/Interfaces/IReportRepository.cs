using Blazy.Core.Entities;

namespace Blazy.Repository.Interfaces;

/// <summary>
/// Repository interface for Report operations
/// </summary>
public interface IReportRepository : IRepository<Report>
{
    /// <summary>
    /// Gets reports with pagination
    /// </summary>
    Task<(IEnumerable<Report> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Gets pending reports
    /// </summary>
    Task<(IEnumerable<Report> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Gets reports by reporter
    /// </summary>
    Task<IEnumerable<Report>> GetReportsByReporterAsync(int reporterId);

    /// <summary>
    /// Checks if user has reported specific content
    /// </summary>
    Task<bool> HasReportedAsync(int reporterId, string contentType, int? targetPostId, int? targetCommentId, int? targetUserId);

    /// <summary>
    /// Gets the last report time for a user
    /// </summary>
    Task<DateTime?> GetLastReportTimeAsync(int reporterId);
}