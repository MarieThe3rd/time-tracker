# Squad Decisions

## Active Decisions

### Journal Categories — Architecture Decisions (2026-03-12)

- **Cascade delete strategy:** Deleting a category nulls out `JournalCategoryId` on all referencing entries (soft disassociation). Entries are not deleted. This is done via `NullCategoryAsync` in `IJournalEntryRepository`.
- **System category protection:** System categories (`IsSystem=true`) cannot be deleted. Guard is in `ManageJournalCategoriesHandler.DeleteAsync` (throws `InvalidOperationException`). UI also hides the delete button for system categories.
- **Invalid category validation:** If a `JournalCategoryId` is provided to AddEntry or UpdateEntry and the category doesn't exist, the handler silently nulls it out. No exception is raised — entries are never blocked from saving due to a bad category reference.
- **Icon selection:** Category icons are selected from a curated 12-icon Bootstrap icon list in the UI. Icon is persisted as a Bootstrap icon class string (e.g., `"bi-briefcase"`).
- **Test isolation:** Cascade delete tests use SQLite in-memory provider (not EF InMemory) because `ExecuteUpdateAsync` requires a real SQL provider.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
