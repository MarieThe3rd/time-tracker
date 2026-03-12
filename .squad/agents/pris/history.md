# Pris — History

## Core Context

- **Project:** A Blazor 10 / .NET time tracking application for managing tasks, time entries, and reporting.
- **Role:** Tester
- **Joined:** 2026-03-12T00:54:10.249Z

## Learnings

<!-- Append learnings below -->

## 2026-03-12 — Journal Categories Test Coverage
- Wrote 9 new tests: category validation in AddEntry/UpdateEntry handlers, cascade delete, system category delete guard
- Fixed 3 existing tests broken by handler constructor changes
- 297/297 tests passing
- Used SQLite in-memory for cascade tests (EF InMemory doesn't support ExecuteUpdateAsync)
