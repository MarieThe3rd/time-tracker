---
applyTo: "**/*.cs"
---

## Backend instructions

- Keep business rules in domain or application services, not buried in UI components.
- Use expressive method and type names.
- Prefer enums or dedicated value objects for task type and task status.
- Protect invariants in constructors, factories, or domain methods.
- Avoid leaking persistence entities directly into UI-facing code.
- Keep export logic separate from task mutation logic.
- Add unit tests for non-trivial rules.
