# Decision 0054: First WVA-to-Windvale kernel seam

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Decision 0052 establishes real kernel-owned memory and a stack, but the UEFI loader and memory implementation are still emitted as raw x86-64 by C# bootstrap code. Only the bounded source Main is compiled from `.wv`. Continuing to add raw exception, paging, or runtime machinery in C# would turn a recovery implementation into the kernel architecture before Windvale has an explicit native system seam.

The native compiler cannot yet lower the control flow, `u64` address arithmetic, memory access, internal calls, or unsafe operations required by the allocator. WVA 1 already provides verified WVO symbols and relative tail transfers, while the compiler work proceeds independently. The first transition therefore needs to be executable without claiming a policy rewrite that the current target cannot express.

## Decision

- Define [kernel native seam version 1](../../Specifications/Windvale-Kernel-Native-Seam.md) with C# as reference/recovery host tooling, WVA as the explicit machine layer, and `.wv` as the destination for kernel policy.
- Add `Operating-System/Kernel/X64-Main-Shim.wva`. It exports `Windvale_kernel_wva_main`, imports compiler export `Windvale_kernel_main`, and tail-jumps through one canonical `relative-i32` relocation.
- Assemble and independently verify that resource during every boot-image build. Reject any architecture, section, symbol, byte, or relocation shape other than the exact five-byte version 1 shim.
- Make the memory object call the WVA export after switching stacks. Resolve the shim's import against compiler-generated Main through the ordinary linker.
- Do not expand source semantics, the native compiler target, or WVA 1 in this slice. Record the concrete compiler capabilities required before memory and exception policy can migrate into `.wv`.
- Treat new raw C# kernel instruction emission as transitional work that must identify its blocking native/WVA capability and replacement path.

## Consequences

The boot image now contains an independently assembled Windvale machine object between the memory bootstrap and source-generated kernel Main. This proves the WVA-to-WV symbol, relocation, stack, and execution seam under the existing QEMU transcript without colliding with concurrent compiler implementation work.

The canonical shim is 158 bytes with SHA-256 `f7525da5e8365b75adc68bd2174ad5763ed05b774d861c2a0cd6aad6c0e8e1b7`. The five-object link produces a deterministic 7,168-byte EFI application with SHA-256 `b4f557fdd39d44858ce05fd6a99b0128a791053a5d3c2aa9e68dc5b5c34a3808`.

This is a transition seam, not a Windvale-written allocator, loader, exception handler, or complete kernel. C# remains necessary as a reference/recovery tool and current bootstrap implementation. Privileged machine mechanics remain future WVA work; kernel policy moves only when the native compiler can express and verify it explicitly.

## Reconsider when

- The native target can lower the memory planner or allocator policy directly.
- WVA needs named exception-entry, descriptor-table, control-register, or return-from-interrupt operations.
- A stable kernel ABI supersedes the tail-transfer boundary.
- The Windvale-written assembler becomes suitable for the build-host role rather than only verified-bytecode qualification.
