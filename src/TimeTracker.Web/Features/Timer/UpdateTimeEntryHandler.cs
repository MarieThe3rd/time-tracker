using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Timer;

public record UpdateTimeEntryInput(
    int Id,
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

public class UpdateTimeEntryHandler(AppDbContext db)
{
  private readonly AppDbContext _db = db;

  public async Task<TimeEntry?> HandleAsync(UpdateTimeEntryInput input)
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

    var entry = await _db.TimeEntries.FirstOrDefaultAsync(e => e.Id == input.Id);
    if (entry is null)
      return null;

    entry.StartTime = input.StartTime;
    entry.EndTime = input.EndTime;
    entry.WorkCategoryId = input.WorkCategoryId;
    entry.Description = input.Description;
    entry.ProductivityRating = input.IsBreak ? null : input.ProductivityRating;
    entry.ValueAdded = input.ValueAdded;
    entry.IsBreak = input.IsBreak;
    entry.AiUsed = input.AiUsed;
    entry.AiTimeSavedMinutes = input.AiUsed ? input.AiTimeSavedMinutes : null;
    entry.AiNotes = input.AiUsed ? input.AiNotes : null;

    await _db.SaveChangesAsync();
    return entry;
  }
}
