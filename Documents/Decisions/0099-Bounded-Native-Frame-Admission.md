# Decision 0099: Bounded native frame admission

- Status: Qualified
- Date: 2026-08-02
- Extends: [Decision 0089](0089-Bounded-Native-Stack-Arguments.md)
- Advances: Native ABI 17
- Retains: Execution-context version 7, service-table version 5, WVB 1.6, WVO 1.0, the 64-parameter internal call convention, and every native service

## Context

ABI 16 admits the language's complete 64-parameter limit, then exact compiler preflight stops at `Compilerˉsourceˉwirˉcompileˉblock`. That function has 1,049 locals and maximum declared WVB stack depth 12, while the baseline backend permits only 1,024 combined local/value frame cells. It is the only exact-compiler function with at least 1,024 locals.

The operating-system process policy independently reaches the same ceiling while describing two client generations. The backend therefore needs a larger but still explicit admission envelope; silently removing the limit or weakening the policy would hide the measured constraint.

## Decision

- Advance the current target to `x86-64-wvb-baseline-v17`.
- Double the hard frame envelope from 1,024 to 2,048 16-byte cells. The maximum generated frame is therefore 32 KiB, before the existing bounded outgoing-call reservation.
- Retain one zero-initialized cell per local and numbered lowered value. This decision does not introduce liveness reuse, register allocation, spills, roots, safe points, or dynamic stack growth.
- Apply the same 2,048-cell bound in semantic preflight, lowering, selection, and independent fragment reconstruction. A function that needs another cell fails before WVO or executable publication.
- Improve `WVN2004` so lowered-value exhaustion names the exact function, the first required combined slot count, and the accepted limit.
- Preserve all context fields, services, parameter cells, descriptor rules, status returns, instruction/call-depth counters, and host entry conventions.

## Evidence

Focused compiler verification must show that the former 1,049-local function is admitted past the ABI-16 preflight boundary. Exact compiler preflight must then either execute or name its next real bounded blocker without a host failure.

The implemented preflight advances to `Compilerˉbodyˉparseˉprimary` and reports `WVN2004`: it requires at least 2,049 combined local/value slots against the new 2,048-slot limit. That is deliberate progress evidence, not a claim that the compiler now runs natively.

Exact implementation commit `4a077ab9ebaf2108201927eef3095e87ef2ed907` passes GitHub [Verify run 30749304867](https://github.com/eworker-inc/Windvale/actions/runs/30749304867). Windows and digest-pinned Debian 12 each pass all 67 Seed tests, all 25 OS tests, and the complete non-Fast verifier. Seed elapsed time is 221.700 seconds on Windows and 201.079 seconds on Linux; both logs emit the same 56 SHA-256 values in exact order.

## Consequences and limits

The backend can now admit larger but still statically bounded functions, including the current OS policy. The exact compiler's former local-count blocker is gone, and its next cost is visible in lowered numbered values.

ABI 17 does not execute the exact compiler, optimize frames, alter WVO serialization, stabilize a public FFI, or retire C#/.NET. A later decision should prefer measured slot reuse or function decomposition before repeatedly increasing the ceiling.

## Reconsider when

- a required function exceeds 2,048 combined cells;
- measured frame initialization or stack use becomes material;
- liveness-based cell reuse or register allocation can retain independent verification; or
- managed references require roots, safe points, or a different frame representation.
