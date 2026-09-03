# Windvale typed source IR

## Status and purpose

`Compilerˉsourceˉwir` is the first portable Windvale-written typed lowering phase. It consumes one complete WVSS 1 source graph, reuses validated WVSD symbol evidence and WVLB local evidence, checks expression and control-flow semantics, and publishes canonical `WVIR 1` bytes.

WVIR is a compiler boundary, not executable bytecode. It preserves typed operations, basic blocks, calls, and stable declaration identities so later passes can lower the same program to WVB or future native and system targets without inheriting C# host behavior. Construction failures preserve exact source locations; successful persisted operations do not retain redundant source spans.

## Public result

```text
Compilerˉvalidateˉsourceˉwir(Input: bytes)
    -> Compilerˉsourceˉwirˉsummary
```

On success, the summary contains module, function-entry, block, operation, temporary, and operand counts plus an independently validated WVIR directory. On failure, the directory is empty and the summary identifies the first deterministic failure by module, related module, WVSD function entry, byte offset, and one-based line/column.

The status contract distinguishes upstream source-binding rejection, evidence limits, malformed constructed evidence, type mismatch, invalid conditions and returns, missing returns, unreachable statements, invalid data/index/field/operator use, invalid call arguments, invalid local inference, invalid constant evidence, named-record failures, loop-control placement, invalid or non-exhaustive enum/variant matching, unknown variant cases, invalid payload bindings, invalid collection shapes, invalid or consumed builders, an invalid result-propagation contract, invalid unit use, invalid record update, invalid named variant construction, and invalid value blocks. Appended values `23` through `31` own those match, variant, collection, and builder failures, `Invalidˉtry = 32` owns propagation failures, `Invalidˉunit = 33` owns a unit expression outside edition 1, `Invalidˉrecordˉupdate = 34` owns a cross-edition or wrong-nominal-base update, values `35` through `37` own an invalid variant literal plus duplicate or missing variant fields, `Invalidˉvalueˉblock = 38` owns a malformed or valueless value-producing control arm, values `39` through `41` own generic resolution, specialization, and bounded-iteration failures, `Invalidˉarray = 42` owns fixed-array construction and access failures, `Invalidˉborrow = 43` owns invalid borrow formation, mode, origin, read-through, or escape, `Invalidˉresource = 44` owns a `using` initializer that is not an admitted resource, `Invalidˉcallable = 45` owns a named-function value or indirect call outside the closed noncapturing callable profile, `Invalidˉtask = 46` owns a structured-task violation, `Unsafeˉcontextˉrequired = 47` rejects a direct or indirect unsafe invocation outside a lexical unsafe context, and `Invalidˉunsafeˉvalue = 48` rejects ordinary construction or field observation of an exact compiler-owned Foundation unsafe identity, without renumbering retained values.

## Typed lowering rules

The phase currently lowers:

- `unit`, `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `f32`, `f64`, `rune`, `bool`, `text`, `bytes`, record, enum, variant, sequence, and local builder values, plus return-only `never` control evidence;
- literals including edition-1 `()`, storage-free typed constants, parameters, explicitly typed or initializer-inferred locals, simple or compound assignment, data length/load, positional or named record construction, named variant construction, aggregate fields, enum members, Foundation intrinsics, noncapturing named-function values, functions, and declared capabilities;
- checked arithmetic including division/remainder, fixed-width bitwise/shift operations, comparison, exact scalar/enum/text/bytes equality, short-circuit Boolean conjunction/disjunction, boolean negation, and signed negation;
- exhaustive enum/variant match, named variant-field destructuring, variant construction/case tests/field extraction, builder creation/push/freeze, sequence length/index, and `for` lowering;
- expression statements, exact `try` propagation, semantic `using`, `return`, lexical blocks, lexical unsafe statement and value blocks, statement and value-producing `if`/`else if`/`else` and exhaustive enum/variant `match`, `while`, `for`, `break`, and `continue`;
- exact local callable invocation, canonical Foundation unsafe scratch
  construction and observation, checked mutable write-region borrowing,
  contained write-pointer derivation, typed Foreign calls, plus explicit jump,
  branch, and return terminators.

Shape `0` remains Seed's return-only `void`. Shape `9` is the ordinary edition-1
`unit` value and shape `10` is edition-1 `never`, valid only as a function result.
`Unitˉconstant = 163` produces one shape-`9` temporary for `()`, `return;`, and
implicit unit fallthrough. A call returning `never` emits the physical call with
shape zero, closes its current block with a self-loop, and returns logical
shape-`10` evidence to the enclosing expression; no shape-`10` temporary exists.
A non-returning expression satisfies any expected result position and makes
following source unreachable. Shapes `1` through `6` are `i32`, `u8`, `u32`,
`bool`, `text`, and `bytes`; `7` and `8` are `i64` and `u64`; `11`, `12`, and
`13` are `i8`, `i16`, and `u16`; `14` and `15` are `f32` and `f64`; shape `16`
is `rune`. Record shapes start at
`65536`; enum shapes start at `131072 + RecordCount`; variant shapes start at
`196608`. Exact singleton capability-reference shapes are `268435456 +
RootCapabilityDirectoryEntry`. Packed high families retain sequence/builder
element identity and maximum. Nominal suffixes are canonical WVSD nominal
indices; Foundation generic values do not reserve another packed shape family.

Main analysis may additionally retain private WVGT shapes
`0x80000000..0x800000ff` in function returns, parameter/local operations, and
temporary evidence. Such a shape is valid only when its zero-based instance is
present in the exact WVGT catalog embedded by the paired WVLB 1.3 directory.
The catalog selects the even WVIR minor in the current `1.9` through `1.32`
family; it is not a runtime identity. Source WVB must materialize and replace
every private shape before publishing bytecode.

WVFT callable shapes occupy the adjacent compiler-private range
`0x80000100..0x800001ff`. WVIR never serializes the complete WVFT or WVCF
directories. When a function value is lowered, WVIR 1.17/1.18 appends one
reduced `WVIC 1.0` catalog derived from and checked against those directories.
Each retained instance contains only the portable profile, result shape, and
ordered by-value parameter shapes admitted by this executable checkpoint.

Each result-producing operation receives the next function-local temporary ID. Operands may refer only to earlier temporaries in the same function. Basic-block IDs are function-local and canonical in construction order. WVIR 1.9 function entries align one-for-one with WVSD declaration entries; non-function declarations have all-zero function entries. WVIR 1.10 retains those positions, leaves a generic declaration's source position as an all-zero placeholder, and appends concrete specialization entries after the complete WVSD directory in WVGC catalog order. WVIR 1.11 is the corresponding non-specialized directory when operation `171` or `172` is present; WVIR 1.12 combines either operation with the 1.10 specialization envelope. WVIR 1.13 is the non-specialized directory when operation `173` is present, and WVIR 1.14 combines append with the specialization envelope. WVIR 1.15 is the non-specialized directory when operation `175` is present, and WVIR 1.16 combines growth with the specialization envelope. WVIR 1.17 is the non-specialized callable directory, and WVIR 1.18 combines callable evidence with the specialization envelope. WVIR 1.19 is the non-specialized plain-capture environment directory, and WVIR 1.20 combines that evidence with the specialization envelope. WVIR 1.21 is the non-specialized structured-task directory, and WVIR 1.22 combines structured-task operations with the specialization envelope. WVIR 1.23 is the non-specialized Foundation unsafe-scratch construction directory, and WVIR 1.24 combines operation `186` with the specialization envelope. WVIR 1.25 is the non-specialized immutable scratch-observation directory, and WVIR 1.26 combines operation `187` with the specialization envelope. WVIR 1.27 is the non-specialized mutable write-region-borrowing directory, and WVIR 1.28 combines operation `188` with the specialization envelope. WVIR 1.29 is the non-specialized contained write-pointer directory, and WVIR 1.30 combines operation `189` with the specialization envelope. WVIR 1.31 is the non-specialized typed-Foreign-call directory, and WVIR 1.32 combines operation `190` with the specialization envelope. Operation `174` is valid in the lowest family member otherwise selected by the module; it does not introduce another feature envelope. WVIR 1.1 through 1.8 are rejected rather than retained through a parallel decoder.

## WVIR 1 binary directory

All integers are unsigned little-endian and the directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVIR` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `9` through `32` selected by the features below |
| 8 | 4 | Function-entry count |
| 12 | 4 | Function-entry size `48` |
| 16 | 4 | Block count |
| 20 | 4 | Block-entry size `28` |
| 24 | 4 | Operation count |
| 28 | 4 | Operation-entry size `28` |
| 32 | 4 | Temporary count |
| 36 | 4 | Temporary-entry size `4` |
| 40 | 4 | Operand count |
| 44 | 4 | Operand-entry size `4` |

WVIR 1.17, WVIR 1.19, WVIR 1.21, WVIR 1.23, WVIR 1.25, WVIR 1.27, WVIR 1.29, and WVIR 1.31 append function-type-catalog byte length and
catalog-layout version `1` at offsets 48 and 52, so function entries begin at
offset 56. WVIR 1.18, WVIR 1.20, WVIR 1.22, WVIR 1.24, WVIR 1.26, WVIR 1.28, WVIR 1.30, and WVIR 1.32
first retain specialization count/version at offsets 48 and 52, then append
function-type-catalog byte length/version at offsets 56 and 60, so function
entries begin at offset 64. Sections follow in their exact order, and the WVIC
catalog follows the operand section with no padding when its declared length is
nonzero. The task and unsafe-memory pairs retain the function-type-catalog length/version fields
even when the length is zero; callable operations still require one complete
nonempty catalog.

Source without an admitted generic instance or any memory, append, growth,
callable, closure-environment, structured-task, or unsafe-memory feature uses that exact
48-byte WVIR 1.9 header. Specialized source without either memory
operation publishes WVIR 1.10. Either memory operation without specialization publishes WVIR 1.11
with the same 48-byte header and section positions as 1.9. A module containing
specialization and either memory operation publishes
WVIR 1.12 with the specialization envelope of 1.10. Append without specialization
publishes WVIR 1.13; append with specialization publishes WVIR 1.14. Growth
without specialization publishes WVIR 1.15; growth with specialization
publishes WVIR 1.16. A callable function reference or indirect call without
specialization publishes WVIR 1.17; the same feature with specialization
publishes WVIR 1.18. A `Closureˉcreate` environment without specialization
publishes WVIR 1.19; the same feature with specialization publishes WVIR 1.20.
Any operation `180` through `185` without specialization publishes WVIR 1.21;
the same structured-task vocabulary with specialization publishes WVIR 1.22.
Operation `186` without specialization publishes WVIR 1.23; the same operation
with specialization publishes WVIR 1.24. Operation `187` without specialization
publishes WVIR 1.25; the same operation with specialization publishes WVIR
1.26. Operation `188` without specialization publishes WVIR 1.27; the same
operation with specialization publishes WVIR 1.28. Operation `189` without
specialization publishes WVIR 1.29; the same operation with specialization
publishes WVIR 1.30. Operation `190` without specialization publishes WVIR
1.31; the same operation with specialization publishes WVIR 1.32. The
Foreign-call pair may also carry earlier features. The unsafe-memory pairs may also carry
structured-task operations and retain the same function-type-catalog header,
including a zero-length catalog when no callable instance is present.
The earlier even versions 1.10, 1.12, 1.14, and 1.16 append the
specialization count at offset 48 and specialization-layout version `1` at offset
52, and begin the function section at offset 56. Their function-entry count is
exactly `WvsdEntryCount + SpecializationCount`; the count must equal the valid
WVGC instance count embedded in the paired WVLB 1.2. All section entry layouts
remain unchanged. A 1.11/1.12 directory must contain operation `171` or `172` and
must not contain `173` or `175`. A 1.13/1.14 directory must contain operation
`173` and must not contain `175`. A 1.15/1.16 directory must contain operation
`175`. A 1.17/1.18 directory must contain operation `177` or `178` and one
complete reduced callable catalog. A 1.19/1.20 directory must contain operation
`179` and one complete reduced callable catalog. A 1.21/1.22 directory must
contain at least one operation `180` through `185`; lower minors must not contain
those operations. A 1.23/1.24 directory must contain operation `186` and must
not contain operation `187`, `188`, or `189`; lower minors must not contain any
of those operations. A 1.25/1.26 directory must contain operation `187`, may
also contain operation `186`, and must not contain operation `188` or `189`. A
1.27/1.28 directory must contain operation `188`, may also contain operations
`186` and `187`, and must not contain operation `189`. A 1.29/1.30 directory
must contain operation `189` and may also contain operations `186` through
`188`. A 1.31/1.32 directory must contain operation `190` and may also contain
earlier operations.

Each 48-byte function entry contains twelve `u32` fields: module, first block/count, first operation/count, first temporary/count, first operand/count, parameter count, local count, and return shape.

Each 28-byte block entry contains seven `u32` fields: block ID, first operation/count, terminator, value temporary, first target, and second target. The owning function and module are derived from the enclosing canonical function range. The sentinel `4294967295` represents an absent value or target.

Each 28-byte operation entry contains owning block `u32` at offset `0`, operation
kind `u16` at offset `4`, operand count `u16` at offset `6`, result shape `u32`
at offset `8`, result temporary `u32` at offset `12`, first operand `u32` at
offset `16`, target `u32` at offset `20`, and auxiliary value `u32` at offset
`24`. Operation kind and operand count are independently bounded before their
narrow representation is published. The owning module and function are derived
from the canonical function range. Source failures retain their exact location
while WIR is being constructed. A persisted operation does not repeat a source
span that emission does not consume.

The temporary section is a sequence of result shapes. The operand section is a sequence of function-local temporary IDs.

The appended WVIC catalog begins with ASCII `WVIC`, major `1`, minor `0`, a
`u32` instance count, and a `u32` total byte length. Each instance contains a
`u32` profile, `u32` result shape, `u32` parameter count, then one `u32` shape
per parameter. There are 1 through 256 instances, no instance has more than 64
parameters, and retained WVIC evidence is at most 131,072 bytes. First callable
use assigns identities in ascending order; every instance is used. Complete
WVFT flags, transfer modes, and effect masks are not erased accidentally: this
checkpoint admits an instance only when flags, result mode, every parameter
mode, and both resolved effect masks are zero.

## Operation families

Operation values `1` through `63` retain the prior constants, storage, Foundation, nominal, scalar, and call contract. `Valueˉphi = 64` joins two exact same-shape values selected by control flow; its earlier Boolean short-circuit use remains the shape-`4` specialization. Values `65` through `67` are variant create/test/legacy one-field payload. Values `68` through `72` are builder create/push/freeze and sequence length/element. Values `73` through `92` cover `i32`/`u8`/`u32`, text, and bytes operations. Values `93` and `94` are `i64` and `u64` constants; `95` and `96` are their formatting intrinsics; values `97` through `119` are wide arithmetic, comparison, division, and remainder; values `120` through `125` are `u64` bitwise, complement, and shift operations; values `126` and `127` are exact little-endian `u64` byte read and construction; value `128` is lossless `u32` to `u64` conversion; and values `129` through `147` are the typed fixed-integer constant, checked arithmetic, comparison, signed negation, `u16` bitwise, and `u16` shift family. The operation's shape selects exactly `i8`, `i16`, or `u16`; comparisons produce `bool` while retaining the operand shape in `Target`, and shifts require a `u32` right operand. Values `148`, `149`, and `150` are rune constant, equality, and inequality. A rune constant has shape `16`, no operands, and its exact scalar in `Target`; comparisons consume two shape-`16` values and produce `bool`. Values `151` through `162` are the `f32`/`f64` constant, arithmetic, negation, and comparison family. Value `163` is `Unitˉconstant`: it has shape `9`, no operands, and zero target and auxiliary fields. Value `164` is `Variantˉfield`: it consumes one exact nominal variant, stores the canonical variant index in `Target`, packs `case * 64 + field` in `Auxiliary`, and produces that field's exact shape. Values `165` through `170` retain the fixed-array and exact Foundation Sequence/Vector operations. Value `171` is `Foundationˉmemoryˉsplit`; value `172` is `Foundationˉvectorˉconstructˉreserved`. Both are defined below. Value `0` is invalid in published evidence.

Value `173` is `Foundationˉvectorˉappend`, defined below with the same exact
Foundation identity discipline. Value `174` is `Releaseˉlocal`, the explicit
compiler boundary for semantic resource cleanup defined below. Value `175` is
`Foundationˉvectorˉgrowˉreserved`, defined below. Value `176` is
`Platformˉsourceˉlength`, the exact hosted source-snapshot observation defined
below. Value `177` is `Functionˉreference`; value `178` is `Callˉindirect`; and
value `179` is `Closureˉcreate`. Values `180` through `185` are the
structured-task construct, operation-context, spawn, await, cancel, and
scope-exit family. Value `186` is
`Foundationˉunsafeˉconstructˉscratch`, and value `187` is
`Foundationˉunsafeˉscratchˉlength`; both are defined below. Values `188` and
`189` are mutable write-region borrowing and contained write-pointer
derivation. Value `190` is the typed Foreign call defined below.

The numeric mapping is frozen by `Compilerˉsourceˉwirˉoperation` and verified by the focused demo. Adding an operation requires updating its result shape, operand arity and shapes, target/auxiliary contract, demo coverage, this specification, and both native qualification scripts.

### Foundation unsafe scratch construction

Operation `186` is selected only by an explicitly generic qualified call to
the exact edition-1 System module `Foundationˉunsafe` member
`Constructˉscratch::<Abi>`. `Abi` must resolve to one declared enum identity.
The contextual result must be the exact canonical
`Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>` identity from the
canonical Foundation result and unsafe modules. A lookalike module or nominal
type cannot acquire this operation.

The call takes the named by-value arguments `Budget`, `Length`, and `Alignment`.
`Budget` must be one available exact `Memoryˉbudget` slot. `Length` and
`Alignment` each produce an earlier shape-`8` (`u64`) temporary; they are the
operation's two ordered operands. The result is the private generic result
shape, `Target` is the budget slot, and `Auxiliary` is the exact enum shape for
`Abi`. The operation contributes `memory.allocate`. Positive length,
power-of-two alignment, budget capacity, provider allocation, zeroing, and
addressability are runtime result conditions rather than compile-time literal
restrictions.

Independent validation reconstructs the exact Foundation generic identities,
requires two `u64` operands, proves the budget slot is in range and available,
proves the ABI auxiliary is a declared enum, and treats the result as affine
owned evidence. Ordinary record construction and field observation of the
opaque scratch identity remain invalid. The source-WVB backend maps an
otherwise valid operation `186` to WVB 1.33 opcode `DC`, retaining the budget,
result, and ABI identities as exact instruction immediates. The complete
bytecode verifier, bounded scalar provider, and native x86-64 backend admit the
implemented construction subset.

### Foundation unsafe scratch length

Operation `187` is selected only by an explicitly generic qualified call to
the exact edition-1 System module `Foundationˉunsafe` member
`Scratchˉlength::<Abi>`. It requires one named `Scratch` argument written as
an immutable borrow of a directly named local whose exact generic identity is
`Foreignˉscratch<Abi>`. `Abi` must be one explicit declared enum identity and
must match the scratch type argument. The contextual result is exact shape `8`
(`u64`), and the operation contributes no effect.

The operation has zero operands. `Target` is the borrowed scratch parameter or
local slot, and `Auxiliary` is the exact ABI enum shape. Independent validation
reconstructs the canonical generic scratch identity, checks the ABI relation,
requires the target in the current function, and preserves the affine owner.
WVIR 1.25/1.26 require at least one operation `187`; operation `186` may also be
present. The source-WVB backend maps `187` to WVB 1.35 opcode `DD`, preserving
the scratch-local and ABI identities as instruction immediates.

### Foundation unsafe mutable write-region borrowing

Operation `188` is selected only by an explicitly generic qualified call to
the exact edition-1 System module `Foundationˉunsafe` member
`Borrowˉwriteˉregion::<Abi>`. The call must occur inside a lexical unsafe
context and takes the named arguments `Scratch`, `Start`, `Length`, and
`Requiredˉalignment`. `Scratch` is a mutable borrow of one directly named
`Foreignˉscratch<Abi>` parameter or local; the remaining arguments are exact
`u64` values. `Abi` is one explicit declared enum identity matching both the
scratch and result region.

The operation has three ordered operands for start, length, and required
alignment. `Target` is the mutable-borrow scratch slot and `Auxiliary` is the
ABI enum shape. Its result is the exact contextual
`Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>` private generic
shape, and it contributes `unsafe.address`. Independent validation reconstructs
the Result, region, pointer-failure, scratch, and ABI relationships and rejects
an immutable or by-value scratch origin. WVIR 1.27/1.28 require at least one
operation `188`; earlier unsafe-scratch operations may also be present.

This checkpoint does not expose a pointer or execute the borrow. WVB encoding,
affine region lifetime containment, runtime/provider checks, native lowering,
and authenticated Foreign calls remain separate later boundaries.

### Foundation unsafe contained write-pointer derivation

Operation `189` is selected only by an explicitly generic qualified call to
the exact edition-1 System module `Foundationˉunsafe` member
`Writeˉpointer::<Abi>`. The call must occur inside a lexical unsafe context and
take the named argument `Region` as an immutable borrow of one directly named
parameter or local with exact type `Foreignˉwriteˉregion<Abi>`. `Abi` is one
explicit declared enum identity matching both the region and result pointer.

The operation has zero operands. `Target` is the region parameter or local
slot, `Auxiliary` is the exact ABI enum shape, and its result is the exact
contextual `Foreignˉpointer<u8, Abi>` private generic shape. It contributes
`unsafe.address`. Independent validation reconstructs the canonical pointer
and region identities, their opaque one-`u64` layouts, the `u8` element, and
the ABI relationship. WVIR 1.29/1.30 require at least one operation `189`;
earlier unsafe-memory operations may also be present.

Candidate WVB 1.37 serializes operation `189` as the non-executing
`unsafe.write-pointer.borrow` instruction after compiler-aligned affine
validation. Execution consumers still reject WVB 1.37, so this checkpoint forms
no machine address. Provider representation, native address formation,
authenticated no-retain Foreign calls, and ordinary generic-call inference
remain separate later boundaries.

### Typed Foreign call

Operation `190` is selected only for a resolved Foreign declaration called
inside a lexical unsafe context. It consumes exactly three by-value operands in
declaration order: the exact `Foundationˉunsafe.Foreignˉpointer<u8, Abi>` value,
an exact `u64` capacity, and an exact `u64` expected generation. Its result is
exact `i64`. `Target` is the declaration's canonical WVSD Foreign entry and
`Auxiliary` is zero.

Independent validation reconstructs the declaration signature from the source
graph, requires a kind-`9` WVSD target with three parameters, and verifies the
canonical Foundation pointer identity, its one-`u64` opaque layout, the `u8`
element argument, the shared explicit ABI enum, the two `u64` scalars, and all
by-value modes. WVIR 1.31/1.32 require at least one operation `190`; lower
minors reject it.

WVIR alone does not authenticate the declaration's native symbol, ABI spelling,
no-retain promise, or no-unwind promise. Production consumption pairs the
operation with the exact retained WVFB record already authenticated for the
same source module and declaration. Only that retained coordinator relationship
may invoke the private emitter form, which independently rechecks WVFB against
WVSD and WVIR and serializes operation `190` as candidate WVB 1.38 opcode `E0`.
The WVB carries registered binding identity `1`, the exact pointer-record type,
and matching ABI enum; it is not an authentication certificate. Complete WVB
verification, provider execution, and native ABI invocation remain later
checkpoints.

## Independent validation

`Compilerˉsourceˉwirˉdirectoryˉisˉvalid` verifies:

- magic, selected 1.9 through 1.32 version, exact feature-to-minor correspondence, fixed entry sizes, bounded counts, exact section offsets, and exact total length;
- canonical function ranges aligned with WVSD and WVLB, including generic placeholders, appended catalog-order specializations, parameter/local counts, and substituted source return shapes;
- canonical block IDs and ownership, gap-free operation coverage, valid targets, and terminator value types;
- operation ownership and kind, result shape, temporary sequencing, and operand sequencing;
- prior-temporary use, local slots, inferred-local establishment by a non-void first store, consistent later local loads/stores, data and nominal identities, field/member/case indices, variant field counts and exact field shapes, collection descriptors, builder transitions, call targets, arity, dynamic parameter/result shapes, ordinary unit values, and return-only never shapes;
- value-phi placement as the first operation of its join block, two distinct valid predecessor blocks, two exact same-shape operands owned by those predecessors, a result of that same non-void/non-never shape, an unconditional jump from both predecessors to the join, and no branch or third predecessor targeting that join; and
- rejection of trailing bytes and corrupted function, block, operation, temporary, or operand entries.

Construction uses function-private payloads. At most 16 consecutive completed
functions are combined before one bounded publication into the global WIR and
binding payloads; a generic placeholder, non-function declaration, module end,
or the batch limit flushes the current group. Callable lookup uses the
deterministic target-module-and-name WVSI 1.2 index over absolute WVSS spans.
Canonical record/enum shapes and directory identities use the private WVSI
bidirectional nominal tables rather than repeated ordinal rescans.

The first WIR planning pass is retained rather than discarded. Its ordinary
functions and generic placeholders become the canonical base. Concrete generic
functions are then appended in WVGC order beginning at the first not-yet-built
instance. If compiling one specialization discovers another, only that new
catalog suffix is compiled in the next bounded round. Closures are appended
only after the specialization catalog stabilizes. If closure lowering discovers
another generic instance, its partial closure payload is discarded while its
discovery catalogs are retained; the new specialization is appended before the
closure suffix is rebuilt. The existing 32-round limit remains authoritative.

Parameter/local WVLB evidence and typed WVIR are constructed in the same successful-path statement traversal. A local initializer is lowered before its declaration becomes visible; an omitted annotation takes that initializer's exact non-void shape, and the resolved growing binding state is carried through nested blocks. The independent validator can consume standalone WVLB 1.1 evidence by establishing each shape-`0` inferred local from its first verified store and requiring all subsequent accesses to agree. For specialized WVLB 1.2/WVIR 1.10 it additionally validates the embedded catalog substitution and maps every specialized call target back to its source declaration before checking arity and dynamic operand/result shapes. If typed lowering fails, the local-only and complete binding passes remain diagnostic oracles so established binding failures retain precedence.

A bare required capability name emits the existing `U32ˉconstant = 3` operation
with its exact internal capability-reference result shape and zero target and
auxiliary fields. Calling a local of that shape resolves the root capability
directory entry and emits the existing `Callˉcapability = 63` operation. The
validator accepts the custom shape only when it names an actual required root
capability and rejects capability shapes in records, variants, and collections.

The bounded generic-collection checkpoint structurally matches one formal
`sequence<Type, Maximum>` or `builder<Type, Maximum>` against the concrete
argument descriptor. Family, element shape, and maximum are separate evidence:
the families must match, repeated type or maximum contributions must be equal,
and an explicit constant argument must have the declaration's exact fixed-
integer width. Each selected specialization substitutes one concrete collection
descriptor before ordinary body lowering. Its WVIR function body therefore
contains only the same collection shape and operations as a hand-written
monomorphic function. Multiple bodies use the WVIR 1.10 directory envelope
above; no WVIR operation value changes.

The call-scoped borrow checkpoint rereads each validated function signature and
retains a compile-time mode for the selected parameter and result: by value,
immutable borrow, or mutable borrow. These modes are bounded `u32` compiler
facts, not nominal program types, and are erased before WVIR publication. A call
requires the actual and formal modes to agree. The one exception is the frozen
read-through rule: an explicit borrowed actual may satisfy a by-value formal
only when conservative classification proves the value Copy or shared
immutable. Scalars and enums are Copy; `text`, `bytes`, and immutable sequences
are shared; builders and capability-shaped values are conservatively owned; an
unproven aggregate remains unknown.

A direct name or field argument rooted in a parameter derives its actual mode
from that parameter's declaration offset already carried by the transient local
match. This is a bounded local lookup; it does not rescan the complete current
signature for every call. An owned or unknown aggregate therefore cannot pass a
by-value position through a bare borrowed parameter.

An explicit mutable borrow currently requires a direct `var` local or a
parameter already declared `borrow mut`. A standalone borrow expression cannot
be stored or returned and fails with `Invalidˉborrow`; borrow formation is
accepted only while compiling one call argument. Borrowed result signatures are
parsed but rejected until the one-owner provenance rule is represented and
validated. That call-scoped checkpoint by itself did not claim move
invalidation, overlapping-borrow lifetime analysis, aggregate-derived
ownership, resource cleanup, or the complete Slice 5 checker. It added no WVIR
operation, WVB opcode, runtime object, or serialized borrow record. The exact
owned-Vector checkpoint below now adds one bounded move proof without changing
those representation decisions.

A concrete generic-function specialization may use one applied generic nominal
whose arguments are direct function type or constant parameters. Signature,
parameter, explicit-local, construction, return, and field analysis resolve
that use against the owning WVGC instance and the shared WVGT catalog before
ordinary lowering. Generic-call inference may decompose an actual WVGT identity
only when the formal names the exact same generic declaration and full arity;
each formal argument then contributes through the existing WVGS equality and
kind rules. A different template or conflicting contribution fails with
`Genericˉresolution`. The specialized body contains only concrete ordinary or
private catalog shapes, and validation reconstructs both catalogs before
accepting its signature and operations.

An edition-1 accepted `try` evaluates its expression once and requires the exact
Foundation `Result<T, E>` identity. The current function must return
`Result<U, E>` with the same exact substituted error shape. Lowering emits the
general variant case test and branch. The success block selects `Valid.Value`
as `T` through `Variantˉfield = 164`; statement
`try` discards that extracted value. When `T` equals `U`, the failure block
returns the original result temporary. Otherwise it extracts `Failure.Error`
through the same field operation and constructs the exact materialized
`Result<U, E>.Failure` before returning. Different
error shapes are rejected and require an explicit source adapter. The retained
descriptorless Seed statement subset still accepts its prior concrete nominal
shape. No inferred conversion, hidden call, new WVIR operation, or
directory-version change is introduced.

A constant read resolves its WVSD 1.1 entry, reevaluates the validated root declaration under the source-symbol contract, and emits the matching scalar, Boolean, or enum constant operation, including `I64ˉconstant`, `U64ˉconstant`, and `Fixedˉintegerˉconstant`. Wide values carry exact low/high `u32` limbs; fixed signed values carry their exact named-width two's-complement bits in the operation target. No data identity, local slot, or runtime lookup is introduced.

An enum operation carries the exact nominal enum shape and the zero-based source
member identity, not a narrowed copy of the declared integer tag. The symbol
phase has already validated that member against its explicit Language 1.0
backing type. This representation lets matching, equality, and name lookup stay
nominal and lossless for every admitted fixed-integer backing while each output
contract decides which backing representations it can serialize. The current
WVB writer accepts executable exact `i32`- and `u8`-backed enums and rejects any
retained nominal use of another backing before publishing bytecode; WIR does
not truncate a wider tag to fit that boundary. Optimized emission
may omit the complete nominal declaration family only when a full WIR-closure
scan proves that no nominal shape is used anywhere.

A named record literal resolves one accessible record, lowers each field expression left to right in source order, rejects unknown, duplicate, missing, or mismatched fields, and places the resulting temporary IDs into declaration-order operands before emitting the existing `Recordˉcreate = 17` operation. For an applied generic record target, typed lowering admits the complete type through the paired WVGT catalog, reconstructs and independently validates its substituted record layout, and gives the operation that private shape as both result and target; `Auxiliary` retains the template's WVSD declaration identity. A field read from that value emits the existing `Recordˉfield = 18` with the private receiver shape as target and the zero-based substituted field identity in `Auxiliary`. Its result is the field's exact substituted shape. Validation requires the private instance, declaration, record kind, layout, operand arity, field identity, and result shape to agree with the paired catalog.

The exact edition-1 System module `Foundationˉunsafe` reserves four
compiler-owned generic nominal identities:
`Foreignˉpointer<T, Abi>`, `Nullableˉforeignˉpointer<T, Abi>`,
`Foreignˉscratch<Abi>`, and `Foreignˉwriteˉregion<Abi>`. Typed lowering
recognizes one only when its module, edition, profile, name, generic arity, valid
one-field record layout, and exact `Opaqueˉidentity: u64` carrier agree.
Named construction and direct or chained field observation then fail with
`Invalidˉunsafeˉvalue`; no `Recordˉcreate` or `Recordˉfield` operation is
published. The same spelling and layout in another module remains an ordinary
record. This is a source opacity boundary, not an address, lifetime, capability,
serialization, Foreign-call, or native-ABI representation contract. Ordinary
source has no producer or observer until the separately specified compiler
intrinsics are implemented.

A Language 1.0 record update first lowers its exact same-nominal base once, lowers each uniquely named replacement left to right, extracts every unreplaced declaration-order field from that one base temporary with `Recordˉfield = 18`, and emits the same `Recordˉcreate = 17` operation. Field extraction is storage-only and occurs after the source-ordered replacement evaluations; it adds no user-visible evaluation. No new WVIR operation, value representation, or WVB opcode is introduced. Recursive `else if` lowers through the existing conditional blocks and terminators.

An edition-1 variant case has zero through 64 uniquely named fields. Named
construction evaluates every supplied expression left to right exactly once,
rejects unknown, duplicate, missing, or mismatched fields, reorders only the
result temporary identities to declaration order, and emits `Variantˉcreate =
65` with exactly that many operands. A no-data case uses the explicit source
construction braces and emits zero operands. The older positional spelling and
`Variantˉpayload = 67` remain the descriptorless Seed one-field path.

A named variant match pattern must name every declared field exactly once; names
may appear in any order and `_` discards without creating a binding. Each other
binding is immutable and scoped to its arm. Lowering first emits the retained
case test and branch, then emits `Variantˉfield = 164` for each bound field with
the exact variant operand, nominal index, packed case/field identity, and result
shape. WIR remains version 1.1 because operation identities are already an
explicit field of its bounded directory; independent validation rejects a bad
case, field, arity, operand nominal, packed identity, or result type.

For an applied generic variant construction, the type arguments follow the
selected case: `Outcome.Value<Point> { Item: Value, Attempts: Count }`. Typed
lowering binds the owning `Outcome<Point>` through WVGT, reconstructs and
independently validates its complete substituted case layout, and emits
`Variantˉcreate = 65` with the private instance shape as both result and target.
The case identity remains its declaration-order index. A match selector already
has one exact concrete variant shape, so a pattern names `case Outcome.Value`
without repeating type arguments. Lowering validates that the pattern's
template and case belong to that selector, then emits `Variantˉcase = 66` and
`Variantˉfield = 164` with the same private target. Bound fields receive their
exact substituted shapes. The independent validator reconstructs the same WVGT
layout and rejects a mismatched template, case, field, arity, packed identity,
operand, or result. No unresolved generic parameter or runtime lookup enters
published WIR or WVB.

A dotted local record path emits one `Recordˉfield = 18` operation per segment
in source order. Each intermediate result must retain an exact record nominal
shape; a scalar or enum before the final segment is an invalid field target.
Unknown members are diagnosed against the owning intermediate nominal type.

An edition-1 value-producing `if` requires `else`. Its condition is lowered once and must have Boolean shape. Each braced arm lowers zero or more ordinary statements followed by one final expression without a semicolon; an `else if` is the recursive value form. Reachable arms must produce the same exact shape, and only the selected arm is reached at runtime. Their results join through `Valueˉphi = 64`; arm-local binding evidence is retained with lexical scope ending at that arm's closing brace. A `never` arm contributes no value and the surviving reachable arm flows through the join without an invented conversion or temporary.

An edition-1 value-producing `match` lowers its selector exactly once and
requires the same exhaustive, duplicate-free, exact-nominal enum or variant case
set as statement `match`. Named variant fields bind immutably only inside their
selected arm. Each arm is a value block, reachable arm shapes must agree
exactly, and pairwise `Valueˉphi = 64` joins carry the selected value. A `never`
arm contributes no value. Descriptorless Seed rejects the value form, while its
statement match remains unchanged. No new operation or WVIR version is needed.

`&&` and `||` lower the left operand, branch to either a short-result block or a right-operand block, and join those Boolean values with `Valueˉphi`. The right expression therefore has no operation or runtime behavior on the skipped path. The operation records the short and right predecessor identities so independent validation does not infer phi ownership from layout alone.

Borrow-mode checking classifies the canonical WVGT kind-11 `Vector<T>` identity
as owned and kind-12 `Sequence<T>` as shared immutable. A borrowed sequence may
therefore satisfy a by-value read-through position without consuming its shared
backing, while a borrowed vector cannot satisfy a consuming by-value position.
The classification consumes the validated generic-type catalog; a private shape
without matching evidence is not inferred from its numeric range.

Independent WVIR validation applies a second bounded ownership proof to every
function containing an exact kind-11 `Vector<T>` or a record, variant, or fixed
array that recursively contains one. Owned parameters begin available,
borrowed parameters begin non-owning, and explicit owned locals begin
unavailable until one unique value is stored. A local load records its exact
source-slot provenance. A by-value ordinary call, unique local store, or owned
return consumes the temporary and its source slot; an immutable or mutable
borrowed call observes the value without consuming that slot. Aggregate
construction consumes every owned field or element. Formal modes are
reconstructed from the validated source declaration and bindings because
call-scoped borrow syntax remains erased from WVIR.

The proof admits at most 64 blocks, 64 parameter/local slots, and 4,096
operations. Owned temporaries cannot escape their producing block and an owned
`Valueˉphi` is not yet admitted. A forward join retains an owned or borrowed
slot state only when every incoming state is identical; an asymmetric move
therefore makes the slot unavailable. A backward edge is accepted only when
its complete slot state exactly equals the already established loop-header
state. This bounded fixed-point rule rejects ownership drift per iteration,
borrow-after-move, duplicate transfer, and post-join reuse while admitting
borrow-then-transfer, owned results and returns, equal transfers on forward
paths, and ownership-invariant loops.

Recursive aggregate classification follows exact validated nominal layouts and
is bounded to 64 ancestor types. It covers concrete and specialized generic
records and variants plus fixed arrays; the source nominal contract remains
bounded and acyclic, and malformed layout evidence rejects independently. The dedicated affine
`Result<Memoryˉbudget, ...>` and `Result<Vector<T>, ...>` paths produced by
operations `171` and `172` retain their existing proof instead of becoming a
second overlapping aggregate owner.

Reading a field or array element observes the parent aggregate and creates
borrowed temporary evidence; it does not move the parent. If the selected field
itself is owned, that result remains non-owning and cannot satisfy a by-value
consumer. An explicit mutable field borrow is accepted only when the parent is
a directly resolved mutable binding. Moving a field out of an owned aggregate,
updating an aggregate while ownership could remain hidden in its old value, or
using the parent after a whole-value move rejects before publication. The first
profile does not admit borrowed aggregate parameters, borrowed aggregate
results, owned phis, or user-defined destruction.

The analyzer may publish provisional WVIR evidence; the emitter independently
reconstructs the recursive ownership classification. WVB 1.26 serializes exact
Vector parameter modes as value shape `23`, immutable-borrow shape `26`, or
mutable-borrow shape `27`. WVB 1.28 keeps whole aggregate parameter and result
shapes ordinary and uses local-only borrowed view shapes for field and element
observation. The WVIR proof remains the source-slot provenance authority; WVB
does not add a source slot, pointer, or borrow handle.

The private compiler shape `805306368` represents only the exact edition-1
`Foundationˉmemory.Memoryˉbudget` identity and is classified as owned. An
explicit immutable borrow may satisfy an observing borrowed parameter, while a
borrowed budget cannot satisfy a consuming by-value parameter. Source cannot
construct this value or substitute a forgeable record for it. The numeric shape
is private compiler evidence rather than a Language ABI value.
WVB 1.34 serializes an immutable borrowed call boundary as shape `36` while
retaining shape `25` for the affine owner. That view shape is not a second WVIR
budget identity: the writer derives it only for a proven borrowed call operand,
and the verifier confines it to the canonical direct-call sequence. Mutable
budget borrowing remains source/WVIR evidence only until a write-through WVB
and native alias contract exists.

The exact `Foundationˉmemory.Split` call has three arguments and is recognized
only through the canonical edition-1 module identity. Its first argument must be
`borrow mut` of one directly named mutable local whose exact shape is
`Memoryˉbudget`; the borrow itself is not serialized. Its second and third
arguments are evaluated left to right as exact `u64 Maximumˉbytes` and `u32
Maximumˉchildren` values. The expected result must be the canonical materialized
`Foundationˉresult.Result<Memoryˉbudget,
Foundationˉmemory.Allocationˉfailure>`. That failure record has exactly three
declaration-order fields: the canonical same-module `Allocationˉreason` enum,
then `Requestedˉbytes: u64`, then `Availableˉbytes: u64`.

Lowering emits `Foundationˉmemoryˉsplit = 171`. Its result is that exact private
Result instance. Its two operands are the already-evaluated `u64` and `u32`
limit temporaries. `Target` is the borrowed parent budget's direct local slot;
`Auxiliary` is the canonical Foundation memory module index. Independent
validation reconstructs the result/failure layout, validates the module and
numeric operand shapes, and performs bounded affine analysis over at most 64
blocks and 64 owned slots. It tracks live budget parameters/locals and moved
budget temporaries, intersects availability at forward joins, requires an exact
slot-state match on every backward edge, rejects duplicate ownership and use
after move, and requires temporary owners to be consumed. WVB 1.23 executes
Split through its separately verified provider accounting contract.

The exact
`Foundationˉcollections.Vectorˉconstructˉreserved::<T>(Budget, Maximumˉitems)`
call requires one explicit type argument and two arguments. `T` must be in the
currently executable resource-free scalar collection subset. `Budget` must be
one directly named available local of exact private `Memoryˉbudget` shape, and
`Maximumˉitems` must be exact `u64`. The expected result must be the canonical
materialized `Foundationˉresult.Result<Foundationˉcollections.Vector<T>,
Foundationˉmemory.Allocationˉfailure>`; neither result-context inference nor a
lookalike failure record is accepted.

Lowering emits `Foundationˉvectorˉconstructˉreserved = 172`. Its result is that
exact private Result instance. Its sole operand is the already evaluated `u64`
maximum-items temporary, `Target` is the consumed budget local slot, and
`Auxiliary` is the canonical Foundation memory module index that owns
`Allocationˉfailure`. Independent validation reconstructs the Result, Vector,
element, and failure identities and consumes the available budget slot in the
same bounded ownership proof used by Split. WVB 1.24 lowers the operation to
opcode `CF`, which carries the consumed budget-local index and exact Result
type. The maximum remains the sole stack operand. The bytecode verifier derives
the Vector type from the Result Valid payload and never serializes the private
lease, provider generation, heap address, or backing representation.

The exact `Foundationˉcollections.Vectorˉappend::<T>(Vector, Value)` call
requires one explicit type argument and two arguments. `T` must be in the
currently executable resource-free scalar collection subset. The first
argument must be `borrow mut` of one directly named mutable non-parameter local
whose exact private shape is `Vector<T>`; the borrow is compile-time evidence
and is not serialized. The second argument is evaluated once as exact `T`. The
expected result must be the canonical materialized
`Foundationˉresult.Result<unit,
Foundationˉcollections.Vectorˉappendˉfailure<T>>`. That failure record has
exact declaration-order fields `Error: Collectionˉfailure` and `Value: T`.

Lowering emits `Foundationˉvectorˉappend = 173`. Its result is that exact
private Result instance, its sole operand is the already evaluated item
temporary, `Target` is the borrowed Vector local slot, and `Auxiliary` is the
exact private `Vector<T>` shape. Independent validation reconstructs the
Vector, element, Result, failure record, and canonical Collection failure
identity. The operation preserves the Vector owner; it neither consumes the
local nor creates a second owner. WVB 1.25 lowers it to opcode `D0`, which
carries the Vector-local and Result-type indices. Capacity refusal is an
ordinary exact Result path that leaves the Vector unchanged and returns the
item; it is not a trap or a partial append.

The exact
`Foundationˉcollections.Vectorˉgrowˉreserved::<T>(Vector, Budget,
Newˉmaximumˉitems)` call requires one explicit type argument and three
arguments. The first is `borrow mut` of one directly named mutable
non-parameter `Vector<T>` local. The second is `borrow mut` of a distinct,
available exact `Memoryˉbudget` parameter or local. The third evaluates once
as exact `u64`. The expected result is the canonical materialized
`Foundationˉresult.Result<unit,
Foundationˉmemory.Allocationˉfailure>`. The current executable checkpoint
admits only the resource-free scalar Vector element subset.

Lowering emits `Foundationˉvectorˉgrowˉreserved = 175`. Its sole operand is
the evaluated new-maximum temporary, `Target` is the Vector local slot, and
`Auxiliary` is the budget slot. Independent validation reconstructs the exact
Vector element, Result, unit payload, and Allocation-failure identity; proves
both slots available; and preserves both owners. A successful runtime operation
replaces the backing and its lease but does not create a second observable
Vector owner. Refusal leaves the Vector and supplied budget state unchanged.
WVB 1.27 lowers the operation to opcode `D1`, carrying Vector-local,
budget-local, and Result-type indices. A requested maximum that is not greater
than the current maximum violates the operation precondition and traps before
allocation rather than returning a misleading success.

Exact Foundation Sequence reads reuse that catalog proof. Operation 167
`Foundationˉsequenceˉlength` consumes one temporary whose shape is the target
kind-12 private Sequence and produces `u64`. Operation 168
`Foundationˉsequenceˉelement` consumes that Sequence plus `u64`, and its result
must equal the catalog's element shape. Both require auxiliary zero and the
resource-free Copy scalar subset accepted by WVB 1.19. For that subset only,
the public `borrow T` result is read and copied as `T`; resource-bearing element
borrows remain rejected until provenance is represented. Independent WVIR
validation reconstructs the same kind and element from WVGT, so a private-range
number, mismatched result, wrong index, or unsupported element cannot publish.

An edition-1 `using Name = Expression Block` evaluates `Expression` exactly once
before `Name` exists. The current semantic resource class is the exact
Foundation kind-11 `Vector<T>` identity; a scalar, shared Sequence, lookalike
record, or other value fails with `Invalidˉresource`. The compiler creates an
immutable binding-kind-`4` local visible only within `Block`, stores the unique
Vector owner into that slot, and emits `Releaseˉlocal = 174` whenever control
leaves its scope through normal fallthrough, `return`, failed `try`
propagation, `break`, or `continue`. Only scopes actually exited are released.
Nested resources are emitted in reverse binding/slot order, so the innermost
owner is released first.

`Releaseˉlocal` has shape `0`, no result temporary, no operands, a direct
non-parameter exact-Vector or `Sourceˉfile` local slot in `Target`, and zero
`Auxiliary`. It consumes the one available owner. Moving or freezing a Vector,
or moving the source resource, before its
implicit release therefore makes the directory invalid rather than turning the
cleanup into a no-op or double release. The operation is a typed compiler
boundary, not a user-callable destructor protocol. WVB lowering uses the
existing `local.take <slot>` followed by `pop`, so no new bytecode opcode or
minor version is required.

The private source shape `805306369` belongs only to the exact imported
`Platformˉfile.Sourceˉfile` identity. It is move-owned and cannot be copied,
stored in a field or collection, returned, or introduced outside the exact
exported `Main(Sourceˉfile) -> i32` parameter. The entry parameter must move
into an inferred semantic-`using` local before use, so every exit retains the
same explicit release proof as other resources.

`Platformˉsourceˉlength = 176` has result shape `8` (`u64`), no value operands,
one direct non-parameter live `Sourceˉfile` local in `Target`, and zero
`Auxiliary`. Source syntax supplies an explicit immutable borrow, but WVIR
serializes only the checked local identity; no pointer, path, handle, provider,
capability call, or runtime byte access is represented. The operation is
accepted within the existing WVIR family and makes WVB 1.29
the required executable output feature.

`Functionˉreference = 177` has one WVFT-private callable result shape, no value
operands, the concrete WVIR function entry in `Target`, and the reduced WVIC
instance in `Auxiliary`. The target must be a nongeneric function with an
explicit empty effect clause, by-value parameters, and a by-value non-`void`,
non-`never` result. Its exact `async` and `unsafe` declaration bits are retained
in the authoritative WVFT instance rather than erased. Constructing or moving
the reference does not invoke the function and therefore does not require an
unsafe context. Independent validation rereads the source signature, bindings,
profile, prepared function entry, WVCF disposition, flags, and authoritative
WVFT instance before accepting the reference.

`Callˉindirect = 178` consumes the callable temporary first and then each
argument in declaration order. `Target` is the reduced WVIC instance and
`Auxiliary` is zero. Its result and every operand shape must exactly match that
instance. A local call does not perform overload selection, inferred conversion,
effect widening, or structural guessing. Invoking an instance whose retained
WVFT flags contain `unsafe` requires a lexical unsafe context at the call site;
the same rule applies to a direct call of an unsafe function or Foreign
declaration.

Unsafe statement and value blocks are source-only lexical evidence. WVIR
construction tracks an internal depth from `0` through `64`, increments it
while lowering an unsafe block, and restores the enclosing depth on every
success or failure path. A new ordinary or synthetic function begins at depth
zero, so an enclosing unsafe block cannot grant ambient unsafe context to a
nested function body. Marking a function declaration `unsafe` classifies its
invocation; it does not make the function body an implicit unsafe context.
Unsafe wrappers add no operation, temporary, operand, block, flag, or WVIR
version. Equivalent safe and explicitly wrapped bodies therefore publish the
same structural WVIR counts. This checkpoint controls invocation visibility;
it is not runtime authority, memory-safety verification, Foreign-call lowering,
or a native containment boundary.

`Closureˉcreate = 179` has one WVFT-private callable result shape and 1 through
64 capture operands in declaration order. `Target` is the concrete physical
function entry and `Auxiliary` is the reduced WVIC instance. The target's
parameters are exactly the copied capture prefix followed by the WVIC public
parameters, and the result is exact. Validation rereads the source declaration,
bindings, profile, flags, generic arity, effect clause, parameter modes, and
result. Captures are limited to shapes `1`, `2`, `4`, `5`, `7`, `8`, `11`
through `16`, and exact enum shapes; text, bytes, aggregates, callables,
resources, move owners, and borrows reject. Source closure-body construction
does not emit this operation yet: WVIR 1.19/1.20 currently establishes the
validated prepared-evidence and WVB-lowering substrate.

Borrowed callable parameters/results, effectful callable values, captured move
or borrow invalidation and escape, callable mutation/escape, and native callable
ABI lowering remain outside this executable checkpoint.

`Compilerˉsourceˉwirˉconsumer` owns the small immutable-summary and signature
helpers shared by downstream WVB compilation. It is a cohesive extraction from
the WVIR implementation to keep compiler closures under native frame and source
evidence limits; it is not a second IR, validator, or semantic path.

`Compilerˉsourceˉwir` owns typed construction,
`Compilerˉsourceˉwirˉvalidation` independently validates persisted WVIR, and
`Compilerˉsourceˉwirˉchecked` composes both for a one-shot public checked
result. The validator may reuse immutable decoding primitives from the consumer
module, but never construction state or producer conclusions. A persisted WVIR
crossing into emission is therefore revalidated without pulling the complete
producer into the emitter executable closure.

`break` closes the current block with a jump to the nearest enclosing loop's after-block. `continue` closes it with a jump to that loop's condition block. Nested loops replace those targets while their bodies are lowered. Compound assignment emits exactly one local load, lowers the right operand, applies the corresponding checked `i32` or `u32` arithmetic operation, and emits one store; an immutable, missing, mismatched, or unsupported target is rejected before publication.

## Verification tiers and current boundary

The fast conformance case compiles the core, runs the semantic/corruption demo, and sends a control-heavy hosted fixture through the real file-reading tool. The fixture produces:

```text
source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=2760
```

The last retained deterministic candidate artifacts before the named
variant-field checkpoint were:

- `Source-Wir-Core.wvb`: 836,098 bytes, SHA-256 `985a03dd51b7599586181ecc9da797fba35ea69f7184ac75104ce402f0d8a542`.
- `Source-Wir-Demo.wvb`: 843,004 bytes, SHA-256 `19441dce68e8b86288662acc4548fc687498e7b2b0d5a24e7a5041c57cdcc62f`.
- `Source-Wir-Tool.wvb`: 834,992 bytes, SHA-256 `e3f3c1abea8ad18e171c13713af5c718f0a2914d1a5ea800f39a03fd525a37f9`.

These historical identities include inferred-local verification, storage-free
typed-constant lowering, named-record remapping, recursive `else if`,
loop-control targets, compound assignment, and structurally verified
short-circuit Boolean phi nodes. They do not identify the current modified
source; refreshed whole-compiler identities require cross-host requalification
before a new qualification claim.

Decision 0518 moves ordinary construction of all three products to the generic
native Project front door and moves exact core inspection to the paired native
front-door helper. The managed demo and hosted-tool executions remain retained
behavior evidence because the scalar native runner stops the demo with code
`3004` and does not bind the tool's hosted capabilities.

The original typed-WVIR candidate was cross-host qualified at `bf77f70`, the fused local-discovery/typed-WVIR implementation at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index implementation at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055's validated-scan reuse implementation is cross-host qualified at `1a4fca7e295545b3b815bbf187fc048f1a885c74`; Decision 0058's exact bootstrap artifact set is cross-host qualified at `5c16547`.

The ten-module compiler closure is intentionally not in the fast loop. Decision 0042 reduced the focused typed-WVIR fixture from 8,074,045 to 5,735,695 instructions; Decision 0050 reduced it again to 5,715,847 and removed directory-entry construction and nominal-rank derivation as dominant costs. Decision 0055's implementation falls to 3,626,693 focused instructions and completes the exact ten-module input in 3,912,239,584 instructions under the unchanged 4,000,000,000 ceiling. That clears the typed-WVIR performance entry gate. Decision 0058's separate dedicated verifier proceeds through WVB and qualifies exact Stage 1 to Stage 2 convergence.

WVIR-to-WVB lowering is specified separately in the initial [source-to-WVB backend contract](Compiler-Source-Wvb.md). WVIR execution, optimization, native IR, and OS-specific lowering are not part of this contract.
