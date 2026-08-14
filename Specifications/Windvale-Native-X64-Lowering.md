# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.11 module, independently verifies the metered scalar-control and direct-call subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

This is algorithmic machine-byte selection, not a lowering plan, private intermediate format, or collection of whole-program stencils. The bounded core now has a paired [native WVB-to-WVO application candidate](Windvale-Native-Wvb-To-Wvo.md). The Windvale-native compiler is the normal implementation for this accepted subset. Wider recovery and differential evidence remains outside the normal build and execution path.

## Public result

```text
Compilerˉlowerˉwvbˉnativeˉx64(Input: bytes)
    -> Compilerˉnativeˉx64ˉsummary
```

The summary contains a stable status, WVO object bytes on success, a reserved zero `Value` field, native ABI `22`, and the computed code size. Failure returns an empty object and never publishes a partial WVO.

The status vocabulary is:

| Status | Meaning |
| --- | --- |
| `Valid` | A complete canonical WVO was emitted. |
| `Invalidˉwvb` | Header, section envelope, length, order, trailing bytes, or module identity is malformed. |
| `Unsupportedˉprofile` | The module is neither portable nor hosted. |
| `Unsupportedˉmodule` | Capabilities, nominal declarations, or static data outside the accepted bounded profiles are present. |
| `Unsupportedˉfunction` | The function or export shape is outside the first slice. |
| `Unsupportedˉcode` | The instruction stream is outside the first slice. |
| `Outputˉlimit` | Projected or emitted code/object bytes exceed or disagree with the bounded layout. |

## Accepted WVB subset

The core accepts exactly:

- WVB 1.11 with seven canonical sections, an explicit absent-metadata byte, no trailing bytes, and a valid Seed module identifier;
- portable profile with no capabilities, or hosted profile with one through six canonically ordered declarations drawn from the exact `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count` signatures; all six exact signatures may be called in this slice;
- zero through 64 canonical record declarations, zero through 64 canonical enum declarations, at most 128 nominal declarations in total, and zero through 256 immutable text, bytes, or `[i32]` declarations; every record has one through 64 named fields whose shapes are admitted primitives, enums, or acyclic admitted records and whose recursively flattened backing is at most 64 cells, every enum has one through 256 named members with explicit unique signed backing values, text contains at most 1 MiB of valid UTF-8, bytes contains at most 4 MiB, and each i32 array contains at most 262,144 elements;
- one through 512 functions with exactly one exported parameterless `Main() -> i32` or `Main() -> bytes` at any ordinal and every other function non-exported;
- parameterless `Main() -> i32` or `Main() -> bytes`, zero through 64 `i32`, `bool`, `text`, `u8`, `u32`, `u64`, `bytes`, admitted enum, or admitted record helper parameters and those same primitive or nominal helper returns, declared locals using those seven primitive types plus admitted enum or record identities, fewer than 2,048 combined parameters and declared locals per function, a declared maximum stack depth from one through 1,024, at most 131,072 code bytes and 32,768 decoded instructions per function, adjacent exact code ranges, and no unclaimed function-section bytes; record-bearing functions retain the narrower 1,024-basic-block, 8,192-instruction, 1,024-declared-record-local, 256-produced-record-value-per-block, and immutable-record-parameter limits;
- one control-flow graph per function of at most 8,192 instructions drawn from `i32.const`, `bool.const`, `text.const`, `u8.const`, `u32.const`, `u64.const`, `i32.format`, `u32.format`, `u32.from_u8`, `u64.from_u32`, `bytes.const`, `enum.const`, `local.load`, `local.store`, `data.length`, `data.load.i32`, `bytes.length`, `bytes.slice`, `bytes.read_u8`, `bytes.read_u16_little`, `bytes.read_u32_little`, `bytes.read_i32_little`, `bytes.read_u64_little`, `bytes.concat`, `bytes.from_u8`, `bytes.from_u16_little`, `bytes.from_u32_little`, `bytes.from_i32_little`, `bytes.from_u64_little`, `text.concat`, `text.utf8_is_valid`, `text.from_utf8`, `text.quote`, `text.to_utf8`, `enum.equal`, `enum.not_equal`, `enum.name`, `record.create`, `record.field`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, checked `u32.add`, `u32.subtract`, `u32.multiply`, `u32.divide`, `u32.remainder`, `u32.bitwise_and`, `u32.bitwise_or`, `u32.bitwise_xor`, `u32.bitwise_not`, `u32.shift_left`, `u32.shift_right`, checked `u64.add`, `u64.subtract`, `u64.multiply`, `u64.divide`, `u64.remainder`, all six signed `i32` comparisons, all six unsigned `u32` and `u64` comparisons, `u8.equal`, `u8.not_equal`, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, `call`, `call.capability`, and `return`; every data, enum-constant, record-construction, field, or capability operation names a declaration and item of the exact required kind, comparisons consume the same nominal enum identity, every direct call names any in-range function and consumes its exact typed signature, and every capability call names one of the six admitted signatures and consumes and produces its exact typed stack shape; and
- instruction-aligned forward or backward targets, empty stacks at every block edge, complete fixed-point reachability from entry, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, malformed capability ranges or signatures, invalid local, data, type, enum-member, record-field, function-call, or capability-call indices, wrong descriptor data kinds, mismatched enum or record identities, other type or stack mismatches, malformed branch targets, unreachable control blocks, record temporaries live across block edges, cyclic record containment, flattened records wider than 64 ABI cells, and mismatched maximum-stack declarations fail closed. Forward, backward, self-recursive, mutually recursive, and cyclic call edges share the same exact signature checks. Every function entry consumes ABI 22's shared call-depth budget before its frame executes, and every emitted WVB instruction, including every data, enum, record, or capability operation, backward jump, and call, consumes the shared instruction budget before its operation. Recursion and nonterminating cycles therefore reach `WVR3004` or `WVR3011` rather than escaping resource accounting. Mutable record parameters, mutable or other data declarations, other capability declarations and calls, and broader nominal identities remain outside the subset.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes and, when static data exists, the canonical alignment padding;
- an optional 16-byte-aligned `.rodata` section containing every admitted declaration's exact packed payload bytes;
- canonical local `$function_NNNN` symbols for every non-main ordinal followed by one exported `Main` function symbol, plus local `$data_NNNN` symbols in declaration order, each spanning its exact range; and
- one ordered `Relative_i32` relocation with addend `-4` for each i32-array load or static text/bytes descriptor address, and no platform imports.

The selector assigns one deterministic 16-byte ABI cell to each parameter or local. It reuses separately typed `i32`, `bool`, `text`, `u8`, `u32`/`u64`, `bytes`, enum, and record-handle value cells at empty-stack block boundaries in that canonical order. A `u64` value occupies the complete low eight bytes of its scalar cell; it is loaded, stored, passed, returned, and copied as one 64-bit quantity, including fifth-and-later stack arguments and record fields. Enum analysis retains the nominal type index while its runtime cell contains the signed 32-bit backing value. For a record-bearing function, a bounded fixed-point liveness pass derives persistent record-local interference across validated successors; a separate last-use pass requires scratch record values to remain inside their defining block. Scratch identities are unique across the function, but physical scratch-field allocation is independent per block and a compact block-offset table maps the global identities to those block-local ranges. Both persistent and scratch plans assign deterministic field ranges by descending width and first-fit placement. A record backing begins with one 16-byte cell per direct field, followed depth-first by the flattened backing of each nested record field in declaration order. The direct cell for a nested field is reserved and zero; `record.field` returns a typed view into its child backing. Construction, local movement, and return copy the complete cached flattened width, so no callee-frame or source-backing pointer escapes. Record parameters retain their incoming handle, while record call results use caller-owned scratch backing. `Main` establishes the shared execution context at its exported ordinal; every helper reuses its `R11` instruction budget, `R10` call-depth budget, and `R15` context. A byte-returning `Main` receives its caller-owned descriptor result cell in `RCX` and the execution context in `RDX`, saves the result cell before frame initialization, publishes the returned pointer and length directly with a zero reserved word because its caller retains the entry arena, and restores `R15` on return. Every function entry charges depth and every normal, propagated, or trap exit restores it. `process.argument_count` loads the service table through that `R15` context, calls ABI 22's slot at byte offset 16, and stores the returned `EAX` value in the selected `u32` cell. `process.argument` loads its `u32` index into `R8D`, places the destination borrowed-text descriptor in `R9`, calls slot 24, and branches on failure to the existing runtime-service tail. `file.read_bytes` passes its borrowed-text pointer and length in `R8` and `R9D`, places the destination borrowed-bytes descriptor in `RCX`, calls slot 32, and uses the same failure tail; the service-owned immutable snapshot remains borrowed for the execution lifetime. Before an ordinary function call, scalar arguments are loaded in source order into ABI 22's `R8D`, `R9D`, `ECX`, and `EDX` positions, widened `u64` arguments use the corresponding 64-bit registers, and record handles use their 64-bit register counterparts. A descriptor argument instead places the address of its complete caller-owned 16-byte cell in the corresponding 64-bit register; helper entry copies both address and length into its first ordinary local cells. Fifth-through-64th arguments use one 16-byte outgoing stack cell each after the caller reserves `(parameters - 4) * 16` bytes: narrow scalars copy four bytes, `u64` and record values copy eight bytes, and descriptors copy both eight-byte words. After allocating its own frame, the callee reads those cells beyond its return address and copies the same representations into ordinary parameter cells. Caller-owned descriptor and record result addresses include the temporary outgoing-area adjustment; the caller restores that area immediately after return and before status propagation. A descriptor-returning callee saves its hidden result pointer immediately after ordinary local and value cells, before persistent and scratch record backing, and preserves valid borrowed results while validating and compacting arena-owned results before publication. A record-returning callee saves its hidden result pointer after scratch backing, copies every returned field cell into the caller-owned range, and preserves the required arena checkpoint. A first bounded pass collects every signature, a second measures every function against that complete set and fixes a 76-byte-per-function directory containing machine offset and length, one-byte parameter count, one-byte encoded return type, two reserved zero bytes, and 64 padded encoded parameter types, and a third emits code. Every direct call reads its target's complete signature and offset from that directory, tests the packed high status word, propagates failure before another WVB instruction, and stores the successful scalar, descriptor, or caller-owned record result in the declared type's value group. Static i32-data loads use the exact unsigned bounds branch to ABI 22's data-bounds status. Static text/bytes constants construct a borrowed 16-byte address/length descriptor; descriptor local movement and `text.to_utf8` copy the complete descriptor, while slices and little-endian reads branch to the distinct byte-bounds status. Text concatenation, UTF-8 validation, conversion from UTF-8, and quoting call the existing ABI 22 service-table entries and branch to the exact runtime-service or invalid-UTF-8 status tails. Byte concatenation checks the 4 MiB value ceiling, validates and reuses a generation-owned left buffer only when its arena/header/tail invariants hold, otherwise makes a bounded exact or owned allocation, copies both inputs, and branches to the existing runtime-service status with exact value-limit or arena-exhaustion detail. `bytes.from_u8` and `bytes.from_i32_little` respectively check exact one- and four-byte arena growth, publish complete owned descriptors of those lengths, store the source scalar at the new allocation in its required width and byte order, and use the same arena-exhaustion detail. Enum constants store their declared backing values, comparisons require exact nominal identity, and `enum.name` calls the existing ABI 22 nominal-metadata service before producing text. Every static address starts as a zeroed RIP-relative field represented by canonical WVO relocation. The selector otherwise charges every WVB instruction, selects checked scalar and comparison operations, patches signed forward or backward control displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

`bytes.from_u64_little` checks exact eight-byte arena growth, publishes one complete owned descriptor, stores the complete source scalar in little-endian order, and uses the existing arena-exhaustion detail.

The admitted `u64` arithmetic is exact and checked: addition and subtraction branch on carry or borrow, multiplication rejects a nonzero high half, and division/remainder reject zero divisors. Unsigned comparisons use the complete 64-bit value. `Bytesˉreadˉu64ˉlittle` performs the existing exact eight-byte bounds proof before one little-endian load, `Bytesˉfromˉu64ˉlittle` performs the symmetric exact store, and `U64ˉfromˉu32` zero-extends losslessly. This slice deliberately does not yet admit `i64`, `u64` formatting, or `u64` bitwise and shift operations.

Persistent and scratch record interference use one packed bit per ordered pair. The packed representation preserves Stage 0's deterministic descending-width, first-fit allocation while avoiding a dense byte-per-pair compiler-scale envelope.

`bytes.from_u32_little` shares the signed constructor's exact four-byte allocation and store sequence while retaining a distinct `u32` typed-stack requirement. The stored bit pattern is not converted or sign-extended.

`bytes.from_u16_little` checks exact two-byte arena growth, rejects inputs above 65,535 through `WVR3016`, publishes a complete owned descriptor, and stores the accepted low word in little-endian order.

`u32.from_u8` copies the canonical 32-bit contents of its `u8` value cell into a newly selected `u32` cell. The widening is lossless and adds no service call or failure branch.

Checked `u32.add` and `u32.subtract` emit their ordinary 32-bit operation and branch to `WVR3007` when the carry flag reports overflow or borrow. Checked `u32.multiply` emits the full `EDX:EAX` product, tests the high word, and reaches the same overflow tail when it is nonzero. A successful operation stores the low 32-bit result only after that proof.

`i32.format` and `u32.format` load the source's unchanged 32-bit representation into `R8D`, place the destination text-descriptor address in `R9`, call service-table slot 80 or 88 respectively, and branch to the shared runtime-service failure tail. Signed interpretation belongs to the slot-80 service leaf; lowering does not alter the two's-complement bits. The returned descriptor follows the same bounded owned-text lifetime as the existing dynamic text services.

`file.write_bytes` passes its borrowed-text pointer and length in `R8` and `R9D`, passes its borrowed-bytes pointer and length in `RCX` and `EDX`, calls service-table slot 96, and branches to the runtime-service failure tail without producing a value. Success retains the hosted service's whole-value replacement and durable-flush contract; the lowering adds no atomicity, rollback, or retry promise.

`console.write_line` passes its borrowed-text pointer and length in `R8` and `R9D`, calls service-table slot 8, and branches to the runtime-service failure tail without producing a value. The service writes the exact UTF-8 text followed by one LF. Output can be partially visible before failure; the lowering adds no buffering, transaction, rollback, or retry promise.

`diagnostic.write_line` uses the same verified borrowed-text input sequence, calls service-table slot 48, and writes exact UTF-8 text plus one LF to the separate diagnostic channel. The two calls share one parameterized emitter but retain distinct capability identities, services, grants, sinks, and failure paths.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

### Segmentable object regions

The focused object writer also exposes a validated region plan without
changing WVO 1.0. It owns the header-plus-text-section prefix, optional
read-only-section header, canonical symbol records, canonical relocation
records, and exact final length. Machine code, zero-through-fifteen `0x90`
alignment bytes, and immutable data remain separate spans. Concatenating
prefix, code, padding, read-only header, data, symbols, and relocations in that
order reproduces the ordinary emitter byte for byte.

Planning validates the directory, data layout, relocation ordering and ranges,
padding, counts, and projected length without requiring one complete code
value. The ordinary emitter additionally proves that every relocation field in
its complete code value is the required zero placeholder. A future segmented
producer must make the same proof within its owned function chunks before
publication. This boundary does not itself authorize a large-native profile,
publish a file, widen `bytes`, or change `file.write_bytes` semantics.

### Bounded function artifacts

The lowering core exposes one immutable analysis plan after WVB structure,
module/profile, capability, data, type, function, export, code, and aggregate
output validation succeeds. The plan owns the canonical 76-byte function
directory, exact function-record cursors, per-function relocation byte counts,
total machine-code and relocation lengths, and the already validated immutable
data, type, and capability tables. It is phase evidence produced by the core,
not a serialized input format or a substitute for validating the original WVB.

Given that plan, a function-batch request names the first function ordinal and
an artifact ceiling from 16 bytes through the ordinary 4 MiB value limit. The
core selects the largest non-empty contiguous function range whose exact
`WVFA 1` header, relocation entries, and machine code fit that ceiling. It then
uses the existing balanced range emitter, requires the emitted relocation and
code lengths to equal the plan, and returns the exclusive next-function
ordinal. A function that cannot fit by itself fails with `Outputˉlimit`; an
invalid emitted artifact fails closed. Traversing batches in ordinal order
therefore retains canonical code order, canonical relocation order, exact
machine offsets, and bounded complete values without re-running module
analysis for every batch.

The bounded successor entry additionally accepts a maximum function count and
a maximum grouped-function payload. The function count must be from one
through the admitted layout limit, and the grouped payload must be nonzero and
fit beneath the artifact ceiling after the 16-byte `WVFA 1` header. An
individual function larger than the grouping threshold occupies one batch by
itself when it still fits the artifact ceiling; later functions are not joined
to it. The publication cursor uses at most 16 functions and 64 KiB of grouped
function payload, clamped to its current artifact payload ceiling. The legacy
batch entry delegates with the complete function limit and complete artifact
payload, preserving its exact bytes and behavior.

The ordinary complete-object entry currently requests one 4 MiB batch and
requires it to contain every function, preserving exact ordinary WVO bytes.
It does not join multiple batches.

The private record-storage evidence is version 2. Its validated header owns
persistent-field count, maximum per-block scratch-field count, global scratch
value count, and exact byte lengths for persistent offsets, scratch offsets,
and block offsets. It is phase evidence only and is not a distributable
format.

### Bounded object-publication cursor

The separate capability-free publication module consumes the immutable
lowering plan and the same bounded function batches. It first traverses every
batch to validate progress and artifact lengths, prove every relocation field
is a zero placeholder inside the code chunk that owns it, retain canonical
relocation order, and construct the segmentable WVO region plan without one
complete code value. The projected object remains bounded to 32 MiB.

An immutable cursor then yields one exact `(position, bytes)` step in canonical
WVO order: prefix, one or more code batches, alignment padding, optional
read-only header, immutable data, symbols, and relocation records. Every code
position is derived from the plan's canonical function offset. The next cursor
contains the exclusive next-function ordinal and exact next position; a
changed, skipped, repeated, or out-of-range cursor fails closed. Completion is
valid only at the planned final object length. Concatenating the yielded values
therefore reproduces the ordinary WVO byte for byte while no yielded value
exceeds the requested function-artifact ceiling.

The plan, regions, and cursor are immutable in-process compiler evidence, not
a serialized input format or a capability grant. This module does not create,
resize, write, flush, rename, replace, or delete a host resource. A later
versioned hosted owner must preserve exact positions, distinguish rejection,
partial progress, and indeterminate mutation, durably finish a unique sibling,
replace the requested path atomically, and clean up every prepublication
failure. It must not silently retry an indeterminate write.

### Versioned object staging

`Compiler/Windvale/Native-X64-Lowering-Staging-Tool.wv` carries cursor values
across the existing hosted file boundary without widening `bytes` or adding a
new source capability:

```text
wvnative-stage <input.wvb> <chunk-prefix> <manifest.wvop>
```

The tool requests the ordinary 4 MiB publication ceiling, omits zero-length
cursor steps, and writes each remaining value exactly once to
`<chunk-prefix>.chunk-<decimal-index>`. The prefix is nonempty and at most
4,078 UTF-8 bytes, so the longest `u32` index suffix remains below the hosted
4,096-byte argument limit. A generated chunk resource must not equal the input
or manifest resource. Chunk index zero is the first nonempty cursor value;
indices increase by one and their recorded WVO positions are contiguous.

Only after every chunk write returns does the tool write the little-endian
`WVOP 1.0` manifest:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVOP` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Total manifest bytes, exactly `24 + chunk-count * 12` |
| 12 | 4 | Final WVO bytes |
| 16 | 4 | Chunk count |
| 20 | 4 | Maximum chunk bytes, `4,194,304` |

Each following 12-byte entry contains the `u32` chunk index, `u32` WVO
position, and `u32` chunk length. The entry at ordinal `N` names
`<chunk-prefix>.chunk-N`. The manifest is a structural staging marker, not a
digest, capability, durable-commit record, or claim that the destination WVO
is atomically visible.

The focused capability-free staging-manifest module owns canonical
serialization and validation. A valid manifest has exactly `24 + chunks * 12`
bytes, one through 518 chunks, a nonzero WVO no larger than 32 MiB, and the
exact 4 MiB ceiling. Indices must equal their entry ordinals; the first
position is zero; every nonzero length is within the ceiling and remaining WVO
extent; positions are contiguous; and the final position equals the declared
object length. Counts are bounded before multiplication and every rejection
returns zeroed size/count evidence with a named status.

The staging shell uses the same module to build the manifest and validates the
canonical result before its manifest-last write. The portable reader rejects
truncated, oversized, inconsistent, reordered, duplicate, gapped, extended,
and otherwise malformed evidence without host mutation. It does not open or
identify chunk resources.

A focused native bridge accepts the borrowed manifest descriptor through ABI
22 and maps the reader status to one `u32`: `0` is valid; `1` through `10` are
truncated, magic, version, manifest size, object size, chunk count, chunk
limit, chunk index, chunk position, and chunk length. The packed native return
continues to carry runtime failure separately from this low-word format token.
The bridge requires no service and exposes no admitted size or count on
failure. A fixed package builder must resolve its verified function ordinal
before mapping the generated `$function_NNNN` symbol to a platform import.

The same bridge exposes validated scalar queries for object length, chunk
count, and each chunk's position and length. Every query reruns strict
validation over the adapter-owned immutable snapshot. Invalid object length,
count, or chunk length is zero; invalid or out-of-range chunk position is
`0xffffffff`. Those sentinels cannot represent valid `WVOP 1` values. The
index is bounded by the admitted chunk count before its entry offset is
calculated. Fixed platform adapters therefore do not parse header or entry
fields themselves; the bounded repeated scan covers at most 518 chunks.

The focused large-native section-envelope reader then consumes that same
manifest snapshot plus the actual bounded metadata chunks. Chunk zero must be
the exact 49-byte WVO 1.0 x86-64 header and canonical `.text` declaration. The
compiler-output profile has one or two sections; an optional `.rodata`
declaration is an exact 27-byte chunk at the computed end of text. Counts,
flags, alignment, equal data/memory extents, the combined 32 MiB section-data
ceiling, following manifest boundaries, and the minimum declared symbol and
relocation tail are checked without loading the intervening section data into
one value. Valid evidence exposes object length, counts, both section lengths,
and the exact symbol position; rejected evidence exposes only a named status
and zeroes. Its ABI-22 scalar bridge reruns the same validation for each query.
This is the compiler-produced section envelope, not yet symbol, relocation,
placeholder, or complete staged-content verification.

Decision 0290's focused reader consumes the actual symbol chunk at that admitted
position. It accepts only the native compiler's bounded symbol profile:
sequential local `$data_NNNN` records cover `.rodata`; ascending local
`$function_NNNN` records describe every non-main ordinal; and one final
exported `Main` fills the one omitted ordinal and exact code-range gap. Reserved
fields, bindings, kinds, section indices, names, ranges, data coverage, function
coverage, and the optional zero-through-15-byte text padding are checked. The
complete chunk must be consumed, with at most 256 data symbols, 512 functions,
and 768 symbols total. The combined bound is the sum of the already admitted
data and function inventories; it does not widen either component limit. Its
end is the exact relocation position. A nonempty
relocation table must be one following manifest chunk of `count * 20` bytes
ending at the object extent; with no relocations, the symbol chunk ends the
object. Valid evidence exposes data/function counts and the relocation extent;
rejection exposes named status and zeroes. The scalar ABI bridge reruns the
same validation over the same immutable snapshots for each query. Relocation
semantics and code-placeholder validation remain separate.

Decision 0291's following reader consumes the actual relocation chunk and
accepts only the native compiler's canonical 20-byte `Relative_i32` records:
zero flags/reserved fields, section zero, addend `-4`, ascending nonoverlapping
four-byte code ranges, and targets limited to the admitted data symbols. It
derives exact code length from the validated function ranges and requires any
optional text padding to begin at one manifest boundary. Valid summary evidence
exposes code bytes, text-chunk count, and relocation count; rejection exposes
named status and zeroes. A per-text-chunk call binds the actual bounded chunk to
its manifest entry, rejects code/padding crossings, requires every owned
relocation placeholder to contain four zero bytes, and requires the separate
padding chunk to contain only `0x90`. The scalar ABI bridge reruns the same
checks over borrowed snapshots. The fixed adapter must call this validator for
every admitted text chunk.

Decision 0293's content cursor then binds every actual nonempty staged chunk to
both the strict manifest entry and the retained publication cursor. The caller
constructs and preserves one typed lowering plan and publication-region plan.
`content.begin` captures the admitted manifest count and initial publication
position; each `content.next` skips only canonical zero-length publication
regions, requires the exact following position and length, compares every byte
of the actual value with the publication value, and advances only on equality.
`content.finish` requires all manifest entries to be consumed and publication
to reach `Complete` at the admitted object length. The flat scalar cursor has
nine explicit active, complete, and rejection states. It is not a serialized
host token, and the operation never constructs one complete WVO or widens the
ordinary 4 MiB value contract. Staged resource identity remains separate
publication evidence.

Decision 0295's resource plan fixes the execution-local acquisition contract.
Input WVB and manifest occupy snapshot ordinals zero and one; at most 62
canonical `<prefix>.chunk-<decimal-index>` resources occupy ordinals two
through 63. Control names are nonempty, bounded to 4,095 UTF-8 bytes, exact-name
distinct, and separated from every derived chunk name. The prefix is at most
4,078 bytes so the longest `u32` suffix remains within the same fixed adapter
buffer. A hosted admission root preflights those names, reads input and manifest
once, reads each chunk once in index order, and passes the same borrowed values
through the complete content cursor without creating the destination. A fixed
platform adapter may consume those retained snapshot descriptors only after it
independently verifies the native table; ordinal names do not prove host file
identity or eliminate alias checks.

Decision 0299 adds that fixed commit adapter for Windows and Linux. It validates
the exact native `WVFI` table and every canonical pointer, reopens each retained
resource only to bind host identity and compare its complete bytes with the
immutable snapshot, rejects destination aliases, and writes only snapshot
ordinals two through the admitted count. One exclusive sibling is written with
exact partial-progress accounting, durably flushed, reread against the same
snapshots through exact EOF, and atomically renamed. Pre-replacement failure
cleans the sibling; post-replacement failure remains distinct and is never
silently replayed. The fixed package uses the 64-snapshot compiler-capacity
storage plan while binding only the exact eight service-table entries required
by the Windvale admission module.

Decision 0389 extracts the format-neutral part of that snapshot admission into
one focused x64 object. It validates the exact `WVFI 1` table, arena geometry,
record pointers and bounds, then selects immutable payload records by a bounded
first ordinal, stride of one or two, and fixed header skip with checked aggregate
accounting. The staged-WVO wrapper retains ordinals two onward with no skip and
the 32 MiB ceiling. The hosted-container wrapper selects alternating response
ordinals three onward, skips each 40-byte `WVHU 1` envelope, requires an even
snapshot count, and bounds at most 31 canonical segment payloads. The WVO path
has current-host execution evidence; hosted execution remains part of its
publisher connection slice.

Decision 0390 separates the Linux durable mutation protocol from that WVO
adapter. The format-neutral function consumes an admitted snapshot ordinal
range, stride, fixed header skip, and exact final mode; it exclusively creates
an anchored sibling with that mode, handles exact partial writes, flushes and
rereads through exact EOF, performs same-directory replacement and directory
flush, cleans pre-replacement failure, and preserves the distinct indeterminate
outcome after replacement. Decision 0422 admits only `0600` and `0755`. The WVO
adapter supplies `(2, count, 1, 0, 0600)`; the hosted-container adapter supplies
`(3, count, 2, 40, 0755)`. Mode is therefore complete before atomic replacement
rather than repaired afterward by a host script.

Decision 0391 establishes the matching Windows boundary. The shared Windows
function exclusively creates a full-path sibling, anchors it without replacement
below the verified directory handle, performs exact partial writes and
flush/reread comparison, replaces through handle-relative `FILE_RENAME_INFO`,
flushes the renamed handle, cleans pre-replacement failure, and preserves the
indeterminate outcome. WVO supplies `(2, count, 1, 0)`; hosted containers will
supply `(3, count, 2, 40)`. The extracted Windows path has real current-host
success, corruption, alias, preservation, and cleanup evidence.

Decision 0392 separates the remaining platform acquisition shell from WVO
policy. Each Windows/Linux shell receives the selected first ordinal, stride,
header skip, and exact validator address; it owns process arguments, hosted
runtime storage, resource reopening and complete byte comparison, native file
identity, destination-alias rejection, and invocation of the reusable durable
transaction. A thin tail-jump WVO wrapper selects `(2, 1, 0)` and the WVO
validator. A thin hosted-container wrapper selects `(3, 2, 40)` and the hosted
validator. Policy wrappers must preserve the original platform entry stack and
alignment, and no shell may infer format policy from a filename or payload.

Decision 0393 connects the hosted policy, shared shell, validator, and durable
transaction as paired Windows/Linux publisher applications. A shared 433-byte
WVA object now owns the narrow publication-token ABI formerly supplied by
private WVB-fragment bridge functions. The public `compile` and `aot` targets
construct exact packages; current-host Windows execution proves admitted
multi-segment publication, content and alias rejection, preservation, and zero
scratch without loading .NET.

Decision 0394 removes the superseded private WVB publication bridge from the
staged admission source closure. The admission project now contains 26 modules,
431,568 WVB bytes, and no publication-transaction dependency. Its paired
packages continue to resolve `Native_publication_begin` and
`Native_publication_apply` only from the shared WVA object.

The Windows WVO and hosted-container applications have current-host execution
evidence. The Linux images,
complete compiler self-staging integration, package promotion, native
replacement of the Stage 0 constructor, extended fault/concurrency matrix, and
grouped dual-host qualification remain separate gates.

Decision 0300 packages the existing staging producer as exact Windows and Linux
applications. It preserves the module's six-capability and ten-service hosted
profile and emits the same bounded chunk sequence plus strict `WVOP 1` manifest.
The current-host producer and fixed publisher compose as separate native
processes on the canonical return-42 fixture without loading .NET. Full compiler
self-lowering, Linux process composition, ordinary launcher ownership, native
replacement of both Stage 0 constructors, promotion, and grouped qualification
remain deferred.

The Windows native source front door and retained segmented toolset reconstruct
the current lowerer generation and paired applications. The accepted subset
including checked `u64` and nested immutable records now builds as a
457,041-byte WVB at SHA-256
`15a91a965860c4a36ae114651e87b82e5cd31869f4852040bb428f19f9d0382a`.
The same native segmented path reconstructs its 6,498,816-byte Windows and
6,500,352-byte Linux applications exactly after rebuilding and repinning the
embedded staging producer. The fixed Return-42 WVB/WVO pair remains unchanged.
The earlier Decision 0497 identities remain historical evidence rather than
identities for the current source generation. This current-host reconstruction
is not complete-backend transfer, a non-circular bootstrap, independent Linux
reconstruction or execution, qualification, or promotion.

## Adapters

`Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Compiler/Windvale/Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB. The paired `WVHN 1` profile packages the hosted shell as a direct Windows/Linux candidate without adding platform assembly.

## Conformance and limits

`Wvb-To-Wvo-File-Read-Bytes.wv` obtains a temporary path through `process.argument`, reads one `A` byte through `file.read_bytes`, and returns 42. It agrees across the reference interpreter, Stage 0 native execution, both Windvale adapters, and the direct native package while retaining exact object bytes.

`Wvb-To-Wvo-File-Write-Bytes.wv` obtains a temporary output path through `process.argument`, writes one immutable `A` byte through `file.write_bytes`, and returns 42. The reference host captures the exact request, the real current-host native service durably creates the expected file, and both Windvale adapters plus the direct package retain Stage 0's exact object bytes.

`Wvb-To-Wvo-Console-Write-Line.wv` writes one immutable `A` line and returns 42. The reference interpreter and Stage 0 native execution both produce exact `A` plus LF output, while both Windvale adapters and the direct native package retain Stage 0's exact object bytes. Existing output-service tests retain rejected and partial-sink failure coverage.

`Wvb-To-Wvo-Diagnostic-Write-Line.wv` writes one immutable `A` diagnostic line and returns 42. The reference interpreter and Stage 0 native execution both produce exact `A` plus LF on the diagnostic channel, while both Windvale adapters and the direct native package retain Stage 0's exact object bytes. Existing output-service tests retain separate-channel, Unicode, empty-line, authorization, rejection, and partial-sink coverage.

`Wvb-To-Wvo-Large-Envelope.wv` declares nine immutable data items, nine nominal types, and ten functions. Its WVO contains canonical `$data_0008` and `$function_0008` symbols and agrees byte for byte through Stage 0, the Windvale memory adapter, and the hosted Windvale tool. The explicit 256-data, 64-record plus 64-enum, and 512-function limits cover the current compiler seed's measured 110 data declarations, 63 records plus 16 enums, and 413 functions without relaxing any per-entry or instruction boundary.

`Wvb-To-Wvo-Descriptor-Calls.wv` passes and returns borrowed `text`, borrowed `bytes` slices, and arena-owned concatenation results, retains a returned value across later allocation, and returns 42. Its six-parameter helper mixes descriptor and scalar register positions, then carries an `i32` and a complete `bytes` descriptor through the fifth and sixth stack cells. The reference interpreter and Stage 0 native execution agree, while the memory adapter and hosted Windvale tool reproduce Stage 0's complete WVO byte for byte.

`Wvb-To-Wvo-Static-Descriptors.wv` returns 42 through one text, one bytes value, one i32 array, descriptor locals, UTF-8 conversion, length, slice, and all four admitted reads. The memory adapter, hosted tool, and generated native tool reproduce Stage 0's exact 7,626-byte WVO with two sections, three data symbols, and three relocations. `Wvb-To-Wvo-Text-Services.wv` additionally formats `4294967295u32` as its exact ten-byte decimal text and reproduces Stage 0's exact 6,388-byte WVO with 6,160 code bytes. Compiler-produced `Data-And-Text.wv` composes those operations with generic `bytes.concat`, returns 13 across interpretation and native execution, and reproduces Stage 0's exact 15,123-byte WVO at SHA-256 `cc987e81e8f8dfb8d19b13e91e5f259c86b3f5eb8a6ae79db66ed6ef3dca4263`. `Wvb-To-Wvo-Enums.wv` returns 42 through enum local movement, both comparison directions, `enum.name`, UTF-8 conversion, and a byte read; the memory adapter and hosted tool reproduce Stage 0's complete WVO byte for byte. `Wvb-To-Wvo-Records.wv` returns 42 through direct construction of a two-field `Pair`, record local storage and reload, and both field reads. `Wvb-To-Wvo-Record-Calls.wv` declares an enum whose first member is 2, passes a `Pair` through `Keep(Value: Pair) -> Pair`, reads the caller-owned result, and returns 42. Compiler-produced `Nominal-Types.wv` carries descriptor-bearing records through multiple control blocks, passes `Reading` through both record- and scalar-returning helpers, and returns 11. Its exact 22,404-byte WVO has SHA-256 `460695af54b5cd4f7d4597f9bc60a17e29e236ddacc0330b1f541ab455759085`; the memory adapter, hosted source tool, and direct current-host native package reproduce Stage 0 byte for byte. `Wvb-To-Wvo-Process-Argument-Count.wv` declares the hosted scalar capability and returns 42 with an empty argument vector. `Wvb-To-Wvo-Process-Argument.wv` obtains one borrowed argument, converts it to bytes, validates `A`, and returns 42. Both process-input cases agree through interpretation, Stage 0 execution, the Windvale adapters, and the direct current-host package. Additional malformed cases reject an out-of-range descriptor data index, a text constant naming bytes data, a runtime text operation receiving the wrong descriptor type, a bytes concatenation mutated to require text inputs, out-of-range enum and record type indices, an out-of-range capability index, and a mutated capability return signature before publication.

`Wvb-To-Wvo-Byte-Construction.wv` constructs `255u8`, `-7`, `2309737967u32`, and the maximum narrowing input `65535u32`; requires exact one-, two-, and four-byte results; reads each value back through its typed little-endian operation; and returns 42 without a runtime service. Its 1,107-byte WVB has SHA-256 `aa3736cee76c6aaf7e19e7eb36c715ce1614d8b007db03da7229b253784f305f`; the exact 8,657-byte WVO contains 8,584 code bytes and has SHA-256 `973a428de734f5414c33c1f5f91dd3b3943110ea747eb7565b34f269382802f4`. The exact object includes the `WVR3016` branch for inputs above 65,535. Both Windvale adapters reproduce Stage 0 byte for byte.

`Wvb-To-Wvo-U32-Arithmetic.wv` forces checked add, subtract, and multiply over high unsigned values and returns 42. Its 471-byte WVB has SHA-256 `7ce9f43e05a16be16c5df444e4dc5ed80f0c206a6816cafa9a91466ebf1fa6fb`; the exact 3,362-byte WVO contains 3,255 code bytes and has SHA-256 `d342e76095e397686defcddaf68a122517fba1d54606cbc738165eda0cb71591`. Both Windvale adapters reproduce Stage 0 byte for byte. A separate `65536u32 * 65536u32` case traps as `WVR3007` under the reference runtime and Stage 0 native execution before both adapters reproduce that exact overflow object.

`Wvb-To-Wvo-U32-Conversion.wv` converts both `255u8` and `0u8` through a helper and returns 42. Its 420-byte WVB has SHA-256 `981a9a104e69bea9f0ef808f9107e3c0dc06da40b8b595140152d056bcbcc782`; the exact 2,546-byte WVO contains 2,439 code bytes and has SHA-256 `2ae4cfff8e8de357177710238a9fea7d2f33ec8049d1d66ee009c5debf5bc5cc`. Both Windvale adapters reproduce Stage 0 byte for byte.

`Wvb-To-Wvo-U64.wv` exercises the accepted wide-scalar boundary with the maximum `u64` literal and exact little-endian read, checked addition, subtraction, multiplication, division, `u32` widening, unsigned comparisons, local movement, a `u64` record field, a `u64` helper return, and a fifth stack argument. Its exact 1,024-byte WVB has SHA-256 `ced899022e388413714456621f029974fa6e4db5361ff6348b6f127f97316bf5`. The Windvale-native lowerer emits the exact 8,032-byte WVO at SHA-256 `0807b47f84505a256cdb1a542df2ccab1bce87b07424cdeaeae895e2b86d2459`; its 7,848-byte linked image runs to result 42 on the current host and packages byte-identically for the other permanent host. The focused `native-u64-lowering` suite reconstructs the lowerer from current Windvale source before asserting that evidence, so it does not rely on a previously retained lowerer that lacks the new subset.

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, checked arithmetic, nested expressions, all admitted scalar shapes and comparisons, boolean negation, both conditional routes, forward and backward jumps, loops, early returns, bounded calls, mixed scalar arguments in all four register positions, mixed stack arguments after position four, typed scalar and descriptor returns, general call order, recursion, and canonical static data. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The metered loop oracle is exactly 1,665 code bytes and a 1,738-byte WVO; native execution succeeds at its exact 157-instruction requirement and reaches `WVR3011` at 156. The parameterless call oracle is exactly 795 code bytes and a 902-byte WVO, with exact shared instruction and two-entry depth boundaries retained by Stage 0 execution. The four-parameter mixed-scalar oracle is exactly 2,581 code bytes and a 2,688-byte WVO. The existing three-function `Add -> Build -> Main` fixture additionally calls both lower ordinals through control flow and returns 42. A verifier-approved `Alpha, Main, Zeta` oracle combines a real forward edge with same-signature forward/back mutual recursion; it returns 42 at exact call depth five, reaches `WVR3004` at four, and produces Stage 0's exact 4,350-byte `.text` and 4,491-byte WVO. Canonical 493-byte `Sum-Data.wv` combines one `[i32]` declaration, `data.length`, a bounds-checked load, backward control, and a scalar call; it produces the exact 3,088-byte `.text`, 16-byte `.rodata`, one relocation, and 3,288-byte WVO emitted by Stage 0. Canonical compiler-produced `Function-Only.wv` returns 6 through `u32` loop state and checked addition, a `u8` argument/comparison, and a Boolean helper result; the memory adapter, hosted tool, and generated native tool reproduce Stage 0's exact 6,041-byte `.text` and 6,216-byte WVO. A small source-defined `Byte() -> u8` and `Count() -> u32` vector returns 42 while exercising every `u32` and `u8` comparison across true and false routes; it reproduces Stage 0's exact 5,263-byte `.text` and 5,404-byte WVO through the hosted lowerer. The same test executes every retained input through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, invalid-data, invalid-branch, unreachable control, out-of-range call target, and mismatched-parameter-type streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The mixed-scalar call code and WVO SHA-256 identities remain `1a0a541d2bd59378b4fa6df53248c3c359e909a0b7446198ebb1a58ca5a79721` and `cb7d2c74edb7aa3443e1e23cf0d762d4c15b79c39ea4f363531b2ec80633c13f`. Existing retained oracle objects remain byte-identical. The current hosted tool is the exact 457,041-byte WVB at SHA-256 `15a91a965860c4a36ae114651e87b82e5cd31869f4852040bb428f19f9d0382a` and reproduces through the pinned native source front door.

This slice does not yet transfer mutable record parameters, dynamic byte builders beyond concatenation, `i64`, `u64` formatting/bitwise/shift/byte construction, the remaining scalar conversions and multi-byte construction instruction families, modules beyond the 512-function/64-data/64-type envelope, other hosted capability declarations and calls, broader nominal values, general relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. WVO 1.0 also does not serialize the required-service and nominal-type metadata needed to execute hosted calls or `enum.name` independently from the verified fragment. This slice does not satisfy the native-retirement gate by itself.
