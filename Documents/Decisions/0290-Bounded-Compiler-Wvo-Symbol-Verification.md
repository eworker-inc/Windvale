# Decision 0290: Bounded compiler-WVO symbol verification

- Date: 2026-08-06
- Status: Implemented candidate; relocation/content adapter and grouped dual-host qualification pending
- Advances: [Decision 0288](0288-Segmented-Large-Native-Wvo-Section-Envelope.md), [Decision 0287](0287-Validated-Native-Staging-Manifest-Accessors.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Windvale object format](../../Specifications/Windvale-Object-Format.md) and [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0288 validates the compiler-produced WVO header and section envelope
without joining large section data, then exposes the exact symbol position.
The native writer already emits the complete symbol table as one separately
owned bounded value after `.text` and optional `.rodata`. Leaving that chunk
unparsed would still force a platform adapter either to trust unvalidated WVO
records or duplicate compiler-specific name, order, and range policy in
Windows and Linux assembly.

The current native lowerer has a deliberately narrower symbol profile than
general WVO 1.0. It emits at most 64 local data symbols, at most 511 local
helper-function symbols, and one exported `Main`; it emits no imports.

## Decision

- Add a focused capability-free symbol reader above the validated section
  envelope. It receives the actual bounded symbol chunk and requires its length
  to equal the manifest entry at the admitted symbol position.
- Require zero reserved fields and the exact compiler bindings, kinds, section
  indices, and names: sequential `$data_NNNN`, ascending `$function_NNNN` for
  every non-main function ordinal, and one final exported `Main`.
- Require data symbols to cover `.rodata` contiguously in declaration order.
  A two-section object has one through 64 data symbols; a one-section object
  has none.
- Reconstruct the one omitted Main ordinal from the helper sequence. Require
  all function ranges to be nonempty, ordered, nonoverlapping, and contiguous,
  with the exported Main range filling the exact omitted gap. Admit one final
  zero-through-15-byte text padding extent only when `.rodata` exists.
- Bound the resulting compiler profile to 512 functions and 576 total symbols.
- Consume the complete symbol chunk exactly. Derive the relocation position,
  require the declared relocation table to be one exact manifest chunk of
  `count * 20` bytes, and require that extent to end at the admitted object
  length. With no relocations, the symbol chunk itself must end the object.
- Return data-symbol count, function count, relocation position, and relocation
  bytes only after every check succeeds. Rejection returns one of thirteen
  named statuses and zero evidence.
- Add a focused ABI-22 scalar bridge that reruns this complete validation for
  each query over the same borrowed snapshots.

This reader verifies the complete compiler-produced symbol table, not general
WVO imports/exports. Relocation record semantics and code-placeholder bytes
remain the next segmented boundary.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 1.868 test seconds
  after a 25.42-second zero-warning Release build. No broader local
  verification level was run.
- The matrix accepts one- and two-section objects and a three-function object
  with Main in the middle ordinal. It rejects invalid envelope, chunk length,
  truncation, flags, shape, name, order, range, limits, data coverage, function
  coverage, and relocation-boundary evidence with zeroed results.
- A capability-free native runner calls every scalar query over immutable
  manifest, prefix, read-only-header, and symbol values, passes independent
  fragment verification, requires zero services, executes as x86-64 machine
  code, and returns 42.
- The native source front door compiles the five-module evidence adapter to
  33,091 bytes at SHA-256
  `375e906a095c1c5dd8f98a92876312af434c0d2d385be280568ed1cbf15000aa`
  and the five-module native runner to 32,516 bytes at SHA-256
  `024c261ed2469410c095fabe8f8ddbd9a51dbc6de653f7874f2baec169201e3d`.
- The parser, scalar bridge, fixtures, and tests remain focused files rather
  than enlarging the lowering core or the existing 100 KiB WVB-to-WVO test.

No C# product implementation or WebAssembly implementation changed. This
slice does not validate relocation record contents or zero placeholders,
compare section-data chunks, retain native resource identities, construct or
replace a durable sibling, clean up, complete tool self-lowering, promote
artifacts, cut over the ordinary path, or retire .NET. Development, Standard,
Qualification, Linux execution, WebAssembly verification, and the grouped gate
remain deferred.

## Reconsideration triggers

Revisit this profile if the native writer emits imports, multiple exports,
noncontiguous symbol ranges, more section kinds, more than four decimal suffix
digits, or a segmented symbol table. General WVO evolution belongs in the
object-format contract and must not be inferred from this compiler-specific
publication profile.
