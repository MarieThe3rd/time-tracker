using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Settings;

public class SettingsHandler(IUserSettingsRepository settingsRepo, IWorkCategoryRepository categoryRepo)
{
    public Task<UserSettings> GetAsync() => settingsRepo.GetAsync();

    public Task SaveAsync(UserSettings settings) => settingsRepo.UpdateAsync(settings);

    public Task<List<WorkCategory>> GetCategoriesAsync() => categoryRepo.GetAllAsync();

    public async Task<WorkCategory> AddCategoryAsync(string name, string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        var cat = new WorkCategory { Name = name.Trim(), Color = color, Icon = icon };
        return await categoryRepo.AddAsync(cat);
    }

    public async Task UpdateCategoryAsync(WorkCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty.", nameof(category));

        var existing = await categoryRepo.GetByIdAsync(category.Id);
        if (existing is null) return;
        existing.Name = category.Name.Trim();
        existing.Color = category.Color;
        existing.Icon = category.Icon;
        await categoryRepo.UpdateAsync(existing);
    }

    public Task DeleteCategoryAsync(int id) => categoryRepo.DeleteAsync(id);
}