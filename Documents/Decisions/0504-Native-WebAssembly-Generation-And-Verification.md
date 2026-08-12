# Decision 0504: Native WebAssembly generation and verification

- Status: Paired Windows/Linux focused evidence complete; grouped qualification pending
- Date: 2026-08-10
- Scope: normal WebAssembly source/WVB construction, WVB-to-Wasm generation, and strict engine verification without .NET
- Extends: Decisions 0202, 0277, 0278, 0312, 0333, 0457, and 0458

## Context

`Tools/Verify/Verify-WebAssembly.ps1` was the last standalone normal verifier
outside the broad Seed and GitHub qualification gates that directly executed
.NET. It built the managed tool project, used the managed source compiler over
the retained corpus, and executed the managed WebAssembly backend before the
independent Node.js engine and probe phases.

Windvale already has paired manifest-bound native WebAssembly compiler
applications, the native source-to-WVB front doors, a native bootstrap route
for the current compiler, and a portable compiler-WVB builder. The remaining
normal seam was orchestration: those native products needed one generic,
success-only WVB-to-Wasm launcher and the complete verifier needed to consume
them consistently.

## Decision

Windvale adopts the following normal WebAssembly verification route:

1. build ordinary source/project fixtures through the digest-bound native
   `Build-Wvb` front door;
2. reconstruct the current hosted compiler through `Bootstrap-Compiler` and
   the portable compiler through `Build-Compiler-Wvb.mjs`;
3. lower every admitted WVB through
   `Tools/WebAssembly/Compile-Wvb-To-Wasm.mjs`;
4. require that launcher to admit the exact host compiler from
   `Artifacts/WebAssembly-Native-Backend/Manifest.json`, publish through a
   private sibling, validate the completed module, reject host imports, and
   rename only after success; and
5. run the unchanged strict Node.js engine, record-arena probe, and compiler
   probe over the exact produced artifacts.

The native backend package binds these exact normal inputs:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WebAssembly compiler WVB | 347,729 | `8a71377dc22f77747f3c04f1ff3a323ef9e5a48f90a4732bfbb5ebf2cc8a84b1` |
| Windows x64 compiler | 5,476,352 | `b5359908928770140ef54c1d757b64c50cf00df06e2768a640d01287ccc34041` |
| Linux x64 compiler | 5,476,352 | `ea3cd335094a6d2ee237c346a6134cdda6415f33a40f87f0507e52938030f87f` |

The current verifier independently pins the hosted compiler WVB at 921,640
bytes, the portable compiler WVB at 919,317 bytes, and the retained scalar
interpreter WVB at 918,415 bytes. The final four-phase compiler-capacity runs
remain below the unsigned 32-bit instruction ceiling. Their hosted phase
budgets are 3,406,048,155; 1,806,084,638; 1,252,119,172; 3,777,932,325;
1,631,515,149; and 3,282,266,930. The corresponding portable budgets are
3,404,593,950; 1,803,261,683; 1,248,721,455; 3,758,734,827;
1,635,887,489; and 3,276,815,868.

## Evidence boundary

On the current Windows host, the strict Node.js engine phase passed in 1,239.5
seconds. The complete `Verify-WebAssembly.ps1` command then passed in 1,619.5
seconds, including native source/WVB construction, native generation of every
Wasm artifact, exact identity and semantic checks, the record-arena probe, and
the compiler probe. The normal command contains no direct .NET invocation.

The new launcher also passed a bounded success/rejection check: valid input
produced an import-free module, an unsupported WVB was rejected, the existing
destination was preserved, and no private scratch remained.

The complete command also passed on an independent Linux host in 1,371.8
seconds with empty stderr. Its final compiler probe returned the exact 183-byte
payload at SHA-256
`3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`.
The verifier selects the paired native `.cmd` or `.sh` source-build and
compiler-bootstrap front doors for the current host; no private host
configuration is part of the repository evidence.

The automatic Windows changed-file route then passed this exact implementation
state in 1,353.0 seconds. Its plan contained nine changed paths, no fixed suites,
no evidence gaps, planner verification, and exactly one WebAssembly owner.

`Verify-WebAssembly.ps1` remains outside the fixed cross-platform retirement
coordinator because it is a distinct long-running owner. WebAssembly-owned
changes now select one explicit `RunWebAssemblyVerification` plan property, and
`Verify-Changed.ps1` dispatches the command once after any cheaper fixed suites
pass. The five former `webassembly-native-verification` mappings are closed and
never fall back to a managed or unfiltered verifier. W1 remains
`native-candidate` until grouped qualification and its other documented
promotion conditions complete.

The verifier currently creates a cleanup-contained, temporary root-level
`.wvproj` only for generated one-root fixtures because the native builder's
project-path resolution still depends on that placement. No generated project
file remains after success or failure. Durable project organization and
project-relative path resolution are a separate follow-up; this decision does
not prescribe that project files belong at repository root.

## Consequences

- `Tools/Verify/Verify-WebAssembly.ps1` is removed from the direct managed-entry
  inventory. The inventory becomes twelve files: three normal broad
  verification/release entry points and nine recovery commands.
- The managed WebAssembly backend and source compiler are no longer part of the
  normal Windows or Linux WebAssembly gate.
- The Stage 0 backend/compiler reconstruction scripts remain explicit recovery
  owners and keep their managed provenance.
- Segmented backend-package reconstruction, cross-browser evidence, grouped
  qualification, promotion, and final recovery retirement remain open.

## Reconsider when

Reconsider this decision if the source/project closure, either compiler WVB,
the native backend package, emitted Wasm identities, exact engine budgets,
publication contract, Node.js engine boundary, or project-path resolution
changes.
