# Windvale Seed bytecode specification

## Status

This document specifies the current Windvale bytecode family: the WVB 1.11
baseline, the WVB 1.12 fixed-width-integer extension, the WVB 1.13 rune
extension, the WVB 1.14 floating-point extension, and the WVB 1.15 `unit` and
`never` extension, the WVB 1.16 named variant-field extension, the WVB 1.17
fixed-array extension, the WVB 1.18 Vector and Sequence type-representation
extension, the WVB 1.19 scalar Vector/Sequence execution extension, and the
WVB 1.20 owned-Vector local-transfer extension, the WVB 1.21 launcher-owned
memory-budget entry extension, the WVB 1.22 exact `u8`-backed-enum extension,
   the WVB 1.23 executable memory-budget Split extension, the WVB 1.24
   fallible reserved-Vector-construction extension, the WVB 1.25 recoverable
   Vector-append extension, the WVB 1.26 owned-Vector-call extension, the
   WVB 1.27 reserved-Vector-growth extension, the WVB 1.28
   Vector-containing-aggregate ownership extension, the WVB 1.29
   rights-limited source-file snapshot extension, the WVB 1.30 exact
   noncapturing-callable extension, the WVB 1.31 plain-capture closure
   environment extension, the WVB 1.32 structured-task extension, the WVB 1.33
   unsafe-scratch extension, the WVB 1.34 immutable borrowed-memory-budget
   call extension, the WVB 1.35 immutable borrowed-scratch observation
   extension, the verified WVB 1.36 write-region extension, and the
   compiler-verified and contained-execution candidate WVB 1.37 write-pointer
   extension, and the source-publication candidate WVB 1.38 registered
   Foreign-call extension.
Windvale is in early
development and does not preserve obsolete experimental WVB encodings unless a
named compatibility case is approved. WVB 1.11 includes 64-bit scalars,
independent module metadata, nominal payload variants, bounded sequences and
affine builders, division and remainder, unsigned bitwise/shift operations,
exact text/bytes equality, and exact little-endian `u64` byte codecs. WVB 1.12
adds first-class `i8`, `i16`, and `u16` shapes and their checked scalar
instruction family. WVB 1.13 adds a first-class Unicode-scalar `rune` shape,
constant, equality, and inequality. WVB 1.14 adds first-class IEEE 754
binary32 and binary64 shapes and one exact floating-point instruction family.
WVB 1.15 adds an ordinary one-value `unit` shape, a return-only uninhabited
`never` shape, and the canonical unit constant. WVB 1.16 adds bounded
multi-field variant metadata and exact named field extraction while retaining
the zero-field and one-field metadata bytes. WVB 1.17 adds exact nominal
fixed-array metadata, array values, construction, and checked `u64` indexing.
WVB 1.18 adds exact runtime-budgeted Vector and immutable Sequence type
metadata without adding collection operations or a storage representation.
WVB 1.19 adds a first runtime-backed scalar operation subset with explicit
linear Vector evidence.
WVB 1.20 adds an exact local transfer that moves that evidence without
retaining the backing or leaving a second mutable owner.
WVB 1.21 adds one representation-hidden owned token supplied by the launcher
as the sole parameter of exported `Main`. Source bytecode cannot construct,
copy, store, return, or embed the token.
WVB 1.22 adds an exact one-byte member representation for edition-1 enums
whose declared backing is `u8`. The ordinary enum value shape and instruction
family are unchanged.
WVB 1.23 adds affine budget locals and one exact `memory_budget.split`
instruction backed by bounded provider accounting. It does not expose the
token representation, allocate application storage, or add ambient authority.
WVB 1.24 adds exact fallible reserved Vector construction. It consumes one
budget, produces an affine typed Result, and binds allocation lifetime to the
Vector descriptor without exposing a lease or target heap representation.
WVB 1.25 adds exact all-or-nothing append through one exclusive mutable Vector
local. Success consumes the item and produces `Valid(unit)`; capacity refusal
preserves the Vector and returns the item in the exact typed failure value.
WVB 1.26 encodes exact Vector parameter transfer modes in the parameter shape,
executes ordinary by-value and borrowed calls, and releases callee-owned and
temporarily retained descriptors in deterministic reverse slot order.
WVB 1.27 adds explicit fallible replacement growth through one mutable Vector
local and one distinct mutable budget slot. Refusal preserves both; success
atomically swaps the complete backing and lease.
WVB 1.28 recursively classifies records, variants, and fixed arrays that contain
an owned Vector. Whole values move through construction, local storage, calls,
and returns; field and element observation uses a verifier-confined borrowed
view; and runtime teardown releases nested Vector descriptors deterministically.
WVB 1.29 adds one representation-hidden source-file owner supplied by the
launcher as an immutable snapshot. The exact source entry may move that owner
to a local and observe its bounded byte length without exposing a host path,
handle, byte pointer, provider object, or ambient filesystem authority.
WVB 1.30 adds exact first-class references to named noncapturing functions and
typed indirect calls. Callable identity is structural and portable: it contains
the module profile, result shape, and ordered parameter shapes, never a host
address, source spelling, or native ABI layout.
WVB 1.31 adds one bounded immutable closure environment for copied inline
scalars and enums. The environment is created explicitly, remains private to
the runtime, and prepends its captured values to the target's physical
parameters when the existing typed indirect call executes.
WVB 1.32 adds exact async/effect/mode evidence to callable descriptors, opaque
scope/context/task identities, and six affine task instructions. The first
portable execution profile schedules accepted children sequentially while
preserving the same typed outcomes, lexical joins, cancellation state, work and
call-depth limits, retained completion ownership, and deterministic teardown
required of a parallel-capable host.
WVB 1.33 unsafe-scratch publication and bounded scalar execution are specified
below. The Language 1.0 source backend emits it, the complete compiler-aligned
verifier admits it, and the source-built native scalar runner supplies its
first capability-free System-profile allocation oracle.
WVB 1.34 adds one non-owning shape for an immutable borrowed
`Memoryˉbudget` call. The verifier, scalar runner, and native x86-64 lowerer
confine that view to one canonical direct-call sequence while preserving the
caller's shape-`25` owner.
WVB 1.35 adds exact `Scratchˉlength` observation over an immutable borrowed
`Foreignˉscratch<Abi>` view. It returns the retained `u64` extent without
consuming the scratch owner or exposing the backing address.
WVB 1.36 adds exact write-region validation over a mutable scratch borrow. Its
compiler-aligned verifier, bounded scalar provider, and native x86-64 lowerer
retain only an affine opaque region and checked logical geometry.
Candidate WVB 1.37 serializes exact write-pointer derivation from an immutable
region borrow. The complete compiler-aligned verifier confines the pointer as an
affine value. The bounded scalar provider and native x86-64 lowerer execute the
contained derivation by carrying only its private logical region descriptor.
This candidate forms no address and grants no Foreign-call authority.
Candidate WVB 1.38 serializes the first authenticated and paired registered
Foreign call. Its instruction carries a stable binding identity plus exact
pointer-record and ABI-enum type identities; it does not carry a symbol string,
address, library path, capability, or authentication certificate. The complete
verifier and every execution consumer remain closed to this version.
A canonical writer emits the lowest required
minor version: 1.11 when no later extension is present, 1.12 for fixed integers,
1.13 for rune evidence, 1.14 for floating-point evidence, 1.15 for unit or
never evidence, 1.16 for multi-field variant metadata or field extraction, and
1.17 for any fixed-array type, value shape, construction, or indexing operation,
1.18 for any Vector or Sequence descriptor or value shape, 1.19 for any
Vector or Sequence execution operation, 1.20 for `local.take`, and 1.21 for the
exact launcher-owned memory-budget entry. WVB 1.22 is selected for any kind-7
`u8` enum descriptor, including a module that also carries the budget entry.
WVB 1.23 is selected when `memory_budget.split` is present. WVB 1.24 is selected
when `vector.construct_reserved_fallible` is present, and WVB 1.25 when
`vector.append_fallible` is present. WVB 1.26 is selected when any function has
an exact Vector parameter. WVB 1.27 is selected when
`vector.grow_reserved_fallible` is present. WVB 1.28 is selected when a
record, variant, or fixed array recursively contains an owned Vector. WVB 1.29
is selected when the exact source-file entry, shape `34`, or `source.length`
operation is present. WVB 1.30 is selected when a callable descriptor,
shape `35`, `function.reference`, or `call.indirect` is present without a
closure environment. WVB 1.31 is selected when `closure.create` is present
without structured-task evidence. WVB 1.32 is selected when a task instruction
or extended callable descriptor is present. WVB 1.33 is selected when
`unsafe.scratch.construct` is present, including when inherited task evidence
is also present. WVB 1.34 is selected when any function has an immutable
borrowed `Memoryˉbudget` parameter. WVB 1.35 is selected when
`unsafe.scratch.length` is present. WVB 1.36 is selected when
`unsafe.write-region.borrow` is present. Candidate WVB 1.37 is selected when
`unsafe.write-pointer.borrow` is present. Candidate WVB 1.38 is selected when
`foreign.call` is present. The compiler-aligned verifier accepts all twenty-seven
versions through candidate WVB 1.37 and never admits an
extension under an earlier header. The source-built native scalar runner
accepts WVB 1.33 through candidate WVB 1.37 only through the bounded provider
below. The native x86-64 lowerer admits the same focused unsafe-scratch,
immutable-budget-borrow, scratch-observation, write-region, and contained
write-pointer matrix. Candidate WVB 1.38 has only source-writer and focused
independent structural-reader evidence; the complete verifier, scalar and native
execution, browser, WebAssembly-package, and Windvale OS consumers retain
explicit narrower version boundaries until their own slices land.

## Verified WVB 1.33 unsafe-scratch publication and scalar execution

WVB 1.33 represents exact
`Foundationˉunsafe.Constructˉscratch::<Abi>`. Its file header uses major `1`,
minor `33`, and the unchanged seven-section envelope. It inherits every WVB
1.32 type and instruction encoding; a module that carries an extended callable
descriptor retains the WVB 1.32 descriptor trailer unchanged.

The new instruction is:

```text
DC unsafe.scratch.construct u32 budget-local index,
                            u32 construction-Result type index,
                            u32 ABI-enum type index
```

It consumes ordered `u64` length and alignment stack operands, consumes the
named available `Memoryˉbudget` local, and produces the exact affine
`Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>`. The ABI immediate must
name a declared enum and binds the opaque materialized scratch nominal to that
ABI inside WVB; all instructions naming that scratch nominal must use the same
ABI. Every WVB 1.33 module contains 1 through 4,096 `DC`
instructions and at most 256 distinct scratch-nominal/ABI bindings. `DC` is
invalid under every earlier minor.

WVB 1.33 widens shape `25` beyond the earlier launcher-only profiles:
it may appear as an exact by-value function parameter or an affine
non-parameter local in any function. It remains invalid as a function result,
borrowed parameter, new nominal field or payload, collection element, or Types
entry. The inherited exact
`Result<Memoryˉbudget, Allocationˉfailure>` layout remains admitted; the new
widening does not authorize any other nominal budget payload. Calls and local
transfers move the opaque owner; ordinary loads never copy it. `DC` must name
an available shape-`25` parameter or local in its own function and consumes
that owner on every ordinary Result path.

The current source writer, compiler-aligned verifier, and focused hostile-input
oracle implement this format. The verifier checks complete structure,
canonical materialized Foundation layouts, typed stack behavior, affine local
and control-flow ownership, and the bounded scratch/ABI relation.

The source-built scalar runner accepts WVB 1.33 ordinary execution only for a
System-profile module with no declared host capabilities and 1 through 4,096
`DC` instructions. Its first provider accepts lengths 1 through 64, accepts
power-of-two alignments through 8, constructs an exact budget lease, and
zero-initializes the bounded backing allocation. Invalid length, invalid
alignment, budget refusal, and heap refusal use the canonical failure shapes.
The visible scratch carrier is the non-address-like value `1u64`; the backing
allocation and lease remain private to the interpreter and are finalized by
invocation teardown. This oracle does not expose a pointer, implement a native
ABI, or grant a capability. The separately verified native x86-64 lowerer
accepts the same bounded scratch subset; browser, WebAssembly-package,
published-front-door, and Windvale OS execution remain closed.

## Verified WVB 1.34 immutable borrowed memory-budget calls

WVB 1.34 inherits the WVB 1.33 System profile, metadata, type vocabulary,
instructions, and bounds. It reserves value shape `36` for a non-owning
immutable view of the exact shape-`25` `Memoryˉbudget` owner. Shape `36` is
valid only as an immutable borrowed function parameter and as a
compiler-generated non-parameter local. It is invalid as a function result,
nominal field or payload, collection element, callable-descriptor shape,
mutable borrow, or owned value. Every WVB 1.34 module contains at least one
shape-`36` parameter.

The only local construction is the adjacent sequence:

```text
local.load  <available shape-25 owner>
local.store <shape-36 view local>
local.load  <same shape-36 view local>
local.load  <zero through 64 remaining argument locals>
call        <target whose corresponding parameter is shape 36>
```

The view load cannot be replaced with `local.take`, and no other instruction
may intervene before the direct call except the bounded remaining argument
loads. A shape-`36` parameter cannot itself be loaded in this first profile;
the callee accepts the observation boundary, but no budget-query operation is
yet defined. Constructing and passing a view does not change the owner's affine
availability. The caller may subsequently move that same owner once, including
to `unsafe.scratch.construct`.

The scalar interpreter and native x86-64 backend use one opaque `u64` cell for
both representations, but validation retains identities `25` and `36`
separately. The shared cell is an internal carrier, not a public token layout,
pointer, provider handle, implicit copy, or new allocation authority. WVB 1.34
may inherit opcode `DC` when present, but a borrow-only module need not contain
one.

## Verified WVB 1.35 immutable borrowed unsafe-scratch observation

WVB 1.35 inherits the WVB 1.34 System profile, metadata, type vocabulary,
instructions, ownership rules, and bounds. It additionally admits shape `28`
as an immutable borrowed helper parameter only when its nominal type is the
exact canonical `Foreignˉscratch<Abi>`. Compiler-generated shape-`28` locals
retain the same nominal identity and remain confined to the canonical owner-
load/view-store/view-load/direct-call sequence. The owned shape-`7` scratch
remains the sole affine owner.

The new instruction is:

```text
DD unsafe.scratch.length u32 scratch-local index,
                         u32 ABI-enum type index
```

`DD` is nine bytes, consumes no operand-stack values, produces one `u64`, and
does not change scratch-owner availability. Its local must be an exact owned
scratch or immutable borrowed scratch view. Its ABI immediate must name a
declared kind-`2` or kind-`7` enum matching the scratch generic identity. Every
observed scratch/ABI relation must be covered by the module's construction
relation. A WVB 1.35 module contains 1 through 4,096 `DD` instructions and at
most 256 distinct length relations.

The scalar provider returns the exact accepted construction length stored in
the private scratch record. Native x86-64 lowering reads that same private
field through the validated record handle. Both are constant-time observations;
neither scans or copies backing bytes, consumes the owner, returns the carrier,
or exposes an address. Mutable borrowing, write-region and pointer operations,
authenticated Foreign calls, and cross-host containment remain outside this
profile.

## Verified WVB 1.36 bounded scalar write-region borrowing

WVB 1.36 inherits the WVB 1.35 System-profile metadata and instruction
vocabulary and adds one instruction:

```text
DE unsafe.write-region.borrow u32 scratch-local index,
                              u32 region-Result type index,
                              u32 ABI-enum type index
```

`DE` is 13 bytes. Before the opcode it consumes three ordered `u64` values:
`Start`, `Length`, and `Requiredˉalignment`. Its local names the exact owned
scratch or compiler-authenticated mutable borrowed scratch view. Its Result
names exactly
`Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>`, and its ABI
immediate names the declared enum bound to both scratch and region.

The source writer selects minor 36 only when WVIR operation `188` is present
and moves the exact Result affinely. The compiler-aligned verifier now admits
minor 36 under a non-executable containment profile. It requires the exact
seven-case `Foreignˉpointerˉfailure`, exact region Result, distinct one-token
scratch and region nominals, and a kind-`2` or kind-`7` ABI enum. The explicit
scratch/region/ABI relation is unique in a module and must agree with a scratch
construction relation when one is present.

The verifier consumes three exact `u64` stack values, marks the named scratch
unavailable through every branch and the remainder of the function, and
produces an internal affine region-Result value. That value may be moved,
stored, taken, discarded, and case-tested. Its Failure payload may be extracted
and matched to observe exact `Foreignˉpointerˉfailure` data. Its Valid region
payload and fields remain inaccessible. The Result may not be constructed
through ordinary variant operations, cross a direct or indirect call, appear
in a function signature, or be returned. A minor-36 module contains 1 through
4,096 `DE` instructions and at most 256 distinct 12-byte relation entries.

The bounded scalar runner embeds the compiler-aligned verifier and admits minor
36 to its provider only after that verifier succeeds.
For this minor, the private scratch field holds the provider's existing
eight-byte `{heap offset u32, length u32}` allocation descriptor rather than a
native address. WVB 1.33-through-1.35 retain their earlier private length
representation, and `DD` observes the descriptor length under minor 36.

`DE` requires an exact live scratch record, matching heap allocation and lease,
construction length from 1 through 64, and construction alignment through 8.
It applies zero-length, `u64` relative/base/exclusive-end overflow, owner-range,
and requested/actual-alignment checks in that order. Failure constructs exact
`Outˉofˉrange`, `Addressˉoverflow`, or `Misaligned` data and no region. Success
stores only a checked `{subrange offset u32, length u32}` descriptor in the
opaque region record. The region owns no second lease; normal function teardown
releases the scratch allocation and lease. Compiler containment makes dynamic
alias and stale-owner cases unreachable in this first scalar profile.

The compiler-verified native x86-64 lowerer admits the same five outcomes while
retaining only checked logical start and length. Browser and WebAssembly hosts
and OS consumers retain their narrower boundaries. Contained pointer execution
is defined below; authenticated Foreign calls and cross-host containment require
later checkpoints.

## Verified candidate WVB 1.37 contained write-pointer execution

WVB 1.37 inherits the complete WVB 1.36 System-profile metadata and instruction
vocabulary and adds one candidate instruction:

```text
DF unsafe.write-pointer.borrow u32 region-local index,
                               u32 pointer-record type index,
                               u32 ABI-enum type index
```

`DF` is 13 bytes, consumes no operand-stack values, and produces the exact
opaque `Foreignˉpointer<u8, Abi>` value named by its pointer-record immediate.
The region local must be an exact immutable borrowed
`Foreignˉwriteˉregion<Abi>` parameter or local encoded as shape `28`. Its
nominal type and the pointer nominal are distinct kind-`1` records, and the ABI
immediate names a kind-`2` or kind-`7` enum. The source writer validates the
complete canonical Foundation, generic, element, ABI, borrow, effect, and
opacity relationships before publishing these categorical WVB operands.

The writer selects minor 37 only when WVIR operation `189` is present; a
minor-37 module contains at least one `DF`. The focused independent reader
rejects an earlier minor, unknown opcode, invalid local or type index,
non-borrowed region shape, and equal region/pointer nominals.

The complete compiler-aligned verifier admits only an exact immutable borrowed
region parameter for `DF` in this checkpoint. It classifies the produced pointer
as verifier-internal affine kind `38`. The value may be popped directly or
stored and moved once through the compiler-generated
`local.store`/`local.load`/`local.store` sequence between two exact pointer
locals. The load consumes source-local availability; forward joins intersect
that availability and backedges require exact ownership state. `local.take`, a
load from an unavailable pointer local, call or return escape, construction, and
record embedding reject. The borrowed region remains available. At most 4,096
`DF` instructions and 256 explicit region/pointer/ABI relations are admitted in
one module.

After complete verification, the bounded scalar provider and native x86-64
lowerer execute `DF` by copying the private packed logical `{start, length}`
descriptor into pointer-owned storage. The operation forms no address, permits
no pointer escape, and authorizes no Foreign call. The browser, WebAssembly
host, and Windvale OS consumers retain their narrower declared boundaries.

## Candidate WVB 1.38 paired registered Foreign-call publication

WVB 1.38 inherits the WVB 1.37 System-profile metadata and instruction
vocabulary and adds one candidate instruction:

```text
E0 foreign.call u32 registered-binding identity,
                u32 pointer-record type index,
                u32 ABI-enum type index
```

`E0` is 13 bytes. It consumes an exact non-null
`Foreignˉpointer<u8, Abi>`, `u64` capacity, and `u64` expected generation in
that order and produces exact `i64`. The pointer immediate names a kind-`1`
record and the ABI immediate names its kind-`2` or kind-`7` enum argument.
Registered binding identity `1` denotes the exact
`windvale.paper.buffer_source.sysv_amd64_c_v1` contract: native symbol
`wv_paper_buffer_source_read_v1`, SysV AMD64 C v1, System profile,
`ffi.call`, unsafe, no-retain, no-unwind, those three by-value arguments, and
the exact `i64` result. Zero and every other identity are unregistered in this
candidate.

The writer selects minor 38 only when reachable WVIR operation `190` is
present, and every minor-38 module contains at least one `E0`. The authenticated
production coordinator admits at most 4,096 paired calls, invokes a private
emitter form, and rechecks the six retained authenticated snapshots plus WVFB
before and after emission. The emitter independently validates WVFB against the
source-symbol directory and every typed Foreign call. Neither WVFB nor WVB
proves that the coordinator performed those steps, so direct possession of
either value grants no capability or native-call authority.

The focused independent reader validates the exact header, seven sections,
bounded type/function/code geometry, identity `1`, pointer-record kind, and ABI
enum kind. It rejects an earlier minor, an unknown opcode, an unregistered
binding, invalid type indices, wrong type kinds, and swapped pointer/ABI
indices. This is source-publication evidence, not admission to execution. The
complete compiler-aligned verifier, scalar provider, native lowerer, launchers,
browser, WebAssembly, package, and OS consumers reject minor 38. They must add
their own containment, provider-binding, symbol-resolution, and ABI-invocation
rules before opening execution.

## Encoding

- All integers are little-endian.
- All text is strict UTF-8 without a byte-order mark.
- Lengths, counts, indices, and code offsets use unsigned 32-bit integers.
- A string is a `u32` byte length followed by that many UTF-8 bytes.
- Decoders use checked arithmetic and reject trailing or missing payload bytes.
- Module, data, function, and export names use the Seed source-identifier grammar. These UTF-8 metadata names are not native ABI symbols.
- Capability names use their separately specified qualified lowercase ASCII grammar.

## File header

```text
4 bytes  magic: 57 56 42 31 (ASCII WVB1)
u16      major version: 1
u16      minor version: 11 through 38
u32      section count: 7
```

The minor version identifies the admitted vocabulary. Canonical source
publication calculates the lowest required version from all function, local,
temporary, record-field, variant-payload, collection-element, and instruction
shapes. An otherwise ordinary module remains byte-identical WVB 1.11.

Every section has this envelope:

```text
u8       section kind
u8       flags, must be zero
u16      reserved, must be zero
u32      payload byte length
bytes    payload
```

The seven mandatory sections occur exactly once in this order:

1. Module
2. Capabilities
3. Data
4. Functions
5. Code
6. Exports
7. Types

## Module section

```text
u8       profile: 1 portable, 2 hosted, 3 system
string   module name
u8       metadata present: 0 or 1
if present:
  <metadata fields below>
metadata fields:
  u8       metadata encoding version: 1
  u8       authority: 1 library, 2 application, 3 service, 4 system
  u32      platform-scope count
  string[] strictly sorted platform scopes
  u32      required-capability count
  repeat:
    string capability identity
    u32    major version
  u32      optional-capability count
  repeat:
    string capability identity
    u32    major version
```

The presence byte is mandatory even when metadata is absent. Platform scopes are unique, strictly sorted lowercase ASCII identities with optional dot-separated segments. At least one scope is required when metadata is present. Required and optional entries are independently unique and sorted. An identity cannot be both required and optional. Seed currently admits catalog identities at major version 1. Required metadata identities exactly equal the executable Capabilities section; optional identities are admission and provider-selection metadata only. System authority and the retained system profile must agree.

## Capabilities section

```text
u32      capability count
repeat:
  string capability name
  u32    parameter count
  u8[]   parameter value types
  u8     return type
```

Entries are strictly sorted by ordinal capability name and cannot be duplicated. Portable modules require zero capabilities.

The canonical catalog contains the seven process, file-byte, console, and
diagnostic signatures defined by [Hosted-Resources.md](Hosted-Resources.md), plus
`filesystem.directory_read_v1(text, u32, u32) -> bytes` and
`storage.random_access_v1(u32, u64, u64, u32, bytes) -> bytes`, plus
`standard_output.write_v1(bytes) -> bytes`, plus
`model.catalog_v1(bytes) -> bytes` and `model.inference_v1(bytes) -> bytes`.
Their semantic
contracts are defined by [Read-Only-Directory-Capability.md](Read-Only-Directory-Capability.md)
and [Random-Access-Storage-Capability.md](Random-Access-Storage-Capability.md),
with byte output defined by
[Standard-Byte-Output-Capability.md](Standard-Byte-Output-Capability.md),
with the model seam defined by
[Windvale-Bound-Model-Provider.md](Windvale-Bound-Model-Provider.md).
Extending the recognized catalog does not change this encoding or the WVB version
when every signature uses existing value types.

## Data section

```text
u32      data count
repeat:
  string data name
  u8     data type: 3 text, 4 immutable i32 array, 5 immutable bytes
  if text:
    string UTF-8 value
  if i32 array:
    u32  element count
    i32[] elements
  if bytes:
    u32  byte count
    bytes value
```

Entries are strictly sorted by ordinal data name and cannot be duplicated.

## Functions section

```text
u32      function count
repeat:
  string function name
  u32    parameter count
  shape[] parameter types
  shape   return type
  u32    non-parameter local count
  shape[] local types
  u32    code offset within the Code section
  u32    code byte length
  u32    declared maximum operand-stack depth
```

Entries are strictly sorted by ordinal function name and cannot be duplicated. Function code ranges must be contiguous, ordered, non-overlapping, and cover the entire Code section.

## Exports section

```text
u32      export count
repeat:
  string export name
  u8     export kind: 1 function
  u32    function index
```

Exports are strictly sorted by ordinal name. An exported name must equal the referenced function's Seed name.

For WVB 1.11 through 1.20, the reference launcher selects exported
`Main() -> i32` as the executable source entry point. WVB 1.21 instead requires
exactly one exported `Main(Memoryˉbudget) -> i32`; the launcher transfers one
fresh root-budget token into that parameter before the first instruction. WVB
1.22 uses the ordinary zero-parameter entry unless shape `25` is present. When
it is present, the same exact one-parameter transfer rule applies.
WVB 1.23 through WVB 1.28 require the same exact one-parameter entry because
the currently admitted recoverable construction, append, and growth profile
begins with a launcher-owned root budget.
WVB 1.29 instead requires exactly one exported
`Main(Platformˉfile.Sourceˉfile) -> i32`; the launcher transfers one admitted
immutable source snapshot into that parameter before the first instruction.
WVB 1.30 and WVB 1.31 use the ordinary exported `Main() -> i32` entry unless
either also carries
the inherited shape-`34` source profile; when shape `34` is present, its entry
and ownership confinement remain exact.
WVB 1.32 structured-task applications instead export exact hosted
`Main(Memoryˉbudget, Operationˉcontext) -> i32`. The launcher transfers one
fresh root budget and one live immutable root operation context. Neither opaque
identity is ambient, source-constructible, or representation-observable.
The scalar runner selects execution-request major `6` for this exact entry.
Major `6` retains the bounded 16-byte request header used by the portable
entry, requires hosted profile `2`, zero declared capabilities, WVB 1.32, and
the exact two-parameter signature, and carries no source-file snapshot. Request
major `5` remains exclusively the source-file snapshot contract.
WVB 1.33 through WVB 1.37 System-profile execution instead exports exact
`Main(Memoryˉbudget) -> i32`; shape `36` and the WVB 1.35/1.36 scratch-
specific parameter uses of shape `28` remain confined to non-exported helpers
and compiler-generated view locals. Candidate WVB 1.37 additionally admits the
exact immutable write-region shape-`28` parameter directly targeted by `DF`;
bounded scalar and native x86-64 execution require the complete containment
rules above.
Future native object formats must define an ASCII-safe external symbol mapping
separately.

## Types section

```text
u32      type count
repeat:
  u8     kind: 1 record, 2 i32-backed enum, 3 variant,
               4 fixed array, 5 Vector, 6 Sequence,
               7 u8-backed enum (WVB 1.22 and later),
               8 callable descriptor (WVB 1.30 through WVB 1.37)
  if kind 1 through 7:
    string nominal type name
  if record:
    u32    field count
    repeat:
      string field name
      shape field type
  if i32-backed enum:
    u32    member count
    repeat:
      string member name
      i32  member value
  if u8-backed enum:
    u8     source backing identity: 6 (`u8`)
    u32    member count
    repeat:
      string member name
      u8     member value
  if variant:
    u32    case count
    repeat:
      string case name
      u8     field encoding:
             0 no fields
             1 one legacy field
             2 field list, WVB 1.16 and later
      if encoding 1:
        string field name
        shape  field type
      if encoding 2:
        u32    field count, 2 through 64
        repeat:
          string field name
          shape  field type
  if fixed array:
    shape  element type
    u32    element count, 0 through 4095
  if Vector or Sequence:
    shape  element type
  if callable descriptor:
    u8     module profile: 1 portable, 2 hosted, 3 system
    shape  result
    u32    parameter count, 0 through 64
    shape[] parameters in declaration order
    if WVB 1.32:
      u8   flags: bit 0 async, bit 1 unsafe, all other bits zero
      u8   result transfer mode: 0 value in WVB 1.32
      u32  exact finite language-effect mask
      u32  exact finite capability-effect bitmap
      u8[] parameter transfer modes in declaration order: 0 value,
           1 immutable borrow, 2 mutable borrow
```

Nominal types are grouped by semantic category, then strictly sorted by ordinal
name, and names are unique across all categories. Kinds `2` and `7` share the
one enum category, so differently backed enums are ordered together by name.
Nominal shapes may refer forward to a later Types entry. Dependency discovery
or generic materialization order is not a serialized ordering rule; writers
must remap every nominal reference to the canonical category/name order.
Record field order is declaration order and
therefore constructor order; field names are unique within the record. Seed
requires between 1 and 64 fields. Enums contain 1 through 256 uniquely named
members with unique values in their exact encoded backing. Kind `2` retains the
canonical signed `i32` member encoding of every earlier WVB. Kind `7` exists
in WVB 1.22 and later, carries exact source backing identity byte `6`, and encodes
each member in one byte without narrowing or sign reinterpretation. The backing
identity is an edition-1 compiler type identity, not WVB value-shape byte `4`.
Variants contain 1 through 256 unique ordered
cases and zero through 64 uniquely named fields per case. Encoding `0` is
canonical for no fields, encoding `1` is canonical for exactly one field and
preserves all earlier WVB bytes, and encoding `2` is canonical only for two
through 64 fields in WVB 1.16 and later. A fixed-array descriptor exists in
WVB 1.17 and later, contains one exact non-`never` value shape plus its length, and has no
field names or capacity. Its compiler-generated private name identifies the
concrete source `Array<T, N>` instance. Field and element shapes obey the
bounded, acyclic source restrictions. Vector and Sequence descriptors exist
in WVB 1.18 and later. Each contains one exact non-`never` element shape and a
compiler-generated private name identifying the concrete `Vector<T>` or
`Sequence<T>` instance. Neither descriptor contains a source maximum, backing
capacity, allocator, or authority.

Kind `8` exists only in WVB 1.30 through WVB 1.32. Callable descriptors are unnamed and occur
after all canonically ordered nominal entries, in the producer's deterministic
first-complete-signature order. There are at most 256. Their parameter and
result shapes are exact, their parameter modes are by value, and the result is
neither `void` nor `never` in this first executable profile. The descriptor
profile exactly equals the containing WVB module profile; a producer cannot
retain a narrower imported source profile in this flattened executable
contract. The descriptor contains no source function identity, serialized closure environment, host
address, calling-convention selection, or effect mask. WVB 1.30 admits only the
separately checked empty-effect, noncapturing callable subset. WVB 1.31 retains
the same descriptor and admits the separately verified plain-capture
environment subset; the environment itself is never serialized in Types.
WVB 1.32 appends the exact flag, result-mode, language-effect,
capability-effect, and per-parameter-mode trailer above. The first task-spawn
profile requires an async, safe, zero-parameter callable whose result is exact
`Result<T, E>` and whose declared finite effects cover the scope operation.

## Value types

```text
0 void
1 i32
2 bool
3 text
4 u8
5 u32
6 bytes
7 record
8 enum
9 i64
10 u64
11 variant followed by u32 nominal-type index
12 sequence followed by element shape and u32 maximum
13 builder followed by element shape and u32 maximum
14 i8 (WVB 1.12 and later)
15 i16 (WVB 1.12 and later)
16 u16 (WVB 1.12 and later)
17 rune (WVB 1.13 and later)
18 f32 (WVB 1.14 and later)
19 f64 (WVB 1.14 and later)
20 unit (WVB 1.15 and later)
21 never (WVB 1.15 and later)
22 fixed array followed by u32 nominal-type index (WVB 1.17 and later)
23 Vector followed by u32 nominal-type index (WVB 1.18 and later)
24 Sequence followed by u32 nominal-type index (WVB 1.18 and later)
25 Memoryˉbudget opaque owner (WVB 1.21 through 1.28 and WVB 1.32 exact entries; WVB 1.23 through 1.28 and WVB 1.32 Main locals; WVB 1.33 through WVB 1.38 by-value parameters and affine locals)
26 immutable-borrowed Vector parameter followed by u32 nominal-type index (WVB 1.26 and later)
27 mutable-borrowed Vector parameter followed by u32 nominal-type index (WVB 1.26 and later)
28 borrowed record view followed by u32 nominal-type index (WVB 1.28 through 1.38 local only, except the exact scratch helper parameters in WVB 1.35/1.36 and exact immutable write-region parameter in WVB 1.37/1.38)
29 borrowed variant view followed by u32 nominal-type index (WVB 1.28 through 1.38 local only)
30 borrowed fixed-array view followed by u32 nominal-type index (WVB 1.28 through 1.38 local only)
34 Platformˉfile.Sourceˉfile opaque owner (WVB 1.29 through 1.38 under the exact entry rules)
35 callable value followed by u32 kind-8 callable-type index (WVB 1.30 through WVB 1.38)
36 immutable-borrowed Memoryˉbudget view (WVB 1.34 through WVB 1.38 parameter or compiler-generated local only)
```

`void` and `never` are valid only as return types. `unit` is an ordinary value
shape in parameters, results, locals, fields, payloads, and operand-stack
values. It has exactly one logical value; a runtime may erase its native storage,
but the bytecode stack represents it as a canonical zero scalar cell. `never`
has no values and therefore cannot appear in a parameter, local, field, payload,
collection element, or operand-stack value. Immutable integer arrays are module
data and are not operand-stack values. A `bytes` value is an immutable sequence
or slice view and can be stored in locals, passed to functions, and returned.

Function parameter, result, local, record-field, variant-payload, and array
element types use a value shape. A primitive shape is its one-byte value type. A
nominal shape is byte `7`, `8`, `11`, `22`, `23`, or `24` followed by a `u32` Types-section
index of the matching kind. A collection shape is byte `12` or `13`, its
recursively encoded non-collection element shape, then its `u32` maximum.
Nominal identity and collection kind/element/maximum are exact. Enum shape byte
`8` accepts either kind `2` or kind `7`; the Types index retains the backing
distinction without introducing a second runtime enum value shape.

Shape `35` is followed by one exact kind-`8` Types index. It is an ordinary
copyable typed cell for a function reference or admitted plain-capture closure
and may occur wherever
an ordinary value shape is admitted. Equality, ordering, arithmetic,
reinterpretation, construction from an integer, and representation inspection
are not defined. A call consumes the exact shape identity named by its
instruction; no compatible-signature inference or variance is performed.

WVB 1.26 and later additionally permit shape `26` or `27` in a function parameter list
only. Shape `26` is one immutable borrow and shape `27` is one exclusive mutable
borrow; each is followed by a kind-5 Vector Types index exactly like shape
`23`. Shape `23` in a parameter list remains a by-value transfer. Shapes `26`
and `27` are invalid as results, non-parameter locals, fields, payloads,
collection elements, or Types entries. No parameter-mode trailer follows the
function directory.

WVB 1.28 additionally permits shapes `28`, `29`, and `30` as non-parameter
temporary locals. They identify, respectively, a borrowed view of an exact
kind-1 record, kind-3 variant, or kind-4 fixed array that recursively contains a
kind-5 Vector. They are invalid as ordinary parameters, results, declared source
locals, fields, payloads, collection elements, or Types entries. WVB 1.35 and
1.36 admit only their exact borrowed scratch helper parameter, and candidate WVB
1.37/1.38 admits only the exact immutable write-region parameter for opcode `DF`.
These shapes do not encode a pointer or an independently storable value. The
verifier confines every admitted view to its exact generated sequence, including
the WVB 1.37 region parameter and affine pointer move.

Shape `25` is not a general value shape. In WVB 1.21 or 1.22 it occurs at most
once: as parameter zero of the one-parameter function named `Main`. WVB 1.21
requires that occurrence; WVB 1.22 permits it only when the module also needs
the kind-7 enum extension. That
function returns `i32` and is exported under the same name. Shape `25` is
invalid as any other parameter, result, non-parameter local, record field,
variant payload, collection element, or Types entry. No instruction constructs
it, and `local.load` and `local.store` reject its parameter slot. This first
encoding makes launcher transfer and deterministic top-level release
executable without exposing representation or claiming allocation leases or
collection allocation.

WVB 1.23 through WVB 1.28 retain that exact entry parameter and additionally permit shape
`25` in non-parameter locals of `Main` only. These locals are affine: they are
initialized only by `local.take` from another available budget local or by the
Valid payload of the exact Split result, cannot be read with `local.load`, and
cannot overwrite an available owner. Shape `25` remains invalid in every other
function, return, nominal payload, collection element, or Types entry.

WVB 1.32 re-admits the same opaque shape `25` only as parameter zero of the
exact structured-task `Main`, in its affine non-parameter locals, and through
the inherited memory-budget operations. Parameter one is the exact canonical
`Foundationˉoperation.Operationˉcontext` record identity. Task scope, task
handle, construction Result, spawn Result, and outcome values remain exact
nominal types distinguished by their canonical layouts and verifier-owned
affine state; source cannot forge their identity fields.

Shape `34` is not a general value shape. WVB 1.29 requires it exactly once as
parameter zero of the one-parameter function named `Main`; that function
returns `i32` and is exported under the same name. Shape `34` may additionally
occur in non-parameter locals of that `Main` only. It is invalid in every other
function, result, nominal payload, collection element, or Types entry. It is
move-only: `local.take` transfers its available ownership evidence, ordinary
loads never copy it, and a store cannot overwrite an available owner. Source
cannot construct it or expose its representation.
WVB 1.30 through WVB 1.38 retain those confinement rules whenever shape `34`
is present.

`i64`, `u64`, `i8`, `i16`, `u16`, `rune`, `f32`, and `f64` are ordinary scalar
shapes. They do not
widen counts, indices, lengths, code offsets, enum backing values, or existing
binary Foundation operations, which remain explicitly `u32` or `i32`. Shape
bytes 14 through 16 and opcode `C0` are invalid in WVB 1.11. Shape byte 17 and
opcode `C1` are invalid before WVB 1.13. Shape bytes 18 and 19 and opcode `C2`
are invalid before WVB 1.14. Shape bytes 20 and 21 and opcode `C3` are invalid
before WVB 1.15. Variant field-list marker `2` and opcode `C4` are invalid
before WVB 1.16. Shape byte 22, type kind 4, and opcodes `C5` and `C6` are
invalid before WVB 1.17. Shape bytes 23 and 24 and type kinds 5 and 6 are
invalid before WVB 1.18. Opcodes `C7` through `CC` are invalid before WVB
1.19, opcode `CD` is invalid before WVB 1.20, opcode `CE` is invalid before
WVB 1.23, opcode `CF` is recognized in WVB 1.24 through 1.38, opcode `D0` is
recognized in WVB 1.25 through 1.38, and opcode `D1` is recognized in WVB 1.27
through 1.38. Their budget-entry ownership preconditions remain exact, so the
WVB 1.29 source-file entry cannot execute them. Opcode `D2` and shape byte `34`
are valid in WVB 1.29 through 1.38. Type kind `8`, shape byte `35`, and opcodes
`D3` and `D4` are valid in WVB 1.30 through WVB 1.38. Opcode `D5` is valid in
WVB 1.31 through WVB 1.38. Opcodes `D6` through `DB` and the extended kind-`8`
descriptor trailer are valid in WVB 1.32 through WVB 1.38. Opcode `DC` and
shape byte `25` under the System-profile rules are valid in WVB 1.33 through
WVB 1.38. Shape byte `36` is valid in WVB 1.34 through WVB 1.38. Opcode `DD`
is valid in WVB 1.35 through WVB 1.38, opcode `DE` in WVB 1.36 through
WVB 1.38, opcode `DF` in WVB 1.37 and 1.38, and opcode `E0` only in WVB 1.38.
Type kind `7` is valid in
WVB 1.22 and later, and every WVB 1.22 module
contains at least one kind-7 descriptor so an earlier vocabulary is never
published under an unnecessarily high version. Every WVB 1.23 module contains
at least one opcode `CE`, every WVB 1.24 module contains at least one opcode
`CF`, and every WVB 1.25 module contains at least one opcode `D0`.
Every WVB 1.26 module contains at least one exact Vector parameter using shape
`23`, `26`, or `27`.
Every WVB 1.27 module contains at least one opcode `D1`. Every WVB 1.28 module
contains at least one record, variant, or fixed array that recursively contains
an owned Vector. Every WVB 1.29 module contains the exact source-file entry and
at least one opcode `D2`. Every WVB 1.30 module contains at least one opcode
`D3` or `D4`. Every WVB 1.31 module contains at least one opcode `D5`.
Every WVB 1.32 module contains at least one opcode `D6` through `DB` and at
least one exact extended callable descriptor. Every WVB 1.33 module contains at
least one `DC`, every WVB 1.34 module contains at least one shape-`36`
parameter, every WVB 1.35 module contains at least one `DD`, every WVB 1.36
module contains at least one `DE`, and every candidate WVB 1.37 module contains
at least one `DF`. Every candidate WVB 1.38 module contains at least one `E0`.
Each later version admits
the complete instruction and type vocabulary of every earlier version, subject
to that version's ownership rules.

## Instruction encoding

```text
01 i32.const       i32 value
02 bool.const      u8 value (0 or 1)
03 text.const      u32 text-data index
04 local.load      u32 local index
05 local.store     u32 local index
06 data.length     u32 i32-array data index
07 data.load.i32   u32 i32-array data index; consumes i32 index
08 u8.const        u8 value
09 u32.const       u32 value
0A bytes.const     u32 byte-data index
0B bytes.length
0C bytes.slice     consumes bytes, u32 offset, u32 length
0D bytes.read_u8   consumes bytes, u32 offset
0E bytes.read_u16_little consumes bytes, u32 offset
0F bytes.read_u32_little consumes bytes, u32 offset

10 i32.add
11 i32.subtract
12 i32.multiply
13 i32.negate
14 u32.add
15 u32.subtract
16 u32.multiply

20 i32.equal
21 i32.not_equal
22 i32.less
23 i32.less_equal
24 i32.greater
25 i32.greater_equal
26 bool.equal
27 bool.not_equal
28 bool.not

60 u32.equal
61 u32.not_equal
62 u32.less
63 u32.less_equal
64 u32.greater
65 u32.greater_equal
66 u8.equal
67 u8.not_equal
68 record.create     u32 record-type index; consumes fields in declaration order
69 record.field      u32 field index; consumes one nominal record value
6A enum.const        u32 enum-type index, u32 member index
6B enum.equal        consumes two values of the same nominal enum
6C enum.not_equal    consumes two values of the same nominal enum
6D enum.name         consumes enum, produces its declared member name as text
6E i32.format        consumes i32, produces invariant decimal text
6F u8.format         consumes u8, produces invariant decimal text
70 u32.format        consumes u32, produces invariant decimal text
71 text.concat       consumes two text values, produces bounded concatenation
72 bytes.read_i32_little consumes bytes and u32 offset, produces signed i32
73 text.utf8_is_valid consumes bytes, produces bool without trapping on invalid UTF-8
74 text.from_utf8     consumes bytes, produces text or traps on invalid UTF-8
75 text.quote         consumes text, produces bounded ASCII JSON-style quoted text
76 u32.from_u8        consumes u8, produces the same value as u32
77 bytes.concat       consumes two bytes values, produces bounded immutable concatenation
78 bytes.from_u8      consumes u8, produces one byte
79 bytes.from_u16_little consumes u32 in the range 0..65535, produces two bytes
7A bytes.from_u32_little consumes u32, produces four bytes
7B bytes.from_i32_little consumes i32, produces four two's-complement bytes
7C text.to_utf8       consumes text, produces its strict UTF-8 bytes
7D bytes.sha256_hex   consumes bytes, produces 64 lowercase ASCII hex characters

80 i64.const          i64 little-endian value
81 u64.const          u64 little-endian value
82 i64.add
83 i64.subtract
84 i64.multiply
85 i64.negate
86 u64.add
87 u64.subtract
88 u64.multiply
89 i64.equal
8A i64.not_equal
8B i64.less
8C i64.less_equal
8D i64.greater
8E i64.greater_equal
8F u64.equal
90 u64.not_equal
91 u64.less
92 u64.less_equal
93 u64.greater
94 u64.greater_equal
95 i64.format          consumes i64, produces invariant decimal text
96 u64.format          consumes u64, produces invariant decimal text

97 variant.create      u32 variant-type index, u32 case index; consumes every case field in declaration order
98 variant.is_case     u32 variant-type index, u32 case index; consumes variant, produces bool
99 variant.payload     u32 variant-type index, u32 case index; consumes variant, produces payload
9A builder.create      u32 element-shape descriptor, u32 maximum
9B builder.push        consumes builder and exact element, produces replacement builder
9C builder.freeze      consumes builder, produces immutable sequence
9D sequence.length     consumes sequence, produces u32
9E sequence.element    consumes sequence and u32 index, produces exact element
9F i32.divide
A0 i32.remainder
A1 u32.divide
A2 u32.remainder
A3 i64.divide
A4 i64.remainder
A5 u64.divide
A6 u64.remainder
A7 u8.bitwise_and
A8 u8.bitwise_or
A9 u8.bitwise_xor
AA u8.bitwise_not
AB u8.shift_left       consumes u8 and u32
AC u8.shift_right      consumes u8 and u32
AD u32.bitwise_and
AE u32.bitwise_or
AF u32.bitwise_xor
B0 u32.bitwise_not
B1 u32.shift_left      consumes u32 value and u32 count
B2 u32.shift_right     consumes u32 value and u32 count
B3 u64.bitwise_and
B4 u64.bitwise_or
B5 u64.bitwise_xor
B6 u64.bitwise_not
B7 u64.shift_left      consumes u64 and u32
B8 u64.shift_right     consumes u64 and u32
B9 text.equal
BA text.not_equal
BB bytes.equal
BC bytes.not_equal
BD bytes.read_u64_little consumes bytes and u32 offset, produces u64
BE bytes.from_u64_little consumes u64, produces eight bytes
BF u64.from_u32    consumes u32, produces the same numeric value as u64
C0 fixed.integer   u8 type tag, u8 operation, then operation-specific payload
C1 rune            u8 operation, then operation-specific payload
C2 floating        u8 type tag, u8 operation, then operation-specific payload
C3 unit.const      no immediate; produces the sole `unit` value
C4 variant.field   u32 variant-type index, u32 packed case/field; consumes variant, produces exact field
C5 array.create    u32 array-type index; consumes exactly N elements in index order, produces exact array
C6 array.element   no immediate; consumes exact array and u64 index, produces exact element
C7 vector.create_reserved u32 Vector-type index; consumes u64 maximum, produces unique exact Vector
C8 vector.append_unchecked u32 Vector-type index; consumes unique Vector and exact element, produces unique replacement Vector
C9 vector.freeze   u32 Vector-type index, u32 Sequence-type index; consumes unique Vector, produces exact Sequence
CA vector.length   u32 Vector-type index; preserves unique Vector and pushes u64 current length
CB sequence.length u32 Sequence-type index; preserves Sequence and pushes u64 current length
CC sequence.element u32 Sequence-type index; preserves Sequence, consumes u64 index, and pushes exact element
CD local.take      u32 non-parameter local index; empties one available exact Vector local and produces its unique Vector
CE memory_budget.split u32 parent-local index, u32 Result-type index; consumes u64 maximum bytes then u32 maximum children, preserves the parent owner, produces exact affine Result
CF vector.construct_reserved_fallible u32 budget-local index, u32 Result-type index; consumes u64 maximum items and the budget owner, produces exact affine Result<Vector<T>, Allocation_failure>
D0 vector.append_fallible u32 Vector-local index, u32 Result-type index; consumes exact T, mutates the named Vector only on success, produces exact Result<unit, Vector_append_failure<T>>
D1 vector.grow_reserved_fallible u32 Vector-local index, u32 budget-local index, u32 Result-type index; consumes u64 new maximum, atomically replaces the named Vector backing only on success, produces exact Result<unit, Allocation_failure>
D2 source.length    u32 Sourceˉfile local index; observes one live immutable source snapshot and produces u64 byte length
D3 function.reference u32 function index, u32 callable-type index; produces one exact callable value
D4 call.indirect    u32 callable-type index; consumes one exact callable followed by its exact arguments and produces its result
D5 closure.create  u32 function index, u32 callable-type index, u32 capture count; consumes captures in declaration order and produces one exact callable value
D6 task.scope.construct u32 budget-local index, u32 construction-Result type; consumes Taskˉlimits and Operationˉcontext, consumes the budget local, produces the exact affine Result
D7 task.operation_context u32 scope-local index, u32 context type; observes one live scope and produces its lifetime-bound context
D8 task.spawn       u32 scope-local index, u32 spawn-Result type, u32 Task type; consumes one exact async callable and produces accepted handle or rejection carrying the original work
D9 task.await       u32 handle-local index, u32 origin-scope-local index, u32 outcome type; consumes the handle and produces its exact Taskˉoutcome
DA task.request_cancel u32 scope-local index, u32 cancel-outcome type; closes the scope idempotently and produces its exact live-child observation
DB task.scope.exit  u32 scope-local index, u8 policy: 0 join, 1 cancel_join, 2 fail_join; joins all children and consumes the scope
DC unsafe.scratch.construct u32 budget-local index, u32 construction-Result type index, u32 ABI-enum type index; consumes u64 length and alignment plus the budget owner and produces the exact affine Result
DD unsafe.scratch.length u32 scratch-local index, u32 ABI-enum type index; observes an exact owned or immutable borrowed scratch and produces u64 without consuming it
DE unsafe.write-region.borrow u32 scratch-local index, u32 region-Result type index, u32 ABI-enum type index; consumes u64 start, length, and required alignment, consumes the exact scratch owner or mutable view, and produces the canonical affine Result
DF unsafe.write-pointer.borrow u32 region-local index, u32 pointer-record type index, u32 ABI-enum type index; WVB 1.37 and 1.38, consumes no stack value and produces the exact opaque pointer
E0 foreign.call     u32 registered-binding identity, u32 pointer-record type index, u32 ABI-enum type index; WVB 1.38 only, consumes pointer, u64 capacity, and u64 expected generation and produces i64

30 jump            u32 absolute byte offset in the function
31 branch.false    u32 absolute byte offset; consumes bool

40 call            u32 function index
41 call.capability u32 capability index

50 pop
51 return
```

Opcodes `C7` through `CC` exist in WVB 1.19 and later and require matching kind-5
or kind-6 Types entries. In this first runtime checkpoint, their element shape
must be a resource-free scalar: `i32`, `bool`, `u8`, `u32`, an exact nominal
enum, `i64`, `u64`, `i8`, `i16`, `u16`, `rune`, `f32`, or `f64`. Records,
variants, fixed arrays, text, bytes, nested collections, and other values that
need element-owned destruction or tracing remain invalid for these opcodes.

`vector.create_reserved` is a low-level checked backend primitive. Its current
portable scalar profile admits a positive maximum through 2,047 cells and may
report bounded allocation failure before publishing a Vector. The public
Language 1.0 Foundation constructor remains a fallible `Memoryˉbudget` API;
the opcode does not synthesize that source-level `Result` or grant allocation
authority. `vector.append_unchecked` is emitted only after a successful
capacity check; a violated precondition reports `WVR3008` without publishing a
replacement. Freeze consumes the mutable owner and may share its backing with
the immutable result. Both length operations return the current count, not the
retained maximum. `sequence.element` checks the full `u64` index and reports
`WVR3008` when it is not below the current length.

The WVB 1.19 verifier attaches non-serialized linear evidence to the Vector
produced by `vector.create_reserved`. Append preserves that evidence, Vector
length observes without consuming it, and freeze consumes it. In WVB 1.19 an
ordinary `local.load`, field extraction, array extraction, or call result does
not create the evidence, so it cannot be used by these Vector operations. This
deliberately keeps the first executable subset alias-free. Sequence is shared
immutable and therefore retains ordinary copyable local behavior.

WVB 1.20 `local.take` transfers the unique evidence from an exact kind-23
non-parameter local to the operand stack. It does not retain or release the
backing. The source local becomes unavailable and its runtime cell becomes
zero; a later unique `local.store` may initialize it again. A WVB 1.20 function
declared to return exact Vector must return unique evidence, and a successful
`call` to that declaration produces unique Vector evidence. Parameter slots
cannot be taken in this checkpoint because calls do not yet transfer unique
Vector arguments into parameters. `local.load` remains a retaining shared read
and never creates unique evidence. Consequently, every Vector `local.store` in
a WVB 1.20 module consumes a unique Vector, while WVB 1.19 retains its earlier
non-unique local-store behavior for compatibility.

The WVB 1.20 verifier proves definite Vector-local availability before typed
execution. Vector parameters begin initialized and available for shared loads
but cannot be taken; Vector locals begin unavailable. A unique store makes the
target available; a load requires it to be available without consuming it; a
take requires it and then makes it unavailable. Forward control-flow joins
intersect availability so a local is usable only when every incoming path owns
it. A backward branch is accepted only when the complete owned-slot state at the
edge exactly equals the previously established target-header state. The bounded
profile admits at most 64 Vector local slots and 4,096 instructions in a
function that uses `local.take`. These are verifier limits, not portable source
collection limits.

In WVB 1.23 through WVB 1.28, `local.take` also transfers an available shape-25
budget local or exact affine Result variant. A budget take clears the source
cell and pushes the same opaque owner. A Split-result take may transfer the
Valid child budget exactly once. A WVB 1.24 constructor-result take may transfer
the Valid Vector exactly once; WVB 1.25 through 1.31 retain that rule. WVB 1.29
through WVB 1.31 also permit `local.take` to transfer the exact source-file
parameter or local.
Ordinary loads never copy any affine value.

WVB 1.21, WVB 1.22 with shape `25`, or WVB 1.23 through WVB 1.28 transfers one opaque
launcher-owned root-budget token into `Main`'s parameter-zero cell. The current scalar profile
represents the token as an
identity plus provider generation and keeps its byte maximum and accounting
state outside bytecode. A completed top-level return
validates the token, clears its cell, and releases the root exactly once before
publishing the `i32` result. Rejection, trap, or provider teardown invalidates
the invocation domain without making the token available to bytecode.

WVB 1.29 transfers one rights-limited source-file owner into `Main`'s
parameter-zero cell. The public `wvrun --source-file` mode reads the named host
file once before guest execution, rejects a snapshot larger than 1 MiB, and
passes only immutable bytes, one read right, and matching nonzero provider and
resource generations. The runtime cell retains only the bounded length and
generation. Return, rejection, trap, and teardown invalidate and release the
owner exactly once; no host path or handle enters bytecode.

`source.length` is a five-byte WVB 1.29 instruction. Its immediate must identify
one available non-parameter shape-34 local in the exact source-file `Main`.
Execution checks the live resource generation and 1 MiB snapshot bound, then
produces the exact `u64` byte length without consuming or mutating the owner.
At most 64 `source.length` instructions are admitted in one module.

`function.reference` is a nine-byte WVB 1.30-or-1.31 instruction. Its first
immediate identifies a named function whose ordered by-value parameter shapes
and result shape exactly equal the kind-`8` descriptor named by its second
immediate. Before flattening, the source producer separately proves that the
target's owning source-module profile equals the descriptor profile. It
produces shape `35` with that descriptor identity. The function reference is
representation-hidden and noncapturing; it grants no capability and carries no
host pointer.

`call.indirect` is a five-byte WVB 1.30-or-1.31 instruction. Its immediate identifies
one kind-`8` descriptor. Before the call, the operand stack contains the exact
shape-`35` callable followed by the declared arguments in source order. The
call consumes arguments in reverse order, then the callable, creates one
ordinary bounded frame for the carried function, and produces the descriptor's
exact result. The runtime checks that the value's function identity and type
identity remain valid even after bytecode verification. At most 65,536
`function.reference` and 65,536 `call.indirect` instructions are admitted per
module.

`closure.create` is a thirteen-byte WVB 1.31 instruction. Its first immediate
identifies the physical target function, its second identifies the public
kind-`8` callable descriptor, and its third is a capture count from 1 through
64. The stack contains those captures in declaration order; creation consumes
them and produces the same exact shape-`35` identity used by
`call.indirect`. The target's physical parameters are exactly the captured
prefix followed by the descriptor's public parameters, and its result exactly
matches the descriptor. Every physical parameter is by value. The source
compiler additionally proves that the descriptor profile matches the target's
owning source-module profile and publishes only synchronous, safe, nongeneric,
effect-free targets. Those source-only properties are not reconstructed from a
flattened WVB function record.

This first environment profile admits only inline scalar cells and enum values:
`i32`, `bool`, `u8`, `u32`, `i64`, `u64`, `i8`, `i16`, `u16`, `rune`, `f32`,
`f64`, and exact nominal enums. Text, bytes, callable values, records, variants,
fixed arrays, collections, budgets, source files, owned values, and borrowed
values are rejected. The runner stores immutable snapshots in one
representation-private per-execution arena bounded to 536,576 bytes (524 KiB)
and 1,024 created records; it is discarded as a unit when execution ends. This
deliberately avoids hidden
retain/release, borrow, escape, or tracing behavior. At most 65,536
`closure.create` instructions are admitted per module.

WVB 1.32 task instructions operate only on exact canonical Foundation layouts
and verifier-tracked affine locals. `task.scope.construct` consumes the named
budget local on every ordinary result path; a rejected construction releases
that budget before publishing the exact failure. `task.spawn` transfers its
callable and captures only when accepted and otherwise returns the identical
work value. `task.await` consumes exactly one handle whose encoded origin is the
still-live scope local. `task.scope.exit` is the only ordinary scope teardown
and consumes the scope after joining all children according to its immutable
policy. Forward joins require identical owned-task state and backedges require
exact state equality; no instruction can forge, duplicate, serialize, or move a
handle outside its origin scope.

The portable sequential profile counts one work unit for each verified WVB
instruction dispatched for a spawned child. The spawn instruction establishes
the baseline; synthetic trap unwind and outcome construction do not consume an
additional child work unit. Exhaustion before the next child instruction
produces `Taskˉoutcome.Trapped(3011)`. Child-relative call depth counts the child
root frame as one; a call that would exceed the accepted scope limit produces
`Taskˉoutcome.Trapped(3004)`. Ordinary arithmetic traps retain their stable
runtime identity, including signed division by zero as `3007`.

Completed aggregate results remain roots of the interpreter's bounded aggregate
arena until their handle is awaited or the origin scope tears down. The first
opcode family creates no task-owned timer or diagnostic record, so validated
`Maximumˉtimers` and `Maximumˉdiagnostics` currently remain at zero use; any
future creation instruction must charge its limit before publishing state.

`memory_budget.split` is a nine-byte WVB 1.23 instruction. The immediate parent
index must identify one available shape-25 `Main` local, and the immediate type
index must identify the exact materialized
`Result<Memoryˉbudget, Allocationˉfailure>` layout. The top stack operand is
`u32` maximum children and the next is `u64` maximum bytes. Success atomically
reserves both limits, preserves the parent owner, and returns Valid with one new
child owner. Refusal preserves the parent accounting state and returns Failure
with exact reason, requested bytes, and available bytes. The reference runner
supplies a 98,304-byte, 64-child root for this executable profile. That number
is a runner profile bound, not a portable language constant.

`vector.construct_reserved_fallible` is a nine-byte WVB 1.24 instruction. The
first immediate identifies one available shape-25 `Main` local. The second
identifies exact `Result<Vector<T>, Allocationˉfailure>`, whose Valid payload
names one kind-5 Vector with a resource-free scalar element. The sole stack
operand is `u64 Maximumˉitems`. Zero traps with `WVR3008`. A positive supported
maximum consumes the budget into one private allocation lease and returns Valid
with an empty reserved Vector. A positive maximum that the target cannot
represent returns exact Failure with `Targetˉunaddressable`; requested-byte
evidence saturates at `u64` instead of wrapping. Either ordinary Result path
consumes the budget owner. Provider refusal releases any still-local lease or
budget before publishing Failure, and final Vector descriptor release credits
the lease exactly once. The reference scalar profile admits at most 2,047 cells
per backing; that is not a portable source limit.

`vector.append_fallible` is a nine-byte WVB 1.25 instruction. Its first
immediate identifies one non-parameter local containing the exact unique
kind-5 Vector under exclusive mutable access. Its second identifies the exact
`Result<unit, Vectorˉappendˉfailure<T>>`; the failure record has declaration-
order fields `Error: Collectionˉfailure` and `Value: T`, and `T` exactly equals
the Vector element shape. The sole stack operand is one exact `T`. When length
is below the reserved maximum, the instruction appends the item atomically and
returns Valid with canonical unit. At the maximum it leaves length, contents,
capacity, iteration, and the Vector owner unchanged and returns Failure with
`Collectionˉfailure.Capacityˉexhausted(Maximumˉitems)` plus the original item.
No prefix success, hidden reallocation, budget acquisition, implicit element
copy, or trap represents ordinary capacity refusal. The first executable
profile admits only resource-free scalar `T` and at most 64 append instructions
per function; broader element destruction remains a later profile rather than
different source semantics.

`vector.grow_reserved_fallible` is a thirteen-byte WVB 1.27 instruction. Its
first immediate identifies one available direct non-parameter unique kind-5
Vector local; its second identifies a distinct available shape-25 budget slot
in `Main`; and its third identifies exact
`Result<unit, Allocationˉfailure>`. The sole stack operand is exact `u64`
`Newˉmaximumˉitems`. The new maximum must be greater than the current
maximum or execution traps with `WVR3008` before allocation. The first scalar
profile supports at most 2,047 cells and at most 64 growth instructions per
module; a larger positive maximum returns `Targetˉunaddressable`.

Growth has a strong transaction boundary. The runtime reserves and allocates
the full replacement while the old backing and lease remain live, copies only
the initialized prefix, then attaches the new lease, releases the old
descriptor, and swaps the local. Success preserves item order and length while
changing the admitted maximum. Any refusal before the swap returns exact
`Allocationˉfailure` and preserves the Vector owner, length, contents,
capacity, and the supplied budget's committed/reserved counts and generation.
Because both allocations coexist temporarily, requested-byte evidence and
budget admission use the complete replacement size, not merely the final-size
delta. No partial copy, hidden in-place mutation, ambient provider acquisition,
or retry is observable.

WVB 1.26 ordinary calls interpret an exact Vector parameter shape as its
transfer contract. Shape `23` consumes unique evidence produced by
`local.take`; shape `26` or `27` consumes one retaining shared load and
preserves the caller's originating owner. The callee owns a shape-23 parameter
and releases it if it survives to return. A borrowed parameter owns only the
temporary retain introduced for the call; reverse-order callee cleanup releases
that retain without consuming the caller's owner. A value/borrow mismatch is a
typed-verification failure. Mutable borrow remains exclusive in the validated
source/WVIR ownership proof; bytecode does not expose a pointer, source slot, or
borrow handle. Every ordinary return releases surviving descriptor-owning
parameters and locals in reverse slot order before restoring the caller frame.

Functions using budget/result affine or owned-call operations admit at most 64
owned slots and 4,096 instructions. Their control proof intersects availability
at forward joins and requires exact equality with the saved target state at
every backedge. This is the bounded loop ownership fixed point: ownership may
flow through a loop but cannot appear, disappear, or change class per
iteration.

WVB 1.28 and WVB 1.29 extend the same availability proof to records, variants, and fixed
arrays that recursively contain a Vector. Construction consumes every owned
field or element. A whole-value `local.store`, by-value call, or return consumes
the aggregate, and `local.take` is the only instruction that removes an
available aggregate from a local. Aggregate parameters and results use their
ordinary shapes: the verifier derives ownership recursively from Types and uses
non-serialized transfer tags internally. Borrowed aggregate parameters and
borrowed aggregate results are not admitted in this profile.

Field or element observation does not move the parent. The canonical emitter
materializes one local-only shape-`28`, `29`, or `30` view and emits exactly
`local.load owner; local.store view; local.load view; observer`. The verifier
requires matching nominal identity, prohibits `local.take` of the view, and
rejects any other producer, consumer, store, call, return, or escape. Mutable
field borrowing additionally requires a mutable owner binding in source.
Partial moves and record updates that would leave hidden ownership behind are
rejected; the entire aggregate must move as one value.

The runtime normalizes a borrowed-view cell to the parent's ordinary aggregate
representation. Aggregate construction retains descriptor-bearing fields, and
function/top-level teardown performs bounded deterministic reachability and
recursive release so every nested Vector descriptor and allocation lease is
released exactly once. This profile does not add user-visible destructors,
pointer values, ambient allocation, or an unbounded object graph.

Typed WVIR independently validates exact Vector transfer through ordinary calls
and returns before WVB publication. WVB 1.26 serializes those validated
parameter modes; it does not weaken the earlier source-slot provenance,
forward-join, or exact-backedge proof.

The `C0` type tag is exactly `14` (`i8`), `15` (`i16`), or `16` (`u16`). Its
operation byte is:

| Value | Operation | Operand/result rule |
| ---: | --- | --- |
| 0 | constant | followed by one raw little-endian `u16`; `i8` requires the high byte to be zero |
| 1 | add | two same-type values to one same-type value |
| 2 | subtract | two same-type values to one same-type value |
| 3 | multiply | two same-type values to one same-type value |
| 4 | divide | two same-type values to one same-type value |
| 5 | remainder | two same-type values to one same-type value |
| 6 | negate | one signed value to one same-type value |
| 7 | equal | two same-type values to `bool` |
| 8 | not equal | two same-type values to `bool` |
| 9 | less | two same-type values to `bool` |
| 10 | less equal | two same-type values to `bool` |
| 11 | greater | two same-type values to `bool` |
| 12 | greater equal | two same-type values to `bool` |
| 13 | bitwise and | two `u16` values to `u16` |
| 14 | bitwise or | two `u16` values to `u16` |
| 15 | bitwise xor | two `u16` values to `u16` |
| 16 | bitwise not | one `u16` value to `u16` |
| 17 | shift left | `u16` value and `u32` count to `u16` |
| 18 | shift right | `u16` value and `u32` count to `u16` |

The raw constant is the exact named-width two's-complement or unsigned bit
pattern. Arithmetic is checked. Overflow traps with `WVR3007`, division by zero
with `WVR3032`, and a shift count outside 0 through 15 with `WVR3033`. Signed
minimum divided or remaindered by minus one is overflow. Signed division
truncates toward zero and remainder has the dividend's sign. `u16` left shift
discards bits above bit 15; right shift fills with zero. Signed bitwise and shift
forms and unsigned negation are invalid bytecode.

The `C1` operation byte is:

| Value | Operation | Operand/result rule |
| ---: | --- | --- |
| 0 | constant | followed by one little-endian `u32` Unicode scalar; produces `rune` |
| 1 | equal | two `rune` values to `bool` |
| 2 | not equal | two `rune` values to `bool` |

A rune constant is an exact Unicode scalar from `0000` through `10FFFF`,
excluding the surrogate range `D800` through `DFFF`. There is no normalization,
locale mapping, numeric conversion, pointer interpretation, or text allocation.
Selectors above 2, missing immediate bytes, non-scalars, or another operand shape
are malformed bytecode.

The `C2` type tag is exactly `18` (`f32`) or `19` (`f64`). Its operation byte
is:

| Value | Operation | Operand/result rule |
| ---: | --- | --- |
| 0 | constant | followed by one raw little-endian `u32` for `f32` or `u64` for `f64` |
| 1 | add | two same-type values to one same-type value |
| 2 | subtract | two same-type values to one same-type value |
| 3 | multiply | two same-type values to one same-type value |
| 4 | divide | two same-type values to one same-type value |
| 5 | negate | one value to one same-type value |
| 6 | equal | two same-type values to `bool` |
| 7 | not equal | two same-type values to `bool` |
| 8 | less | two same-type values to `bool` |
| 9 | less equal | two same-type values to `bool` |
| 10 | greater | two same-type values to `bool` |
| 11 | greater equal | two same-type values to `bool` |

Values use IEEE 754 binary32 or binary64 interchange encodings. Arithmetic is
round-to-nearest, ties-to-even and preserves finite values, subnormals,
infinities, and signed zero. A NaN operand or an invalid arithmetic result
produces the type's canonical quiet NaN: raw `7FC00000` for `f32` or
`7FF8000000000000` for `f64`. Negating NaN also produces that canonical value;
otherwise negate toggles the sign bit. Division by zero and invalid operations
produce the corresponding IEEE infinity or canonical NaN rather than a Windvale
trap. Positive and negative zero compare equal. A comparison involving NaN is
false except `not equal`, which is true. There is no implicit conversion between
either floating type or any other value type. An unknown selector, a wrong type
tag, a truncated or over-wide immediate, or mismatched operands is malformed
bytecode.

`C3` has no immediate payload and produces one canonical `unit` operand-stack
value. It is not a `void` placeholder: calls returning `unit` push one value and
the ordinary return instruction consumes that value. Calls returning `never`
push no value because normal return from the callee is impossible. A function
whose result is `never` contains no return instruction; its verified reachable
control flow cannot fall through.

`C4` carries one canonical variant Types index and one selector encoded as
`case index * 64 + field index`. It consumes one value of that exact nominal
variant and produces the selected field's exact declared shape. Both indices
must exist, and the selected case must contain that field. Opcode `99`
(`variant.payload`) remains valid only when the selected case has exactly one
field. Construction consumes exactly zero through 64 operands in declaration
order. There is no dynamic field lookup, name lookup, allocation, conversion,
or representation exposure in either instruction.

`C5` carries one canonical kind-4 Types index. It consumes exactly that
descriptor's `N` element values in ascending index order, requires every operand
to have the descriptor's exact element shape, and produces one shape-22 value
with the same type index. `C6` consumes that exact array followed by one complete
`u64` index and produces the descriptor's exact element shape. An index greater
than or equal to `N` traps with `WVR3008`. Construction and access expose no
backing address, capacity, host layout, conversion, common-type inference, or
dynamic element lookup. A target may use inline storage, an arena, or another
unobservable bounded representation; resource exhaustion remains a target
failure and cannot change successful value semantics.

## Verification

The complete compiler-aligned verifier currently admits WVB 1.11 through 1.37.
It rejects candidate WVB 1.38 before execution. The focused independent 1.38
reader checks only the publication structure and `E0` categorical immediates
listed above; it is not a substitute for complete metadata, typed-stack,
control-flow, affine-lifetime, capability, provider, or ABI-call verification.

Verification is required before execution and rejects a module unless:

- The header, sections, strings, counts, types, and code ranges are structurally valid and within implementation limits.
- The version is WVB 1.11 through 1.37 and the Module metadata presence byte is
  encoded exactly as specified above. Fixed integers require at least 1.12,
  runes 1.13, floating point 1.14, `unit` or `never` 1.15, multi-field variants
  1.16, fixed arrays 1.17, Vector or Sequence shapes 1.18, collection execution
  1.19, Vector `local.take` 1.20, kind-`7` enums 1.22, budget split 1.23,
  reserved Vector construction 1.24, recoverable append 1.25, exact Vector
  parameters 1.26, reserved growth 1.27, and Vector-containing aggregates 1.28.
  Each extension requires its first named minor or a later admitted inherited
  profile. Shape `25` uses only its exact
  WVB 1.21-through-1.28, WVB 1.32, or WVB 1.33-through-1.37 rules; shape `34`
  and `source.length` require WVB 1.29 through 1.37; callable kind `8`, shape
  `35`, `function.reference`, and `call.indirect` require WVB 1.30 through
  1.37; `closure.create` requires WVB 1.31 through 1.37; extended callable or
  task evidence requires WVB 1.32 through 1.37; `unsafe.scratch.construct`
  requires WVB 1.33 through 1.37; shape `36` requires WVB 1.34 through 1.37;
  `unsafe.scratch.length` requires WVB 1.35 through 1.37;
  `unsafe.write-region.borrow` requires WVB 1.36 or 1.37; and
  `unsafe.write-pointer.borrow` requires exactly WVB 1.37.
- Platform scopes, authority, required capabilities, optional capabilities, and capability major versions satisfy the independent module-metadata rules.
- Every function decodes completely into known instructions.
- Branch targets identify instruction boundaries in the same function.
- Every local, data, function, and capability index is valid and has the required type.
- Every record, enum, variant, fixed-array, Vector, Sequence, or callable declaration, nominal or callable shape, constructor operand, field or element access, legacy payload access, case test, constant, and enum comparison has valid identity, bounded indices and counts, unique names where applicable, and exact types.
- Every collection shape has an admitted non-collection element and maximum; builder transitions and sequence operations have exact types and cannot cross forbidden boundaries.
- Division/remainder, bitwise/shift, fixed-integer, rune, floating-point, and content equality operations have exact operand types; shifts use a `u32` count and content equality is limited to text and bytes.
- Every byte-data declaration is bounded and every byte intrinsic receives exactly the required operand types.
- Strict UTF-8 decoding and encoding, safe quoting, signed and `u64` little-endian reads, fixed-width byte construction, byte concatenation, SHA-256 identity, and explicit `u8` to `u32` conversion receive and produce their exact declared types.
- Operand-stack types and depths agree at control-flow merges.
- WVB 1.20 Vector local stores, loads, and takes preserve definite unique-owner availability at every forward control-flow join and exact owned-slot equality at every backedge; functions using `local.take` satisfy its explicit instruction, Vector-local, 64-owner, and 4,096-instruction limits.
- WVB 1.21 contains exactly one shape-25 token in the sole parameter of exported `Main`, returns `i32`, and has no instruction that reads, stores, copies, returns, embeds, or constructs that token. WVB 1.22 applies the same rule if shape `25` is present; otherwise its exported entry is ordinary `Main() -> i32`. WVB 1.23 through WVB 1.28 retain the exact entry and permit affine budget/result locals only in `Main` under the bounded forward-join and exact-backedge ownership proof.
- Every WVB 1.22-or-later kind-7 descriptor has exact backing identity `6`, 1 through 256 uniquely named members, unique one-byte values, and participates in the same canonical enum ordering and shape-8 identity as kind `2`; WVB 1.22 contains at least one kind-7 descriptor, WVB 1.23 contains at least one exact opcode `CE`, WVB 1.24 contains at least one exact opcode `CF`, WVB 1.25 contains at least one exact opcode `D0`, WVB 1.26 contains at least one exact Vector parameter, WVB 1.27 contains at least one exact opcode `D1`, and WVB 1.28 contains recursively owned aggregate evidence.
- Every opcode `CE` references an available shape-25 parent local and the exact structurally validated Split Result type, consumes `u64` then `u32`, preserves failure atomicity, and produces one affine result owner.
- Every opcode `CF` references an available shape-25 budget local and exact structurally validated Vector Result type, consumes one `u64`, consumes the budget on every ordinary Result path, and produces one affine result owner whose Valid payload transfers one exact Vector.
- Every opcode `D0` references a direct non-parameter exact Vector local and exact structurally validated append Result type, consumes one matching scalar item, preserves the Vector and returns that item on capacity refusal, and mutates the Vector exactly once only on success.
- Every opcode `D1` references a direct non-parameter exact Vector local, a distinct available shape-25 budget slot, and exact structurally validated `Result<unit, Allocationˉfailure>`; consumes one `u64` maximum; preserves both owners on refusal; and swaps the complete backing and lease only on success.
- Every WVB 1.26-or-later Vector parameter uses exact shape `23`, `26`, or `27` with a matching kind-5 nominal index; borrowed shapes occur nowhere else; calls supply unique evidence to shape `23` and retaining evidence to shapes `26` and `27`; and reverse-order callee cleanup cannot release the caller's preserved owner.
- Every WVB 1.28 through WVB 1.37 owned aggregate is classified recursively and moved as a whole through constructors, stores, calls, and returns; each shape-`28`, `29`, or `30` view is a matching local-only observer sequence and cannot be taken or escape, except that WVB 1.35 and 1.36 admit the exact scratch shape-`28` helper parameters required by `DD` and `DE`, and WVB 1.37 admits the exact immutable region parameter required by `DF`; teardown releases each reachable nested Vector descriptor exactly once in the executable profiles.
- Every WVB 1.29 module contains exactly one exported `Main(Platformˉfile.Sourceˉfile) -> i32`, admits shape `34` only in that parameter and its move-owned locals, contains 1 through 64 exact `source.length` instructions over available non-parameter source locals, and cannot construct, copy, return, embed, or expose the source owner. WVB 1.32 admits inherited source-file evidence only where its independent entry/profile constraints remain exact.
- Every kind-`8` descriptor has the containing module's exact profile, one exact non-`void`/non-`never` result, at most 64 exact parameters, and a terminal position after all nominal types; every shape `35` names one kind-`8` entry; every `function.reference` target exactly matches its descriptor; every `call.indirect` consumes that exact callable plus the descriptor's exact arguments; every `closure.create` has 1 through 64 admitted captures and an exact physical target signature; source publication separately proves the applicable safety, generic, effect, capture, and lifetime restrictions before flattening; every WVB 1.30 module contains at least one `D3` or `D4`; every WVB 1.31 module contains at least one `D5`; and WVB 1.32 validates the exact descriptor trailer plus at least one task operation.
- Every WVB 1.32 structured-task entry, canonical Foundation identity, construction/spawn/outcome layout, async callable descriptor, effect mask, scope/handle local, origin relation, policy, and affine ownership transition is exact; every accepted child is awaited or joined before scope consumption; and invalid limits, forged handles, repeated await, missing exit, mismatched outcomes, scope escape, and control-flow ownership disagreement reject before execution.
- Every WVB 1.33 module uses system profile `3`, contains 1 through 4,096 exact `DC` instructions and at most 256 distinct scratch-nominal/ABI bindings, inherits the exact WVB 1.32 callable and task encodings when present, and validates the canonical scratch, Result, foreign-memory failure, allocation-failure, and reason-enum layouts. Each `DC` consumes two `u64` stack values and one available shape-`25` owner, produces one affine scratch Result, names a kind-`2` or kind-`7` ABI enum, and agrees with every other ABI binding for the same scratch nominal. Ordinary record operations cannot construct or expose the opaque scratch token.
- Every WVB 1.34 module uses system profile `3`, contains at least one shape-`36` parameter, and confines every shape-`36` local to the canonical owner-load/view-store/view-load/direct-call sequence. Shape `36` is invalid in results, nominal payloads, collection elements, callable descriptors, `local.take`, indirect calls, and modules other than 1.34 through 1.37. Its direct-call target has shape `36` at the exact corresponding parameter position; the source shape-`25` owner remains available. When opcode `DC` is present, the complete WVB 1.33 scratch, ABI, layout, and count rules remain exact.
- Every WVB 1.35 module uses system profile `3`, contains 1 through 4,096 exact opcode-`DD` instructions and at most 256 distinct length relations, and admits an exact shape-`28` scratch helper parameter only as an immutable view. Each `DD` names an available canonical scratch owner or view and a matching kind-`2` or kind-`7` ABI enum, produces `u64`, preserves the owner, and is covered by an exact construction relation. The inherited WVB 1.33 construction and WVB 1.34 budget-borrow rules remain exact when those features are present.
- Every WVB 1.36 module uses system profile `3`, contains 1 through 4,096 exact opcode-`DE` instructions and at most 256 distinct scratch/region/ABI relations, and admits an exact shape-`28` scratch helper parameter only as a mutable view. Each `DE` consumes three `u64` values, consumes an available exact scratch owner or mutable view, and produces the canonical affine region Result. The Valid region payload cannot be extracted, called, returned, copied, or otherwise escaped; exact Failure data remains observable. The inherited WVB 1.33-through-1.35 rules remain exact when those features are present.
- Every WVB 1.37 module uses system profile `3`, contains 1 through 4,096 exact opcode-`DF` instructions and at most 256 distinct region/pointer/ABI relations, and admits the exact shape-`28` immutable region parameter only for a direct `DF`. Each `DF` observes an available canonical region, produces verifier-internal affine pointer kind `38`, and agrees with every prior scratch/region/ABI relation. A pointer may be discarded or moved once through the compiler-generated two-local sequence; it cannot be taken, copied, loaded after consumption, embedded, passed to a call, returned, or carried across an ownership-state disagreement. The inherited WVB 1.33-through-1.36 rules remain exact when those features are present.
- Calls consume the declared parameter types, push one result for every result
  other than `void` or `never`, and push nothing for `void` or `never`.
- Returns match the function return type; a `never` function has no return
  instruction.
- Control cannot fall past the end of a function.
- Every instruction is reachable in Seed.
- Computed maximum stack depth equals the declared maximum.
- Capabilities and their signatures are recognized by the Seed capability catalog.

## Implementation limits

- Module bytes: 16 MiB
- Source-file snapshot: 1 MiB before guest execution
- Sections: exactly 7
- WVB 1.33 unsafe-scratch instructions: 4,096
- WVB 1.33 distinct scratch/ABI bindings: 256
- WVB 1.34 local loads between a borrowed-budget view and its direct call: 64
- WVB 1.35 unsafe-scratch length instructions: 4,096
- WVB 1.35 distinct scratch-length/ABI relations: 256
- WVB 1.36 unsafe write-region instructions: 4,096
- WVB 1.36 distinct scratch/region/ABI relations: 256
- WVB 1.37 unsafe write-pointer instructions: 4,096
- WVB 1.37 distinct region/pointer/ABI relations: 256
- WVB 1.38 paired Foreign calls admitted by the source coordinator: 4,096
- WVB 1.38 registered binding identities: exactly one, identity `1`
- UTF-8 value: 1 MiB
- Byte-data value: 4 MiB
- Declaration name: 255 UTF-8 bytes
- Capabilities: 32
- Platform scopes: 32
- Required capability requirements: 32
- Optional capability requirements: 32
- Data declarations: 4,096
- Functions: 4,096
- Total Types entries: 1,024
- Callable descriptors: 256
- Parameters per callable descriptor: 64
- `function.reference` instructions per module: 65,536
- `call.indirect` instructions per module: 65,536
- `closure.create` instructions per module: 65,536
- Task instructions per module: 65,536
- Lexical task-scope nesting: 32
- Task scopes per function: 256
- Accepted children per portable scope: 64
- Retained completions per portable scope: 64
- Retained task state per portable scope: 1 MiB
- Work units per portable scope: 1,000,000
- Child call frames per portable scope: 64
- Created closure environments per runtime execution: 1,024
- Runtime closure-environment arena per execution: 536,576 bytes (524 KiB)
- Fields per record: 64
- Members per enum: 256
- Parameters plus locals per function: 8,192
- Code per function: 1 MiB
- Instructions per function: 100,000
- Operand stack: 4,096 values
