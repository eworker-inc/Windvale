# Windvale typed source IR

## Status and purpose

`Compilerˉsourceˉwir` is the first portable Windvale-written typed lowering phase. It consumes one complete WVSS 1 source graph, reuses validated WVSD symbol evidence and WVLB local evidence, checks expression and control-flow semantics, and publishes canonical `WVIR 1` bytes.

WVIR is a compiler boundary, not executable bytecode. It preserves typed operations, basic blocks, calls, source spans, and stable declaration identities so later passes can lower the same program to WVB or future native and system targets without inheriting C# host behavior.

## Public result

```text
Compilerˉvalidateˉsourceˉwir(Input: bytes)
    -> Compilerˉsourceˉwirˉsummary
```

On success, the summary contains module, function-entry, block, operation, temporary, and operand counts plus an independently validated WVIR directory. On failure, the directory is empty and the summary identifies the first deterministic failure by module, related module, WVSD function entry, byte offset, and one-based line/column.

The status contract distinguishes upstream source-binding rejection, evidence limits, malformed constructed evidence, type mismatch, invalid conditions and returns, missing returns, unreachable statements, invalid data/index/field/operator use, invalid call arguments, invalid local inference, invalid constant evidence, named-record failures, loop-control placement, invalid or non-exhaustive enum/variant matching, unknown variant cases, invalid payload bindings, invalid collection shapes, invalid or consumed builders, an invalid result-propagation contract, invalid unit use, invalid record update, invalid named variant construction, and invalid value blocks. Appended values `23` through `31` own those match, variant, collection, and builder failures, `Invalidˉtry = 32` owns propagation failures, `Invalidˉunit = 33` owns a unit expression outside edition 1, `Invalidˉrecordˉupdate = 34` owns a cross-edition or wrong-nominal-base update, values `35` through `37` own an invalid variant literal plus duplicate or missing variant fields, and `Invalidˉvalueˉblock = 38` owns a malformed or valueless value-producing control arm, without renumbering retained values.

## Typed lowering rules

The phase currently lowers:

- `unit`, `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `f32`, `f64`, `rune`, `bool`, `text`, `bytes`, record, enum, variant, sequence, and local builder values, plus return-only `never` control evidence;
- literals including edition-1 `()`, storage-free typed constants, parameters, explicitly typed or initializer-inferred locals, simple or compound assignment, data length/load, positional or named record construction, named variant construction, aggregate fields, enum members, Foundation intrinsics, functions, and declared capabilities;
- checked arithmetic including division/remainder, fixed-width bitwise/shift operations, comparison, exact scalar/enum/text/bytes equality, short-circuit Boolean conjunction/disjunction, boolean negation, and signed negation;
- exhaustive enum/variant match, named variant-field destructuring, variant construction/case tests/field extraction, builder creation/push/freeze, sequence length/index, and `for` lowering;
- expression statements, exact `try` propagation, `return`, lexical blocks, statement and value-producing `if`/`else if`/`else`, `while`, `for`, `break`, and `continue`;
- explicit jump, branch, and return terminators.

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
element identity and maximum. Nominal suffixes are canonical WVSD nominal indices.

Each result-producing operation receives the next function-local temporary ID. Operands may refer only to earlier temporaries in the same function. Basic-block IDs are function-local and canonical in construction order. Function entries align one-for-one with WVSD declaration entries; non-function declarations have all-zero function entries.

## WVIR 1 binary directory

All integers are unsigned little-endian and the directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVIR` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `1` |
| 8 | 4 | Function-entry count |
| 12 | 4 | Function-entry size `48` |
| 16 | 4 | Block count |
| 20 | 4 | Block-entry size `28` |
| 24 | 4 | Operation count |
| 28 | 4 | Operation-entry size `40` |
| 32 | 4 | Temporary count |
| 36 | 4 | Temporary-entry size `4` |
| 40 | 4 | Operand count |
| 44 | 4 | Operand-entry size `4` |

Sections follow in that exact order.

Each 48-byte function entry contains twelve `u32` fields: module, first block/count, first operation/count, first temporary/count, first operand/count, parameter count, local count, and return shape.

Each 28-byte block entry contains seven `u32` fields: block ID, first operation/count, terminator, value temporary, first target, and second target. The owning function and module are derived from the enclosing canonical function range. The sentinel `4294967295` represents an absent value or target.

Each 40-byte operation entry contains ten `u32` fields: block, operation kind, result shape, result temporary, first operand/count, target, auxiliary value, source byte offset, and source byte length. The owning function and module are derived from the canonical function and block ranges.

The temporary section is a sequence of result shapes. The operand section is a sequence of function-local temporary IDs.

## Operation families

Operation values `1` through `63` retain the prior constants, storage, Foundation, nominal, scalar, and call contract. `Valueˉphi = 64` joins two exact same-shape values selected by control flow; its earlier Boolean short-circuit use remains the shape-`4` specialization. Values `65` through `67` are variant create/test/legacy one-field payload. Values `68` through `72` are builder create/push/freeze and sequence length/element. Values `73` through `92` cover `i32`/`u8`/`u32`, text, and bytes operations. Values `93` and `94` are `i64` and `u64` constants; `95` and `96` are their formatting intrinsics; values `97` through `119` are wide arithmetic, comparison, division, and remainder; values `120` through `125` are `u64` bitwise, complement, and shift operations; values `126` and `127` are exact little-endian `u64` byte read and construction; value `128` is lossless `u32` to `u64` conversion; and values `129` through `147` are the typed fixed-integer constant, checked arithmetic, comparison, signed negation, `u16` bitwise, and `u16` shift family. The operation's shape selects exactly `i8`, `i16`, or `u16`; comparisons produce `bool` while retaining the operand shape in `Target`, and shifts require a `u32` right operand. Values `148`, `149`, and `150` are rune constant, equality, and inequality. A rune constant has shape `16`, no operands, and its exact scalar in `Target`; comparisons consume two shape-`16` values and produce `bool`. Values `151` through `162` are the `f32`/`f64` constant, arithmetic, negation, and comparison family. Value `163` is `Unitˉconstant`: it has shape `9`, no operands, and zero target and auxiliary fields. Value `164` is `Variantˉfield`: it consumes one exact nominal variant, stores the canonical variant index in `Target`, packs `case * 64 + field` in `Auxiliary`, and produces that field's exact shape. Value `0` is invalid in published evidence.

The numeric mapping is frozen by `Compilerˉsourceˉwirˉoperation` and verified by the focused demo. Adding an operation requires updating its result shape, operand arity and shapes, target/auxiliary contract, demo coverage, this specification, and both native qualification scripts.

## Independent validation

`Compilerˉsourceˉwirˉdirectoryˉisˉvalid` verifies:

- magic, version, fixed entry sizes, bounded counts, exact section offsets, and exact total length;
- canonical function ranges aligned with WVSD and WVLB, parameter/local counts, and source return shapes;
- canonical block IDs and ownership, gap-free operation coverage, valid targets, and terminator value types;
- operation ownership, kind, source span, result shape, temporary sequencing, and operand sequencing;
- prior-temporary use, local slots, inferred-local establishment by a non-void first store, consistent later local loads/stores, data and nominal identities, field/member/case indices, variant field counts and exact field shapes, collection descriptors, builder transitions, call targets, arity, dynamic parameter/result shapes, ordinary unit values, and return-only never shapes;
- value-phi placement as the first operation of its join block, two distinct valid predecessor blocks, two exact same-shape operands owned by those predecessors, a result of that same non-void/non-never shape, an unconditional jump from both predecessors to the join, and no branch or third predecessor targeting that join; and
- rejection of trailing bytes and corrupted function, block, operation, temporary, or operand entries.

Construction uses function-private payloads and merges each completed function once. Symbol lookup uses a deterministic first-byte index over absolute WVSS spans. Canonical record/enum shapes and directory identities use the private WVSI bidirectional nominal tables rather than repeated ordinal rescans. Parameter/local WVLB evidence and typed WVIR are constructed in the same successful-path statement traversal. A local initializer is lowered before its declaration becomes visible; an omitted annotation takes that initializer's exact non-void shape, and the resolved growing binding state is carried through nested blocks. The independent validator can consume standalone WVLB 1.1 evidence by establishing each shape-`0` inferred local from its first verified store and requiring all subsequent accesses to agree. If typed lowering fails, the local-only and complete binding passes remain diagnostic oracles so established binding failures retain precedence.

A bare required capability name emits the existing `U32ˉconstant = 3` operation
with its exact internal capability-reference result shape and zero target and
auxiliary fields. Calling a local of that shape resolves the root capability
directory entry and emits the existing `Callˉcapability = 63` operation. The
validator accepts the custom shape only when it names an actual required root
capability and rejects capability shapes in records, variants, and collections.
No WVIR operation value or directory version changes.

An accepted `try` evaluates its expression once, requires its shape to equal the
current function return shape, and resolves that shape to a nominal variant with
exactly ordered `Valid` and `Failure` cases. The former has no payload and the
latter has one non-void payload. Lowering emits the existing
`Variantˉisˉcase` operation and a branch. The failure block returns the
expression's original temporary; the success block continues. No payload
extraction, variant reconstruction, conversion, hidden call, new WVIR operation,
or directory-version change is introduced.

A constant read resolves its WVSD 1.1 entry, reevaluates the validated root declaration under the source-symbol contract, and emits the matching scalar, Boolean, or enum constant operation, including `I64ˉconstant`, `U64ˉconstant`, and `Fixedˉintegerˉconstant`. Wide values carry exact low/high `u32` limbs; fixed signed values carry their exact named-width two's-complement bits in the operation target. No data identity, local slot, or runtime lookup is introduced.

A named record literal resolves one accessible record, lowers each field expression left to right in source order, rejects unknown, duplicate, missing, or mismatched fields, and places the resulting temporary IDs into declaration-order operands before emitting the existing `Recordˉcreate = 17` operation. A Language 1.0 record update first lowers its exact same-nominal base once, lowers each uniquely named replacement left to right, extracts every unreplaced declaration-order field from that one base temporary with `Recordˉfield = 18`, and emits the same `Recordˉcreate = 17` operation. Field extraction is storage-only and occurs after the source-ordered replacement evaluations; it adds no user-visible evaluation. No new WVIR operation, value representation, or WVB opcode is introduced. Recursive `else if` lowers through the existing conditional blocks and terminators.

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

A dotted local record path emits one `Recordˉfield = 18` operation per segment
in source order. Each intermediate result must retain an exact record nominal
shape; a scalar or enum before the final segment is an invalid field target.
Unknown members are diagnosed against the owning intermediate nominal type.

An edition-1 value-producing `if` requires `else`. Its condition is lowered once and must have Boolean shape. Each braced arm lowers zero or more ordinary statements followed by one final expression without a semicolon; an `else if` is the recursive value form. Reachable arms must produce the same exact shape, and only the selected arm is reached at runtime. Their results join through `Valueˉphi = 64`; arm-local binding evidence is retained with lexical scope ending at that arm's closing brace. A `never` arm contributes no value and the surviving reachable arm flows through the join without an invented conversion or temporary.

`&&` and `||` lower the left operand, branch to either a short-result block or a right-operand block, and join those Boolean values with `Valueˉphi`. The right expression therefore has no operation or runtime behavior on the skipped path. The operation records the short and right predecessor identities so independent validation does not infer phi ownership from layout alone.

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
