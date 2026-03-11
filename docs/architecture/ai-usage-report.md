# AI Usage Report Feature

## Overview

AI usage insights are integrated into the main Reports experience (`/reports`) under the `AI Insights` tab. The page shows AI-assisted entries, time saved, qualitative notes, and a productivity comparison chart.

## Data Source

- **TimeEntry** model fields:
  - `AiUsed` (bool)
  - `AiTimeSavedMinutes` (int?)
  - `AiNotes` (string?)

## UI

- Accessible at `/reports` via the `AI Insights` tab.
- Date range filter.
- Chart: weekly `Actual Time Spent (min)` vs `Projected Without AI (min)` where projected is `time spent + time saved` for AI-assisted entries.
- Table: Weekly AI summary including AI task count, total time saved, and qualitative fields.

## Implementation

- Handler: `ReportsHandler.GetWeeklyAiUsageAsync` (queries entries with `AiUsed == true`)
- Page: `ReportsPage.razor` (`AI Insights` tab)
- Chart: Renders via Chart.js (`renderAiUsageChart` in `charts.js`)

## Tests

- Unit: `ReportsHandlerTests` and markdown export tests (xUnit)
- UI: `ReportsTests` (Playwright)

## Navigation

- Link in sidebar: "Reports"
