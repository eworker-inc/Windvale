# Windvale compiler body, local, and call binding

## Status and purpose

`Compilerˉsourceˉbindings` is the portable body-binding phase introduced and cross-host qualified under Decision 0034. The current WVIR candidate adds prepared-symbol and local-only entry points while preserving the qualified full-binding and WVLB contracts. It consumes one complete, valid, acyclic WVSS 1 graph; reuses WVSD 1 declaration evidence; binds function parameters, locals, data reads, assignments, constructors, functions, capabilities, and Foundation intrinsics; and publishes independently validated local-binding evidence.

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

Parameters and `let` locals are immutable. Only a visible `var` local may be an assignment target. Local type annotations accept primitive types or visible record/enum types under the qualified source-symbol rules.

## Name and call binding

A name expression resolves first to an active local/parameter and then to accessible global data. A field expression currently proves that its base is an active local/parameter or an accessible nominal declaration; field ownership and result type are deferred to typed expression binding. Indexed data names must resolve to accessible global data.

A call resolves in this order:

1. implemented Foundation intrinsic;
2. record constructor;
3. function;
4. declared capability.

The target must be visible through the WVSD visibility matrix, and the supplied argument count must match the constructor field count, function parameter count, intrinsic arity, or capability arity. Arguments are bound in source order before target failure is reported. A known capability that was not declared returns `Undeclaredˉcapability`; an otherwise absent target returns `Unknownˉcall`.

Reads, assignments, and calls are deterministic occurrence counts, not optimization or liveness information.

## WVLB 1 binding directory

All integers are unsigned little-endian. The directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVLB` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Binding entry count |
| 12 | 4 | Fixed binding entry size `36` |
| 16 | 4 | Function-range count, exactly the WVSD entry count |
| 20 | 4 | Fixed function-range entry size `8` |

The header is followed by one range entry for every WVSD declaration entry. Each range contains `FirstBindingEntry` and `BindingCount`. Non-function declarations have zero bindings. Function ranges form one canonical, gap-free cover of all binding entries.

Each 36-byte binding entry contains nine `u32` fields in this order: module index, WVSD function-entry index, binding-kind value, slot, name byte offset, name byte length, shape, scope-start byte offset, and exclusive scope-end byte offset.

Shape values `1` through `6` represent `i32`, `u8`, `u32`, `bool`, `text`, and `bytes`. A record shape is `65536 + NominalIndex`; an enum shape is `131072 + NominalIndex`. Nominal indices are the canonical WVSD identities.

Before publication, `Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid` checks the complete header, exact length, canonical ranges, declaration ownership, slot/kind consistency, shape bounds, identifier spans, scope bounds, order, and trailing data. Identifier validation operates directly over absolute WVSS spans; it does not materialize a source copy or rescan from the start of the module for each binding.

## Deterministic processing and performance

The phase validates source symbols first, then traverses modules, declarations, statements, and expression children in canonical source order. A local initializer is bound before its declaration is appended. Call arguments are bound before the call target and arity. The first failure under this order is returned with current module, related module or sentinel, WVSD function entry, byte offset, and one-based line/column.

One combined full pass constructs local evidence and binds body references. Hot lookups pass the immutable binding payload plus the current function range directly. Global lookup uses the prepared symbol index and compares names against absolute offsets in the packed WVSS input. Each function constructs its binding payload privately and merges it once, avoiding quadratic global byte-buffer growth.

The real nine-module compiler closure must complete below the fixed 4,000,000,000-instruction ceiling. Raising that ceiling is not an accepted substitute for correcting repeated materialization or rescan work.

## Candidate artifacts and evidence

- `Source-Bindings-Core.wvb`: 334,172 bytes, SHA-256 `8656850137e843920f8296660936d6f9043b2804095035e87289c4569ebe535b`.
- `Source-Bindings-Demo.wvb`: 341,161 bytes, SHA-256 `a78ecea3579a2ff64ae2ae19e40a424663193982fad497fe944f5fac6c262d81`.
- `Source-Bindings-Tool.wvb`: 336,758 bytes, SHA-256 `abcd8a8edda501d3a357fee1af368304687cca0b8a3f7686c2ebe01361d554f7`.

The focused demo covers valid parameters/locals/data/calls; mutable and immutable assignment; nested scope and initializer visibility; duplicate locals; primitive, visible, unknown, and inaccessible local types; unknown and inaccessible names/calls; undeclared capabilities; arity; upstream symbol failures; and corrupted header, range, entry, and trailing-data evidence.

The hosted tool binds the current real closure as:

```text
source bindings status=Valid modules=9 functions=187 parameters=813 locals=944 reads=8229 assignments=639 calls=1450 directory-bytes=65148
```

The pre-preparation `9185b28` archive passed the stated qualification on both hosts. The hashes above belong to the current WVIR candidate and require a new exact-archive Windows/Debian qualification before they are described as cross-host qualified.
