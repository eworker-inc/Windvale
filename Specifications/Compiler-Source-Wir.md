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

The status contract distinguishes upstream source-binding rejection, evidence limits, malformed constructed evidence, type mismatch, invalid conditions and returns, missing returns, unreachable statements, invalid data/index/field/operator use, invalid call arguments, invalid local inference, invalid constant evidence, named-record failures, loop-control placement, invalid or non-exhaustive enum/variant matching, unknown variant cases, invalid payload bindings, invalid collection shapes, and invalid or consumed builders. Appended values `23` through `31` own those match, variant, collection, and builder failures without renumbering the retained values.

## Typed lowering rules

The phase currently lowers:

- `i32`, `i64`, `u8`, `u32`, `u64`, `bool`, `text`, `bytes`, record, enum, variant, sequence, and local builder values;
- literals, storage-free typed constants, parameters, explicitly typed or initializer-inferred locals, simple or compound assignment, data length/load, positional or named record construction and fields, enum members, Foundation intrinsics, functions, and declared capabilities;
- checked arithmetic including division/remainder, fixed-width bitwise/shift operations, comparison, exact scalar/enum/text/bytes equality, short-circuit Boolean conjunction/disjunction, boolean negation, and signed negation;
- exhaustive enum/variant match, variant construction/case tests/payload extraction, builder creation/push/freeze, sequence length/index, and `for` lowering;
- expression statements, `return`, lexical blocks, `if`/`else if`/`else`, `while`, `for`, `break`, and `continue`;
- explicit jump, branch, and return terminators.

Shape `0` is `void`; `1` through `6` are `i32`, `u8`, `u32`, `bool`, `text`, and `bytes`; `7` and `8` are `i64` and `u64`. Record shapes start at `65536`; enum shapes start at `131072 + RecordCount`; variant shapes start at `196608`. Packed high families retain sequence/builder element identity and maximum. Nominal suffixes are canonical WVSD nominal indices.

Each result-producing operation receives the next function-local temporary ID. Operands may refer only to earlier temporaries in the same function. Basic-block IDs are function-local and canonical in construction order. Function entries align one-for-one with WVSD declaration entries; non-function declarations have all-zero function entries.

## WVIR 1 binary directory

All integers are unsigned little-endian and the directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVIR` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Function-entry count |
| 12 | 4 | Function-entry size `48` |
| 16 | 4 | Block count |
| 20 | 4 | Block-entry size `36` |
| 24 | 4 | Operation count |
| 28 | 4 | Operation-entry size `48` |
| 32 | 4 | Temporary count |
| 36 | 4 | Temporary-entry size `4` |
| 40 | 4 | Operand count |
| 44 | 4 | Operand-entry size `4` |

Sections follow in that exact order.

Each 48-byte function entry contains twelve `u32` fields: module, first block/count, first operation/count, first temporary/count, first operand/count, parameter count, local count, and return shape.

Each 36-byte block entry contains nine `u32` fields: module, function, block ID, first operation/count, terminator, value temporary, first target, and second target. The sentinel `4294967295` represents an absent value or target.

Each 48-byte operation entry contains twelve `u32` fields: module, function, block, operation kind, result shape, result temporary, first operand/count, target, auxiliary value, source byte offset, and source byte length.

The temporary section is a sequence of result shapes. The operand section is a sequence of function-local temporary IDs.

## Operation families

Operation values `1` through `64` retain the prior constants, storage, Foundation, nominal, scalar, call, and Boolean-phi contract. Values `65` through `67` are variant create/test/payload. Values `68` through `72` are builder create/push/freeze and sequence length/element. Values `73` through `92` cover `i32`/`u8`/`u32`, text, and bytes operations. Values `93` and `94` are `i64` and `u64` constants; `95` and `96` are their formatting intrinsics; values `97` through `119` are wide arithmetic, comparison, division, and remainder; values `120` through `125` are `u64` bitwise, complement, and shift operations; values `126` and `127` are exact little-endian `u64` byte read and construction; and value `128` is lossless `u32` to `u64` conversion. Value `0` is invalid in published evidence.

The numeric mapping is frozen by `Compilerˉsourceˉwirˉoperation` and verified by the focused demo. Adding an operation requires updating its result shape, operand arity and shapes, target/auxiliary contract, demo coverage, this specification, and both native qualification scripts.

## Independent validation

`Compilerˉsourceˉwirˉdirectoryˉisˉvalid` verifies:

- magic, version, fixed entry sizes, bounded counts, exact section offsets, and exact total length;
- canonical function ranges aligned with WVSD and WVLB, parameter/local counts, and source return shapes;
- canonical block IDs and ownership, gap-free operation coverage, valid targets, and terminator value types;
- operation ownership, kind, source span, result shape, temporary sequencing, and operand sequencing;
- prior-temporary use, local slots, inferred-local establishment by a non-void first store, consistent later local loads/stores, data and nominal identities, field/member/case indices, variant payload presence, collection descriptors, builder transitions, call targets, arity, and dynamic parameter/result shapes;
- Boolean phi placement as the first operation of its join block, two distinct valid predecessor blocks, one Boolean operand owned by each predecessor, an unconditional jump from both predecessors to the join, and no branch or third predecessor targeting that join; and
- rejection of trailing bytes and corrupted function, block, operation, temporary, or operand entries.

Construction uses function-private payloads and merges each completed function once. Symbol lookup uses a deterministic first-byte index over absolute WVSS spans. Canonical record/enum shapes and directory identities use the private WVSI bidirectional nominal tables rather than repeated ordinal rescans. Parameter/local WVLB evidence and typed WVIR are constructed in the same successful-path statement traversal. A local initializer is lowered before its declaration becomes visible; an omitted annotation takes that initializer's exact non-void shape, and the resolved growing binding state is carried through nested blocks. The independent validator can consume standalone WVLB 1.1 evidence by establishing each shape-`0` inferred local from its first verified store and requiring all subsequent accesses to agree. If typed lowering fails, the local-only and complete binding passes remain diagnostic oracles so established binding failures retain precedence.

A constant read resolves its WVSD 1.1 entry, reevaluates the validated root declaration under the source-symbol contract, and emits the matching scalar, Boolean, or enum constant operation, including `I64ˉconstant` and `U64ˉconstant`. Wide values carry exact low/high `u32` limbs in the operation target and auxiliary fields. No data identity, local slot, or runtime lookup is introduced.

A named record literal resolves one accessible record, lowers each field expression left to right in source order, rejects unknown, duplicate, missing, or mismatched fields, and places the resulting temporary IDs into declaration-order operands before emitting the existing `Recordˉcreate = 17` operation. No new WVIR operation or WVB opcode is introduced. Recursive `else if` lowers through the existing conditional blocks and terminators.

A dotted local record path emits one `Recordˉfield = 18` operation per segment
in source order. Each intermediate result must retain an exact record nominal
shape; a scalar or enum before the final segment is an invalid field target.
Unknown members are diagnosed against the owning intermediate nominal type.

`&&` and `||` lower the left operand, branch to either a short-result block or a right-operand block, and join those Boolean values with `Boolˉphi`. The right expression therefore has no operation or runtime behavior on the skipped path. The operation records the short and right predecessor identities so independent validation does not infer phi ownership from layout alone.

`break` closes the current block with a jump to the nearest enclosing loop's after-block. `continue` closes it with a jump to that loop's condition block. Nested loops replace those targets while their bodies are lowered. Compound assignment emits exactly one local load, lowers the right operand, applies the corresponding checked `i32` or `u32` arithmetic operation, and emits one store; an immutable, missing, mismatched, or unsupported target is rejected before publication.

## Verification tiers and current boundary

The fast conformance case compiles the core, runs the semantic/corruption demo, and sends a control-heavy hosted fixture through the real file-reading tool. The fixture produces:

```text
source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200
```

Current deterministic candidate artifacts are:

- `Source-Wir-Core.wvb`: 824,959 bytes, SHA-256 `32f71092a7e0a82c8da5fdd0bc332318a5ce158b79772e40673980ffd44f68e5`.
- `Source-Wir-Demo.wvb`: 829,787 bytes, SHA-256 `1259896022fcee5aa660fae2831fa0ae3ab3be2a6fc8b7eb6678b0c292d210dc`.
- `Source-Wir-Tool.wvb`: 823,255 bytes, SHA-256 `9414ab75c69f15bffca0b0ddf6cd51280196c54b743ab6dd11c27f9d559703be`.

These local identities include inferred-local verification, storage-free typed-constant lowering, named-record remapping, recursive `else if`, loop-control targets, compound assignment, and structurally verified short-circuit Boolean phi nodes; they require cross-host requalification before a new qualification claim.

Decision 0518 moves ordinary construction of all three products to the generic
native Project front door and moves exact core inspection to the paired native
front-door helper. The managed demo and hosted-tool executions remain retained
behavior evidence because the scalar native runner stops the demo with code
`3004` and does not bind the tool's hosted capabilities.

The original typed-WVIR candidate was cross-host qualified at `bf77f70`, the fused local-discovery/typed-WVIR implementation at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index implementation at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055's validated-scan reuse implementation is cross-host qualified at `1a4fca7e295545b3b815bbf187fc048f1a885c74`; Decision 0058's exact bootstrap artifact set is cross-host qualified at `5c16547`.

The ten-module compiler closure is intentionally not in the fast loop. Decision 0042 reduced the focused typed-WVIR fixture from 8,074,045 to 5,735,695 instructions; Decision 0050 reduced it again to 5,715,847 and removed directory-entry construction and nominal-rank derivation as dominant costs. Decision 0055's implementation falls to 3,626,693 focused instructions and completes the exact ten-module input in 3,912,239,584 instructions under the unchanged 4,000,000,000 ceiling. That clears the typed-WVIR performance entry gate. Decision 0058's separate dedicated verifier proceeds through WVB and qualifies exact Stage 1 to Stage 2 convergence.

WVIR-to-WVB lowering is specified separately in the initial [source-to-WVB backend contract](Compiler-Source-Wvb.md). WVIR execution, optimization, native IR, and OS-specific lowering are not part of this contract.
