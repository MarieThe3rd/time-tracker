---
name: blazor-tdd
description: Test-driven guidance for implementing new Blazor and .NET features in small slices. Use this when building or refactoring features that require unit, component, or integration tests.
---

Use this skill when implementing new feature slices with tests first or test-first thinking.

## Workflow

1. Identify the smallest meaningful behavior.
2. Write a failing test for that behavior.
3. Implement the minimal code to pass.
4. Refactor safely.
5. Repeat.

## Priorities for this repository

- domain logic tests first
- service tests second
- component behavior tests for critical UI flows
- integration tests for export and persistence boundaries

## For task manager work

Start with tests for:

- valid task creation
- invalid date combinations
- allowed and disallowed status transitions
- filtering by type and status
- markdown export formatting

## Completion rules

- No feature slice is complete without passing tests.
- Each bug fix should have a regression test where feasible.
- If a component is hard to test, recommend design changes that improve testability.
