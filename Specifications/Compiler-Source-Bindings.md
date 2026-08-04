# Windvale compiler body, local, and call binding

## Status and purpose

`Compilerˉsourceˉbindings` is the portable body-binding phase introduced and cross-host qualified under Decision 0034. The later WVIR integration adds prepared-symbol and local-only entry points while preserving the qualified full-binding and WVLB contracts. It consumes one complete, valid, acyclic WVSS 1 graph; reuses WVSD 1 declaration evidence; binds function parameters, locals, data reads, assignments, positional and named record construction, functions, capabilities, and Foundation intrinsics; and publishes independently validated local-binding evidence.

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

Parameters and `let` locals are immutable. Only a visible `var` local may be an assignment target. Ordinary `=` contributes one assignment occurrence. `+=`, `-=`, and `*=` bind the same simple mutable-local target as one read followed by one assignment before traversing the right operand; the exact operator and value types remain WVIR responsibilities. Local type annotations accept primitive types or visible record/enum types under the qualified source-symbol rules. An omitted local annotation is recorded as unresolved inference evidence; complete expression typing remains owned by WVIR.

## Name and call binding

A name expression resolves first to an active local/parameter and then to an accessible global data or constant declaration. A root constant is storage-free: binding recognizes its value-namespace declaration but creates no parameter/local WVLB entry or runtime data slot. Typed WVIR reparses its already validated declaration and substitutes the exact value. Constants in imported modules are rejected by the preceding symbol phase. A field expression currently proves that its base is an active local/parameter or an accessible nominal declaration; field ownership and result type are deferred to typed expression binding. Indexed data names must resolve to accessible global data.

A positional call resolves in this order:

1. implemented Foundation intrinsic;
2. record constructor;
3. function;
4. declared capability.

The target must be visible through the WVSD visibility matrix, and the supplied argument count must match the constructor field count, function parameter count, intrinsic arity, or capability arity. Arguments are bound in source order before target failure is reported. A known capability that was not declared returns `Undeclaredˉcapability`; an otherwise absent target returns `Unknownˉcall`.

Reads, assignments, and calls are deterministic occurrence counts, not optimization or liveness information.

A named record literal binds every field value left to right before resolving its record target, requires that target to be an accessible record declaration, and contributes one constructor call. Field existence, uniqueness, completeness, exact value types, and declaration-order operand placement are owned by typed WVIR so those failures use the typed semantic status contract. Recursive `else if` spans are traversed as one nested statement and preserve ordinary lexical block behavior. `break` and `continue` carry no name-binding evidence; loop ownership and reachability are proved by WVIR.

## WVLB 1.1 binding directory

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

Each 36-byte binding entry contains nine `u32` fields in this order: module index, WVSD function-entry index, binding-kind value, slot, name byte offset, name byte length, shape, scope-start byte offset, and exclusive scope-end byte offset.

Shape `0` is permitted only on a `let` or `var` entry whose source type is inferred and means “resolve from typed initializer evidence.” Parameter shapes are always concrete. Shape values `1` through `8` represent `i32`, `u8`, `u32`, `bool`, `text`, `bytes`, `i64`, and `u64`. A record shape is `65536 + NominalIndex`; an enum shape is `131072 + NominalIndex`. Nominal indices are the canonical WVSD identities.

Before publication, `Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid` checks the complete header, exact length, canonical ranges, declaration ownership, slot/kind consistency, concrete shape bounds or the local-only inference marker, identifier spans, scope bounds, order, and trailing data. Identifier validation operates directly over absolute WVSS spans; it does not materialize a source copy or rescan from the start of the module for each binding.

## Deterministic processing and performance

The phase validates source symbols first, then traverses modules, declarations, statements, and expression children in canonical source order. A local initializer is bound before its declaration is appended. Call arguments are bound before the call target and arity. The first failure under this order is returned with current module, related module or sentinel, WVSD function entry, byte offset, and one-based line/column.

One combined full pass constructs local evidence and binds body references. The parameter phase and final publication step are also explicit so typed WVIR construction can carry the same function-private binding state while it lowers statements, resolve an inferred local to the initializer's exact shape, then publish canonical WVLB evidence without a second successful-path body traversal. A standalone WVLB keeps shape `0` for such a local because the binding pass deliberately does not duplicate expression typing. Hot lookups pass the immutable binding payload plus the current function range directly. Global lookup uses the prepared symbol index, rejects unequal UTF-8 byte lengths before ordinal comparison, and compares names against absolute offsets in the packed WVSS input. Nominal shapes use the WVSI forward directory-to-ordinal table rather than rescanning and reranking WVSD. Each function constructs its binding payload privately and merges it once, avoiding quadratic global byte-buffer growth.

Intrinsic-call lookup dispatches candidates by exact UTF-8 byte length, checks the most common compiler intrinsics first within each length group, and returns on the first match. A nonmatching length does not materialize the candidate text as bytes.

The real nine-module compiler closure must complete below the fixed 4,000,000,000-instruction ceiling. Raising that ceiling is not an accepted substitute for correcting repeated materialization or rescan work.

## Current deterministic artifacts and retained evidence

- `Source-Bindings-Core.wvb`: 539,903 bytes, SHA-256 `55a2d97d55dc7e52f6732dc6312b04ed066a997e2b92be625354645a28370c22`.
- `Source-Bindings-Demo.wvb`: 545,630 bytes, SHA-256 `caecddffa3ee83c35424f46b7581e185e434d85533b56ca05c497d90da9d08e3`.
- `Source-Bindings-Tool.wvb`: 539,928 bytes, SHA-256 `f30016abc392c6e0141426f488397ac74a404d8f1aa636c9ed5ed69d16c458b4`.

These are local deterministic WVLB 1.1 candidate identities; they do not claim cross-host requalification.

The focused demo covers valid parameters/locals/data/calls; ordinary and compound mutable assignment plus immutable rejection; loop-control and short-circuit expression traversal; nested scope and initializer visibility; duplicate locals; primitive, visible, unknown, and inaccessible local types; unknown and inaccessible names/calls; undeclared capabilities; arity; upstream symbol failures; and corrupted header, range, entry, and trailing-data evidence.

The current local hosted closure report is:

```text
source bindings status=Valid modules=9 functions=261 parameters=1154 locals=1584 reads=13346 assignments=1098 calls=2314 directory-bytes=101120
```

The pre-preparation implementation was qualified at `9185b28`, the prepared-symbol/local-only baseline at `bf77f70`, and Decision 0041's fused consumer at `b124115`. Decision 0050's bidirectional nominal map and length-filtered equality paths are qualified at `e37204f`. Decision 0055's artifacts consume validated lexer cursors and the bounded nominal range and are cross-host qualified at `1a4fca7`. Decision 0058 uses the reverse equality helper on exact-name paths without changing WVLB format or binding semantics and is cross-host qualified at `5c16547`.
