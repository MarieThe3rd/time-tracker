# Rachel — History

## Core Context

- **Project:** A Blazor 10 / .NET time tracking application for managing tasks, time entries, and reporting.
- **Role:** Backend Dev
- **Joined:** 2026-03-12T00:54:10.242Z

## Learnings

<!-- Append learnings below -->

## 2026-03-12 — Journal Categories Backend Completion
- Seeded system categories (Work, Personal, Learning, Health) via EF migration
- Added category existence validation to AddEntryHandler and UpdateJournalEntryHandler
- Added NullCategoryAsync to IJournalEntryRepository + SqlJournalEntryRepository
- Added cascade delete + system category guard to ManageJournalCategoriesHandler
