# Windvale compiler declaration and signature symbols

## Status and purpose

`Compilerˉsourceˉsymbols` is the portable declaration and signature phase introduced and cross-host qualified under Decision 0033. The current WVIR candidate adds deterministic indexed lookup evidence while preserving the qualified namespace, signature, and WVSD contracts. It consumes one complete, valid, acyclic WVSS 1 graph, validates declaration namespaces and signature types, and publishes evidence for later semantic phases.

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

Capability names, data names, nominal type names, and function names each form one global namespace across the complete supplied graph. Records and enums share the nominal namespace. Record constructors and functions share the callable constructor namespace. Function and record names matching a Foundation intrinsic are reserved.

Capabilities must belong to the implemented catalog. A portable-profile module may not declare capabilities. The aggregate bounds are 32 capabilities, 4,096 data declarations, 1,024 nominal types, and 4,096 functions.

Records and enums are nonempty. Field names are unique within a record. Enum member names and explicit values are each unique within an enum. Parameter names are unique within a function. These rules apply after complete syntax validation, so the symbol phase operates only on qualified declaration and body spans.

A named signature type resolves by exact ordinal UTF-8 name against the global nominal namespace. The owner module must be the declaration module or transitively import it. Record fields may contain primitives or enums; a named record field returns `Invalidˉfieldˉtype`. Function parameters and results may use visible record or enum types.

Nominal indices are deterministic and independent of source order: all records sorted by ordinal name receive the first indices, then all enums sorted by ordinal name. The current global nominal namespace makes identical names unambiguous.

## WVSD 1 declaration directory

All integers are unsigned little-endian. The directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVSD` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Entry count |
| 12 | 4 | Fixed entry size `24` |

Each entry contains six `u32` fields in this order: WVSS module index, declaration-kind value, declaration byte offset, name byte offset, name byte length, and declaration item count. Imports are excluded. Entries use canonical WVSS module order and source declaration order.

The directory length must be exactly `16 + EntryCount * 24`. `Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid` remains an exported strict validator that reparses every accepted source as a stream. The normal phase constructs counts and entries together, checks the complete binary shape, and compares every entry with its source declaration during the namespace pass. This preserves an independent canonical comparison without a redundant whole-source traversal.

## Internal lookup index

`Lookup` is a private `WVSI 1` acceleration index. It groups WVSD entry indices by the first UTF-8 byte of the declaration name and stores record/enum rank prefixes for each of the 256 byte buckets. Name equality remains exact ordinal UTF-8 comparison over validated absolute WVSS spans. The index never changes namespace semantics and is not a separately published compatibility format.

## Visibility matrix

`Visibility` is exactly `Modules * Modules` bytes in row-major owner/target order. Every byte is zero or one. Each module sees itself and its direct imports; deterministic transitive closure adds indirect imports. With the WVSS limit of 64 modules, the matrix is at most 4,096 bytes. A type declared in module `Target` is accessible from module `Owner` only when the corresponding byte is one.

## Deterministic processing order

The phase validates in this order: source graph; aggregate counts plus WVSD construction; directory shape; namespaces, canonical entry correspondence, and capability policy; visibility construction; then record, enum, and function signatures in canonical WVSS module/source order. Within a declaration, members are checked in source order. Inputs containing multiple faults receive the first failure under this order.

Failure evidence names the current module, a related prior/target module when applicable, declaration kind, name/token byte offset, and one-based line/column. `Modules` is the sentinel when no related module exists.

## Candidate artifacts and evidence

- `Source-Symbols-Core.wvb`: 262,263 bytes, SHA-256 `624fd35749645c0cf269c6d298303b614efad1e112e86cb045016485386d58f6`.
- `Source-Symbols-Demo.wvb`: 274,814 bytes, SHA-256 `ca513e0ea10a84f6c5ccc630927b3c18793b6c2e3d1badabffab08fdcdd2146c`.
- `Source-Symbols-Tool.wvb`: 266,044 bytes, SHA-256 `840492af48d93af014fb12c59b6711752e80519d50ec45dbecee4483b42dce05`.

The Windows and Debian verifiers each pass a zero-warning Release build, all 45 conformance tests, and the complete native CLI checks. The demo exercises valid and rejected namespaces/signatures plus corrupted directories. The hosted tool validates the real eight-module, 283,765-byte compiler closure as:

```text
source symbols status=Valid modules=8 capabilities=0 data=0 records=24 enums=14 functions=135 fields=290 members=181 parameters=597 directory-bytes=4168 visibility-bytes=64
```

The pre-index `d57a6d8` archive passed the stated qualification on both hosts. The hashes above belong to the current WVIR candidate and require a new exact-archive Windows/Debian qualification before they are described as cross-host qualified.
