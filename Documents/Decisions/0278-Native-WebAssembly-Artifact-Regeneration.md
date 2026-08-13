# Decision 0278: Native WebAssembly artifact regeneration

- Status: Accepted with exact Windows-local execution evidence
- Date: 2026-08-06
- Scope: browser-interpreter WVB-to-WebAssembly regeneration
- Builds on: [Decision 0169](0169-Public-Format3-Compiler-Targets.md), [Decision 0255](0255-Scalar-Local-WebAssembly-Dispatch.md), [Decision 0266](0266-Pinned-WebAssembly-Playground-Package.md), and [Decision 0277](0277-Native-WebAssembly-Compiler-Regeneration.md)
- Retains: WVB 1.11, WebAssembly execution ABI 3, `WVHA 1`, native ABI 22, the fixed six-capability/ten-service compiler authority, the 48-billion-instruction ceiling, the 128 MiB dynamic arena, and Stage 0 as explicit recovery

## Context

Decision 0277 removes Stage 0 from normal portable-compiler WVB regeneration. The remaining browser-package seam was applying the Windvale-authored WebAssembly backend to the natively built scalar-interpreter WVB. Its ordinary hosted wrapper builds through the native source front door, but it conditionally formats scalar results and therefore requires `i32.format` in addition to the fixed format-3 compiler services. The format-3 writer correctly rejects that different service bundle.

Decision 0255 also measured and rejected composing the source compiler, project driver, and WebAssembly backend into one 1,400,728-byte application: its 34,076,699 native fragment bytes exceeded the shared 32 MiB large-native bound. That result does not describe the separately built 321 KiB WebAssembly backend. Keeping source-to-WVB and WVB-to-WebAssembly as two independently verified compiler processes avoids the oversized combined image and preserves narrower failure boundaries.

Two alternative WebAssembly self-hosting probes fail closed. A portable backend memory adapter has seventeen nominal types, while the current direct WebAssembly target deliberately admits no type table and returns `Unsupportedˉmodule` after 2,116 recovery-runtime instructions. Feeding that adapter to the existing interpreter Wasm returns no execution envelope after 203 outer instructions because the guest lies outside its accepted executable subset. Expanding either target into a self-hosting general compiler is not the smallest artifact-production transfer.

## Decision

- Add `Projects/Tools/Windvale-WebAssembly-Artifact-Tool.wvproj` as the exact artifact-only WebAssembly compiler. Its hosted wrapper accepts one WVB and one output path, reports only module bytes and execution ABI, and omits the unrelated scalar-result `i32.format` service. It performs an explicit strict UTF-8 round trip for its report label so its native fragment retains the exact format-3 compiler service set.
- Treat format 3 as a bounded compiler family rather than one source-compiler digest. A member must still declare the exact six capabilities in canonical order, require the exact ten services, fit the unchanged limits, and be named and digest-pinned by an external artifact inventory. This is not a general hosted-application profile and does not admit a different authority or service set.
- Pin the exact WVB and paired format-3 Windows/Linux applications under `Artifacts/WebAssembly-Native-Backend`. Keep package construction under the explicit Stage 0 recovery script; do not change C# product source or invoke Stage 0 during normal use.
- Make `Tools/WebAssembly/Build-Interpreter-Wasm.mjs` the normal application route. Before execution it verifies the selected native application identity. It writes only to a unique same-directory candidate, then requires valid WebAssembly, zero imports, the exact ten exports, execution ABI 3, 129 pages, and the fixed input/output regions. `--check` additionally requires byte equality with the pinned interpreter Wasm. Normal publication flushes the verified candidate and atomically renames it over the destination.
- Keep source-to-WVB separate: rebuild `Wvb-Scalar-Interpreter.wvb` through the ordinary native front door, then apply the native WebAssembly compiler. Do not rebuild or deploy the native tool packages as part of the website build.

## Exact evidence

The ordinary native source front door publishes the 321,699-byte artifact compiler WVB at SHA-256 `c674a95d1aef3317eeede3d8fd171419058fcd3868013856908215e189feefe1` in 13.192 seconds on the measured Windows host. Stage 0 recovery packages that unchanged WVB as a 5,203,968-byte Windows application at SHA-256 `16b0f9feac13823cf1a133a72de5ed864aaee0538a6351edf060a58a7bb3d80c` and a 5,206,016-byte Linux application at SHA-256 `2f579e329141343296dcbe839e43a435e797e0d93be84aabdb065f0f0754dd60`.

The Windows native application lowers the checked-in 112,216-byte interpreter WVB to exactly 839,104 WebAssembly bytes in 0.823 seconds. The result has SHA-256 `f65c4e203d4b244ec52e0619f9d1a99ce1d2809296313cb154bba8316c6d916c`, byte-identical to the packaged Stage 0 recovery oracle. It reports execution ABI 3 and exits zero.

These measurements establish exact current-host execution and artifact identity. The unchanged format-3 container contract retains its earlier cross-host qualification, but the exact new Linux package has not yet executed independently on Linux and is not promoted as fresh dual-host evidence.

## Consequences

Every artifact consumed by the normal browser playground now has a no-.NET regeneration route: native source compilation publishes both WVB inputs, and the native WebAssembly compiler publishes the interpreter Wasm. Website build, verification, deployment, browser compilation, returned-WVB verification, and execution remain .NET-free. Stage 0 remains explicit, digest-checked recovery for reconstructing the two native compiler package families.

The broader WebAssembly differential/qualification gate still uses managed oracles and remains part of the final project-wide .NET retirement gate. This decision closes the normal website artifact-production seam; it does not retire .NET from the whole repository, remove recovery source, or claim a self-hosting direct WebAssembly backend.

## Reconsideration triggers

Revisit this decision if:

- a compiler-family member requests a capability or service outside the fixed format-3 contract;
- the artifact tool no longer fits the format-3 native image, runtime arena, file-value, or instruction bounds;
- the native Windows/Linux outputs disagree with the independently verified WebAssembly oracle;
- direct WebAssembly lowering or the interpreter admits the backend's complete nominal/compiler surface at practical bounds;
- a Windvale-native container constructor can reconstruct these packages without Stage 0; or
- any normal website artifact-production or publication command starts .NET.
