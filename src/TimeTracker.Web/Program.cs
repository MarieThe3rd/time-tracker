using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Features.Timer;
using TimeTracker.Web.Features.Timer.ManualEntry;
using TimeTracker.Web.Features.Journal.AddEntry;
using TimeTracker.Web.Features.Journal.ListEntries;
using TimeTracker.Web.Features.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Journal feature handlers
builder.Services.AddScoped<AddEntryHandler>();
builder.Services.AddScoped<ListEntriesHandler>();
builder.Services.AddScoped<DeleteJournalEntryHandler>();

// Timer feature handlers
builder.Services.AddSingleton<RunningTimerService>();
builder.Services.AddScoped<StartTimerHandler>();
builder.Services.AddScoped<StopTimerHandler>();
builder.Services.AddScoped<ManualEntryHandler>();
builder.Services.AddScoped<GetTodayEntriesHandler>();
builder.Services.AddScoped<DeleteTimeEntryHandler>();
builder.Services.AddScoped<UpdateProductivityHandler>();

// Dashboard
builder.Services.AddScoped<DashboardHandler>();

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
