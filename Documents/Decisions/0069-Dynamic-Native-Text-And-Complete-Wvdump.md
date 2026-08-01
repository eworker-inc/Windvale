# Decision 0069: Dynamic native text and complete wvdump

- Date: 2026-08-01
- Status: Cross-host qualified at exact commit `79799332deb2d144e9b7557700829f14322a3062`
- Refines: [Decision 0068](0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md)'s ABI-9 structural parser
- Advances: The complete checked-in Windvale-written `wvdump` through JIT and WVO/AOT

## Context

ABI 9 ran the substantive envelope and payload parser from `Wv-Dump-Core.wv`, but stopped at the report boundary. The remaining checked-in code uses dynamic text concatenation, integer formatting, enum names, strict byte-to-text conversion, deterministic quoting, text-returning helpers, void helpers, and diagnostic output. Keeping those operations interpreter-only would leave the principal hosted Windvale inspection tool split across execution tiers.

Returning a borrowed descriptor in `RAX` would collide with the packed status convention. Allocating every result independently would also leave no aggregate per-run bound. Calling platform ABIs directly from generated code would make Windows and Linux fragments differ.

## Decision

- Advance the experimental target to `x86-64-wvb-baseline-v10` and native ABI version 10. ABI-9 artifacts remain historical evidence and are not accepted through a compatibility branch.
- Retain execution-context version 2 and its 48-byte record-arena layout. Advance the closed runtime-service table to version 4 and 96 bytes.
- Add pure runtime services for `Enumˉname`, `Textˉconcat`, `Textˉquote`, `I32ˉformat`, and `U32ˉformat`; `U8ˉformat` uses the zero-extended `U32ˉformat` entry. Add explicitly authorized `diagnostic.write_line`. Existing console, argument, file, and strict UTF-8 services retain their meanings.
- Give each native execution one fixed 16 MiB monotonic text arena. Every individual text value retains the WVB 1 MiB UTF-8 limit and traps with `WVR3012`; aggregate arena exhaustion becomes `WVR3018`. All arena text expires with the execution. No general heap, free, managed object, or garbage collector is introduced.
- Implement `Textˉfromˉutf8` by invoking the independently decoded strict-validation service, branching to packed status 8 / `WVR3014` on malformed input, and then copying the proven borrowed descriptor without allocation.
- Preserve packed scalar and status returns in `RAX`. A descriptor-returning caller passes the address of its verified result cell in `RAX`; the callee saves that hidden pointer in a dedicated final frame cell before zero-initialization, copies the complete 16-byte descriptor on successful return, and returns zero status. A void function returns zero status without a result cell. The four explicit register parameters remain unchanged.
- Carry bounded nominal type metadata beside a `Nativeˉfragment` so the pure enum-name adapter can map the proven type index and value to the exact member name. WVO 1.0 still serializes code/data rather than hosted execution metadata, so hosted AOT execution remains paired with its original verified fragment.
- Independently decode the new prologue, hidden-result cell, descriptor/void call and return shapes, every new service-table load and argument form, UTF-8 failure edge, text-result provenance, nominal metadata, and all balanced exits before WVO serialization or W^X publication.
- Differentially execute the complete checked-in `Examples/Foundation/Wv-Dump-Core.wv`, without slicing or reimplementation, through the reference interpreter, Windows x86-64 W^X JIT, and linked WVO/AOT. Require byte-for-byte identical standard output, diagnostics, exit status, and hosted file access.
- Advance the kernel native probe to version 5 and firmware probe identity to version 12 because the service-free OS AOT consumer is rebuilt through ABI 10. The guest still requires no runtime-service table or text arena.
- Retain C#/.NET as Stage 0 compiler, selector, independent decoder, platform adapter, semantic oracle, recovery implementation, and OS image builder. The retirement gates in Decision 0057 remain unchanged.

## Implementation evidence

The focused dynamic-text test covers enum naming, signed/u8/u32 formatting, concatenation, strict UTF-8 conversion, deterministic quoting, text-returning functions, void calls, console output, diagnostics, JIT, linked WVO/AOT, malformed UTF-8 as `WVR3014`, aggregate text-arena exhaustion as `WVR3018`, and corrupted hidden-result prologues rejected as `WVN3030`.

The complete-wvdump test compiles the 1,441-line checked-in source, inspects a real compiler-produced WVB, and compares the full report across the interpreter, JIT, and linked AOT. The exact archive exercises real Windows and System V x86-64 W^X paths. Both hosts pass all 15 deterministic OS tests for probe 12; the reproducible EFI application remains 15,872 bytes with SHA-256 `3010bc72b9c26386f062f78481c900cac841321b040b41447a0bbb65a9e392fe`.

Exact commit `79799332deb2d144e9b7557700829f14322a3062`, tree `d86080a14f654c26e839fe50574ae5c441670572`, was archived as 2,872,181 bytes with SHA-256 `c9fe842f0f137b523003acd8360196b138cfaae36c73b2ad7d08f0d837c9a57e`. Windows and Debian GNU/Linux 12 x64 pass zero-warning Release builds, all 56 tests, and the complete native CLI verifier. Their normalized contracts match, and all 61 portable artifacts totaling 7,752,647 bytes are byte-identical with the retained canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`.

The Windows suite takes 212.675 seconds and the Debian suite 233.729 seconds. The 15,563-byte Windows report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; the 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`. Pinned QEMU 11.0/Q35/TCG on Windows emits the complete `windvale-os-boot 12` success transcript and returns guest-controlled host exit code 1. The Debian QA host lacks QEMU, so its OS evidence is the complete 15-test suite rather than a duplicate emulator run. The 2,303,863-byte retrieved evidence bundle has SHA-256 `a6b0962dbaad4b0991f99e9beb523758e192f200334f830cead48dedea1cca38`; after comparison, the exact QA directory and transferred archives were removed and confirmed absent.

## Consequences

The complete current Windvale `wvdump` program can now execute as native code; the earlier report-boundary exception is removed. This materially increases the amount of real Windvale application code that can leave the managed interpreter while preserving one portable WVB identity and one shared JIT/AOT backend.

The service implementations are still Stage 0 C# callbacks. The generated program does not depend on .NET object layouts or platform calling conventions, but .NET remains required to provide these services until a native Windvale runtime implements the same closed table.

The text arena is deliberately bounded and monotonic. It fits finite compiler/report phases, not long-lived interactive state. WVO hosted-service and nominal metadata still need a self-contained native container before independently loadable hosted AOT programs are possible.

## Reconsider when

- A native runtime can implement the version-4 services without managed callbacks.
- Text must survive beyond one execution or requires measured reclamation.
- More than four explicit parameters justify a stack-argument convention.
- A standalone PE, ELF, or Windvale container serializes and verifies hosted service and nominal metadata.
- A general value-return ABI can replace the hidden descriptor-result convention without weakening packed trap propagation.
