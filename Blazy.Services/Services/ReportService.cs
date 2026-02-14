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

        var reportDto = await MapToReportDto(report);
        return (true, "Report submitted successfully.", reportDto);
    }

    public async Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize)
    {
        var (reports, totalCount) = await _reportRepository.GetReportsAsync(pageIndex, pageSize);

        var reportDtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            var reportDto = await MapToReportDto(report);
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
            var reportDto = await MapToReportDto(report);
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
            var reportDto = await MapToReportDto(report);
            if (reportDto != null)
            {
                reportDtos.Add(reportDto);
            }
        }

        return reportDtos;
    }

    private async Task<ReportDto> MapToReportDto(Report report)
    {
        var targetPostTitle = report.TargetPostId.HasValue
            ? (await _postRepository.GetByIdAsync(report.TargetPostId.Value))?.Title
            : null;

        var targetCommentContent = report.TargetCommentId.HasValue
            ? (await _context.Comments.FindAsync(report.TargetCommentId.Value))?.Content
            : null;

        var targetUsername = report.TargetUserId.HasValue
            ? (await _userRepository.GetByIdAsync(report.TargetUserId.Value))?.Username
            : null;

        var reviewedByAdminUsername = report.ReviewedByAdminId.HasValue
            ? (await _userRepository.GetByIdAsync(report.ReviewedByAdminId.Value))?.Username
            : null;

        return new ReportDto
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterUsername = report.Reporter.Username,
            ContentType = report.ContentType,
            TargetPostId = report.TargetPostId,
            TargetPostTitle = targetPostTitle,
            TargetCommentId = report.TargetCommentId,
            TargetCommentContent = targetCommentContent,
            TargetUserId = report.TargetUserId,
            TargetUsername = targetUsername,
            Reason = report.Reason,
            IsReviewed = report.IsReviewed,
            ReviewedByAdminId = report.ReviewedByAdminId,
            ReviewedByAdminUsername = reviewedByAdminUsername,
            Status = report.Status,
            AdminNotes = report.AdminNotes,
            CreatedAt = report.CreatedAt,
            ReviewedAt = report.ReviewedAt
        };
    }
}