---
name: peer-review-dotnet
description: Structured peer review checklist for .NET and Blazor feature work. Use this when reviewing changes for correctness, maintainability, and requirement alignment.
---

Use this skill to perform a disciplined review before calling work done.

## Review focus

- correctness against requirement
- hidden business rule gaps
- naming and readability
- nullability and optional data handling
- performance risks in common screens
- test completeness
- documentation completeness
- export/reporting behavior

## Review questions

- Are task type and status both modeled clearly?
- Are dates validated consistently?
- Is deliverable ownership stored in a usable format?
- Are filtering and reporting behaviors obvious and testable?
- Does export logic avoid leaking UI concerns?
- Are there migration concerns for existing data?

## Required output

Provide:

1. approve or request changes
2. top risks
3. missing tests
4. missing docs
5. suggested follow-ups
