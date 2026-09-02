# Verification instructions

These instructions apply under `Tools/Verify/` in addition to the repository
handbook.

## Change-aware verification

- Run `Verify-Changed.ps1 -PlanOnly` before executing an ordinary local plan.
  Account for the cold duration of cache preparation, compilation, packaging,
  reconstruction, test execution, and cleanup against the repository-wide
  ten-minute development budget. Do not launch an over-budget or unknown-cost
  owner and wait to discover that it takes hours.
- A longer run requires explicit advance human approval for a named command and
  maximum duration, or a named qualification need after disclosing the expected
  duration. Stop at the approved budget, preserve completed evidence and caches,
  and never publish an interrupted run as a pass.
- Treat routing as a proposed evidence plan. Every selected owner needs a causal
  connection between the changed contract and a failure it can detect. Stop and
  correct a disproportionate plan instead of running it mechanically.
- `Verify-Changed.ps1` is the ordinary local entry point. After reviewing its
  plan and cost, run it once after a coherent edit has settled when the full plan
  fits the development budget.
- The classifier uses lightweight scope for ordinary documentation and
  editor-package-only work, website scope for static site, browser packaging,
  Cloudflare function, and website-tool changes, and development scope for
  implementation and specification changes with mapped native owners.
- Website scope runs `Verify-Website.ps1`.
- Development scope maps maintained boundaries to focused native owners in
  canonical order. Refuse uncovered gaps; do not hide them with an unfiltered
  or managed fallback.
- Focused native OS owners run through
  `Invoke-WindvaleTests.ps1 -Owner <owner-name>`.
- Non-normative generated indexes and companion summaries belong to
  documentation verification even when they route to a specification. Do not
  weaken routing for an actual semantic, format, limit, or failure-rule change.
- Keep `Tools/Editors/Windvale/` synchronized with changes to the implemented
  `.wv` lexical surface and run its focused editor verifier after grammar or
  package-metadata changes. WVA assembly remains separate from source
  classification.

## Broad and cross-host claims

- Managed Stage 0 source and tests are absent from `main`. Restore the exact
  recovery release in a separate workspace only for a named recovery, security,
  or historical differential investigation.
- Use managed Development, Standard, or Qualification tiers only for that
  explicit recovery or differential evidence, or for the final retirement gate.
- GitHub runs affected focused native owners on Linux for ordinary implementation
  and specification pushes and pull requests. It adds the Windows development
  host only for Windows command, PowerShell, platform, or binary changes. Each
  automatic development job has a 15-minute wall-clock bound.
- Invoke the independent complete dual-host Qualification workflow explicitly
  for a release candidate, promotion, security boundary, or deliberate
  qualification claim. It is not a per-commit gate.
- Run broad native, managed comparison, bootstrap, WebAssembly-engine, or live
  OS-boot gates only when the changed boundary or named claim requires them.
- Cold bootstrap, reconstruction, or packaging that cannot fit the development
  budget must be a separately selected qualification product or be replaced by
  a focused current-source owner with valid reusable inputs. It must not occur
  as hidden setup inside an ordinary development owner.
- Portable semantics, bytecode, serialization, runtime behavior, and golden
  hash changes need evidence from both hosts before claiming cross-host
  conformance.

## Verifier implementation

- `Invoke-WindvaleTests.ps1` is the sole cross-host owner coordinator. Do not
  add another `.cmd`/`.sh` coordinator pair. Existing paired owner scripts are
  transitional leaf implementations and remain behind the PowerShell runner
  until they can move without changing their evidence contract.
- Give every verifier one clear owner, bounded inputs, bounded diagnostics,
  stable progress, and explicit failure behavior.
- Declare every input, tool identity, host dependency, and verifier version used
  by cached results. Never reuse evidence across an undeclared dependency.
- Keep independent owners parallel and resumable where practical. Do not add a
  wrapper that only reruns an existing owner without adding a distinct contract
  or required host boundary.
- Tests for the change classifier must prove both positive routing and important
  exclusions. A new maintained source boundary needs a focused owner mapping.
