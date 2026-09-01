# Browser application instructions

Follow the repository handbook and the shared
[browser application architecture](../../Documents/Architecture/Browser-Application-Development.md).

- Keep each deployable application host thin and give its durable state one
  explicit owner.
- Keep drafts, validation, execution, and durable saves visible as different
  states.
- Verify application-specific lifecycle cleanup, server-boundary validation,
  accessibility, theme, and localization behavior.
