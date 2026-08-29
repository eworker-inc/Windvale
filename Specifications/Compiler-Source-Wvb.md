# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes prepared validated source evidence, lowers the accepted `WVIR 1` subset to one complete canonical WVB 1.11 through 1.32 module, and returns the bytes without using hosted capabilities. `Compilerˉsourceˉwvbˉcompilation` separately owns direct source analysis and source-profile composition.

For the execution subset through WVB 1.30, including the current
Vector/Sequence, launcher-resource, and noncapturing-callable checkpoints, the implementation proves
the complete source set → symbols/bindings → typed WVIR → canonical
cross-module identity flattening → static data, nominal and capability metadata,
and code → WVB → verifier → source-built scalar runtime path. Every accepted
compilation emits one self-contained module and does not introduce runtime
linkage. Direct native lowering, browser packaging, and Windvale OS execution
retain their separately versioned subsets.

WVIR 1.19/1.20 and WVB 1.31 establish the prepared-evidence and portable-runtime
substrate for frame-owned plain captures. WVIR 1.21/1.22 and WVB 1.32 add the exact
structured-task lowering selected by
[Decision 0861](../Documents/Decisions/0861-Execute-Structured-Tasks-As-Wvb-1.32.md).
The task extension preserves async and effect evidence, affine scope/handle
ownership, exact generic outcome identities, and runtime bounds without
exposing a scheduler in source.

## Direct compilation result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

This direct entry point belongs to `Compilerˉsourceˉwvbˉcompilation`. The
prepared backend keeps the summary type and byte encoder so emitter-only source
sets do not carry source-profile or duplicate analysis dependencies.

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete canonical WVB module using the lowest required minor version, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved plus the one-based source line when the upstream WVIR boundary supplied it. Command-line and build-driver diagnostics print that location instead of leaving a long compiler phase silent about its source failure.

The status contract distinguishes upstream WVIR rejection, shapes and operations, invalid data, and WVB limits. The former unsupported-declaration value and the existing module-count and profile values remain reserved for stable diagnostic numbering: upstream WVIR admission now owns declaration-kind rejection, and every currently validated WVSS module count and root profile is accepted.

The backend body also has a prepared-evidence entry point for the internal
analysis/emission phase split:

```text
Compilerˉemitˉpreparedˉsourceˉwvb(
    Input, Scan, Symbols, Bindings, Wirˉsummary, Optimize
) -> Compilerˉsourceˉwvbˉsummary
```

The separate direct-compilation module prepares those exact values and delegates to
this function. Untrusted persisted evidence must enter through
`Compilerˉsourceˉemission`, which validates WVCA, WVLB, and WVIR before calling
the prepared backend. The prepared function is not an alternate semantic path
and does not by itself authorize callers to bypass validation. The artifact and
validation contract is specified in
[source-analysis phase artifact](Compiler-Source-Analysis.md).

## Language 1.0 source-profile admission

The edition-1 direct-compilation module has the separate portable entry point:

```text
Compilerˉcompileˉsourceˉwvbˉwithˉprofileˉinputs(
    Input: bytes,
    Lock: bytes,
    Expectedˉlockˉhash: bytes,
    Profile: bytes,
    Optimize: bool
) -> Compilerˉsourceˉwvbˉprofileˉcompilation
```

`Input` is external WVSS 1. The lock and selected `.wvsp` are exact immutable
bytes supplied by the build plan; the compiler performs no path lookup, registry
search, download, installation discovery, or locale selection. Admission:

1. parses each module's universal descriptor;
2. hashes and validates the lock against the externally pinned lowercase digest;
3. finds the descriptor's exact profile identity/version in the canonical lock;
4. hashes and validates the supplied composite profile against the locked digest;
5. checks its identity, version, source edition, component identities, component
   versions, and component hashes; and
6. resolves the currently implemented exact English component chain to the private
   English lexer binding.

The current implementation admits the frozen `en@1` composite chain only. A
well-formed but unavailable or unsupported chain fails explicitly; there is no
built-in or ambient `en@1` fallback. Admission hashes each supplied artifact once
and parses it once per compilation. The result separates profile-admission status
and failure offset from the ordinary compiler summary.

After successful admission the front door removes each descriptor and creates one
private in-memory WVSS version 2 view whose directory carries the already-resolved
edition, lexer binding, and descriptor-origin length. Existing graph, symbol, WIR,
optimization, and WVB phases consume that evidence without reopening or rehashing
the profile artifacts. WVSS 2 is an internal phase boundary, not a serializable
build input: the ordinary public compiler entry points require WVSS 1 and reject
external WVSS 2 values.

The first Language 1.0 Slice 2 checkpoint appended front-end token, source-type,
and internal binding-shape identities for `unit`, `never`, `i8`, `i16`, `u16`,
`f32`, `f64`, and `rune`; subsequent checkpoints carry their implemented forms
through explicit WVB versions rather than aliasing an existing shape. WVB 1.15
now maps internal shapes `9` and `10` to `unit` and `never` tags `20` and `21`.
`Unitˉconstant = 163` lowers to opcode `C3`, a unit-returning call pushes its one
value, and a typed return consumes it. `never` is return-only; a call emits no
result value and its source-WIR path has no normal continuation. The
fixed-integer checkpoint now admits `i8`, `i16`, and `u16`
as stored values and operations through WVB 1.12. The rune checkpoint admits
exact Unicode-scalar values and equality through WVB 1.13. The floating-point
checkpoint admits `f32` and `f64` stored values, exact hexadecimal literals,
arithmetic, unary negation, and comparison through WVB 1.14.

The multi-field-variant checkpoint admits zero through 64 named fields per
case. Cases with zero or one field retain their exact WVB 1.11 metadata bytes;
a case with two through 64 fields uses WVB 1.16 field-list marker `2`. WVIR
`Variantˉcreate = 65` continues to lower to opcode `97` with exact
declaration-order operands. WVIR `Variantˉfield = 164` lowers to WVB opcode
`C4`, carrying the nominal index and packed `case * 64 + field` identity. Either
multi-field metadata or a `C4` instruction selects version 1.16.

The 731-byte WVB 1.15 `Unit-Control.wv` artifact proves parameters, locals,
assignment, record storage, explicit/fallthrough returns, and a unit-returning
call. `Never-Control.wv` proves non-returning call propagation, a never-valued
loop condition, Boolean short-circuiting, and unconditional loops in an
853-byte WVB 1.15 artifact. Both execute through the
current scalar runner and return `42`.

The next Slice 2 checkpoint admits Language 1.0 named record update without a
format change. Typed WIR has already evaluated one exact-nominal base and every
replacement in source order, filled unreplaced declaration-order operands through
existing record-field operations, and emitted the existing record-construction
operation. The backend therefore sees only retained WVB 1.11 record instructions.
The deterministic 1,116-byte `Record-Update.wv` artifact executes through the
ordinary scalar runner and returns `42`; wrong-nominal bases, duplicate or unknown
replacement fields, and descriptorless Seed use are rejected without output.

Value-producing `match` adds no format change. Its selector, case tests, named
variant fields, branches, and pairwise value joins arrive as existing validated
WVIR operations. The three-arm `Value-Match.wv` therefore compiles to a
deterministic 588-byte WVB, selects its middle arm, performs two pairwise joins,
and executes with result `42`; the unselected recursive arm in the 431-byte
`Value-Match-Lazy.wv` has no runtime path. The 422-byte WVB 1.15
`Value-Match-Never.wv` admits a `never`-typed arm without inventing a value and
returns `42` through the source-built scalar runner. The 634-byte WVB 1.16
`Value-Match-Variant.wv` selects a brace-form two-field variant construction,
destructures both fields by name, and returns `42` through the source-built
scalar runner. Descriptorless Seed value match is rejected upstream and
publishes no WVB.

## Portable in-memory adapter

`Compiler/Windvale/Source-Wvb-Memory-Adapter.wv` is the capability-free execution adapter for hosts that already own immutable source bytes, including the browser playground. Its exported contract is:

```text
Main(Input: bytes) -> bytes
```

`Input` is one complete canonical WVSS 1 value. The adapter passes it unchanged to `Compilerˉcompileˉsourceˉwvb`; it does not resolve paths, enumerate files, read process arguments, write output, or print diagnostics. `Projects/Compiler/Windvale-Compiler-Memory.wvproj` builds the adapter with the same compiler-core source inventory as the hosted tool.

The hosted `Source-Wvb-Tool` selects
`Compilerˉcompileˉsourceˉwvbˉoptimized` for application publication. The portable
memory adapter retains the complete-emission entry point until its response-level
consumers gain equivalent optimized-output conformance evidence.

The hosted tool accepts an optional leading `--complete` argument before the root
source path. This differential and diagnostic mode selects complete emission from
the same executable; absence of the argument remains the optimized application
publication path.

Its edition-1 form is:

```text
source-wvb [--complete]
  --source-input-lock <inputs.wvlock> <sha256>
  --source-profile <profile.wvsp>
  <root.wv> [sorted-dependency.wv ...] <output.wvb>
```

It reads every source, the lock, and the profile once and passes their byte values
to the portable profile-aware entry point. Omitting the profile arguments is not
an English default: ordinary edition-1 input is rejected on that path.

The returned `WVCO 1` value has this exact little-endian layout:

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVCO` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Kind: `0` WVB success, `1` UTF-8 diagnostic |
| 12 | 4 | Payload length |
| 16 | `Payload length` | Exact payload bytes |

The payload must end exactly at the response length. Success carries the complete canonical WVB returned by the portable compiler. Failure carries strict UTF-8 text with stable `source-wvb status=`, `wir-status=`, `function=`, and `operation=` fields derived from the compiler summary. A consumer must reject an unknown version or kind, overflow, inconsistent length, malformed UTF-8 diagnostic, malformed WVB success payload, or trailing bytes before publication.

## Accepted subset

The backend accepts:

- one complete validated WVSS graph containing a `portable`, `hosted`, or `system` root plus as many as 63 portable dependencies;
- zero capabilities for a portable module or declarations from the complete current Seed capability catalog for hosted/system modules;
- private or exported functions, static data, storage-free constants, records, enums, and variants in any valid source declaration order;
- `[i32]`, `text`, and `bytes` static data;
- immutable nominal record, enum, and variant declarations, materialized general
  generic record and variant instances, the exact Foundation Option and Result
  generic variant families, plus bounded sequence and local affine-builder
  shapes;
- `void`, `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `bool`, `text`, `bytes`, record, enum, variant, and sequence function returns, parameters, explicitly typed or initializer-inferred locals, and temporaries, with builders restricted to verified locals;
- literal operations produced directly or by typed-constant substitution, parameter/local load and store, static-data length and integer-array indexing, and function calls;
- positional and named record construction through the same canonical operation, record field reads, enum constants, exact equality/inequality, and declared names;
- capability calls with their validated catalog parameter and result shapes;
- the implemented Foundation byte, text, formatting, conversion, and SHA-256 intrinsics, including exact little-endian `u64` read and construction plus lossless `u32` to `u64` widening;
- checked `i8`/`i16`/`i32`/`i64`/`u16`/`u32`/`u64` arithmetic including division and remainder; `u8`/`u16`/`u32`/`u64` bitwise and shift operations; exact text/bytes equality; full admitted scalar comparison, signed negation, invariant formatting, Boolean negation, short-circuit Boolean conjunction/disjunction, and mutable-local compound assignment; and
- variant and collection operations, semantic `using` cleanup, plus explicit jump, branch, and return terminators produced by `if`, `else if`, `else`, `match`, exact `try` propagation, `while`, `for`, `break`, and `continue`.

`try` is source-only control-flow sugar. WVIR presents only its existing variant
case test, field extraction, construction, branch, and return, so canonical WVB
gains no opcode, section, flag, or version change.

The root owns the emitted module name, profile, capabilities, static data, and exports. Dependencies follow the WVSS contract: imports, records, enums, and exported functions only. Their functions become internal WVB functions. Invalid graph topology, dependency order/profile/shape, unknown or repeated capabilities, and portable-profile capabilities remain upstream semantic failures rather than being silently omitted.

## Canonical identity translation

WVSD entries are source-declaration identities. WVIR preserves those identities for function calls and data references. WVB instead numbers its function and data sections in strict ordinal name order.

For specialized WVIR 1.10, ordinary reachable source functions retain that
ordinal-name rule. An all-zero generic declaration placeholder is not emitted.
Concrete specializations are appended to the function order in WVGC instance
order, use bounded private names `__Generic_000000` onward, and are never
exported merely because their source template is visible. Direct calls are
translated through the same immutable WVIR-entry-to-WVB-rank table used by
ordinary functions. The emitted function bodies and signatures are fully
concrete; WVGS, WVGC, and source generic parameters are absent from WVB. This
also applies when a specialized body used `Box<T>`: its private WVGT shape is
mapped through the materialization plan, so the emitted parameter, local,
return, record construction, and field read all name the one ordinary
`Box<Point>` Types entry.

The backend derives canonical function and data ranks from the independently validated global WVSD directory. Each directory entry names its owner module, so comparisons and declaration reads resolve the corresponding WVSS source first. It builds one immutable entry-to-rank and rank-to-entry table for each capability, data, record, enum, and function kind, then reuses those tables throughout emission. This removes repeated whole-directory ranking without changing any public packed format. It emits functions, code, exports, and data in canonical order and translates each WVIR target during emission. Source declaration order and owner module are therefore irrelevant to final indices.

Only root function exports are emitted, in canonical function order, and they target translated global WVB function indices. Dependency exports are internalized. Explicit and synthetic data share one canonical ordinal namespace.

WVSD constant entries participate in source validation and name lookup but are excluded from every WVB runtime section. A constant read has already become an ordinary typed literal or enum WVIR operation. Even an `export const` declaration creates no WVB export or storage identity in this root-only slice.

## Closed-world reachability evidence

`Compilerˉanalyzeˉsourceˉwvbˉreachability` provides deterministic, analysis-only
evidence for application-size and compiler-performance work. It does not change the
canonical output of `Compilerˉcompileˉsourceˉwvb`.

Every exported function owned by the WVSS root module is a reachability root. A
bounded work queue visits each reachable function at most once and follows only
validated direct `Callˉfunction` WVIR targets. This makes recursion and call cycles
finite without a repeated whole-program fixed-point scan. The result reports root,
reachable, unreachable, total-call, reachable-call, total-code-byte,
reachable-code-byte, and unreachable-code-byte counts. Its one-byte-per-WVSD-entry
map is indexed by the validated source symbol directory; function entries contain
one when reachable and zero otherwise.

Unreachable functions still pass source, symbol, binding, typed-WVIR, and WVB
operation/shape analysis. An invalid or unsupported body therefore cannot become
acceptable merely because no export calls it. Any pruning mode must preserve
that validation boundary, remap retained call targets and referenced data through
explicit canonical tables, preserve every root export, and prove reproducible bytes
and equivalent retained behavior before it can replace complete emission.

`Compilerˉcompileˉsourceˉwvbˉoptimized` is the first explicit pruning mode. It
keeps `Compilerˉcompileˉsourceˉwvb` byte-for-byte unchanged while the optimized
contract is qualified. The optimized path builds a canonical retained-function
order from the reachability map, remaps every retained direct call through that
order, and emits every root export at its retained rank. Unreachable functions
still undergo operation, shape, temporary-slot, parameter, local, return, and
metadata validation, but contribute no Functions metadata or Code bytes.

One text-planning traversal decodes and validates literals over the complete source
closure while retaining synthetic values only for reachable functions. The
reachable-function scan retains only explicit data targeted by `Bytesˉconstant`,
`Dataˉlength`, `Dataˉloadˉi32`, or a text literal that reuses an explicit text
declaration. Retained explicit declarations are validated while they are encoded;
omitted declarations are validated once without adding payload bytes. The filtered
canonical data order remaps explicit and synthetic values into one ordinal
namespace. Optimized emission now has one conservative all-or-nothing nominal
case: if the complete validated WVIR closure contains no declared or generic
nominal shape in a function result, parameter, local, temporary, operation
result, record/variant target, or collection target, it emits the canonical
zero-count Types section. If any such use exists, the complete Types order is
retained unchanged. This removes wholly unused imported declaration families
without introducing partial index remapping; a future referenced-type closure
is still required for selective nominal pruning. Identical optimized inputs
produce identical retained order, section contents, and bytes.

The optimized compiler derives reachability from the same validated source scan,
symbol directory, WVIR summary, and canonical orders already prepared for emission.
It does not rebuild those whole-program models through the public analysis adapter.
Normal optimized compilation uses map-only call traversal because function encoding
already performs the complete operation and shape validation pass. The public
reachability-analysis API retains its full per-function code-size attribution.

The source encoder performs UTF-8 scalar partitioning with the language's checked
`u32` division and remainder operations over the nonzero constant 64. It does not
retain the obsolete private quotient/remainder record that preceded those
operators. This keeps the complete compiler build driver at the native x64
runtime's explicit 64-record representation bound without changing UTF-8 bytes or
widening the one-byte native nominal-type encoding.

## Nominal type translation

WVSD assigns canonical source nominal indices independently of source order or
module ownership: records sorted by ordinal name first, then enums, then
variants. WVB extends that category order with fixed arrays, Vectors, and
Sequences. Within every category, retained concrete and compiler-materialized
names are strictly ordinal. When templates exist, the backend builds bounded
target maps, omits every template, compacts concrete declarations, and remaps
all downstream shapes and operation targets before serialization.

Each Types entry carries its exact WVB kind tag and name. Record fields and enum members retain source declaration order. Record field types are rebound through the validated symbol evidence so enum fields carry the exact canonical Types index. An `i32`-backed enum retains kind `2`; its member values preserve the exact signed value as the canonical two's-complement 32-bit pattern, including `-2147483648`. Descriptorless Seed declarations continue to supply their historical nonnegative `i32` subset. A `u8`-backed edition-1 enum emits WVB 1.22 kind `7`, exact source backing identity byte `6`, and one exact member byte. Both kinds remain in one name-sorted enum category and both use ordinary enum shape `8`. An edition-1 enum backed by `i8`, `i16`, `i64`, `u16`, `u32`, or `u64` remains valid source-analysis evidence but makes the current writer return `Unsupportedˉshape` before it publishes a partial module whenever executable WVIR actually requires nominal Types; it is never narrowed to `i32` or `u8`. A program whose entire nominal declaration family is unused may instead take the zero-count optimized Types path above. Private WVGT kind-11 Vector shapes encode byte `23` plus their planned kind-5 index; kind-12 Sequence shapes encode byte `24` plus their planned kind-6 index.

Primitive value shapes occupy one byte. Internal shapes `7` and `8` encode WVB `i64` and `u64` value tags `9` and `10`; shapes `9` and `10` encode WVB 1.15 `unit` and `never` tags `20` and `21`; shapes `11`, `12`, and `13` encode WVB 1.12 tags `14`, `15`, and `16` for `i8`, `i16`, and `u16`; shapes `14` and `15` encode WVB 1.14 tags `18` and `19` for `f32` and `f64`; shape `16` encodes WVB 1.13 tag `17` for `rune`; and exact intrinsic shape `805306368` encodes byte `25` under the WVB 1.21/1.22 launcher-entry rule. Record shapes encode byte `7` plus their planned WVB Types index; enum shapes encode byte `8` plus their planned index. `never` is restricted to a function result. Shape `25` is restricted to the sole parameter of exported `Main` returning `i32`; the remaining encodings apply uniformly wherever their source types are admitted.

The main backend gives the generic serializer one independently validated
materialization plan plus the exact source-to-WVB target maps. It emits each
retained instance—including Foundation `Option<T>` and `Result<T, E>`—as an
ordinary concrete record or variant entry. Dependency/materialization order is
internal evidence only. Before any shape is encoded, the backend constructs a
separate canonical WVB entry map: concrete and generic records, all enums,
concrete and generic variants, arrays, Vectors, then Sequences, with ordinal
name order inside each category. Private fixed-width names are `__WvY0000`
through `__WvY1023`, so they sort after legal capitalized source names. Nested
materialized shapes refer to these final ordinary Types indices, including
forward references. Generic templates are not runtime entries, and no
Foundation-only private-name or type suffix exists.

The serializer records the byte boundary between its generic-record and
generic-variant payloads so the main Types writer can insert each group in its
canonical category. It admits at most 1,024 total Types entries and bounds its
emitted entry payload to 4 MiB. It rejects an invalid
materialization, an unsupported shape, an inconsistent nested nominal kind, or
a type or evidence limit without returning partial output. It preserves the existing
record/variant metadata and WVB feature bits, including multi-field marker `2`;
it adds no WVB version or runtime generic representation.

Main Source WVB extracts WVGT from WVLB, reconstructs both the dependency plan
and canonical output map, inserts each Types entry in the category/name order,
and remaps private function, field, temporary, and nominal operation identities.
Public reachability analysis constructs the same canonical evidence instead of
using template-bearing source indices. `Box<Point>` produces a
concrete private `__WvY0000` record whose field targets the compacted `Point`
entry; the source `Box<T>` template is absent. This proves deterministic main
WVB metadata and target translation. A `Recordˉcreate = 17` or
`Recordˉfield = 18` operation whose validated target is a private WVGT shape is
translated through that same materialization entry to the ordinary WVB Types
index. Result and operand shapes use the existing shape planner. The WVB
operation is therefore indistinguishable from an ordinary monomorphic record
construction or read, and no private shape or runtime generic lookup survives.

The same translation applies to a private general-generic variant target.
`Variantˉcreate = 65`, `Variantˉcase = 66`, and `Variantˉfield = 164` resolve
through the validated WVGT materialization entry, then serialize the established
ordinary WVB variant construction, case-test, and field operations against the
ordinary `__WvY` Types index. The materialized case and field order and each
substituted field shape are the runtime contract; the source template, its type
arguments, and the private WIR shape are absent from WVB.

WVIR operations `17` through `22` lower to the established WVB record construction/field and enum constant/equality/inequality/name opcodes. Their target and auxiliary fields are already canonical type and field/member identities validated by WVIR. For operation `19`, the auxiliary is the member identity; the Types entry supplies the exact signed `i32` tag. This keeps WIR nominal and prevents declaration-order indices from being mistaken for source enum values.

WVIR operations `126` and `127` lower to WVB opcodes `BD` and `BE` for `Bytesˉreadˉu64ˉlittle` and `Bytesˉfromˉu64ˉlittle`. They are ordinary members of the canonical WVB 1.11 vocabulary; the backend does not select another minor version when they occur.

WVIR operation `128` lowers to WVB opcode `BF` for `U64ˉfromˉu32`. It preserves the complete `u32` numeric domain exactly and is likewise part of canonical WVB 1.11.

WVIR operations `129` through `147` lower to WVB 1.12 opcode `C0`, followed
by the WVB fixed-integer type tag and operation selector. A constant additionally
carries its raw little-endian `u16`; `i8` requires a zero high byte. Comparisons
produce `bool`, signed negation is admitted only for `i8`/`i16`, and bitwise or
shift selectors are admitted only for `u16`. The backend never emits an invalid
type/selector pair.

WVIR operations `148` through `150` lower to WVB 1.13 opcode `C1`. The following
selector is `0` for a constant, `1` for equality, and `2` for inequality. A
constant is followed by one exact little-endian `u32` Unicode scalar; comparison
forms carry no immediate payload. Any rune shape or operation selects WVB 1.13.

WVIR operations `151` through `162` lower to WVB 1.14 opcode `C2`. The following
bytes carry the exact WVB floating type tag and selector `0` through `11` for
constant, add, subtract, multiply, divide, negate, equality, inequality, and the
four ordered comparisons. A constant additionally carries its raw little-endian
`u32` or `u64` bits. Any `f32`/`f64` shape or floating operation selects WVB
1.14; an unaffected module keeps its prior lowest required version.

WVIR operation `163` lowers to the one-byte WVB 1.15 opcode `C3`. It produces
one canonical unit stack cell and has no immediate. Any unit or never shape or
unit operation selects WVB 1.15; an unaffected module keeps its prior lowest
required version.

WVIR operation `164` lowers to the nine-byte WVB 1.16 opcode `C4`, followed by
the canonical variant Types index and packed `case * 64 + field` selector. It
consumes one exact nominal variant and produces the selected field's exact
shape. The retained opcode `99` is valid only for a case with exactly one field;
it cannot reinterpret a multi-field case as one payload. Any `C4` operation
selects WVB 1.16 even when it addresses the legacy marker-`1` encoding of a
single-field case.

WVIR operation `165` lowers to WVB 1.17 opcode `C5`, followed by the canonical
kind-4 Types index for its exact `Collections.Array<T, N>` instance. It consumes
the literal's `N` exact element temporaries in index order and produces the
private array shape. WVIR operation `166` lowers to the one-byte opcode `C6`; it
consumes that exact array plus a complete `u64` index and produces `T`. Any
materialized array type, array shape, or either operation selects WVB 1.17.
The current source parser and WIR operation representation admit at most 64
elements in one literal even though the serialized type descriptor supports
lengths zero through 4,095. That is a named compiler construction limit, not a
different array type or an inferred dynamic collection.

Named-record syntax has disappeared by this boundary: typed WVIR has already evaluated source fields left to right and reordered their temporary operands to canonical declaration order. It therefore lowers through the same record-construction opcode and value layout as the retained positional spelling.

## Capability translation

The validated root profile maps directly to the existing WVB profile byte. A
dependency may declare capabilities when its profile is compatible with its
importer. The root must explicitly redeclare every capability required by its
complete dependency closure; compilation does not treat a library requirement as
authority. The root declarations alone form the emitted capability table, sorted by
ordinal name independently of source declaration order. Dependency capability calls
are rebound to those root-owned indices by canonical capability name. Parameter and
result tags come from the fixed eleven-entry Seed catalog, including
`filesystem.directory_read_v1`, `storage.random_access_v1`,
`standard_output.write_v1`, `model.catalog_v1`, and `model.inference_v1`.

WVIR operation `63` carries a validated WVSD capability directory entry. The backend ranks that entry among capability declarations by ordinal name and emits WVB `call.capability` with the resulting index. No capability is inferred, removed, or authorized by compilation; host support and runtime authorization remain separate required boundaries.

An internal typed singleton capability-reference shape encodes as the existing WVB
`u32` value tag in parameters, results, locals, and temporaries. Acquisition lowers
from the typed WVIR `U32ˉconstant` to `u32.const 0`; the payload is an erased witness
and cannot select a provider or grant authority. Calls through that reference have
already resolved its exact root capability identity and lower to the same static
`call.capability` index as a qualified source call. WVB 1.11, its verifier, and the
runtime capability table therefore require no new serialized shape or opcode.

## Text and static data

Integer-array elements are serialized as exact 32-bit little-endian two's-complement values. Byte data preserves each declared byte. Text data and text literals are serialized as strict UTF-8.

The portable backend decodes the source escape set `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX`. A UTF-16 high-surrogate escape must be followed by its low-surrogate escape and is emitted as one Unicode scalar value.

Root explicit text declarations register decoded values in source order. A string literal from any module first reuses a matching explicit declaration, then a prior synthetic value. New literal values receive the first available six-digit name beginning at `__Text_000000`; root data-name collisions are skipped. Literal discovery traverses global functions in canonical function order and operations in WVIR order. The final merged data section is sorted by ordinal name.

The combined explicit and synthetic data count is bounded to 4,096 entries. Synthetic names are bounded by `__Text_999999`. Any limit or invalid data condition fails before a WVB value is published.

Canonical WVB admits 8,192 combined parameter/local slots. The current Windvale-native source emitter retains its narrower 4,096 combined parameter, source-local, and WIR-temporary implementation limit; compiler-generated temporaries consume only the remaining slots and do not enlarge the source namespace. Stage 0 may emit the wider canonical envelope, but removing this narrower bootstrap limit requires a separately reproduced compiler artifact.

## Code lowering contract

Every WVIR temporary becomes a WVB local after the function's parameter and user-local slots. A concrete WVLB local shape maps directly to WVB metadata; an inferred WVLB shape marker is resolved from the first verified WVIR store for that slot. Each operation loads its temporary operands, executes one WVB instruction, and stores a result temporary when present. The operand stack is therefore empty between WVIR operations and at every basic-block boundary.

The common identity temporary-slot allocation is represented by an empty mapping
and computes `parameters + locals + temporary` directly. Its temporary shapes are
an immutable slice of the already validated WIR shape table. Only the lifetime
allocator materializes a nonidentity temporary-to-slot mapping. This avoids two
incremental four-byte construction chains per temporary while preserving local
indices, shapes, emitted instructions, and canonical WVB bytes.

The backend makes two deterministic passes over each function. The first computes every block byte offset, exact function code length, and maximum operand-stack depth. The second emits code using those offsets, so branches never require mutable backpatching.

`Valueˉphi = 64` is typed control evidence rather than a new WVB opcode. It has zero operation bytes. Each unconditional predecessor jump to a phi join emits the selected exact-shape temporary load and phi-result local store immediately before the ordinary jump. This serves value-producing `if`, exhaustive value-producing `match`, and the retained Boolean short-circuit lowering. Only the selected predecessor has a bytecode execution path, and WVIR validation forbids a conditional or third predecessor from targeting such a join. No WVB opcode or minor-version change is required.

The complete source set is fully validated before WVB emission. Declaration reads during emission therefore begin at their already validated byte offsets with relative diagnostic coordinates instead of rescanning from the module header to reconstruct absolute line and column. All source-facing syntax and semantic diagnostics are established by the checked upstream boundary; backend emission consumes only accepted offsets and declaration shapes. This optimization does not change accepted source, emitted bytes, or public diagnostic identities.

Primitive WVIR shapes map to WVB shapes as follows:

| WVIR shape | Meaning | WVB shape byte |
| ---: | --- | ---: |
| 0 | `void` | 0 |
| 1 | `i32` | 1 |
| 2 | `u8` | 4 |
| 3 | `u32` | 5 |
| 4 | `bool` | 2 |
| 5 | `text` | 3 |
| 6 | `bytes` | 6 |
| 7 | `i64` | 9 |
| 8 | `u64` | 10 |
| 9 | `unit` | 20 |
| 10 | `never` | 21 |
| 11 | `i8` | 14 |
| 12 | `i16` | 15 |
| 13 | `u16` | 16 |
| 16 | `rune` | 17 |

An ordinary private WVGT instance of intrinsic kind `10` maps to WVB shape byte
`22` followed by its planned kind-4 Types index. Its Types entry carries one
exact encoded element shape followed by the `u32` fixed length. It is nominal
only at the serialization boundary so nested arrays and exact `T, N` identity
remain unambiguous; source cannot name the private `__WvY` identity.

An ordinary private WVGT instance of intrinsic kind `11` maps to WVB shape byte
`23` and a kind-5 Types entry; intrinsic kind `12` maps to shape byte `24` and a
kind-6 entry. Each entry carries only its private name and exact encoded element
shape. The representation deliberately carries no fixed maximum or capacity,
and selecting either descriptor or shape selects WVB 1.18. This checkpoint does
not add an operation or executable collection storage layout.

The encoder writes canonical Module, Capabilities, Data, Functions, Code,
Exports, and Types section envelopes. It emits WVB 1.18 for Vector or Sequence
metadata or shapes, otherwise WVB 1.17 for fixed-array metadata, shapes,
construction, or access, otherwise WVB 1.16 for multi-field variant
metadata or a named variant-field instruction, otherwise WVB 1.15 for unit or
never evidence, WVB 1.14 for floating evidence, WVB 1.13 for rune evidence,
WVB 1.12 for fixed-width integer evidence, and the byte-identical WVB 1.11
baseline when no extension occurs.
Every module carries the metadata-presence byte, including modules without
metadata. Capabilities and Types contain canonical zero counts when absent and
canonical entries when their accepted declarations are present. Function
metadata includes user locals followed by temporary locals, contiguous code
offsets, exact code lengths, and the computed maximum stack depth.

`Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv` exercises literals,
typed constants, parameters/locals/results, checked arithmetic, comparison,
signed negation, and `u16` bitwise/shift behavior. It publishes deterministically
as a 5,335-byte WVB 1.12 module with SHA-256
`b3cca3ae81dfadc78d45b1f83b5bdd7a3deaff1d42624e12c2a610bdb3f222a9`
and executes with result `42`. The unchanged minimum edition-1
program remains exactly 221-byte WVB 1.11 with SHA-256
`25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae`.

`Tests/Fixtures/Language-1.0/Rune-Program.wv` covers direct Japanese and emoji
scalars, braced Unicode and simple escapes, typed constants,
parameters/locals/results, and equality. It publishes deterministically as a
1,148-byte WVB 1.13 module with SHA-256
`116ff74b5b9c18a76af21785b7aa9017fe4f0c4ff73fa363dfa72898cf9d3dde`
and executes with result `42`.

`Tests/Fixtures/Language-1.0/Floating-Program.wv` covers `f32` and `f64`
constants, parameters, locals, results, round-to-nearest arithmetic, unary
negation, every comparison, signed-zero equality, infinities, and canonical NaN
behavior. It publishes deterministically as a 2,809-byte WVB 1.14 module and
executes with result `42`. Four source-rejection cases and eight independent
malformed-WVB mutations prove suffix, spelling, type/operator, header, type,
selector, immediate-width, and operand-shape boundaries without publication.

`Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv` covers a three-field case,
one no-data case, trailing commas, source-order evaluation with
declaration-order construction, named destructuring in a different order, and
zero-field named construction, plus empty `if` and `else` blocks that remain
unambiguous beside that construction syntax. It compiles twice to the same
918-byte WVB 1.16 module with SHA-256
`f3ceb596f1bcedda877ceea5aeb99aff1d5bcfa3b984fdae0e16eb21570562d1`.
`Named-Variant-Field.wv` isolates a one-field marker-`1` case plus opcode `C4`
and therefore proves instruction-driven 1.16 selection in a deterministic
428-byte module with SHA-256
`2dea4aa515633e85863e51279f320d53f09c2bf4628b72d93fdc79559479209f`.
Both pass the compiler-aligned verifier and execute through the source-built
native scalar runner with result `42`; the multi-field fixture takes 76 guest
instructions and the named single-field fixture takes 26. Nine source
rejections and ten
byte-level mutations cover declaration, construction, destructuring, version,
marker, count, nominal, case, field, type, runtime case mismatch, and truncation
boundaries. The in-range case-mismatch module is rejected by the verifier and
fails direct scalar execution with `WVR3017`.

`Tests/Fixtures/Language-1.0/Fixed-Array-Main-Pipeline.wv` imports the canonical
`Foundationˉcollections` owner, constructs an exact
`Collections.Array<i32, 3u64>` from `[40i32, 42i32, 44i32,]`, and reads index
`1u64`. The current compiler publishes a deterministic 375-byte WVB 1.17 module
with SHA-256
`e2125aba54aca71af5d10a6c7c4228460f2de28230503ad61b0b2877e8b593a7`.
Its Types section contains one kind-4 `Array<i32, 3>` descriptor; its code uses
one `C5` and one `C6`. The independent compiler-aligned verifier accepts it and
the source-built native scalar runner returns `42`. Six focused WVB cases prove
the success path, deterministic `WVR3008` bounds failure, pre-1.17 version
rejection, the 4,095 type-count boundary, and constructor type-index validation.

`Tests/Fixtures/Language-1.0/Vector-Sequence-Wvb-Types.wv` retains exported
`Vector<i32>` and `Sequence<i32>` function signatures plus an unaffected
`Main() -> i32`. It publishes deterministically as a 436-byte WVB 1.18 module
with SHA-256
`c51529baa7fb7b5cfb24e2508520044cce9f2661b9fb1dccb2321b5e122ec73d`.
Its Types section contains kind-5 Vector and kind-6 Sequence descriptors; the
function shapes use tags `23` and `24` and target those entries exactly. The
compiler-aligned verifier accepts it and the metadata-capable scalar runner
executes only the independent `Main`, returning `42`. Four malformed mutations
prove minor-version, element-shape, descriptor-kind, and target-kind rejection.
No collection value is constructed or executed by this checkpoint.

`Tests/Fixtures/Language-1.0/Sequence-Read-Main-Pipeline.wv` is the first exact
source-to-runtime Sequence API selection. Its qualified calls bind only through
the canonical `Foundationˉcollections` owner. WVIR operations 167 and 168 plan
the exact private Sequence shape; emission selects WVB 1.19 `CB` and `CC` with
the matching kind-6 Types index. Because both operations preserve their shared
owner below the scalar result, emission stores the result and follows it with
one `pop`; operation-length and maximum-stack planning include that release.
The 472-byte module has SHA-256
`8f8cb926df946bff3b254b37304ac7cf8ffa744ccea963703cfcfebfdf7e1831`,
passes the collection-aware compiler verifier, and returns 42 from its
independent `Main`. This checkpoint copies only resource-free scalar elements;
it does not claim general borrowed-result lowering.

`Tests/Fixtures/Language-1.0/Vector-Read-Freeze-Main-Pipeline.wv` connects the
owned scalar Vector subset to the same canonical Foundation identity. A direct
non-parameter `Vector<T>` local may be observed only by the explicit immutable
borrow `Vectorˉlength(borrow Values)` or consumed by
`Vectorˉfreeze(Values)`. Parameters, mutable borrows, borrowed freeze operands,
indirect expressions, and non-scalar element shapes remain rejected. WVIR
operation 169 carries the exact Vector shape in its auxiliary field and returns
`u64`; operation 170 carries the Vector auxiliary shape and the exact same-
element Sequence result shape. Both carry the source-local slot as their target
and no ordinary operands. Independent WVIR validation reconstructs both types
from WVGT and rejects a mismatched element or catalog index.

WVB emission uses `local.take` before `CA vector.length`, stores the scalar
result, then stores the still-unique Vector back into its original local. Freeze
uses `local.take` followed by `C9 vector.freeze` and leaves that local
unavailable. A WVB 1.20 function declared to return Vector must return unique
Vector evidence, and a call to that declaration produces the corresponding
unique result; generated Vector stores and Vector returns therefore use
`local.take`, never retaining `local.load`. The initial source ownership check is
straight-line and admits one outstanding consumed Vector local per function.
Branch-sensitive moves, multiple simultaneous consumed locals, and Vector
parameters remain later ownership work. Freeze receives one already-declared
exact result context from a typed local initializer, assignment target,
enclosing function return, or parameter of an already selected non-generic
fixed-signature call. That context must be the canonical same-element
`Sequence<T>`; an inferred local, missing context, or mismatched result rejects
before WVIR publication. The result context never selects a callable or solves
generic arguments. Context propagation through value-producing control flow
and generic calls remains later work.

The exact edition-1 `Foundationˉmemory.Memoryˉbudget` source identity reaches
valid WVIR as private owned shape `805306368`. WVB 1.21 and 1.22 encode that
shape as byte `25` only when it is parameter zero and the sole parameter of
exported `Main`, whose result is exactly `i32`. The encoder rejects the intrinsic in
every other parameter, result, local, temporary, record, variant, or operation
position. It emits no constructor or move opcode and does not place the
intrinsic in Types. The compiler-selected minor is 1.21 when the entry token is
the newest feature and 1.22 when a retained `u8` enum is also present; all
earlier vocabularies remain available under either header.

The canonical Foundation memory import currently also contains exact
`Allocationˉreason: u8` and `Allocationˉfailure` declarations. The optimized
no-nominal-use rule removes both from the entry fixture's executable Types
section, so unused nominal declarations do not enlarge an unrelated budget
program. `Enum-Backing-All.wv` is the matching positive dead-type oracle and
emits a 217-byte executable with zero Types. `Enum-U8-Used-Main.wv` is the
retained representation oracle: it deterministically emits a 415-byte WVB 1.22
at SHA-256
`961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed`,
whose exact kind-7 descriptor contains `Pending = 1` and `Complete = 2`; the
compiler-aligned verifier accepts it and the source-built runner returns `42`.
The writer still rejects retained wider enum backings without output.

`Memory-Budget-Entry-Main.wv` deterministically emits a 242-byte WVB 1.21
module with one 16-byte function body and a zero-count Types section. The
independent verifier accepts that module and rejects version downgrade,
primitive substitution, renamed entry, budget result, second budget parameter,
budget local, budget local-load, budget local-store, and missing-export
mutations. The source-built runner transfers one fresh opaque token, releases it
once on completed top-level return, and produces `42`.

Typed WVIR 1.11/1.12 operation `Foundationˉmemoryˉsplit = 171` lowers to WVB
1.23 opcode `CE` with the parent local and exact materialized Result type as
its two immediates. The emitter maps the private budget to shape `25`, requires
`u64` and `u32` operands, and uses `local.take` for affine budget and Split
Result locals. A bounded proof tracks at most 64 owned slots across at most 64
blocks, intersects availability at forward joins, and accepts a backward edge
only when its complete ownership state exactly matches the saved loop-header
state.

`Memory-Budget-Split-Executable.wv` deterministically emits a 752-byte WVB
1.23 module at SHA-256
`5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53`.
The compiler-aligned verifier accepts successful and refused Split programs,
rejects nine version/opcode/local/type/layout mutations, and the source-built
runner returns `42` for both provider outcomes. General owned arguments/returns
and direct
native/browser/OS execution remain later connected checkpoints.

Typed WVIR 1.11/1.12 operation
`Foundationˉvectorˉconstructˉreserved = 172` lowers to WVB 1.24 opcode `CF`.
The instruction carries the consumed `Memoryˉbudget` local and exact
`Result<Vector<T>, Allocationˉfailure>` type as immediates; the exact `u64`
maximum-items value remains its sole stack operand. The verifier reconstructs
the scalar Vector element and exact failure record from that Result and tracks
the Result as affine until matching transfers its Valid Vector. The runner
converts the budget to one private allocation lease, publishes either the
reserved backing or exact typed refusal, and releases the lease with the final
descriptor. Neither WVIR nor WVB exposes a lease token, provider generation,
heap address, capacity pointer, or target representation.

Generic source instances remain dependency-ordered internally, while WVB Types
entries use a separate canonical nominal-category rank. Private nominal
references are remapped to that output rank. The exact constructor fixture
therefore emits Result before Vector and lets Result's Valid payload point
forward to the later Vector descriptor; dependency discovery order is not a
serialized ordering rule.

`Vector-Construct-Reserved-Executable.wv` deterministically emits a 1,107-byte
WVB 1.24 module at SHA-256
`881bcbabc9620188964a63601490ad81acf63587f70501443d97447cdd45f7c5`.
The compiler-aligned verifier accepts success, target-unaddressable refusal,
and zero-precondition modules, rejects ten exact opcode/local/type/layout
mutations, and the source-built runner returns `42` for both Result paths. Zero
traps with `WVR3008` after four guest instructions.

Typed WVIR 1.13/1.14 operation `Foundationˉvectorˉappend = 173` lowers to WVB
1.25 opcode `D0`. The instruction carries one direct non-parameter
`Vector<T>` local and the exact
`Result<unit, Vectorˉappendˉfailure<T>>` type as immediates; the already
evaluated exact `T` is its sole stack operand. The verifier reconstructs the
Vector element, unit Valid payload, failure record, canonical Collection
failure, and returned item. The mutable borrow remains source/WVIR evidence;
WVB names the local without serializing a borrow handle or pointer.

The runner mutates the reserved backing only when capacity remains. Success
consumes the item and returns canonical unit. Capacity refusal preserves the
Vector owner, length, contents, and backing, and produces exact
`Capacityˉexhausted(Maximumˉitems)` plus the original item. No allocation or
budget transition occurs during append.

The paired WVIR validator proves exact kind-11 Vector moves through ordinary
by-value and borrowed function calls, owned results and returns, forward joins,
and ownership-invariant loops. WVB 1.26 publishes that proof by encoding the
transfer mode directly in
each exact Vector parameter shape: `23` for value, `26` for immutable borrow,
and `27` for mutable borrow. Borrowed shapes are invalid outside parameter
lists, and no trailer, pointer, borrow handle, owner bit, or source slot is
serialized.

The emitter uses `local.take` for a value argument and a retaining load for a
borrowed argument. When the same source owner is first borrowed and later
transferred, the first source load remains non-destructive and the later load
performs the take. The verifier reconstructs the callee signature at each call
and rejects value/borrow mismatches. Runtime parameter-directory decoding
retains the mode while normalizing both borrowed tags to the ordinary Vector
cell representation; deterministic reverse-slot teardown releases either the
transferred value owner or the borrow's temporary retain. The caller's owner is
therefore preserved by a borrow and invalidated by a value transfer exactly as
the WVIR proof requires.

`Owned-Vector-Calls-And-Joins-Wir.wv` deterministically emits a 1,733-byte WVB
1.26 module at SHA-256
`ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387`.
The compiler-aligned verifier accepts it and rejects six exact version,
parameter-mode, return, and local-shape corruptions. The source-built scalar
runner returns `42`. Borrow-after-move, duplicate transfer, and asymmetric-join
fixtures still fail closed as `Invalidˉanalysis` / `Invalidˉwir` before WVB
publication.

WVIR operation `Releaseˉlocal = 174` lowers without advancing WVB beyond the
minor otherwise required by the function. It has no operands or result and
names one direct non-parameter exact-Vector local. The emitter writes the
existing six-byte sequence `local.take <slot>; pop`: `local.take` removes the
unique owner from the local, and `pop` performs the runtime's ordinary
descriptor release. The verifier sees the same ownership transition as an
explicit take and therefore rejects a release after move, a missing release on
a loop edge, and any backedge whose complete owned-slot state differs from the
saved header state. No destructor name, hidden call target, pointer, cleanup
table, new WVB opcode, or new bytecode version is serialized.

`Vector-Append-Executable.wv` deterministically emits a 3,096-byte WVB 1.25
module at SHA-256
`6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f`.
The compiler-aligned verifier accepts the executable success/capacity fixture
and rejects twelve append version, opcode, local, result, unit, failure,
element, and canonical-type mutations. The source-built runner appends `7`,
refuses the attempted `9` at capacity, checks the returned `9` and exact maximum
`1`, and returns `42`.

Typed WVIR 1.15/1.16 operation
`Foundationˉvectorˉgrowˉreserved = 175` lowers to the thirteen-byte WVB
1.27 opcode `D1`. Its three `u32` immediates select one direct non-parameter
`Vector<T>` local, one distinct available `Memoryˉbudget` slot, and the exact
`Result<unit, Allocationˉfailure>` type. The new maximum is the sole exact
`u64` stack operand. The verifier reconstructs the scalar Vector element,
unit payload, and canonical allocation record; validates both local indices;
and preserves both owner states.

The scalar runner implements growth as a strong transaction. It first reserves
the complete replacement lease under the supplied budget while the old backing
remains live, allocates and zero-initializes the replacement, copies exactly the
initialized prefix, attaches the new lease, then releases the old descriptor
and swaps the local. Any budget, target, provider, fragmentation, lease, or
allocation refusal before the swap returns exact `Allocationˉfailure` while
leaving Vector length, contents, capacity, owner identity, and supplied-budget
accounting/generation unchanged. The temporary full replacement means a grow
can refuse even when the final post-swap retained total would fit; callers that
need a lower peak must select a separately specified future operation rather
than receive hidden in-place partial progress. A new maximum less than or equal
to the existing maximum traps as precondition `WVR3008`.

`Vector-Grow-Reserved-Executable.wv` deterministically emits a 3,628-byte WVB
1.27 module at SHA-256
`30de39bdd12ad7718ad1fb465b14bc42f8463b6ecfc6ba1f10494cb6e67c5b59`.
Its first request proves exact budget refusal (`40` requested, `24` available)
and preservation of length `1`; its second request grows maximum `1` to `2`,
appends the second item, and returns `42`. The compiler-aligned verifier accepts
the exact module and rejects fifteen version, opcode, Vector-local,
budget-local, Result, allocation-layout, and truncated-width mutations.

WVB 1.28 recursively extends owned transfer from exact Vector values to
records, variants, and fixed arrays that contain a Vector. Classification uses
the canonical concrete or materialized generic layouts, follows at most 64
ancestor types, and leaves the specialized affine Result paths from operations
`171` and `172` under their existing proof. A constructor uses `local.take` for
every owned field or element. Whole-value stores, by-value calls, and returns
also take the owner; borrowed calls preserve it. Borrowed aggregate parameters
and results remain unsupported in this first executable profile.

A field or element observation must preserve its parent. The emitter therefore
keeps identity temporary allocation for affected functions and gives the
generated observation temporary one local-only shape: `28` for record, `29`
for variant, or `30` for fixed array. It emits exactly `local.load owner`,
`local.store view`, `local.load view`, then the matching observer instruction.
The compiler-aligned verifier reconstructs recursive ownership from Types,
requires the matching nominal identity and exact sequence, and rejects taking,
calling, returning, storing elsewhere, or otherwise escaping the view. Internal
transfer tags used by verification are not serialized WVB shapes.

`Owned-Aggregate-Vector-Executable.wv` deterministically emits a 1,538-byte
WVB 1.28 module at SHA-256
`b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395`.
It materializes `Workˉqueue<Vector<i32>>`, performs immutable and mutable field
observation, and transfers the whole record through an ordinary call before the
source-built scalar runner returns `42`. The verifier accepts the exact product
and rejects version downgrade, a borrowed aggregate parameter, mismatched view
identity, replacing the owner local with a view, taking before observation, and
taking the borrowed view. Four source fixtures reject use after whole-value
move, duplicate move, owned-field extraction by value, and mutable borrow from
an immutable parent.

Typed WVIR operation `Platformˉsourceˉlength = 176` lowers to the five-byte
WVB 1.29 opcode `D2`, followed by one little-endian `u32` local index. The
private source identity serializes as shape `34`. Shape `34` is valid only as
the sole by-value parameter of exported `Main(Sourceˉfile) -> i32` and as a
non-parameter local that receives that owner through `local.take`; it is invalid
in Types entries, fields, payloads, collections, results, other signatures, and
ordinary source-declared locals.

Opcode `D2` names only the available non-parameter shape-34 local and pushes
one `u64` length. It has no stack operand and cannot target the entry parameter.
The compiler-aligned verifier requires the exact hosted entry, at least one
`D2`, bounded ownership on all control paths, and the ordinary explicit
`local.take; pop` cleanup produced by semantic `using`. A WVB 1.29 header
without that exact source entry and operation rejects; earlier minors cannot
encode either shape `34` or opcode `D2`.

`Source-File-Snapshot-Executable.wv` deterministically emits a 373-byte WVB
1.29 module at SHA-256
`01065b752d7ea6d64e3bf36bdd4d8a0d2e5b7faf6794de173580003ed3935d05`.
It moves the launcher-supplied owner into `using`, observes its length through
an immutable borrow, releases it on each return path, and returns `42` for a
42-byte snapshot. The verifier accepts the exact product and rejects version
downgrade, forgeable parameter/local shapes, unknown opcode, observation before
parameter transfer, and copying instead of moving the parameter. WVB carries
no host path, handle, source bytes, provider object, or ambient filesystem
grant.

WVIR 1.17 operation `Functionˉreference = 177` lowers to the nine-byte WVB
1.30 opcode `D3`, followed by the emitted function rank and the exact callable
Types index. WVIR 1.18 operation `Callˉindirect = 178` lowers to the five-byte
opcode `D4`, followed by that same callable Types index. The WVB producer maps
the compiler-private WVIC instance to one terminal kind-`8` descriptor after
all nominal Types entries:

```text
u8      kind: 8
u8      profile: 1 portable, 2 hosted, 3 system
shape   result
u32     parameter count
shape[] parameters in declaration order
```

There are at most 256 callable descriptors, and each has at most 64
parameters. Serialized value shape `35` followed by the descriptor's `u32`
Types index carries the exact callable identity through signatures, locals,
and the verifier stack. The producer interns complete descriptor identities in
deterministic WVIC order; it never relies on source names or host pointers.

`D3` proves that the target function's profile, parameter shapes, and result
shape exactly equal the referenced kind-`8` descriptor, then pushes one
shape-`35` value. For `D4`, the callable value precedes its arguments on the
operand stack. The verifier consumes the arguments in reverse declaration
order, then requires the remaining callable value to name the instruction's
exact descriptor. Execution performs one ordinary bounded frame call to the
function carried by the value and preserves the existing call-depth,
instruction, local, and stack limits. The current scalar runner stores the
function index in the low `u32` and the callable type index plus one in the high
`u32` of its eight-byte value cell. That cell layout is an implementation
detail, not a portable WVB or native ABI promise.

WVB 1.30 must contain at least one `D3` or `D4`; each opcode is limited to
65,536 occurrences. This first executable profile admits only named,
non-generic, noncapturing functions with explicit empty `effects()`, no
`async` or `unsafe` flag, by-value parameters, and a value result other than
`unit` or `never`.

`Callable-Indirect-Execution.wv` deterministically emits a 400-byte WVB 1.30
module at SHA-256
`30eab353a6187ead317438d2c63a2bd6aa53d9ec682bc5c59d9d3b82530edfaf`.
The source-built scalar runner executes its function reference and indirect
call, returns `42`, and reports 24 guest instructions. The compiler-aligned
verifier accepts the exact module and rejects version downgrade, target
signature mismatch, reference-type mismatch, invocation-type mismatch, and a
non-callable descriptor kind.

WVIR 1.19 operation `Closureˉcreate = 179` lowers to the thirteen-byte WVB
1.31 opcode `D5`, followed by the emitted target-function rank, public callable
Types index, and capture count. WVIR 1.20 is the same closure vocabulary plus
the generic-instance header inherited from WVIR 1.18. The operation has 1
through 64 capture operands in declaration order and produces the callable
shape for its WVFT instance. WVB emission loads the captures in that same order
before `D5`.

The physical target's parameter list is exactly the captured prefix followed
by the public descriptor parameters; its result is the descriptor result. WVIR
validation additionally requires an ordinary function declaration with the
same module profile, explicit empty `effects()`, no generic parameters, no
`async` or `unsafe` flag, and by-value parameters. The copied prefix is limited
to inline scalar and enum shapes. Text, bytes, callable values, aggregates,
collections, resource owners, and borrows reject before WVB publication.

`D5` consumes the captures and publishes one shape-`35` callable. The existing
`D4` indirect call recognizes the representation-private environment, copies
the immutable captures into the physical parameter prefix, then appends the
public arguments and enters the target through the ordinary bounded call-frame
path. The scalar runner admits at most 1,024 created environments and retains at
most 536,576 bytes (524 KiB) of environment records for one execution, then
discards the arena at teardown. A WVB 1.31 module must contain at least one
`D5`; `D5` is limited to 65,536 occurrences.

The deterministic closure-environment WVB oracle is 325-byte WVB 1.31 at
SHA-256
`397f716af132192697c77d9f4f03e72c937e188aca78cf0474c9faaa2234e0e2`.
It snapshots captured `i32` value `40`, calls the public one-parameter callable
with `2`, returns `42`, and reports 11 guest instructions. The verifier accepts
the exact product and rejects version downgrade, target/type mismatch, zero or
65 captures, capture-shape mismatch, a reference-backed capture, indirect-call
type mismatch, and a non-callable descriptor kind.

The source compiler now lowers one synthetic physical body for every admitted
closure site. Copy, move, and immutable-borrow captures of inline scalars and
enums enter the WVB 1.31 environment; move invalidates the outer slot, and an
immutable-borrow callable is confined to the captured owner's lifetime and an
immediate local indirect-call use. The native x86-64 backend executes that same
frame-owned subset. Effectful or flag-bearing callable values, borrowed
callable signatures, mutable write-through captures, retained captures,
general dispatch, and escaping environments remain separately versioned work.

The Edition 1 source front end now appends token identity 102 for `effects` and
parses an optional exact clause after a function return type. It retains clause
presence, byte span, and identity count under fixed 32-identity,
16-segment-per-identity, 128-canonical-byte, and 16,384-source-byte limits.
That initial syntax checkpoint did not change WVIR or WVB. The current WVEF and
WVCF phases now resolve canonical identities, infer exact transitive effects,
enforce declaration equality, and catalog concrete signatures. WVB 1.30 and
WVB 1.31 admit only the empty-effect callable subsets above. WVB 1.32 appends
the exact flags, result mode, language-effect mask, capability-effect bitmap,
and parameter modes to every callable descriptor in that module.

WVIR 1.21 operations 180 through 185 lower in order to WVB opcodes `D6` through
`DB`; WVIR 1.22 retains the same operations with the generic-specialization
header. The writer remaps all scope, handle, construction, spawn, and outcome
types into the canonical Types directory and writes exact local/type immediates.
The verifier reconstructs the canonical `Foundationˉtask` and
`Foundationˉoperation` layouts, proves scope and handle availability at every
edge, consumes a handle at its sole await, and requires an immutable exit policy
before control leaves the lexical scope.

The deterministic source fixture emits as a 4,231-byte WVB 1.32 module at
SHA-256
`11a2bed917a9a30dc12fc565b0cc93e2731ee8b48c8bd2b6d1f54ebe97a145c8`.
It contains one scope construction, derived context, spawn, await, and scope
exit, and returns `42` through the source-built sequential runner. Separate
fixtures preserve a completed aggregate across garbage collection and observe
stable child trap, work-limit, and call-depth-limit identities. Version,
spawn-result, await-origin, and exit-policy corruptions reject before execution.

The 1,199-byte WVB 1.20 fixture has SHA-256
`c73f2e77aa4208a74385046a27beba7dea42e4cece730bfd9ac0ac61ca7a77bc`.
Its independent `Main` returns 42; its nine functions retain direct-return,
declared-local, assignment, and nested fixed-signature argument lowering
oracles independently of fallible Vector construction. Six
byte-level corruptions cover version, unique return, unique length access, and
three Types immediates. Eight source rejections cover use after freeze, wrong
borrow modes, parameters, an unsupported element, a missing inferred result,
and mismatched result and argument contexts. The focused harness therefore has
11 cases and the front-door phase has 19 cases.

The scalar representation uses one eight-byte value cell and the existing
fixed 768-cell immutable aggregate arena. The low `u32` is the first field slot
or `0xffffffff`; the high `u32` is
`0x80000000 + (type + 1) * 256 + case`. Stack values, active locals, and saved
frame locals are roots. A bounded mark/sweep pass reclaims unreachable record
and variant spans and releases descriptor fields before one retry. The
512-byte one-field pressure fixture performs 900 replacements, returns `42` in
26,134 guest instructions, and proves reclamation beyond arena capacity.

Direct native array lowering, browser packaging, and Windvale OS execution remain
explicit subsets below the WVB 1.17 fixed-array execution vocabulary. Vector and
Sequence execution also remains below the WVB 1.18 metadata vocabulary. Decision 0773 owns this scalar execution
checkpoint without silently advancing those consumers.

`Tests/Fixtures/Language-1.0/Foundation-Generic-Result.wv` exercises concrete
Option and Result construction and matching, same-error/different-success
`try`, statement `try`, explicit migration adapters, and 16 ordinary concrete
specializations spanning private ranks 0 through 15. Its constructors carry
explicit complete type arguments, matching every other generic nominal
construction. It emits deterministically as a 3,143-byte WVB with SHA-256
`fb3d07717252b60dcbcd6da1a95dbf6bccb8b85ba79d3a08c5e0e6306b722a81`,
passes the current compiler-aligned verifier, and executes with result `42`.
Wrong arity, an extra argument, bare Result use, omitted construction arguments,
and a mismatched `try` error shape are rejected before WVB publication.

## Verification

The focused conformance test compiles the backend core, runs its profile/acceptance demo, and runs the hosted tool over the complete differential fixture family. Each returned WVB passes the mandatory Stage 0 verifier, executes in the reference runtime when it exposes an executable entry, and compares byte for byte with Stage 0 compiler output.

A separate control-flow oracle compiles nested `&&`/`||` precedence, a skipped out-of-bounds operand, `break`, `continue`, and all three compound assignments through both compilers. Their WVB bytes compare exactly and execute with result `7`.

`Tests/Fixtures/Source-Wvb/Function-Only.wv` retains the original four-function primitive/control-flow baseline while using storage-free typed constants, inferred mutable numeric locals, and multiline trailing commas. Both backends produce the exact 816-byte WVB 1.11 module with SHA-256 `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936`; it executes with result `6`.

`Tests/Fixtures/Source-Wvb/Data-And-Text.wv` deliberately interleaves unsorted functions and static data. It covers inferred integer, text, and bytes locals; signed integer arrays; multiline trailing commas; explicit and synthetic text; literal reuse; a synthetic-name collision; escaped Unicode and a surrogate pair; Foundation intrinsics; remapped data references; and remapped calls. Both backends produce the exact 1,652-byte WVB module with SHA-256 `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc`; it executes with result `13`.

`Tests/Fixtures/Source-Wvb/Pruning.wv` is the closed-world optimization oracle. Its
complete form contains four functions and two data entries in 395 bytes with
SHA-256 `42810451eb302f79d0c167eda3fe62b681277661b277a06badcffd177aba5f35`.
The hosted optimized compiler removes `Dead`, removes `Deadˉvalues`, remaps the
retained call and data indices, and deterministically produces three functions and
one data entry in 308 bytes with SHA-256
`d2f8b67a3a83f393fba16d4f1294000d631e401abd0c4fdde521c9654407b02a`.
The optimized module passes the compiler-aligned verifier and executes with result
`42` in the native WVB runner.

An alternating two-pair Windows hosted-tool measurement over the current complete
13-module compiler-tool closure observed optimized compilation averaging 45.861
seconds and complete emission averaging 46.400 seconds. The optimized closure
contains 415 instead of 442 functions and is 926,108 instead of 950,265 bytes, a
2.54% reduction. Both runs in each mode produced identical SHA-256 values.

A retained pre-implicit-mapping compiler processed the same current closure with
the same harness in 46.305 seconds optimized and 46.089 seconds complete. The new
compiler was 0.96% faster in optimized mode while the complete-mode difference was
0.67% slower and within the observed run variation. Both compilers produced the
same optimized and complete bytes. The implicit identity therefore has direct
allocation and byte-identity evidence, but this small sample does not claim a
portable timing improvement or fixed threshold.

`Tools/Native/Measure-Source-Wvb-Compilation.ps1` makes that comparison
repeatable. It reads the canonical Project 2 source order, alternates optimized
and complete runs to reduce ordering bias, requires byte-identical output within
each mode, and emits per-run and aggregate JSON. The compiler application digest,
source count, output sizes, and output digests are included so results from
different binaries or closures are not silently combined.

`Tests/Fixtures/Source-Wvb/Nominal-Types.wv` deliberately interleaves records, enums, data, and unsorted functions. It covers inferred record, enum, and text locals; named literals; canonical record/enum grouping and ordering; every primitive record field plus enum fields; nominal parameters/results/locals/temporaries; multiline trailing commas; and all six nominal WVIR operations. Both backends produce the exact 1,782-byte WVB module with SHA-256 `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b`; it executes with result `11`.

`Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv` deliberately declares the
baseline hosted catalog out of order. The library suite separately covers the two
rights-limited application capability signatures and capability calls imported from
a dependency. Successful compilation canonicalizes both cases without invoking an
I/O operation.

The three `Tests/Fixtures/Source-Wvb/Composition-*.wv` sources cover canonical flattening across a root and two transitive dependencies. Dependency-owned functions, records, enums, a variant, and text literals combine with root static data and a synthetic-name collision. Only `Main` remains exported. Both backends produce the exact 1,388-byte WVB module with SHA-256 `42d134ee0674dcc2cfa97d018ea03b27f014b2f916d8273ba02a0aee868e0fd5`; it executes with result `42`. Reversed dependency order is rejected before output publication.

`Tests/Fixtures/Source-Wvb/Wide-Scalars.wv` covers checked `i64`/`u64` constants, arithmetic, comparisons, bitwise operations, formatting, and exact little-endian `u64` byte construction and reading. Both backends produce the exact 2,750-byte WVB module with SHA-256 `b898bc07461f7d93b2c8bd5806e06fa5c98cdaa5c11a7f4ce1fef89b77a7bf69`; it executes with result `64`.

The last retained deterministic compiler artifacts before the WVB 1.16 named
variant-field checkpoint were:

- `Source-Wvb-Core.wvb`: 1,033,007 bytes, SHA-256 `e8ed25a0f259a8402409d9474b7ade42b8064ef84cf2298eeb05cc44a6d2df7c`.
- `Source-Wvb-Demo.wvb`: 1,038,806 bytes, SHA-256 `1a1b68e261e736a59a9b4629e14bcab041de8f1b2b43db15b7f41918f07fdf89`.
- `Source-Wvb-Tool.wvb`: 1,033,177 bytes, SHA-256 `cc450810ba8a62357d995c55f1312e8c33e4f8c6d9e8ade3b9fa849f68e7f4f8`.
- `Source-Wvb-Memory-Adapter.wvb`: 1,027,855 bytes, SHA-256 `f6e668ed8782b36635870b025f5d4e7e1134017c1b62aaa92b3ae154119ed805`.

These identities remain historical inputs and are not a claim about the current
modified source. The WVB 1.16 checkpoint instead records its exact focused
fixture identities above; refreshed whole-compiler artifact identities require
the later paired-host qualification gate.

Decision 0518 moved ordinary construction of the core, demo, and tool products to
the bounded native compiler-seed launcher. Decision 0528 now routes repository
project builds through explicit Workspace 1 and Project 2 inputs, and Decision 0529
adds native capability-bearing library composition evidence. Historical
differential results remain evidence, but the normal build and focused verification
path for this boundary is native-owned.

The memory adapter contains 472 functions. These are local candidate identities
and measurements. Complete Stage 1/Stage 2 bootstrap and dual-host qualification
must still be rerun before the candidate becomes a new cross-host bootstrap claim.

The static multi-module behavior was first qualified at `cb1db235`, the fused typed-WVIR artifact set at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index artifact set at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055 changes embedded compiler implementation bytes but preserves all five differential fixture outputs byte-identical to Stage 0 and is cross-host qualified at `1a4fca7`.

For Decision 0058, Stage 0 compiled the then-canonical 12-module source inventory into a 599,868-byte Stage 1 tool. Stage 1 then compiled the same inventory in 6,700,562,174 VM instructions and produced an independently verified 599,868-byte Stage 2 module with the same SHA-256. Stage 1 and Stage 2 compare byte for byte. The dedicated bootstrap verifier reconstructs both stages from the explicit inventory and refuses any verification, size, digest, or byte-identity mismatch. This retained historical proof and artifact set are cross-host qualified at `5c16547`; they do not qualify the candidate identities above.

## Expansion path

Exact bytecode compiler self-reproduction is cross-host qualified under Decision 0058. The 4 MiB WVSS envelope is sufficient for the real compiler closure, while parity with Stage 0's larger input limit remains a separate future contract decision.

The retained recovery archive remains historical independent evidence. Ordinary
compiler execution, Project 2 construction, WVB verification, and publication use
the native front door. Native object emission, executable containers, and
OS-specific lowering remain separate contracts.
