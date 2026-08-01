# Decision 0078: Multi-patch Windvale native stencil

- Date: 2026-08-01
- Status: Qualified at exact commit `50294d9d5cc24edc26a3e56994cb3aa28e16352c`
- Extends: [Decision 0077](0077-First-Windvale-Owned-Native-Stencil.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, and firmware probe 16

## Context

Decision 0077 proves one live Windvale-assembled template with one typed patch, but that case cannot show how repeated and semantically distinct ABI values coexist without positional C# construction. The already-qualified 70-byte `process.argument` leaf is the smallest active second case: it has control flow, two repeated contract meanings, six distinct meanings, and eight byte locations while requiring no call, relocation, or architecture change.

## Decision

- Define the deliberately bounded ordered `WVSP 2` record in [the WVA native-stencil specification](../../Specifications/Wva-Native-Stencil.md). Its header fixes patch count and template size; each record carries a template-relative offset, width, and closed semantic kind.
- Author the complete zero-hole machine shell and all eight ordered records in `Compiler/Native/Stencils/Process-Argument.wva`.
- Retain the canonical 321-byte WVO produced by the Windvale-written WVA assembler as an embedded native-compiler resource. Its SHA-256 is `307e61dcb2a156eb0d4b77f7d93676d7b1ac24f9bb6fe1f31217837213352bad`.
- Name six distinct ABI meanings rather than accepting caller-chosen positional values. The service-detail and borrowed-text-length kinds each appear twice and must receive one checked value at both locations.
- Require strict object, metadata, ordered-record, fixed-shell, zero-hole, and instantiation validation. Missing values, unused kinds, duplicate or descending locations, changed widths, and unknown kinds fail closed.
- Route the live `X64ˉnativeˉargumentˉservices.Build(Processˉargument)` path through this artifact. The final leaf remains exactly 70 bytes with SHA-256 `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1`.
- Retain `WVSP 1` unchanged for recovery and compatibility within the current source state. `WVSP 2` is a measured second exact contract, not a permissive generalization or a stable public format.
- Keep C# as the strict consumer, identity oracle, W^X publisher, executor, and Stage 0/recovery implementation for this slice.

## Evidence boundary

The focused Windows candidate compiles with zero warnings. Its combined conformance case compiles the Windvale-written assembler once, runs it twice over each accepted WVA source, requires deterministic WVO bytes and exact Stage 0 equality, verifies the two embedded artifact identities, instantiates one and eight patches, and preserves both final leaf digests. It rejects corrupt counts, sizes, offsets, widths, kinds, opcodes, holes, incomplete value sets, and duplicate locations. The live native hosted-input case executes the new `process.argument` construction successfully. Cold focused runs take 2.906 seconds for the stencil case and 0.632 seconds for live hosted input. The change-aware Windows gate selects all eight test areas and passes all 60 tests in 221.385 seconds with zero warnings; its warm stencil and live-input cases take 1.155 and 0.132 seconds. This gate includes the 166.719-second golden contract but remains development feedback rather than qualification evidence.

Exact commit `50294d9d5cc24edc26a3e56994cb3aa28e16352c`, tree `4dca2de1f65b929f18d3e12cffe3f4b47f6536ff`, was archived as 2,956,277 bytes with SHA-256 `d3015804c219993e3c75d53f6ea19842593612a1c37a7093f331fe0a08a14398`. The archive retained that identity on the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 60 integrated Seed tests, exact compiler reproduction, and the complete native CLI/reproduction gate. Their suite times are 224.398 and 242.491 seconds; complete Qualification takes 459 and 477 seconds wall-clock. The combined stencil cases take 1.475 and 1.550 seconds, and live native hosted input takes 0.123 and 0.101 seconds. The 15,563-byte Windows report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 12,160-byte timing report has SHA-256 `637e0f465a28a3d9e9f3160eadc276236de488aea88dba1fe4997d50498e7485`. Their normalized contracts match exactly with SHA-256 `d7af450f930865f91672e6291d2da80f7226330c53d5d4b66c0d597c088c9711`.

All 62 current portable artifacts, totaling 7,753,361 bytes, match byte for byte. The newly qualified 714-byte project-build artifact accounts for the increase from 61 and is byte-identical to the existing composition outputs. The canonical name/size/SHA-256 manifest has SHA-256 `5c21498b51ad93d5e41e895249e288aaa203abb07bcb00de305b9b686764bf17`. The 2,299,392-byte retrieved Debian evidence bundle has SHA-256 `38c1884323badcab44d4a5b5e53d8bbdc51e2311560e97ea0837a19ca8737e15`.

Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG boots unchanged firmware probe 16 as an exact 15,872-byte image with SHA-256 `206a036f8cbe3198544b6878bf52c80ef8d489c14d5437c6c7004ff1d6599504`, emits the complete success transcript, and returns guest-controlled host exit code 1. GitHub [Verify run 30708475858](https://github.com/eworker-inc/Windvale/actions/runs/30708475858) independently passes classification plus Windows and Linux verification for the exact implementation commit.

After evidence retrieval, the exact Debian QA tree, transferred source archive, and remote evidence bundle were removed and confirmed absent.

## Consequences

Both process-input service leaves are now constructed from WVA-authored, Windvale-assembled objects. Production C# no longer contains either final argument-service byte array, although its strict loader intentionally retains exact shells and semantic mappings as the independent acceptance oracle.

This still does not constitute a general JIT. The contract has no branch-target patches, calls, relocations, data references, template selection, native Windvale loader, or executable-memory ownership. The next ownership transfer should move bounded object/stencil validation and patch application into a Windvale-written consumer before broadening executable shapes, unless measured implementation evidence shows a branch-bearing stencil is the smaller prerequisite.

## Reconsider when

- A measured compiler operation requires wider constants, branch targets, calls, data references, or multiple templates.
- A Windvale-written consumer can replace C# object loading and patch application while retaining the independent Stage 0 oracle and hostile-input boundary.
- Repeated canonical WVO loading materially affects startup, package size, or code-cache identity.
