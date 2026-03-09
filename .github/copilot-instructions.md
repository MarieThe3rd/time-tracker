# Copilot Instructions — My-Apps

This monorepo contains two projects:

- **`time-tracker/`** — A Node.js app managed with the [Squad](https://github.com/bradygaster/squad) multi-agent CLI framework.
- **`WeavingProjectPlanner/`** — A .NET solution (early stage).

---

## Squad CLI (time-tracker)

The `time-tracker` project uses `@bradygaster/squad-cli` as a dev dependency. Squad is a programmable multi-agent runtime for GitHub Copilot — it lets you define an AI team, assign roles, and delegate work through an interactive shell.

### Requirements

- Node.js ≥ 20
- GitHub CLI (`gh`) — required for issue/PR operations
- GitHub Copilot

### Running Squad

```bash
cd time-tracker

# Initialize Squad in the repo (first time only — creates .squad/)
npx squad init

# Launch the interactive shell (talk to your team)
npx squad

# Create an agent
npx squad hire --name <name> --role "<role>"

# Scan for work
npx squad triage

# Continuous work loop
npx squad loop
```

### Squad Team Context

Once initialized, Squad stores its state under `.squad/` at the repo root:

| File | Purpose |
|------|---------|
| `.squad/team.md` | Team roster, member roles, and Copilot capability profile |
| `.squad/routing.md` | Work routing rules — which agent handles which type of issue |
| `.squad/decisions.md` | Shared team decisions |
| `.squad/agents/{name}/charter.md` | Per-agent domain expertise, coding style, and ownership areas |

**Before starting any issue**, read `.squad/team.md` and `.squad/routing.md`. If the issue has a `squad:{member}` label, read that member's charter and work in their voice.

### Capability Self-Check

Check the **Coding Agent → Capabilities** section in `.squad/team.md` before picking up work:

- 🟢 **Good fit** — proceed autonomously.
- 🟡 **Needs review** — proceed, but flag in the PR description for squad review.
- 🔴 **Not suitable** — do NOT start. Comment on the issue explaining why and suggest reassignment.

### Branch Naming

```
squad/{issue-number}-{kebab-case-slug}
```

Example: `squad/12-add-time-entry-form`

### PR Guidelines

- Reference the issue: `Closes #{issue-number}`
- If the issue had a `squad:{member}` label: `Working as {member} ({role})`
- For 🟡 tasks, add: `⚠️ This task was flagged as "needs review" — please have a squad member review before merging.`
- Check `.squad/decisions.md` for any conventions that apply.

### Recording Decisions

If you make a decision other agents should know about, write it to:

```
.squad/decisions/inbox/copilot-{brief-slug}.md
```

The Scribe agent will merge it into the shared decisions file.

---

## Coding Standards (time-tracker)

### C\# Conventions
- Follow [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- `PascalCase` — types, methods, properties, constants; `camelCase` — locals/parameters; `_camelCase` — private fields
- `async`/`await` throughout — never `.Result` or `.Wait()`
- Nullable reference types enabled; file-scoped namespaces

### Project Structure — Vertical Slice Architecture
Each slice is self-contained: Blazor component(s) + handler class + slice-local models. No shared service layer. No MediatR.

```
TimeTracker.Web/
  Features/
    Timer/
      TimerPage.razor              ← page
      StartTimerHandler.cs         ← DbContext injected directly
      StopTimerHandler.cs
      ManualEntry/
        ManualEntryForm.razor
        ManualEntryHandler.cs
    Journal/
      AddEntry/
        QuickAddPanel.razor
        AddEntryHandler.cs
      ListEntries/
        JournalPage.razor
        ListEntriesHandler.cs
    Dashboard/
      DashboardPage.razor
      DashboardHandler.cs
    Reports/
      DailyNote/
        DailyNoteExport.razor
        DailyNoteHandler.cs      ← builds Markdown, writes to vault
      ReviewExport/
        ReviewExportPage.razor
        ReviewExportHandler.cs
    Settings/
      SettingsPage.razor
      SettingsHandler.cs
  Data/
    AppDbContext.cs
    Models/                      ← EF entities only (shared across slices)
  Shared/
    Layout/                      ← MainLayout, NavMenu
    Components/                  ← truly shared UI (StarRatingPicker, MarkdownPreview)
  wwwroot/
```

### Testing
- **Unit tests:** xUnit in `TimeTracker.Tests/` — built-in `Assert` only, no FluentAssertions (commercial license)
  - Handler classes tested with EF Core in-memory provider (`UseInMemoryDatabase`)
  - Mirror `Features/` slice structure in test project
  - Run one test: `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"`
  - Run all unit tests: `dotnet test src/TimeTracker.Tests/`
- **UI tests:** Playwright with xUnit in `TimeTracker.UITests/`
  - Use `Microsoft.Playwright` NuGet package (not the NUnit variant)
  - Page Object Model pattern — one class per page/major component
  - Run UI tests: `dotnet test src/TimeTracker.UITests/`
  - Install browsers once after clone: `pwsh bin/Debug/net10.0/playwright.ps1 install`

### Blazor Conventions
- Use `@code { }` blocks unless logic is substantial (then code-behind is acceptable)
- Parameters: `[Parameter]` with PascalCase names; event callbacks named `On{Event}`
- Inject services with `@inject`; call `StateHasChanged()` only when necessary (e.g. timer ticks)

### Testing
- **xUnit** for all tests — no FluentAssertions (license concerns), use xUnit's built-in `Assert` class
- Test project at `time-tracker/src/TimeTracker.Tests/`
- Mirror the `Features/` slice structure in the test project
- Use `Assert.Equal`, `Assert.True`, `Assert.NotNull`, etc. — no third-party assertion libraries
- Handler classes tested directly with an in-memory EF Core provider (`UseInMemoryDatabase`)
- Run a single test: `dotnet test --filter "FullyQualifiedName~YourTestClass.YourTestMethod"`

---

### Resolving Paths

Always run `git rev-parse --show-toplevel` to find the repo root. All `.squad/` paths are relative to the repo root — do not assume CWD is the root, especially when working from a subdirectory or worktree.
