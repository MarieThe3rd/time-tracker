using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlUserSettingsRepository(AppDbContext db) : IUserSettingsRepository
{
    public async Task<UserSettings> GetAsync()
        => await db.UserSettings.FirstOrDefaultAsync() ?? new UserSettings { Id = 1 };

    public async Task UpdateAsync(UserSettings settings)
    {
        var existing = await db.UserSettings.FindAsync(settings.Id);
        if (existing is null)
            db.UserSettings.Add(settings);
        else
        {
            existing.VaultRootPath = settings.VaultRootPath;
            existing.DailyNotesSubfolder = settings.DailyNotesSubfolder;
            existing.WeeklyNotesSubfolder = settings.WeeklyNotesSubfolder;
        }
        await db.SaveChangesAsync();
    }
}
