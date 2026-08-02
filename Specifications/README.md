# Windvale specification index

These documents define the current implemented or accepted contracts. Dated rationale and supersession history live under [`Documents/Decisions/`](../Documents/Decisions/); project status lives in the [progress dashboard](../Documents/Project/Progress.md) and [roadmap](../Documents/Project/Roadmap.md).

## Language, naming, and hosted resources

- [Seed language](Seed-Language.md)
- [Source naming](Source-Naming.md)
- [Immutable records](Seed-Records.md)
- [Enums and bounded formatting](Seed-Enums-And-Formatting.md)
- [Hosted resources](Hosted-Resources.md)
- [Foundation bytes](Foundation-Bytes.md)
- [Foundation machine contracts](Foundation-Machine-Contracts.md)
- [Foundation byte ordering](Foundation-Byte-Ordering.md)
- [Foundation decimal parsing](Foundation-Decimal-Parsing.md)
- [Foundation byte construction](Foundation-Byte-Construction.md)

## Compiler and project input

- [Source lexer](Compiler-Source-Lexer.md)
- [Declaration parser](Compiler-Source-Declaration-Parser.md)
- [Body parser](Compiler-Source-Body-Parser.md)
- [Canonical source set](Compiler-Source-Set.md)
- [Import graph](Compiler-Source-Graph.md)
- [Declaration and signature symbols](Compiler-Source-Symbols.md)
- [Body, local, and call bindings](Compiler-Source-Bindings.md)
- [Typed source IR](Compiler-Source-Wir.md)
- [Source-to-WVB backend](Compiler-Source-Wvb.md)
- [Project manifest](Windvale-Project.md)

## Bytecode, runtime, CLI, and tools

- [Seed bytecode](Seed-Bytecode.md)
- [Seed CLI](Seed-CLI.md)
- [Seed conformance](Seed-Conformance.md)
- [Browser playground host](Browser-Playground.md)
- [Experimental WebAssembly target and execution ABI](Windvale-WebAssembly.md)
- [`wvdump` structural core](Wv-Dump-Core.md)
- [`wvdump` report](Wv-Dump-Report.md)

## Assembly, objects, and linking

- [WVO object core](Wvo-Object-Core.md)
- [Windvale object format](Windvale-Object-Format.md)
- [Windvale textual assembly](Windvale-Assembly.md)
- [Windvale assembler core](Wva-Assembler-Core.md)
- [Windvale linking](Windvale-Linking.md)
- [Windvale linker core](Wv-Linker-Core.md)

## Native execution

- [Native execution context and ABI](Windvale-Native-Execution-Context.md)
- [WVA native stencils](Wva-Native-Stencil.md)
- [Native publication plan](Windvale-Native-Publication-Plan.md)
- [Native publication lifetime](Windvale-Native-Publication-Lifetime.md)
- [Bounded x86-64 kernel target](Windvale-X64-Kernel-Target.md)

## Boot and operating system

- [x86-64 UEFI boot environment](Windvale-Os-Boot-Environment.md)
- [UEFI application format](Windvale-Uefi-Application.md)
- [OS boot probe](Windvale-Os-Boot-Probe.md)
- [Kernel handoff](Windvale-Kernel-Handoff.md)
- [Kernel memory](Windvale-Kernel-Memory.md)
- [Kernel paging](Windvale-Kernel-Paging.md)
- [WVB admission](Windvale-Os-Wvb-Admission.md)
- [User-space bytecode interpreter](Windvale-Os-Bytecode-Interpreter.md)
- [Protected processes](Windvale-Protected-Process.md)
- [Kernel native seam](Windvale-Kernel-Native-Seam.md)
- [Kernel trap frame](Windvale-Kernel-Trap-Frame.md)
- [Kernel CPU exceptions](Windvale-Kernel-Exceptions.md)
- [Kernel shutdown](Windvale-Kernel-Shutdown.md)
