# Decision 0421: Import-free browser console envelope

- Status: Implemented candidate; cross-browser qualification pending
- Date: 2026-08-08
- Advances: [Decision 0333](0333-Segmented-Direct-WebAssembly-Compiler.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Browser playground](../../Specifications/Browser-Playground.md) and [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md#retained-profile-17-interpreter-bounded-browser-console-envelope)

## Context

The static playground's direct Windvale compiler could compile the repository's
portable scalar sample, but its import-free interpreter admitted only portable,
capability-free modules. The documented `Hello-Windvale.wv` sample is hosted,
declares `console.write_line`, and therefore stopped after compilation once the
normal browser path no longer used the managed playground engine.

Loading a .NET runtime again would reverse the retirement direction. A Chrome
extension would add an installation and browser-vendor boundary without making
the language capability safer. A WebAssembly host import would make execution
depend on reentrant browser callbacks and a new import ABI. The existing
request/result boundary can instead carry explicit authority and bounded output
while the interpreter remains import-free.

## Decision

- Add `WVXI 3` for portable scalar modules and one exact hosted profile:
  `console.write_line(text) -> void` with `Main() -> i32`.
- Carry the capability grant as bit zero in each request. A declaration is not
  authorization; a missing grant returns `WVR3010` before guest execution.
- Carry a caller-selected standard-output limit capped at 65,536 bytes. Each
  line append is all-or-nothing and exhaustion returns `WVR3013`.
- Return scalar result and accumulated output in `WVXO 3`. Reserve a separately
  sized diagnostic channel but require it to remain zero in version 3.
- Keep the interpreter WebAssembly module import-free. JavaScript decodes the
  completed envelope after execution; there is no DOM callback, server call,
  browser extension, or ambient browser-console authority.
- Expose one visible per-tab grant in the static page and select it by default
  only for the exact Hello example.
- Preserve `WVXI`/`WVXO` versions 1 and 2 for their existing callers.

## Evidence and consequences

The native front door builds the five-function interpreter to 118,542 WVB bytes
with SHA-256
`186e28569f9047503cc1ce70823a72f186d47c010ec66d990090b2acc64a0982`.
The pinned native WebAssembly lowerer publishes 918,415 import-free ABI-3 bytes
with SHA-256
`69377b4893021d2c8bcebbc6af415be73bbc5930fb7b1400f9607d42c75f5e98`.

The direct compiler produces 253 canonical Hello WVB bytes with SHA-256
`0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f`.
The granted run returns `Hello from Windvale` plus LF and scalar zero after
eight guest and 15,623 outer instructions. The denied run returns `WVR3010`
with zero guest instructions and no output. The portable 183-byte baseline
still returns 42 after four guest and 8,877 outer instructions through the same
version-3 response boundary. A focused four-line limit case retains exactly
three 16,384-byte lines plus three LF bytes (49,155 bytes total), rejects the
fourth line with `WVR3013`, and publishes no partial fourth line.

Implementing the capability exposed and corrected a stale interpreter ownership
classification: canonical WVB 1.11 value shape `3` is text, while shape `5` is
`u32`. Descriptor retain/release sites now classify text as `3` and bytes as
`6`, matching the format contract.

This does not create general hosted execution. Diagnostic output, console
without a line terminator, arguments, files, clocks, randomness, storage,
network access, DOM access, and system modules remain unavailable. The
completed response retains output in memory until the run ends, so 65,536 bytes
is a hard semantic and resource ceiling rather than a streaming hint.

## Reconsideration triggers

Introduce a new versioned contract before adding another capability, partial
writes, a diagnostic channel, or a larger output bound. If interactive or
unbounded-looking programs need output, use an explicitly resumable event
protocol with per-resume work and byte budgets; do not add ambient imports or
silently replay a capability call. Qualify the current envelope independently
across supported browsers before promoting it beyond experimental status.
