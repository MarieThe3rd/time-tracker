using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Settings;

public class SettingsHandler(AppDbContext db)
{
    public async Task<UserSettings> GetAsync() =>
        await db.UserSettings.FirstOrDefaultAsync() ?? new UserSettings { Id = 1 };

    public async Task SaveAsync(UserSettings settings)
    {
        var existing = await db.UserSettings.FindAsync(settings.Id);
        if (existing is null)
            db.UserSettings.Add(settings);
        else
        {
            existing.VaultRootPath = settings.VaultRootPath;
            existing.DailyNotesSubfolder = settings.DailyNotesSubfolder;
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<WorkCategory>> GetCategoriesAsync() =>
        await db.WorkCategories.OrderBy(c => c.Name).ToListAsync();

    public async Task<WorkCategory> AddCategoryAsync(string name, string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        var cat = new WorkCategory { Name = name.Trim(), Color = color, Icon = icon };
        db.WorkCategories.Add(cat);
        await db.SaveChangesAsync();
        return cat;
    }

    public async Task UpdateCategoryAsync(WorkCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty.", nameof(category));

        var existing = await db.WorkCategories.FindAsync(category.Id);
        if (existing is null) return;
        existing.Name = category.Name.Trim();
        existing.Color = category.Color;
        existing.Icon = category.Icon;
        await db.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var cat = await db.WorkCategories.FindAsync(id);
        if (cat is not null && !cat.IsSystem)
        {
            db.WorkCategories.Remove(cat);
            await db.SaveChangesAsync();
        }
    }
}
