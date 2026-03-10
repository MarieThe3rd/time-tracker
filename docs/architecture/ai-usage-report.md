# AI Usage Report Feature

## Overview

The AI Usage Report provides insights into how AI is used in time entries. It displays all entries where AI was used, the time saved, and any notes, with a chart visualizing usage over time.

## Data Source

- **TimeEntry** model fields:
  - `AiUsed` (bool)
  - `AiTimeSavedMinutes` (int?)
  - `AiNotes` (string?)

## UI

- Accessible at `/reports/ai-usage`.
- Date range filter.
- Chart: AI usage count and total time saved per day.
- Table: Details for each AI usage entry.

## Implementation

- Handler: `AiUsageReportHandler` (queries entries with `AiUsed == true`)
- Page: `AiUsageReportPage.razor`
- Chart: Renders via Chart.js (`renderAiUsageChart` in `charts.js`)

## Tests

- Unit: `AiUsageReportHandlerTests` (xUnit)
- UI: `AiUsageReportTests` (Playwright)

## Navigation

- Link in sidebar: "AI Usage"
