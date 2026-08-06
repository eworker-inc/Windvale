# Decision 0287: Validated native staging-manifest accessors

- Date: 2026-08-06
- Status: Implemented candidate; platform resource adapter and grouped dual-host qualification pending
- Advances: [Decision 0286](0286-Scalar-Native-Staging-Manifest-Bridge.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0286 carries the strict manifest status across ABI 22, but a platform
adapter would still have to read the admitted object length, chunk count, and
entry offsets itself. Implementing those reads in both Windows and Linux
assembly would duplicate `WVOP 1` field offsets and range assumptions at the
host boundary. Passing the typed summary record directly would require a new
native foreign-record contract solely for this publication seam.

The adapter will already retain one immutable manifest snapshot. Revalidating
that bounded value before each scalar query is therefore simple, deterministic,
and keeps all format interpretation in Windvale source. The current maximum of
518 chunks makes the repeated scan finite and explicit.

## Decision

- Extend the capability-free ABI bridge with four scalar queries over the same
  borrowed manifest descriptor: admitted object bytes, admitted chunk count,
  chunk position by index, and chunk length by index.
- Re-run the strict reader before every query. Object bytes, chunk count, and
  chunk length return zero for invalid input; their valid domains are nonzero.
  Chunk position returns `0xffffffff` for invalid input or an out-of-range
  index; every valid position is below the 32 MiB object ceiling.
- Bound the index by the admitted count before calculating an entry offset.
  Platform assembly must not read manifest header or entry fields directly.
- Require the eventual native adapter to preserve the exact immutable manifest
  snapshot across validation and all accessor calls. Resource identity and
  content checks remain separate from manifest parsing.
- Extend the malformed-input adapter so status and all admitted fields come
  through the scalar bridge. Extend the no-capability native runner across
  valid values, out-of-range sentinels, and malformed-input sentinels.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 15.625 test seconds
  after a 16.94-second zero-warning Release build. No broader local
  verification level was run.
- The native runner calls all five bridge functions through ABI 22, requires
  zero services, passes independent fragment verification, and returns 42.
  The complete malformed matrix confirms that rejected input cannot expose an
  object length, count, first-entry position, or first-entry length.
- The qualified native source front door compiles the three-module manifest
  adapter to 7,991 bytes at SHA-256
  `e7a29d26e78c3cdae93868960d5be537709fc7ed8ef83de1c0bf84ca5e63c3fa`
  and the three-module native runner to 8,807 bytes at SHA-256
  `f4f5e00013d370a431af5b78d36beca37d9fe5504e788204aea6025341607417`.
- The bridge and focused adapters remain reviewable 3,368-, 1,105-, and
  2,353-byte files. No platform assembly or large source file was added.
- The 394,780-byte staging tool and existing unpromoted Windows/Linux
  WVB-to-WVO package identities remain unchanged.

No C# product implementation or WebAssembly implementation changed. Stage 0
remains the independent oracle and native-fragment executor. This slice does
not derive chunk resource names, retain native file identities, compare chunk
contents with exact lengths, verify the reconstructed WVO, own sibling
replacement/cleanup, complete self-lowering, promote artifacts, cut over the
ordinary path, or retire .NET. Development, Standard, Qualification, Linux
execution, WebAssembly verification, and the grouped gate remain deferred.

## Reconsideration triggers

Replace repeated validation with an equally strict immutable admission handle
only if Windvale gains a versioned native foreign-state contract that preserves
snapshot identity and lifetime. Do not trade the bounded scan for unchecked
platform parsing or a forgeable scalar token.
