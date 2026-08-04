# Decision 0203: Evolved-compiler hosted-tool capacity

- Status: Accepted; implemented locally
- Date: 2026-08-04
- Scope: compiler-aligned native verifier and build-driver integration
- Extends: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md), [Decision 0196](0196-Windvale-Compiler-Build-Driver-Applications.md), [Decision 0201](0201-Expanded-Exact-Compiler-Native-Capacity.md), and [Decision 0202](0202-Four-Phase-Compiler-Capacity-WebAssembly-Verification.md)
- Retains: `WVHV 1`, `WVHB 1`, native ABI 22, execution-context format 7, service-table format 5, the canonical verifier rules, fixed authority profiles, and active-development replacement without compatibility shims

## Context

The language-evolution batch makes import aliases explicit and imported declarations private by default. The retained native x64 adapter, compiler verifier, and build driver arrived concurrently from the integration base with implicit imports and two adapter-facing native declarations that were not public. Preserving that obsolete source behavior would contradict the early-development compatibility policy; these tools must consume the current language contract directly.

The evolved 859,555-byte compiler also exceeds the hosted verifier's retained 8,000,000,000-instruction ceiling. A reference diagnostic run reaches that ceiling before publishing a phase result. The independent compiler-capacity WebAssembly phases require about 11.59 billion instructions in aggregate for the same hosted compiler. WebAssembly must retain four separate `u32` meters, but `WVHV 1` already serializes a `u64` instruction budget and can keep one monolithic semantic, typed-execution, and control/reachability verifier.

Decision 0201 separately advances the shared hosted dynamic arena to 128 MiB. The one-snapshot verifier runtime therefore no longer fits its obsolete below-80-MiB test expectation even though it still retains exactly one input snapshot and no file-output scratch region.

## Decision

- Require explicit aliases in every repository-maintained Windvale import. Qualify imported references through those aliases.
- Export only the native x64 status enum and summary record needed by the public lowering adapter; do not restore implicit module-wide visibility.
- Advance the `WVHV 1` hosted verifier instruction ceiling from 8,000,000,000 to 16,000,000,000. Keep the metadata format, native ABI, execution context, service table, authority profile, and monolithic verifier entry unchanged.
- Retain the Decision 0201 128 MiB dynamic arena. Prove that the verifier still has one snapshot, a 32-byte snapshot table, no output scratch space, and total virtual runtime data below 144 MiB.
- Keep `WVHB 1` on the shared 48,000,000,000 hosted compiler/build-driver ceiling. Its compiler and in-process verifier work is broader than the standalone verifier profile and already uses the shared `u64` compiler metadata contract.
- Replace exact WVB, PE, and ELF identities with the current artifacts. No legacy implicit-import sources, 8-billion verifier package, dual metadata acceptance, or migration path is retained.

## Exact local evidence

The compiler-aligned hosted verifier is 118,496 WVB bytes with SHA-256 `19760a4438a48c945de3e39fd612ed72f3ea3a33373b5d9da09cd1e2411938d7`. Its 961,536-byte Windows package has SHA-256 `cac82b26c7af4edea01a808db718e66e65fd859f421d5e73f144b017f390bc59`; its 962,560-byte Linux package has SHA-256 `d99f5d9c95f1ab7e731eaf4ea7f15e48a19cc72e689f99d1b00d5a58f2984ede`.

The build driver is 1,008,678 WVB bytes with SHA-256 `090f7ce9e00708dc029cbe98448c30ee1e6e0544bd2dcac1de045c44dcc226b2`. Its 27,656,704-byte Windows package has SHA-256 `3f72b96ef0697c1b531f566180fea9f406b7213b88a6dd7000235d82d1878819`; its 27,656,192-byte Linux package has SHA-256 `8302f75a2ff9effaa72fa7ee58a6ee93a7e780bf39a4471aa41e397e94bcb568`. A three-source Project 1 composition now publishes the exact current 856-byte WVB with six functions and 383 code bytes.

The focused exact-compiler AOT transport case passes a zero-warning Release build, deterministic package reconstruction, malformed-container rejection, direct Windows verifier execution over the evolved compiler, corrupted-candidate rejection, build-driver explicit/project execution, output preservation, and host-module inspection in 78.100 test seconds. This is local Windows development evidence; independent Debian execution and dual-host qualification remain pending.

## Consequences

The hosted verifier and build driver now compile under the same explicit import/privacy contract they verify. The native verifier uses its existing wide host meter instead of adopting a WebAssembly-specific multi-instance protocol. The higher ceiling is bounded capacity, not a promise of constant verification cost.

Historical Decisions 0185 and 0196 remain evidence for their exact artifacts. They do not create a compatibility obligation for current hosted-tool packages.

## Reconsideration triggers

Revisit this decision if:

- the compiler-aligned native verifier approaches 16,000,000,000 instructions;
- a shared verifier decomposition reduces native and WebAssembly work without creating parallel semantics;
- the verifier needs more than one input snapshot or any output capability;
- the 128 MiB shared arena or 144 MiB one-snapshot runtime bound becomes operationally unacceptable; or
- a named release policy creates a compatibility obligation for `WVHV` or `WVHB` artifacts.
