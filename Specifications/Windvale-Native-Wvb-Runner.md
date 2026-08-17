# Windvale native WVB-runner reconstruction

## Status and scope

The profile-5 WVB runner is a current-host-focused native candidate. It
preserves the fixed portable `Main() -> i32` execution command and additionally
owns the internal bounded scripting mode defined by
[Decision 0735](../Documents/Decisions/0735-Implement-The-First-Windvale-Scripting-Slice.md).
The outer runner binds five capabilities to nine ordered services. The exact
candidate reconstructs from the complete
Project 1 source closure through the Windvale-native compiler, lowerer, linker,
hosted-verifier profile, and paired Windows/Linux container materializers.

The project names its root tool plus the SHA-256, scalar-interpreter, envelope,
and formatting dependencies in canonical module order. Project paths are
relative to the manifest; this contract does not require all `.wvproj` files to
live at the repository root. Component-local manifests remain appropriate, and
a future workspace/index contract may improve discovery without changing
Project 1 semantics.

## Exact products

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB runner | 136,020 | `65cdb8a1ab0776dfb4da2c89a53e53dfa67072ff3e6958adc58c461e82d1a9d6` |
| ABI-22 WVO | 1,233,367 | `14011cc7879b2b876d5b2cf17db0ecc522a1b47d6a9eaf2e9d3773850a5c7553` |
| linked fragment | 1,231,745 | `30078a81a9412a135066c836fbfb9340e2ce05d8692cf2f67afc0fba27d7a314` |
| Windows application | 1,248,768 | `b30390b51542648f6e69b2078135f25b77cde18432e72b9137bdf6066e8c2f1d` |
| Linux application | 1,249,280 | `ab318af04fab63833d569787a0977d7239e0eb53a268e508f25823eb32c212cb` |

The WVO contains 1,227,856 text bytes and 721 read-only-data bytes, with 33
symbols and 27 relocations. Linking at base zero selects `Main` at address
60,426.

## Construction and execution

The paired constructors accept one existing output directory:

```text
Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd <existing-output-directory>
./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>
```

They reject the live candidate directory, bind both tool inventories and every
artifact digest, build the WVB from its source project, lower and link once,
assemble both inspector startup objects, then construct profile-5 Windows and
Linux applications. Success reports:

```text
native WVB runner reconstruction status=Complete artifacts=4
```

`Run-Wvb.cmd` and `Run-Wvb.sh` execute the corresponding digest-bound candidate
with either one module argument or the exact optional `--report-steps` flag.
The runner supplies the scalar interpreter with a fixed 1,000,000-instruction
budget, matching the Stage 0 CLI's default execution budget. Default output
remains `Result: <i32>`. Reporting adds one
`Instructions: <u32>` line; the canonical Sum fixture reports result `29` and
exactly `203` instructions.

The installed `wv run` composition invokes the same candidate through its
internal `--script <module.wvb> [argument ...]` mode only after an independent
complete-verifier pass. That mode uses `WVXI 4`, grants the four fixed scripting
base capabilities, replays the two bounded line-output buffers to their
separate outer sinks, and returns guest statuses from zero through 255 exactly.
It is an implementation boundary for `wv`; direct users should use the public
command contract in [`Windvale-Scripting.md`](Windvale-Scripting.md).

The three-case fixed owner proves exact candidate inventory, source-built
paired reconstruction, current-host result and instruction reporting, invalid
option rejection, malformed-module rejection, and input preservation. The
Windows owner passes 3/3 in 49.8 seconds. The paired 185-case native Seed
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
