# Documentation instructions

These instructions apply throughout `Documents/` in addition to the repository
handbook.

- Read `Documentation-Policy.md` before changing current project,
  architecture, decision, evidence, or runbook material.
- Keep Progress about present standing, Roadmap about forward gates,
  specifications about exact current contracts, decisions about rationale, and
  evidence about completed runs and artifact identities.
- Write active guidance in day-to-day language: outcome first, then what works,
  what is missing, what comes next, and where exact evidence lives.
- Give active documents explicit status, authority, and last-reviewed metadata.
- Use the shared terminology guide instead of redefining common status and
  format words differently in each overview.
- Keep complete hashes in machine-readable manifests or exact evidence records.
  Link to those records from current narrative pages.
- Use the evidence-record schema for new durable run or artifact claims. Do not
  expand the old append-only evidence pages for ordinary new work.
- Regenerate the specification and decision catalogs after changing a title,
  filename, or opening status that they expose.
- Never turn a proposal into implemented behavior through wording alone.
- Preserve historical evidence and superseded decisions. Correct broken facts or
  links, but use a current owner document to explain later standing.
- Refer to a duplicated legacy decision with its linked title, never by the
  number alone. Do not create a new decision-number collision.
- Run `pwsh -NoProfile -File Tools/Verify/Verify-Documentation.ps1` after a
  coherent documentation edit.
