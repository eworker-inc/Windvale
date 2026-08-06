# Decision 0251: Bounded native wide calls

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0249](0249-Bounded-Native-Descriptor-Calls.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0249 completed descriptor parameters and returns inside ABI 22's four
register positions. The compiler-produced hosted lowerer then reached a six-parameter
helper. Its current closure contains 40 functions wider than four parameters and a
measured maximum width of 16, while the shared Stage 0 ABI already defines a bounded
maximum of 64 call parameters.

Keeping the Windvale candidate at four parameters would preserve an artificial
bootstrap gap even though the stable ABI, native verifier, and independent Stage 0
implementation already own the required stack-cell contract. Descriptors also make
this more than a scalar-width change: a caller must copy both words and adjust any
caller-owned result address while the outgoing area is reserved.

## Decision

### Match ABI 22's bounded 64-parameter call contract

Admit zero through 64 helper parameters while retaining parameterless exported
`Main() -> i32`. The first four arguments keep the existing `R8`, `R9`, `RCX`, and
`RDX` representations. A call with later arguments reserves
`(parameter-count - 4) * 16` bytes. Each fifth-through-64th argument occupies one
canonical 16-byte outgoing cell: scalars copy their low 32 bits, record values copy
their 64-bit handle, and descriptors copy both 64-bit words.

After allocating its ordinary frame, the callee reads later arguments at
`frame-bytes + 8 + (argument - 4) * 16` and copies the same exact representation into
its local parameter cell. The caller includes the outgoing-area adjustment when it
places a descriptor result cell or record result range in `RAX`, restores the outgoing
area immediately after the call, and only then follows the existing packed-status and
result-storage path.

Expand the internal signature directory from 16 to 76 bytes per function. It retains
machine offset, machine length, one-byte parameter count, one-byte return type, and two
reserved zero bytes, followed by 64 padded parameter-type bytes. This is private
lowerer evidence rather than a new serialized format or ABI revision.

### Give call transport one focused owner

Add the 364-line `Native-X64-Lowering-Call-Arguments.wv` module. It owns exact byte
measurement and emission for register arguments, outgoing stack cells, callee
parameter copies, and stack reservation/release across scalar, record, and descriptor
representations. The call-instruction, record, descriptor, and core modules delegate
to this boundary and remove their duplicated register-only emitters. This reduces the
already-large core and adjacent modules by more lines than the focused module adds.

### Extend the existing differential fixture

Extend `Wvb-To-Wvo-Descriptor-Calls.wv` with one six-parameter helper whose register
positions mix descriptor and scalar values and whose fifth and sixth positions carry
an `i32` and a complete `bytes` descriptor through outgoing stack cells. It returns
the sixth argument and retains the existing exact result-42 oracle. Require the
reference interpreter and Stage 0 native backend to agree on behavior, then require
both Windvale adapters to reproduce Stage 0's complete WVO byte for byte.

## Evidence and consequences

- The focused native WVB-to-WVO test passed all retained objects and the widened
  descriptor-call object in 14.770 seconds. It also executed the direct current-host
  package and verified the complete Windows/Linux application identities.
- A direct single-fixture Windvale execution completed with status `Valid`, ABI 22,
  18,416 code bytes, and an 18,847-byte WVO before the shared exact-object check.
- The current layout, core, memory-adapter, and hosted-tool WVB hashes are
  `29f9d724e9cd5029a923e550fde3832c186de75067259c6efa8f7737d8494391`,
  `a9f37d0bbd551d328ca21d0aa810bc2a1c067c1ae824b866e94b08d31fe7c820`,
  `414e3ec8ebe2b5010f12675cb69a9a931ee3cf17974c38aed62c5dcd516b5e0f`,
  and `aab08feba3c5694e94ba0af582a3481dfe53afa6ebefe1ca9d3312c688a31e4f`.
  The latter two contain 317,949 and 318,977 bytes and reproduce exactly through the
  pinned native build driver.
- The unpromoted Windows package is 4,406,272 bytes with SHA-256
  `32089b25357de28ba9c63dbaa9718109a7f6ae87712ac5cd4cdbbc13cf7fda3a`.
  The Linux package is 4,407,296 bytes with SHA-256
  `170ed07261b51ee1d18c7f39465ac9fd337ddfa60d4bd745c34d1ec9c295c3f2`.
- The native front door reports 330 functions in each updated adapter closure and
  reproduces the complete Stage 0 WVB identities without invoking .NET in the build
  child.
- No C# implementation changed. Stage 0 remains the frozen recovery and independent
  differential lane until the grouped retirement gate passes.
- Local Standard, Qualification, full Seed/OS suites, and artifact promotion remain
  deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit this representation if ABI 22 changes its register count, value-cell width,
record handle, descriptor layout, or maximum call width. Do not add an unbounded
variadic path or infer host calling conventions from this internal directory.
