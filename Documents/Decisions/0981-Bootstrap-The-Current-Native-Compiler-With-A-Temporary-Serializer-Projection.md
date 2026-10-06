# Decision 0981: Bootstrap the current native compiler with a temporary serializer projection

## Status

Accepted candidate bootstrap direction, 3 October 2026, under the maintainer's
coherent compiler, runtime and maintained-consumer memory change. The coordinator
is under implementation. Construction, same-input comparison and Windows/Debian
qualification have not been established by this decision.

## Problem and result

The current native compiler now uses the reserved byte builder to serialize its
function signatures and machine-code directory. Compiling that source produces
WVB 1.44, while the retained staging producer can execute only older compiler
bytecode. Building a successor through that producer therefore has a cycle.

Use one named, temporary source projection of the same current compiler to cross
that boundary. A projection is an exact copy with a small, checked adaptation.
Restore only the historical pure 76-byte serializer and its caller/entry plumbing
inside that copy. Preserve the current verifier, instruction selection, shared
storage machine emission, object writer and publication implementation exactly.
The resulting bootstrap executable lowers the true, unmodified successor source.
This introduces neither a permanent second compiler nor a new host compiler.

The scalar interpreter is a separate execution obligation. Its guest storage,
cell identities, entry services and hosting storage require integration beyond
recognizing thirteen new operations. Do not add those features solely to make
this bootstrap possible or treat this projection as interpreter completion.

## Exact adaptation and authority

The existing construction owner gains
[Bootstrap-Native-Compiler-Projection.mjs](../../Tools/Native/Bootstrap-Native-Compiler-Projection.mjs).
Its fixed selectors may alter only the following copied source regions:

- `Native-X64-Lowering-Core.wv`: serializer-only Foundation imports, the two
  reserved-directory constructor wrappers, transferred builder parameters and
  stores in the signature and directory writing helpers, their
  initialization/append/freeze regions, and the budget argument carried through
  the ordinary and bounded public lowering wrappers. The bounded wrapper's
  object-size selection and writer preflight remain unchanged.
- `Native-X64-Lowering-Layout-Writer.wv`: the serializer function and its
  now-unused Foundation imports. The pure limits and readers in
  `Native-X64-Lowering-Layout.wv` remain unchanged.
- `Native-X64-Lowering-Staging-Tool.wv`: the supplied-budget import, Main entry
  and call argument, restoring the older three-argument hosted staging entry.

The projected project omits the four canonical Foundation declarations that
become unused after these adaptations. Other source and project declarations
remain exact copies. The compiler's actual source never receives the fallback.

Retrieve only the Layout file at the selected recovery revision
`ed37c35c8cf328f7797241eaac39aaae6190b312` as the single historical input. Require
the selected old serializer identity; do not search ancestors, silently select
another implementation or restore whole Core or backend files from history.
Capture current Git HEAD separately and bind the actual current source bytes,
including working changes, in the snapshot. Every current adapted region has an
exact preimage selector. Changed or repeated regions refuse before construction.
The current serializer now lives in its focused Writer module. Its projected
historical body qualifies the one parameter-limit call through the unchanged
Layout import. This is the only adjustment to the historical function body.
Current Layout branches, including native7/8 slot layout, remain unchanged.

The old serializer and reserved serializer produce identical entries for
admitted parameters: two little-endian u32 offsets/lengths, one parameter-count
byte, one return-shape byte, two zero bytes and 64 parameter-shape bytes. The
parameter bound of 64 makes the reserved serializer's packed u32 field equal to
those four historical header bytes. This is successful-output equivalence;
the projection does not claim the successor's budget or allocation behavior.

## Snapshot, admission and construction

### Bounded nominal identities

The true compiler contains more than 64 record and enum declarations before its
first Result variant. The backend's 64 variant slots must count variants rather
than use positions in the complete type table. Current source maps variant
ordinals to those slots and resolves each slot through the admitted type table
for calls, views, aggregate storage and cleanup. WVB retains full declaration
indices; the 256-declaration and 64-variant bounds remain. This is a private
compiler representation change, without a WVB, WVO or ABI encoding change.
Retained executables keep their recorded restriction until rebuilt.

Keep the layout limits/readers independent of the allocating serializer. Their
existing focused test consumer can then run through the ordinary bootstrap
profile before compiler reconstruction. Its type cases include 116 records,
64 enums and 64 variants, round trips for every variant slot, borrowed views,
and rejection of a 65th variant, truncated tables and trailing input. The shared
source consumer also exercises record/variant aliases after 80 preceding record
declarations. These cases do not replace full successor execution and comparison.

The serializer constructs the two-field `Maximumˉexceeded` case. The same backend
therefore carries all declared case operands through constructor analysis,
scratch tracking and emission, including nested record backing. Its existing
64-cell bound rejects oversized scalar payloads and nested fields before unsigned
capacity subtraction. The focused owner checks operand order, emitted lengths,
wrong shapes, stack underflow and those bounds. Shared-source alias coverage
includes a case with both a nested record and a separate byte share.

Shape readers continue to accept isolated descriptors without a module header.
Function-shape admission uses its explicit minor version for budget-view identity;
table readers inspect a version only after checking the header extent and magic.
The same focused consumer covers short inputs, out-of-range Vector references,
and accepted and rejected borrowed-owner shapes through WVB 1.45.

### Bounded staging capacity bridge

The projected current compiler exceeds the retained staging tool's
1,024-function bound. Select 2,048 functions for this current-source candidate;
keep per-function analysis, frame, call-depth, data, code, relocation and output
bounds unchanged. This is a native execution-profile change, not a WVB, WVO or
ABI encoding change. Retained tools and their exact recovery artifacts keep
their recorded capacity.

Prepare one temporary capacity bridge from the exact staging source graph at
`b3e5b8e5721d126d60f3f6ce7c28a866e90f59ae`. In its private copied Layout file,
change only `functionˉlimit()` from `1024u32` to `2048u32`, requiring one exact
preimage. Compile that graph with explicitly prepared current compiler and
admission products. Independently verify its older-edition WVB and package it
with the unchanged pinned tools. This graph remains below their function limit;
no pinned source or executable is changed. Refuse unexpected source identity,
selector multiplicity, WVB edition or construction bounds.

Separately compile the current compact segmented image linker through those
same prepared compiler products and unchanged pins. Its current symbol reader
must share the 2,048-function bound; the old linker cannot be silently retained
for larger modules. Its ordinary symbol ceiling becomes 2,561, and its existing
shared profile becomes 2,566. The complete object envelope still admits at most
4,096 symbols, and the four-digit function-name encoding is unchanged.

The existing segmented checkpoint APIs package the projected compiler through
this explicitly admitted producer/linker pair. Bind the pair's complete source
closures, exact historical adaptation, current compiler/admission identities,
tools, commands, host, profile and WVB/native products into its record and cache
identity. Check those inputs before and after construction and reuse. Projection
preparation explicitly selects this record; its retained provenance includes
the pair and rejects missing or changed inputs. Prepared behavior may only read
already constructed products. The ordinary pinned construction route remains
available to its named bootstrap callers without an implicit override.

This bridge is a construction edge for the same backend, alongside the
serializer projection. It does not establish native self-lowering or waive the
successor comparison and retirement checkpoint below. Actual construction and
cross-host execution remain required evidence.

### Current-source projection

Create ordinary copied files beneath an explicitly selected canonical private
temporary parent outside the repository on either host. Prefer
`E:/Windvale/Work` for local Windows construction when available; CI can use its
host temporary directory. Preserve repository-relative
source, lock, profile and target paths. Keep the untouched current closure and
projected closure separately. Invoke the existing authenticated
`Run-Split-Compiler.mjs` with that explicit workspace and project, using one
already prepared current compiler/admission checkpoint. No compiler preparation
is hidden inside this operation.

The local `Projection.json` is the required bootstrap selector manifest. It owns
the selected revision and serializer identities, exact adaptation selectors,
current/projected source identities, tooling closure, compiler checkpoint,
products, host and current phase. It binds unfinished work as unfinished;
construction alone ends at `AwaitingSuccessorAndQualification`.

The `prepare` action creates the snapshot. `construct` builds the projected
stager, admits it with the existing complete WVB verifier, packages it through
the existing segmented construction APIs with the admitted capacity pair,
builds the true capability-free
native compiler source and stages that successor with the projected executable.
The selected true project is `Windvale-Native-X64-Lowering.wvproj`, whose entry
is `Main(Input: borrow bytes, Budget: Memory_budget) -> bytes`.

Package the projected stager under existing hosted profile 8. The current
1,618,565-byte serializer-consumer WVB exhausted profile 7's instruction bound
after publishing 47 object chunks. Profile 8 supplies the existing `2^38`
instruction allowance for compiler-scale products and accepts the stager's one
file input within its 32-input bound. Its declared arena and outer runtime
extent remain those of the
[hosted-container contract](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md).
The package cache binds the profile separately from the reusable native image.
This construction selection does not change the successor's owned-memory
limits or establish its behavior; both hosts still require execution evidence.

Require the projected WVB to use an admitted minor below 44. The complete reader
then rejects builder/Slice/mutable-builder shapes and operations 229 through 241
at parsed positions under that older minor. Literal bytes, names and integer
immediates are not searched for opcode values. Require the true successor WVB
to be version 1.44 or its accepted
[1.45 budget-helper amendment](0982-Complete-Direct-Budget-Helper-Authority-In-WVB-1.45.md)
and admit it independently before native lowering.

Each action requires an explicit absolute deadline at most two hours away.
Native packaging retains its smaller existing cold deadline. Bound source
closures to 64 modules per project and 4 MiB of source payload through the
existing authenticated reader; the copied union is at most 128 files and
16 MiB. WVB input is at most 16 MiB. Staged objects retain the existing 64 MiB,
518-chunk and 4 MiB-per-chunk bounds. Preserve partial products and the named
incomplete phase on failure; do not retry unchanged failures automatically.

## Successor and retirement checkpoint

The true successor initially uses the capability-free ABI-25 byte-result entry
from [Decision 0980](0980-Bind-Native-Shared-Values-To-Budgets-And-Tool-Entries.md).
Do not assume a hosted WVB-44 StagingTool with a budget parameter can launch.
Its actual caller must validate and copy the result while live, release it and
close the complete domain. That caller remains a required implementation and
qualification dependency.

The initial true byte-result bridge has a 4 MiB response limit, including its
32-byte private response header. Its raw WVO limit is therefore 4 MiB minus
32 bytes. Its adapter selects that ceiling in the existing lowering path; the
writer measures the complete WVO once and returns typed output-limit refusal
before joining code/helper or serializing an oversized object. The ordinary
complete-object and segmented-publication selections remain unchanged. The
adapter accepts the same already supported input WVB editions; this ceiling
belongs to the ABI-25 source entry. Report whether the
successor object fits it. The current true compiler's own object is approximately 55 MiB,
so the initial caller exercises representative WVB inputs whose objects fit the
limit. Building the true compiler through the projected StagingTool uses the
existing chunked publication path. Native-25 self-lowering and projection
comparison/retirement remain open until a retained-plan publication session or
an exact hosted-25 provider slice is integrated. Replanning the entire compiler
for every output piece is not selected. Never widen the byte-result limit
silently.

## Initial actual compiler host bridge

[Build-Shared-Compiler-Host.mjs](../../Tools/Native/Build-Shared-Compiler-Host.mjs)
constructs the candidate caller without preparing a compiler implicitly. It
requires the completed projection record and its identity, a separately built
current segmented image linker and its identity, a separately built independent
result-reader WVB and its identity, and an explicitly identified old-format WVB
for outer host metadata. The current complete WVB verifier runs in prepared-only
mode. The true compiler WVB is independently admitted as 1.44 or 1.45. The old carrier
supplies only the retained hosted container's service metadata; it does not
define the true compiler's source semantics or entry contract.

The existing hosted image packager supplies an outer ABI-22/context-7 I/O carrier.
[X64-Shared-Compiler-Host.wva](../../Linker/Startup/X64-Shared-Compiler-Host.wva)
uses its argument and file services while constructing a separate, fresh
ABI-25/context-11 domain. It never reinterprets the outer context. The source
compiler receives zero capability-provider fields and its one application
budget. Initial limits are a 4 MiB input, 4 MiB output copy, 16 MiB physical
arena, 16 MiB runtime maximum and 16 MiB application maximum. A 40 MiB root
maximum also funds the two mapped extents, each bounded to 4 MiB. Fixed metadata
and the output copy occupy an explicit prepaid region of the outer text arena.
The domain uses epoch 1 once in this process invocation.

The wrapper validates shared state through its existing runtime leaf, presents
the immutable live-row directory and result descriptor to the existing
[independent byte-result reader](../../Runtime/Windvale/Native-Byte-Result-Admission-Core.wv),
checks its unchanged descriptor, resolved pointer and epoch, and admits a
`WVNR 1` response while the result remains live. Its 32-byte header has eight
little-endian u32 words: magic, version 1, complete response bytes, lowering
status, native ABI, code bytes, WVO bytes and zero reserved word. Status 0 carries
the unchanged raw WVO payload. Statuses 1 through 6 are the existing summary's
finite refusals: invalid WVB, unsupported profile/module/function/code and
output limit. Refusals contain no payload, use ABI 22 and zero code/WVO counts.
No plan is recomputed to diagnose a refusal. The adapter constructs this header
with the existing immutable-byte operations funded by the named runtime budget,
and uses the hosted tool's existing normalize-if-nonempty metadata selection.
This private tool response changes neither WVB nor the runtime byte descriptor.

The wrapper validates the success WVO header and exact code/payload counts, then
copies only that payload. It releases the result, invokes the
true module's exported close operation and checks physical, accounting and
shared closure before writing the output file and emitting
`native x64 status=Valid abi=N code-bytes=N object-bytes=N`. A typed refusal
instead emits the same finite status-name report on the diagnostic channel,
exits 1 and makes no output-file call. Malformed response bytes, terminal traps,
independent-admission failures, cleanup failures and host I/O failures retain
distinct host failure outcomes; an arbitrary nonzero exit never proves semantic
refusal. Failure after initialization tears down this same owned domain.
The small result-reader adapter uses an ordinary hosted `Main() -> i32` entry
under the older ABI. During that call, the wrapper temporarily binds its
`file.read_bytes` and `file.write_bytes` slots to private memory providers.
They accept only the exact `request` and `response` names, one read followed by
one write, and bounded request/response extents. No operating-system file is
opened by these providers. The write copies the response into wrapper-owned
metadata before the reader's function-return reclamation. The wrapper restores
the outer service table and disables both providers before checking the reader's
status or response. Other outer-context services remain separate from the
ABI-25 domain.

The current segmented admission admits one exact ABI-25 symbol tail after Main:
blob, directory, close, budgeted-storage import and shared-storage import. It
checks data-directory rows, kind-4 alignment, zero padding, text encoding and
the close alias's 32-byte offset and 17-byte extent. The selected shared path
also proves the canonical shared-value helper and byte-result entry template,
including both exact provider-relocation fields. The whole entry prefix must
fit its containing publication chunk, as in current whole-function publication;
scalar ABI-25 entries are outside this initial tool profile. The previous final-Main
symbol layout remains the older path. Ordinary segmented APIs refuse unresolved
runtime imports; selected APIs require the following private bootstrap packets.
These are construction records, not new WVB or runtime value encodings. All
words are little-endian u32.

| Packet | Fields in byte order |
| --- | --- |
| `WVSP`, 40 bytes | Magic; version 1; size 40; aligned companion position; companion byte count; driver entry; budgeted-storage address; shared-storage address; two zero words. |
| `WVSC`, 64 bytes | Magic; version 1; size 64; true-module image bytes; Main offset/bytes; blob offset/bytes; directory offset/bytes; close offset/bytes; companion position/bytes; driver entry; zero word. |

Companion placement is the true module's image size rounded up to 16 bytes.
The companion is at most 4 MiB and the complete image at most 64 MiB. The three
entry/provider addresses are distinct and lie inside that companion. An inspect
configuration has zero companion fields and cannot launch. The independently
reconstructing linker applies only the two exact runtime imports. It requires
one complete bounded read-only publication region and validates its directory
against the canonical local data symbols.

The ordinary Windvale assembler and linker build the host wrapper, six existing
runtime leaves and the result-reader adapter as a small companion unit. The
first link determines its export addresses. The admitted compiler metadata and
provider packet then generate the final immutable configuration. A second link
must have identical layout and identical bytes outside that exact 64-byte
configuration. Host JavaScript neither emits compiler machine instructions nor
applies native relocations. Existing image transport and hosted packaging remain
the owners of canonical chunk transport and Windows/Linux container production.

The coordinator snapshots bounded source inputs in that explicit private
workspace, preserves partial products and records incomplete phases on failure.
It reports `Produced` with `qualified=false` after packaging. Running bounded
consumers, same-output comparison and real Windows/Debian lifetime qualification
remain separate required evidence; packaging alone establishes none of them.

`compare-retire` consumes a caller-selected true successor driver and its exact
identity, the exact successor WVB identity, and an external qualification record
with its identity. The driver's two arguments are an input WVB path and output
WVO path. Execute that real successor on the same WVB used by the projected
stager and require complete object-byte equality. The external driver owns ABI,
result-lifetime and domain-closure proof; merely naming it does not prove those
properties.

Write a comparison/retirement record outside the snapshot before removing the
temporary workspace. The coordinator identifies the supplied qualification
record without interpreting or upgrading its claims. The existing Windows and
real Debian owners must establish qualification before selecting retirement.
Delete only the verified, uniquely named workspace; preserve the surviving
record and external successor. Do not publish the projection as a maintained
compiler, reuse it as a fallback after retirement or report this bootstrap as
general Language 1.0 memory completion.

The forward executable contract remains
[Decision 0979](0979-Connect-Reserved-Byte-Construction-To-WVB-1.44.md). The temporary
adapter closes a construction dependency while that same compiler's true source
continues to use reserved storage.
