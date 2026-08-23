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

## Retained exact products

The following table is the last promoted profile-5 runner candidate. The current
source development checkpoint described below advances portable execution to
the launcher-owned budget entry through WVB 1.21 but has not repinned the paired
reconstruction inventory.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB runner | 183,537 | `1926cf33e359c56c8b457cbd96c685ffee052feb9f1330053c43d77e18f38d3e` |
| ABI-22 WVO | 1,808,213 | `dfcfb2360d496a5ab873539b4d6dbcdfe3824e8593dfe3e007cc71cd9bc55480` |
| linked fragment | 1,805,265 | `62985dc1a0090726b3b9e96810f6c37d476b1e9d8e54e9d85ce26c38d11689ab` |
| Windows application | 1,822,208 | `7a2f245b405d01c1f0f9c7f2b9e9cbe0d88370232e8cf1843616207aa155e7bd` |
| Linux application | 1,822,720 | `7dac00ed67f7622af2fcd4c9ededd17afced3ad54ea309d749320249188b15b4` |

The WVO contains 1,804,544 text bytes and 721 read-only-data bytes, with 71
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

The current source-built runner accepts WVB 1.11 through 1.21. Its shared scalar interpreter
implements the WVB 1.12 `i8`, `i16`, and `u16` family with the exact checked
overflow, division-by-zero, and shift traps from Decision 0768. The bounded
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
WVB 1.21 supplies one fresh opaque root-budget token to the sole parameter of
exported `Main(Memoryˉbudget) -> i32`. The interpreter validates that exact
shape placement before execution and rejects every bytecode load or store of
the token. The active invocation owns an identity/generation pair outside the
ordinary forgeable value vocabulary. On completed top-level return it verifies
the pair, zeros the parameter cell, and releases the invocation token exactly
once before publishing the result. A failed invocation is torn down as one
resource domain. The launcher's maximum-byte accounting is intentionally
unobservable until `Split` and allocation-lease operations land; no bytecode
constructor, general call transfer, or fallible allocation API is claimed by
this checkpoint.
Allocation metadata reuses inactive entries and first-fits released spans.
Text, bytes, aggregates, and nested collection elements remain outside this
checkpoint because their element-owned destruction and tracing are not yet
implemented. This low-level runtime surface does not claim the public fallible
`Memoryˉbudget` constructor or its `Result` lowering.
The same bounded scalar path executes the `u64` constant, arithmetic,
comparison, bitwise, shift, `bytes.from_u64_little`, and `u64.from_u32`
operations emitted by the compiler's exact floating-literal parser. The focused
Language 1.0 owner executes both the compiler front-end self-test and the
compiler-produced floating program through the retained candidate.

The current Windows development build is a 230,259-byte WVB at SHA-256
`d1393ec3cb83d95cf86902768893846e4dc0e5a742b46363c86e19712ec674ba`.
The promoted profile-5 application identity in the table above has not been
repinned to this source checkpoint.
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
