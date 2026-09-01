# Windvale native WVB-runner reconstruction

## Status and scope

The profile-5 WVB runner is a paired-host-reconstructed,
qualification-pending native candidate. It preserves the fixed portable
`Main() -> i32` execution command and additionally
owns the internal bounded scripting mode defined by
[Decision 0735](../Documents/Decisions/0735-Implement-The-First-Windvale-Scripting-Slice.md).
The outer runner binds five capabilities to nine ordered services. The exact
candidate reconstructs from the complete Project 2 source closure through the
current split compiler, segmented native staging/link/transport path,
hosted-verifier profile, and paired Windows/Linux container materializers.
The same source-built scalar core also owns the focused capability-free System
oracle for WVB 1.33 unsafe scratch and WVB 1.34 immutable borrowed-memory-budget
calls. That local Windows evidence is not part of the older profile-5 paired
artifact identity or a paired-host qualification claim.

The project names its root tool plus the SHA-256, scalar-interpreter, envelope,
and formatting dependencies in canonical module order. Project paths are
relative to the manifest; this contract does not require all `.wvproj` files to
live at the repository root. Component-local manifests remain appropriate, and
a future workspace/index contract may improve discovery without changing
Project 1 semantics.

## Current exact products

The following table is the current profile-5 runner candidate. Independent
Windows/Linux reconstruction reproduces all three identities; the pending
qualification field advances only after the final Slice 7 Qualification gate.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB runner | 482,767 | `fc4724c7756f22eb52dd6ed4da9737a865e14ea4d52df1de69fc10236970ff4f` |
| Windows application | 5,907,456 | `2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b` |
| Linux application | 5,906,432 | `611cfbf9fd95e9b29df4a38e3ac392dc9eea87b760b81ff572bad8af6f235eae` |

Segmented staging emits 5,899,132 object bytes across 13 chunks. Linking emits a
5,889,164-byte native image with entry offset 150,541; canonical transport
reduces that image to two chunks. Those intermediate chunks are reproducible
construction evidence, not retained shipment artifacts. Removing the obsolete
monolithic WVO avoids carrying a second copy of the runner's native code.

## WVB 1.33 and WVB 1.34 focused System execution

Execution-request major `1` admits a capability-free System-profile
`Main(Memoryˉbudget) -> i32` only for the exact WVB 1.33/1.34 verifier-approved
subset. The provider supplies one fresh bounded budget token. WVB 1.33 may
consume it through exact unsafe-scratch construction. WVB 1.34 may first create
one shape-`36` immutable view, pass that view to an exact direct-call parameter,
and later consume the unchanged shape-`25` owner.

The interpreter carries both identities in the same opaque scalar slot but
never treats shape `36` as an owner. It rejects shape `36` as a result, field,
payload, collection element, callable shape, parameter load, `local.take`, or
noncanonical local use. The focused oracle returns `42` after a borrowed call
followed by a 64-byte scratch allocation and rejects six shape/version
corruptions before execution. Mutable borrowing, budget-query operations,
pointer access, browser packaging, and Windvale OS execution remain outside
this profile.

## Construction and execution

The paired constructors accept one existing output directory:

```text
Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd <existing-output-directory>
./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>
```

They reject the live candidate directory, build the WVB through the pinned
bootstrap and current split compiler, stage/link/transport one canonical native
image, then construct profile-5 Windows and Linux applications from that same
image. Success reports:

```text
native WVB runner reconstruction status=Complete artifacts=3
```

`Run-Wvb.cmd` and `Run-Wvb.sh` execute the corresponding digest-bound candidate
with either one module argument, the exact optional `--report-steps` flag, or
the strict structured-task environment form documented below.
The runner supplies the scalar interpreter with a fixed 1,000,000-instruction
budget, matching the Stage 0 CLI's default execution budget. Default output
remains `Result: <i32>`. Reporting adds one
`Instructions: <u32>` line; the canonical Sum fixture reports result `29` and
exactly `203` instructions.

The current source-built runner accepts WVB 1.11 through 1.32. Its shared scalar
interpreter implements the WVB 1.12 `i8`, `i16`, and `u16` family with the exact
checked overflow, division-by-zero, and shift traps from Decision 0768. The bounded
instruction-directory scan and fixed-integer evaluator live in focused modules
to keep the main interpreter below compiler/lowerer function limits; they are
not a second interpreter or alternate semantic path.

The same interpreter implements the WVB 1.13 rune constant, equality, and
inequality family from Decision 0769. Rune scanning and execution live in one
focused core module, reject non-scalars and unknown selectors before execution,
and retain the ordinary shared stack and control-flow path.

The WVB 1.14 floating core executes raw binary32 and binary64 values with a
software integer implementation of round-to-nearest, ties-to-even arithmetic,
canonical NaN, signed zero, subnormals, infinities, unary negation, and all six
comparisons. It does not inherit host floating-point modes or locale behavior.
The WVB 1.15 path represents `unit` as one canonical zero scalar cell and
executes opcode `C3` through the ordinary stack, local, record, call, and return
machinery. It admits `never` only as a function result; a call pushes no value,
and verified bytecode cannot return from that function.
The WVB 1.16 path stores a variant in one eight-byte cell containing a bounded
aggregate slot and exact nominal/case owner token. Variant fields share the
fixed 768-cell immutable record arena. Construction allocates only declared
fields; case tests allocate nothing; payload and named-field reads preserve the
verified shape. Stack values, active locals, and saved frame locals are traced
through selected field metadata when the arena collects. A malformed
case/value mismatch returns `WVR3017`.
The WVB 1.17 path represents an immutable fixed array in the same traced,
bounded aggregate arena, tagged by its exact kind-4 Types identity. `C5` copies
the descriptor's fixed number of already evaluated element cells in index order;
`C6` consumes a full `u64` index, returns the exact element shape, and reports
`WVR3008` when the index is not below the fixed length. Nested array, record, and
variant cells participate in the same type-directed mark/sweep traversal.
The format admits lengths through 4,095; this profile's 768-cell shared arena is
an explicit finite runtime resource and may report bounded aggregate exhaustion
for a valid value that does not fit. It never changes the value's length,
silently allocates dynamic capacity, or skips the bounds check.
The WVB 1.18 envelope and function-directory paths parse kind-5 Vector,
kind-6 Sequence, and matching shapes `23` and `24`. WVB 1.19 adds executable
reserved construction, unchecked append after a proved capacity check,
consuming freeze, Vector/Sequence length, and checked Sequence element access
for resource-free scalar elements. Each value is one eight-byte descriptor
into the existing 64 KiB refcounted heap. Its backing stores a `u32` current
length, a positive `u32` retained maximum, and exactly that many eight-byte
cells. The scalar profile admits at most 2,047 cells per backing allocation,
so one value occupies at most 16 KiB.

Vector mutation updates the uniquely owned backing; freeze transfers the
linear Vector token to an immutable Sequence without allocating. The token is
created only by reserved construction, is preserved by append and Vector
length, and is consumed by freeze. Ordinary Vector local loads do not recreate
it. Sequence length and element access preserve their shared immutable owner.
The runner mirrors these verifier rules with separate descriptor, collection,
and unique-Vector stack flags. Sequence local loads retain; stores release the
replaced owner; function teardown releases Vector and Sequence locals. WVB
1.20 adds `local.take`: the runner copies an available non-parameter Vector
local descriptor to the operand stack, zeros the eight-byte source local, and
transfers the unique-Vector flag without changing the allocation reference
count. Parameter slots are rejected until calls transfer unique evidence. The
verifier rejects out-of-range, uninitialized, and repeated takes before
execution.
WVB 1.21, WVB 1.22 with shape `25`, or WVB 1.23 through WVB 1.28 supplies one fresh
opaque root-budget token to the sole parameter of exported
`Main(Memoryˉbudget) -> i32`. The
interpreter validates that exact
shape placement before execution and rejects every bytecode load or store of
the token. The active invocation owns an identity/generation pair outside the
ordinary forgeable value vocabulary. On completed top-level return it verifies
the pair, zeros the parameter cell, and releases the invocation token exactly
once before publishing the result. A failed invocation is torn down as one
resource domain.
WVB 1.22 parses kind-7 enum descriptors only with exact source backing identity
`6` and one-byte member tags. The existing enum value cell, constant,
comparison, and name operations use the same nominal identity path as kind `2`;
there is no host enum conversion or second interpreter. A module whose newest
feature is kind `7` may retain the ordinary `Main() -> i32` entry. If it also
contains shape `25`, the WVB 1.21 budget transfer and teardown rules remain
mandatory.
WVB 1.23 adds exact opcode `CE`. Its two immediates select one available budget
local and the exact Split Result type; its stack operands are `u64` maximum
bytes followed by `u32` maximum children. The runner binds a 98,304-byte,
64-child root and uses the fixed-capacity accounting provider. Success
atomically reserves both limits and returns one Valid child owner; refusal
preserves the parent and returns the exact Failure evidence. Shape-25 budget
locals and Split-result locals move through `local.take`, never through a
retaining load. Invocation teardown releases every surviving domain under the
provider's fixed bounds.
WVB 1.24 adds exact opcode `CF`. Its two immediates select one available budget
local and exact `Result<Vector<T>, Allocationˉfailure>` type, and its sole stack
operand is `u64 Maximumˉitems`. Zero traps with `WVR3008`. For the current
resource-free scalar element subset, a positive representable maximum converts
the budget to one private allocation lease, allocates the reserved Vector
backing, and publishes Valid. A positive target-unrepresentable maximum
publishes exact Failure and releases the still-local owner. The lease remains
attached to the backing and is released exactly once with the final descriptor.
Requested-byte evidence saturates at `u64`. The scalar runner's 2,047-cell
backing limit is an explicit profile bound, not a portable Language maximum.
Allocation metadata reuses inactive entries and first-fits released spans.
Text, bytes, aggregates, and nested collection elements remain outside this
checkpoint because their element-owned destruction and tracing are not yet
implemented.
WVB 1.25 adds exact opcode `D0`. Its two immediates select one direct mutable
non-parameter `Vector<T>` local and exact
`Result<unit, Vectorˉappendˉfailure<T>>`; its sole stack operand is exact `T`.
The current scalar profile appends atomically when capacity remains. At the
reserved maximum it leaves the backing unchanged and returns exact
`Capacityˉexhausted(Maximumˉitems)` plus the original item. Append neither
allocates nor changes the allocation lease. The same resource-free scalar
element restriction remains until element-owned destruction and tracing land.
WVB 1.26 adds exact Vector parameter modes without a new opcode. Parameter
shape `23` receives a transferred unique owner, shape `26` receives an
immutable borrow, and shape `27` receives an exclusive mutable borrow. The
function-directory reader validates that borrowed tags occur only in parameter
lists, retains one bounded internal mode byte per parameter, and normalizes the
runtime cell shape to ordinary Vector. By-value arguments arrive through
`local.take`; borrowed arguments arrive through a retaining `local.load`.
Normal return releases descriptor-owning cells in reverse slot order, so the
callee consumes a transferred owner or balances a borrow's temporary retain
without releasing the caller's preserved owner. A trap tears down the bounded
invocation domain. No raw pointer, source slot, borrow handle, or mode trailer
enters the runtime representation.

WVB 1.27 adds exact opcode `D1`. Its three immediates select one direct
non-parameter `Vector<T>` local, one distinct available `Memoryˉbudget` slot,
and exact `Result<unit, Allocationˉfailure>`; its sole stack operand is the
new `u64` maximum. The current resource-free scalar profile reserves and
allocates the full replacement before releasing the old descriptor. It copies
the initialized prefix, attaches the new lease, then swaps the local exactly
once. Target or budget refusal returns exact failure and leaves Vector plus
budget state unchanged. A non-increasing maximum traps with `WVR3008`, and the
2,047-cell target bound remains a profile limit. No provider acquisition,
partial growth, or backing address becomes visible.

WVB 1.28 recursively treats records, variants, and fixed arrays containing a
Vector as owned values. Whole aggregates move through construction, locals,
ordinary calls, and returns. Generated field and element observation uses
local-only borrowed-view shapes `28`, `29`, and `30`; function-directory
decoding validates those shapes only in 1.28 and normalizes their runtime cells
to the ordinary aggregate representation. The compiler-aligned verifier confines
each view to one exact store/load/observer sequence, so the runtime receives no
general borrow handle, pointer, or independently storable alias.

Aggregate construction retains descriptor-bearing fields. At ordinary return,
the bounded aggregate mark/sweep root set includes the remaining operand stack,
the returned aggregate, and caller frames, but excludes departing locals.
Unreachable aggregate cells are swept in deterministic arena order and release
nested Vector descriptors and their allocation leases. The same bounded sweep
runs before top-level budget teardown. This extends the existing 768-cell arena
and 64 KiB descriptor heap without adding unbounded tracing or host allocation.
Borrowed aggregate parameters/results, resource-bearing Vector elements, and
user-defined destruction remain outside this profile.

WVB 1.29 adds one launcher-supplied `Sourceˉfile` resource. Public mode
`--source-file <module.wvb> <snapshot-file>` reads an immutable snapshot before
guest execution and rejects more than 1,048,576 bytes. Request major `5`
contains the bounded bytes plus exactly one read right, a nonzero provider
generation, and an equal resource generation. Envelope validation rejects
wrong rights, zero or stale generations, inconsistent lengths, and oversized
input before the first guest instruction. The guest receives neither the path
nor an open host handle.

The runtime represents shape `34` as one fixed cell containing the admitted
`u32` length and provider generation. The exact exported
`Main(Sourceˉfile) -> i32` parameter begins as the sole owner, must move into a
non-parameter local, and is released exactly once through ordinary owned-local
teardown. Opcode `D2 source.length <local>` validates shape, the 1 MiB bound,
and the current generation before pushing `u64`. Stale or malformed runtime
state fails closed. There is no byte-reading opcode, path conversion, provider
acquisition, retry, or ambient filesystem authority in this checkpoint.

WVB 1.30 adds representation-hidden noncapturing callable values. Opcode `D3`
stores the verified function index in the low `u32` of the ordinary eight-byte
scalar cell and the exact kind-`8` callable Types index plus one in the high
`u32`. Opcode `D4` requires that encoded type identity, validates the target
function against the descriptor again at execution, removes the callable and
its exact arguments, and enters the existing bounded call-frame path. The
value is not a host pointer and the cell layout is not a portable ABI promise.
Call depth, instruction budget, local count, stack depth, and frame-storage
bounds remain unchanged.

WVB 1.31 adds opcode `D5`, which consumes 1 through 64 verified inline scalar
or enum captures and publishes the same exact shape-`35` callable identity.
The low cell word is tagged as an offset into a representation-private
environment arena; the high word remains the callable Types index plus one.
`D4` recognizes the tag, validates the stored target, type, capture count, and
physical arity, copies the immutable capture prefix into callee locals, and
then installs the public arguments through the ordinary frame path. Direct
function references retain their WVB 1.30 representation.

One invocation creates at most 1,024 environments and retains at most 536,576
bytes (524 KiB) of environment records, each containing a target, callable
type, capture count, and exact captured cells. The complete arena is discarded on invocation
teardown. Text, bytes, aggregates,
collections, callables, resource owners, and borrows cannot enter it, so this
checkpoint adds no hidden tracing, reference counting, or lifetime behavior.

The owned-call fixture is a deterministic 1,733-byte WVB 1.26 module at
SHA-256
`ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387`.
The compiler-aligned verifier rejects six version, mode, return, and local
corruptions before execution. The source-built runner executes borrow followed
by value transfer, owned results/returns, and equal forward-path consumption,
then returns `42`.

The same bounded scalar path executes the `u64` constant, arithmetic,
comparison, bitwise, shift, `bytes.from_u64_little`, and `u64.from_u32`
operations emitted by the compiler's exact floating-literal parser. The focused
Language 1.0 owner executes both the compiler front-end self-test and the
compiler-produced floating program through the retained candidate.

An earlier fixed-array development checkpoint produced a 336,214-byte WVB at SHA-256
`e5ecddf54f743ee38c07d83d34a421984d48138ea046669a9b29e42c48d73686`.
It contains 156 functions and 302,459 code bytes. Cohesive directory, request,
data/local/bytes, collection, aggregate, and extended-operation helpers keep
every source-built function below the existing bytecode and 2,048 native
physical-cell limits; neither limit was raised.
That historical build was not promoted independently; the current candidate in
the table above includes its behavior together with the later task closure.
It accepts the deterministic 375-byte fixed-array compiler fixture, returns
`42`, and reports code `3008` for the verified out-of-bounds mutation. It also
parses the deterministic 436-byte WVB 1.18 Vector/Sequence metadata fixture and
executes its independent `Main` with result `42`. A deterministic 1,156-byte
WVB 1.20 derivative executes all six collection opcodes plus `local.take`,
performs six 16-KiB allocation cycles that require descriptor reclamation, and
returns `42`; its
SHA-256 is
`baa69aadf3b9c65900110d9aa3372989e051045e30207a87b720dbc0a663dd25`.
Five malformed type/version/index cases reject semantically, one copied-Vector case
rejects during typed execution, uninitialized and repeated transfers reject
during control/ownership analysis, and capacity and index violations remain
valid bytecode and fail exactly as `WVR3008`. Paired
reconstruction, browser execution, and candidate promotion remain separate
gates.

The compiler-produced budget-entry fixture is a deterministic 242-byte WVB
1.21 module with 16 code bytes and SHA-256
`499c59fa1207917fd64ee0703569d3dc4a80c5075fc99923e657adc5e4f9ed65`.
The compiler-aligned verifier accepts it and the source-built runner returns
`42` after releasing the entry token. A separate bounded verifier accepts the
canonical module and rejects nine mutations spanning version, parameter shape
and count, entry identity, return and local placement, local load and store, and
a missing export. This is
development evidence for the source contract, not a repinning of the promoted
paired-host runner.

The compiler-produced `u8` enum fixture is a deterministic 415-byte WVB 1.22
module at SHA-256
`961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed`.
The compiler-aligned verifier accepts its exact kind-7 descriptor and the
source-built runner returns `42`. A separate bounded verifier rejects nine
mutations spanning version downgrade/advance, backing identity, duplicate and
truncated values, a missing kind-7 feature, unknown kind, and an out-of-range
enum shape index.

The executable Split fixture is deterministic 752-byte WVB 1.23 at SHA-256
`5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53`.
The source-built runner returns `42` after a successful 4,096-byte/two-child
reservation. A second fixture requests 100,000 bytes and returns `42` through
the typed refusal branch. The compiler-aligned verifier accepts both and
rejects nine version, opcode, local, type, and layout corruptions. This is
development evidence; paired-host runner reconstruction remains unrepinned.

The executable fallible Vector fixture is deterministic 747-byte WVB 1.24 at
SHA-256
`e25ff63b466d3e4a219afdc03a64c2ff53418dffc9039fea0678ff3328d2dcd1`.
The compiler-aligned verifier accepts successful, refused, and zero-precondition
modules. The runner returns `42` for successful construction and typed
target-unaddressable refusal; zero fails with `WVR3008` after four guest
instructions. Ten exact version, opcode, local, Result, Vector, and failure-
layout mutations reject. This is current Windows development evidence; paired-
host reconstruction and promoted-candidate repinning remain separate gates.

The executable growth fixture is deterministic 3,628-byte WVB 1.27 at SHA-256
`30de39bdd12ad7718ad1fb465b14bc42f8463b6ecfc6ba1f10494cb6e67c5b59`.
It proves a 40-byte request refuses against 24 available bytes without changing
length `1`, then a 24-byte replacement succeeds, accepts a second item, and
returns `42`. Fifteen exact version, opcode, local, result, allocation-layout,
and truncated-width mutations reject. The exact 88-case focused owner passed on
Windows and Linux with identical portable fixture identities. That historical
checkpoint predated the current candidate repin.

The executable aggregate fixture is deterministic 1,538-byte WVB 1.28 at
SHA-256
`b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395`.
It observes mutable and immutable fields of
`Workˉqueue<Vector<i32>>`, transfers the whole record through an ordinary call,
releases its nested Vector exactly once, and returns `42`. Six exact
version/view/local/take mutations reject before execution. The combined focused
owner's paired-host result is recorded in the Language 1.0 migration evidence;
that historical checkpoint also predates the current candidate repin.

The executable source-snapshot fixture is deterministic 373-byte WVB 1.29 at
SHA-256
`01065b752d7ea6d64e3bf36bdd4d8a0d2e5b7faf6794de173580003ed3935d05`.
The source-built runner returns `42` for a 42-byte snapshot and `1` for a
41-byte snapshot; a 1,048,577-byte input returns command status `64` with
`wvb run status=Sourceˉsnapshotˉtooˉlarge`. Six exact version, shape, opcode,
target, and transfer mutations reject before execution. This is focused Windows
development evidence; independent Linux reproduction and promoted-candidate
repinning remain separate gates.

The executable callable fixture is deterministic 400-byte WVB 1.30 at SHA-256
`30eab353a6187ead317438d2c63a2bd6aa53d9ec682bc5c59d9d3b82530edfaf`.
It creates one exact named function reference, invokes it indirectly with one
`i32` argument, returns `42`, and reports 24 guest instructions. The
compiler-aligned verifier accepts the exact product and rejects five version,
target-signature, reference-type, call-type, and descriptor-kind mutations.
The current development runner WVB contains 166 functions in 360,679 bytes at
SHA-256
`8ea9c8dd900f684bc32e24fb8a37309f91b9df002c866c3c6eba99200492dd37`.
This is focused Windows development evidence; independent Linux reproduction,
the broader Qualification gate, and promoted-candidate repinning remain
separate.

The executable closure-environment oracle is deterministic 325-byte WVB 1.31
at SHA-256
`397f716af132192697c77d9f4f03e72c937e188aca78cf0474c9faaa2234e0e2`.
It captures `i32` value `40`, supplies public argument `2`, returns `42`, and
reports 11 guest instructions. The compiler-aligned verifier accepts the exact
module and rejects nine version, target, type, count, capture-shape,
reference-backed-capture, indirect-call, and descriptor mutations. This is a
portable scalar-runtime checkpoint. Source closure-body lowering and the
selected frame-owned native callable ABI are implemented; paired-host
reconstruction and candidate promotion remain separate gates. The current development runner WVB contains 167
functions and 324,568 code bytes in 361,080 module bytes at SHA-256
`3cbd89599025499f3d5147e50fc94a1de82ff15bc27d19d298087fed401b3acd`.
Factoring closure creation out of the indirect-call path and removing repeated
upper-version tests reduced the first 366,728-byte implementation without
raising the unchanged 64-MiB native lowering plan limit. Each bounded closure
record is also assembled before one arena append, so a creation does not copy
the complete retained arena once per header field.

WVB 1.32 adds a deterministic queued structured-task scheduler beneath the
same verified source semantics. The launcher supplies the exact root memory
budget and operation context. Opcodes `D6` through `DB` construct one lexical
scope, derive its context, transfer accepted async work, consume handles at
await, request cancellation idempotently, and join/consume the scope. The
runtime stores fixed task records in one 10,040-byte version-3 state value and
admits at most 64 accepted/runnable/retained children under the lower
application limit. The bounded header retains exact root-context, clock,
deadline, and task-runtime generation evidence. A cooperative observation
applies deadline, runtime loss/restart, cancellation, or byte-identical
continuation in that priority order.
Every accepted live child reserves one completion position. `Task.Spawn`
materializes the child locals, appends a bounded queued-child descriptor, and
returns the typed handle before child execution. A full runnable, completion,
or queue bound rejects spawn before work ownership moves; completion retains
the reservation until the exact affine handle is awaited or the scope tears
down.

Each accepted child also reserves its exact sequential scheduler footprint
before ownership moves: one 56-byte queued-child descriptor, the child locals,
and the newly suspended parent frame. The descriptor covers the smaller
40-byte active continuation plus reserved 8-byte completion cell used after
dispatch. The complete queue is bounded to 1 MiB and each child-local payload to
4,096 bytes. A reservation larger than the scope's remaining retained-byte
limit returns the original closure as typed
`Memoryˉfailure(Budgetˉexhausted)` with exact requested and available bytes.
Completion keeps the reservation; await or scope teardown releases it.

When await observes a runnable handle, the reference scheduler selects only
queued children from that handle's origin scope. Each consecutive four-slot
group uses lane priority `3, 1, 0, 2`. Child completion returns to the same
await, which then consumes the completed task; source reports therefore remain
creation ordered even though runtime completion order differs. This is a
deterministic single-thread oracle, not a portable worker-count promise.

One child work unit is one dispatched verified WVB instruction after the spawn
baseline. Exhaustion is observed by the parent as
`Taskˉoutcome.Trapped(3011)`. Child-relative call depth counts the root as one;
excess is `Taskˉoutcome.Trapped(3004)`. Completed aggregates remain explicit
garbage-collection roots until await or scope teardown, preventing a retained
completion from being reclaimed while unrelated aggregate pressure runs.
Timer and diagnostic limits are validated but remain at zero use because the
first task opcode family creates neither resource.

The public one-module command recognizes the canonical hosted WVB 1.32 profile
and selects execution-request major `6`. A module without capabilities uses
minor `1`. Its fixed 72-byte little-endian header contains magic, version,
instruction/depth limits, module length, context generation, clock generation,
deadline, expected and admitted task-runtime generations, observation tick,
and observed task-runtime generation, followed by the exact module.

A module that declares the exact existing
`console.write_line(text) -> void` capability uses request and response minor
`2`. Its fixed 84-byte request header appends one capability-grant bit at offset
72, a standard-output byte limit at offset 76, and a required-zero reserved
word at offset 80 before the exact module. Grants above bit zero, an output
limit above 65,536 bytes, a nonzero reserved word, any capability count other
than one, or a different name, version, signature, or hosted profile is
rejected before execution. The ordinary command grants that single capability
with a 64-byte output ceiling. The minor-2 response appends bounded standard-
and diagnostic-output lengths and exact bytes after the common 20-byte result;
the runner validates both lengths before emitting each stream to its matching
host sink.

Both forms require WVB 1.32, exact request/module length, nonzero
context/clock/expected-runtime generations, and exact
`Main(Memoryˉbudget, Operationˉcontext) -> i32`; execution then creates both
launcher-owned values. Request major `5` remains reserved for `--source-file`
and cannot be reused as a task envelope.

The ordinary command supplies context/clock/runtime generation `1`, deadline
`u64::MAX`, and tick `0`. The explicit form is:

```text
wvrun --task-environment <module.wvb> <context-generation> <clock-generation> <deadline> <expected-runtime-generation> <admitted-runtime-generation> <observation-tick> <observed-runtime-generation>
```

Every value is canonical unsigned decimal. Context must fit nonzero `u32`;
clock and expected runtime generation must be nonzero; the remaining fields may
use the full `u64` range. Invalid arity or values return status `64` before
execution. Successful task execution preserves `Result: <i32>` on standard
output and process exit zero.

The current promoted runner contains 228 functions and 430,435 code bytes in a
482,767-byte WVB at SHA-256
`fc4724c7756f22eb52dd6ed4da9737a865e14ea4d52df1de69fc10236970ff4f`.
It packages to the exact Windows and Linux applications in the current-product
table above.
The 4,231-byte success fixture, the 5,057-byte environment fixture, child-trap
fixture, aggregate-retention pressure fixture, one-work-unit fixture,
call-depth-one fixture, retained-memory refusal fixture, four-child cancellation
fixture, 6,544-byte observable completion-order fixture, and 46-case task-state
self-test all pass their exact outcomes. The completion-order fixture writes
`3`, `1`, `0`, `2`, then `Result: 42`; its WVB SHA-256 is
`6b6eb29ae5b711358e582c42d2667ab21c0861ac1ca5b1bc70b3ab575711c80c`.
The complete focused owner passes all 61 named phases and 172 cases, including
the provider-generation recovery workload. GitHub Actions run `33232989584`
passes the exact three-case runner reconstruction owner independently on Linux
and Windows. GitHub Actions run `33235016333` separately proves the bounded
parallel-capable host policy on both systems. The final Slice 7 Qualification
gate remains pending.

The installed `wv run` composition invokes the same candidate through its
internal `--script <module.wvb> [argument ...]` mode only after an independent
complete-verifier pass. That mode uses `WVXI 4`, grants the four fixed scripting
base capabilities, replays the two bounded line-output buffers to their
separate outer sinks, and returns guest statuses from zero through 255 exactly.
It is an implementation boundary for `wv`; direct users should use the public
command contract in [`Windvale-Scripting.md`](Windvale-Scripting.md).

The three-case fixed owner proves exact candidate inventory, source-built
paired reconstruction, current-host result and instruction reporting, invalid
option rejection, malformed-module rejection, and input preservation. An
earlier candidate passed that Windows owner 3/3 in 49.8 seconds; the repinned
segmented candidate passes on Linux in 603 seconds and Windows in 1,260.210
seconds in run `33232989584`. The paired 185-case native Seed
front-door helper builds 105 exact artifacts and passes one uninterrupted
Windows run in 939.6 seconds plus one independent Linux 6.1 x86-64 run in
873.7 seconds over the identical tracked state. The helper owns the four Foundation
module builds and inspections, all four
Foundation demo builds, the native-stencil and selected runtime-service builds
and inspections, the complete output/file-output/file-input generator builds
and bridge inspections, the fixed-service/enum-metadata/publication/
service-bundle build and inspection closure, the complete runtime-table and
entry-metadata build/inspection closure, hosted metadata/startup/container/
runtime-header construction, publication-lifetime construction, source-lexer/
declaration-parser/body-parser core/demo/tool construction and core inspection,
source-set/source-graph/source-symbol core/demo/tool construction and core
inspection, source-bindings/typed-WVIR/source-WVB core/demo/tool construction
and core inspection,
WvDump/WVO-object/WVA-assembler/Wv-linker construction, independent
verification, and inspection,
WvDump self-test/valid/invalid execution, WVO inspector self-test, native
construction of the canonical WVO fixture, and its digest-bound verification
and inspection,
WVA/linker self-test, scanner, semantic rejection and preservation, provider
construction, canonical image/map publication, and undefined-import
preservation,
and native execution of the Machine Contracts, Byte Ordering, and Decimal
Parsing demos.
The 4 MiB Byte
Construction demo remains in the managed differential lane because the current
scalar runner returns bounded failure `3015` before completing it. The Stencil
demo also remains managed because its explicit 20,000,000-instruction policy
exceeds the runner's fixed ordinary budget.

The three source-parser demos remain in the managed differential lane. Direct
native probes do not produce the required result: declaration and body stop at
runtime code `3004`, and the lexer exits without `Result: 0`. The declaration
and body hosted tools also require console, diagnostic, file, and process
capabilities that this scalar profile does not bind. Decision 0516 therefore
transfers their construction and inspection without changing this runner's
execution contract.

The three source-semantic demos also remain in the managed differential lane.
Direct native probes stop with runtime code `3004`: source set after 13,098
instructions, source graph after 1,511, and source symbols after 1,430. Their
hosted tools require console, diagnostic, file, and process capabilities that
this scalar profile does not bind. Decision 0517 therefore transfers the nine
builds and three core inspections without changing this runner's execution
contract.

The final three source-compiler demos also remain in the managed differential
lane. Direct native probes stop with runtime code `3004`: source bindings after
791 instructions, typed WVIR after 767, and source WVB after 770. The bindings
and WVIR tools require console, diagnostic, file, and process capabilities that
this scalar profile does not bind. The WVB tool additionally owns the retained
fixture/differential/oracle sequence. Decision 0518 therefore transfers these
nine builds and three core inspections without changing the runner or oracle
contracts.

## Evidence boundary

Profile 5 intentionally omits enum-name and text-quote. Its startup request is
the only profile allowed to encode those two exact target positions as absent;
all other relocation targets and all other profiles remain nonzero.

The feature-frozen Stage 0 compiler remains a recovery and differential owner,
not the current product oracle. For this source closure it emits a distinct
126,271-byte WVB with SHA-256
`a2644f4bbe6209b033de7b1080113a8fcb4e5da3376d462d7d50c5edeb4a580c`,
which the current native semantic verifier rejects. The native Project front
door emits the compiler-aligned product pinned above. That expected divergence
does not weaken the exact native reconstruction contract.

This is current-Windows-host source-to-WVB and cross-target construction. It is
not independent Linux execution, a clean or previous-release bootstrap,
complete capability-bearing execution, per-function profiling, grouped
qualification, artifact promotion, or recovery deletion.
