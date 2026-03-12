using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings> GetAsync();
    Task UpdateAsync(UserSettings settings);
}
