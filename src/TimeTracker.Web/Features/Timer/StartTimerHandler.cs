namespace TimeTracker.Web.Features.Timer;

public class StartTimerHandler(RunningTimerService timerService)
{
    private readonly RunningTimerService _timerService = timerService;

    public void Handle(int? categoryId, string? description) =>
        _timerService.Start(categoryId, description);
}
