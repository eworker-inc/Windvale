# Decision 0374: Windvale-owned native output table

- Status: Accepted current-host normal-path `WVIO` construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0373](0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md), [Decision 0369](0369-Windvale-Owned-Native-Output-Leaves.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native output-table construction](../../Specifications/Windvale-Native-Output-Table-Construction.md)

## Context

Decision 0369 moved all Windows/Linux output-leaf machine bytes to Windvale,
and Decision 0373 moved their executable-image placement and fill. The normal
runtime still wrote the complete 48-byte `WVIO` table in C#: magic, version,
size, platform, flags, reserved field, console/diagnostic targets, and the
Windows `WriteFile` pointer.

The targets are dynamic host resources and must remain pinned by a narrow host
adapter, but their values do not require C# to own the table format. Windvale
can validate their presence/range rules and copy their opaque bytes into the
established layout without interpreting pointers.

## Decision

- Define exact 48-byte `WVIQ 1` input and a 32-byte `WVIR 1` response envelope.
- Let portable Windvale validate platform, flags, reserved fields, target
  presence, Linux descriptor range, and Windows/Linux writer rules before
  constructing the unchanged 48-byte `WVIO 1` table.
- Treat every 64-bit target as an opaque byte range. Do not add pointer or host
  handle semantics to portable Windvale.
- Retain host ownership of channel preflight, safe-handle lifetime references,
  `kernel32.dll` loading, `WriteFile` resolution, native allocation/copy,
  independent reread, and teardown.
- Consume one exact digest-bound service-free WVNF in the normal runtime. Keep
  the source and retained WVB as recovery/differential evidence without
  embedding the WVB.
- Keep the constructor, artifact loader/response verifier, and host context in
  separate focused files rather than expanding the output-leaf or executor
  owners.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Output-table core WVB | 4,710 | `ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8` |
| Retained output-table bridge WVB | 4,714 | `b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8` |
| Retained output-table bridge WVNF | 50,493 | `f444e80b2afbaaee251892ab7a7a6a879b3e5cffcbf029b0fc382b64bef97afb` |

## Evidence and consequences

The reviewed focused constructor test pins and reproduces all source, WVB, and
WVNF identities; confirms that the runtime embeds no constructor WVB; compares
six Windows/Linux flag/target models plus nine malformed requests through the
reference interpreter and verified native fragment; requires exact expected
`WVIO` bytes; and reproduces the bridge through the ordinary native source
front door. It passes 1/1 in 1.677 seconds.

The existing focused live-output case then passes 1/1 in 0.806 seconds across
authorization, real output, dual channels, failure mapping, linked execution,
and teardown. The final affected Release build passes with zero warnings and
errors in 8.85 seconds. No Development, Standard, Qualification, or Linux gate
was run.

The normal runtime no longer writes `WVIO` fields in C#. It still projects the
small request, acquires and pins dynamic host targets, verifies and copies the
Windvale result into native memory, owns W^X application publication, invokes
the entry, and tears resources down. `WVFI` and `WVFO` table construction remain
the next analogous managed binding seams.

## Reconsideration triggers

Replace this request boundary when a native host adapter can acquire targets
and invoke the constructor without managed projection. Keep dynamic targets
out of WVB, WVO, caches, and retained artifacts. Version the contract if a
platform requires a target shape that cannot preserve `WVIO 1`.
