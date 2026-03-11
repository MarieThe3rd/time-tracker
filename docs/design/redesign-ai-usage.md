# AI Usage Redesign

## Goal

Integrate AI usage tracking into the existing time-tracking experience instead of treating it as a separate feature. The Dashboard should surface AI impact alongside time and productivity, and the Reports page should own both operational reporting and AI analytics.

## Implemented Changes

1. Dashboard now includes an AI impact summary card showing weekly and daily AI time saved, plus weekly assisted-entry count.
2. Recent entries on the Dashboard now mark AI-assisted work inline so AI usage reads as part of the same activity stream.
3. The standalone AI Usage navigation item and `/reports/ai-usage` page were removed.
4. Reports now includes a dedicated `AI Insights` tab inside the main reporting surface.
5. Daily note and weekly markdown exports now include AI usage summary data when relevant.

## Interaction Model

1. Dashboard remains the landing page for quick operational awareness.
2. Reports starts with a shared date range and top-level summary cards.
3. Users switch between `Summary`, `AI Insights`, `Daily Note`, `Weekly Summary`, and `Review Export` within one page.
4. AI-specific exports stay available, but they now live inside the Reports workflow rather than a separate feature area.

## Wireframes

### Dashboard

```text
+------------------------------------------------------------------+
| DASHBOARD                                                        |
+------------------------------------------------------------------+
| +----------------+ +----------------+ +----------------+ +------+ |
| | Time Today     | | Avg Product.   | | Entries Today  | | AI   | |
| | 6h 30m         | | 4.2 / 5.0      | | 5              | | 2h15 | |
| |                | |                | |                | | wk   | |
| |                | |                | |                | | 45m  | |
| |                | |                | |                | | today| |
| +----------------+ +----------------+ +----------------+ +------+ |
|                                                                  |
| Recent Entries                                                   |
| [Dev] Setup DB                         [AI 15m]           1h 30m |
| [Meetings] Standup                                       0h 30m |
|                                                                  |
| Recent Journal                                                   |
| [Success] Shipped timer cleanup                                  |
+------------------------------------------------------------------+
```

### Reports

```text
+------------------------------------------------------------------+
| REPORTS                                             [Date Range] |
+------------------------------------------------------------------+
| Logged Time | AI-Assisted Entries | AI Time Saved | Journal      |
+------------------------------------------------------------------+
| Tabs: Summary | AI Insights | Daily Note | Weekly Summary | Review|
+------------------------------------------------------------------+
| AI INSIGHTS                                                       |
| +----------------+ +----------------+ +------------------------+ |
| | AI Work Count  | | Time Saved     | | Avg Saved Per Entry    | |
| +----------------+ +----------------+ +------------------------+ |
|                                                                  |
| AI Usage Over Time                         [Download] [Export]   |
| [ grouped bars: Actual vs Projected Without AI ]                 |
|                                                                  |
| Weekly AI Summary                                                |
| Week Start | Week End | AI Tasks | Time Saved | Value | Notes    |
| 2026-03-02 | 2026-03-08 |   4     | 1h 10m     | ...   | ...     |
+------------------------------------------------------------------+
```

Chart semantics:
`Actual Time Spent (min)` represents logged minutes on AI-assisted entries.
`Projected Without AI (min)` represents `actual spent + time saved` for those same entries.

## Files Touched

1. `src/TimeTracker.Web/Features/Dashboard/*`
2. `src/TimeTracker.Web/Features/Reports/DailyNote/*`
3. `src/TimeTracker.Web/Features/Reports/MarkdownExportService.cs`
4. `src/TimeTracker.Web/Shared/Layout/NavMenu.razor`
5. `src/TimeTracker.Tests/Features/Dashboard/*`
6. `src/TimeTracker.Tests/Features/Reports/*`
7. `src/TimeTracker.UITests/PageObjects/*`
8. `src/TimeTracker.UITests/Tests/*`

## Validation Plan

1. Run unit tests for dashboard and reports handlers plus markdown export behavior.
2. Run UI tests for Dashboard and Reports to verify the integrated navigation and AI tab behavior.
3. Manually verify chart rendering on the Reports AI tab and markdown export output in the app.
