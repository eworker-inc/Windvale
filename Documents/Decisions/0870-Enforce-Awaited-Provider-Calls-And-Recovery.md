# Decision 0870: enforce awaited provider calls and recovery

## Status

Accepted on 2026-08-28.

## Context

Language 1.0 already requires a potentially suspending provider operation to
be `async`, carry `task.suspend`, and be called with `await`. The implemented
source compiler retained the async bit in callable types and WVB descriptors,
but ordinary direct and indirect calls did not yet compare that bit with the
source call's `await` spelling. A program could therefore omit `await` from an
async call or write `await` before a synchronous call without rejection.

Slice 7 also requires child-provider loss and restart to remain distinct from
task-runtime loss and restart. The existing runtime-environment fixtures cover
the task runtime, but did not prove that child work can observe a provider
generation change, preserve an indeterminate mutation result without replay,
explicitly refresh its endpoint, and use that refreshed generation in later
accepted work.

While building that workload, the scalar runner exposed a record allocator
boundary. After collection, a free record range could start below the retained
value-buffer tail and extend beyond it. The allocator selected the correct
slot, but appended all new fields at the old tail, so the published handle
could address a stale prefix followed by a valid suffix.

## Decision

For both a directly named function and an exact indirect callable value, the
source call's `await` marker must exactly equal the callable's async flag. An
awaited call is valid only from an async function. Existing effect analysis
still proves `task.suspend`; this rule neither infers effects nor introduces a
second call operation. `Task.Await` retains its separate affine-handle lowering.

The permanent provider-recovery fixture executes four accepted children
against provider generation 41. Two complete normally. One reports a restart
whose mutation is determinate, and one reports a restart whose mutation is
indeterminate. The parent consumes all four typed handles without retrying
either failed request, validates exact expected and observed generations,
refreshes to one new rights-limited generation-42 endpoint, and then accepts a
fifth child that performs an awaited provider call through that generation.

The scalar aggregate allocator must publish field bytes beginning at the
selected record offset. When the new record crosses the current value-buffer
tail, it preserves only the prefix before that offset and appends the complete
new field sequence. A selected offset beyond the current tail is invalid. The
record handle is published only after this replacement succeeds.

## Evidence

Seven bounded source cases cover direct and indirect async calls, aggregate
returns, omitted `await`, awaiting a synchronous call, and awaiting from a
synchronous caller. Three accepted modules are deterministic, four rejected
cases publish no WVB, and the evidence SHA-256 is
`d08ace8e9143b4ae4fc6e8762769d3705fdb8424cc53f21783079dc3d7eb15bd`.

The provider-recovery fixture compiles to a deterministic 12,297-byte WVB at
SHA-256
`eb8dc8047fd2ddd7e7eb98c7e443396ac5e9d240fabb060acb88769888d4f067`.
It contains 15 functions; its largest function has 2 parameters, 131 locals,
5 maximum stack cells, and 2,141 code bytes. The compiler-aligned verifier
accepts it. The pre-fix runner returns diagnostic result `95`; the corrected
runner returns `42`.

The corrected development runner contains 228 functions and 430,435 code bytes
in a 482,767-byte WVB at SHA-256
`fc4724c7756f22eb52dd6ed4da9737a865e14ea4d52df1de69fc10236970ff4f`.
Its 5,907,456-byte current-host Windows package has SHA-256
`2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b`.
These are development identities, not promoted distribution artifacts.

The complete affected owner passes all 61 named phases and 172 declared cases,
including 24 valid modules, 69 malformed modules, 33 structured-task cases, 46
task-runtime cases, 17 task-environment cases, and the 7 async-call cases. The
114-owner registry declares 5,568 cases in 18,828 LF-only bytes at SHA-256
`6138ca33e6c8e06b4baa1d99fc01c8a5be9bf6d69768592d4e80b4750bfb2b34`.

The immutable 0861 source-amendment manifest remains unchanged. Decisions 0864
through 0869 subsequently changed the frozen Foundation and migration-plan
inputs while implementing their accepted structured-task contracts. This
decision binds those two resulting identities through the new 3,841-byte
`Windvale-Language-1.0-Source-Amendment-0870-Candidate.txt` manifest at
SHA-256
`e5a8928696ec8626adbfe94faf9284d037b6b40aa5d3dea6a250ddc6b6b770d4`.
Its 251-input closure contains 1,759,474 bytes; the 46,260-byte canonical entry
stream has SHA-256
`f574e26f1a7397517f192be412268bf7768a6cfbc8685b892fb38f6a44672c80`.

## Consequences

- Suspension is visible and mechanically enforced at every direct and indirect
  source call.
- Async and sync calls retain the same existing WVB call instructions; the
  async bit remains verifier evidence rather than a hidden runtime dispatch.
- Provider restart is an application/provider outcome, not a task-runtime
  terminal outcome.
- An indeterminate mutation is observed and preserved but never silently
  retried.
- Endpoint refresh is explicit and produces a new generation-bound value; it
  does not mutate ambient provider state or grant additional rights.
- Record reuse across the value-buffer tail cannot publish a handle over stale
  prefix fields.
- Source grammar, Foundation task signatures, WVIR 1.21, and WVB 1.32 remain
  unchanged.

## Reconsideration triggers

Reconsider the source rule only if Language 1.x introduces an explicit
non-suspending projection of an async callable. Reconsider the provider fixture
when a real hosted provider adapter can replace the source model while
preserving the same typed generation and no-replay contract. Reconsider the
record storage strategy if the runtime replaces fixed-slot aggregate storage;
the replacement must keep an equivalent crossing-tail regression.
