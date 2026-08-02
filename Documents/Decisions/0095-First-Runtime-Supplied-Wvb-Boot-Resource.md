# Decision 0095: First runtime-supplied WVB boot resource

- Status: Candidate
- Date: 2026-08-02
- Owners: Windvale compiler/runtime and operating-system boundaries
- Contracts: [Interpreter profile 3](../../Specifications/Windvale-Os-Bytecode-Interpreter.md), [protected process version 5](../../Specifications/Windvale-Protected-Process.md), kernel memory version 4, kernel paging version 3, ABI 16/context 7/service table 5, and firmware probe 26

## Context

Qualified Decision 0094 proved section-derived validation and execution in a Windvale-written CPL3 interpreter, but Stage 0 injected the complete admitted WVB into the interpreter source before compilation. The program identity and interpreter identity were distinct in policy records while their bytes were still fused into one linked RX image. That prevented the process boundary from demonstrating a real runtime input.

The smallest coherent next step is not a filesystem, package manager, arbitrary module loader, new ABI, or third process. It is one immutable, bounded boot resource delivered through the already qualified `file.read_bytes` generated-code convention. This preserves the verified interpreter, the two-process isolation proof, and the native ABI while exposing the exact ownership seam a later init/package service must replace.

## Decision

- Advance firmware to probe 26, protected processes to `WVPROC05`, interpreter runtime profile to `3`, and kernel memory to `WVKMEM04`. Do not retain compatibility branches for the preceding experimental records.
- Make `Bytecode-Interpreter.wv` hosted Windvale with exactly one `file.read_bytes` capability and one exact resource name, `boot:main.wvb`. Compile it once. Tests vary only the supplied immutable resource.
- Remove the admitted WVB declaration and all source injection from the interpreter. Require structural tests to prove the complete 174-byte WVB is absent from both interpreter WVB and linked client RX image.
- Keep ABI 16, context version 7, and service-table version 5 unchanged. Publish only the `file.read_bytes` slot for process `2`; every other service slot remains zero.
- Map the admitted WVB in its own 4 KiB user-readable, read-only, non-executable page. The unused tail is zero. The planner checks length 12 through 4,096, exact SHA-256 agreement with the process record, and a complete in-image service-leaf range before publishing any process state.
- Use context offset 96 for one OS-private 32-byte little-endian `WVBR` version-1 table. It carries magic/version/size, the immutable page pointer, exact length, and a zero reserved word. This table is a Windvale OS adapter, not a change to the Windows/Linux `WVFI` host table.
- Add one exact 199-byte x86-64 ABI-16 service leaf. It accepts only the exact UTF-8 name `boot:main.wvb`, validates the private table and resource bounds, returns a borrowed-bytes descriptor, writes file-not-found detail `6` for a wrong name and unavailable detail `8` for a bad table, and preserves `R10`, `R11`, and `R15`.
- Author the leaf bytes in WVA as one exported read-only stencil. WVA continues to reject arbitrary byte statements in code sections. Stage 0 verifies the exact relocation-free 199-byte data symbol and republishes the identical bytes as one code/function WVO before linking. This explicit publication adapter is a bootstrap replacement seam, not a general executable-byte escape.
- Expand the client extent from 41 to 42 pages and the fixed arena from 58 to 59 pages. Process `2` owns 32 RX code pages, four RW/NX stack pages, one RW/NX context page, and one RO/NX input page. Its user-page budget is `38`, instruction budget is the measured `4,678`, and call-depth budget remains `3`.
- Keep the fixed AOT admission policy in front of process creation. The admitted program remains the exact 174-byte WVB with SHA-256 `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2`. Runtime supply changes transport, not admission or semantics.

## Candidate evidence

The focused Windows OS suite passes 25 of 25 tests. It covers deterministic interpreter, service-stencil, published service, process, paging, and firmware artifacts; reference execution with alternate and malformed runtime inputs; absence of the admitted WVB from the interpreter/client image; RO/NX resource mapping; zero tail; exact service and resource tables; process/profile identities; resource digest and leaf-range rejection; and all earlier isolation and fault invariants.

Important candidate artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| WVA read-only resource stencil WVO | 314 | `1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816` |
| Published resource code WVO | 314 | `610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d` |
| Linked normal client image | 128,157 | `5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655` |
| Normal process-machine WVO | 137,665 | `6d1517bbf5f947f55e07cbb582b3bf7050199bd8b31a1425a82a891a68730f14` |

All four pinned Windows QEMU scenarios pass. Normal is 215,552 bytes with SHA-256 `ab6b818beee3ac7419d48ad7ac5d04f06bab0ec67ab3909de64ed1b88c2e1170` and host code `0`; invalid opcode is 215,552 bytes with `e439c3afa5168743076981aa4c0a278384508b65481d8f7ed0dc68959d4f49e8` and code `3`; general protection is 215,552 bytes with `710d34c567f3411cf7728fd211b67e39fcdd175e9a4441960e0f4846d3ef4f52` and code `3`; contained user fault is 216,064 bytes with `55b2bb810c34e2c4ea6f4f68558c09913ff802c4357282d6b24a6b6b3d6e1dcc` and code `0`. Normal and contained-fault paths complete the real CPL3 resource call, interpret result `29`, transfer it to init, and shut down. Windows/Linux qualification is still required before this decision becomes qualified.

## Consequences

The interpreter is now a genuine consumer of runtime-owned bytes. Its executable identity can remain constant while tests and future resource owners supply different valid inputs. Program bytes have a distinct page permission, lifetime, address, and digest check instead of masquerading as interpreter code/data.

No new portable language semantics or native ABI are introduced. Windows, Linux, and Windvale OS continue to adapt the same `file.read_bytes` generated convention through platform-owned private tables. The one OS leaf demonstrates that a hosted Windvale module can run in the guest without importing a host filesystem.

The fixed boot-resource owner remains Stage 0. The current init/resource service does not yet choose or transfer the WVB, and the kernel coordinator remains fixed. Moving that authority requires an explicit byte/handle transfer and lifetime decision; it must not be implied by renaming the current table.

## Deliberate non-claims

This decision does not accept arbitrary WVB, establish a filesystem or resource namespace, provide package lookup, transfer capabilities or page ownership, implement a general semantic verifier, interpret additional functions/types/control flow, publish executable memory, JIT code, cache native output, add scheduling, create or reclaim processes, stabilize the syscall ABI, or remove Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- an init/package service can own and transfer an immutable module without weakening lifetime or digest checks;
- a real third runnable creates scheduling pressure;
- a broader real WVB requires the next verifier/interpreter semantic slice;
- the fixed 32-page interpreter RX extent motivates a compact lowering or different representation; or
- executable publication is justified as a separate capability-oriented JIT decision.
