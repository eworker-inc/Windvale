# Windvale Language 1.0 foreign-memory semantics

## Status and scope

`Compilerˉsourceˉforeignˉsemantics` is the bounded rule-level oracle for the
first accepted System/FFI caller-owned memory profile. It models the exact
64-byte registered buffer ABI without lowering a native call, allocating host
memory, dereferencing a host pointer, or authenticating a foreign catalog.

The oracle consumes two kinds of upstream evidence:

- a validated `Compilerˉsourceˉtargetˉdescriptor` for the exact
  `linux.x86_64.sysv_amd64_c_v1` target; and
- normalized callable identity facts that an Analyzer adapter may construct
  only after the WVFC record has been authenticated against its admitted WVSS
  and WVAE identities.

The normalized callable record is not an authentication authority. The oracle
accepts no WVFC bytes and exposes no alternate parser. This checkpoint therefore
does not claim end-to-end WVFC authentication or native ABI conformance.

The current implemented compiler slice cannot yet express frozen Language 1.0's
representation-hidden `opaque <class> type` surface or its compiler intrinsics
honestly. Their identities remain compiler-owned; this checkpoint does not
publish forgeable Foundation record substitutes.

This checkpoint follows accepted Decisions
[0764](../Documents/Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
[0883](../Documents/Decisions/0883-Open-Slice-8-With-The-Exact-System-Ffi-Front-Door.md),
[0884](../Documents/Decisions/0884-Preserve-The-Slice-8-Native-Data-Bound.md),
and
[0886](../Documents/Decisions/0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md),
[0887](../Documents/Decisions/0887-Use-A-Separately-Bounded-Admission-Validator.md),
[0888](../Documents/Decisions/0888-Publish-The-Canonical-WVFC-Producer.md),
and
[0889](../Documents/Decisions/0889-Publish-The-Bounded-System-Ffi-Foreign-Memory-Semantic-Oracle.md).

## Fixed bounds

The registered profile admits one positive allocation of at most 64 bytes with
power-of-two alignment at most 8, one positive exclusive region, one pointer,
one foreign call, 16 diagnostics, and 524,288 retained evidence bytes. Only the
registered call requires capacity exactly 64 and actual destination alignment
8. Each completed allocation, region, pointer, or call transition retains
exactly 64 bytes and checks headroom before provider observation or successor
construction. State validity derives a checked minimum from those four history
counts; unrelated preloaded evidence above the minimum remains permitted. Slice
borrow/release is lexical lifetime state and does not add retained evidence.
The one-shot history counts do not become reusable after release.

## Effects and unsafe context

Scratch construction requires `memory.allocate` bit 1 and is not itself unsafe.
Region construction, pointer extraction, named non-null validation, and
dereference admission require `unsafe.address` bit 128 plus lexical unsafe.
Foreign-call admission requires `ffi.call` bit 256 plus lexical unsafe. No bit
implicitly grants System profile or lexical unsafe context.

## State validity

A valid state carries the exact registered ABI. Allocation, region, and pointer
counts are at most one. A zero history count requires generation zero; a count
of one requires a retained nonzero generation even after release.
History is monotonic: `call <= pointer <= region <= allocation`. Retained
scratch, region, and pointer generations remain equal through that hierarchy.
An uncalled dead pointer and a completed call with a live pointer are invalid
temporal states.

A live scratch has positive bounded length, power-of-two alignment, a nonzero
aligned base, and a representable inclusive extent `Base + Length - 1`. Dead
scratch payload length, alignment, and zeroed flag are canonical zero/false
values. A live region has positive length, is within its scratch, aligned,
generation-matched, and has representable native start and exclusive end. Dead
region address, length, and alignment payloads are zero. A live pointer is tied
to the live region generation; pointer generation may remain after death as
anti-reuse history.

One immutable scratch slice may be live only while scratch is live and no
region or pointer is live. Its normalized borrow generation must equal the
scratch generation. Region construction and scratch release reject while the
slice is live; explicit oracle release ends the slice before either transition.
This state models the Analyzer's lexical borrow and is not new Foundation
syntax or a public dereference API.

Every operation returning `Valid` first establishes this state predicate.
Structurally invalid or contradictory state returns `Invalidˉevidence`; it is
not collapsed into a lifetime failure. The exported state record is a normalized
rule-simulation shape, not provider or provenance authority. A future
compiler-owned opaque state and authenticated Analyzer transition own provenance
before these facts may guide lowering.

## Scratch and region admission

Scratch construction checks, in order: System profile, `memory.allocate`, exact
target, exact ABI, input state, length, alignment, allocation history, budget,
evidence headroom, provider outcome, and provider address witness. A null,
misaligned, or wrapping provider address maps to the existing
target-unaddressable allocation failure and publishes no owner.

The inclusive rule permits aligned base `18446744073709551552` with length 64:
its last byte is `u64::max`. A whole-allocation region still fails
`Addressˉoverflow` because its exclusive native end is not representable. This
preserves rejected case 21 without admitting a forged owner.

Region construction checks, in order: System profile, lexical unsafe,
`unsafe.address`, target, ABI, state/lifetime, relative arithmetic, native start
and exclusive end, owner extent, alignment, exclusivity, region history, and
evidence headroom. Failure publishes no region or pointer and leaves the owner
unchanged.

## Pointer and call admission

`Requireˉnonˉnull` proves only the named nullable-to-non-null transition after
profile, unsafe-context, effect, ABI, and state validation. It does not prove
extent, alignment, or aliasing. Null is valid on an empty state; non-null
success requires an actual live, generation-matched pointer and region rather
than a caller Boolean alone. Null is accepted only with exact empty
allocation/region/pointer/call history and no live scratch, region, pointer, or
slice. Contradictory null/non-null facts are `Invalidˉevidence`. The operation
does not accept a separate lifetime proof or admit dereference.

`Compilerˉsourceˉforeignˉadmitˉdereference` is the only dereference admission
operation in this compiler rule model. It accepts normalized Analyzer facts
bound to one pointer/owner generation and concrete offset, length, and required
alignment. Those facts are simulation input, not authority. The oracle
independently derives live lifetime, initialized storage, alias freedom, checked
extent, native address, alignment, and generation agreement from validated
state. Frozen Foundation 1.0 publishes no general pointer-dereference function;
the generic pointer-operation API rejects dereference so it cannot bypass this
compiler-only rule. Integer conversion, serialization, escape, and retention
are also rejected.

Foreign-call admission requires the exact normalized callable identity, System
profile and effects, no-retain and no-unwind declarations and behavior, a live
generation-matched pointer, one-call history, and capacity equal to both the
region length and registered constant 64. The actual destination must also meet
the registered 8-byte alignment. Capacity above the live region is
`Outˉofˉrange`; an in-range capacity or region that differs from registered 64
is `Invalidˉcallˉcontract`. Evidence headroom is checked before observing
simulated retain/unwind behavior. Success ends pointer lifetime and retains the
region for post-call observation. A forbidden retain or unwind is terminal and
returns a poisoned invalid state with all live authority scrubbed. One-shot
enforcement follows from the consumed pointer lifetime; there is no separate
unreachable call-limit failure path.

## Evidence level

The fixture gives rule-level evidence for rejected cases 3 and 4, 11, and
15–30. Case 5 is stronger source-to-WIR graph evidence from the separate exact
compile-rejection project. Cases 3, 4, 11, and 15–30 are not represented as
source-front-door or authenticated-WVFC proof in this checkpoint.

The current native compiler accepts the focused project and two preflight
builds produce a byte-identical 147,912-byte WVB with SHA-256
`1ba359cc2372a43ba941d4b2baadd926774b706bfd5e1eeaca8a287329772003`.
Source-to-WIR publication proves that the complete oracle and its 29 rule
selectors bind and lower together. The focused owner bounds the host-side
candidate at 4 MiB, requires byte-identical rebuilds, delegates the candidate's
single complete verification to the generic segmented-hosted-WVB cache, and
rechecks its bytes after execution. A narrow x64 preflight stages the current
WVB to a 5,297,182-byte object set in 7 chunks with a 108-byte manifest. The
focused owner packages it directly; no scalar-interpreter runner or
duplicate verification closure is required.

An initial focused-owner execution caught a precise lifecycle defect: live
slice authority was classified as generic `Aliasing` during region construction
instead of `Sliceˉaliasing`. The corrected rule tests `Sliceˉlive` first and
retains `Aliasing` for live region or pointer authority. A corrected cached
profile-7 selector-`y` probe returns 42 with no output. The corrected focused
owner then passes its termination probe, byte-identical build and rebuild,
exact profile rejection, sole complete verification, immutable cache hit at
key `e2f5a700bd1fd84f9012780dd1a26f0e60526eb6532cb01742e4561f29ea6408`,
four negative dispatch probes, all 29 isolated rule selectors, and the exact
two-module source-graph rejection.

The prior Windows profile-7 application identity and its 29-selector execution
belonged to the superseded 121,635-byte WVB. The corrected 147,912-byte owner
result above is the current local Windows development evidence.

The separate two-module case-5 project is compile-rejection evidence. It asks a
portable/Core-equivalent root to import a System dependency and requires exact
`Dependencyˉprofile` rejection with valid source-set, parse, and body evidence,
plus no output artifact. It does not duplicate the source-graph implementation
or execute a rejected module.

This is current-host rule-level evidence, not cross-host qualification,
source-to-native ABI lowering, or native-ABI behavior.
