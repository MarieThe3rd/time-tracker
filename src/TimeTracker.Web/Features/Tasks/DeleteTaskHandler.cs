using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Tasks;

public class DeleteTaskHandler(ITaskItemRepository taskRepo)
{
    public async Task HandleAsync(int id)
        => await taskRepo.DeleteAsync(id);
}
