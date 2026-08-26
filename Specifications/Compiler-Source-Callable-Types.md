# Windvale source callable-type catalog

## Status and boundary

`Compilerˉsourceˉcallableˉtypes` connects validated source declarations,
concrete WVLB bindings, typed WVIR, and exact WVEF effects to the structural
function identities in WVFT. Successful analysis publishes compiler-private
`WVCF 1.0`: one disposition for every prepared WVIR function entry followed by
the exact WVFT catalog used by all concrete callable entries.

WVCF is post-effect compiler evidence. It is not a function value, closure
environment, code address, WVB type, import, export, or authority grant. Source
value expressions, WVIR operations, the closed WVB representation, verifier
rules, and runtime/native execution remain separate consumers of this catalog.

Every integer is unsigned little-endian. Every length and offset is exact. No
partial directory is published.

## Prepared-input contract

The phase consumes immutable evidence already validated by the owning source
symbol, generic binding, WVIR, and effect phases. It checks each phase status
and the cross-artifact identities it relies on. For WVIR it additionally
checks the complete structural envelope and the exact contiguous function,
block, operation, temporary, and operand ranges before reading a function
entry. It does not repeat the preceding phase's operation-by-operation semantic
proof.

This separation is deliberate. Re-running the complete WVIR verifier would
reparse and revalidate the same immutable evidence and would pull the entire
WVIR compiler implementation into this phase's build closure. Untrusted
serialized WVIR must still enter through the full WVIR validation boundary;
the prepared callable API is not an alternate admission path.

The analyzer parses each of at most 64 module headers once and retains a
four-byte profile value per module. It parses each concrete function signature
once in declaration order and constructs its parameter evidence in one linear
pass. It does not rescan earlier parameters or reparse a module header per
function.

## Function dispositions

Each function-entry ordinal receives exactly one disposition:

| Value | Disposition | Meaning |
| ---: | --- | --- |
| 0 | `Nonˉfunction` | The corresponding WVEF entry is not executable function evidence. |
| 1 | `Genericˉtemplate` | The source declaration still contains generic parameters and is not a materialized concrete specialization. |
| 2 | `Legacyˉvoid` | The prepared WVIR result shape is zero. It remains classified for migration and cannot become a WVFT callable value. |
| 3 | `Borrowedˉresult` | The concrete signature returns an immutable or mutable borrow. WVFT 1.0 deliberately does not erase that lifetime requirement into a by-value result. |
| 4 | `Callable` | The concrete non-void, by-value-result function has one exact WVFT instance. |

A materialized generic specialization may be callable even though its source
declaration is generic: its concrete WVLB/WVIR function ordinal is outside the
unspecialized source-symbol entries and supplies exact parameter and result
shapes. The unspecialized template itself remains disposition 1.

Concrete identity includes declaration parameter order, every binding-derived
parameter shape, every source transfer mode, result shape, `async`/`unsafe`
flags, module profile, and exact transitive WVEF masks. Parameter names are not
WVFT identity. Two functions with the same complete identity reuse the first
WVFT instance while retaining distinct WVCF function entries.

## WVCF 1.0 directory

The directory begins with this 32-byte header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVCF`. |
| 4 | 2 | Major version `1`. |
| 6 | 2 | Minor version `0`. |
| 8 | 4 | Function-entry count. |
| 12 | 4 | Function-entry size, exactly `8`. |
| 16 | 4 | Embedded WVFT catalog bytes. |
| 20 | 4 | Function-entry offset, exactly `32`. |
| 24 | 4 | WVFT catalog offset, exactly `32 + functions * 8`. |
| 28 | 4 | Exact total directory bytes. |

Each function entry is eight bytes:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Disposition value `0..4`. |
| 4 | 4 | WVFT instance for disposition 4; otherwise exactly `0xffffffff`. |

The embedded catalog is one independently valid `WVFT 1.0` catalog, including
its own 24-byte header. Callable entries may reuse an earlier instance. The
first reference to instances must occur in ascending instance order, and every
catalog instance must be referenced. Thus injected, reordered, forward, or
unreferenced type identities reject without a separate unbounded graph walk.

## Bounds and rejection

| Boundary | Maximum |
| --- | ---: |
| Prepared functions | 87,380 |
| Source modules | 64 |
| Retained WVCF directory | 2,097,152 bytes |
| Embedded WVFT instances | 256 |
| Parameters per WVFT instance | 64 |
| Aggregate WVFT estimated output | 16,777,216 bytes |

Validation rejects bad magic or versions, inconsistent counts or offsets,
truncation, trailing bytes, invalid dispositions, wrong sentinels, invalid or
unreferenced WVFT instances, malformed WIR structure, invalid source profiles,
unknown declaration/function relations, signature/binding mismatches, invalid
effect evidence, and every retained or estimated limit crossing. Public
disposition and instance accessors independently bound their entry reads, so a
truncated directory returns the non-callable/sentinel result rather than
trapping.

## Runtime separation

WVCF answers which exact callable type a concrete named function can inhabit;
it does not itself create a runtime value. WVIR 1.17/1.18 consumes that answer
for the closed noncapturing, effect-free, by-value profile, embeds reduced WVIC
evidence, and lowers named-function references plus exact local indirect calls.
WVIR 1.19/1.20 additionally binds a copied plain-capture prefix to the same
public descriptor. WVB 1.30/1.31 replaces every private WVFT shape with a
kind-8 Types descriptor and shape `35`, and its verifier/runtime own the
portable value representation.

This does not make WVCF a serialized format or a runtime address directory.
Source closure-body lowering, move and borrow environments, borrowed callable
signatures, nonempty effects, flags, escape ownership, and native callable ABI
lowering still require later explicit representations and proofs.
