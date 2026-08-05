# Decision 0213: Stage 0 semantic freeze and native front door

- Date: 2026-08-04
- Status: Accepted migration policy; semantic-freeze baseline qualified; normal-path cutover pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0178](0178-Project-Stewardship-Archives-And-Recovery.md), and Phase 10
- Builds on: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md), [Decision 0186](0186-First-Windvale-Native-Compiler-Build-Driver.md), and [Decision 0187](0187-Project-Aware-Windvale-Native-Build-Driver.md)

## Context

Windvale now has two different migration goals that need not finish together:

1. stop implementing each new source-language feature in both Windvale and C#; and
2. remove .NET from the complete normal build, test, packaging, and execution workflow.

The first goal can be reached before the complete native backend, runtime, test
suite, and recovery release satisfy Decision 0057. The Windvale compiler already
self-reproduces, and native Windows/Linux compiler, compiler-aligned verifier, and
project-aware build-driver applications exist. Continuing to evolve the C# source
frontend in lockstep would make a temporary recovery implementation a permanent
second compiler.

The evolved WVB 1.11 compiler and native front-door artifacts were qualified on
both permanent hosts at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`
in GitHub [Verify run 30964566192](https://github.com/eworker-inc/Windvale/actions/runs/30964566192).
That state is the retained semantic-freeze baseline. The existing C# implementation
remains correctable recovery evidence while the normal-path cutover continues.

## Decision

### Qualified freeze baseline

Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` is the first descendant
containing this decision to pass the complete Windows and digest-pinned Debian 12
Qualification gate. Each host passed all 97 Seed tests, all 39 OS tests, the golden
compiler contract, and the native CLI gate. The Windows job completed in 19m47s;
the Linux job completed in 23m50s. This activates the semantic freeze below: new
source-language behavior advances only through `Compiler/Windvale`, while
`Compiler/Reference` retains the bounded correction and recovery role defined here.

### Freeze Stage 0 source semantics at the next qualified baseline

The first descendant containing this decision that qualifies the current canonical
WVB 1.11 compiler, verifier, build driver, and required recovery evidence on Windows
and Linux becomes the exact Stage 0 semantic-freeze baseline.

Until that exact state is qualified, C# changes may correct the current candidate,
close malformed-input or security gaps, and preserve recovery. They must not start
an unrelated source-language expansion.

After qualification:

- `Compiler/Windvale` is the only forward implementation of Windvale source syntax,
  binding, typing, WIR construction, and WVB lowering.
- `Compiler/Reference` is a feature-frozen reference and recovery compiler. It may
  receive recovery, security, diagnostic-correctness, host-compatibility, and
  reproducibility fixes for its accepted baseline, but no new language feature is
  added merely to preserve frontend parity.
- The frozen overlap corpus continues to require exact or structurally defined
  C#/Windvale agreement. A future source fixture may explicitly be outside the
  frozen compiler's accepted surface.
- A new source feature must have Windvale-owned semantic tests, deterministic WVB,
  malformed-input coverage, and acceptance by an independent Windvale-owned
  verifier. It does not require a second C# implementation.

This is a semantic freeze, not immediate source deletion. The retained C# compiler
remains part of Stage 0 recovery evidence until the final archive is qualified.

### Make forward bytecode and ABI work Windvale-owned first

New WVB operations, verifier rules, native ABI behavior, or runtime semantics must
not be added to C# solely to act as a temporary bridge that will then be rewritten.
Their first production consumer must be Windvale-owned. A bounded C# change remains
permitted when it repairs the frozen oracle, independently reconstructs a format or
native artifact, or is required to qualify and archive the existing recovery
baseline.

If the current Windvale-native backend cannot execute a new operation, the feature
remains on an already supported portable/interpreted path or waits for the owned
backend boundary. The project does not widen Stage 0 first and transfer the same
semantic work later by default.

### Promote the native source-to-WVB front door in measured steps

Use this order:

1. qualify the current compiler, compiler-aligned verifier, project-aware build
   driver, and Windvale-owned x86-64 lowering evidence on Windows and Linux;
2. define a named atomic-replacement capability or a separate exact native
   publication step for accepted build output;
3. distribute exact native build-driver and verifier artifacts with source,
   manifests, digests, target identity, and recovery provenance;
4. change the documented and automated ordinary source-to-verified-WVB path to
   those native tools; and
5. retain the C# path in an explicit recovery/differential lane rather than the
   ordinary developer loop.

The normal-path switch happens only after the supplied native artifacts can be
identified and reconstructed without introducing undocumented binary trust.

### Preserve independent evidence without preserving duplicate product code

The C# implementation remains useful where independence is the evidence: frozen
source overlap, binary decoding, malformed-input rejection, native-fragment
reconstruction, container verification, and final recovery. New semantic breadth
uses specification fixtures, deterministic artifacts, previous-native-seed
bootstrap, Windvale verifier evidence, semantic transcripts, and hostile inputs.

Do not port the monolithic C# test harness line for line. Move reusable cases toward
versioned manifests and fixture data that native tools and the retained recovery
suite can consume independently.

## Retirement-gate status at adoption

| Decision 0057 condition | Status at this decision |
| --- | --- |
| 1. Complete compiler graph and Stage 1/Stage 2 comparison | Qualified for the evolved WVB 1.11 semantic-freeze baseline at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`. |
| 2. Native tools build, verify, test, link, package, and run | Partial: native compiler/verifier/build-driver applications and Windvale assembler/linker cores exist; general runtime, packaging, tests, and workflow replacement remain. |
| 3. Windvale-native decoder and verifier protect execution | Partial: compiler-aligned and bounded profile verifiers exist; one complete general native execution boundary remains open. |
| 4. Native runtime owns values, memory, traps, capabilities, entry, and adapters | Partial: ABI 22 and exact service leaves are substantial; general loader/runtime ownership and some host orchestration remain Stage 0. |
| 5. Shared native backend supplies deterministic AOT and baseline JIT | Partial: the qualified Stage 0 backend supplies both; the Windvale-owned selector covers a bounded ABI-22 subset and is not yet the complete toolchain backend. |
| 6. Interpreter/JIT/AOT differential evidence | Partial: extensive evidence exists, but accepted-subset coverage and ordinary native orchestration remain incomplete. |
| 7. Clean bootstrap from documented native seeds | Partial: exact native compiler artifacts and recovery instructions exist; a complete previous-native-release rebuild of the accepted toolchain remains open. |
| 8. Final .NET recovery release archived | Partial: evidence is accumulated incrementally; the final clean dual-host archive is not yet produced. |

No open condition is reclassified as complete by this decision.

## Consequences

- Source-language development stops paying the permanent cost of two forward
  compiler implementations after the freeze baseline qualifies.
- The next native milestone is the source-to-verified-WVB front door, not another
  unrelated fixed tool profile.
- C# remains present for recovery and independent evidence while its normal-path
  responsibilities shrink behind explicit gates.
- New bytecode or ABI work may wait for a Windvale-owned consumer instead of using
  Stage 0 as an easy but duplicative bridge.
- The final Decision 0057 gate, not this policy, authorizes removal of .NET from
  normal automation.

## Reconsideration triggers

Reconsider the freeze boundary if:

- the current Windvale compiler cannot be reconstructed from the selected native
  seed without extending the C# frontend;
- independent verifier evidence proves insufficient to contain future source or
  WVB changes;
- a security or recovery defect requires a deliberately broader Stage 0 correction;
  or
- dual-host qualification identifies a different exact freeze candidate.
