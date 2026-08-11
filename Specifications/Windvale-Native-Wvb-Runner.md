# Windvale native WVB-runner reconstruction

## Status and scope

The profile-5 WVB runner is a current-host-focused native candidate. It admits
the fixed portable `Main() -> i32` execution subset and binds five capabilities
to nine ordered services. The exact candidate reconstructs from the complete
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
| WVB runner | 121,593 | `e58f653445cd717d19c32fe1a0fbc57f03f475187cdec571825b9fd6685b3097` |
| ABI-22 WVO | 1,078,577 | `7d0ec719ade7e55d46c5a6dc6f7cb63102db4633172bcab1812e16651002106d` |
| linked fragment | 1,077,675 | `83dc076c137557495a24e65894c26c7f794e0d67f31dd59a476e1dc7715828d1` |
| Windows application | 1,094,656 | `6af8988f18c69a6757daeef8376c22ecbae406c31652813607fe2c3a6aa43ffc` |
| Linux application | 1,093,632 | `a674b455aecaec48889318fd190a2123bc8bc784b1ee9b9eaa76b491ebebcb2d` |

The WVO contains 1,077,216 text bytes and 459 read-only-data bytes, with 18
symbols and 13 relocations. Linking at base zero selects `Main` at address
14,790.

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

The three-case fixed owner proves exact candidate inventory, source-built
paired reconstruction, current-host result and instruction reporting, invalid
option rejection, malformed-module rejection, and input preservation. The
Windows owner passes 3/3 in 49.8 seconds. The paired 174-case native Seed
front-door helper builds 102 exact artifacts and passes one uninterrupted
current-Windows run in 1,197.8 seconds. The helper owns the four Foundation
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
WvDump self-test/valid/invalid execution, native construction of the canonical
WVO fixture, and its digest-bound verification and inspection,
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
