# Decision 0286: Scalar native staging-manifest bridge

- Date: 2026-08-06
- Status: Implemented candidate; platform resource adapter and grouped dual-host qualification pending
- Advances: [Decision 0285](0285-Strict-Native-Object-Staging-Manifest-Validation.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0285 returns a typed manifest-summary record. That is the right
portable API, but fixed Windows and Linux assembly adapters need one minimal
scalar call boundary before they can use it. Duplicating enum-to-status mapping
in each platform adapter would make malformed-input behavior host-specific.
Changing the source language or native ABI for a publication-only convenience
would be broader than this retirement seam requires.

ABI 22 already admits borrowed `bytes` parameters and `u32` returns. A native
caller passes the address of the complete input descriptor in `R8`, retains the
existing `R10` call-depth and `R11` instruction budgets, and observes the
language/runtime status separately from the successful low `EAX` result.

## Decision

- Add a focused capability-free bridge that accepts one borrowed manifest
  value and returns the strict reader's status as `u32`: `0` is valid and
  `1` through `10` retain the reader's truncated, magic, version, manifest
  size, object size, chunk count, chunk limit, chunk index, chunk position,
  and chunk length order.
- Keep the typed summary in the manifest module. The scalar bridge does not
  expose object size or count on rejection, allocate host resources, derive
  paths, open chunks, or advance publication state.
- Make the malformed-manifest adapter obtain every status through the bridge,
  while reading admitted size/count evidence only from the strict typed API.
- Add a separate no-capability native runner with one valid one-chunk manifest
  and one bad-magic manifest in immutable data. It must call the bridge twice
  through ABI 22's descriptor path and return 42 only for exact results `0`
  and `2`.
- Give the native runner its own checked-in Project 1 manifest. A future
  package builder must resolve the verified function ordinal and map its
  `$function_NNNN` symbol to the platform adapter import, as the qualified WVB
  publisher already does for its transaction bridge.

## Evidence and consequences

- The final reviewed focused compiler selection passes 1/1 in 14.872 test
  seconds after an 8.65-second zero-warning Release build. No broader local
  verification level was run.
- The malformed matrix now receives all eleven status values through the
  scalar bridge. The native runner lowers, passes independent fragment
  verification, requires zero services, executes both descriptor calls as
  machine code, and returns 42.
- The qualified native source front door compiles the three-module malformed
  adapter to 6,942 bytes at SHA-256
  `f5ee0ed8d06e3c444c2cdc5a5a220d62712dd6700eebab4d66bf2995ac7ce344`
  and the three-module native runner to 6,785 bytes at SHA-256
  `7bfbaf5f79fae879d534c05f5c52b9f354519d84fdbeb4acc008d9b90c0a711c`.
- The bridge and two adapters are focused 1,740-, 887-, and 818-byte files.
  No platform assembly or large source file was added.
- The 394,780-byte staging tool and existing unpromoted Windows/Linux
  WVB-to-WVO package identities remain unchanged.

No C# product implementation or WebAssembly implementation changed. Stage 0
remains the independent oracle and invokes the focused native fragment for
local evidence. This slice does not map the bridge into a Windows or Linux
container, open or retain chunk identities, verify chunk contents or a
reconstructed WVO, own replacement/cleanup, complete self-lowering, promote
artifacts, cut over the ordinary path, or retire .NET. Development, Standard,
Qualification, Linux execution, WebAssembly verification, and the grouped gate
remain deferred.

## Reconsideration triggers

Revisit this bridge if the manifest status set changes, ABI 22 changes
descriptor transport, or a future typed native-foreign boundary can consume
the summary record directly. Do not encode admitted sizes into a lossy token or
make platform assembly reinterpret rejected header fields.
