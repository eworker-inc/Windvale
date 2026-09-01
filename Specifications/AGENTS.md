# Specification instructions

These instructions apply under `Specifications/` in addition to the repository
handbook.

- Specifications own exact current behavior, formats, limits, and failure
  rules. Decisions own rationale; Progress owns present project standing.
- Put a `## Status` or `## Status and ...` section near the title of every new
  specification. Do not infer implementation, verification, or qualification
  from acceptance.
- Give long or frequently used entry points a concise at-a-glance summary using
  the documentation-policy questions. Put it in the generated index when the
  specification is identity-bound, frozen, or would otherwise trigger
  implementation verification without changing a contract.
- State versions, byte order, widths, bounds, validation, malformed-input
  behavior, and platform scope wherever the contract depends on them.
- Keep proposals visibly proposed. Do not add implementation simply to make a
  specification appear complete.
- After changing a specification filename, title, or opening status, run
  `pwsh -NoProfile -File Tools/Documentation/Update-Documentation-Catalogs.ps1`.
- `Legacy-Status-Classifications.json` supplies conservative search metadata
  for reviewed old files whose bytes should not be changed merely to add a
  status. It must never infer implementation, verification, or qualification.
- Do not edit `README.md`, `Specification-Catalog.json`, or files under
  `Indexes/` by hand; the catalog generator owns them.
