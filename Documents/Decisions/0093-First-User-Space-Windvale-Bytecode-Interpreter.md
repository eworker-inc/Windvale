# Decision 0093: First user-space Windvale bytecode interpreter

- Date: 2026-08-02
- Status: Qualified at exact commit `190174a01299369fb855e27ea676d34062e09c5b`; superseded for current development by Decision 0094
- Implements: The first bounded part of step 6 in [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Contracts: [Protected process version 3](../../Specifications/Windvale-Protected-Process.md) and [interpreter profile 1](../../Specifications/Windvale-Os-Bytecode-Interpreter.md)

## Context

Qualified Decision 0092 established two isolated CPL3 domains, reduced endpoint capabilities, cross-process IPC, deterministic block/wake, and a Windvale init service. Its client still executed the admitted program's host-built AOT derivative. Decision 0084 requires the interpreter and later JIT to live outside the kernel, but implementing a general loader, full verifier, runtime selector, and executable-publication service together would hide the first real boundary under too many new contracts.

The smallest honest step is one Windvale-written interpreter process for the already admitted module. It must derive result `29` from decoded WVB instructions, not call the program's AOT image, and must preserve the existing service, isolation, capability, W^X, and fault-containment evidence.

## Decision

- Advance the protected-process record to `WVPROC03` and firmware to probe 24. Do not retain compatibility code for the experimental version-2 record.
- Add portable [`Bytecode-Interpreter.wv`](../../Operating-System/Runtime/Bytecode-Interpreter.wv). Compile the interpreter itself through canonical WVB and the shared ABI-16 AOT backend, then run it as ordinary process `2` at CPL3.
- Bind the interpreter WVB digest separately from the admitted program WVB digest. Record runtime kind `2`, eight code pages, two stack pages, the program digest, instruction budget `567`, call-depth budget `2`, and user-page budget `11`.
- Interpret only the exact already admitted 174-byte WVB profile: bounded `i32.const`, `local.store 0`, `local.load 0`, and `return`. Deterministic malformed and mutation tests must prove that the result comes from decoded bytes.
- Remove the admitted program's AOT derivative from the client link. The normal and deliberate-fault WVA entries call only the interpreter export, then retain the existing send/exit or send/fault behavior.
- Keep the interpreter outside the kernel and give it no executable-publication authority. All mapped code is prelinked RX; stack and context are RW/NX.
- Preserve the receive-only init service, capacity-one channel, fixed coordinator, contained client fault, terminal CPL0 faults, admission bridge version 2, WVA seam version 8, ABI 16/context 7, and retained bridge 10.
- Treat the Stage 0 page/object/coordinator code as an explicit replacement seam. This slice changes who performs the computation, not the durable language semantics.

## Evidence

The focused Windows OS suite passes 25 of 25 tests. It covers exact interpreter/policy/process/firmware artifacts, deterministic repetition, reference execution count `567`, malformed and changed interpreter inputs, separate runtime identities, role-specific page extents, two NX interpreter stack pages, and all earlier protection and fault invariants.

Pinned QEMU `pc-q35-11.0,accel=tcg` passes all scenarios:

| Scenario | Bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| normal | 114,176 | `4248f3402a0abfe9eda531109460fb37a7c1f3f907ded162a49a785bab38fbb7` | `0` |
| invalid opcode | 114,176 | `465ac8d095160e169187860413b0b347148df4acc2f24802f7a5e166981f6c59` | `3` |
| general protection | 114,176 | `0a65ba2982f551434553054fc92d82a1a6e727a73527c85a79526f328105b58c` | `3` |
| contained interpreter fault | 114,688 | `8cd17f693ae088eefaccb2b8449fd47f34e345352bb46beebd4a2a32fe8fad5d` | `0` |

Normal serial evidence includes `wvb-runtime=interpreted` only after process result `29` has crossed the existing service boundary. Live testing caught a real one-page stack underrun caused by the interpreter's measured AOT frame. The corrected contract uses two contiguous RW/NX stack pages, initializes `RSP` at their exclusive end, and records the page count in `WVPROC03`.

Exact commit `190174a01299369fb855e27ea676d34062e09c5b` is the cross-host-qualified probe-24 checkpoint. GitHub [Verify run 30732061301](https://github.com/eworker-inc/Windvale/actions/runs/30732061301) passes the complete non-Fast verifier on Windows and digest-pinned Debian 12: each host passes all 67 Seed tests and all 25 OS tests. Windows Seed elapsed time is 247.422 seconds and Debian is 145.020 seconds. The four exact QEMU scenarios above remain the recorded Windows machine evidence; the cross-host workflow does not claim a Debian QEMU run.

## Consequences

Windvale OS now executes the admitted program through Windvale-written bytecode interpretation in user space. The kernel does not interpret bytecode, and the program's separately compiled AOT derivative is no longer its guest execution path. This is the first concrete implementation of Decision 0084 step 6.

The interpreter is intentionally small enough to inspect. Its fixed input and offsets expose what a later general loader must replace: bounded section discovery, function/type validation, multi-function state, runtime selection, and executable publication. The exhausted 32-page boot arena also turns future process growth into a measured allocator/arena decision rather than silent expansion.

## Deliberate limits

This decision does not add arbitrary WVB input, a complete module decoder or semantic verifier, multiple functions, general value stacks, calls, branches, heap values, capabilities, JIT compilation, cached native code, W^X publication requests, a third runnable, a scheduler, process creation/teardown, page reclamation, Hyper-V, or physical-hardware evidence.

## Reconsider when

- a second admitted module requires general section and function discovery;
- a branch, call, or heap value requires a general interpreter frame/value model;
- a third runnable requires scheduling rather than fixed coordination;
- JIT publication needs a capability and checked writable-to-executable transition;
- the fixed 32-page arena must grow or release pages; or
- system-profile Windvale/WVA can replace the current Stage 0 process constructor and dispatcher.
