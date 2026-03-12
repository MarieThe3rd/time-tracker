using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Timer.ManualEntry;

public record ManualEntryInput(
    DateTime StartTime,
    DateTime EndTime,
    int? WorkCategoryId,
    string? Description,
    int? ProductivityRating,
    string? ValueAdded,
    bool IsBreak,
    bool AiUsed,
    int? AiTimeSavedMinutes,
    string? AiNotes);

public class ManualEntryHandler(ITimeEntryRepository timeEntryRepo)
{
    public async Task<TimeEntry> HandleAsync(ManualEntryInput input)
    {
        if (input.EndTime <= input.StartTime)
            throw new ArgumentException("End time must be after start time.", nameof(input));

        if (input.IsBreak && input.ProductivityRating.HasValue)
            throw new ArgumentException("Break entries cannot include a productivity rating.", nameof(input));

        if (input.ProductivityRating.HasValue && (input.ProductivityRating.Value < 1 || input.ProductivityRating.Value > 5))
            throw new ArgumentOutOfRangeException(nameof(input), "Productivity rating must be between 1 and 5.");

        if (!input.AiUsed && (input.AiTimeSavedMinutes.HasValue || !string.IsNullOrWhiteSpace(input.AiNotes)))
            throw new ArgumentException("AI details can only be provided when AI-Used is true.", nameof(input));

        if (input.AiUsed)
        {
            if (!input.AiTimeSavedMinutes.HasValue || input.AiTimeSavedMinutes.Value < 0)
                throw new ArgumentException("AI time saved must be provided and be zero or greater when AI-Used is true.", nameof(input));

            if (string.IsNullOrWhiteSpace(input.AiNotes))
                throw new ArgumentException("AI notes are required when AI-Used is true.", nameof(input));
        }

        var entry = new TimeEntry
        {
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            WorkCategoryId = input.WorkCategoryId,
            Description = input.Description,
            ProductivityRating = input.IsBreak ? null : input.ProductivityRating,
            ValueAdded = input.ValueAdded,
            IsBreak = input.IsBreak,
            AiUsed = input.AiUsed,
            AiTimeSavedMinutes = input.AiUsed ? input.AiTimeSavedMinutes : null,
            AiNotes = input.AiUsed ? input.AiNotes : null
        };

        return await timeEntryRepo.AddAsync(entry);
    }
}
