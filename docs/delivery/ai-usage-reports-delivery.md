# AI Usage Reports — Delivery Summary

| Field | Value |
|---|---|
| Feature | AI Usage Reports (weekly grouping + Obsidian export) |
| Branch | `feature/ai-usage-reports` |
| Delivered | 2025-07-23 |
| Test result | 158 passing, 0 failing |

---

## Files Changed

### Production code

| File | Change |
|---|---|
| `AiUsageReportHandler.cs` | Added `AiUsageWeeklyItem` class and `GetWeeklyAiUsageAsync(DateTime from, DateTime to)` |
| `MarkdownExportService.cs` | Added `BuildAiUsageWeeklyReport(DateOnly from, DateOnly to, List<AiUsageWeeklyItem> weeks)` |
| `ReportsHandler.cs` | Added `PushAiUsageReportAsync(DateOnly from, DateOnly to, string markdown, UserSettings settings)` |
| `AiUsageReportPage.razor` | Rewritten — weekly table, chart, qualitative insights section, Export and Download buttons, date-range guard |

### Test code (new / extended)

| File | Tests added |
|---|---|
| `AiUsageReportHandlerTests.cs` | 7 weekly aggregation tests |
| `MarkdownExportServiceTests.cs` | 7 builder tests (including pipe-escape coverage) |
| `ReportsHandlerAiUsagePushTests.cs` | 6 push tests (new file) |

---

## Test Summary

- **Total passing:** 158
- **Failing:** 0
- Coverage areas: weekly grouping logic, ISO week boundary handling, markdown rendering, pipe escaping, vault file write, overwrite detection, date-range guard behaviour on the page.

---

## Peer Review — Issues Found and Resolved

### Blockers (2) — both fixed

1. **Pipe injection in markdown table** — User-supplied `ValueAdded` and `Notes` strings containing `|` characters broke the markdown table structure. Fixed by escaping `|` → `\|` at render time inside `BuildAiUsageWeeklyReport`. Test added in `MarkdownExportServiceTests.cs`.

2. **Overwrite silently discarded** — The initial `PushAiUsageReportAsync` implementation wrote the file without signalling whether an existing file was replaced. Fixed by returning a `(string Path, bool Overwritten)` tuple so callers have visibility. Tests added in `ReportsHandlerAiUsagePushTests.cs`.

### Minors (2) — both fixed

3. **Date-range guard placement** — Guard originally blocked the entire page render when the date range was invalid. Changed to an inverted guard that disables only the Export and Download action buttons, keeping the rest of the page usable.

4. **UTC assumption undocumented** — Week grouping in `GetWeeklyAiUsageAsync` operates on the UTC date of `StartTime` with no comment. XML doc comment added to the method to surface this assumption.

---

## Known Open Items / Future Work

| Item | Detail |
|---|---|
| UTC vs local week boundaries | Week grouping uses UTC dates. Users in timezones that differ from UTC may see time entries near midnight cross into an adjacent ISO week. No fix scoped — tracked as a known limitation. |
| Silent overwrite | Overwrite is indicated by the `Overwritten` flag returned from `PushAiUsageReportAsync` but there is no UI confirmation dialog before an existing vault file is replaced. A future iteration could add a confirmation step. |
| Qualitative insights content | `ValueAdded` and `Notes` are free-text fields populated by the user per entry; no automated insight generation is in scope. |

---

## Obsidian Export Format

Output file: `{VaultRootPath}/AI-Usage-Report-{from}-to-{to}.md`

```markdown
---
report: ai-usage
from: YYYY-MM-DD
to: YYYY-MM-DD
generated: YYYY-MM-DDTHH:mm:ssZ
---

# AI Usage Report — YYYY-MM-DD to YYYY-MM-DD

| Week Start | Week End | AI-Assisted Tasks | Time Saved (min) | Value Added | Notes |
|---|---|---|---|---|---|
| YYYY-MM-DD | YYYY-MM-DD | 4 | 120 | High | Drafted spec |
```

- Pipe characters in `Value Added` and `Notes` cells are escaped as `\|`.
- If a file with the same name already exists in the vault it is overwritten; the `Overwritten` flag in the return value reflects this.
