# Decision 0280: Bounded native analysis and artifact aggregation

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0150](0150-Bounded-Native-Dynamic-Value-Lifetimes.md), and [Decision 0279](0279-Bounded-Record-Planner-Lifetimes.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0279 identified exact native text-arena exhaustion but attributed the
largest reference-runtime construction total to machine-code emission. Mapping
the measured function identity back to source corrected that attribution: the
345,206,567-byte total belonged to `layout.append`, which repeatedly copied a
growing directory while adding one 76-byte function entry. Staging the entry
reduced that function's 100-million-instruction construction total to about
6.16 million bytes and exposed full-map replacement in control analysis and
record-storage planning.

Those maps were semantically bounded but had unsuitable native lifetimes. A
helper received the caller's immutable map, returned a complete replacement,
and left the former caller-owned value below the callee checkpoint. Repeating
that operation retained every historical version in one analysis frame even
though the reference runtime reported a small live set.

## Decision

- Build each layout-directory entry independently and concatenate it with the
  directory once.
- Build instruction markers in bounded chunks. Collect leader offsets in an
  append-only list, materialize the leader map once, and traverse reachable
  blocks through an append-only queue instead of replacing full leader and
  reachability maps.
- Derive record-storage block ordinals from the validated leader map. Append
  block starts, ends, and successors once in block order. Replace the dense
  code-offset table and block-by-record last-use table with bounded leader and
  event-stream scans.
- Record local-use and definition cells in one append-only sparse log and
  materialize each dense liveness matrix once immediately before the retained
  planner.
- Combine ordinary-size `WVFA 1` function artifacts as balanced ranges rather
  than repeatedly extending one aggregate in the exported lowerer. Keep text
  alignment bytes in a separate value and serialize code, padding, data,
  symbols, and relocations through staged object regions. Exact WVO bytes do
  not change.
- Do not raise the 128 MiB arena or silently widen Windvale's ordinary 4 MiB
  `bytes` and file-write contracts. Complete-tool output needs a separately
  versioned multipart object/publication contract or an equally explicit
  qualified builder/streaming owner.

The large lowering core remains under its existing owner because the analysis
helpers depend on its private records and instruction rules. The repository's
reviewable-file guidance remains active; later extraction must follow a real
dependency boundary rather than split the file into numbered fragments.

## Evidence and consequences

- A direct native differential check preserves the canonical 479-byte
  return-42 WVO at SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
  A record-bearing differential check preserves its 2,620-byte WVO at SHA-256
  `bf05545b4081605df90495319e9c74ccf323896836f404b0821a81d7d3cf005e`.
- The reviewed focused Seed selection passes 1/1 in 10.498 test seconds. Its
  single Release build takes 13.36 seconds with zero warnings or errors; total
  command time is 28.215 seconds. It covers exact Stage 0 object agreement,
  control and nominal-record inputs, hosted capabilities, deterministic
  repetition, malformed input, PE/ELF packaging, and absence of CLR modules.
- The current core closure is 363,074 bytes at SHA-256
  `21f18d93f5bbc5c0e03926b659944b6b74e0c392d7bc6aece3437adbfafcbc57`.
  The memory adapter is 357,732 bytes at SHA-256
  `11487dfde84c82ef790dd1f07cfb56365b2c16da99b050fd0717fff3a4ffd541`;
  the hosted tool is 358,760 bytes at SHA-256
  `5bb1fb9eca7b5fe3cefdb43aa58b9db8fdfcd9ddbf0079cdfa130db5658f7b05`.
- Current unpromoted packages are 4,983,808 Windows and 4,984,832 Linux bytes
  at SHA-256
  `0cf26d5ae2e941d8cf106d92b591b977a1fb84b2f358c39e0f49c498991f2655`
  and `ddb96cdf4802c076f29c097a975e2c80195bfda7679f46ae4d5ac136f1f558b2`.
- A measured intermediate reference candidate reduced the
  100-million-instruction construction total from 77,862,904 bytes with a
  730,115-byte peak live set to 47,534,801 bytes with a 188,548-byte peak live
  set after removing the dense block and last-use replacements. Both capped
  runs ended only at their requested `WVR3011` instruction ceiling.
- The final standalone self-lowering probe still exits 1 after 5.532 seconds
  and publishes no partial WVO. This slice therefore does not claim complete
  self-lowering, normal-path cutover, or .NET retirement.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent oracle and recovery path.

Local Development, Standard, Qualification, the full Seed/OS suites, Linux
execution, WebAssembly verification, GitHub verification, artifact promotion,
and ordinary-path cutover remain deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit the sparse scans if valid inputs approach their bounded instruction
envelope. Replace them with a native-supported bounded builder only when that
owner is independently verified. Revisit `WVFA 1` aggregation when a multipart
large-object transport owns exact ordering, size accounting, failure
atomicity, and final WVO identity without redefining ordinary `bytes`.
