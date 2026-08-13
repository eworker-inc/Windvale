# Windvale compiler declaration and signature symbols

## Status and purpose

`Compilerˉsourceˉsymbols` is the portable declaration and signature phase introduced and cross-host qualified under Decision 0033. The current candidate retains Decision 0050's bidirectional nominal identity evidence and Decision 0055's bounded nominal-range lookup while adding typed-constant validation under Decision 0184. It consumes one complete, valid, acyclic WVSS 1 graph, validates declaration namespaces, signature types, and deterministic root constants, and publishes evidence for later semantic phases.

It does not bind function bodies, locals, calls, expressions, control flow, construct WIR, or emit WVB.

## Result contract

```text
enum Compilerˉsourceˉsymbolˉstatus {
    Valid = 0;
    Sourceˉgraph = 1;
    Capabilityˉlimit = 2;
    Dataˉlimit = 3;
    Nominalˉtypeˉlimit = 4;
    Functionˉlimit = 5;
    Duplicateˉcapability = 6;
    Unknownˉcapability = 7;
    Capabilityˉprofile = 8;
    Duplicateˉdata = 9;
    Duplicateˉtype = 10;
    Duplicateˉfunction = 11;
    Constructorˉconflict = 12;
    Reservedˉname = 13;
    Emptyˉrecord = 14;
    Duplicateˉfield = 15;
    Emptyˉenum = 16;
    Duplicateˉenumˉmember = 17;
    Duplicateˉenumˉvalue = 18;
    Unknownˉtype = 19;
    Inaccessibleˉtype = 20;
    Invalidˉfieldˉtype = 21;
    Invalidˉdirectory = 22;
    Duplicateˉparameter = 23;
    Constantˉlimit = 24;
    Invalidˉconstantˉname = 25;
    Invalidˉconstantˉtype = 26;
    Invalidˉconstantˉinitializer = 27;
    Constantˉtypeˉmismatch = 28;
    Constantˉforwardˉreference = 29;
    Constantˉoverflow = 30;
    Importedˉconstant = 31;
}

record Compilerˉsourceˉsymbolˉsummary {
    Status: Compilerˉsourceˉsymbolˉstatus;
    Graphˉstatus: Compilerˉsourceˉgraphˉstatus;
    Modules: u32;
    Capabilities: u32;
    Data: u32;
    Records: u32;
    Enums: u32;
    Functions: u32;
    Fields: u32;
    Members: u32;
    Parameters: u32;
    Directory: bytes;
    Visibility: bytes;
    Lookup: bytes;
    Failureˉmodule: u32;
    Failureˉrelatedˉmodule: u32;
    Failureˉkind: Compilerˉsourceˉdeclarationˉkind;
    Failureˉoffset: u32;
    Failureˉline: u32;
    Failureˉcolumn: u32;
}

Compilerˉvalidateˉsourceˉsymbols(Input: bytes)
    -> Compilerˉsourceˉsymbolˉsummary
```

Success returns aggregate declaration/member counts, a valid WVSD directory, a valid visibility matrix, an internal deterministic lookup index, both failure module indices equal to `Modules`, failure kind `End`, failure offset equal to the complete WVSS length, and zero failure line/column. Failure returns empty evidence values. A graph rejection preserves the graph status and its failure evidence.

## Namespace and signature rules

Capability names, value names, nominal type names, and function names each form one global namespace across the complete supplied graph. Data and constants share the value namespace. Records and enums share the nominal namespace. Record constructors and functions share the callable constructor namespace. Function and record names matching a Foundation intrinsic are reserved.

Capabilities must belong to the implemented catalog. A portable-profile module may not declare capabilities. The aggregate bounds are 32 capabilities, 4,096 data declarations, 4,096 constants, 1,024 nominal types, and 4,096 functions.

Records and enums are nonempty. Field names are unique within a record. Enum member names and explicit values are each unique within an enum. Parameter names are unique within a function. These rules apply after complete syntax validation, so the symbol phase operates only on qualified declaration and body spans.

A named signature type resolves by exact ordinal UTF-8 name against the global nominal namespace. The owner module must be the declaration module or transitively import it. Record fields may contain primitives, enums, or immutable records; another nominal kind returns `Invalidˉfieldˉtype`. Function parameters and results may use visible record or enum types. A nested record field preserves the referenced nominal identity and does not imply mutability, aliasing, or an ambient allocation capability.

Nominal indices are deterministic and independent of source order: all records sorted by ordinal name receive the first indices, then all enums sorted by ordinal name. The current global nominal namespace makes identical names unambiguous.

Constants are currently permitted only in WVSS module zero. Their names use ASCII `ALL_CAPS_WITH_UNDERSCORES`; their explicit type is `i32`, `i64`, `u8`, `u32`, `u64`, `bool`, or a visible enum; and their initializer may use matching literals, enum members, earlier constants, parentheses, and the currently admitted exact-type operators. Boolean `&&` and `||` evaluate left to right and skip their right operand when the left value determines the result; invalid, unresolved, or would-overflow syntax on a skipped path therefore does not reject the constant. Calls, data reads, allocation-bearing expressions, evaluated forward/cyclic references, unsupported operators, mismatched types, and checked overflow/underflow fail before symbol evidence is published. Wide integers are evaluated with explicit low/high `u32` limbs so their range and overflow behavior do not inherit the host runtime.

## WVSD 1.1 declaration directory

All integers are unsigned little-endian. The directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVSD` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `1` |
| 8 | 4 | Entry count |
| 12 | 4 | Fixed entry size `24` |

Each entry contains six `u32` fields in this order: WVSS module index, declaration-kind value, declaration byte offset, name byte offset, name byte length, and declaration item count. Imports are excluded. Values `1` through `6` retain import, capability, data, record, enum, and function identity; `Constant = 7` is appended. Entries use canonical WVSS module order and source declaration order.

The directory length must be exactly `16 + EntryCount * 24`. `Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid` remains an exported strict validator that reparses every accepted source as a stream. The normal phase constructs counts and entries together, checks the complete binary shape, and compares every entry with its source declaration during the namespace pass. This preserves an independent canonical comparison without a redundant whole-source traversal.

## Internal lookup index

`Lookup` is a private `WVSI 1.1` acceleration index. Its 16-byte header contains magic `WVSI`, major/minor version `1.1`, the WVSD entry count, and bucket count `256`. The header is followed by 256 16-byte ranges. Each range contains the first payload index, entry count, prior-record count, and prior-enum count for one possible first UTF-8 byte. The bucket payload then stores every WVSD directory index exactly once.

Two tables follow the bucket payload. The reverse table contains one `u32` for each record and enum in canonical nominal order and maps that ordinal to its WVSD directory index. The forward table contains one `u32` for each WVSD entry and maps nominal declarations to their canonical ordinal; the total nominal count is the nonnominal sentinel. The complete length is `4112 + EntryCount * 8 + NominalCount * 4` bytes.

Exact nominal lookup searches the reverse table in two bounded passes: first the record range for the requested first UTF-8 byte, then the corresponding enum range. Unequal byte lengths are rejected before ordinal comparison. This preserves record-then-enum identity and avoids scanning unrelated WVSD entries.

Name equality remains exact ordinal UTF-8 comparison over validated absolute WVSS spans. Construction is deterministic and total even before duplicate-name rejection. The index never changes namespace semantics and is not a separately published compatibility format.

## Visibility matrix

`Visibility` is exactly `Modules * Modules` bytes in row-major owner/target order. Every byte is zero or one. Each module sees itself and its direct imports; deterministic transitive closure adds indirect imports. With the WVSS limit of 64 modules, the matrix is at most 4,096 bytes. A type declared in module `Target` is accessible from module `Owner` only when the corresponding byte is one.

## Deterministic processing order

The phase validates in this order: source graph; aggregate counts plus WVSD construction; directory shape; namespaces, canonical entry correspondence, constant names, root-only placement, and capability policy; visibility construction; then constants, records, enums, and function signatures in canonical WVSS module/source order. A constant recursively evaluates only earlier constant declarations, with a depth bound of 64. Within a declaration, members and expression operands are checked in source order. Inputs containing multiple faults receive the first failure under this order.

Failure evidence names the current module, a related prior/target module when applicable, declaration kind, name/token byte offset, and one-based line/column. `Modules` is the sentinel when no related module exists.

## Candidate artifacts and retained qualified evidence

- `Source-Symbols-Core.wvb`: 442,471 bytes, SHA-256 `29cdfca436073bf628fa92a10f70915f14bdbcddffb659b25dec793722790e2b`.
- `Source-Symbols-Demo.wvb`: 453,357 bytes, SHA-256 `b4aed72b84f8c23f3f391b663d1c87a27912bfff355e3f1def848f057b5e8e65`.
- `Source-Symbols-Tool.wvb`: 441,304 bytes, SHA-256 `01b96a2a6f2d6f1d0210e57020b928f4dad5b3ac1407fd0e0a04b875048f87e7`.

Decision 0517 reproduces all three identities through the current-Windows
native Project front door and natively inspects the core portable type/export
surface. Independent Linux execution and native demo/tool execution remain
pending.

The candidate demo additionally exercises valid constants, enum and earlier-constant references, Boolean short-circuiting over invalid or would-overflow skipped operands, invalid names/types/initializers, exact type mismatch, forward reference, checked overflow, and imported-module rejection. The hosted tool retains namespace/signature reporting; constants contribute WVSD entries but deliberately do not change the existing public aggregate `Data` count. The current local whole-compiler closure report is:

```text
source symbols status=Valid modules=8 capabilities=0 data=0 records=31 enums=14 functions=204 fields=344 members=245 parameters=897 directory-bytes=5992 visibility-bytes=64
```

The pre-index implementation was qualified at `d57a6d8`, and the first indexed implementation at `bf77f70`. Decision 0050's implementation is qualified at `e37204f`; it advanced the private acceleration contract from `WVSI 1.0` to `WVSI 1.1`. Decision 0055 added bounded reverse-table lookup and is cross-host qualified at `1a4fca7`. Decision 0058 changed equality-only implementation paths and embedded artifact bytes while preserving the then-current WVSD 1.0 and WVSI 1.1 formats and is cross-host qualified at `5c16547`. WVSD 1.1 and typed constants are new local candidate behavior and do not inherit those cross-host claims.
