using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Blazy.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Services.Services;

/// <summary>
/// Service implementation for report operations
/// </summary>
public class ReportService : Interfaces.IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly Blazy.Data.BlazyDbContext _context;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public ReportService(
        IReportRepository reportRepository,
        Blazy.Data.BlazyDbContext context,
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        _reportRepository = reportRepository;
        _context = context;
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<(bool Success, string Message, ReportDto? Report)> CreateReportAsync(int reporterId, CreateReportDto model)
    {
        // Check if user has already reported this content
        var hasReported = await _reportRepository.HasReportedAsync(
            reporterId,
            model.ContentType,
            model.TargetPostId,
            model.TargetCommentId,
            model.TargetUserId);

        if (hasReported)
        {
            return (false, "You have already reported this content.", null);
        }

        // Check cooldown (5 minutes)
        var lastReportTime = await _reportRepository.GetLastReportTimeAsync(reporterId);
        if (lastReportTime.HasValue && DateTime.UtcNow < lastReportTime.Value.AddMinutes(5))
        {
            var remainingMinutes = Math.Ceiling((lastReportTime.Value.AddMinutes(5) - DateTime.UtcNow).TotalMinutes);
            return (false, $"Please wait {remainingMinutes} minutes before making another report.", null);
        }

        // Validate content exists
        if (model.ContentType == "Post" && model.TargetPostId.HasValue)
        {
            var post = await _postRepository.GetByIdAsync(model.TargetPostId.Value);
            if (post == null)
            {
                return (false, "Post not found.", null);
            }
        }
        else if (model.ContentType == "Comment" && model.TargetCommentId.HasValue)
        {
            var comment = await _context.Comments.FindAsync(model.TargetCommentId.Value);
            if (comment == null)
            {
                return (false, "Comment not found.", null);
            }
        }
        else if (model.ContentType == "Account" && model.TargetUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(model.TargetUserId.Value);
            if (user == null)
            {
                return (false, "User not found.", null);
            }
        }

        var report = new Report
        {
            ReporterId = reporterId,
            ContentType = model.ContentType,
            TargetPostId = model.TargetPostId,
            TargetCommentId = model.TargetCommentId,
            TargetUserId = model.TargetUserId,
            Reason = model.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);

        // FIX: Reload the report with all navigation properties from the database
        // The freshly created report entity does not have its navigation properties loaded,
        // which caused NullReferenceException when accessing report.Reporter.Username in MapToReportDto
        var savedReport = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.TargetPost)
            .Include(r => r.TargetComment)
            .Include(r => r.TargetUser)
            .FirstOrDefaultAsync(r => r.Id == report.Id);

        if (savedReport == null)
        {
            return (true, "Report submitted successfully.", null);
        }

        var reportDto = MapToReportDto(savedReport);
        return (true, "Report submitted successfully.", reportDto);
    }

    public async Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize)
    {
        var (reports, totalCount) = await _reportRepository.GetReportsAsync(pageIndex, pageSize);

        var reportDtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            var reportDto = MapToReportDto(report);
            if (reportDto != null)
            {
                reportDtos.Add(reportDto);
            }
        }

        return (reportDtos, totalCount);
    }

    public async Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize)
    {
        var (reports, totalCount) = await _reportRepository.GetPendingReportsAsync(pageIndex, pageSize);

        var reportDtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            var reportDto = MapToReportDto(report);
            if (reportDto != null)
            {
                reportDtos.Add(reportDto);
            }
        }

        return (reportDtos, totalCount);
    }

    public async Task<(bool Success, string Message)> ReviewReportAsync(int adminId, int reportId, string status, string? adminNotes)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            return (false, "Report not found.");
        }

        report.IsReviewed = true;
        report.Status = status;
        report.AdminNotes = adminNotes;
        report.ReviewedByAdminId = adminId;
        report.ReviewedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        return (true, "Report reviewed successfully.");
    }

    public async Task<IEnumerable<ReportDto>> GetReportsByReporterAsync(int reporterId)
    {
        var reports = await _reportRepository.GetReportsByReporterAsync(reporterId);

        var reportDtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            var reportDto = MapToReportDto(report);
            if (reportDto != null)
            {
                reportDtos.Add(reportDto);
            }
        }

        return reportDtos;
    }

    /// <summary>
    /// Maps a Report entity to a ReportDto.
    /// FIX: Now uses safe null-conditional access for all navigation properties
    /// to prevent NullReferenceException when navigation properties are not loaded.
    /// When reports are loaded via repository methods with Include(), the navigation
    /// properties will be populated. For freshly created reports, we reload from DB with Includes.
    /// </summary>
    private ReportDto MapToReportDto(Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterUsername = report.Reporter?.Username ?? "Unknown",
            ContentType = report.ContentType,
            TargetPostId = report.TargetPostId,
            TargetPostTitle = report.TargetPost?.Title,
            TargetCommentId = report.TargetCommentId,
            TargetCommentContent = report.TargetComment?.Content,
            TargetUserId = report.TargetUserId,
            TargetUsername = report.TargetUser?.Username,
            Reason = report.Reason,
            IsReviewed = report.IsReviewed,
            ReviewedByAdminId = report.ReviewedByAdminId,
            ReviewedByAdminUsername = report.ReviewedByAdmin?.Username,
            Status = report.Status,
            AdminNotes = report.AdminNotes,
            CreatedAt = report.CreatedAt,
            ReviewedAt = report.ReviewedAt
        };
    }
}