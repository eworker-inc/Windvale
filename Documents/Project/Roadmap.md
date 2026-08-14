# Windvale development roadmap

## Active goal

Evolve Windvale from the qualified C# Stage 0 and portable bytecode foundation into a small, understandable, self-hosted computing stack whose normal Windows, Linux, and Windvale OS workflows require no .NET dependency. Build useful Windvale-written binary tools and an explicit Foundation library first; then grow the language, compiler, assembler, object model, linker, shared JIT/AOT native backend, runtime, memory system, and reproducible bootstrap; finally boot a minimal virtual-machine operating system that can load and run the same verified Windvale modules used on native Windows and Linux hosts.

Individual application and library parts may be shared, platform-subset, or OS-specific. Portability remains a positive contract and an important cross-host proof, but it is derived from the complete dependency graph rather than imposed on every imported part. [Decision 0140](../Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) owns that direction and the first filesystem capability family.

The destination is stable, but the route is not frozen. An intermediate design may be revised or replaced when implementation evidence shows that it is impractical or that a materially better alternative is available. Consequential changes require an updated specification or an accepted decision, preserved verification evidence, and a clear migration of current fixtures. Adaptability must not weaken deterministic semantics, mandatory verification, explicit platform boundaries, or the end-to-end portability proof.

## Status

This roadmap owns the forward phase gates, sequencing, and next deliverables. It may state whether a gate is open or complete, but it is not the project's activity diary; the [Progress dashboard](Progress.md) owns the current implementation snapshot and immediate measured transfer. Update this roadmap when evidence changes the route, gate, or intended order.

[Decision 0523](../Decisions/0523-Grouped-Staging-And-Linux-Publisher-Closure.md)
closes grouped code-chunk admission, Linux executable publication, and
existing-destination policy preservation in the native publisher chain.
Construction candidate 20 coherently repins the affected paired tools,
admission, promoter, and generic publisher family. Focused evidence plus all
224 Standard Seed and 39 OS tests pass on Windows and Linux with equal
normalized conformance contracts. Decision 0522's 105-artifact/185-case
helper and 193 cumulative managed-call removals remain unchanged. Grouped
repository Qualification, promotion, capability-bearing execution transfer,
and recovery retirement remain open. Specifying a versioned native
hosted-service failure boundary and transferring those remaining executions
form the next block.

[Decision 0516](../Decisions/0516-Native-Source-Parser-Build-And-Inspection-Transfer.md)
moves eight lexer, declaration-parser, and body-parser core/demo/tool builds plus
three core inspections from each broad Seed script into the paired native
helper. The helper owns 79 exact artifacts and 132 cases at that boundary; the
cumulative normal-path removal is 138 managed invocations per host script.
Decision 0517 continues from this boundary into the source-set, graph, and
symbol construction phases while preserving execution as a separate gap.
Decision 0518 continues through bindings, typed WVIR, and WVB emission.
Decision 0519 then transfers the four canonical binary-tool products through
construction, verification, and inspection while keeping execution separate.
Decision 0520 transfers the supported WvDump/WVO execution subset and separates
pinned product identity from current recovery-writer reconstruction evidence.
Decision 0521 transfers the native-equivalent WVA/linker self-test, semantic,
publication, rejection, and preservation block with independent dual-host
helper execution.
Decision 0522 repairs WVO inspector enum-service reconstruction, refreshes the
paired candidate identity, and transfers its no-argument self-test.
Decision 0523 repairs grouped staged-content comparison plus Linux mode and
existing-destination publication policy, then repins the complete affected
publisher construction closure.

[Decision 0515](../Decisions/0515-Native-Hosted-Construction-Build-And-Inspection-Transfer.md)
moves twelve hosted-tool metadata, startup, hosted-container, runtime-header,
and publication-lifetime builds plus nine inspections from each broad Seed
script into the paired native helper. The helper now owns 71 exact artifacts
and 121 cases; the cumulative normal-path removal is 127 managed invocations
per host script. Single-component manifests are local to Runtime, Linker, and
Compiler, while genuine cross-component manifests remain repository-root
aggregates. Decision 0516 continues from this boundary into the first three
source-compiler construction phases while preserving capability-bearing
execution as a separate gap.

[Decision 0514](../Decisions/0514-Native-Runtime-Table-Build-And-Inspection-Transfer.md)
moves sixteen runtime-table, execution-context, argument, entry, and
byte-result-admission builds plus eight bridge inspections from each broad Seed
script into the paired native helper. The helper now owns 59 exact artifacts
and 100 cases; the cumulative normal-path removal is 106 managed invocations
per host script. All sixteen manifests are component-local and eight obsolete
root manifests are removed. Capability-bearing execution, the broad managed
harness, independent Linux evidence, grouped qualification, and recovery
retirement remain open.

[Decision 0513](../Decisions/0513-Native-Fixed-Service-And-Publication-Build-Inspection-Transfer.md)
moves twelve fixed-service, enum-metadata, publication, and service-bundle
builds plus eleven inspections from each broad Seed script into the paired
native helper. The helper now owns 43 exact artifacts and 76 cases; the
cumulative normal-path removal is eighty-two managed invocations per host
script. Ten manifests are component-local, four obsolete root manifests are
removed, and only the genuine cross-component service-bundle aggregates remain
at root. Capability-bearing execution, the broad managed harness, independent
Linux evidence, grouped qualification, and recovery retirement remain open.

[Decision 0512](../Decisions/0512-Native-Io-Service-Build-And-Inspection-Transfer.md)
moves eleven output/file-output/file-input service builds and three bridge
inspections from each broad Seed script into the paired native helper. The
helper now owns 31 exact artifacts and 53 cases; the cumulative normal-path
removal is fifty-nine managed invocations per host script. All eleven touched
manifests are component-local and three obsolete root bridge manifests are
removed. Capability-bearing execution, the broad managed harness, independent
Linux evidence, grouped qualification, and recovery retirement remain open.

[Decision 0511](../Decisions/0511-Native-Service-Source-Build-And-Inspection-Transfer.md)
moves eight native-stencil/runtime-service builds and seven inspections from
each broad Seed script into the paired native helper. At that decision the helper
owned twenty exact artifacts and 39 cases; the cumulative normal-path removal was forty-five
managed invocations per host script. Component manifests are colocated by
default, with one temporary root aggregate for the cross-component Stencil
demo. Its 20-million-step managed execution, the Byte Construction value and
profiling boundary, capability-bearing execution, broad harness, independent
Linux evidence, and grouped qualification are the next T2/E1 boundaries.

[Decision 0510](../Decisions/0510-Native-Foundation-Build-Inspect-And-Execution-Transfer.md)
moves eight Foundation and demo builds, four inspections, and three supported
executions from each broad Seed script into the paired native helper. The
helper owns twelve exact artifacts and 24 transferred calls; the cumulative
normal-path removal is thirty managed invocations per host script. Foundation
component manifests are colocated with their sources, while four temporary
root aggregates explicitly span `Examples/` and `Foundation/`. The 4 MiB Byte
Construction execution/profiling shape, capability-bearing execution, broad
harness, independent Linux evidence, and grouped qualification are the next
T2/E1 boundaries.

[Decision 0509](../Decisions/0509-Native-Wvb-Runner-Source-Reconstruction-And-Step-Reporting.md)
closes the runner project's omitted-module boundary, reconstructs the exact WVB,
WVO, fragment, and paired profile-5 applications from current source, and moves
the Sum fixture's exact `203`-instruction report into the paired native helper.
The helper now owns nine cases and removes fifteen managed invocations per host
script cumulatively. Independent Linux execution, capability-bearing execution,
per-function profiling, the broad harness, and grouped qualification are the
next E1/T2 boundaries.

[Decision 0508](../Decisions/0508-Native-Seed-Wvb-Execution-Qualification-Smoke.md)
routes the three representative plain WVB executions in both broad Seed
scripts through the current native runner. The paired helper now checks eight
cases, including exact results `29`, `1`, and `42` plus input preservation, and
removes three more managed invocations per host script. Profiling and
capability-bearing execution, the broad harness, independent Linux execution,
and later qualification phases remain the next T2 boundaries.

[Decision 0507](../Decisions/0507-Native-Wvb-Runner-Reconstruction.md)
records the prior retained-WVB runner WVO and paired profile-5
applications through native lower/link and WVHV construction. The ordinary
digest-bound launcher now uses that current candidate, and the focused Windows
owner passes 3/3. Decision 0509 supersedes its source boundary with the current
source-built candidate; independent Linux execution remains open.

[Decision 0506](../Decisions/0506-Native-Seed-Console-Aot-Qualification-Smoke.md)
transfers the next coherent boundary in both broad Seed qualification scripts.
The exact `Sum-Data.wvb` now passes through native lower, WVO admission, flat
linking, paired version-1 packaging, and current-host execution without using
the managed target compiler. Together with Decision 0505 this removes eleven
managed invocations per host script. Profiling and capability-bearing WVB
execution, the managed test harness, later qualification phases, independent
Linux execution, and the GitHub cutover remain the next boundaries.

[Decision 0505](../Decisions/0505-Native-Seed-Front-Door-Qualification-Smoke.md)
transfers the source/project build, WVB verify/inspect, and malformed-project
smoke at the start of both broad Seed qualification scripts to paired native
helpers. The current Windows five-case owner passes and removes nine managed
invocations from each host script. The scripts remain managed-normal because
profiling and capability-bearing execution, the test harness, and later
qualification phases have not moved; Linux execution and the remaining
transfers are the next boundary.

[Decision 0504](../Decisions/0504-Native-WebAssembly-Generation-And-Verification.md)
removes the standalone WebAssembly verifier from the direct managed-entry
inventory. The complete current-Windows command now builds its source/WVB
corpus through native front doors, lowers every artifact through the
manifest-bound native backend, and passes the strict Node.js engine plus
record-arena and compiler probes without loading .NET. Independent Linux
execution, paired changed-file ownership, backend-package reconstruction,
cross-browser evidence, and grouped qualification remain.

[Decision 0503](../Decisions/0503-Native-Console-Application-Publisher-Reconstruction.md)
closes the managed application-writer seam for the exact current
console-application-publisher candidate. The current Windows native route pins
the WVB and raw-lowerer WVO oracle, links the admitted object, and constructs
both target bases and final applications through explicit publisher-overlay
variant 4 without invoking either target publisher. The refreshed final
identities bind the current file-input leaf and replace the stale application
digests without changing the WVB, WVO, fragment, or bases. Retained same-release
seeds, independent Linux reconstruction and execution, clean previous-seed
renewal, qualification, atomic installation, promotion, and Stage 0 recovery
release remain.

[Decision 0502](../Decisions/0502-Native-Console-Application-Verifier-Reconstruction.md)
closes the managed application-writer seam for the exact current profile-7
console-application-verifier candidate. The current Windows native route pins
the WVB and raw-lowerer WVO oracle, links the admitted object, and constructs
both two-snapshot applications through the retained hosted-container and
publisher-construction toolsets. Retained same-release seeds, independent Linux
reconstruction and execution, clean previous-seed renewal, qualification,
atomic installation, promotion, and Stage 0 recovery release remain.

[Decision 0501](../Decisions/0501-Native-Wv-Linker-Reconstruction.md)
closes the managed application-writer seam for the exact current standard
Wv-Linker candidate. The current Windows native route pins a raw-lowerer WVO
oracle, derives the fragment through the distinct segmented stage/link/transport
path, and constructs both profile-4 target applications without invoking either
target standard linker. Retained same-release seeds, independent Linux
reconstruction and execution, clean previous-seed renewal, qualification,
atomic installation, promotion, and Stage 0 recovery release remain.

[Decision 0499](../Decisions/0499-Native-Wvo-Publisher-Reconstruction.md)
closes the managed application-writer seam for the exact current WVO publisher
candidate. The current Windows native route uses the raw lowerer to avoid
self-publication, requires the exact WVO oracle, and reconstructs both target
applications through role 3 of the retained publisher pipeline. Independent
Linux reconstruction and execution, clean bootstrap, qualification, promotion,
and Stage 0 recovery release remain.

[Decision 0498](../Decisions/0498-Native-Console-Packager-Application-Reconstruction.md)
closes the managed application-writer seam for the exact current ordinary and
segmented console-packager candidates. The current Windows native route builds
and lowers each project once, links each object once, and constructs both target
applications through profile 5 of the retained hosted-container toolset without
using either target packager. The C1/P1 route still requires the retained seed,
independent Linux reconstruction and execution, broader packaging closure,
promotion, and the grouped retirement gate.

[Decision 0497](../Decisions/0497-Native-Wvb-To-Wvo-Reconstruction.md)
closes the managed application-writer seam for the exact current accepted-subset
lowerer candidate. The current Windows native path reconstructs its 414,298-byte
WVB, both paired target applications, and the unchanged fixed WVB/WVO vector
through the retained segmented toolset. The route still requires the retained
seed, independent Linux reconstruction and execution, complete-backend work,
promotion, and the grouped retirement gate; it is not clean-bootstrap evidence.

[Decision 0496](../Decisions/0496-Native-Segmented-Compiler-Toolset-Reconstruction.md)
closes the managed application-writer seam for the three segmented compiler
process families: the current Windows native path reconstructs their three WVBs
and all six target applications exactly. The path consumes the retained same-
candidate toolset, so the C1 route still requires a previous qualified seed,
independent Linux reconstruction and execution, current full Stage 2, paired
promotion, later-release consumption, and the grouped retirement gate.

[Decision 0310](../Decisions/0310-Fixed-Native-Wvo-Test-Cases.md) advances Phase 10 by moving one accepted WVO and three structural rejection cases into the fixed 26-case .NET-free native plan. Linux execution and the grouped end-of-goal gate still precede promotion.

[Decision 0311](../Decisions/0311-Fixed-Native-Linker-Rejections.md) adds a separate three-case .NET-free linker rejection command, preserving exact diagnostics and an existing output without rebuilding the successful AOT chain. Linux execution and the grouped gate still precede linker promotion.

[Decision 0313](../Decisions/0313-Fixed-Native-Console-Packager-Rejections.md) does the same for entry and empty-image rejection through the public console-packager launcher, using a dedicated six-byte fixture instead of rebuilding or relinking. Linux execution, native host-container construction, and the grouped gate still precede packager promotion.

[Decision 0314](../Decisions/0314-Fixed-Native-Publisher-Rejections.md) moves invalid console-application and WVO admission into one fixed .NET-free command, requiring exact phase reports, destination preservation, and zero publication scratch. Linux execution, native host-container construction, and the grouped gate still precede publisher promotion.

[Decision 0317](../Decisions/0317-Fixed-Native-Wvb-To-Wvo-Rejections.md) fixes malformed-WVB and valid-but-unsupported-function outcomes through the public lowerer launcher, with exact native reports, destination preservation, and isolated-work cleanup. Linux execution, native host-container construction, broader backend completion, and the grouped gate still precede lowerer promotion.

[Decision 0321](../Decisions/0321-Fixed-Native-Wva-Assembler-Rejection-Families.md) transfers one exact output-preserving case for every stable WVA diagnostic family to a focused .NET-free command. The already-qualified assembler route is unchanged; Linux execution of this matrix and the grouped retirement gate remain.

[Decision 0322](../Decisions/0322-Fixed-Native-Wvo-Read-Only-Rejection-Families.md) fixes all thirteen stable WVO 1.0 rejection families through both digest-bound native read-only launchers, requiring identical reports and unchanged inputs without a live C# oracle. Linux execution of this matrix and the grouped retirement gate still precede WVO ordinary-path promotion.

[Decision 0325](../Decisions/0325-Expanded-Native-Linker-Rejection-Families.md) expands the fixed linker command to every externally driven `WVL1001` through `WVL1010` family with exact reports and destination preservation. The internal `WVL1011` reconstruction trap and large-map `WVL1012` boundary retain separate evidence; Linux execution and the grouped gate still precede linker promotion.

[Decision 0327](../Decisions/0327-Fixed-Native-Linker-Map-Limit.md) transfers that separate `WVL1012` boundary to a one-case .NET-free native command. A compact archive replaces large generated WVA source while preserving exact 16,384-definition, report, input, and output evidence; Linux execution and the grouped gate still precede linker promotion.

[Decision 0329](../Decisions/0329-Fixed-Native-Wvb-Unsafe-Rejections.md) transfers five core unsafe instruction-stream boundaries to both digest-bound WVB read-only launchers with immutable compact fixtures and exact phase reports. Broader nominal/limit cases, seeded randomized containment, Linux execution, and the grouped gate remain.

[Decision 0330](../Decisions/0330-Manifest-Driven-Native-Retirement-Test-Suite.md) composes all ten fixed native commands and 74 transferred cases through one digest-bound Windows/Linux coordinator. Exact filters remain the narrow inner loop; the unfiltered command, Linux execution, and grouped retirement gate remain deferred until the final goal candidate.

[Decision 0332](../Decisions/0332-Fixed-Native-Linker-Hostile-Input-Corpus.md) replaces the linker's framework-seeded hostile-byte loop with a portable, immutable 200-input corpus and exact public `WVL1002` behavior. The retirement coordinator now owns 11 suites and 274 cases; Linux execution and the unfiltered grouped gate remain deferred.

[Decision 0334](../Decisions/0334-Fixed-Native-Console-Container-Hostile-Input-Corpus.md) replaces the managed PE/ELF verifier random-byte loops with 256 immutable bounded candidates through the Windvale-native publisher. Exact rejection, input/destination preservation, and zero scratch grow the coordinator to 12 suites and 530 cases; curated valid-shaped mutations, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0335](../Decisions/0335-Fixed-Native-Wvo-Differential-Corpus.md) freezes the exact managed WVO differential sequence and reference decisions into 128 valid-shaped mutations plus 128 arbitrary values. The native verifier agrees on all 32 accepted and 224 rejected cases, growing the coordinator to 13 suites and 786 cases; hostile-size WVO, WVA/source differential, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0336](../Decisions/0336-Fixed-Native-Wva-Differential-Corpus.md) freezes the exact managed 200-case seeded WVA mutation sequence in one compact archive. The native assembler agrees on all 199 Stage 0 rejection codes and the sole accepted 243-byte WVO, growing the coordinator to 14 suites and 986 cases; other extended WVA vectors, arbitrary-source containment, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0337](../Decisions/0337-Fixed-Native-Random-Containment-Corpus.md) freezes the exact continued 2,000-value Stage 0 random sequence across source, WVB, and WVO families. Three focused native lanes preserve every input and assembler destination without a live managed oracle, growing the coordinator to 17 suites and 2,986 cases; Linux execution, remaining differentiated families, and the grouped gate remain deferred.

[Decision 0338](../Decisions/0338-Fixed-Native-Console-Container-Mutations.md) transfers 10 canonical PE and 9 canonical ELF truncation, structural, padding, context, relocation, and trailing-byte cases to the public native publisher while retaining exact Stage 0 code provenance. The coordinator now owns 18 suites and 3,005 cases; the two segmented maximum-size values, hosted version-2 mutations, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0339](../Decisions/0339-Fixed-Native-Wvo-Hostile-Size.md) transfers the first standard WVO byte beyond the ordinary 4-MiB object/value limit to the exact native file-snapshot boundary. Verify, inspect, link, and publish all fail safely while preserving every applicable file, growing the coordinator to 19 suites and 3,009 cases; large-native segmented-object transfer, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0340](../Decisions/0340-Windvale-Native-Hosted-Console-Admission.md) transfers format-2 hosted PE/ELF admission, SHA-256 metadata/output/native checks, canonical startup recovery, and the exact thirteen managed mutations to portable Windvale. Two valid bases and all rejections pass through the native atomic publisher, growing the coordinator to 20 suites and 3,024 cases; segmented maximum-size admission, large-native hosted construction, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0341](../Decisions/0341-Fixed-Native-Console-Segmented-Size-Rejections.md) transfers both version-1 maximum-plus-one application boundaries to a dedicated two-snapshot Windvale verifier. The fixed Windows and Linux-shaped inputs retain their Stage 0 provenance and exact portable rejection ordering, growing the coordinator to 21 suites and 3,026 cases; maximum-size valid construction, large-native segmented-object transfer, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0342](../Decisions/0342-Native-Segmented-Console-Application-Construction.md) transfers both maximum valid version-1 constructions to a focused Windvale recipe streamer. Exact two-chunk PE/ELF outputs match the complete Stage 0 application identities and pass the independent segmented verifier, growing the coordinator to 22 suites and 3,028 cases; native source/host-container reconstruction, durable public publication, large-native segmented-object transfer, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0343](../Decisions/0343-Native-Console-Packager-Source-Reconstruction.md) corrects canonical dependency inventory order for both console-packager projects. The digest-bound native Project 1 front door now reconstructs, compiler-align verifies, and atomically publishes both exact WVB identities, growing the coordinator to 23 suites and 3,030 cases; native PE/ELF host-container reconstruction, durable segmented publication, Linux execution, and the unfiltered grouped gate remain deferred.

[Decision 0344](../Decisions/0344-Native-Console-Packager-Wvo-Reconstruction.md) reorganizes the measured oversized packager, recipe-verification, target-header, and PE-section routines along cohesive boundaries without widening the native arena. The existing two-case source lane now requires exact native WVO reconstruction for both packagers; verifier and publisher projects also rebuild their WVBs through the native front door, while their broader hosted closures and every affected PE/ELF tool container remain explicit later lowering or Stage 0 recovery work. The coordinator stays at 23 suites and 3,030 cases.

[Decision 0345](../Decisions/0345-Verifier-Scale-Native-Staged-Wvo-Publication.md) replaces complete-function byte accumulation with bounded emission, makes instruction-position construction linear, shortens retained record evidence, and fixes multi-iteration native staging writes and rereads on both host adapters. The Windows native chain now produces, publishes, independently verifies, and exactly reconstructs the real seven-chunk 1,049,615-byte verifier WVO without loading .NET. Deterministic Linux packages are pinned but not yet executed. The publisher's separate self-lowering probe reaches the unchanged 128 MiB text arena; resolving that measured lifetime pressure is the next narrow backend slice rather than a reason to widen the arena.

[Decision 0346](../Decisions/0346-Bounded-Native-Publisher-Self-Lowering.md)
closes that measured publisher lifetime boundary on Windows without widening
the native arena. Bounded function-count and grouped-byte limits, block-local
scratch-record allocation, and the exact ABI 22 hidden-result layout let the
native producer and publisher reproduce the Stage 0 publisher WVO exactly.
Decision 0394 removes the later-unused publication bridge from that closure;
the current admission WVB is 431,568 bytes and its WVO is 6,355,569 bytes.
Refreshed extended execution, Linux execution, and grouped qualification remain
open.

[Decision 0347](../Decisions/0347-Fixed-Native-Nominal-Wvb-Rejections.md)
extends the fixed native unsafe-WVB lane from five instruction-stream cases to
ten instruction and nominal-type cases. The same 23-suite retirement plan now
owns 3,035 cases without a live managed oracle; broader nominal limits and
typed opcode families remain explicit later transfers.

[Decision 0429](../Decisions/0429-Fixed-Native-Assembler-Golden-Objects.md)
adds one focused positive assembler lane for the canonical Hello, expanded-x64,
and typed-scalar-x64 sources. Repeated native assembly, exact WVO identities,
and independent WVO admission grow the plan to 24 suites and 3,038 cases.
Additional dynamic WVA vectors, Linux execution, and the grouped gate remain.

[Decision 0430](../Decisions/0430-Fixed-Native-Typed-Wvb-Rejections.md)
extends the existing unsafe-WVB lane from ten to sixteen compact fixed cases by
transferring six typed-control and nominal-shape rejections. The retirement plan
remains 24 suites and now owns 3,044 cases. Larger value-limit cases, additional
typed opcode families and dynamic WVA vectors, Linux execution, and the grouped
gate remain.

[Decision 0431](../Decisions/0431-Compact-Native-Wvb-Rejection-Closure.md)
extends that lane to twenty cases with four compact stack, receiver, and
nominal-kind rejections. The 24-suite plan now owns 3,048 cases. Redundant scalar
truncation, non-serializable invalid UTF-16, and multi-megabyte value limits keep
their decoder, recovery-object, or hostile-size owners; dynamic WVA vectors,
Linux execution, and the grouped gate remain.

[Decision 0432](../Decisions/0432-Fixed-Native-Scalar-X64-Golden-Object.md)
adds the managed positive scalar/SIB assembler source to the existing golden
lane. Its exact 199-byte WVO covers immediate ALU, multiply, shifts, rotates,
and indexed memory, growing the 24-suite plan to 3,049 cases. Generated dynamic
register vectors, Linux execution, and the grouped gate remain.

[Decision 0433](../Decisions/0433-Fixed-Native-Wva-Positive-Matrix.md)
adds every paired 8/16-bit register vector plus the typed narrow immediate and
shift groups to the existing WVA differential lane through one compact archive.
The plan remains 24 suites and grows to 3,066 cases. Other WVA inventory, Linux
execution, and the grouped gate remain.

[Decision 0434](../Decisions/0434-Expanded-Native-Wva-Positive-Matrix.md)
extends the same archive with every paired 32/64-bit register, condition branch,
condition materialization, label-scope, and RIP-relative vector. The plan remains
24 suites and grows to 3,118 cases. Remaining WVA inventory, Linux execution,
and the grouped gate remain.

[Decision 0435](../Decisions/0435-Digest-Bound-Os-Boot-Execution.md)
separates digest-bound Probe 40 QEMU execution from Stage 0 image construction.
The verifier now accepts and preserves one exact caller-supplied EFI without
invoking `dotnet`; the builder has an explicit recovery command. The repaired
normal image passes its exact supplied-image boot contract.
[Decision 0436](../Decisions/0436-Windvale-Native-Uefi-Application-Construction.md)
then transfers canonical UEFI v3 construction and independent verification to
portable Windvale with native Project 1 front doors. The other four boot
scenarios, upstream native Probe 40 composition, and promotion remain open.
[Decision 0437](../Decisions/0437-Native-Linker-To-Uefi-Packaging.md) connects
the real digest-bound native linker output and its entry evidence to one hosted
Windvale packager. Retained host-container construction, independent Linux
execution, complete five-scenario composition, and promotion are open at that
checkpoint.
[Decision 0438](../Decisions/0438-Retained-Native-Uefi-Packager-Containers.md)
then makes hosted packaging explicitly cross-target, retains paired native-built
UEFI packager containers behind digest-bound launchers, and transfers three
cases into the native retirement plan. Independent Linux execution, durable
UEFI publication, upstream five-scenario composition, and promotion remain.
[Decision 0439](../Decisions/0439-Native-Uefi-Recovery-Packaging-Cutover.md)
then cuts the normal Probe 40 recovery workflow over to that retained native
packager. Stage 0 still creates the scenario objects and links their flat image;
native object production/linking, the other four scenarios, Linux execution,
and promotion remain.
[Decision 0440](../Decisions/0440-Probe-40-Object-Inventory-Boundary.md)
exposes fifteen logical components as fourteen ordered WVO containers and
proves their managed differential link remains byte-identical.
[Decision 0441](../Decisions/0441-Scale-Safe-Native-Wv-Linker-Relocation-Emission.md)
isolates the retained linker's 128 MiB arena failure and makes the current
Windows candidate reproduce the exact image without enlarging that arena.
Independent Linux execution is the next qualification transfer.
[Decision 0442](../Decisions/0442-Native-Probe-40-Recovery-Linking-Cutover.md)
removes the managed link from the current-host normal recovery command. Stage 0
now ends at ordered object production; Linux recovery execution and native
Probe 40 object/scenario production are the next transfer.
[Decision 0443](../Decisions/0443-Native-Probe-40-Top-Level-Wva-Assembly.md)
moves the three top-level WVA shim objects to the qualified native assembler
and leaves Stage 0 producing eleven link inputs. Inner process-image WVA,
remaining object construction, and Linux recovery execution are next.
[Decision 0444](../Decisions/0444-Native-Probe-40-Inner-Process-Wva-Handoff.md)
moves the four scenario-selected inner WVA objects to the same native assembler
and feeds their exact WVOs into Stage 0 composition. The three inner links,
remaining object construction, and Linux recovery execution are next.
[Decision 0445](../Decisions/0445-Digest-Bound-Native-Probe-40-Object-Seed.md)
freezes those eleven remaining normal-scenario WVOs as an explicit Stage 0 seed
and adds a native-only ordinary Windows/Linux build. Linux execution and native
replacement of each frozen producer remain before promotion.
[Decision 0446](../Decisions/0446-Native-Probe-40-Windvale-Source-Producer.md)
then compiles and lowers the existing native-probe Windvale source through the
ordinary native toolchain, removes its WVO from the seed, and leaves ten frozen
producers. Linux execution and the remaining producer transfers stay pending.
[Decision 0447](../Decisions/0447-Native-Probe-40-Admission-Source-Producer.md)
adds a verified native WVO export rename and moves the admission Windvale source
through the same ordinary compiler/lowerer path, leaving nine frozen producers.
[Decision 0448](../Decisions/0448-Native-Probe-40-Exception-Object-Producer.md)
adds a focused Windvale-native x64 exception-object producer over verified WVO
construction, leaving eight frozen producers.
[Decision 0449](../Decisions/0449-Native-Probe-40-Admission-Bridge-Producer.md)
consolidates that producer with the WVB admission-bridge recipe, leaving seven
frozen producers without retaining a second host package.
[Decision 0450](../Decisions/0450-Native-Probe-40-Native-Bridge-And-Support-Producer.md)
adds the native bridge/support recipe to the same bounded producer, leaving six
frozen producers and keeping its source at a reviewable 211 lines.
[Decision 0451](../Decisions/0451-Native-Probe-40-Paging-Object-Producer.md)
adds the exact paging installer without widening WVA, leaving five frozen
producers and a 317-line source that must not absorb memory without reassessment.
[Decision 0452](../Decisions/0452-Native-Probe-40-Memory-Object-Producer.md)
adds the normal memory object through a separate 158-line producer behind the
same public launcher, leaving four frozen producers without growing the compact
recipe source into a catch-all.
[Decision 0453](../Decisions/0453-Native-Probe-40-Loader-Object-Producer.md)
reconstructs the normal UEFI loader object from one pinned architecture fixture
through a 75-line Windvale producer, leaving three frozen producers without
duplicating 6,115 decimal byte literals or prematurely widening WVA.
[Decision 0454](../Decisions/0454-Native-Probe-40-System-Kernel-Target.md)
replaces the frozen kernel object with a real Windvale source-to-WVB-to-WVO
path. Its bounded reader and hosted emitter remain separate reviewable sources,
leaving two frozen producers without treating machine bytes as source.
[Decision 0455](../Decisions/0455-Native-Probe-40-Process-Policy-Source-Path.md)
uses the general native builder, lowerer, and export renamer for the existing
portable process-policy source, leaving only the large process object frozen.
[Decision 0456](../Decisions/0456-Native-Probe-40-Process-Object.md) regenerates
that process object's 463,531 payload bytes from canonical Windvale sources,
WVA shims, and versioned records while retaining one 46,678-byte reviewed
architecture fixture. The normal Probe 40 object inventory now has eleven
native producers and zero frozen WVOs. Linux execution, final Decision 0057
qualification, and the digest-bound recovery release remain.
[Decision 0489](../Decisions/0489-Native-Probe-40-Architecture-Fault-Scenarios.md)
then extends the focused memory-object producer and ordinary image builder to
the exact invalid-opcode and general-protection variants. Both native images
match the Stage 0 oracle byte for byte and pass their pinned QEMU vector/error
contracts. Linux execution and the two contained process-fault images remain
before final qualification.
[Decision 0457](../Decisions/0457-Normal-Path-Dotnet-Audit.md) establishes the
normal-path audit baseline. Decisions 0458 and 0504 remove its indirect local
Seed invocation and standalone WebAssembly entry respectively. Three direct
normal managed files now remain—the paired broad Seed gates and the GitHub
qualification/release workflow—while nine other direct entries are correctly
isolated recovery tools. The remaining route is focused backend/runtime and
broad-suite evidence-gap closure followed by one GitHub dual-host cutover,
rather than a line-for-line managed-test rewrite.
[Decision 0458](../Decisions/0458-Native-Changed-File-Verification.md) completes
that local front door on Windows. Qualification-scoped changes now select
focused native suites in canonical order or stop on stable named gaps; unknown
paths invoke neither .NET nor the complete native gate. Non-Windows dispatch
execution, gap closure, independent Linux WebAssembly verification, and GitHub
cutover remain.

The destination is durable; intermediate phases are adaptable. When experiments reveal an impractical contract or a clearly better alternative, update the relevant specification or decision and revise this roadmap rather than preserving accidental early designs.

## Sequencing principle

Windvale remains bytecode-first for as long as that reduces bootstrap loops. A new Windvale-written tool should become useful and reproducible on Windows and Linux before Windvale OS depends on it. Portable logic remains separate from hosted I/O, and each qualified phase requires deterministic artifacts, mandatory verification, adversarial coverage, and real cross-host evidence. C#/.NET remains the reference and recovery path until [Decision 0057's native-retirement gate](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md#native-retirement-gate), but [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) ends forward C# source-language expansion at the next qualified WVB 1.11 baseline and moves the ordinary source-to-verified-WVB entry point first. After the complete gate, .NET leaves normal automation rather than becoming a permanent host dependency.

## Phases

| Phase | Deliverable and qualification gate | Status |
| --- | --- | --- |
| 0. Seed and byte primitives | C# Stage 0, typed WIR, verified runtime, `u8`, `u32`, immutable bytes, and Windows/Debian equality. | Qualified |
| 1. `Wvˉdumpˉcore` | Windvale source safely walks complete WVB headers and section envelopes over supplied bytes, including hostile lengths and malformed cases. | Qualified |
| 2. Structured inspection | Add only the records, enums, structured results/errors, and bounded formatting demanded by useful section descriptions. | Qualified |
| 3. Hosted resource boundary | Explicit arguments, file-byte input, diagnostics, and output capabilities with portable parsing kept independent. | Qualified |
| 4. Useful `wvdump` | Inspect the same real modules identically on Windows and Debian with golden machine-readable reports. | Qualified |
| 5. Object foundation | Deterministic byte construction, sections, symbols, relocations, and the smallest shared object contracts needed by an assembler. | Qualified |
| 6. Assembler and linker | Windvale-written assembler and linker running first as verified bytecode on Windows and Linux. | Qualified |
| 7. Foundation and platform libraries | Compact reusable collections, text, binary-format, diagnostics, testing, capability-provider, and shared or platform-scoped I/O modules driven by concrete tool and application needs. | Ongoing; Decision 0153 adds the first versioned rights-limited directory-read contract after Decision 0145's explicit transitive approval, Decision 0208 adds its bounded reference Windows/Linux launcher binding, Decision 0210 composes that provider with the unqualified `WVDB 1` reader, Decision 0211 adds format-neutral checked `u64` page geometry, and implemented-candidate Decision 0212 adds one pre-opened mutable `u64` storage object with a shared Stage 0 Windows/Linux adapter |
| 8. Self-hosted compiler | Windvale-written lexer, parser, semantics, and code generation for a meaningful subset, followed by a reproducible bootstrap closure. | Qualified bytecode self-reproduction on Windows and Debian |
| 9. Shared native backend | Native WIR/WVB lowering, x86-64 ABI, WVO/AOT output, baseline JIT, and interpreter/JIT/AOT differential tests. | Cross-host-qualified ABI 22 composes frame-owned records, Decision 0147 ownership-plan evidence, bounded dynamic-value mechanics, complete integrated native compiler reproduction, and Decision 0151's physical full-allocator schedule |
| 10. Native host tools and .NET retirement | Produce and qualify native Windvale tools, runtime, JIT/AOT execution, and bootstrap recovery on Windows and Linux without a normal .NET dependency. | Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` qualifies the Decision 0213 WVB 1.11 semantic-freeze baseline; `9d36387867ebff80ee94c6f9f7996da4ef32a4a3` qualifies the first deterministic Decision 0214 Windows/Linux publisher profile; `d2e71c1d6491153afb715674fc13ba2c6276326a` qualifies the distributed composition as the ordinary project source-to-WVB path; and `e2d9c52548fd782a57765b1a9635d8cbe009df20` qualifies native verification and inspection. Decision 0217 pins the runner candidate; Decisions 0218, 0226, 0227, 0229, and 0230 expand its fixed native plan to five portable results, three exact runtime failures, five malformed-WVB envelope rejections, eight typed-execution corruptions, and one control-reachability corruption; and Decision 0220 qualifies and promotes the native assembler without adding startup assembly. Decisions 0221–0224 add paired native linker, WVO read-only, version-1 console-packager, and accepted-subset WVB-to-WVO candidates by reusing existing startup/service layouts; Decision 0225 composes the build/lower/link/package chain into one fixed current-host executable; Decision 0228 expands the lowerer to eight decreasing-ordinal scalar/control functions through a focused layout module; Decision 0231 adds one bounded immutable i32 declaration with exact canonical `Sum-Data.wv` `.rodata` and relocation lowering through focused data/object modules; Decision 0232 admits arbitrary exported-Main order plus bounded forward/self/cyclic scalar calls through a complete signature pass; Decision 0233 adds the bounded `u8`/`u32` and typed-return shapes required by compiler-produced `Function-Only.wv`; Decision 0234 completes the bounded `u32`/`u8` comparison families shared by the remaining compiler fixtures; Decision 0235 adds bounded multiple immutable declarations plus static borrowed text/bytes descriptor locals, views, slicing, length, and reads through an extracted instruction-state module; Decision 0236 adds bounded service-backed text concatenation, UTF-8 validation/conversion, and quoting; Decision 0237 adds bounded generation-owned bytes concatenation with exact compiler-produced `Data-And-Text.wv` WVO equality; Decision 0238 adds bounded nominal-table admission plus enum locals, constants, comparisons, and name lookup with exact focused WVO equality; Decision 0239 adds deterministic one-block direct record construction, local copying, and field access with exact focused WVO equality; Decision 0240 adds nonzero-first enum admission plus one-block record parameters, caller-owned returns, and bounded record calls through an extracted call-instruction module; Decision 0241 adds multi-block record-local liveness, block-scoped record temporaries, and scalar-returning record consumers with exact compiler-produced `Nominal-Types.wv` WVO equality through the direct current-host package; Decision 0251 matches ABI 22's bounded 64-parameter register-plus-stack call contract through a focused argument-transport module and exact widened descriptor-call evidence; Decision 0254 expands the measured general function envelope to the real tool's local/code/instruction needs while preserving the hard 2,048-cell native frame; Decision 0256 compacts record liveness to 256 declared record locals while expanding its bounded control envelope to 1,024 blocks and 8,192 instructions; Decision 0259 replaces single-record event accounting with bounded multi-record call uses while preserving exact liveness checks; Decision 0260 admits exact enum parameters and returns through the existing 32-bit ABI path; Decision 0262 adds service-backed unsigned formatting with exact maximum-value differential evidence; Decision 0265 adds bounded one-byte construction while reusing the existing dynamic-byte ownership module; Decisions 0267 and 0268 extend that owned construction path to four-byte signed and unsigned little-endian values; Decision 0269 closes the checked `u32` add/subtract/multiply family; and Decision 0271 adds bounded two-byte little-endian construction with exact narrowing failure. Grouped source qualification, artifact promotion, exact-descendant runner/test evidence, remaining malformed/unsafe-test transfer, complete native-backend and hosted-capability transfer, remaining tools, and the final recovery archive remain. |
| 11. Boot path and kernel | x86-64 UEFI/QEMU boot, diagnostics, memory foundation, minimal kernel boundary, and Hyper-V qualification. | Cross-host-qualified Probe 40 adds `WVKMEM17`, fixed generation-safe memory objects, and WVA-owned non-tail client release/zero/reuse while the directory object remains live; all 87 Seed and 39 OS tests pass on Windows/Debian, plus five pinned Windows QEMU scenarios |
| 12. Runtime on Windvale OS | Load, verify, and run one identical WVB through equivalent Windvale-native execution contracts across Windows, Linux, and Windvale OS. | Qualified at `f3eca7c`: canonical `Sum-Data.wv` WVB returns `29` through Windows/Linux reference/native paths and a protected Windvale OS interpreter |
| 13. Public foundation | Reproducible recovery bootstrap, security limits, licensing, governance, contribution rules, and public-release criteria. | Public repository and policies are live; the initial publication baseline and ongoing public operations remain |

Phase 10's implemented-candidate sequence continues with Decision 0272's lossless `u8`-to-`u32` conversion, Decision 0274's exact `u32.divide` / `u32.remainder` family, [Decision 0276](../Decisions/0276-Capability-Aware-Record-Storage.md)'s capability-aware supplemental record-storage closure, [Decision 0279](../Decisions/0279-Bounded-Record-Planner-Lifetimes.md)'s packed phase evidence and bounded planner-lifetime grouping, Decisions [0280](../Decisions/0280-Bounded-Native-Analysis-And-Artifact-Aggregation.md) through [0283](../Decisions/0283-Bounded-Native-Object-Publication-Cursor.md)'s segmentable WVO regions, bounded batches, and exact publication cursor, Decisions [0284](../Decisions/0284-Versioned-Native-Object-Staging-Manifest.md) through [0287](../Decisions/0287-Validated-Native-Staging-Manifest-Accessors.md)'s versioned staging transport and scalar manifest bridge, [Decision 0288](../Decisions/0288-Segmented-Large-Native-Wvo-Section-Envelope.md)'s bounded 32 MiB compiler-output section-envelope reader, [Decision 0290](../Decisions/0290-Bounded-Compiler-Wvo-Symbol-Verification.md)'s complete bounded compiler-symbol reader and exact relocation boundary, [Decision 0291](../Decisions/0291-Bounded-Compiler-Wvo-Relocation-And-Placeholder-Verification.md)'s canonical relocation, per-chunk zero-placeholder, and padding verifier, [Decision 0293](../Decisions/0293-Bounded-Staged-Wvo-Content-Identity.md)'s byte-for-byte content cursor over every bounded staged value, [Decision 0295](../Decisions/0295-Exact-Staged-Wvo-Snapshot-Admission.md)'s exact four-resource preflight plus input/manifest/chunk snapshot sequence under the existing 64-entry native table, [Decision 0299](../Decisions/0299-Fixed-Native-Staged-Wvo-Publication.md)'s fixed host-identity, exclusive-sibling, durable-reread, atomic-replacement, and cleanup adapters, and [Decision 0300](../Decisions/0300-Native-Staged-Wvo-Producer-Publisher-Composition.md)'s exact native staging producer plus current-host producer/publisher composition. [Decision 0301](../Decisions/0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md) separately pins the existing WVO read-only packages behind candidate Windows/Linux launchers, [Decision 0302](../Decisions/0302-Digest-Bound-Native-Wvo-Linker-Candidate.md) does the same for the standard flat linker, [Decision 0303](../Decisions/0303-Digest-Bound-Native-Console-Packager-Candidate.md) pins the bounded version-1 materializer while explicitly retaining Stage 0 construction, [Decision 0304](../Decisions/0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md) pins the current accepted-subset lowerer plus its native-produced fixed vector, [Decision 0305](../Decisions/0305-Digest-Bound-Native-Aot-Chain-Test.md) composes those launchers into a permanent no-.NET fixed-vector AOT test, [Decision 0307](../Decisions/0307-Native-Console-Application-Publication.md) adds portable completed-application admission plus the reused native atomic replacement transaction to digest-bound packaging, and [Decision 0308](../Decisions/0308-Native-Wvo-Publication.md) shares complete portable WVO admission between inspection and a five-service atomic publisher used by the accepted-subset lowerer; their ordinary-path cutovers still wait for the grouped gate. Direct self-lowering clears the previously observed unsupported-operation and analysis-amplification boundaries but still has not run through this staged native chain on the complete tool. That long run, Linux composition, native replacement of Stage 0 application constructors, complete backend and test transfer, promotion, and the grouped end-of-goal gate remain separate. Raising the arena or silently widening `bytes` is not the plan.

The current database-driven language expansion is an implemented local candidate under Decisions [0138](../Decisions/0138-Conditional-Wvb-1-7-64-Bit-Scalars.md), [0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md), [0199](../Decisions/0199-Nominal-Payload-Variants-And-Recoverable-Results.md), [0200](../Decisions/0200-Bounded-Sequences-Affine-Builders-And-For.md), [0209](../Decisions/0209-Single-Current-Wvb-1-11-Format.md), [0211](../Decisions/0211-U64-Database-Storage-Geometry.md), [0212](../Decisions/0212-First-Preopened-Random-Access-Storage.md), and durable-engine Decisions [0534](../Decisions/0534-First-Durable-Database-Superblock.md) through [0551](../Decisions/0551-General-Depth-Two-Upsert-And-Obsolete-Ownership.md). Stage 0 and the Windvale-written compiler emit only canonical WVB 1.11 and cover inference, constants, module ownership/privacy/metadata, named records, exhaustive match and payload variants, bounded sequences and affine builders, deterministic `for`, loop and short-circuit control, compound assignment, checked integer operators, exact text/bytes equality, exact little-endian `u64` byte codecs, and explicit lossless `u32` to `u64` widening. Native and bounded WebAssembly paths consume explicit subsets of the evolved compiler artifact locally. The hosted storage binding has launcher-owned lifetime and provider generation fencing; general typed resource values, source-scoped cleanup, multiple instances, and manifest binding remain deliberately undecided.

Implemented-candidate [Decision 0207](../Decisions/0207-U64-Binary-Fields-For-Durable-Storage.md) adds exact little-endian `u64` byte fields. Decision 0209 folds those operations into canonical WVB 1.11 and brings the Windvale-written compiler to the same source/WVB surface; native, WebAssembly, and Windvale OS consumers retain explicit narrower 1.11 subsets. Implemented-candidate [Decision 0208](../Decisions/0208-Native-Read-Only-Directory-Snapshot-Binding.md) completes the ordinary reference-launcher binding for the existing immutable directory-read capability on Windows and in shared .NET host code intended for Linux. Decision 0212 now supplies [`storage.random_access_v1`](../../Specifications/Random-Access-Storage-Capability.md) and a shared Stage 0 adapter for one existing mutable file. Both adapters still await the independent Linux gate.

The [experimental Windvale Database reader](../../Specifications/Windvale-Database-Reader.md) first applied those facilities to immutable bytes. The durable successor now has checked `u64` geometry, bounded random-access mutation, dual superblocks, checksummed immutable pages and compact logs, recovery-safe publication, variable-key tree nodes, provider-backed depth-two lookup, repeated routed-leaf replacement and split propagation, and unique obsolete-page ownership. Compiler/lowerer/tool latency and generated-code size are the immediate measured priority; the next database engine gate is depth-three root growth and internal split propagation. Concurrency, group commit, native path replacement, and directory durability remain separate interfaces.

[Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md) keeps that database and compiler evidence cold while reducing its feedback critical path. The sharding decision measured 52 suites and 3,287 cases; the current manifest contains 3,289 cases after the depth-two transaction fixture and structural-only WVO hostile-size case. GitHub runs all four shards on both hosts, preserves the sequential oracle and focused filters, reports non-semantic owner timing, and retains the unchanged aggregate Verification gate. Content-addressed compiler-product reuse remains separate qualification work rather than an implicit cache shortcut.

## Exploratory WebAssembly goal

WebAssembly remains an optional interoperability track and does not reorder the native Windows/Linux or Windvale OS phase gates. [Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md) accepts an early experimental Windvale-native route and a later complete replacement gate. Direct source compilation consumes typed WIR; canonical verified WVB remains the distribution identity and input to the browser interpreter or later verified runtime compilation. Both preserve Windvale results, traps, capabilities, and defined resource counters without making WebAssembly the language definition.

[Decision 0102](../Decisions/0102-First-Windvale-WebAssembly-Backend-Slice.md) implements the first constant slice, [Decision 0104](../Decisions/0104-WebAssembly-Checked-Addition-And-Execution-Contract.md) adds checked arithmetic and execution ABI 1, and cross-host-qualified [Decision 0106](../Decisions/0106-Bounded-Straight-I32-WebAssembly-Lowering.md) adds bounded straight-line scalar lowering. [Decision 0107](../Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) integrates a disposable browser worker. Cross-host-qualified [Decision 0113](../Decisions/0113-Metered-WebAssembly-Control-Flow.md) adds ABI-2 dynamic instruction limits and one contained loop. Cross-host-qualified [Decision 0116](../Decisions/0116-Sequential-WebAssembly-Control-Regions.md) advances the implementation to sequential loops, `if`, and `if/else`, including both conditional routes and the deployed .NET-free page. [Decision 0120](../Decisions/0120-Bounded-WebAssembly-Call-Graph.md) adds two through eight acyclic direct functions, real Wasm calls, and one shared ABI-2 instruction budget across callees. [Decision 0121](../Decisions/0121-WebAssembly-Calls-With-Structured-Control.md) composes those calls with loops and conditionals, including calls inside both routes. [Decision 0123](../Decisions/0123-Versioned-WebAssembly-Linear-Memory-And-Utf8-Buffers.md) adds execution ABI 3 with fixed disjoint 4 MiB input/output regions, strict guest-side UTF-8, exact bytes/text identity profiles, and editable .NET-free page input. [Decision 0128](../Decisions/0128-Bounded-WebAssembly-Runtime-Values.md) adds a statically checked straight-line primitive/bytes runtime, internal descriptors, byte readers/builders, concatenation, and a bounded output arena with explicit resource failures. Cross-host-qualified [Decision 0131](../Decisions/0131-Windvale-Native-WebAssembly-Wvb-Envelope-Verifier.md) composes checked unsigned scalar operations and compiler-produced nested control with those values, then executes the first Windvale-written WVB envelope verifier as import-free Wasm. [Decision 0134](../Decisions/0134-Windvale-Native-WebAssembly-Wvb-Structural-Verifier.md) scales that bounded selector and completely consumes all seven WVB payload schemas in Windvale-authored import-free Wasm. [Decision 0139](../Decisions/0139-Descriptor-Bearing-WebAssembly-Call-Graph.md) adds two through eight acyclic `bytes -> bytes` functions with real private Wasm calls and one shared ABI-3 status, instruction budget, and arena across control. [Decision 0144](../Decisions/0144-Modular-WebAssembly-Wvb-Canonical-Metadata-And-References.md) uses that boundary for an eight-function canonical metadata/reference verifier; [Decision 0146](../Decisions/0146-Expanded-Descriptor-Bearing-WebAssembly-Call-Graph.md) expands the graph to sixteen functions; [Decision 0149](../Decisions/0149-Windvale-Native-WebAssembly-Wvb-Executable-Verifier.md) adds compiler-aligned typed executable proof; [Decision 0152](../Decisions/0152-First-Wasm-Hosted-Wvb-Scalar-Interpreter.md) runs complete-verifier-approved scalar WVB through a separate import-free interpreter; [Decision 0157](../Decisions/0157-Wasm-Hosted-Wvb-Text-And-Bytes-Values.md) adds bounded static data, descriptor calls, immutable text/bytes operations, strict UTF-8, and explicit value/heap failures; [Decision 0158](../Decisions/0158-Wasm-Hosted-Wvb-Formatting-And-Quoting.md) adds invariant scalar formatting and deterministic UTF-16-compatible text quoting; [Decision 0162](../Decisions/0162-Import-Free-WebAssembly-Sha256-Lowering.md) adds deterministic import-free SHA-256 target lowering; [Decision 0166](../Decisions/0166-Wasm-Hosted-Record-And-Enum-Values.md) adds typed record/enum cells, defaults, field/member operations, and bounded record allocation; and [Decision 0174](../Decisions/0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md) adds a capability-free compiler memory adapter plus byte-array guest entry and pins that stage at the sixteen-function preflight ceiling. Capability authorization, complete compiler execution, cross-host profile-11-through-16 evidence, cross-browser evidence, and complete worker containment remain host-hardening extensions.

[Decision 0170](../Decisions/0170-Compiler-Capacity-Wasm-Wvb-Verifier-Bundle.md) establishes the original three-phase exact compiler-admission contract. [Decision 0174](../Decisions/0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md) adds the capability-free WVSS-to-WVB adapter and proves `Main(bytes) -> bytes` guest execution through `WVXI 2` / `WVXO 2`. [Decision 0175](../Decisions/0175-Compiler-Scale-Wasm-Interpreter-Execution-Entry.md) expands the interpreter to compiler-scale metadata, and [Decision 0177](../Decisions/0177-Exact-Per-Function-Wasm-Interpreter-Frames.md) replaces candidate-wide frames with exact per-function locals and compact saved frames. [Decision 0189](../Decisions/0189-Bounded-Reclaiming-Wasm-Value-Storage.md) adds bounded first-fit value reclamation, descriptor reference transitions, coalescing, reset, and constant-time local-shape metadata. [Decision 0197](../Decisions/0197-Bounded-Reclaiming-Wasm-Guest-Records.md) adds stable-slot guest records and conservative bounded tracing across every interpreter root, clears the former record boundary, and reaches an ordinary 100,000-instruction compiler budget result. Current [Decision 0202](../Decisions/0202-Four-Phase-Compiler-Capacity-WebAssembly-Verification.md) admits the evolved hosted and portable compilers through metadata/reference, two complementary typed partitions, and control/reachability, all below the retained unsigned 32-bit meter ceiling. Ownership and reclamation for the separately retained 64 KiB guest text/bytes heap is the next measured implementation boundary.

[Decision 0273](../Decisions/0273-Warmed-WebAssembly-Compiler-Worker.md) refreshes the exact browser package with a five-function interpreter and validates a same-instance bounded warmup. [Decision 0275](../Decisions/0275-Normal-Browser-Native-Playground.md) makes the static worker the normal Monaco playground and removes Blazor/.NET from local startup and deployment. [Decision 0277](../Decisions/0277-Native-WebAssembly-Compiler-Regeneration.md) pins the native source compiler that reproduces the portable browser compiler WVB, and [Decision 0278](../Decisions/0278-Native-WebAssembly-Artifact-Regeneration.md) pins the native WebAssembly compiler that reproduces the interpreter Wasm. [Decision 0289](../Decisions/0289-Bounded-WebAssembly-Interpreter-Warmup.md) replaces the expensive compiler self-warmup with a digest-pinned 292-byte Windvale guest and reduces the measured local Chromium path to 64.3 seconds. [Decision 0294](../Decisions/0294-Static-WebAssembly-Opcode-Effect-Table.md) uses the new bounded direct static-data lowering to replace the interpreter's per-instruction branch tree with one exact table lookup, reducing compiler execution by 7.23% to 1,404,070,227 outer instructions and the measured complete Node.js path to 59.4 seconds. [Decision 0296](../Decisions/0296-Bounded-Direct-WebAssembly-Nominal-Tables.md) completely validates the exact compiler's 82 nominal declarations without changing primitive output. [Decision 0297](../Decisions/0297-Compiler-Scale-WebAssembly-Function-Inventory.md) admits its bounded 417-function directory. [Decision 0298](../Decisions/0298-Compiler-Scale-WebAssembly-Code-Inventory.md) then decodes all 157,844 instruction encodings and 2,991 direct-call targets. [Decision 0306](../Decisions/0306-Compiler-WebAssembly-Function-Directory.md) materializes a separate immutable 32-byte-entry function directory, and [Decision 0309](../Decisions/0309-Typed-Compiler-WebAssembly-Call-Agreement.md) adds the corresponding nominal type directory plus a compact typed stack. Sharded exact evidence validates all 417 functions and all 2,991 direct calls, while an in-range incompatible call target fails precisely. [Decision 0312](../Decisions/0312-General-Compiler-WebAssembly-Executable-Graph.md) now replaces the small selector's structural assumptions with a deterministic graph: arbitrary `Main` index 2 reaches 397 functions through all 2,991 ordered targets without a sixteen-function mask or call-direction rule. Direct lowering advances to operation/control representation and direct execution of the portable memory tool; the established artifact compiler still fails closed at `Unsupportedˉcode`. [Decision 0504](../Decisions/0504-Native-WebAssembly-Generation-And-Verification.md) removes the managed compiler/backend execution from the complete current-Windows generation-and-verification command and passes its full strict engine and probe evidence through the manifest-bound native backend. The complete normal website and Windows verifier paths are now .NET-free. Independent Linux execution of the exact tool packages, current cross-browser measurement, broader direct-compiler source coverage, backend-package reconstruction, and the project-wide retirement gate remain open.

## Detailed execution plan

### Phase 6 - assembler and linker

Phase 6 is split so that parsing, object production, and link semantics can fail or evolve independently.

| Gate | Deliverable | Qualification evidence |
| --- | --- | --- |
| 6A. WVA contract oracle | Versioned WVA 1 grammar, strict Stage 0 parser, x86-64 encoder, independent WVO verification, and canonical examples. | Qualified on Windows and Debian at `3bfc6bb`; exact object bytes agree. |
| 6B. Windvale source scanner | A Windvale-written bounded UTF-8/line/token scanner that recognizes WVA 1 without host text parsing. | Qualified on Windows and Debian at `e5fd109`; exact module bytes and hosted reports agree. |
| 6C. Windvale semantic inspector | Multi-pass symbol, section, definition, statement, reference, ordering, and limit validation expressed in verified bytecode. | Qualified on Windows and Debian at `cc57bf9`; exact module bytes, accepted/rejected classifications, and hosted reports agree. |
| 6D. Windvale object encoder | Instruction/data encoding, derived offsets and sizes, symbol records, and relocations emitted as WVO 1.0. | Qualified on Windows and Debian at `a689617`; canonical, boundary, complete-statement, register, multi-definition, line-ending, empty, and accepted mutation outputs are byte-for-byte identical to Stage 0 and pass the independent WVO verifier. |
| 6E. Hosted assembler shell | Explicit input/output arguments and byte capabilities around a portable assembler core; output is written only after complete validation. | Qualified on Windows and Debian at `a689617`; real CLI output agrees, rejected input invokes no writer, and native failure cases leave no new or modified object. |
| 6F. Linker contract and oracle | A separate link specification covering inputs, duplicate/undefined symbols, layout, alignment, relocation arithmetic, limits, map output, and the first flat-image target. | Qualified on Windows and Debian at `9c4b9f5`; 31 tests, real multi-object CLI output, exact image/map bytes, hostile objects, all resolution failures, aggregate/map limits, layout/address overflow, both relocation overflows, independent image reconstruction, and no-output failures agree. |
| 6G. Windvale linker | A Windvale-written verified-bytecode linker implementing the accepted contract. | Qualified on Windows and Debian at `40ac57d`; the exact WVB, 24-byte image, 1,721-byte map, normalized contract, success publication, deterministic no-write failures, existing-output preservation, and host-write failure boundary agree. |

The post-qualification WVA 1 expansion adds all sixteen 8/16/32/64-bit general-purpose register families, typed REX/ModRM/SIB operations, definition-local labels, every near condition code, register stack and indirect control, condition materialization, zero/sign extension, RIP-relative symbols, signed width-appropriate immediate ALU/test, signed multiply for 16/32/64 bits, bounded immediate shifts/rotates, deterministic base/index/scale/`disp32` memory access at every scalar width, and byte port I/O without changing WVO 1.0. The Windvale and C# implementations have local byte-for-byte differential evidence, and the kernel's former raw C# exception-terminal emitter has moved to canonical WVA/WVO. Cross-host and pinned-QEMU requalification are pending. Division, variable-count shifts, general 64-bit immediates, conditional moves, bounded label tables, broader emitter migrations, and a shared production encoder with the native backend remain follow-up work.

Phase 6 is complete only after 6G. A parser demo, hard-coded object producer, or host-only wrapper is useful evidence but is not a substitute for the accepted assembler or linker.

### Phase 7 - Foundation modules driven by real tools

The first enabling slice, bounded static source-module composition, is qualified on Windows and Debian at `df80f91` under Decision 0019. It deliberately changes neither WVB 1.6 nor runtime loading. The first two-consumer module, `Foundationˉmachineˉcontracts`, is cross-host qualified at `d46af86` under Decision 0020. The next measured extraction, `Foundationˉbyteˉordering`, is cross-host qualified at `4fdea22` under Decision 0021 for the object core, assembler, and linker. Static contracts with dependency records/enums and `Foundationˉdecimalˉparsing` are cross-host qualified together at `6d2a351` under Decisions 0022 and 0023. `Foundationˉbyteˉconstruction` is cross-host qualified at `26e2fd1` under Decision 0024; it replaces duplicated assembler/linker repeat and patch logic and supplies the immutable backpatching seam needed by a future WVB encoder. Implemented-candidate Decision 0145 retains those bytes and runtime linkage while admitting profile-compatible capability-bearing dependencies only through explicit transitive approval. Implemented-candidate Decision 0153 then adds `filesystem.directory_read_v1`: one pre-bound immutable directory, strict segment names, exact chunks of at most 3 KiB, mandatory provider-envelope validation, and a Windvale-owned typed decoder. Decisions 0210 through 0212 add immutable snapshot lookup, checked `u64` page geometry, and the shared pre-opened mutable storage boundary. Decisions 0534 through 0551 build repeated depth-two variable-key generations over it. Windvale OS/WebAssembly bindings, depth-three growth, reclamation, and concurrency remain separate measured seams.

1. Identify duplicated bounded scanning, byte construction, name validation, diagnostics, result/status, and test behavior in the qualified assembler and linker.
2. Introduce the smallest module/import and collection facilities needed to express those reusable contracts without hidden mutation or unbounded allocation.
3. Extract one capability at a time into explicit Foundation modules while preserving exact tool outputs.
4. Keep portable algorithms independent from hosted file, argument, console, clock, environment, and process behavior.
5. Add module-level conformance suites, resource limits, ownership rules, and deterministic serialization tests.
6. Publish a compact Foundation surface only after at least two real consumers justify each shared abstraction.

The completion gate is a documented, versioned Foundation layer used by the assembler and linker on both hosts, not a speculative general-purpose standard library.

### Phase 8 - self-hosted compiler

The first slice is cross-host qualified at `d91dbfb` under Decision 0025: a streaming Windvale-written lexer over immutable UTF-8 bytes. It preserves the complete implemented Seed keyword/operator identities, byte spans, UTF-16-compatible source positions, integer classification, strict string validation, and bounded failures without introducing a token collection. This intentionally overlaps Phase 7: parser pressure, rather than a speculative library roadmap, will determine the next collection or diagnostic facility.

The declaration pass is cross-host qualified at `fc87a3e` under Decision 0026. It parses module headers and complete top-level declaration shapes into streaming immutable source views, then identifies balanced function-body spans for the later statement pass. It parses both the real lexer and its own declaration source without a token/declaration collection.

The body parser is cross-host qualified at `ddfa9e3` under Decision 0027. It reproduces the complete Stage 0 statement/expression grammar as flat parent/child source views, validates the lexer, declaration parser, and itself, and still retains no syntax collection. The parser evidence did not justify a token, declaration, syntax, or recoverable-diagnostic collection. Semantic binding is the next pressure test; it starts with bounded rescanning and may introduce a packed node/index facility only when measured correctness, ownership, or performance evidence requires one.

Semantic input pressure produced WVSS 1, cross-host qualified at `00ef0b1` under Decision 0029. This compiler-owned packed byte contract carries one root plus canonically ordered dependencies, provides indexed immutable source views, validates every member with the qualified frontend, and preserves dependency profile/shape rules without exposing host paths or collections. Windows and Debian pass all 43 tests, including the 64-module boundary and the real five-module frontend set, with matching normalized reports and byte-identical direct artifacts. Its current 4 MiB aggregate limit is explicit. The later complete compiler closure uses 677,073 source bytes, so this limit is sufficient for bytecode self-hosting; parity with the Stage 0 source-set envelope remains a separate future contract decision.

Decision 0030's Windvale-written import graph is cross-host qualified at `09c6f54`. It resolves exact module names, rejects repeated and missing imports, computes the complete root closure, and proves acyclicity over WVSS without host collections. Windows and Debian pass all 44 tests, including an exact 64-module/63-edge chain and the real seven-module compiler closure, with matching normalized reports and byte-identical direct artifacts. Declaration namespaces and signature/body binding remain the next semantic slice.

Decision 0033's Windvale-written declaration/signature phase is cross-host qualified at `d57a6d8`. It enforces global namespace and capability policy, binds visible nominal signature types, assigns canonical nominal indices, and publishes an independently validated `WVSD 1` declaration directory plus a bounded transitive-visibility matrix. A repeated-rescan prototype exhausted 4,000,000,000 instructions on the real closure; the retained packed evidence removes that impractical path. Windows and Debian pass all 45 tests and the complete native CLI verifier with matching normalized reports and 42 byte-identical direct artifacts. Body/local/call binding and typed expression/control-flow semantics are next.

Decision 0034's Windvale-written body/local/call phase is cross-host qualified at `9185b28`. It assigns stable parameter/local slots and scopes, binds reads and assignments, resolves visible constructors/functions/capabilities and Foundation intrinsics, checks arity, and publishes an independently validated `WVLB 1` directory. Measured temporary-directory and per-candidate source-slicing variants exceeded the fixed 4,000,000,000-instruction ceiling; the retained packed-span design binds the real nine-module closure within that ceiling. Windows and Debian pass all 46 tests and the complete native CLI verifier with matching normalized reports and 45 byte-identical direct artifacts. Complete expression types, field/operator validation, control-flow proof, and typed WIR are next.

Decision 0035's Windvale-written typed source IR is cross-host qualified at `bf77f70`. It performs complete implemented expression typing, field/operator/call validation, explicit basic-block and temporary construction, return and reachability proof, and publication through an independently checked `WVIR 1` directory. Windows and Debian pass all 47 tests and the complete native verifier with matching normalized reports and 48 byte-identical direct artifacts. The control-heavy fixture remains fast, while full ten-module self-lowering stays outside the development loop until local discovery and IR construction share one body traversal under the unchanged instruction ceiling.

Decision 0036's first Windvale-written WVB backend is cross-host qualified and published at `d65d286`; its tree is byte-identical to exact qualified candidate `ca56996`. Decision 0037 extends that backend with canonical WVSD-to-WVB function/data translation, arbitrary valid declaration ordering, `[i32]`, text and bytes data, deterministic escaped-Unicode literal interning, and the primitive Foundation intrinsic surface. Exact commit `636627c` is cross-host qualified: the original four-function fixture remains byte-identical to Stage 0 and executes with result `6`, while the interleaved data/text fixture is also byte-identical to Stage 0, includes a synthetic-name collision and surrogate-pair escape, and executes with result `13`. At that decision boundary, nominal types, capabilities, imports, multi-module translation, and full bootstrap closure remained later expansions; Decision 0038 closes the nominal-type part only.

Decision 0038 adds canonical WVB Types serialization, nominal shapes in functions and compiler temporaries, immutable record construction/field access, and enum constants/equality/inequality/names. Exact commit `f39ff73` is cross-host qualified: its deliberately interleaved nominal fixture is byte-identical to Stage 0 and executes with result `11`, while the preceding primitive and data/text fixtures retain their exact identities and results. At that decision boundary, capabilities, imports, multi-module backend translation, and full bootstrap closure remained later expansions; Decision 0039 closes the capability/profile part only.

Decision 0039 preserves portable/hosted/system profiles, serializes the exact seven-entry Seed capability catalog in canonical name order, translates WVSD capability identities, and lowers WVIR capability calls. Exact commit `98117c1` is cross-host qualified: its deliberately unsorted hosted fixture is byte-identical to Stage 0, exposes all seven call indices, and executes its authorized no-argument path with result `0` without file mutation. At that decision boundary, imports, multi-module backend translation, and full bootstrap closure remained later expansions; Decision 0040 closes the static multi-module part only.

Decision 0040 lowers a complete validated WVSS graph to one ordinary WVB without adding runtime linkage. It resolves every global WVSD identity through its owner source, internalizes dependency functions and nominal types, preserves root data/profile/capabilities/exports, and discovers text literals across canonical global function order. Exact commit `cb1db23` is cross-host qualified: its three-module fixture is byte-identical to Stage 0, verifies, exposes only `Main`, and returns `42`; noncanonical dependency order produces no output. Source-envelope/performance closure and full compiler bootstrap closure remain later work.

Decision 0041 fuses parameter/local WVLB discovery with typed-WVIR construction in one successful-path statement traversal while preserving the standalone binding API and binding-error diagnostic oracles. Exact commit `b124115` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The exact ten-module typed-IR input still reaches bounded diagnostic `WVR3011` at the unchanged 4,000,000,000-instruction ceiling, so remaining lookup/typed-lowering performance and full compiler self-hosting remain later work.

Decision 0042 bounds keyword, ordinary-identifier, and Unicode-whitespace dispatch in the Windvale lexer and adds opt-in per-function instruction reporting to the C# reference runtime and CLI. Exact commit `5d67463` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The original fixed lexer workload falls by 28.2%, and the focused typed-WVIR workload falls by 29.0%. The exact ten-module input still reaches `WVR3011` at 4,000,000,000 instructions; structural symbol-directory and name-evidence work is next.

Decision 0050 keeps public `WVSD 1.0` unchanged and advances the private `WVSI` index to 1.1 with deterministic mappings between source-order directory entries and canonical nominal ordinals. Binding and typed-WVIR consumers use those mappings directly, packed directory scans avoid unsuccessful match materialization, and equality paths reject unequal byte lengths before comparison. The real nine-module binding closure falls from 2,972,056,275 to 2,600,859,185 instructions despite a larger source-derived workload. Exact commit `e37204f` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The ten-module typed-WVIR input still exceeds four billion; repeated lexical/parser traversal is the next measured performance slice.

Decision 0055 reuses complete-source lexical and declaration evidence inside the compiler, retains checked standalone boundaries, replaces valid function-body token skipping with a bounded string/comment/brace scanner, contains over-deep checked body spans iteratively, and narrows nominal lookup through existing WVSI canonical ranges. The focused typed-WVIR fixture falls from 5,715,847 to 3,626,693 instructions. Exact commit `1a4fca7` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The exact ten-module typed-WVIR input completes at 3,912,239,584 instructions under the unchanged four-billion ceiling, clearing the performance entry gate for Stage 0 → Stage 1 → Stage 2 convergence without yet claiming self-hosting.

Decision 0058 implements and qualifies reproducible compiler bootstrap at exact commit `5c16547`. Equality-only source lookups use reverse span equality, WVB emission builds immutable canonical entry/rank tables once per declaration kind, and accepted declaration offsets are consumed without rescanning module prefixes for line/column coordinates. The canonical 12-module inventory contains 677,073 source bytes. Stage 0 produces a verified 599,868-byte Stage 1 compiler; Stage 1 compiles the same inventory in 6,700,562,174 VM instructions and produces a verified, byte-identical Stage 2 compiler. Both artifacts have SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`. The dedicated verifier reconstructed this proof from the exact committed inventory on Windows and isolated Debian QA; both ordinary qualification suites, normalized reports, and all 61 portable artifacts also matched. This completes the Phase 8 bytecode self-hosting gate while leaving Decision 0057's native execution and .NET-retirement work to Phases 9 and 10.

1. Freeze the meaningful compiler subset required to compile its own lexer, parser, semantic model, and bytecode encoder.
2. Add language facilities only from concrete compiler pressure: likely bounded collections, richer aggregates, explicit result/error flow, and controlled memory ownership.
3. Build a Windvale lexer and parser that reproduce Stage 0 syntax decisions over the accepted subset.
4. Build name/type/control-flow semantics and typed WIR construction with independent validation.
5. Emit canonical WVB and compare decoded structure, verifier results, runtime behavior, and exact bytes where canonicalization promises equality.
6. Compile the compiler with Stage 0, compile it again with the Windvale compiler, and compare the defined self-hosting artifacts.
7. Preserve the C# implementation as the reference/recovery compiler through convergence and the native-retirement gate. Under Decision 0213, freeze its forward source semantics at the next qualified WVB 1.11 baseline rather than implementing later language features twice. Under Decision 0178, accumulate its exact source, dependencies, instructions, identities, and milestone evidence gradually, then produce one complete clean dual-host recovery release before it leaves normal automation.

The completion gate is reproducible compiler self-hosting on Windows and Debian, including a clean-environment recovery procedure and exact dependency inventory.

### Phase 9 - shared native backend

1. Define the x86-64 calling convention, value representation, stack discipline, register ownership, traps, and portable/native semantic equivalence rules.
2. Define a structured native machine-IR or fragment boundary whose instruction selection, register assignment, encoding, and typed patches can serve WVO/AOT and in-memory JIT sinks.
3. Extend WIR, WVB lowering, and WVA only with operations demanded by measured native cases, including internal control flow, calls, data addressing, runtime services, and address materialization.
4. Lower a small verified pure WVB subset and the matching typed-WIR subset to WVO through the same object contract used by handwritten assembly.
5. Implement the first low-latency baseline-JIT experiment with WVA-generated machine stencils or another explicitly accepted mechanism, writable-or-executable publication, checked in-memory relocation, and bounded code-cache accounting.
6. Add PE/COFF, ELF, and later Windvale-native container output through explicit linker/loader target adapters rather than host conditionals in portable code.
7. Differentially run the same programs in the verified interpreter, baseline JIT, native sandbox, and AOT image, comparing acceptance, results, output, diagnostics, traps, capabilities, and defined resource counters.
8. Add content-addressed native caching, lazy compilation, compact micro-operations, an optimizing tier, or profile-guided AOT only after the preceding baseline supplies measurements and stable safety boundaries.
9. Expand through integers, calls, aggregates, memory, text, bytes, hosted bridges, and reclamation only after each preceding slice is qualified.

[Decision 0049](../Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) supplies an early bounded instance of steps 1, 3, and 4 for the special kernel-entry target: typed WIR lowers to verified code-only WVO, obeys handoff version 1, and links into the explicit UEFI adapter. It deliberately does not satisfy this phase's general ABI or bytecode/native differential gate.

[Decision 0059](../Decisions/0059-First-Shared-Native-Wvb-Slice.md) implements the first general instance of steps 2, 4, 5, and 7 for one constant-return program. A verified portable WVB lowers to explicit native operations, one versioned x86-64 fragment feeds both WVO/AOT and in-memory sinks, the runtime publishes memory writable-then-executable, and interpreter/JIT/AOT results agree on Windows and Debian x64 at exact commit `962bb85`. The 79-byte WVO and six code bytes are deterministic. Every wider operation remains open, so Phase 9 is not complete.

[Decision 0060](../Decisions/0060-Checked-Native-I32-Arithmetic-And-Traps.md) adds the first checked computation and recoverable native trap. Verified straight-line add, subtract, multiply, and negate lower through numbered machine-IR values into one bounded x86-64 frame. `jo` reaches a checked epilogue that returns packed overflow status without a host signal; the runtime translates it to `WVR3007`. The independent fragment decoder admits only the exact allowed instructions, initialized contiguous slots, overflow targets, and balanced epilogues. Exact commit `84dd908` is qualified on Windows and Debian x64: all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical. Boolean comparisons and structured control flow are the next Phase 9 slice.

[Decision 0061](../Decisions/0061-Typed-Native-Blocks-And-Forward-Control-Flow.md) replaces the straight-line operation list with typed locals, typed static values, canonical blocks, and explicit terminators. It lowers all signed i32 comparisons plus bool equality/inequality/negation, forward branches, early returns, and mutable frame-backed locals through the same WVO/AOT and W^X fragment. The strict decoder proves complete frame initialization, admitted instruction groups, forward boundary targets, reachability, and balanced exits. Exact commit `f0a53a9` is qualified on Windows and Debian x64: all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical. Backward edges intentionally fail until a native execution-budget or safe-point contract makes loops safe.

[Decision 0062](../Decisions/0062-Dynamic-Native-Instruction-Budgets-And-Backward-Control-Flow.md) gives each execution a positive dynamic instruction maximum and charges every lowered WVB instruction through a shared `RDX`/`R11` convention whose bytes are identical under Windows and System V x64. Packed status 2 maps to `WVR3011`; all control targets land on charge boundaries; cyclic reachability and both trap epilogues are independently decoded. Exact commit `2b67c8a` is qualified on Windows and Debian x64: finite JIT/AOT loops agree with the reference interpreter at the success and exhaustion boundary, a nonterminating loop is bounded, all 49 tests pass, normalized reports match, and all 61 portable artifacts are byte-identical.

[Decision 0063](../Decisions/0063-Shared-Budget-Native-Calls-And-Static-Data.md) extends that shared counter across a real function graph and adds a separate exact call-depth counter. The version-5 selector supports as many as four i32/bool parameters and results, nested and recursive calls, immutable i32 array length/load operations, recoverable depth and bounds traps, RIP-relative data patches, and deterministic WVO `.rodata`. One strict decoder verifies all functions, call edges, counter transitions, trap propagation, patches, symbol ranges, and reachable bytes before either sink. Exact commit `1af2eca` is qualified on Windows and Debian x64: interpreter/JIT/AOT success and resource boundaries agree, all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical.

[Decision 0064](../Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md) adopts ABI 5 in the first downstream OS consumer. One ordinary portable module compiles to canonical verified WVB, then shared native WVO, and executes internal calls, a bounded loop, and immutable `.rodata` on the kernel-owned stack before the special system-profile Main may complete. Exact candidate `708242e` passes all 15 OS tests, the 48-test Development tier, the 49-test Standard tier, the pinned QEMU/OVMF environment check, and the complete version-7 boot gate. This is AOT consumption and does not yet provide an in-guest WVB verifier or runtime loader.

[Decision 0065](../Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) advances the qualified target to ABI 6. One 32-byte versioned context replaces positional resource arguments and carries an optional 16-byte service table. The first closed service lowers immutable static UTF-8 through `console.write_line`, requires explicit authorization and implementation before W^X publication, uses identical generated bytes plus tiny runtime-owned Windows/System V thunks, and contains service failure as a packed trap. The OS bridge constructs the same context with an empty service table. Exact candidate `2fcf531` passes all 50 tests and complete CLI verification on Windows and Debian, byte-identical portable-artifact comparison, all 15 OS tests, and the pinned version-8 QEMU gate.

[Decision 0066](../Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) qualifies ABI 7's first compiler-tool data representation: immutable module bytes become pointer/length descriptors in zero-initialized 16-byte value cells; bounded slicing and fixed-width little-endian reads return `WVR3008` instead of a host fault; and `u8`/`u32` constants, comparisons, conversion, and checked arithmetic share the JIT/WVO selector. Up to four internal parameters may now include borrowed bytes, copied into the callee frame. The independent decoder rejects corrupt descriptors, argument forms, bounds branches, and scalar retyping. Exact candidate `8d375bf` passes complete Windows/Debian qualification, byte-identical portable-artifact comparison, all 15 OS tests on both hosts, and the pinned firmware-probe-9 QEMU gate.

[Decision 0067](../Decisions/0067-Borrowed-Hosted-Input-And-First-Native-Wvb-Inspector.md) qualifies ABI 8's first hosted input boundary at exact candidate `d970c27`. Borrowed text and bytes share an execution-bounded descriptor shape; service-table version 2 admits explicitly authorized argument count, argument text, file snapshot input, and console output. The checked-in `Wvb-Header-Inspector.wv` reads a real compiler-produced WVB and validates `WVB1`/version `1.6` identically under the reference interpreter and real Windows/System V W^X paths. All 52 tests and 61 portable artifacts agree across Windows and Debian, and firmware probe 10 passes pinned QEMU. Full `wvdump` still needs native nominal aggregates, bounded dynamic text formatting, and diagnostic policy.

[Decision 0068](../Decisions/0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md) qualifies ABI 9 at exact candidate `7edc243`. Enums use canonical dword values; immutable records use checked offsets in one 1 MiB execution arena; service-table version 3 adds pure strict UTF-8 validation. The existing structural portion of `Wv-Dump-Core.wv` validates complete envelope and payload fixtures identically under the reference interpreter, Windows/Linux W^X JIT, and linked WVO/AOT. Both hosts pass all 54 tests and all 15 OS tests, normalized contracts match, all 61 portable artifacts are byte-identical, and firmware probe 11 passes pinned QEMU. Full `wvdump` output still needs bounded dynamic text, descriptor returns, void calls, and diagnostic policy.

[Decision 0069](../Decisions/0069-Dynamic-Native-Text-And-Complete-Wvdump.md) qualifies ABI 10 at exact commit `7979933`. Service-table version 4 plus a 16 MiB execution-owned text arena supply bounded enum names, integer formatting, concatenation, quoting, and diagnostics. Hidden verified result cells admit descriptor returns while preserving packed status; void calls use the same propagation path. The complete 1,441-line checked-in `Wv-Dump-Core.wv` produces byte-identical reports under the interpreter, Windows/Linux W^X JIT, and linked WVO/AOT. Malformed UTF-8, aggregate text exhaustion, and corrupted result conventions have deterministic negative coverage. Both hosts pass all 56 tests and all 15 OS tests, normalized contracts and all 61 portable artifacts match, and firmware probe 12 passes pinned QEMU on Windows. Native implementations of the closed runtime services are the next .NET-retirement slice.

[Decision 0070](../Decisions/0070-First-Runtime-Native-Utf8-Service.md) cross-host qualifies the first such leaf at exact commit `53cee69` without changing ABI 10: strict UTF-8 validation uses one exact 800-byte x86-64 implementation on Windows and Linux instead of a C# delegate and platform thunk. The managed decoder remains the oracle; both hosts pass all 56 tests and all 15 OS tests, their normalized contracts match, and all 61 portable artifacts are byte-identical. Native text-arena ownership is now required before formatting and concatenation can follow.

[Decision 0071](../Decisions/0071-Native-Text-Arena-And-Core-Text-Services.md) cross-host qualifies ABI 11/context 3 at exact commit `8888951`. The 72-byte context makes the 16 MiB text-arena base, capacity, one shared managed/native allocation cursor, and exact native failure detail explicit. `Textˉconcat`, `I32ˉformat`, and `U32ˉformat` now use deterministic platform-neutral native leaves rather than managed callbacks or Windows/System V adapters; enum naming and quoting deliberately remain managed. Both hosts pass all 56 tests and all 15 OS tests, normalized contracts and all 61 portable artifacts match, and pinned QEMU probe 13 passes on Windows.

[Decision 0072](../Decisions/0072-Final-Pure-Runtime-Native-Services.md) cross-host qualifies the final two deterministic pure-service leaves at exact commit `f97d221` without advancing ABI 11/context 3. `Enumˉname` uses a fixed exact native leaf plus a bounded, independently verified runtime-private `WVEN` block reconstructed from canonical fragment types. `Textˉquote` uses an exact two-pass strict-UTF-8 native leaf while retaining the existing UTF-16-code-unit escape semantics. All six pure runtime services are now qualified on Windows and Debian; both hosts pass all 56 Seed tests and all 15 OS tests, all portable artifacts match, and pinned QEMU retains exact probe 13. Hosted/capability adapters and Stage 0 construction/publication remain managed.

[Decision 0073](../Decisions/0073-Native-Argument-Table-And-Process-Input-Services.md) cross-host qualifies ABI 12/context 4 at exact commit `328e455`. One execution-owned immutable table captures at most 67 prevalidated strict-UTF-8 arguments; its pointer/count are appended to the context and every descriptor is independently rebuilt and checked before publication. Exact platform-neutral `process.argument_count` and checked `process.argument` leaves remove both managed callbacks and platform adapters without changing generated service-call shapes. Windows and Debian pass all 56 Seed tests and all 15 OS tests, their normalized reports and all 61 portable artifacts match, GitHub passes independently, and pinned-QEMU probe 14 passes on Windows.

[Decision 0074](../Decisions/0074-Native-Windows-And-Linux-Output-Services.md) cross-host qualifies ABI 13/context 5 at exact commit `66b273f`. One exact runtime-private output table carries explicit console and diagnostic handles. Fixed Windows `WriteFile` and Linux `write` leaves emit strict UTF-8 plus LF, complete partial writes, preserve native counters/context, and map rejected writes to `WVR3029`. Windows and Debian JIT plus linked WVO/AOT execution agree over separate channels, empty text, euro and supplementary Unicode. Both hosts pass all 56 Seed tests and all 15 OS tests, normalized reports and all 61 portable artifacts match, GitHub passes independently, and pinned-QEMU probe 15 passes on Windows.

[Decision 0076](../Decisions/0076-Native-Windows-And-Linux-File-Input.md) cross-host qualifies ABI 14/context 6 at exact commit `ef08619`. Exact Windows and Linux file-input leaves plus a checked runtime-private `WVFI` table remove the final managed native-service callback while retaining authorization, 64 ordinal first-success snapshots, the 4 MiB result bound, and stable `WVR302x` failures. Direct JIT and linked WVO/AOT execute real files without calling the supplied Stage 0 reader, including the complete Windvale `wvdump`. Windows and Debian pass all 57 Seed tests and all 15 OS tests, normalized reports and all 61 portable artifacts match, GitHub passes independently, and pinned-QEMU probe 16 passes on Windows.

[Decision 0077](../Decisions/0077-First-Windvale-Owned-Native-Stencil.md) cross-host qualifies the first active runtime-construction transfer at exact integrated commit `da59312`. A WVA source defines the exact five-byte `process.argument_count` shell plus one versioned, typed execution-context-offset patch; the Windvale-written assembler produces the retained canonical WVO, and the live native executor consumes it through a strict bounded loader. The final ABI-14 leaf remains byte-identical. Windows and Debian pass all 59 Seed tests and both complete qualification gates; normalized contracts and all 61 established portable artifacts match, both hosts pass all 15 OS tests, and pinned-QEMU probe 16 remains exact. This is the first qualified evidence for Phase 9 step 5's construction mechanism, not yet a general baseline JIT or .NET-retirement gate.

[Decision 0078](../Decisions/0078-Multi-Patch-Windvale-Native-Stencil.md) cross-host qualifies the measured second construction transfer at exact commit `50294d9`. `WVSP 2` carries eight strictly ordered one-byte locations with six named ABI meanings, including repeated service-detail and borrowed-text-length values. The Windvale-written assembler produces the 321-byte canonical object, the live runtime consumes it, and the final 70-byte leaf identity remains unchanged. Windows and Debian pass all 60 Seed tests and both complete qualification gates; normalized contracts and all 62 current portable artifacts match, both hosts pass all 15 OS tests, GitHub passes independently, and pinned-QEMU probe 16 remains exact.

[Decision 0079](../Decisions/0079-First-Windvale-Native-Stencil-Consumer.md) cross-host qualifies the first consumer transfer at exact commit `f3a4ba4`. A portable, capability-free Windvale module accepts only the two retained complete WVO/WVSP shapes, validates every byte, derives patch values from closed semantic kinds, and constructs immutable outputs. Its production-tied demo agrees through the reference interpreter, native JIT, and linked WVO/AOT and rejects one changed value at every byte position. Windows and Debian pass all 61 Seed tests and complete qualification gates; normalized contracts and all 64 portable artifacts match, both hosts pass all 15 OS tests, GitHub passes independently, and pinned-QEMU probe 16 remains exact. At that decision boundary, C# still supplied the live bytes because native invocation returned only `i32`; Decision 0080 closes that specific integration seam.

[Decision 0080](../Decisions/0080-Native-Byte-Result-And-Live-Stencil-Consumption.md) cross-host qualifies the bounded result seam and live integration at exact commit `f547af8`. `Main() -> bytes` uses one verified descriptor cell in physical `RCX` on both host ABIs and the existing hidden-result convention; the host accepts only exact static-data or committed-arena ranges and copies before teardown. A retained 21,447-byte WVB built from `Native-Stencil-Bridge.wv` returns the exact 5-byte and 70-byte leaves, and the ordinary process-input service path now consumes those Windvale-produced bytes. ABI 14 and all previously accepted scalar code remain unchanged. Windows and Debian pass all 62 Seed tests and all 15 OS tests; normalized contracts and all 65 portable artifacts match; GitHub passes independently; and pinned-QEMU probe 16 remains exact. W^X publication/lifetime ownership is the next measured transfer.

[Decision 0082](../Decisions/0082-Windvale-Owned-Native-Publication-Layout.md) cross-host qualifies the first part of that transfer at exact commit `ba2cf69`. A portable Windvale core validates a strict bounded request and chooses the complete image extent plus canonical 16-byte service placements. Its retained hosted wrapper runs through the reference interpreter with only an in-memory file capability before allocation, and C# independently reconstructs every successful response. Windows and Debian pass all 63 Seed tests and all 17 OS tests; normalized reports and all 67 portable artifacts match; both pinned-QEMU probe-17 scenarios and GitHub pass. The executor now copies already-verified fragment patch bytes unchanged; Windows/Linux W^X calls and execution lifetime remain the next boundary.

[Decision 0083](../Decisions/0083-Windvale-Owned-Native-Publication-Lifetime.md) cross-host qualifies the next bounded transfer at exact commit `a898fe8`. Portable Windvale code emits the exact nine-transition publication graph, including deterministic release from every post-allocation partial state. C# independently reconstructs that table, and one internal executable-image owner now contains the raw address, actual state, and every Windows/Linux W^X call. Windows and Debian pass all 64 Seed tests and all 17 OS tests; normalized reports and all 69 portable artifacts match; both pinned-QEMU probe-17 scenarios retain their exact identities. Forged plans and invalid action order fail before the host operation. Context, service-table, arena, result-cell, native compiler execution, and standalone-container ownership remain open.

[Decision 0087](../Decisions/0087-Native-Windows-And-Linux-File-Output.md) cross-host qualifies the next real-compiler-driven slice at exact commit `12e9e2e`. ABI 15/context 7 appends service 12 and a checked runtime-private `WVFO` table; exact Windows and Linux leaves publish one bounded whole file without a managed callback. Exact compiler preflight advances from unsupported `file.write_bytes` to `WVN2002` in `Compilerˉbodyˉblockˉstepˉvalid`. Windows and Debian pass all 66 Seed tests and all 18 OS tests; normalized contracts and all 69 portable artifacts match; GitHub passes independently. Probe 20 supplies a zero file-output pointer in the service-free guest bridge while cross-host qualifying Decisions 0085 and 0086's WVA-owned Q35 shutdown and normalized trap entries through all three exact pinned-QEMU scenarios. Later exact inventory corrects the initial record-shape interpretation: this function returns an already-supported record but has eight parameters against ABI 15's four-register ceiling.

[Decision 0089](../Decisions/0089-Bounded-Native-Stack-Arguments.md) cross-host qualifies ABI 16 at exact commit `860c69c`. Four fast register positions remain byte-compatible; positions 4 through 63 use exact 16-byte outgoing cells bounded to 960 bytes. The independent decoder reconstructs every cell and type, adjusted hidden result, call, release, and callee edge. Interpreter/JIT/linked-WVO evidence passes the 64-parameter maximum plus stack descriptors and void calls, rejects targeted corruption, and remains green through the complete Windows/Debian gate. Exact compiler preflight now passes all functions with five through 23 parameters and advances to `Compilerˉsourceˉwirˉcompileˉblock`, whose 1,049 locals exceed the current 1,024-slot frame cap.

Qualified [Decision 0099](../Decisions/0099-Bounded-Native-Frame-Admission.md) advances the current backend to ABI 17 at exact implementation commit `4a077ab`. It retains one independently verified 16-byte cell per local/value while doubling the hard envelope to 2,048 cells. Exact compiler preflight clears the former 1,049-local failure and reaches `Compilerˉbodyˉparseˉprimary`, which requests the first disallowed combined slot at 2,049. This is not yet native compiler execution.

Qualified [Decision 0105](../Decisions/0105-Typed-Block-Scoped-Native-Value-Slots.md) advances the backend to ABI 18 without increasing that physical ceiling. Global semantic value IDs remain canonical, while exact-type physical cells are reused across basic blocks because machine IR already requires an empty operand stack at every edge. The selector independently reconstructs the map and rejects cross-block operands; the fragment decoder admits safe later redefinitions while retaining current-source alias and descriptor-provenance checks. Exact compiler preflight clears slot 2,049 and reaches unsupported `Bytesˉfromˉu8` in `Compilerˉcompileˉsourceˉwvb`. Exact implementation commit `484c228` passes complete Windows/Debian qualification in GitHub run 30762156220.

Qualified [Decision 0108](../Decisions/0108-Native-One-Byte-Construction.md) implements ABI 19's exact `Bytesˉfromˉu8` machine-IR, selector, arena, and independent decoder shape while retaining ABI 18's typed physical map, 2,048-cell ceiling, context, service table, and call convention. Interpreter, W^X JIT, and linked WVO/AOT agree at both `u8` boundaries; corrupt widths and aliases fail closed. Exact compiler preflight advances to `Bytesˉfromˉu16ˉlittle` in the same function. Exact implementation commit `a35c348` passes complete Windows/Debian qualification in GitHub run 30764320109 while all four retained pinned-QEMU scenarios keep their exact identities.

Qualified [Decision 0109](../Decisions/0109-Native-Two-Byte-Little-Endian-Construction.md) advances the implementation to ABI 20. `Bytesˉfromˉu16ˉlittle` checks its `u32` source before allocation, writes an exact length-two arena-backed descriptor, and maps values above 65,535 to `WVR3016`. Interpreter, W^X JIT, and linked WVO/AOT cover both valid boundaries and the first invalid value; the independent decoder rejects corrupt guards, widths, failures, and aliases. Exact compiler preflight now clears every observed operation boundary, completes selection, and measures a deterministic 4,556,121-byte fragment against the retained 1,048,576-byte limit. Exact implementation commit `a63ca0f` passes complete Windows/Debian qualification in GitHub run 30766123518.

Qualified [Decision 0111](../Decisions/0111-Bounded-Exact-Compiler-Fragment-Publication.md) attributes 4,555,263 bytes to 328 functions and 191,632 machine-IR operations, leaving only 858 alignment/data bytes. The 48,578 zeroed frame slots account for at most 1,360,840 emitted bytes, so local frame compaction cannot restore the 1 MiB ceiling. The 8 MiB fragment bound remains below the qualified 34 MiB publication image, keeps ABI 20 and both planner format versions, and admits the exact compiler through independent decoding and W^X publication. Execution now deterministically reaches the retained 1 MiB immutable-record arena as `WVR3017` before output; WVO and flat-linker limits remain separate 4 MiB boundaries. Exact commit `e139e4e` passes complete Windows/Debian qualification in GitHub run 30768107059.

Qualified [Decision 0112](../Decisions/0112-Bounded-Exact-Compiler-Record-Arena.md) measures that next boundary before changing it. The exact compiler consumes 1,480,096 record-arena bytes and 4,340,388 text-arena bytes while compiling the existing function-only fixture. Raising only the host executor's record capacity to 2 MiB leaves 617,056 bytes of headroom and completes native execution with the exact 815-byte Stage 0 WVB, exact success output, and no diagnostics. The arena remains checked, monotonic, and execution-scoped; ABI 20, context 7, generated bytes, the OS profiles, and the separate 4 MiB WVO/linker ceilings do not change. Exact commit `bbec1ae` passes complete Windows/Debian qualification in GitHub run 30769250223.

Qualified [Decision 0115](../Decisions/0115-Exact-Compiler-Record-Lifetime-Pressure.md) measures the complete 12-module native bootstrap boundary. Diagnostic 64 MiB and 256 MiB arenas both exhaust, while a successful reference Stage 1 profile attributes at least 77,821,091 constructed fields to its 40 busiest functions—more than 1.24 GB under ABI 20's monotonic layout. The 2 MiB host limit therefore remains unchanged. A new opt-in semantic profiler and fast exact-inventory boundary test retain the evidence. The next compiler slice preserves nominal record shapes throughout native machine IR and derives bounded reusable storage before any ABI or allocator decision. Exact integration commit `05e5ef1` passes complete Windows/Debian qualification in GitHub run 30771491421.

Qualified [Decision 0117](../Decisions/0117-Nominal-Native-Record-Storage-Plan.md) completes that metadata and measurement slice without changing ABI 20 or selected bytes. Exact nominal identities now survive every native return, binding, value, field, and call edge and are independently compared with verified WVB. Control-flow liveness compacts 137,512 declared record-local field cells to 9,291 persistent cells across the compiler; within-block value liveness reduces an 88,669-cell coarse slot bound to 7,463 peak-live cells. The largest projected function is 1,489 cells, below the retained 2,048-cell frame limit. Exact implementation commit `57416d0` passes complete Windows/Debian qualification in GitHub run 30773327094. Decision 0118 closes exact offset publication; selector/decoder implementation and native reproduction remain the next ABI-21 work.

Qualified [Decision 0118](../Decisions/0118-Deterministic-Native-Record-Storage-Offsets.md) publishes complete absolute frame-cell maps for record locals and semantic results. One deterministic width-first interference allocator serves persistent CFG lifetimes and block-local result lifetimes; independent test code reconstructs both and proves region bounds plus pairwise separation. The exact compiler adds no scratch fragmentation, remains at a 1,489-cell maximum, and pins map digest `aff287fba46a840e454e4cc7bf4751d3152474caf09331a526f3730ba280816e` while ABI-20 machine bytes remain exact. Exact implementation commit `060cf48` passes complete Windows/Debian qualification in GitHub run 30774669075; selection and independent ABI-21 decoding are next.

Implemented [Decision 0119](../Decisions/0119-First-Windows-Console-Application.md) adds the first real host container without broadening portable semantics. `windvale compile --target windows-x64-console-v1` accepts only capability-free scalar fragments, reproduces them through WVO and the flat linker, and packages an import-free deterministic PE32+ with an independently verified startup/context boundary. Cross-host-qualified Decision 0133 rebuilds the shared selector through ABI 21 while retaining the initial 5,120-byte `Sum-Data.wv` executable, which runs directly on Windows to result `29`; nominal records use frame-owned backing and leave the retained record arena unused. Decision 0150 subsequently demonstrates integrated native Stage 1-to-Stage 2 reproduction under ABI 22. Hosted services, the separate WVO/link metadata ceiling, standalone compiler packaging, and .NET retirement remain open.

Cross-host-qualified [Decision 0122](../Decisions/0122-First-Linux-Console-Application.md) adds the paired `linux-x64-console-v1` container over exactly the same verified fragment and flat-link evidence. Its deterministic 8,304-byte sectionless static-PIE ELF has an independently verified startup/context boundary, owns a bounded 64 MiB stack through direct `mmap`, and terminates through `exit` without an interpreter, libc, or .NET. Exact descendant `ea1aa89` directly executes and reproduces the paired Windows/Linux targets on Windows and digest-pinned Debian.

Cross-host-qualified [Decision 0124](../Decisions/0124-Paired-Wva-Console-Startup-Templates.md) transfers both exact startup templates into ordinary WVA instructions. Their independently assembled 98-byte PE and 158-byte ELF code sections expose exactly four typed `relative-i32` imports each; the container tests instantiate those records at final image addresses and compare every byte with the separately encoded C# recovery writers.

Cross-host-qualified [Decision 0127](../Decisions/0127-Windvale-Owned-Console-Application-Layout.md) moves the first live container-construction boundary into portable Windvale. One versioned 32-byte request produces a deterministic 108-byte plan containing every PE/ELF file extent, virtual extent, address, and native/startup placement. Both Stage 0 writers consume the digest-pinned Windvale result only after an independent checked C# reconstruction agrees field for field; their canonical executable bytes remain unchanged.

Cross-host-qualified [Decision 0130](../Decisions/0130-Windvale-Owned-Console-Application-Construction.md) moves exact PE/ELF byte construction into portable Windvale without raising the 4 MiB byte-value limit. Fixed 834-byte Windows and 4,454-byte Linux sparse recipes describe every literal, native-copy span, and zero gap even for maximum inputs. A bounded Stage 0 materializer validates the recipe and compares the complete file with a separate C# recovery writer before the existing untrusted-container verifier runs.

Cross-host-qualified [Decision 0132](../Decisions/0132-Windvale-Owned-Console-Application-Verification.md) closes that structural-verification seam. The portable verifier consumes one logical application through a 4 MiB first chunk plus at most 8,304 trailing bytes, regenerates the canonical construction recipe, verifies every container-owned literal and zero gap, and recovers the exact opaque native image and entry. Both maximum-size targets cross the chunk boundary without raising the value limit. The existing malformed PE/ELF corpora drive both the portable verifier and independently maintained C# parsers; exact descendant `ea1aa89` closes the former dual-host qualification requirement.

Cross-host-qualified [Decision 0133](../Decisions/0133-Frame-Owned-Direct-Native-Records.md) advances the shared host/OS selector and verifier to ABI 21. Record construction and local movement copy complete direct fields into deterministic frame backing; calls pass backing pointers; returns copy into caller-owned destinations. The independent decoder reconstructs these shapes, including the one-word record versus two-word descriptor stack distinction. The exact compiler's 1,489-cell map remains unchanged, its deterministic fragment becomes 16,905,513 bytes / SHA-256 `29a8b354e185fad4b4d8967ee8e263ce68cb9939373d91fb1e7919be887c8569`, and its function-only compilation consumes zero record-arena bytes. The full bootstrap clears `WVR3017` and reaches `WVR3018`, making dynamic text/byte lifetime the next measured blocker. The synchronized Stage 0 and Windvale fragment ceiling is 32 MiB under the unchanged 34 MiB image ceiling. Rebuilt Probe 32 retains its exact guest contract while `Executeˉmain` grows to 755 cells, the exact stack path grows to 24,240 bytes inside the same six pages, the linked client grows to 445,085 bytes and 109 RX pages, and record-arena use falls from 528 to zero. `WVKMEM11` supplies the resulting 120-page client root in a 141-page arena. Windows and Debian pass all 31 OS tests; all four Windows pinned-QEMU scenarios pass. WVO/linker 4 MiB limits remain separate AOT-container work.

Locally implemented [Decision 0136](../Decisions/0136-Exact-Compiler-Dynamic-Value-Pressure.md) profiles that next boundary without changing ABI 21 or its 16 MiB arena. The successful 6,700,562,174-instruction reference bootstrap constructs 1,852,773 allocation-bearing values representing 902,262,268 flat result bytes while reproducing the exact 599,868-byte compiler. `bytes.concat` accounts for 899,106,127 bytes, approximately 99.65% of the total; the largest rows are escaping WIR merges and repeated WIR/WVB emission. The next slice must preserve dynamic-backing identity and measure typed live roots across caller replacement and returns before choosing ownership-aware reuse, bounded chunked construction, or reclamation.

Locally implemented [Decision 0141](../Decisions/0141-Exact-Compiler-Dynamic-Value-Lifetime.md) preserves that identity through stacks, locals, calls, borrowed views, and direct records. The same 902,262,268 constructed bytes reduce to a 9,030,829-byte ideal live and allocation-operation peak across 17 unique backings, leaving 7,746,387 bytes inside the retained 16 MiB arena; all roots balance to zero and the canonical compiler remains byte-identical. This selects bounded ownership/reclamation as the first mechanism to evaluate while requiring an exact allocator replay for metadata and fragmentation before any ABI change.

Locally implemented [Decision 0143](../Decisions/0143-Bounded-First-Fit-Dynamic-Arena-Replay.md) completes that replay with a 16-byte in-band header, 16-byte alignment, address-ordered first fit, splitting, and immediate adjacent coalescing. All 1,852,773 exact-compiler allocations succeed; peak charged storage is 9,031,216 bytes across 17 blocks, maximum addressed extent is 10,700,368 bytes, external fragmentation peaks at 7,324,224 bytes, and completion recovers the full 16 MiB arena. The compiler remains byte-identical. The next slice is no longer capacity discovery: it must publish native machine-IR descriptor ownership and independently reconstruct deterministic retain, release, transfer, frame-cleanup, direct-record, call, and caller-owned-return actions before an ABI successor emits allocator operations.

Cross-host-qualified [Decision 0147](../Decisions/0147-Native-Descriptor-Ownership-Plan.md) closes that planning boundary without changing ABI 21 or its machine bytes. Every descriptor and direct-record descriptor field receives explicit borrowed or owned carrier semantics; 328 exact-compiler functions produce a deterministic 186,557-action map with acquisitions, aliases, replacements, last-use releases, call borrows, caller-owned result acceptance, callee transfer, and frame cleanup. A separately implemented oracle reconstructs every summary and ordered action before selection.

Cross-host-qualified [Decision 0148](../Decisions/0148-First-Wva-Native-Descriptor-Allocator-Leaf.md) adds the first executable reclaiming primitive without advancing that ABI. One digest-bound 2,989-byte WVA leaf implements bounded first fit, split/exact acquisition, reference counting, address-ordered release, and adjacent coalescing. Live W^X execution agrees byte for byte with an independent Stage 0 model across success, exhaustion, stale ownership, overflow, and corrupt-state cases on Windows and pinned Debian. The exact ownership map projects to 180,190 leaf calls plus 6,367 ownership movements.

Cross-host-qualified [Decision 0151](../Decisions/0151-Native-Descriptor-Allocator-Emission-Schedule.md) closes the remaining full-allocator pre-selection ambiguity. Every leaf call now has an independently reconstructed direct-frame or indirect-record owner location and one of five exact phases. Candidate context 8 appends allocator state and leaf pointers without shifting context-7 fields; 265 functions reserve three request cells; generated calls must preserve both live budget registers. The exact compiler splits 180,168 generated invocations from 22 runtime-service acquisitions and reaches a 1,492-cell maximum. Decision 0150's ABI-22 generation/checkpoint policy does not call this leaf; full integration must choose one high-word ownership representation and first prove owner-token copy/call selection plus deterministic reuse in a small successor fixture.

Cross-host-qualified [Decision 0150](../Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) advances the same shared selector and verifier to ABI 22 after Decision 0147 ownership-plan agreement. Generated byte builders carry checked capacity/generation headers and reuse only a valid arena-tail owner; non-entry descriptor functions save a verified arena checkpoint and reset or compact direct results without exposing ownership metadata. Scalar-only direct-record returns roll back, while descriptor-bearing aggregates remain unchanged until caller-liveness evidence makes relocation safe. The deterministic compiler fragment is 17,130,441 bytes / SHA-256 `af8db63675a2441e57a763ca4caa411419a84879cf01a1eb62b4be7556487cab`. It compiles all 12 canonical sources, peaks at 64,476,249 bytes in the 64 MiB host arena, and produces the byte-identical 599,868-byte Stage 2. The integrated Probe-34 normal client is 447,757 bytes and 110 RX pages; `WVKMEM13` supplies a 121-page client root beside the retained 11-page init/resource extent in a 144-page arena. Exact descendant `2591cd5` passes complete Windows and digest-pinned Debian qualification in GitHub Verify run 30797770080; all four pinned-QEMU scenarios pass on Windows. Standalone compiler packaging remains blocked separately by WVO/link metadata and hosted service serialization.

The same exact `50294d9` qualification integrates the Windvale-written Project 1 shell as another real ABI-14 consumer. It admits descriptor-bearing immutable records and the bounded byte construction used by the parser, then agrees across the interpreter, Windows/Linux JIT, and linked WVO/AOT over real manifests without a Stage 0 file-reader call. This is a contained step toward Phase 10's native build driver, not that driver itself: project-relative host resolution, source compilation, artifact publication, and standalone container metadata remain open.

The completion gate is deterministic native AOT output, a qualified baseline-JIT path, and interpreter/JIT/AOT semantic agreement for a documented WVB subset on Windows and Linux. Full language coverage and an optimizing tier are not required yet.

### Phase 10 - native host tools and .NET retirement

1. Produce native compiler, semantic WVB verifier, interpreter/baseline JIT, assembler, linker, inspector, test runner, and build-driver artifacts from the qualified backend.
2. Define the native value representation, allocation/reclamation boundary, runtime-service table, traps, process entry, and narrow Windows/Linux adapters for executable memory, files, arguments, diagnostics, and exit behavior.
3. Keep portable tool cores identical and test adapters through shared capability contracts and a Windvale-owned internal ABI with small platform thunks.
4. Rebuild representative artifacts with the .NET-hosted reference path and native Windvale tools, comparing every promised output; then prove Stage 1 and Stage 2 through the native path.
5. Run repository verification, packaging, and clean-environment recovery on both hosts without invoking .NET. Inventory every remaining system library, platform loader, firmware tool, or external build utility.
6. Complete the incrementally maintained Decision 0178 recovery record, archive the final qualified .NET Stage 0 release, and publish the native seed identity, provenance, previous-compiler bootstrap procedure, and rollback path.
7. Remove .NET from the normal build, test, packaging, release, and execution automation only when every Decision 0057 retirement condition passes from one committed source state.

[Decision 0156](../Decisions/0156-First-Standalone-Hosted-Console-Capability.md) cross-host qualifies the first capability-bearing standalone Windows/Linux application pair. The version-2 containers serialize and independently verify one `console.write_line` authority in `WVHC 1`, install the same ABI-22 runtime tables and native application bytes through canonical WVA startups, and execute directly on both hosts without loading .NET. The outer compiler and container packagers remain Stage 0 hosted; the next narrow boundary is the separate 4 MiB WVO/link limit required to package the already-reproducing compiler.

[Decision 0185](../Decisions/0185-Standalone-Compiler-Wvb-Verifier-Applications.md) implements the first native verifier slice. Current [Decision 0203](../Decisions/0203-Evolved-Compiler-Hosted-Tool-Capacity.md) synchronizes it with explicit imports and private-by-default declarations, advances its `u64` ceiling to 16,000,000,000 instructions, and pins the evolved 118,496-byte verifier plus deterministic Windows/Linux packages. `WVHV 1` still records exactly five application capabilities and six bound services, one of which is startup-internal UTF-8 validation; the runtime retains one read-only file snapshot and no file-output binding. Distinct canonical WVA startups, independently parsed PE/ELF containers, outer corruption coverage, exact-compiler admission, corrupted-candidate rejection, and direct child inspection remain integrated into the existing exact-compiler AOT test so that compiler construction is not repeated. Cross-host qualification is pending, and Stage 0 still owns the build, packagers, outer verifiers, and test orchestration.

[Decision 0186](../Decisions/0186-First-Windvale-Native-Compiler-Build-Driver.md) composes the compiler and that same portable verifier core into the Windvale build driver. Public `windows-x64-build-driver-v1` and `linux-x64-build-driver-v1` targets use distinct `WVHB 1` / format-5 identities while reusing the compiler-authority ABI, runtime, service bundle, and WVA startups. The evolved Decision 0187 profile compiles explicit or Project 1 sources, admits the candidate in memory, and makes its only output call after acceptance; compilation rejection preserves an existing output. Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` qualifies direct Windows/Linux execution without loading .NET. The write capability remains deliberately non-atomic.

[Decision 0187](../Decisions/0187-Project-Aware-Windvale-Native-Build-Driver.md) composes the existing portable Project 1 parser into that same driver without adding capabilities, runtime services, or another top-level compiler test. The canonical driver is now 749,460 bytes and accepts either explicit sources or a `.wvproj`; project mode retains the manifest plus at most 63 source snapshots, derives names beneath a canonical `/` resource prefix, rejects ASCII case aliases conservatively, reads each source once, and produces byte-identical three-module composition output in direct current-host execution. Malformed and duplicate projects preserve an existing output. Native source-to-PE/ELF remains blocked on Windvale ownership of the shared x64 backend rather than on container headers alone; cross-host qualification and atomic publication remain pending.

[Decision 0345](../Decisions/0345-Verifier-Scale-Native-Staged-Wvo-Publication.md) advances that backend transfer from the console packagers to the real verifier-scale staged chain. One Windows native producer/publisher run now owns the exact seven-chunk 1,049,615-byte WVO through independent verification and byte reconstruction. Linux execution and the publisher's measured 128 MiB self-lowering lifetime boundary remain open; neither is hidden by increasing a configured limit.

[Decision 0346](../Decisions/0346-Bounded-Native-Publisher-Self-Lowering.md)
advances the same transfer through exact current-host publisher self-lowering.
The earlier 128 MiB failure is resolved by bounded ownership and corrected
record/frame evidence rather than a larger limit. Linux execution, native
host-container reconstruction, promotion, and the final retirement gate remain
open.

The completion gate is a controlled and recoverable Windvale-native toolchain on Windows and Linux with no silent semantic fork, no normal .NET invocation, and matching native bootstrap evidence. This retires .NET as a dependency without erasing the Stage 0 historical record.

### Post-.NET-retirement language and library product lane (proposed)

The post-retirement product claim begins only after the complete Decision 0057 gate,
and the next numbered phase remains the boot and minimal-kernel path below. Package,
library, and application preparation may advance during Phase 10 when a direct
consumer exists and the work does not widen the frozen C# compiler. In parallel
with Phase 11, the proposed
[post-.NET-retirement language and library stage](Post-Dotnet-Retirement-Language-And-Libraries.md)
defines the product-facing application path: one useful package-backed application,
one portable library, one rights-limited platform library, deterministic lock and
package identities, and explicit capability approval/binding on Windows and Linux.
It recommends typed capability references and, separately, scoped ownership for
caller-closeable resources before new convenience syntax; then narrow result
propagation and one bounded associative collection when measured consumers require
them. It is documentation only until each contract has a focused decision and
evidence, and it does not delay or replace Phase 11.

### Phase 11 - boot path and minimal kernel

[Decisions 0084](../Decisions/0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md), [0173](../Decisions/0173-Windvale-Process-Service-And-Driver-Architecture.md), [0188](../Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md), and [0196](../Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) plus the [Windvale OS architecture](../Architecture/Windvale-Os-Architecture.md) fix the durable destination and current boundary: a small capability-oriented kernel written primarily in system-profile Windvale, a bounded WVA machine layer, one process/thread mechanism for applications, helpers, services, drivers, runtimes, and future VMMs, AOT kernel and low-level drivers, and isolated Windvale services with general JIT compilation outside privileged mode. Probe 39 supplies cross-host-qualified CPL3/capability/IPC/runtime evidence over three protected processes, a ready/wait dispatcher, and one private fixed-preemption experiment. Cross-host-qualified Probe 40 adds one independently lived, generation-safe client memory object. It is not a claim that general verification, loading, scheduling, physical-memory management, resource domains, launch plans, discovery, or supervision are complete.

1. Use the accepted Decision 0044 x86-64 UEFI 2.11, pinned QEMU Q35/TCG, and exact EDK II environment; record the first deterministic image and internal calling-convention decisions from boot evidence.
2. Make the linker produce the smallest bootable image format through a dedicated target adapter.
3. Boot to deterministic serial diagnostics, then add memory-map capture, page allocation, traps, and shutdown one bounded slice at a time.
4. Port the semantic WVB verifier and initial native interpreter behind system-profile capabilities rather than adding a kernel-specific language dialect. Keep later JIT compilation in user space or an isolated system service; kernel and driver code remain AOT.
5. Define the first package/resource source and load one embedded or image-contained verified module.
6. Automate QEMU success, failure, timeout, serial transcript, and image-digest evidence.
7. Qualify the accepted image under Hyper-V after QEMU automation is stable, documenting firmware or device differences explicitly.

Decisions 0044 through 0049 complete the environment, image, firmware-exit, handoff, and first compiler-generated boot slices. Decision 0052 completes the first memory part of step 3: firmware probe version 6 claims only one 64 KiB conventional-memory arena, exercises a zeroing page allocator, copies the handoff, and runs compiler-generated Main on an 8 KiB owned stack under exact QEMU evidence. Decisions 0054 and 0056 establish the bidirectional WVA/WV execution seam, move memory-through-Hello evidence into `.wv`, and assign future machine mechanics to WVA and kernel policy to Windvale source. Decisions 0064 through 0068 advance the qualified shared consumer through ABI 9 and firmware probe 11. Decision 0069 qualifies firmware probe 12's ABI-10/context-2 rebuild without exposing host services, text, or record arenas in the guest. Decision 0071 qualifies probe 13's ABI-11/context-3 rebuild. Decision 0073 qualifies probe 14's full ABI-12/context-4 shape with zero argument fields. Decision 0074 qualifies the service-free probe-15 rebuild with the complete ABI-13/context-5 shape and a zero output-table pointer. Decision 0076 qualifies probe 16's ABI-14/context-6 shape with zero file-input-table state.

[Decision 0081](../Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) cross-host qualifies the first trap part of step 3 at exact commit `ba2cf69`. The first allocated 4 KiB page holds a vector-6-only IDT built from live `CS` and the complete terminal-handler address after the kernel stack switch. The normal image returns through the existing Main chain and emits `cpu-exceptions=armed`; an explicit invalid-opcode image executes `UD2` after Main and terminates with exact panic evidence and QEMU host code 3. Both exact 17,920-byte images pass pinned QEMU. This is one terminal synchronous exception, not a general interrupt or recovery system. In-guest WVB verification/loading, general reclamation, paging, other exceptions, interrupts, clean shutdown, and Hyper-V remain open.

[Decision 0085](../Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) implements the shutdown part of step 3 for the pinned machine. WVA 1 gains only named `disable_interrupts`, `halt`, and `out_u16` mechanics, and `X64-Kernel-Shims.wva` owns an exact Q35 poweroff/retry adapter. The adapter is cross-host qualified through the pre-paging probe-20 baseline and retained unchanged in qualified probe 21. This target adapter does not claim ACPI discovery, Hyper-V or physical-machine shutdown, or process/service lifecycle coordination.

[Decision 0086](../Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) implements the first normalized-trap part of step 3. WVA gains exact `push_i32` stack-cell mechanics and owns entries for vector 6 without a CPU error code and vector 13 with one. Both reach one 40-byte ring-0 frame prefix and terminal handler. Exact commit `12e9e2e` qualifies the retained mechanics through composed probe 20: all 18 OS tests pass on Windows and Debian, and three exact 20,992-byte pinned-QEMU images prove clean shutdown plus normalized `(6, 0)` and `(13, 0)` terminal faults. Page faults, saved registers, IST/TSS, recovery, interrupts, and user-mode delivery remain separate work.

Qualified [Decision 0088](../Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) implements the page-table-ownership part of step 3. WVA gains only named NX/WP and CR3 activation operations; a bounded Stage 0 constructor allocates six zeroed pages and builds one low-1-GiB identity root with page zero absent, ordinary leaves writable/NX, and a fixed 64 KiB read-only/executable payload window. Exact commit `860c69c` passes all 21 OS tests on Windows and Debian and all three composed probe-21 pinned-QEMU scenarios under that root. This is not process isolation, page-fault recovery, or a general virtual-memory manager.

Qualified [Decision 0090](../Decisions/0090-First-In-Guest-Wvb-Admission.md) implements the next vertical slice as admission profile 1. A portable Windvale verifier checks the WVB 1.6 header, seven exact section envelopes, and every byte of one 174-byte canonical module; a 163-byte Stage 0 bridge calls its separately AOT-compiled form only after token 73. Exact commit `860c69c` passes Windows/Debian Qualification, all 21 OS tests on both hosts, and all three exact 47,104-byte pinned-QEMU scenarios. This resolves the bootstrap ordering question without claiming a general decoder, semantic verifier, guest loader, interpreter, process, or user mode. Decision 0091 below implements that next protected boundary.

[Decision 0091](../Decisions/0091-First-Protected-Windvale-Process.md) implements that next slice in probe 22. Windvale policy binds the exact admitted WVB, identities, budgets, capability slot/generation/rights, channel, and lifecycle token. A separate root maps one user RX code page plus RW/NX stack/data; a WVA entry executes the admitted AOT program at CPL3 and issues capability-checked send/receive/exit through `SYSCALL`. A second image executes privileged `CLI` at CPL3 and proves vector-13 containment while the two existing CPL0 fault scenarios remain terminal. All 25 focused OS tests, all six focused assembler tests, and all four pinned-QEMU scenarios pass on Windows; the retained composition is cross-host qualified through probe 25. The next architecture slice is the first minimal Windvale init/resource service using this boundary.

[Decision 0092](../Decisions/0092-First-Windvale-Init-Resource-Service.md) implements that init/resource slice in probe 23. A Windvale service under root 1 blocks with receive-only authority; the client under root 2 sends result 29 with send-only authority; the kernel-owned capacity-one channel wakes the service, which runs Windvale code and exits. The contained-client-fault scenario proves the service still completes after its peer faults. Exact commit `22e350b` passes cross-host build and Seed qualification; the retained composition is cross-host qualified with the OS suite through probe 25.

[Decision 0093](../Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) implements the first bounded runtime slice in cross-host-qualified probe 24. Process 2 runs an AOT-built portable Windvale interpreter, decodes the exact admitted 174-byte WVB subset, and sends interpreted result 29; the program's host-built AOT derivative is absent from the client link. Exact commit `190174a` passes all 67 Seed tests and all 25 OS tests on Windows and pinned Debian 12, with four retained Windows pinned-QEMU scenarios.

[Decision 0094](../Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) advances cross-host-qualified probe 25. The interpreter validates the module envelope, derives all seven section payloads, checks one bounded function/export profile, and still returns 29 when a second compiler-produced module moves its code payload. Runtime-supplied module loading, broader semantic coverage, JIT publication, scheduling, capability lifecycle, and resource namespaces remain later contracts.

Qualified [Decision 0095](../Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) advances probe 26. The hosted interpreter declares only `file.read_bytes`, fetches exact `boot:main.wvb` through a 199-byte WVA-owned ABI-16 leaf, and borrows the admitted bytes from a separate RO/NX process page. The WVB is absent from the interpreter WVB and linked RX image. Stage 0 still creates the fixed resource and publishes the verified stencil; general init/package ownership, transfer, loading, JIT publication, and scheduling remain later contracts.

Qualified [Decision 0096](../Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) advances probe 27. Init owns the admitted WVB page, its Windvale `Main` selects resource `1`, and process `2` begins with an absent target PTE and zero service pointers. One checked syscall installs a RO/NX alias, publishes the unchanged ABI-16 tables, and records a single borrow while init remains owner. Exact commit `4701200` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12; four Windows pinned-QEMU scenarios also pass.

Qualified [Decision 0097](../Decisions/0097-First-Terminal-Resource-Borrow-Revocation.md) advances probe 28. Ordinary client exit and contained client fault converge on one checked cleanup: the process machine accepts only the exact live leaf plus hardware accessed bit, clears the PTE and complete private service/resource publication, preserves init ownership and one historical grant, and reloads init's CR3. Exact commit `b2197fa` passes all 67 Seed tests and all 25 OS tests on Windows and Debian; all four Windows pinned-QEMU scenarios pass. Typed lookup, page/root reclamation, general transfer, loading, JIT publication, and scheduling remain later gates.

Qualified [Decision 0098](../Decisions/0098-First-Typed-Two-Resource-Lookup.md) advances probe 29. Init selects ordered set `(1,2)` containing the admitted WVB and a separate four-byte execution budget. `WVRES003` records, two distinct aliases, and `WVBR002` publish atomically; a WVA-owned leaf performs exact typed lookup; the Windvale interpreter charges one unit per opcode; exit and contained fault clear the complete pair. Pinned QEMU measured four kernel-stack pages as necessary; the arena is now 63 pages and remains exactly exhausted. Exact implementation commit `3fd9ef7` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12; all four Windows QEMU scenarios pass.

Qualified [Decision 0100](../Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) advances probe 30 at exact implementation commit `4a077ab`. Generation 1 runs and cleans the typed pair; memory 7 then accepts only its exact 42-page allocator tail, zeroes/releases it, and returns the same root to generation 2. `WVPROC09` and `WVRES004` generation-stamp identities and preserve grant history so stale generation-1 evidence cannot revive at the reused address. Init grants and receives twice, and both normal and contained-fault paths execute the client twice. Windows and digest-pinned Debian 12 each pass all 67 Seed tests and all 25 OS tests; all four Windows pinned-QEMU scenarios also pass.

Qualified [Decision 0101](../Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) advances Probe 31. The admitted resource is the exact 493-byte canonical WVB compiled from `Examples/Seed/Sum-Data.wv`: immutable data `[3,5,8,13]`, an internal `Add`, and a bounded loop execute 203 guest instructions to result `29`. Interpreter profile 5 runs those same bytes in both protected generations; memory 8 supplies a 137-page arena, paging 4 supplies a 768 KiB supervisor RX window, and `WVPROC10` bounds the 98-page client code plus 13-page stack. A 256-byte record arena fits inside the existing client data page and consumes exactly 240 bytes. The largest generated interpreter frame is 1,883 slots, so ABI 17's 2,048-slot compiler limit remains unchanged. Exact implementation commit `f3eca7c` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 in GitHub [Verify run 30753663882](https://github.com/eworker-inc/Windvale/actions/runs/30753663882); all four Windows pinned-QEMU scenarios pass.

Qualified [Decision 0103](../Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md) advances Probe 32 to the existing cross-compiler fixture `Tests/Fixtures/Source-Wvb/Function-Only.wv`. Stage 0 and the Windvale compiler produce the same exact 815-byte WVB. Interpreter profile 6 validates instruction boundaries and executes its four functions plus `bool`, `u8`, `u32`, and `i32` control flow for 199 guest instructions to result `6` in both rebuilt clients. Verified WVO call-graph preflight derives exactly 58,800 native stack bytes, so 15 pages are the minimal whole-page envelope; the largest individual frame is 1,900 slots under unchanged ABI 17. A 1,024-byte in-page record arena consumes exactly 528 bytes. Memory 9 supplies a 182-page arena and `WVPROC11` binds the 161-page client extent. Exact implementation commit `da93897` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 in GitHub [Verify run 30758910402](https://github.com/eworker-inc/Windvale/actions/runs/30758910402); all four Windows pinned-QEMU scenarios pass.

Qualified [Decision 0105](../Decisions/0105-Typed-Block-Scoped-Native-Value-Slots.md) keeps Probe 32's guest/runtime semantics and `WVPROC11` record shape but rebuilds it through ABI 18's typed block-scoped physical cells. `Executeˉmain` is 745 actual cells, the verified call graph consumes 23,824 bytes in a minimal six-page stack, the interpreter WVO is 418,372 bytes, and the client uses 102 code pages. `WVKMEM10` therefore shrinks the reclaimable client extent to 113 pages and the complete arena to 134 pages. Exact implementation commit `484c228` passes all 68 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 in GitHub [Verify run 30762156220](https://github.com/eworker-inc/Windvale/actions/runs/30762156220); all four Windows pinned-QEMU scenarios pass.

The completion gate is a reproducible VM image that boots, reports machine-readable status, runs a verified module, and shuts down cleanly. A desktop, network stack, and broad device support remain later work.

### Phase 12 - one module across three environments

1. Select one non-trivial portable module with deterministic inputs, output, failure behavior, and bounded resource use.
2. Package the exact same verified WVB bytes for Windows, Linux, and Windvale OS.
3. Run the module through equivalent Windvale-native capability contracts. Record interpreter, baseline-JIT, cached/install-time, or AOT mode explicitly rather than allowing the tier to change observable semantics.
4. Compare module digest, verifier result, return value, output bytes, diagnostics, native ABI/runtime versions, and defined resource counters.
5. Treat any host-specific observable difference as either a defect or a proposed contract change requiring a recorded decision.

The completion gate is the central Windvale portability proof: one module artifact, three environments, one specified result.

### Phase 13 - public foundation

1. Keep the accepted Windvale Community Source License, [E-Worker Inc](https://eworker.ca) stewardship, vendor-neutral AI authorship, public contribution foundation, and third-party notices visible in source distributions; [Decisions 0114](../Decisions/0114-Community-Source-Licensing-And-Commercial-Stewardship.md), [0031](../Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md), and [0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md) define the current policy.
2. Publish the recovery bootstrap, pinned prerequisites, artifact provenance, cross-host qualification procedure, and release manifests.
3. Apply the repository-wide AI-authorship default, recording a specific model or vendor only when technically material to reproducibility, qualification, or a third-party obligation.
4. Maintain the published contributor agreement, contribution, review, security, support, conduct, governance, and project-identity policies and the configured GitHub reporting, CLA, DCO, role, and branch settings. The unchanged history was imported privately under `eworker-inc/Windvale` before public visibility.
5. Audit parsers, verifiers, resource limits, capability authorization, hostile inputs, and reproducible builds against the public threat model.
6. Separate stable public contracts from experimental ones and label compatibility expectations precisely.
7. Prepare small tutorials that build from source language to bytecode, object, linked image, and the VM demonstration without hiding bootstrap dependencies.

The completion gate is a source release that another person can inspect, build, verify, and recover from documented inputs.

### Proposed integrated next-contract sequence

Proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md) connects the accepted future branches from qualified Probe 40 without turning them into current implementation claims. Its recommended main OS dependency line is:

1. retain qualified Probe 40 as the fixed timer and memory-object baseline;
2. add one flat resource domain with reserved recovery capacity and complete accounting over the existing processes and objects;
3. add one atomic clean-spawn transaction from separate semantic and kernel admission plans, generalizing object inventory or page selection only as required;
4. add bounded service supervision and isolate ordinary serial output;
5. qualify directional byte streams, typed terminal events, and one Shell-1 session;
6. add shared-memory/DMA/IOMMU mechanisms, copied `LinkPort 1`, and the minimal modern `virtio-net` profile; and
7. add protocols, secure identity, current TLS 1.3, and finally `WVTS/1`.

The [memory](../Architecture/Memory-Objects-And-Resource-Domains.md), [launch](../Architecture/Process-Launch-And-Supervision.md), [identity/trust](../Architecture/Identity-Time-Entropy-And-Trust.md), and [package/release](../Architecture/Packages-Releases-And-Recovery.md) guides define the proposed defaults. Language metadata, nominal variants/results, bounded sequences/builders, package manifests, lockfiles, release evidence, and native-retirement work may advance in parallel when a direct consumer exists. Review accepts or revises Decision 0198 before any proposed encoding is treated as product direction.

### Future branch - console, shell, and CLI

[Decision 0191](../Decisions/0191-Windvale-Console-Shell-And-Cli-Architecture.md) and the [console architecture guide](../Architecture/Console-Shell-And-Cli.md) accept this future product structure without making it an active implementation claim:

1. Build from qualified Probe 40; add a flat resource domain and clean dynamic launch before treating an interactive shell as reliable process infrastructure.
2. Isolate ordinary serial output while retaining the kernel emergency sink, then add one bounded serial-input adapter and terminal session.
3. Launch one single-session shell as an ordinary capability-restricted application through exact command resolution and an immutable launch plan.
4. Bind arguments, directional standard input, output, and diagnostic streams, current directory, optional environment, exact capability instances, resource ceilings, cancellation, supervision, and completion explicitly; report exact partial and indeterminate mutation progress and inherit none ambiently.
5. Keep Shell 1 to words, literal and escaped quoting, and `--`; add Shell 2 sequencing, bounded byte pipelines, redirection, and status chaining only with their underlying contracts, then add Shell 3 one-argument variables without splitting or execution.
6. Use ordinary verified Windvale programs for substantial automation; keep a language REPL separate and defer POSIX compatibility, typed pipelines, background jobs, graphical terminals, remote sessions, and multi-user login.
7. Keep inspection, filesystem, package, shutdown, and future VM operations as separate applications with exact capabilities rather than privileged shell built-ins.

The first completion gate is one QEMU serial session that survives malformed input and a failed command, resolves and launches an exact verified application inside a bounded resource domain, preserves separate output and diagnostics, reports structured completion, tears down every process and stream endpoint, and leaves the kernel emergency sink usable. Pipelines, filesystem redirection, history, background jobs, and remote access are not part of that first gate.

### Future branch - network stack

[Decision 0192](../Decisions/0192-Capability-Oriented-User-Space-Network-Stack.md) and the [network-stack architecture guide](../Architecture/Network-Stack.md) accept this future product structure without making it an active implementation claim:

1. Build from qualified Probe 40; add resource domains, dynamic launch, service supervision, PCI discovery, interrupts, shared memory, DMA/IOMMU ownership, and deterministic teardown before treating a NIC as an isolated recoverable service.
2. Implement packet parsers, serializers, checksums, route selection, virtual time, loopback, and a deterministic simulated link as capability-free or semantic code reusable on Windows and Linux.
3. Add copied `LinkPort 1` and one isolated modern `virtio-net` driver with fixed buffers, one RX/TX queue pair, the smallest feature set, bounded polling only for bring-up, interrupt-driven completion before usability is claimed, and an explicitly weaker label for any run lacking virtual-IOMMU containment.
4. Add Ethernet, ARP, static IPv4, ICMPv4, and UDP against an isolated deterministic peer before DHCP, DNS, TCP, TLS, applications, remote terminals, or public-Internet access.
5. Keep public address and transport types dual-stack; add IPv6 link-local addressing, ICMPv6, Neighbor Discovery, Duplicate Address Detection, SLAAC, and version-neutral routing before accepting a general host profile.
6. Add configuration, DHCPv4, DNS, address and route lifetimes, then bounded TCP with retransmission, congestion control, close, reset, peer loss, and complete timer/state teardown.
7. Bind resolve, connect, datagram, listen, accept, and later secure-connect operations as independently rights-limited semantic capabilities; do not expose ambient sockets, raw packets, capture, forwarding, or configuration to ordinary applications.
8. Begin with fixed pools and bounded copies. Add shared rings, batching, zero-copy, multiqueue, RSS, offloads, service sharding, and physical device breadth only when a measured workload justifies each complexity.
9. Qualify TLS 1.3 only after entropy, trust, peer identity, key protection, civil-time policy, cryptographic test vectors, authorization, revocation, and bounded handshake behavior exist.
10. Add authenticated remote terminals, package and browser clients, VM virtual ports, bridges, routing, NAT, filtering, capture, QUIC, physical NICs, and Hyper-V adapters as later separately authorized gates.

The first completion gate is one pinned QEMU guest using a modern single-queue `virtio-net` device to exchange exact bounded Ethernet, ARP, static IPv4, ICMP, and UDP traffic with an isolated deterministic peer. Malformed packets, loss, duplication, reordering, delay, queue exhaustion, interrupt storms, link removal, driver fault, reset, DMA revocation, provider loss, and complete buffer reclamation are tested. This gate makes no TCP, DNS, TLS, remote-terminal, VM-network, or public-Internet claim.

### Future branch - simple remote terminal

[Decision 0193](../Decisions/0193-Simple-Windvale-Remote-Terminal-Protocol.md) and the [remote-terminal architecture guide](../Architecture/Remote-Terminal-Protocol.md) accept this later connection without making it an implementation claim:

1. Complete the local terminal service, one shell, immutable launch, standard streams, typed cancellation, structured completion, and resource-domain teardown before adding a remote adapter.
2. Complete TCP listen/connect, secure entropy, current TLS 1.3, server and client identity verification, key protection, authorization, revocation, and bounded secure-stream closure.
3. Implement provisional `WVTS/1` as capability-free framing and state logic on Windows and Linux, including split/coalesced reads, malformed input, fixed bounds, illegal states, provider loss, and deterministic teardown.
4. Permit only in-memory, deterministic simulated, or build-restricted loopback carriers before TLS; provide no production plaintext listener or security downgrade.
5. Build one Windvale terminal client for Windows and Linux and prove it against a hosted reference adapter with pinned identities and one exact rights-limited policy.
6. Add one supervised Windvale OS adapter with exact listener, secure-stream, identity/authorization, terminal, launch, and resource-domain grants; neither the kernel nor shell parses the protocol.
7. Keep the first profile to one connection, one session, text, canonical keys, resize, interrupt, end-input, normal/diagnostic output, orderly close, structured completion, and bounded error.
8. Disable TLS early data; make disconnect tear down the session; defer multiplexing, detach/resume, forwarding, file transfer, graphical surfaces, SSH, WebSocket, and QUIC.

The first completion gate uses one pinned Windows or Linux client identity and one pinned Windvale OS server identity on an isolated network. It negotiates the exact protocol, creates one rights-limited session, exercises every first-profile message, reports structured completion, closes securely, and releases every process, endpoint, timer, buffer, listener grant, identity reference, and session generation. Unauthenticated, unauthorized, replayed, malformed, oversized, stalled, backpressured, and disconnected peers remain contained within exact budgets.

### Future branch - virtualization and accelerator hosting

[Decision 0171](../Decisions/0171-Future-Virtualization-And-Accelerator-Architecture.md) accepts this long-range structure without making it an active milestone or implementation claim:

1. Preserve pinned QEMU/Q35/TCG as the reproducible emulation oracle; add explicitly reported QEMU/KVM and QEMU/WHPX smoke lanes and direct Hyper-V compatibility only as separate evidence contracts. Prefer physical/root providers for baseline qualification; nested runs are optional developer-speed evidence and must record both hypervisor levels unless a later decision makes nesting the explicit feature under test.
2. Complete the prerequisite physical-memory, page-fault, interrupt, timer, scheduler, lifecycle, driver, and physical-hardware foundations before Windvale attempts to host an untrusted guest.
3. Add read-only CPU feature discovery, use nesting for rapid VMX/SVM development when available, then qualify one measured backend on physical Windvale hardware with one vCPU, private memory, no devices, and one terminal exit.
4. Move VM lifecycle, machine, firmware, exit, and device policy into an isolated Windvale VMM service while the kernel retains only privileged guest-memory, vCPU, interrupt, accounting, IOMMU, and teardown enforcement.
5. Qualify a minimal Windvale guest profile before adding a paravirtual performance profile or a UEFI/ACPI/PCIe compatibility profile.
6. Add bounded shared-memory console, timer, block, network, display, graphics, and compute transports one measured need at a time; keep compatibility emulation away from the performance data path.
7. Require separate capability grants for every image, storage, network, display, GPU, accelerator, partition, or passthrough attachment.
8. Permit software, paravirtual shared, hardware-partitioned, and exclusive-passthrough accelerator modes only with explicit ownership, budgets, isolation, reset, failure, and teardown guarantees.
9. Establish reservations, affinity, memory locality, batching, notification coalescing, bounded exit/interrupt rates, and host recovery capacity before permitting CPU or memory overcommit.
10. Treat the second x86 vendor backend, ordinary Linux/Windows guest compatibility, snapshots, migration, nested virtualization, confidential VMs, and live device reassignment as later independent gates.

The first completion gate is intentionally small: one physical Windvale host executes one device-free guest vCPU through one measured hardware backend, contains every exit within explicit limits, reports one exact terminal result, revokes all guest mappings, and returns to a healthy host. No GPU, passthrough, PC compatibility, or production-performance claim is part of that gate.

## Cross-cutting qualification rules

Every gate that changes portable semantics or serialized bytes must provide:

- An accepted or explicitly experimental contract with strict limits and ownership boundaries.
- Positive, boundary, malformed, adversarial, and determinism coverage proportional to its attack surface.
- Independent verification before execution or artifact publication.
- Exact Windows and real Debian evidence from the same committed source archive.
- Digests for compared source archives, reports, and binary artifacts.
- No timestamps, machine paths, locale, host newline conventions, or unordered host collections in canonical output.
- Updated current fixtures rather than compatibility readers for obsolete development formats.
- A short decision record when evidence changes architecture, semantics, or phase order.

Documentation-only planning changes require repository hygiene checks but do not manufacture qualification evidence. A milestone status changes to **Qualified** only after its implementation and cross-host evidence are committed.

## Decision checkpoints

The following choices are intentionally deferred until the preceding experiment supplies evidence:

- Assembly ergonomics wait until the Windvale assembler retires C# from the normal path; they then expand through a source mode or front-end that lowers to canonical WVA with source maps.
- Decision 0179 orders typed results, bounded sequences/builders, deterministic maps, measured floating point, and later structured concurrency; exact syntax and encodings still require consumers.
- Decision 0180 closes the representation split: canonical distributable WVB remains typed stack bytecode while typed stack-independent WIR remains the evolving compiler boundary. Versioned experimental formats may still break before the public stability decision.
- Decision 0057 accepts one shared native ABI/backend family for JIT and AOT. Decisions 0059 through 0087 supply qualified measured value, calling, machine-fragment, service-table, runtime-memory, single-/multi-patch stencil, portable Windvale-consumer, bounded byte-result, live-consumption, publication-layout, lifecycle-policy, and real-compiler-driven file-output evidence. Record-shaped function admission, branch/data stencil shapes, tier policy, cache identity, context/arena lifetime, native platform-call ownership, and host containers still wait for measured cases.
- Compiler folder names describe implementation roles rather than lifecycle status. `Compiler/Windvale` owns the Windvale-written implementation, `Compiler/Reference` owns the active C# Stage 0 reference/recovery implementation, and `Bootstrap` is reserved for the staged transition, provenance, and recovery process. This layout is cross-host qualified at `4fdc6bf`; calling the Windvale implementation a compiler does not claim that it is already self-hosting. After Decision 0057's retirement gate, a separate implementation change may archive or remove the C# project from normal automation without renaming Windvale's owned compiler around another lifecycle label.
- Assembler folder names describe implementation roles rather than maturity. `Assembler/Windvale` owns the qualified Windvale-written WVA implementation, `Assembler/Reference` owns the independent C# Stage 0 reference/recovery implementation, and `Examples/Assembler` retains only canonical WVA inputs. Decision 0051 changes ownership paths without changing WVA, WVO, assembly names, namespaces, module identities, or artifact contracts.
- Linker folder names describe implementation roles rather than target parity. `Linker/Windvale` owns the qualified Windvale-written flat-image implementation, `Linker/Reference` owns the independent C# Stage 0 reference/recovery implementation plus the currently C#-only UEFI target adapter, and `Examples/Linker` retains canonical WVA inputs. Decision 0053 changes ownership paths without changing linking, UEFI, assembly, namespace, module, or artifact contracts.
- UEFI PE32+ is the accepted first boot-container family. The first host containers are the separate capability-free `windows-x64-console-v1` PE and `linux-x64-console-v1` ELF slices; later hosted PE, ELF, and flat-image priorities must not redefine portable language behavior.
- Decision 0084 accepts the conceptual kernel/process/capability boundary. Decisions 0091 through qualified 0098 supply protected execution through two fixed typed names, an atomic ordered set, execution-budget policy data, and paired cleanup. Stable public syscalls, general IPC bytes, dynamic names/enumeration, transfer/reclamation, loader formats, scheduler policy, and compatibility remain deferred.
- Decision 0140 accepts per-part platform scope, explicit transitive capability approval, provider binding, and a small filesystem-core-plus-extensions direction. Exact source/module metadata, typed capability values, filesystem operations, and provider protocols remain implementation decisions requiring measured cases.
- Decision 0171 accepts a future provider-neutral VM-management boundary, a mechanism-only Windvale virtualization kernel layer, an isolated VMM/device-service layer, explicit GPU/AI-accelerator attachment modes, and performance-with-containment rules. It deliberately implements and qualifies none of them.
- Decision 0173 accepts one kernel process/thread mechanism with policy roles, separate process/thread lifecycle, flat aggregate resource domains, immutable clean-spawn launch plans, a minimal service manager, endpoint control planes, measured shared-memory data planes, staged single-CPU preemption, and exact driver teardown. Exact timer, allocator, object, transfer, registry, restart, and public ABI encodings remain measured implementation decisions.
- Decision 0181 selects the next logical OS records, TSC/HPET/APIC direction, bitmap allocator, output-only COM1 service, immutable directory discovery, capability sequence, first Windvale-named filesystem core, provider evidence, minimal VM profile, and exclusive-passthrough proof. Exact encodings, machines, and qualification remain open.
- Decision 0182 accepts an early experimental Windvale-native browser route, later .NET-free default gate, typed-WIR direct compilation, separate permanent-host/target decisions, bounded event stream, and Module Inspector sample without claiming implementation or permanence.
- Decision 0183 accepts immutable content-addressed packages and lockfiles, independent contract versioning, explicit time/entropy/network capabilities, trusted-release evidence, a maintained threat model, bounded observability, and x86-64-first qualification.
- Decision 0184 accepts staged syntax evolution: local inference, typed constants, and named records first; then structured control, module qualification, exhaustive enum match, payload variants/results, bounded collections, and scoped capabilities. Operators remain checked, same-type, non-overloadable, and explicitly separated between assignment, equality, arithmetic, Boolean, ordering, and future unsigned bitwise behavior.
- Decision 0191 accepts a device/terminal/shell/application split, immutable capability-bound command launch, explicit standard streams, a deliberately small shell grammar, structured completion, and an independent kernel emergency sink without claiming implementation.
- Decision 0192 accepts one bounded user-space network service behind an isolated link driver, a protocol-blind mechanism-only kernel boundary, standards-based dual-stack protocols, semantic rights-limited network capabilities, copied-first data planes, modern single-queue `virtio-net`, and deterministic staged qualification without claiming implementation.
- Decision 0193 accepts provisional `WVTS/1` over an authenticated secure ordered stream, with one connection owning one bounded session and shell resource domain, separate identity and authorization, typed terminal control, no production plaintext or replayable early data, disconnect teardown, and compatibility carriers deferred without claiming implementation.
- Public compatibility and support windows wait for the licensed release foundation.

At each checkpoint the project may keep, revise, or replace the proposed mechanism. It may not silently lower the verification gate or declare a narrower demonstration to be the original milestone.

## Current focus

Phase 6 through the self-hosted bytecode compiler remain qualified. Exact commit `ba2cf69` qualifies Decision 0081's terminal invalid-opcode boundary and Decision 0082's Windvale-owned executable-image layout; exact commit `a898fe8` qualifies Decision 0083's publication-lifetime graph; Decision 0084 records the accepted long-lived OS boundary.

Exact implementation descendant `a797e31` is the fully cross-host-qualified ABI-22 baseline through Probe 35. It retains Decisions 0147, 0150, and 0151, portable WebAssembly envelope verification, portable PE/ELF verification, and the bounded native-object/link admission from Decision 0160 while adding Decision 0159's checked guest directory service. GitHub Verify run 30808267999 passes all 87 Seed tests, all 37 OS tests, the golden contract, and the native CLI gate on Windows and digest-pinned Debian; all four Probe-35 QEMU scenarios pass on Windows. Hosted metadata, remaining native tools, repository workflow, caller-visible aggregate liveness, independent platform metadata, and writable filesystem behavior remain later work.

Cross-host-qualified [Decision 0165](../Decisions/0165-Contained-Windvale-Service-Failure.md) advances the OS branch to Probe 36 without changing ABI 22, the compiler, or `WVKMEM14`. `WVPROC15`/`WVCHAN04` accept one exact init-service fault only after a malformed live directory request, scrub the capacity-one channel, wake the blocked client once with transport status `-1`, and complete client resource revocation and clean shutdown. Exact commit `8c7f82a` passes all 87 Seed tests, all 38 OS tests, the golden contract, and native CLI gate on Windows and digest-pinned Debian in GitHub [Verify run 30812801520](https://github.com/eworker-inc/Windvale/actions/runs/30812801520); all five pinned Windows QEMU scenarios pass.

Cross-host-qualified [Decision 0172](../Decisions/0172-First-Kernel-Owned-Service-Endpoint.md) advances Probe 37 to `WVPROC16`. Process capabilities now resolve through one kernel-only `WVENDP01`, which validates provider/client generations, retains the existing `WVCHAN04`, rebinds across the exact client-root reuse, and closes once on provider exit or contained service fault. Exact commit `2a1461b` passes all 87 Seed and 38 OS tests on Windows and digest-pinned Debian; all five pinned Windows QEMU scenarios pass. The object is intentionally smaller than discovery: no names, registry, dynamic publication, replacement, restart, multi-client concurrency, scheduler, or VFS behavior is claimed without a concrete consumer.

Accepted [Decision 0173](../Decisions/0173-Windvale-Process-Service-And-Driver-Architecture.md) chooses the generalization path from the Probe-37 endpoint baseline. Cross-host-qualified [Decision 0176](../Decisions/0176-Third-Protected-Service-And-Ready-Wait-Dispatcher.md) completes its first bounded pressure: the immutable directory provider is a statically constructed third process, resource and directory traffic use separate endpoints, and a three-record ready/wait dispatcher owns every initial entry and explicit wake. Exact commit `aae6818` passes all 87 Seed and 38 OS tests on Windows and digest-pinned Debian; all five pinned Windows QEMU scenarios pass. Cross-host-qualified [Decision 0188](../Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md) then adds `WVKMEM16`, paging 5, and one private four-interrupt HPET/local-APIC proof across those roots without changing `WVPROC17` or ABI 22. Exact commit `6a250c8` passes all 87 Seed and 39 OS tests on Windows and digest-pinned Debian in GitHub Verify run 30847279400; all five pinned Windows QEMU scenarios pass. Cross-host-qualified [Decision 0196](../Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) advances to Probe 40 and `WVKMEM17`: portable Windvale fixes the policy invariant, and WVA first-fits, preflights, zeroes, releases, and rebuilds the client object while the later directory object remains live. Exact commit `c4008e7` passes all 87 Seed and 39 OS tests on Windows and digest-pinned Debian in GitHub Verify run 30853255559; all five pinned Windows QEMU scenarios pass. One flat resource domain, clean dynamic spawn, capability transfer, supervision, and the first isolated normal-console driver remain separate measured slices.

Decisions 0119, 0122, 0124, 0127, 0130, 0132, and 0133 form one cross-host-qualified paired console path at `ea1aa89`: deterministic PE32+ and static-PIE ELF containers, normalized process results, atomic publication, WVA startup templates, portable layout and sparse construction, portable completed-container verification, and independent C# recovery oracles. Decision 0150 advances their admitted fragment metadata to ABI 22 while retaining the canonical scalar PE/ELF identities and separately bounded 16 MiB container arenas.

Cross-host-qualified Decisions 0160, 0161, 0163, 0164, 0167, and 0168 carry the exact compiler through bounded large WVO/link admission, serialized capabilities and service adapters, bounded RW/NX runtime state, deterministic format-3 PE/ELF containers, and direct .NET-free canonical Stage 2 reproduction on Windows and digest-pinned Debian 12. Exact commit `db20fef` passes all 87 Seed tests, all 38 OS tests, the golden contract, and native CLI gate on both hosts in GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900); both report every pinned native, WVO, link, bundle, metadata, runtime, PE, ELF, and reproduced WVB identity unchanged.

Cross-host-qualified Decision 0169 exposes those same 17,157,120-byte Windows and 17,158,144-byte Linux applications as atomically published `windows-x64-console-v3` / `linux-x64-console-v3` targets without changing ordinary 4 MiB values or version-1/version-2 bytes. `windvale build Projects/Examples/Windvale-Compiler.wvproj` constructs the canonical WVB inventory once, and paired `windvale aot` calls package that exact verified module without redefining Project 1; the Stage 0 recovery runbook records its dependency and archive provenance. Exact commit `57d154c` passes all 87 Seed tests, all 38 OS tests, the golden contract, and native CLI gate on both hosts in GitHub [Verify run 30819768981](https://github.com/eworker-inc/Windvale/actions/runs/30819768981). The eventual final archived recovery release and much broader Decision 0057 retirement conditions remain open.

Implemented-candidate [Decision 0126](../Decisions/0126-First-Read-Only-Resource-Store.md) supplies the first concrete package/resource pressure after Probe 32. `WVRS 1` deterministically packages a real WVB, execution budget, and third configuration resource; an independent Stage 0 verifier and portable Windvale core agree on strict dynamic lookup and malformed rejection. The candidate deliberately does not change `WVPROC11`, `WVBR002`, the one-`u32` channel, or QEMU bytes. The next OS integration slice is now bounded request/reply IPC plus an independently lived resource-service capability, after which the existing `file.read_bytes` contract can consume `WVRS 1` inside the guest without putting filesystem policy in the kernel.

Implemented-candidate [Decision 0135](../Decisions/0135-Bounded-Guest-Resource-Request-Reply.md) advances that transport into Probe 33. `WVPROC12` / `WVCHAN02` add directional receive, call, and reply rights plus checked one-page RX-to-RW/NX copies; the larger shims still fit ABI 21's 109 RX pages, preserving the 120-page client extent, 141-page `WVKMEM11` arena, and same-root rebuild proof. Both client generations send the exact configuration request and validate the canonical 116-byte response before interpretation. All four Windows pinned-QEMU scenarios pass. The next filesystem slice is an independently lived immutable `WVRS 1` capability and dynamic guest lookup, not paths or block storage.

Cross-host-qualified [Decision 0142](../Decisions/0142-Immutable-Guest-Resource-Store.md) advances that resource into Probe 34. `WVKMEM12` expands the arena to 143 pages; `WVPROC13` gives init two RX pages plus a separate RO/NX 1,195-byte `WVRS 1` mapping; and a third `WVRES005` record binds the complete image identity without mapping it into either client. The init WVA seam validates the exact bounded three-entry profile, selects the requested opaque name dynamically, and constructs the response in RW/NX data. `WVCHAN03` clears retained message and destination state on terminal client exit/fault and records peer status before a checked generation-2 reopen. The client extent remains 120 pages and both rebuilt clients still interpret the admitted WVB to `6`. Exact descendant `2591cd5` passes all 31 OS tests on Windows and digest-pinned Debian; all four Windows pinned-QEMU scenarios pass. This is an immutable resource service, not path, directory, block-storage, writable, or crash-consistency semantics.

Cross-host-qualified [Decision 0159](../Decisions/0159-First-Guest-Directory-Service.md) advances this path to Probe 35. The 147-page `WVKMEM14` arena gives init a verified RO/NX `WVDS 1` page and dedicated RW/NX response page, gives each 122-page rebuilt client its own response page, publishes the address through `WVPROC14`, and records the init-only capability in a fourth `WVRES006` record. Syscalls 5 through 7 become service-generic without changing their numeric transport contract. Both client generations complete dynamic resource lookup, then validate one maximal 3,096-byte `WVDR 1` reply and all 3,072 file bytes before interpreting the retained WVB. Exact commit `a797e31` passes all 37 OS tests on Windows and digest-pinned Debian; all four pinned-QEMU scenarios pass on Windows.

Cross-host-qualified [Decision 0165](../Decisions/0165-Contained-Windvale-Service-Failure.md) advances the records to `WVPROC15` and `WVCHAN04` while retaining the same arena, mappings, ABI, interpreter, resource formats, and normal two-generation behavior. A fifth scenario completes the first resource exchange, delivers an inconsistent 37-byte directory request, contains init's resulting CPL3 general-protection fault, closes and clears the channel, wakes the waiting client with exact peer-loss result `-1`, and revokes the client grant before clean shutdown. The scenario is bound into the process image so mismatched coordinators are rejected. Exact commit `8c7f82a` passes all 87 Seed tests and all 38 OS tests on Windows and digest-pinned Debian; all five Windows pinned-QEMU scenarios pass.

Cross-host-qualified [Decision 0172](../Decisions/0172-First-Kernel-Owned-Service-Endpoint.md) advances to `WVPROC16` and inserts one kernel-only `WVENDP01` between the existing capability reference and `WVCHAN04`. Every capability-bearing syscall validates the exact endpoint identity, provider state, current process generation, and retained channel before mutation. The normal path resolves eight calls per client generation, rebinds the same endpoint for generation 2, and closes once on init exit; the service-fault path closes once with provider-fault status after six resolutions. Portable Windvale owns the policy model while Stage 0 retains explicit serialization and x86-64 replacement seams. Exact commit `2a1461b` passes all 87 Seed and 38 OS tests on both permanent hosts; five Windows QEMU scenarios pass.

Implemented-candidate [Decision 0129](../Decisions/0129-Bounded-Resource-Service-Request-Reply.md) supplies that protocol boundary without yet changing the guest. `WVRQ 1` and `WVRY 1` fit one 4 KiB message; a format-blind capacity-one exchange enforces directional rights, copied ownership, peer-exit clearing, and terminal close; portable Windvale validates the request and complete `WVRS 1` store before returning a digest-bound inline snapshot. A live hosted client/service exchange agrees byte-for-byte with the independent Stage 0 handler. The next OS slice is therefore the guest ABI itself: checked one-page buffer copies, wait/wake and service-death behavior, an immutable store capability, and one QEMU configuration lookup while the kernel remains ignorant of resource names.

Accepted [Decision 0140](../Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) supplies the application-library direction beyond that bounded resource proof. Capability-bearing libraries may be shared or explicitly platform-scoped; required authority follows requirement, application approval, grant, and provider binding; filesystem guarantees are split between an exact common core and instance-specific optional or native extensions.

Implemented-candidate [Decision 0145](../Decisions/0145-First-Capability-Bearing-Static-Library.md) takes the first bounded step without changing WVB or pretending the coarse profiles are durable platform metadata. Stage 0 admits capability-bearing dependencies only along monotonic profile edges and only when every importer explicitly redeclares its transitive requirements. Portable `WVRS 1` parsing moves to `Libraries/Foundation`; `Libraries/Platform` adds a hosted adapter that requires the existing bounded `file.read_bytes` leaf. Reordered dependency inputs produce identical WVB, the final catalog contains exactly one read requirement, explicit runtime authorization completes a live lookup, and missing root or intermediate approval fails with `WVC0013`. Independent platform scope, optional/versioned interfaces, typed capability values, and a real filesystem instance remain next work.
