# Windvale compiler body, local, and call binding

## Status and purpose

`Compilerˉsourceˉbindings` is the portable body-binding phase introduced and cross-host qualified under Decision 0034. The later WVIR integration adds prepared-symbol and local-only entry points while preserving the qualified full-binding and WVLB contracts. It consumes one complete, valid, acyclic WVSS 1 graph; reuses WVSD 1 declaration evidence; binds function parameters, locals, data reads, assignments, positional and named record construction and update, functions, capabilities, and Foundation intrinsics; and publishes independently validated local-binding evidence.

Complete expression types, field ownership, operator/result types, return proof, and WIR construction now belong to `Compilerˉsourceˉwir`; this phase remains the owner of binding identity and diagnostic precedence. It does not emit WVB.

## Result contract

```text
enum Compilerˉsourceˉbindingˉstatus {
    Valid = 0;
    Sourceˉsymbols = 1;
    Bindingˉlimit = 2;
    Evidenceˉlimit = 3;
    Duplicateˉlocal = 4;
    Unknownˉtype = 5;
    Inaccessibleˉtype = 6;
    Invalidˉdirectory = 7;
    Unknownˉname = 8;
    Inaccessibleˉname = 9;
    Unknownˉassignment = 10;
    Immutableˉassignment = 11;
    Inaccessibleˉcall = 12;
    Undeclaredˉcapability = 13;
    Unknownˉcall = 14;
    Callˉarity = 15;
}

record Compilerˉsourceˉbindingˉsummary {
    Status: Compilerˉsourceˉbindingˉstatus;
    Symbolˉstatus: Compilerˉsourceˉsymbolˉstatus;
    Modules: u32;
    Functions: u32;
    Parameters: u32;
    Locals: u32;
    Reads: u32;
    Assignments: u32;
    Calls: u32;
    Directory: bytes;
    Failureˉmodule: u32;
    Failureˉrelatedˉmodule: u32;
    Failureˉfunction: u32;
    Failureˉoffset: u32;
    Failureˉline: u32;
    Failureˉcolumn: u32;
}

Compilerˉvalidateˉsourceˉbindings(Input: bytes)
    -> Compilerˉsourceˉbindingˉsummary
```

Later portable phases may reuse validated preparation through `Compilerˉsourceˉbindingsˉfromˉsymbols` for the complete binding pass or `Compilerˉsourceˉbindingsˉlocalsˉfromˉsymbols` for parameter/local evidence only. The latter still parses every function body to discover lexical locals but skips reference/call counting. Typed lowering invokes the complete pass as an error oracle when it rejects a program, preserving established binding failures before typed-WVIR failures.

Success returns aggregate body counts, a valid WVLB directory, both failure module indices equal to `Modules`, failure function equal to the WVSD entry count, failure offset equal to the complete WVSS length, and zero failure line/column. Failure returns an empty published directory. A source-symbol rejection preserves the upstream symbol status and failure evidence.

## Local binding rules

Function parameters receive slots first in declaration order. `let` and `var` locals then receive monotonically increasing slots in statement traversal order. A function may contain at most 4,096 parameter/local bindings.

Parameter scope is the complete function body. A local initializer is bound before the local is declared, so a local cannot read itself. A local becomes visible at the end of its declaration statement and remains visible through the end of its containing block. A nested-block local becomes inactive when that block ends.

Parameter and local names are unique across the complete function. Shadowing is deliberately unavailable in this stage: an inner declaration cannot reuse a parameter or earlier local name, even when the earlier binding is inactive. This keeps slots and diagnostics stable and matches Windvale's current explicit-name convention.

Parameters and `let` locals are immutable. Only a visible `var` local may be an assignment target. Ordinary `=` contributes one assignment occurrence. `+=`, `-=`, and `*=` bind the same simple mutable-local target as one read followed by one assignment before traversing the right operand; the exact operator and value types remain WVIR responsibilities. Local type annotations accept primitive types, visible record/enum types, or an exact required root capability-reference type under the qualified source-symbol rules. An omitted local annotation is recorded as unresolved inference evidence; complete expression typing remains owned by WVIR.

## Name and call binding

A name expression resolves first to an active local/parameter and then to an accessible global data or constant declaration. A root constant is storage-free: binding recognizes its value-namespace declaration but creates no parameter/local WVLB entry or runtime data slot. Typed WVIR reparses its already validated declaration and substitutes the exact value. Constants in imported modules are rejected by the preceding symbol phase. A field expression currently proves that its base is an active local/parameter or an accessible nominal declaration; field ownership and result type are deferred to typed expression binding. Indexed data names must resolve to accessible global data.

A positional call resolves in this order:

1. implemented Foundation intrinsic;
2. record constructor;
3. function;
4. declared capability.

A local or parameter with capability-reference shape is callable and resolves only
the exact root capability directory entry embedded in that shape. Binding counts
the callee read plus one capability call and validates catalog arity. It does not
look at the erased witness payload or select a provider dynamically.

The target must be visible through the WVSD visibility matrix, and the supplied argument count must match the constructor field count, function parameter count, intrinsic arity, or capability arity. Arguments are bound in source order before target failure is reported. A known capability that was not declared returns `Undeclaredˉcapability`; an otherwise absent target returns `Unknownˉcall`.

Reads, assignments, and calls are deterministic occurrence counts, not optimization or liveness information.

The Language 1.0 unit literal `()` is a leaf with no name, call, local, or
runtime-storage binding evidence. Its edition and result-shape rules remain typed
WVIR responsibilities.

A named record literal binds every field value left to right before resolving its record target, requires that target to be an accessible record declaration, and contributes one constructor call. An applied target such as `Box<Point> { ... }` retains its complete type application for WVGT-aware typed resolution; the bare `Box` template is not assigned a concrete ordinary shape. A Language 1.0 record update binds its base once, then its replacement values left to right, before applying the same target-resolution and constructor-call evidence. Field existence, uniqueness, completeness or preservation, exact base/value types, generic admission, and declaration-order operand placement are owned by typed WVIR so those failures use the typed semantic status contract. Recursive `else if` spans are traversed as one nested statement and preserve ordinary lexical block behavior. A `try` statement binds its expression exactly as an ordinary expression statement and introduces no local or payload binding; its result shape and propagation contract belong to typed WVIR. `break` and `continue` carry no name-binding evidence; loop ownership and reachability are proved by WVIR.

## WVLB 1.1, 1.2, and 1.3 binding directories

All integers are unsigned little-endian. The directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVLB` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `1` |
| 8 | 4 | Binding entry count |
| 12 | 4 | Fixed binding entry size `36` |
| 16 | 4 | Function-range count, exactly the WVSD entry count |
| 20 | 4 | Fixed function-range entry size `8` |

The header is followed by one range entry for every WVSD declaration entry. Each range contains `FirstBindingEntry` and `BindingCount`. Non-function declarations have zero bindings. Function ranges form one canonical, gap-free cover of all binding entries.

Source without admitted generic instances retains that exact WVLB 1.1 form.
When at least one generic instance is admitted, the compiler publishes WVLB
1.2 instead. Its 32-byte header retains offsets 0 through 16 above, sets minor
version `2`, sets the function-range entry size at offset 20 to `16`, stores the
exact embedded WVGC byte length at offset 24, and stores catalog layout version
`1` at offset 28. The function-range count is the full WVSD entry count plus
the WVGC instance count.

Each WVLB 1.2 function range contains four `u32` fields: first binding entry,
binding count, source WVSD declaration entry, and WVGC instance. Ordinary
source-directory positions name themselves and use instance sentinel
`4294967295`. A generic source declaration retains one ordinary zero-binding
placeholder. Concrete ranges are appended in WVGC order; their directory
identity is `WvsdEntryCount + Instance`, their declaration field names the
generic function, and their instance field is the zero-based WVGC entry.

The exact WVGC 1.0 evidence follows all ranges and precedes the 36-byte binding
entries. This is retained validation evidence, not a runtime representation.
The specialized validator requires a valid bounded catalog, exactly one
appended range per instance, correct declaration/instance mapping, concrete
parameter and local shapes, canonical gap-free binding coverage, and exact
total length.

WVLB 1.3 is the retained-evidence carrier for general generic nominal types.
It is selected only when the paired WVGT catalog contains at least one admitted
record or variant instance. Ordinary source therefore remains byte-for-byte
WVLB 1.1, and function-specialization-only source remains byte-for-byte WVLB
1.2.

The WVLB 1.3 header is 40 bytes. It retains offsets 0 through 24 from WVLB
1.2, sets minor version `3`, stores the exact WVGT byte length at offset 28,
stores combined catalog layout version `2` at offset 32, and requires the
reserved `u32` at offset 36 to be zero. Offset 24 is the WVGC byte length and
may be zero when there are no function specializations. Offset 28 is at least
24 and must describe a non-empty valid WVGT 1.0 catalog. Function ranges retain
the 16-byte WVLB 1.2 shape and cover the WVSD declarations plus any WVGC
instances; generic nominal types do not create function ranges.

After the ranges come the optional WVGC bytes, the required WVGT bytes, and the
36-byte binding entries, in that exact order. A private WVGT shape is valid in
a parameter or local entry only when its zero-based instance exists in the
retained WVGT catalog. The private shape is compiler evidence, not a runtime
type identity: materialization replaces it with the assigned ordinary record
or variant shape before WVB emission. The validator rejects absent or empty
WVGT evidence, out-of-catalog private shapes, cross-catalog length confusion,
nonzero reserved fields, truncation, and trailing bytes.

`Compilerˉsourceˉbindingsˉgenericˉtypesˉfinishˉspecializedˉphase` is the
publication path for the combined envelope. It is owned by the focused
`Compilerˉsourceˉbindingsˉgenericˉtypes` module. An empty WVGT catalog delegates
to the unchanged function-only binding module, preserving its prior format,
source closure, and bytes. The carrier and independent validator are
implemented. Main WVIR construction now threads its admitted WVGT catalog into
this entry point, so a generic nominal signature or explicit local selects WVLB
1.3 while ordinary and function-specialization-only source retain their prior
bytes.

Each 36-byte binding entry contains nine `u32` fields in this order: module index, WVSD function-entry index, binding-kind value, slot, name byte offset, name byte length, shape, scope-start byte offset, and exclusive scope-end byte offset.

Shape `0` is permitted only on a `let` or `var` entry whose source type is inferred and means “resolve from typed initializer evidence.” Parameter shapes are always concrete. Shape values `1` through `8` represent `i32`, `u8`, `u32`, `bool`, `text`, `bytes`, `i64`, and `u64`; values `9` and `10` represent `unit` and `never`; values `11`, `12`, and `13` represent `i8`, `i16`, and `u16`; values `14` and `15` represent `f32` and `f64`; and value `16` represents `rune`. `unit` is concrete binding evidence. `never` is valid only as the later typed-WVIR result shape and is rejected for parameters and locals. A record shape is `65536 + NominalIndex`; an enum shape is `131072 + NominalIndex`. Slice 3 additionally carries the exact private compact Foundation Option and Result shapes defined by the source-symbol contract. An exact singleton capability-reference shape is `268435456 + RootCapabilityDirectoryEntry` and is valid only when that entry is a required module-zero capability. Nominal indices are the canonical WVSD identities.

An admitted function specialization publishes only concrete binding shapes.
For the bounded generic-collection checkpoint, the ordinary private sequence or
builder descriptor contains the selected element shape and exact maximum; no
generic parameter index or WVGS entry enters a binding. The owning WVGC identity
is carried once by the WVLB 1.2 catalog/range envelope rather than copied into
individual binding entries. When a specialized signature or explicitly typed
local names an applied generic nominal such as `Box<T>`, binding resolves the
direct function parameter through that same WVGC instance, admits the resulting
`Box<Point>` through WVGT, and writes only its catalog-bounded private shape.
The combined result uses WVLB 1.3 and retains both catalogs once; neither `T`
nor a transient WVGS entry is serialized as a binding shape.

Before publication, `Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid` checks WVLB 1.1 and 1.2. `Compilerˉsourceˉbindingsˉgenericˉtypesˉdirectoryˉisˉvalid` delegates those versions unchanged and additionally checks WVLB 1.3. Together they check the complete selected-version header, exact length, canonical ranges, declaration ownership, slot/kind consistency, concrete shape bounds or the local-only inference marker, identifier spans, scope bounds, order, catalog agreement, and trailing data. Identifier validation operates directly over absolute WVSS spans; it does not materialize a source copy or rescan from the start of the module for each binding.

## Deterministic processing and performance

The phase validates source symbols first, then traverses modules, declarations, statements, and expression children in canonical source order. A local initializer is bound before its declaration is appended. Call arguments are bound before the call target and arity. The first failure under this order is returned with current module, related module or sentinel, WVSD function entry, byte offset, and one-based line/column.

One combined full pass constructs local evidence and binds body references. The parameter phase and final publication step are also explicit so typed WVIR construction can carry the same function-private binding state while it lowers statements, resolve an inferred local to the initializer's exact shape, then publish canonical WVLB evidence without a second successful-path body traversal. A standalone WVLB keeps shape `0` for such a local because the binding pass deliberately does not duplicate expression typing. Hot lookups pass the immutable binding payload plus the current function range directly. Global lookup uses the prepared symbol index, rejects unequal UTF-8 byte lengths before ordinal comparison, and compares names against absolute offsets in the packed WVSS input. Nominal shapes use the WVSI forward directory-to-ordinal table rather than rescanning and reranking WVSD. Each function constructs its binding payload privately and merges it once, avoiding quadratic global byte-buffer growth.

Intrinsic-call lookup dispatches candidates by exact UTF-8 byte length, checks the most common compiler intrinsics first within each length group, and returns on the first match. A nonmatching length does not materialize the candidate text as bytes.

The real nine-module compiler closure must complete below the fixed 4,000,000,000-instruction ceiling. Raising that ceiling is not an accepted substitute for correcting repeated materialization or rescan work.

## Current deterministic artifacts and retained evidence

- `Source-Bindings-Core.wvb`: 551,917 bytes, SHA-256 `0ea817fc499138bc8ca03fa2d29706aabedd2a4e949c2857437e153fb30de4d1`.
- `Source-Bindings-Demo.wvb`: 558,551 bytes, SHA-256 `d80d4960412067060596f7bfa9e6bb1e04b400e600cdc1bb866ed2c0a4618231`.
- `Source-Bindings-Tool.wvb`: 551,840 bytes, SHA-256 `ac0f807be27ff91b5575463caf58cd1df0afe97e3e0c72e916a2699f5ca4cfee`.

These are local deterministic WVLB 1.1 candidate identities; they do not claim cross-host requalification.

Decision 0518 moves ordinary construction of all three products to the generic
native Project front door and moves exact core inspection to the paired native
front-door helper. The managed demo and hosted-tool executions remain retained
behavior evidence because the scalar native runner stops the demo with code
`3004` and does not bind the tool's hosted capabilities.

The focused demo covers valid parameters/locals/data/calls; ordinary and compound mutable assignment plus immutable rejection; loop-control and short-circuit expression traversal; nested scope and initializer visibility; duplicate locals; primitive, visible, unknown, and inaccessible local types; unknown and inaccessible names/calls; undeclared capabilities; arity; upstream symbol failures; and corrupted header, range, entry, and trailing-data evidence.

The current local hosted closure report is:

```text
source bindings status=Valid modules=9 functions=263 parameters=1160 locals=1597 reads=13401 assignments=1108 calls=2326 directory-bytes=101820
```

The pre-preparation implementation was qualified at `9185b28`, the prepared-symbol/local-only baseline at `bf77f70`, and Decision 0041's fused consumer at `b124115`. Decision 0050's bidirectional nominal map and length-filtered equality paths are qualified at `e37204f`. Decision 0055's artifacts consume validated lexer cursors and the bounded nominal range and are cross-host qualified at `1a4fca7`. Decision 0058 uses the reverse equality helper on exact-name paths without changing WVLB format or binding semantics and is cross-host qualified at `5c16547`.
