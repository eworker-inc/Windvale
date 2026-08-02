# Decision 0094: First section-derived user-space WVB profile

- Date: 2026-08-02
- Status: Accepted and implemented; focused Windows and pinned-QEMU evidence recorded, cross-host qualification pending
- Implements: The next bounded part of step 6 in [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Contracts: [Interpreter profile 2](../../Specifications/Windvale-Os-Bytecode-Interpreter.md), [protected process version 4](../../Specifications/Windvale-Protected-Process.md), kernel memory version 3, kernel paging version 3, and firmware probe 25

## Context

Qualified Decision 0093 proved that a Windvale-written interpreter can run at CPL3, decode the admitted WVB, send result `29` to an independent Windvale init service, and preserve process fault containment. Its interpreter still depended on the admitted module's exact byte length and fixed code offsets. That was sufficient for the first execution proof but was not a usable module boundary: changing only the module name moved later sections and caused rejection.

The next smallest honest step is section discovery inside the existing process. It must not pretend to be a general loader or complete verifier. It must validate a closed WVB shape, derive every section location through checked envelope traversal, and prove offset independence with a second canonical compiler output.

## Decision

- Advance the interpreter to profile 2, the protected-process record to `WVPROC04`, memory to `WVKMEM03`, paging to `WVKPAG03`, and firmware to probe 25. Do not retain compatibility branches for the preceding experimental records.
- Keep [`Bytecode-Interpreter.wv`](../../Operating-System/Runtime/Bytecode-Interpreter.wv) portable and outside the kernel. Its eight functions validate the envelope, discover section payloads, validate the admitted semantic shape, and execute the existing instruction subset.
- Accept WVB 1.6 inputs from 12 through 4,096 bytes only after checking magic, version, exactly seven ordered sections, zero flags/reserved bytes, every payload range, and exact end-of-input. Derive module, capability, data, function, code, export, and type offsets by walking those checked section envelopes.
- Retain a deliberately closed semantic profile: portable module, bounded nonempty module name, empty capability/data/type sections, exactly one exported `Main() -> i32`, one `i32` local, maximum stack depth one, and code consisting only of bounded `i32.const`, `local.store 0`, `local.load 0`, and final `return`.
- Require a focused test to compile another valid module whose longer name moves the code payload, compile the same interpreter around those bytes, and still obtain result `29`. This proves section-derived execution rather than one accepted byte layout.
- Add process runtime profile `2` at record offset `0xB0`; init records retain profile `0`. Bind policy token `94`, exact interpreter identity, exact input identity, instruction budget `4,671`, call-depth budget `3`, and user-page budget `37`.
- Admit the measured ABI-16 AOT growth rather than hiding it. The linked interpreter is 127,598 bytes, so process `2` owns 32 RX pages, four RW/NX stack pages, and one RW/NX context page. Live QEMU established that the eight-function interpreter needs the complete 16 KiB stack; two pages page-faulted before exit.
- Expand the fixed kernel arena to 58 pages/232 KiB and select it at a checked 2 MiB-aligned address inside an eligible conventional-memory descriptor. The alignment makes the existing one-page-table-per-process rule explicit and keeps both complete process extents within one 2 MiB region.
- Expand the kernel executable window to 256 KiB/64 RX leaves. The linked payload must still fit exactly and remains supervisor read-only/executable; all ordinary pages remain writable/non-executable.
- Preserve the receive-only init service, capacity-one channel, fixed coordinator, contained client fault, terminal CPL0 faults, admission bridge version 2, WVA seam version 8, ABI 16/context 7, and retained bridge 10.

## Evidence

The focused Windows OS suite passes 25 of 25 tests. It covers deterministic artifacts, all prior malformed boundaries, the complete seven-section traversal, stable negative interpreter results, a second compiler-produced WVB with a moved code-section payload, runtime-profile records, 2 MiB arena alignment and one-page-short rejection, 37 user leaves, four NX stack pages, and the earlier isolation/fault invariants.

Important exact artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 3,708 | `c84aeb3b9658b9a2c5847bd769aef6eb87a9b46b743123ef49a5faf530f7a65b` |
| Process-policy WVO | 31,998 | `a992d44ce72cbee7b0bd3bb110cb4c5ce2a027d788dfb1cfa67e2ad461ddd0bf` |
| Interpreter WVB | 12,359 | `909e624df86e614b6f7dcaa61e75ffa685467015015bfafd7b0772ee41a89920` |
| Interpreter WVO | 128,129 | `9788ad4159d783ebc35ee5af6c73b7c294643261bc00acfcf0f33a6bdf35c140` |
| Linked normal interpreter image | 127,598 | `c293f84199fecce07c3a0dbafb6406e7c2aad3521782df7095fe8ee6ca58a0e8` |
| Linked fault interpreter image | 127,598 | `6364289e6ddaaa125969bb27626672f08f114ebd738b7b191d2c55125c45fc6e` |
| Normal process-machine WVO | 136,668 | `33ef216d89926bacd53b5a46c5f39f3802c778bdfee44de0d7c79a440637e696` |
| Fault process-machine WVO | 136,700 | `123adcc0c0dfb9c919ae1abdcdc1f4e330e98b86d6d753b110c3ddcb04a9de44` |

Pinned QEMU `pc-q35-11.0,accel=tcg` passes all scenarios:

| Scenario | Bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| normal | 214,528 | `99686e2b2484de156a01bbec0ece68e6a15af7229a477d9245ee4f3068ed4f5b` | `0` |
| invalid opcode | 214,528 | `da14fd39c4ebf19471bb7205aa1cfce5b40a52c1b64979d5ff62eed143d64e90` | `3` |
| general protection | 214,528 | `02e6a297c6495a0c35b4b799403a0cedc8a1538c3a62c52836b2fe6fa6a225c9` | `3` |
| contained interpreter fault | 215,040 | `ede1d1d0ae8638086c81564e3150829137f4f3aceead9e7bfddeedc9445ae2f6` | `0` |

Probe 25 is a candidate until the committed source passes the repository's Windows and pinned-Debian non-Fast verification. Decision 0093 records the exact preceding cross-host-qualified probe-24 checkpoint.

## Consequences

The interpreter now understands section boundaries rather than one serialized address map. This is the first meaningful movement from a demonstration byte sequence toward a loader/verifier boundary while preserving the architecture: bytecode remains in user space, verified semantics remain shared, and the kernel owns only isolation and machine policy.

The measured code and stack growth is intentionally visible. The current baseline native backend favors simple, inspectable lowering over compact code; profile 2 therefore consumes a much larger fixed RX extent. That pressure is evidence for later native-code density work, paging/reclamation policy, or a streamed runtime design, not permission to conceal unbounded memory.

## Deliberate limits

This decision does not accept a runtime-supplied pointer or boot resource, load arbitrary WVB, verify general functions or types, interpret multiple functions, branches, calls, heap values, capabilities, or imports, publish executable pages, JIT code, cache native output, add a third runnable, schedule preemptively, reclaim pages, or expose a stable process ABI. The admitted program bytes remain embedded in the interpreter image.

## Reconsider when

- a boot resource or init message can carry candidate WVB bytes into the interpreter process;
- more than one function requires general signature, call-frame, and control-flow verification;
- the 32-page RX extent motivates compact native lowering or a different interpreter representation;
- another process requires allocator reclamation or a general virtual-memory policy;
- JIT publication needs a capability and checked writable-to-executable transition; or
- system-profile Windvale/WVA can replace the current Stage 0 record and page-table constructors.
