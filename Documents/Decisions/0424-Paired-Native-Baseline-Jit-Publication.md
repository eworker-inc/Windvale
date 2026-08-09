# Decision 0424: Paired native baseline-JIT publication

- Status: Implemented paired candidate; complete backend, qualification, and promotion pending
- Date: 2026-08-08
- Advances: [Decision 0419](0419-Descriptor-Returning-Native-Main.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native baseline-JIT publication](../../Specifications/Windvale-Native-Baseline-Jit-Publication.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The bounded `WVJP 1` plus `WVLT 1` publisher already passed on Windows and had
an exact reconstructed Linux ELF, but the Linux application had never executed
on a genuine host. The first paired workflow reached Debian and stopped before
JIT publication because three digest-pinned tool ELFs in its reconstruction
chain lacked executable Git metadata. Repository preflight found the same
missing mode on two adjacent Linux console candidates.

## Decision

Run the existing permanent native publisher scripts on Windows and digest-pinned
Debian 12 in one manual paired workflow. Preserve the scripts as the behavioral
owners: they rebuild the producer WVB, admit the retained WVO, assemble and
verify the shared and platform WVA objects, link and repackage the exact
application, compare it with the retained candidate, then execute it.

Restore Git mode `100755` on the five Linux candidate ELFs that were tracked as
`100644`. This changes no artifact bytes or digest. Do not add shell `chmod`
repair; executable distribution metadata belongs in the repository index.

## Evidence and consequences

GitHub run
[`31291005460`](https://github.com/eworker-inc/Windvale/actions/runs/31291005460)
records the initial Debian `Permission denied` failure at the native WVO
verifier. Commit `6e73c4dce73d7e332fa4514e643821a5fd489dfd` restores the
five missing modes. Successor run
[`31291079619`](https://github.com/eworker-inc/Windvale/actions/runs/31291079619)
passes on both hosts.

Windows reconstructs and executes the exact 59,904-byte publisher at SHA-256
`8ea1a0d6371c9447031db4ae2b56ecfef5f022a83b6bdd7831020a2628bee01c`.
Debian reconstructs and executes the exact 65,648-byte publisher at SHA-256
`29538c93d28bcd1feae175519f5b2950d5e8dfcde24afa3f0039863fb1706a90`.
Each process rejects corrupted lifetime and patch plans, publishes and invokes
results `42` and `-1`, forces a seal failure with release, returns zero, and
writes no diagnostic. Neither process loads .NET.

This closes genuine Linux execution for the bounded baseline-JIT publication
candidate. It does not make the six-byte profile a complete backend, qualify
general WVB-to-JIT integration, or promote it into an ordinary execution tier.

## Reconsideration triggers

Requalify when `WVJP`, `WVLT`, either platform mapping owner, producer WVB,
retained WVO, linked image, or application container changes. Extend the
profile through explicit typed plans and resource accounting rather than a
general writable/executable-memory escape hatch.
