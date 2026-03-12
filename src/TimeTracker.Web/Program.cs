using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Repositories;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Timer;
using TimeTracker.Web.Features.Timer.ManualEntry;
using TimeTracker.Web.Features.Journal.AddEntry;
using TimeTracker.Web.Features.Journal.ListEntries;
using TimeTracker.Web.Features.Journal.UpdateJournalEntry;
using TimeTracker.Web.Features.Dashboard;
using TimeTracker.Web.Features.Reports;
using TimeTracker.Web.Features.Reports.DailyNote;
using TimeTracker.Web.Features.Settings;
using TimeTracker.Web.Features.Tasks;
using TimeTracker.Web.Features.Reminders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

// Repository implementations
builder.Services.AddScoped<ITimeEntryRepository, SqlTimeEntryRepository>();
builder.Services.AddScoped<IJournalEntryRepository, SqlJournalEntryRepository>();
builder.Services.AddScoped<IJournalTypeRepository, SqlJournalTypeRepository>();
builder.Services.AddScoped<IJournalCategoryRepository, SqlJournalCategoryRepository>();
builder.Services.AddScoped<IWorkCategoryRepository, SqlWorkCategoryRepository>();
builder.Services.AddScoped<IUserSettingsRepository, SqlUserSettingsRepository>();
builder.Services.AddScoped<ITagRepository, SqlTagRepository>();

// Journal feature handlers
builder.Services.AddScoped<AddEntryHandler>();
builder.Services.AddScoped<ListEntriesHandler>();
builder.Services.AddScoped<DeleteJournalEntryHandler>();
builder.Services.AddScoped<UpdateJournalEntryHandler>();

// Timer feature handlers
builder.Services.AddSingleton<RunningTimerService>();
builder.Services.AddScoped<StartTimerHandler>();
builder.Services.AddScoped<StopTimerHandler>();
builder.Services.AddScoped<ManualEntryHandler>();
builder.Services.AddScoped<GetTodayEntriesHandler>();
builder.Services.AddScoped<DeleteTimeEntryHandler>();
builder.Services.AddScoped<UpdateProductivityHandler>();
builder.Services.AddScoped<UpdateTimeEntryHandler>();

// Dashboard
builder.Services.AddScoped<DashboardHandler>();

// Reports
builder.Services.AddScoped<ReportsHandler>();
builder.Services.AddScoped<MarkdownExportService>();

// Settings
builder.Services.AddScoped<SettingsHandler>();

// Tasks feature
builder.Services.AddScoped<ITaskItemRepository, SqlTaskItemRepository>();
builder.Services.AddScoped<AddTaskHandler>();
builder.Services.AddScoped<UpdateTaskHandler>();
builder.Services.AddScoped<DeleteTaskHandler>();
builder.Services.AddScoped<ListTasksHandler>();
builder.Services.AddScoped<GetTaskHandler>();

// Reminders feature
builder.Services.AddScoped<IReminderRepository, SqlReminderRepository>();
builder.Services.AddScoped<AddReminderHandler>();
builder.Services.AddScoped<UpdateReminderHandler>();
builder.Services.AddScoped<DeleteReminderHandler>();
builder.Services.AddScoped<ListRemindersHandler>();
builder.Services.AddScoped<SnoozeReminderHandler>();
builder.Services.AddScoped<DismissReminderHandler>();

builder.Services.AddScoped<TimeTracker.Web.Features.Journal.JournalChangeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<TimeTracker.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// Expose Program class for WebApplicationFactory in UITests
public partial class Program { }
