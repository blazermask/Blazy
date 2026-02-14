using Blazy.Core.DTOs;

namespace Blazy.Services.Interfaces;

/// <summary>
/// Service interface for report operations
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Create a new report
    /// </summary>
    Task<(bool Success, string Message, ReportDto? Report)> CreateReportAsync(int reporterId, CreateReportDto model);

    /// <summary>
    /// Get all reports with pagination
    /// </summary>
    Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Get pending reports
    /// </summary>
    Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Review and update a report
    /// </summary>
    Task<(bool Success, string Message)> ReviewReportAsync(int adminId, int reportId, string status, string? adminNotes);

    /// <summary>
    /// Get reports by reporter
    /// </summary>
    Task<IEnumerable<ReportDto>> GetReportsByReporterAsync(int reporterId);
}