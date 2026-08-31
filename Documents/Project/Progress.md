# Windvale progress

> Status snapshot: 31 August 2026

<a href="Images/Windvale-Roadmap-August-2026.svg"><img src="Images/Windvale-Roadmap-August-2026.svg" alt="Dated August 2026 Windvale roadmap phase map" width="100%"></a>

This is the current-state dashboard. It records measured standing and immediate
work, not the chronological path used to reach it. The
[roadmap](Roadmap.md) owns forward product gates,
[qualification evidence](Seed-Verification-Evidence.md) owns exact historical
runs and identities, and accepted decisions own rationale.

The image is a dated editorial snapshot. Update the Markdown when current state
changes; refresh the image only when it becomes materially misleading.

The signed `v0.1.0` preview and its package/application/offline-lifecycle
foundations remain complete evidence. [Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md)
now targets Windvale 1.0 directly; no `v0.2.0` product release is planned.
Language, Libraries, WVDB, package/service, and integrated qualification work
advance as product workstreams rather than numbered release stages. OS-1 retains
its independent qualification path.

The latest Language 1.0 Slice 8 candidate implements the production ingress
designed by
[Decision 0893](../Decisions/0893-Authenticate-Production-Source-Analysis-Ingress.md).
Target-aware `wvadmit` publishes WVSS, WVTD, WVFC, and WVAE; independent
six-snapshot `wvauth` publishes no certificate; and the private runner rechecks
the retained relationship before selecting the next compiler phase. For a
nonempty catalog it invokes the non-authoritative private `wvbind`, requires
exact digest evidence for the retained WVSS, WVTD, and WVFC, rechecks all six
snapshots again, and stops at exact `Foreignˉloweringˉpending` without launching
the Analyzer or emitter. For an empty catalog it skips `wvbind`, byte-compares
the Analyzer's republished WVSS, gives the emitter the retained original, and
alone atomically publishes the final WVB. The front door therefore owns five
bounded products overall, although each catalog route launches only four. Raw
Project 2 remains development-only and rejects System, platform, and foreign
source before analysis. The 21-case production-ingress owner and dedicated
24-case authenticated-binding owner, together with the callable owner's exact
Foreign-capture rejection, cover their separate hosted and semantic boundaries;
the refreshed product identities and complete local reruns pass on Windows.
Corresponding Linux evidence, foreign lowering, and runtime containment remain
pending. The
shared 482-case Language 1.0 front door passes on Windows and Linux, and its
corrected owner identity passes the four-case registry stream on both hosts.
Decision 0893 and the private-binding
[Decision 0895](../Decisions/0895-Bind-Authenticated-Foreign-Declarations-In-A-Private-Compiler-Phase.md)
therefore remain Proposed.

The coordinator's target-descriptor evolution directly enters the validator
closure. A narrow retained-analyzer audit reproduces its 191,712-byte WVIR at
SHA-256
`067b8bf2761b28189761f226b81cfd743b11ce1edc6a247fdf17a62edc1c3391`
twice with exact WVCA bindings. Validator WVB construction now requires the
current reconstructed compiler. Two builds produce the same verifier-accepted
72,600-byte WVB at SHA-256
`706e34eb031be9c6d4695fe15cc7215d507e8768d44b5f5409813b58f6e46a59`;
two builds also reproduce the verifier-accepted 94,839-byte portable fixture at
SHA-256
`5bc71d4c549da5a22a8210cd15fb01bfe19941d26db3cbb9f4b46be050b0ac7b`.
The final focused owner also pins the 24,292-byte core WVB at SHA-256
`5b76731abff311ff51dd2e302da8da7bfe8439250d5f32647bda5f0ee51f9537`,
packages the current validator, and reports exactly `PASS admission evidence
cases=56 portable-selectors=44 dynamic-cases=12 wvss-structure-subcases=15
execution=native-packaged core-wvb-bytes=24292 wvir-bytes=191712
wvb-bytes=72600`. The focused suite completes in 137,850 ms and its owner
wrapper in 138,530 ms. This is current local Windows execution evidence; the
Linux, paired-host, production-integration, and broader Slice 8 work above
remains pending.

## Indicators

| Indicator | Meaning |
| :---: | --- |
| ✅ Qualified | The named finite gate has reproducible evidence. |
| 🔵 Ongoing | Useful qualified slices exist; direct consumers may require more. |
| 🎯 Current | This is an immediate product or workflow boundary. |
| 🚧 Candidate | Focused implementation evidence exists, but the named gate is not qualified. |
| ○ Proposed | Direction exists without implementation evidence. |

## Current dashboard

| Boundary | Standing | Evidence today | Immediate next result |
| --- | :---: | --- | --- |
| Seed through assembler and linker | ✅ | Source semantics, WVB, verification, runtime, object, assembly, and link foundations have Windows/Linux evidence. | Preserve their contracts as later products consume them. |
| Windvale Language 1.0 | 🚧 | [Decision 0767](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md) freezes the replacement source design, and [migration evidence](Windvale-Language-1.0-Migration-Evidence.md) binds its exact fixtures. Slices 2 through 6 connect values/control, typed failure, generics/collections, ownership/borrowing, elastic memory budgets, one rights-limited hosted source resource, and the selected callable/closure profile. Slice 7 executes bounded queued WVB 1.32 task scopes with consuming handles, exact completion-slot and retained-memory admission, and a cooperative runtime oracle for context, clock, deadline, cancellation, and task-runtime generations under [Decisions 0861](../Decisions/0861-Execute-Structured-Tasks-As-Wvb-1.32.md), [0864](../Decisions/0864-Reserve-Structured-Task-Completion-Slots-Before-Spawn.md), [0865](../Decisions/0865-Reserve-Structured-Task-Retained-Memory-Before-Spawn.md), [0866](../Decisions/0866-Observe-Structured-Task-Runtime-Environment.md), [0867](../Decisions/0867-Inject-Structured-Task-Environment-Through-Request-Major-6.md), and [0868](../Decisions/0868-Queue-Structured-Task-Children-Before-Await.md). Four sibling handles now coexist before execution; the focused four-child cancellation case and six prior task cases return `42`. The complete focused owner passes all 160 cases locally; exact runner reconstruction and paired-host integration remain pending. [Decision 0810](../Decisions/0810-Use-The-Split-Compiler-For-Compiler-Scale-Development.md) keeps immutable Seed recovery provenance while the evolving analyzer/emitter becomes the Windvale 1.0 compiler. | Add child-provider generation and an externally observable completion-order workload, then prove parallel-capable Windows/Linux behavior, reconstruct and promote the candidate, and run the explicit final Slice 7 qualification gate. |
| Windvale-written compiler | ✅ | The committed twelve-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian. | Implement only a frozen edition-1 vertical slice with its named corpus and target evidence; do not widen Stage 0. |
| Shared accepted-subset native backend | 🔵 | Interpreter, deterministic AOT, baseline JIT, hosted execution, and native object/link/container profiles are qualified for their documented subsets. | Broaden only where the package application, tools, or OS require an unsupported operation or ownership shape. |
| Native-only repository | ✅ | [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md) closed all eight retirement conditions; [Decision 0558](../Decisions/0558-Archive-Managed-Stage0-Outside-Main.md) removes all tracked managed source and direct managed entry points from `main`; final [Qualification run 31889107326](https://github.com/eworker-inc/Windvale/actions/runs/31889107326) passed on the post-archive `v0.1.0` state. | Preserve the qualified boundary; create a separate baseline tag only if it adds value beyond the exact signed product tag. |
| Development verification | ✅ | Milestone 1 is complete under [Decision 0560](../Decisions/0560-Linked-Image-Development-Checkpoints.md): front-door feedback takes 13,900 ms, the pinned WebAssembly engine path takes 29,674 ms, and the all-hit database owner fell from 402,638 ms to 87,800 ms with all eight behaviors. Exact warm [GitHub run 31852544894, attempt 2](https://github.com/eworker-inc/Windvale/actions/runs/31852544894/attempts/2) passes end to end in 1m42s on Windows and 1m15s on Linux; three owner closures and five checkpoint families are machine-checked. | Preserve the bounded path and add another checkpoint or owner only from new measured product pressure. |
| Native qualification | ✅ | [Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md) qualified 52 suites and 3,287 cases per host in four shards; the complete workflow took about 15 minutes. | Run this gate only for a selected release, promotion, bootstrap, security, ABI, or conformance state. |
| Package-backed application | ✅ | Milestone 2 is complete under [Decision 0561](../Decisions/0561-First-Admitted-Bundle-Store-And-Rights-Reduced-Wvdb-Query.md). Paired [Bundle 1/store run 31872089188](https://github.com/eworker-inc/Windvale/actions/runs/31872089188) and [capability run 31872429140](https://github.com/eworker-inc/Windvale/actions/runs/31872429140) retain the exact WVB, bundle, package, application, capability, success, denial, and offline-rebuild evidence. | Preserve these results while their declared inputs remain unchanged; do not reopen the completed 0.1 release gate. |
| Development/stable installers | ✅ | [Decision 0749](../Decisions/0749-Compress-Successor-Installer-Archives.md) records the exact historical compressed `0.2.0-dev.1` Windows/Linux candidates. [Decision 0565](../Decisions/0565-First-Stable-Preview-Installers.md) preserves the separate `0.1.0` archives published in the signed [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0); Decision 0562 records the historical `0.1.0-dev.1` identities. | Obtain paired-host evidence for the compressed candidates, then select new 1.0 development identities explicitly rather than renaming published or pinned assets. |
| Offline package lifecycle | ✅ | The historical offline gate is complete. Two-package admission, activation/recovery, verified dispatch, rollback, and recoverable data-preserving uninstall pass on both hosts in run `31906316540`. | Preserve this as the offline publication and activation foundation for Windvale 1.0. |
| Windvale 1.0 host product | 🎯 | [Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md) selects one integrated `v1.0.0` gate across the frozen Language 1.0 implementation, required Libraries 1.0 profiles, WVDB 1.0, packages/services, support, and cross-host qualification. | Reconcile each workstream with the [product plan](Windvale-1.0-Product-Plan.md) and close exact contract, implementation, usefulness, safety, compatibility, qualification, and distribution gaps. |
| WVDB 1.0 | 🚧 | [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md) replaces external rewrite/parity framing. Decisions 0791–0798 accept the upper-layer identity, table, relationship, index/query/transaction, profile/storage, type/size, document/graph, and backup/restore directions. Existing storage and service candidates remain implementation evidence, not silent 1.0 semantics. | Continue normative specifications through storage, durability, service, operations, and conformance, then reconcile implementation slices against them. |
| Windvale OS | 🔵 | OS-1 owns the qualified three-environment WVB portability gate plus bounded endpoint, peer-loss, teardown, and contained-failure foundations. Probe 40 qualifies protected processes, capability IPC, bounded preemption, and generation-safe non-tail memory objects; operation 8 now snapshots, admits, and publishes the already constructed generation-1 child as reference `65538`. Filesystem slices execute portable no-link operations and exact mutation completion, validate service traffic and provider ownership, use real Windows/Linux read-only host leaves, and compose strict FAT32 volume, chain, directory, block-exchange, immutable block-image, partial-sector, and validated shared-read response boundaries. Endpoint transfer profiles admit exact 4,096-byte control, 4,144-byte block, and 65,600-byte filesystem windows with checked identities, mappings, non-overlap, peer loss, and at most 17 pages. Provider launch transaction 1 requires released/closed reusable machine slots: filesystem selects process generation 3 (`196610`) and endpoint generation 2 (`131072`), followed by network process generation 4 (`262146`) and endpoint generation 2 (`131073`). The generation-three filesystem record, W^X paging, image-copy, and native-context bytes are source-owned and covered by a dedicated three-case cross-host packaging owner. Probe 40 now releases the terminal generation-two client, first-fits its root as generation 3, allocates all 85 pages, binds the exact admitted launch-request digest, exactly reuses the terminal directory-endpoint slot for a durable `1/81/1` filesystem domain ledger, advances endpoint slot 0 to provider-only generation 2, and publishes a fresh ready thread and process. Normal plus invalid-opcode and general-protection QEMU paths pass in deterministic 1,698,816-byte images. The endpoint has client reference 0, the new thread is not dispatched, FAT32 media is not bound, and no provider request runs. Network slices 1–2 retain shared bounded-operation and binary address/authority candidates covering reserved capacity, exact progress, deadlines, cancellation, provider loss, scoped addresses, port/direction bounds, resource ceilings, and rights reduction. | Bind one surviving consumer, bind FAT32 media identity, enter the ready filesystem thread, and run one bounded guest read with complete failure rollback and teardown before adding the sequential network generation. Do not claim arbitrary application launch or a live provider until those transitions execute. |
| WebAssembly playground | 🔵 | The static Monaco playground and complete source/WVB-to-Wasm verification route run without .NET. A separate pinned-package engine checkpoint proves compile/verify/execute, capability denial/grant, and bounded output without rebuilding WVB/Wasm. | Keep complete construction cold for qualification; improve package size or browser coverage only when product measurements justify it. |
| Windvale Shell | 🚧 | [Decision 0602](../Decisions/0602-Shell-1-Parser-Contract-And-First-Portable-Core.md) accepts Shell 1; its parser has paired native and bounded browser evidence. `echo` has an active-generation resolution/launch proof, while [Decision 0713](../Decisions/0713-Hosted-Standard-Byte-Output-And-File-Read.md) adds the exact-byte provider and paired hosted `file-read` target for the fixed `cat` alias. This is not yet an interactive or in-OS shell. | Package and resolve `file-read`, then replace the Workbench JavaScript `cat` branch with the verified WVB worker path while retaining the complete 47-case browser corpus as a promotion gate. |
| Public project and release foundation | ✅ | Licensing, contribution, security, governance, authorship, public repository policies, the product threat model, exact WVDB approval/launch records, and Release Envelope 1 creator/verifier are published with the signed [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0). | Preserve the published evidence and define the 1.0 stability, support, migration, and release policies without reopening 0.1.0. |

The latest OS machine slice extends the table's checkpoint through byte 29,474
and 334 relocation fields by validating/publishing the directory endpoint,
scrubbing the completed request channel, advancing bounded lifecycle state, and
delivering the exact 3,096-byte result to client generation 2.

The following completion-cleanup adaptation now extends that checkpoint through
byte 30,825 and 395 relocation fields. It changes only retained and selected
client generations to 2 while preserving all sixty-one fail-closed cleanup
branches. Reclamation preflight and live QEMU execution remain pending.

Completion finalization now extends the checkpoint through byte 31,199 and 405
relocation fields by validating operation 6, closing and scrubbing the channel,
advancing its generation to 2, and resuming the selected client. Provider
shutdown, reclamation, and live QEMU execution remain pending.

The final-state epilogue completes source ownership of all 33,826 retained
process-machine bytes and 569 relocation fields. The retained-object integration
path now builds the 1,691,136-byte normal EFI and completes pinned-QEMU execution
of the fixed embedded application. Direct image construction from the emitted
replacement and a live public start syscall remain separate pending cutovers.

The architecture-neutral application-start boundary copies the exact 64-byte
`WVSR 1` request from an admitted byte window into an immutable value, uses
subtraction-first range checks, and rejects caller impersonation before
decoding. The native x86-64 copy leaf checks one exact page, copies eight bounded
qwords into kernel-owned memory, validates every version-1 field against an
independently supplied caller, and erases rejected snapshots. Its ten new
native cases advanced the application-launch owner to 52. The next native leaf
now binds a separately supplied current process id/generation and page, derives
reference `65537`, and preserves bounded rejection/erasure through nine more
cases, bringing the owner to 61. Loading those values from retained `GS` state,
fault containment, syscall-budget accounting, and public construction/
publication remain pending; these slices do not yet make arbitrary applications
launchable.

Filesystem slice 4 now has four implemented format boundaries. The strict FAT32
volume policy checks a 512-byte boot sector against a separately admitted
70,000-sector device extent, derives 68,890 data clusters, proves both FATs can
address them, and selects the mirrored or active FAT. The chain policy locates
four-byte entries, masks their reserved high nibble, classifies special values,
and rejects cycles, truncation, trailing entries, and work beyond 4,096
clusters. The original 45 volume-and-chain cases remain pinned. The
block-read transaction bounds
the granted device extent, read right, generation, sequence, eight-sector
ceiling, and exact completion bytes. Its 59-case owner composes the exact
`WVBR 1`/`WVBP 1` provider messages, a capacity-one exchange lifecycle, and a
64 MiB-bounded immutable block-image provider. Construction, dispatch,
exact-once completion, pre/post-dispatch cancellation, outside-image rejection,
peer loss, and teardown are bounded; the privileged endpoint syscall adapter,
hardware block driver, and live guest file reads remain pending. Short-directory admission now
scans at most 4,096 exact entries, rejects inconsistent attributes and cluster
fields, distinguishes files and directories, and detects duplicate or
truncated targets. File-read plan 1 maps an admitted `u64` offset and the exact
resolved cluster into a maximum eight-sector block window without crossing the
file or cluster tail. The combined volume/chain/directory/file-plan owner now
has 80 cases. File-read transaction 1 now binds an authorized file reference
and media generation to each exact dispatched exchange, resolves successive
chain ordinals, copies partial-sector bytes, and produces a validator-accepted
shared reply. Its 18 cases include a 4,500-byte read across two clusters and
exchanges. The filesystem launch profile now reserves 48 RX image pages, 17
RW/NX context/transfer pages, and a disjoint 16-page RW/NX native stack. Its
1,024-byte prefix and complete 65,600-byte response fit the admitted domain;
the unsafe 65-page request is rejected. Privileged
endpoint and driver execution remain open. VFAT
long-name/Unicode mapping remains explicit future work.

## .NET retirement result

`████████████████████  8/8 conditions qualified — complete`

| Evidence counter at the retirement release | Result |
| --- | ---: |
| Normal .NET entry points | **0** |
| Explicit recovery-only .NET entry points | **9** |
| Native Windows/Linux qualification jobs | **6/6 passed** |
| Native retirement suites | **45/45 per host** |
| Fixed native cases | **3,206/3,206 per host** |
| Exact selected-release recovery | **2/2 hosts** |
| Published and independently retained recovery assets | **13/13** |

Current work uses a separate 125-owner, 5,936-case native verification registry.
Its 22,366 LF-only bytes have SHA-256
`38c79e6b95e9878e69fd4a0f002bbdf446d605ba0bd9e6a14d7159bab239bde6`.
That evolving coverage is not part of the frozen retirement counter and does
not reopen the completed dependency gate.

## Qualified foundations and active work

### Milestone 2 complete: package-backed host application

The completed product slice is WVDB Query from
[Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md).
The completed manifest/lock closure and offline locked rebuild remain credited.
The completed package path contains:

1. one deterministic admitted bundle;
2. one content-addressed local publication path;
3. one complete capability closure and approval record;
4. one successful rights-reduced hosted execution;
5. one denied or unsupported execution with no ambient fallback; and
6. matching canonical WVB/package identities on Windows and Linux.

All six items have local and paired-host evidence. Bundle 1/store run 31872089188
passes on Windows and Linux at commit `d9795e0e15944b3342ea7c4a42105eee38420708`;
WVDB capability run 31872429140 passes on both hosts at commit
`204e8082fdaabbc7333ac40ed6ca7ff8564de123`. Milestone 2 is closed and no new
feature belongs in it.

SQL, a registry, a network resolver, broad concurrency, and a database server
are not required for this slice.

### Milestone 3 complete: Windvale 0.1 preview

The signed [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0)
was published on 15 August 2026 from exact commit
`c1d350949207c7ee6f82ed2c399b748e188bf949`. Its Windows and Linux installers,
source archive, WVDB Query bundle and approval records, qualification reports,
provenance, root policy, signed manifest, public keys, offline verifier, and
complete release envelope are retained as immutable release assets. Final
[Qualification run 31889107326](https://github.com/eworker-inc/Windvale/actions/runs/31889107326)
passed the selected commit on both hosts, and the signed tag binds manifest
digest `2e28f45c668be869b17cc5547ee0865a0365417f442ecc49c0e481c696b6d85a`.

WVDB Query remains a separate example/application package; database servers and
other applications remain separate packages or projects. Milestone 3 is closed.

OS-1 is not a prerequisite for `v0.1.0`.

### Milestone 4 complete: offline package lifecycle

[Decision 0590](../Decisions/0590-Offline-Package-Lifecycle-And-Generation-Activation-1.md)
selects the offline generation-and-rollback lifecycle without assigning a
`v0.2.0` release. Generation 1 and Activation 1 now have a portable specification,
Windvale-native bounded readers, cross-record package validation, and pure
activation/rollback planning. The first host publisher durably writes and
rereads a private Activation 1 sibling, atomically replaces the public record,
recovers a verified interrupted candidate, rejects stale state, and publishes a
rollback transition. The WVB inspector adds the second exact source package and
lock with a distinct whole-file-read capability closure. One signed offline
stage admits both packages; active-generation dispatch executes both exact
commands; the composed lifecycle updates and rolls back; and Decision 0580 adds
state-first, recoverable uninstall while preserving application data and
unrelated root entries. Exact implementation commit
`df2d15dad0434182b74ad7ae357b4596d4aef82d` passed the 27-case lifecycle and
13-case uninstall owners on Windows and Linux in GitHub run `31906316540`.
Milestone 4 is closed without a new product release. Networking, signed
revocation/minimum-version policy, OS-1, and database breadth stay independent.

### Active target: Windvale 1.0

[Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md) supersedes
the future `v0.2.0` product target in Decision 0595. The active
[product plan](Windvale-1.0-Product-Plan.md) requires the frozen Language 1.0
implementation, selected Libraries 1.0 profiles, WVDB 1.0, installed package and
service operation, support policy, and exact Windows/Linux qualification before
`v1.0.0` can be published.

WVDB is a Windvale-owned database under Decision 0790; an external implementation
may inform comparative research but is not a parity oracle. The external-model
gateway and a complete Windvale OS are independent consumers or workstreams, not
automatic host-product 1.0 blockers.

### Independent promotion checkpoint: native-only repository baseline

The archival decision is implemented in the working repository: the immutable
pre-removal release remains authoritative, the in-tree pointer preserves exact
checksums and recovery custody, and guards require zero tracked managed files and
zero direct managed entry points. The `v0.1.0` selected state passed the explicit
complete Windows/Linux qualification. A separate native-only checkpoint tag is
now optional naming, not an open qualification gate; ordinary per-commit full
qualification remains disabled.

### Parallel track OS-1: launch and service composition

Preserve the already-qualified three-environment WVB, endpoint, bounded queue,
peer-loss, teardown, contained-failure, and memory-object evidence. The current
candidate additionally gates token 97 on one fixed filesystem/operation/network
version-and-limit envelope. The open composition mechanisms are a dynamic
resource-domain owner beyond the fixed Probe 40 accounting gate, a checked
user-buffer/start-syscall interface beyond the two versioned fixed launch
requests, arbitrary admitted-image construction, real filesystem/network
provider processes serving live clients, and generic structured supervision
with a bounded restart or deliberate terminal result.

### Slice 8 authenticated source-admission coordinator candidate

[Decision 0892](../Decisions/0892-Coordinate-Authenticated-Source-Admission.md)
composes locked source-profile admission, exact target admission, canonical
foreign-catalog production, independent format/count validation, exact foreign
predicate enforcement, and WVAE construction into one portable all-or-nothing
result. Success publishes separate WVSS, unchanged WVTD, WVFC, and WVAE values;
failure returns all four empty with bounded subordinate diagnostics.

The producer owner uses the current forward-language build route. Its
518,755-byte WVB at SHA-256
`e930239f2b02f840fbfe92fc64cfa8409aeb38955a52dd16d1a73a8f62062db7`
and 12,891,136-byte Windows application at SHA-256
`35ffb6d0d1b928f43b2d7117eb2092905a40dac045ae84ebd676081d6f0ecbf0`
passed complete verification and all 25 isolated selectors.

The coordinator's identical builds publish a 634,819-byte WVB at SHA-256
`7d004263b350097f8bcd82997d4210464ee2dba8937a5c0b76d32b8139f99ac1`.
Its 14,952,960-byte Windows application at SHA-256
`e466cbb1f89452cc894eeba1ef7406aa2b204dc8b8c2ef66ddc7b750df3b6f16`
passed all 28 isolated selectors. The owner includes exact source-ordered WVFC
readback, rejects structure-valid empty and remapped catalogs, preserves
`UNKNOWN_PLATFORM`, rejects generic and broader-than-exact foreign scopes,
accepts exact-only, and checks max/one-past retention plus
malformed-within-format-ceiling diagnostics. The planner passed 31 general and
244 native routing cases. This completes focused current-Windows development
evidence for the candidate.

The registry has 125 owners and 5,936 cases in 22,366 LF-only bytes at SHA-256
`38c79e6b95e9878e69fd4a0f002bbdf446d605ba0bd9e6a14d7159bab239bde6`.
Its shards contain 1/57, 45/2,847, 38/1,783, and 41/1,249 owners/cases. The
subsequent Decision 0893 candidate implements hosted publication and the
Analyzer handoff. Its exact local product pins and complete owner result pass;
the planner covers its transitive product dependencies and passes 31 general
plus 254 native routing cases. Commit `14fd50ea` passed the 482-case front door
on both hosts in run `33339426829`; commit `a32c9e4c` then passed the corrected
four-case verification-owner stream on both hosts and the final gate in run
`33340292739`. The corresponding Linux production-ingress owner result remains
pending beside foreign lowering and runtime containment.

The pinned bootstrap transition produces 4,182,928 WVIR bytes for the selected
Analyzer closure, leaving 11,376 bytes, and could not carry duplicated ingress
authentication. One packaged current Analyzer has separately compiled its
exact 2,132,771-byte source set to 3,815,704 WVIR bytes, leaving 378,600 bytes.
The 18-phase current-compiler convergence check reproduces stage-2 bytes and
pins the 1,552,090-byte WVB at SHA-256
`5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77`.

The promoted two-file bootstrap then passed its rewritten 16-phase convergence
path locally on Windows x64 with an isolated empty cache. The current analyzer
settled at 1,573,433 bytes and SHA-256
`23d9ec0c223d214a69fcb4179abec5b3b9a6d579d8557f3ccf4248c2904267b6`;
the current emitter settled at 1,575,647 bytes and SHA-256
`0972defc2debdad47cd36268516c15d947a364b93aede84f0b55cf17ad061d77`.
Both Stage 2 products matched Stage 1 byte for byte and passed the independently
rebuilt compiler-aligned verifier. The analyzer retained 320,920 bytes beneath
the 4 MiB WVIR ceiling; the emitter retained 97,520 bytes. Linux execution of
the shortened bootstrap path remains pending before a cross-host claim.

### Preserve bounded development feedback

- Use `Tools/Verify/Verify-Changed.ps1` once after a coherent edit.
- Treat a passing affected-owner result as reusable until that owner's declared
  inputs change.
- Use development checkpoints only for immutable generated products; always
  rerun the behavior being changed.
- Keep complete qualification cold, explicit, and bound to one selected source
  state.
- Promote related compiler and tool identities in coherent batches rather than
  repinning the entire product family on every implementation commit.

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows and Linux.
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image.
- ✅ Canonical WVB → shared accepted-subset lowering → native WVO/AOT → Windows PE or Linux ELF.
- ✅ Native compiler seed → Stage 1 → byte-identical Stage 2 without loading .NET.
- ✅ Hosted console application → explicit capability metadata → deterministic Windows/Linux application.
- ✅ Portable Windvale modules → bounded WebAssembly compiler/interpreter → static browser worker.
- ✅ Canonical WVB → admitted guest resource → protected Windvale OS execution for the qualified examples.
- ✅ Package manifest and lock → admitted immutable bundle/store → rights-limited WVDB Query execution on Windows and Linux.
- 🚧 Probe 40 fixed ResourceDomain1 + versioned executable-publication/machine admission → checked start syscall and dynamic allocation → generation-2 reconstruction, result-55 return, and operation-7 reply publication → supervised live isolated provider.
- ✅ Qualified product artifacts → signed release envelope → offline-verifiable Windvale 0.1 preview.

## Reading the evidence

- [Development roadmap](Roadmap.md) defines the active milestones and gates.
- [.NET retirement inventory](Dotnet-Retirement-Inventory.md) records the current
  native-only boundary and immutable managed recovery identity.
- [Qualification evidence](Seed-Verification-Evidence.md) preserves exact runs,
  reports, and artifact identities.
- [Changelog](../../CHANGELOG.md) summarizes notable implementation changes.
- Dated decisions preserve the detailed historical sequence without making that
  sequence the current plan.
