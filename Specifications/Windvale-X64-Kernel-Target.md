# Windvale x86-64 kernel target

## Status and purpose

`x86-64-kernel-entry-wvo-v1` is the first compiler-native target over Windvale's typed WIR. It exists to produce one verified, position-independent WVO kernel-entry object for the UEFI boot path. It is a bounded integration target, not the general Windvale native ABI or completion of the native-backend phase. [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) owns this boundary.

The C# reference/recovery compiler implements version 1 as `X64ˉkernelˉcompiler`. It uses the ordinary source lexer, parser, module composition, and semantic compiler before examining typed WIR. A successful result is serialized by `Objectˉcodec` and decoded and verified again before publication. Source or WIR that falls outside this specification fails explicitly; it is never interpreted as a similar supported program.

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
- imported function `Windvale_kernel_write_byte`; and
- one `relative-i32` relocation with addend `-4` for every generated `call rel32` byte-output call.

There is no static data section or absolute address. Text is materialized as immediate byte arguments, which is acceptable only because of the 4 KiB target limit. The existing linker resolves the imported adapter and the loader's independent import of `Windvale_kernel_entry` before the UEFI PE32+ adapter accepts the all-code link.

## Entry and adapter ABI

`Windvale_kernel_entry` implements [kernel handoff version 1](Windvale-Kernel-Handoff.md). Before source-derived output, its compiler-generated wrapper independently validates the handoff pointer, magic, version, record size, retained-map pointer and length, 1 MiB map bound, descriptor stride, descriptor version, reserved field, and map-byte divisibility. Invalid input returns `1` without invoking the adapter.

After validation, the generated entry reserves 32 bytes of x64 shadow space plus 8 alignment bytes. For each source-derived ASCII byte it places the zero-extended value in `ECX` and calls `Windvale_kernel_write_byte`. The OS adapter may clobber standard x64 volatile registers and returns no value. The generated entry restores its stack, returns the source `Main` constant through `EAX`, and preserves every nonvolatile register because it uses none.

The first adapter polls legacy COM1 transmitter readiness and writes the low byte of `ECX`. That implementation is an OS bootstrap device choice, not Windvale source semantics, a portable capability implementation, or a general console driver contract.

## Diagnostics and validation boundary

Native-target diagnostics use phase `native-backend`:

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

Identical source and target version produce identical WVO bytes. The canonical `Hello-World.wv` input produces a 905-byte WVO object with SHA-256 `22ccc0d50b6170bc53fb6844d2fb7ec76b8a87e720dac8d7dacf2f2a71256cb9`, 20 relative relocations, and no absolute relocation. Firmware probe version 5 links and executes that object after successful `ExitBootServices`.

This target does not support general expressions, locals, calls, branches, Unicode console output, static-data addressing, multiple functions, a general native calling convention, optimization, unwind information, Windows or Linux executable production, compiler self-hosting, or portable bytecode execution in the OS. Each expansion requires focused semantics, encoding, relocation, and differential evidence rather than silent fallback.
