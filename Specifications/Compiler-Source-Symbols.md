# Windvale compiler declaration and signature symbols

## Status and purpose

`Compilerˉsourceˉsymbols` is the portable declaration and signature phase introduced and cross-host qualified under Decision 0033. The current candidate retains Decision 0050's bidirectional nominal identity evidence and Decision 0055's bounded nominal-range lookup, adds typed-constant validation under Decision 0184, extends that evaluator to `i8`, `i16`, and `u16` under Decision 0768, and conditionally represents Foreign declarations under proposed Decision 0895. It consumes one complete, valid, acyclic WVSS 1 graph, validates declaration namespaces, signature types, and deterministic root constants, and publishes evidence for later semantic phases.

It does not bind function bodies, locals, calls, expressions, control flow, construct WIR, or emit WVB.

Conditional Foreign symbol construction remains compiler-owned portable
semantics under proposed Decision 0895. For a nonempty authenticated catalog,
the private hosted `wvbind` product invokes that logic with the retained WVSS,
WVTD, and WVFC; it does not make symbols coordinator-owned or give the binder
admission authority. The ordinary Analyzer keeps this shared symbol support
but rejects foreign-bearing internal WVSS input before ordinary publication.

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
    Emptyˉvariant = 32;
    Duplicateˉvariantˉcase = 33;
    Invalidˉvariantˉpayload = 34;
    Invalidˉcollectionˉelement = 35;
    Missingˉenumˉbacking = 36;
    Invalidˉenumˉbacking = 37;
    Invalidˉenumˉvalue = 38;
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

Capability names, value names, nominal type names, and callable declaration names each form one global namespace across the complete supplied graph. Data and constants share the value namespace. Records and enums share the nominal namespace. Record constructors, ordinary functions, and Foreign declarations share the callable constructor namespace. Ordinary functions and Foreign declarations use the same duplicate-function and reserved-name rules; a Foreign/function or Foreign/record-constructor collision cannot publish symbol evidence.

Capabilities must belong to the implemented catalog. A portable-profile module may not declare capabilities. The aggregate bounds are 32 capabilities, 4,096 data declarations, 4,096 constants, 1,024 nominal types, and 4,096 functions.

Records and enums are nonempty. Field names are unique within a record. Enum
member names and explicit values are each unique within an enum. Edition 1
requires one explicit `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, or `u64`
backing after the enum name. Each tag is checked against that exact backing: an
unsuffixed literal is contextual, an explicit suffix must match, negative tags
require a signed backing, and every signed minimum is admitted without wrap.
Duplicate comparison uses the complete low/high magnitude and sign and
normalizes `-0` to `0`. `Missingˉenumˉbacking`, `Invalidˉenumˉbacking`, and
`Invalidˉenumˉvalue` distinguish these failures without renumbering retained
statuses. Parameter names are unique within a function. These rules apply after
complete syntax validation, so the symbol phase operates only on qualified
declaration and body spans.

Descriptorless Seed source retains its historical implicit nonnegative `i32`
enum contract. That compatibility path has a compact declaration-local
duplicate scan; it cannot admit negative, suffixed non-`i32`, or out-of-range
tags and it does not define edition-1 semantics.

Generic record, variant, and function declarations share one bounded declaration-parameter descriptor. Record and variant type/constant parameter names are unique in one namespace. A generic record field or variant payload may be one of its declared type parameters. A constant parameter in type position and a builder stored in a record remain rejected. Parameters nested inside collection or nominal type syntax are deferred to the recursive type-use binder rather than partially admitted here. Phantom parameters are valid because concrete identity retains the complete ordered argument list even when a field does not mention one.

An ordinary nominal declaration binds without arguments. A generic nominal
declaration is a template and does not bind as a bare value type; a bare use
returns `Unknownˉtype`. When declaration validation reaches the `<` of an
otherwise parsed generic nominal type production, Source Symbols defers that
application and continues canonical declaration validation. It does not assign
the template or use a concrete shape. The later WVGT-aware semantic phase owns
recursive argument binding, exact arity and kind checks, catalog admission,
substituted-field validation, and concrete WVB materialization. The exact edition-1 Foundation
`Option<T>` and `Result<T, E>` declarations pass the same generic parameter and
payload validation instead of bypassing variant validation.

A named signature type resolves by exact ordinal UTF-8 name against the global nominal namespace. The owner module must be the declaration module or transitively import it. Record fields may contain primitives, enums, or immutable records; another nominal kind returns `Invalidˉfieldˉtype`. Variant fields may contain implemented ordinary value shapes or their own admitted type parameter. Function parameters and results may also use an exact required root capability name. Its type binding retains that root capability's WVSD directory entry. Capability references are rejected as record fields, variant payloads, and collection elements; an optional-only metadata entry does not produce a required capability type. A nested record field preserves the referenced nominal identity and does not imply mutability, aliasing, or an ambient allocation capability.

Language 1.0 recognizes two exact Foundation declaration identities. The edition-1
module `Foundationˉoption` must export exactly `Option<T>` with ordered cases
`Present(Value: T)` and `Absent`. The edition-1 module
`Foundationˉresult` must export exactly `Result<T, E>` with ordered cases
`Valid(Value: T)` and `Failure(Error: E)`. Uses supply exactly one or two
arguments respectively and enter the ordinary bounded WVGT catalog. A bare or
wrong-arity use is rejected. Variant construction requires explicit complete
arguments, just like user-defined generic nominal construction. The exact
Result identity additionally authorizes `try`; no Foundation-only concrete
shape, catalog, or WVB encoding exists.

The first Slice 4 function-lowering checkpoints additionally admit a bounded
function-generic subset. A type parameter may be the complete parameter or
result type, or the element position of one `sequence` or `builder` signature
type. A `const` parameter may occupy that collection type's maximum position
and retains its declared fixed-integer shape. The collection remains non-nested,
its concrete maximum remains 1 through 4,095, and a builder remains forbidden
as an escaping parameter or result by the existing collection ownership rules.
The symbol phase retains parameter kind, fixed-integer shape, and declaration
order for the generic resolver; the resolved value must still fit the existing
positive 1-through-4,095 collection bound. It does not create a runtime generic
type.

Canonical `Foundationˉcollections.Vector<T>` and `Sequence<T>` function
signature uses are deferred through the early symbol pass with an unresolved
shape after exact imported-module and syntax admission. The bounded generic-type
catalog then owns their kind, element, private shape, and dependency evidence;
the generic-aware binding phase substitutes that exact shape before WIR. This
does not admit an unqualified spelling, a lookalike module, or a forgeable
nominal declaration into the symbol directory.

Edition-1 source also recognizes the exact imported identity
`Foundationˉmemory.Memoryˉbudget`. The selected alias must resolve to an
edition-1 module whose header name is exactly `Foundationˉmemory`; an
unqualified spelling and a same-spelled member in any other module remain
`Unknownˉtype`. The binding uses private fixed shape `805306368` and an internal
named/end sentinel rather than a forgeable nominal declaration. It is valid in
function signatures but rejected as a record field until its opaque storage
representation exists. This exact header gate is not package authentication:
the admitted source set and project source lock remain responsible for choosing
the trusted Foundation module.

The exact imported identity `Platformˉfile.Sourceˉfile` is the corresponding
hosted-resource gate. Its alias must resolve to an edition-1 module whose header
name is exactly `Platformˉfile`; a same-spelled user declaration does not gain
the identity. The binding uses private shape `805306369`. It is representation
hidden, move-owned, forbidden in record fields and other aggregate storage, and
admitted only in the exact root signature `Main(Sourceˉfile) -> i32`. The
parameter is by value; a borrowed parameter, a result, a second parameter, a
non-root use, or an unexported entry rejects before symbol evidence publishes.
The module gate establishes compiler identity, not package authentication or a
filesystem grant.

The compiler-supplied `Sequenceˉlength` and `Sequenceˉat` call spellings use
the same exact owner gate. The query must be qualified, its alias must resolve,
the selected member must match exactly, and the target module header must name
`Foundationˉcollections`. The symbol phase returns only the owning module
identity; it does not publish a forgeable function declaration or infer an
intrinsic from the member name alone.

`Platformˉfile.Sourceˉlength` uses the same qualified-owner rule. It is a
compiler-supplied one-argument operation only when the selected member and
module header match exactly. It does not create an ordinary callable
declaration, open a file, recover a path, or grant a provider.

Nominal indices are deterministic and independent of source order: all records sorted by ordinal name receive the first indices, then all enums sorted by ordinal name. The current global nominal namespace makes identical names unambiguous.

Constants are currently permitted only in WVSS module zero. Their names use ASCII `ALL_CAPS_WITH_UNDERSCORES`; their explicit type is `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `rune`, `bool`, or a visible enum; and their initializer may use matching literals, enum members, earlier constants, parentheses, and the currently admitted exact-type operators. Boolean `&&` and `||` evaluate left to right and skip their right operand when the left value determines the result; invalid, unresolved, or would-overflow syntax on a skipped path therefore does not reject the constant. Calls, data reads, allocation-bearing expressions, evaluated forward/cyclic references, unsupported operators, mismatched types, and checked overflow/underflow fail before symbol evidence is published. Wide integers are evaluated with explicit low/high `u32` limbs; narrow signed constants retain sign plus bounded magnitude until WIR emits their exact named-width two's-complement bits; rune constants retain one already validated Unicode scalar. No width or overflow behavior inherits the host runtime.

## Conditional WVSD 1.1 and WVSD 1.2 declaration directory

All integers are unsigned little-endian. The directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVSD` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `1` or `2` |
| 8 | 4 | Entry count |
| 12 | 4 | Fixed entry size `24` |

Each entry contains six `u32` fields in this order: WVSS module index,
declaration-kind value, declaration byte offset, name byte offset, name byte
length, and declaration item count. Imports are excluded. Values `1` through
`8` retain import, capability, data, record, enum, function, constant, and
variant identity. `Foreign = 9` is the stable appended Foreign identity; values
`10` through `12` retain the fixed-array, owned-vector, and immutable-sequence
identities. Entries use canonical WVSS module order and source declaration
order.

Source with no Foreign declaration selects WVSD 1.1 and preserves its existing
bytes. Source containing at least one Foreign declaration selects WVSD 1.2.
WVSD 1.2 changes no header or entry layout: it requires every parsed Foreign
declaration to have exactly one canonical kind-9 entry and forbids kind 9 in
WVSD 1.1. Foreign contributes one directory entry and the total-directory
bound, but does not increment the ordinary `Functions` count or create a
function body. The independent source-aware directory validator reparses WVSS
and requires the selected minor version to agree with whether Foreign occurs.

The directory length must be exactly `16 + EntryCount * 24`. `Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid` remains an exported strict validator that reparses every accepted source as a stream. The normal phase constructs counts and entries together, checks the complete binary shape, and compares every entry with its source declaration during the namespace pass. This preserves an independent canonical comparison without a redundant whole-source traversal.

## Internal lookup index

`Lookup` is a private conditional `WVSI 1.2` or `WVSI 1.3` acceleration index.
WVSD 1.1 selects WVSI 1.2; WVSD 1.2 selects WVSI 1.3. Any other pairing is
invalid. Both versions retain the same layout. Their 16-byte header contains
magic `WVSI`, the selected major/minor version, the WVSD entry count, and
bucket count `256`. The header is followed by 256 16-byte ranges. Each range
contains the first payload index, entry count, prior-record count, and
prior-enum count for one possible first UTF-8 byte. The bucket payload then
stores every WVSD directory index exactly once.

Two tables follow the bucket payload. The reverse table contains one `u32` for
each record, enum, and variant in canonical record-then-enum-then-variant order;
within each kind, exact emitted UTF-8 names are strictly increasing. Each
reverse entry maps that ordinal to one unique WVSD directory index. The forward
table contains one `u32` for each WVSD entry and maps every nominal declaration
back to that exact canonical ordinal; every nonnominal entry must equal the
total-nominal-count sentinel. A one-byte metadata entry then follows for
every WVSD entry: bit 0 records `export`, bit 1 records an `async` function, and
bits 2 through 7 are zero. The bounded module-alias matrix follows.

The index then appends 256 callable ranges of two `u32` fields followed by one
`u32` WVSD directory index per declaration. A declaration's bucket starts with
`(Module + UTF8Length) mod 256` and folds each exact name byte as
`(Hash * 33 + Byte) mod 256`. A query uses its resolved target module and
unqualified member bytes. A bucket match remains only a candidate: consumers
still require exact target module, declaration kind where applicable, UTF-8
byte length, and ordinal byte equality. Hash collisions therefore cannot alter
name resolution. The cached emitted-name directory and payload follow this
callable region and retain their separate bounds. WVSI 1.3 places every
kind-9 Foreign declaration in this same callable index. Its metadata retains
the export bit but requires the asynchronous bit to be zero for Foreign.

`Compilerˉsourceˉsymbolsˉlookupˉshapeˉisˉvalid` rescans the
supplied WVSS rather than accepting caller-provided scan offsets, then validates
the paired WVSD version and shape before trusting the lookup. It checks the exact
WVSD/WVSI version coupling, module and entry counts, bounded primary and
callable ranges, in-range payload indices, canonical nominal reverse order and
uniqueness, exact forward/reverse correspondence, the nonnominal sentinel,
metadata bits, and the emitted-name directory. Emitted-name offsets must form
one canonical contiguous payload, each name length is `1..255`, and the final
name byte must end at the exact lookup length; truncation and trailing bytes
are rejected.

For a non-root module, a Foreign entry uses emitted-name tag `X`, producing the
same deterministic form as `__WvM<module>X<foreign-ordinal>`. The distinct tag
prevents collision with the ordinary function tag `F`; the existing root-name
collision suffix rule remains unchanged.

Exact nominal lookup searches the reverse table in two bounded passes: first the record range for the requested first UTF-8 byte, then the corresponding enum range. Unequal byte lengths are rejected before ordinal comparison. This preserves record-then-enum identity and avoids scanning unrelated WVSD entries.

Name equality remains exact ordinal UTF-8 comparison over validated absolute
WVSS spans. Alias and callable comparison read those spans directly from the
packed source set; they do not materialize the complete owner or target module.
Construction is deterministic and total even before duplicate-name rejection.
The index never changes namespace semantics and is not a separately published
compatibility format.

The current construction and strict-validation algorithms are bounded but do
not claim linear scaling. For `D` WVSD entries and `N` nominal entries,
emitted-name construction, namespace bucket searches, and nominal
forward/reverse construction have bounded-superlinear worst cases, including
`O(D^2 + N * D)`. The 256 primary and callable bucket passes are bounded by the
same entry limits, and an ordinary callable query examines the exact candidates
in one selected bucket. These private acceleration structures prevent
source-wide scanning for an individual ordinary lookup; they do not by
themselves make complete WVSI construction or source-symbol validation linear.

## Visibility matrix

`Visibility` is exactly `Modules * Modules` bytes in row-major owner/target order. Every byte is zero or one. Each module sees itself and its direct imports; deterministic transitive closure adds indirect imports. With the WVSS limit of 64 modules, the matrix is at most 4,096 bytes. A type declared in module `Target` is accessible from module `Owner` only when the corresponding byte is one.

## Deterministic processing order

The phase validates in this order: source graph; aggregate counts plus WVSD construction; directory and paired lookup shape; namespaces, canonical entry correspondence, constant names, root-only placement, and capability policy; visibility construction; then constants, records, enums, and function signatures in canonical WVSS module/source order. A constant recursively evaluates only earlier constant declarations, with a depth bound of 64. Within a declaration, members and expression operands are checked in source order. Inputs containing multiple faults receive the first failure under this order.

Failure evidence names the current module, a related prior/target module when applicable, declaration kind, name/token byte offset, and one-based line/column. `Modules` is the sentinel when no related module exists.

## Candidate artifacts and retained qualified evidence

- `Source-Symbols-Core.wvb`: 445,949 bytes, SHA-256 `66bb79c971a9b4802ad212c55a4f0cfe7ae90a436e6018a9637c2792802932f5`.
- `Source-Symbols-Demo.wvb`: 457,955 bytes, SHA-256 `2d469de3fa57b6071c0e02b6addabee0cfe5252252187754d858661f611cdcb8`.
- `Source-Symbols-Tool.wvb`: 444,686 bytes, SHA-256 `4173ab97e3e9fcc824e08cbd75d6db2f44654a8e9917034900ff60943ae4d68a`.

Decision 0517 reproduces all three identities through the current-Windows
native Project front door and natively inspects the core portable type/export
surface. Independent Linux execution and native demo/tool execution remain
pending.

The candidate demo additionally exercises valid constants, enum and earlier-constant references, Boolean short-circuiting over invalid or would-overflow skipped operands, invalid names/types/initializers, exact type mismatch, forward reference, checked overflow, and imported-module rejection. The hosted tool retains namespace/signature reporting; constants contribute WVSD entries but deliberately do not change the existing public aggregate `Data` count. The current local whole-compiler closure report is:

```text
source symbols status=Valid modules=8 capabilities=0 data=0 records=31 enums=14 functions=206 fields=344 members=245 parameters=897 directory-bytes=6040 visibility-bytes=64
```

The pre-index implementation was qualified at `d57a6d8`, and the first indexed implementation at `bf77f70`. Decision 0050's implementation is qualified at `e37204f`; it advanced the private acceleration contract from `WVSI 1.0` to `WVSI 1.1`. Decision 0055 added bounded reverse-table lookup and is cross-host qualified at `1a4fca7`. Decision 0058 changed equality-only implementation paths and embedded artifact bytes while preserving the then-current WVSD 1.0 and WVSI 1.1 formats and is cross-host qualified at `5c16547`. WVSD 1.1, typed constants, and the proposed conditional WVSD 1.2/WVSI 1.3 Foreign path are candidate behavior and do not inherit those cross-host claims.
