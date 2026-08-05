# Decision 0203: Evolved-compiler hosted-tool capacity

- Status: Accepted and cross-host qualified
- Date: 2026-08-04
- Scope: compiler-aligned native verifier and build-driver integration
- Extends: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md), [Decision 0186](0186-First-Windvale-Native-Compiler-Build-Driver.md), [Decision 0201](0201-Expanded-Exact-Compiler-Native-Capacity.md), and [Decision 0202](0202-Four-Phase-Compiler-Capacity-WebAssembly-Verification.md)
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

## Exact qualified evidence

The compiler-aligned hosted verifier is 125,721 WVB bytes with SHA-256 `259db7fc70679153982ca70843cf002e87b786d04ebeb0eafb628207f44c723f`. Its 1,007,104-byte Windows package has SHA-256 `f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0`; its 1,007,616-byte Linux package has SHA-256 `dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a`.

The build driver is 1,071,093 WVB bytes with SHA-256 `51f680d7fb96819e21ad8ab68988437c3ae5cfc3aa7a7ca5627641cae4fccbfe`. Its 28,840,960-byte Windows package has SHA-256 `1792ec58a433812d3a6cf32786ca968b5fd26155585805bd250d93ead60128e6`; its 28,839,936-byte Linux package has SHA-256 `2728c871c9d6083f02cade80c41aa58328185e0a3b8a2997d935fdf91186b2a2`. A three-source Project 1 composition publishes the exact current 1,388-byte WVB with nine functions and 627 code bytes.

Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` passes GitHub [Verify run 30964566192](https://github.com/eworker-inc/Windvale/actions/runs/30964566192). Windows and Debian each pass all 97 Seed tests, all 39 OS tests, and the native CLI gate, including direct verifier and build-driver execution without loading .NET.

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
