# Diagnostics, source maps, and provenance

## Independent locales

Source profile, presentation locale, and diagnostic locale are independent
request inputs:

- the source profile determines which stored keyword and imported-public-label
  bytes compile;
- the presentation locale changes only an explicit editor/review view; and
- the diagnostic locale selects bounded human message templates.

Changing either presentation or diagnostic locale cannot alter admission,
canonical identities, output artifacts, cache identity, or success/failure.

## Stable diagnostic record

A source-tool diagnostic retains:

- stable canonical diagnostic identity and phase;
- severity and bounded structured expected/observed fields;
- raw source byte start and length plus decoded scalar span when available;
- exact stored spelling and expected primary spelling when applicable;
- canonical token or declaration identity when resolution reached that point;
- source/target profile identity, version, manifest hash, and component hash
  relevant to the failure;
- catalog identity, content hash, and interface hash for a public label;
- at most a fixed bounded set of related locations or candidate labels; and
- canonical machine values for byte counts, limits, hashes, capability/effect
  identities, and target identities.

Localized prose is presentation. Machines never parse it. Missing, corrupt, or
oversized diagnostic templates fall back to the canonical message without
changing the underlying failure.

## Source conversion map

Conversion produces request-owned provenance for each changed span:

| Field | Meaning |
| --- | --- |
| input byte start/length | Exact admitted raw input span. |
| output byte start/length | Exact validated output span. |
| source spelling | Exact source-profile bytes. |
| target spelling | Exact target-profile bytes. |
| canonical identity | Token or imported public declaration identity. |
| mapping owner | Lexicon identity/hash or catalog identity/hash. |

The descriptor receives its own record. Unchanged regions may be represented as
bounded ranges rather than one record per scalar. The map is produced only after
successful target admission and is associated with both complete raw source
hashes.

The map is not semantic compiler input and is not copied into runtime artifacts.
It supports diagnostics, review, cursor preservation, and debugging. A cache may
reuse canonical semantic evidence only when it regenerates request-owned spans,
maps, diagnostics, and debug provenance for the exact raw source.

## Span rules

Every parser and semantic diagnostic points to stored-source bytes, not a
different-length display view. An editor projects those spans into a visible
view through validated token mappings. If a projection is missing, stale, or
ambiguous, the editor falls back to stored source rather than highlighting an
unrelated display range.

Breakpoints and debug locations bind to canonical declaration/instruction
identity plus exact raw source and mapping provenance. A conversion may move raw
byte offsets; tools remap them only through the successful conversion map and
never by searching for similar text.

## Bounded failure behavior

Malformed input produces one primary bounded diagnostic and a fixed small set of
related fields. Candidate suggestions, collision members, and catalog paths are
count- and byte-bounded. Diagnostic rendering has explicit template,
placeholder, expansion, related-location, recursion, and retained-state limits.

Conversion cancellation or failure discards the private candidate, conversion
map, and success report. It may retain only bounded failure telemetry that does
not expose an output as valid source.
