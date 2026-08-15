# Windvale x86-64 kernel target

## Status and purpose

`x86-64-kernel-entry-wvo-v2` is the first compiler-native target over Windvale's typed WIR with a separate validated entry wrapper and source-derived Main export. It produces one verified, position-independent WVO object for the UEFI boot path. It is a bounded integration target, not the general Windvale native ABI or completion of the native-backend phase. [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) owns the original source-to-boot boundary; [Decision 0052](../Documents/Decisions/0052-First-Kernel-Owned-Memory-Foundation.md) owns the version 2 split used for the kernel stack; and the [Windvale system-kernel target](Windvale-System-Kernel-Target.md) defines its current normal WVB-to-WVO implementation.

The normal path now compiles source to canonical WVB through the Windvale compiler, then uses the Windvale-written bounded system-kernel target. The C# `X64ˉkernelˉcompiler` remains feature-frozen recovery and differential evidence. Source, WIR, or WVB outside this specification fails explicitly; it is never interpreted as a similar supported program or silently delegated to Stage 0.

## Source and WIR subset

Version 1 accepts one source module with all of these properties:

- profile `system`;
- no imported source modules;
- no nominal types and only immutable `text` data;
- no declared capability except `console.write_line`;
- exactly one function, exported as `Main() -> i32`;
- no parameters, user locals, or control-flow split; and
- an `i32` constant return value.

The accepted linear WIR operations are `Textˉconstant`, `I32ˉconstant`, and a void `Callˉcapability` to `console.write_line` with one text temporary. The text may come from named data or a compiler-interned literal. Each capability call emits its exact ASCII bytes followed by LF. The aggregate output is limited to 4,096 bytes. Non-ASCII text, another capability, another operation, another function shape, or branched control flow is rejected.

The profile and declared capability remain meaningful. System code does not gain ambient serial-port access: this target maps the declared `console.write_line` request to the explicit `Windvale_kernel_write_byte` OS adapter.

## Generated object

The compiler emits canonical WVO 1.0 with:

- architecture `x86-64`;
- one 16-byte-aligned code-only `.text` section;
- exported function `Windvale_kernel_entry` at offset zero;
- exported function `Windvale_kernel_main` at a later 16-byte-aligned offset;
- imported function `Windvale_kernel_memory_enter`;
- imported function `Windvale_kernel_write_byte`; and
- one `relative-i32` relocation with addend `-4` for the memory-entry call and every generated `call rel32` byte-output call.

There is no static data section or absolute address. Text is materialized as immediate byte arguments, which is acceptable only because of the 4 KiB target limit. The existing linker resolves the imported adapter, the loader's import of `Windvale_kernel_entry`, and the WVA shim's import of `Windvale_kernel_main` before the UEFI PE32+ adapter accepts the all-code link.

## Entry and adapter ABI

`Windvale_kernel_entry` implements [kernel handoff version 1](Windvale-Kernel-Handoff.md). Before source-derived output, its compiler-generated wrapper independently validates the handoff pointer, magic, version, record size, retained-map pointer and length, 1 MiB map bound, descriptor stride, descriptor version, reserved field, and map-byte divisibility. Invalid input returns `1` without invoking the adapter.

After validation, the generated entry reserves 32 bytes of x64 shadow space plus 8 alignment bytes and calls `Windvale_kernel_memory_enter` with the accepted handoff pointer still in `RCX`. That memory layer owns map selection, state initialization, handoff copying, and the stack switch before invoking `Windvale_kernel_main`.

For each source-derived ASCII byte, `Windvale_kernel_main` places the zero-extended value in `ECX` and calls `Windvale_kernel_write_byte`. The OS adapter may clobber standard x64 volatile registers and returns no value. Main restores its stack and returns the source constant through `EAX`. The generated functions preserve every nonvolatile register because they use none.

The first adapter polls legacy COM1 transmitter readiness and writes the low byte of `ECX`. That implementation is an OS bootstrap device choice, not Windvale source semantics, a portable capability implementation, or a general console driver contract.

## Diagnostics and validation boundary

The frozen C# recovery target retains these `native-backend` diagnostics:

| Code | Meaning |
| --- | --- |
| `WVN1001` | Unsupported module profile. |
| `WVN1002` | Unsupported capability, data, or nominal type. |
| `WVN1003` | Unsupported function or control-flow shape. |
| `WVN1004` | Unsupported WIR operation or return value. |
| `WVN1005` | Output is non-ASCII or exceeds 4,096 bytes. |
| `WVN9000` | Generated WVO failed independent object validation. |

Frontend and semantic diagnostics retain their existing `WVC` codes and phases. Version 1 native diagnostics use the start of the root source because typed WIR does not yet preserve instruction-level source spans.

## Determinism and current evidence

Identical source and target version produce identical WVO bytes. The current Probe 40 `Hello-World.wv` source compiles natively to a 1,581-byte WVB at SHA-256 `795734982cded8b3605cb5cf0f110667b71140d5639185c3ef94cde3174b3bc0`, then lowers to the established 13,454-byte WVO at SHA-256 `4bf896ac2b349d9e786bbb7cae0165cb47273aa82ff2985a7ff33c3185978e8b` with no absolute relocation. The firmware probe links this special object with memory/paging/process layers, the bidirectional WVA seam, fixed Windvale-owned admission and two-generation process policy, separate interpreter, init, and directory CPL3 AOT images, the retained ABI-22 portable object, both native bridges, normalized exception destinations, and the WVA Q35 adapter. Main executes only after verifier token 73, resource-domain-gated process-policy token 97, two atomic typed-resource grants, two budgeted interpreter executions, two generation-matched cleanups, generation-safe non-tail client-object release/zero/reuse while the directory object remains live, both results 29, retained native result 29, and the fixed domain transcript's current-zero/peak-3/144/2 evidence.

This special target does not support general expressions, locals, calls, branches, Unicode console output, static-data addressing, multiple functions, optimization, unwind information, Windows or Linux executable production, or compiler self-hosting. The current probe retains AOT execution of broader shared-ABI portable modules and composes paging, fixed WVB admission, two-generation process isolation, a typed two-resource grant, budgeted user-space interpretation, memory-object cleanup/reuse, IPC, and target-specific exception/shutdown mechanics outside this compiler target; none make the special target general or give it a general WVB loader. Each expansion requires focused semantics, encoding, relocation, and differential evidence rather than silent fallback.
