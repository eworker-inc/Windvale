# Windvale native random containment tests

## Status and purpose

This contract transfers the managed Seed test named `bounded random input never
escapes diagnostic boundaries` into fixed .NET-free Windows and Linux evidence.
The permanent tests do not generate new random values and do not treat Stage 0
as a live oracle. They consume one immutable corpus produced from the exact
framework-seeded sequence at commit `d964660`.

The corpus proves containment at four owned boundaries:

- source text through the capability-free Windvale compiler memory adapter;
- the same source text through the Windvale-native WVA assembler;
- arbitrary bytes through the Windvale-native WVB verifier; and
- arbitrary bytes through the Windvale-native WVO verifier.

This is rejection and containment evidence. It does not make random input the
definition of source, WVB, WVA, or WVO semantics, and it does not replace the
focused valid, malformed, limit, or differential matrices.

## Frozen sequence

The one-time oracle starts framework `Random` with seed `0x00575642` and uses
one continued stream in this order:

1. 500 source values. Each length is selected from zero through 511 characters.
   Every character is selected from the exact alphabet
   `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789{}[]();:,.+-*!<>=_ˉ `
   plus tab, CR, LF, backslash, and double quote. Each value is passed to both
   the Stage 0 source compiler and assembler.
2. 1,000 WVB values. Each byte length is selected from zero through 511 and the
   value is filled by `Random.NextBytes` before Stage 0 WVB admission.
3. 500 WVO values. The same length and byte rules continue from the preceding
   PRNG state before Stage 0 WVO admission.

The fixed corpus contains all 2,000 inputs, their ordinal, logical length, byte
length, SHA-256, Stage 0 outcome, primary diagnostic code and offset, and the
assembler outcome and code for source cases. Source text is retained as strict
UTF-8. Every Stage 0 outcome in version 1 is rejection.

The compact archive is 617,645 bytes with SHA-256
`c3d17ee927d8c485fc98b85c4b50d5fb6110532b8a2d02b818d7018903f2edc6`.
Its 240,966-byte LF-only manifest has SHA-256
`d7076c44f43192db832796553cbe605c20829361d7249e111a270ff22458186c`.
The archive owns exactly `Manifest.txt`, 500 `Source-NNN.wv` values, 1,000
`Wvb-NNNN.wvb` values, and 500 `Wvo-NNN.wvo` values in manifest order. Entries
are plain filenames; duplicates, paths, unknown families, noncontiguous
ordinals, inconsistent sizes, changed digests, invalid UTF-8 metadata, and
out-of-bound values are rejected before native execution.

## Source containment

The source lane verifies the exact import-free direct compiler WebAssembly
artifact and its ABI-4 exports, fixed memory extent, input/output regions, and
artifact identity once. Every nonempty source becomes one canonical WVSS 1
root module. The sole empty source uses the memory adapter's documented empty
input rejection because canonical WVSS forbids a zero-length module. Each case
receives a fresh compiler instance, at most 2,000,000 instructions, and must
return a complete `WVCO 1` kind-one strict-UTF-8 diagnostic beginning with
`source-wvb status=`. A trap, host import, resource escape, success, malformed
envelope, or over-budget result fails the lane.

The source lane then invokes the exact host's digest-bound native assembler for
the same 500 byte values. Every case must exit through rejection, write exactly
one `WVA1001` report, preserve the input, and preserve an existing canonical
WVO destination byte for byte. One source case represents both compiler and
assembler checks but contributes one declared corpus case.

Node.js supplies the cross-host WebAssembly engine and bounded process
coordination for this test. It owns no Windvale source, WVB, WVA, or WVO
semantics. The direct compiler has no host imports, while the assembler remains
the ordinary digest-bound Windows/Linux Windvale application.

## Binary containment

The WVB and WVO lanes select the exact host artifact and verify its byte length,
SHA-256, and Linux executable mode before any case. At most four independent
native processes run at once inside one lane. Each process has a 65,536-byte
combined per-channel bound.

Windows and Linux use the same asynchronous child-process collector. Completion
is observed only after the child has exited and both output channels have closed;
the collector then reports the native status, bounded standard output, and bounded
diagnostic bytes as one result. Windows verifier and inspector applications own
explicit `ExitProcess` termination, so no synchronous host workaround or loader
fall-through participates in the exit contract.

All 1,000 WVB values must return the native verifier's rejection exit, write no
standard output, emit one structured `wvb status=Invalid phase=...` diagnostic,
and preserve the input. All 500 WVO values must return the native object
verifier's rejection exit, write no standard output, emit one structured
`object status=...` diagnostic with bounded counts and offset, and preserve the
input. Stage 0 codes and offsets remain provenance; representative fixed
matrices continue to own exact native diagnostic-family agreement.

## Host commands

The three selectable commands are:

```text
Tools/Native/Test-Source-Containment.cmd
Tools/Native/Test-Wvb-Containment.cmd
Tools/Native/Test-Wvo-Containment.cmd
```

Linux uses the paired `.sh` names. The manifest-driven retirement coordinator
runs the lanes sequentially and requires the exact summaries `500`, `1000`, and
`500` passed cases. Local development should select only the changed lane; the
unfiltered coordinator remains reserved for the final grouped retirement gate.

## Change and qualification rule

Changing the seed, ordering, count, alphabet, framework sequence, any input,
oracle result, archive/manifest identity, native artifact, compiler ABI, report
shape, resource bound, or preservation rule requires a new corpus version and
reviewed Windows/Linux evidence. The one-time managed generator is recovery
provenance only and is not part of the permanent commands.
