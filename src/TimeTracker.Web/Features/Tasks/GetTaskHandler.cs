using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Tasks;

public class GetTaskHandler(ITaskItemRepository taskRepo)
{
    public async Task<TaskItem?> HandleAsync(int id)
        => await taskRepo.GetByIdAsync(id);
}
