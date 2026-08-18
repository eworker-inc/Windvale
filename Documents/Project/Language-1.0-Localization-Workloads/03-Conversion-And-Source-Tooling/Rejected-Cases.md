# Rejected conversion and source-tooling cases

Every rejection leaves the original source unchanged and publishes no candidate
as successful output.

| # | Case | Required rejection |
| ---: | --- | --- |
| 1 | Source descriptor/profile/lock/component/catalog is absent, corrupt, oversized, or hash-mismatched | Fail source admission before replacement planning. |
| 2 | Source catalog is bound to an old interface hash | Fail as stale input; do not reinterpret labels against the new library. |
| 3 | Target profile or component is unavailable | Fail before output construction; do not fall back to English or an installed profile. |
| 4 | Target catalog is incomplete or stale | Fail before output construction and identify the canonical missing/mismatched owner. |
| 5 | Target mapping contains an exact, namespace, keyword, or confusable collision | Fail target admission; do not choose by source order. |
| 6 | Input contains incomplete or invalid editor text | Refuse whole-file conversion; never guess intended tokens. |
| 7 | A project-owned Chinese identifier resembles a localized library label | Preserve it by semantic ownership; textual substitution is forbidden. |
| 8 | A keyword spelling appears inside a longer identifier, string, comment, or resource | Do not replace it; only admitted token spans are eligible. |
| 9 | Conversion would change a literal, comment, documentation, whitespace, line ending, resource, or machine identity | Treat as internal preservation failure and publish nothing. |
| 10 | Target validation resolves any token/declaration differently | Treat as semantic mismatch and publish nothing. |
| 11 | Destination exists without an explicit replacement policy | Refuse overwrite. |
| 12 | In-place input changes after its initial hash | Report publication conflict and preserve the newer file. |
| 13 | Host cannot guarantee atomic same-filesystem replacement | Refuse in-place mode; offer a distinct output path. |
| 14 | Cancellation or I/O failure occurs while a private candidate exists | Remove only that operation's exact private file; preserve input and unrelated files. |
| 15 | Plain pasted text has no trusted profile provenance | Do not guess its language or silently convert it. |
| 16 | Clipboard metadata is malformed, oversized, stale, or disagrees with raw text | Ignore/reject metadata and never grant it semantic trust. |
| 17 | Display-only text is pasted as if it were stored source | Require explicit conversion or insert as ordinary text with normal diagnostics. |
| 18 | Formatter attempts to select the editor locale or another profile | Reject hidden profile conversion. |
| 19 | Formatter chooses a secondary alias or mixes lexicons for width | Reject non-primary/mixed output. |
| 20 | Consumer rename attempts to mutate a dependency's canonical declaration through a localized label | Reject ownership violation and reveal the owner/catalog. |
| 21 | Semantic rename also changes matching prose or string contents | Reject/undo the over-broad semantic operation; prose changes require a separate request. |
| 22 | Canonical reveal or display view writes translated bytes to disk | Reject hidden mutation. |
| 23 | Search conflates exact text, display label, and semantic identity without showing the mode | Reject ambiguous search result presentation. |
| 24 | A diagnostic points at display bytes rather than stored-source bytes | Reject invalid provenance; fall back to stored view. |
| 25 | A stale conversion map is used after either raw source hash changes | Reject remapping and recompute from admitted source. |
| 26 | Compiler cache returns another request's raw spans, diagnostics, or debug provenance | Reject cache entry as identity mismatch. |
| 27 | Diagnostic locale changes success, canonical fields, artifacts, or cache key semantics | Reject locale leakage. |
| 28 | Candidate/collision diagnostic grows without count or byte limits | Stop at the defined bounds and report truncation structurally. |
| 29 | AI translation is invoked automatically during source-profile conversion | Reject as outside the deterministic operation. |
| 30 | Tool claims native review or executable qualification from paper fixtures | Reject the status transition as unsupported evidence. |
