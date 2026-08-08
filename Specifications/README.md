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
- [Native source-to-WVB front door](Windvale-Native-Source-To-Wvb-Front-Door.md)
- [Native WVB-to-WVO application](Windvale-Native-Wvb-To-Wvo.md)
- [Native WVB-to-WVO rejection tests](Windvale-Native-Wvb-To-Wvo-Rejection-Tests.md)
- [Native source-to-AOT composition](Windvale-Native-Source-To-Aot-Composition.md)

## Bytecode, runtime, CLI, and tools

- [Seed bytecode](Seed-Bytecode.md)
- [Seed CLI](Seed-CLI.md)
- [Seed conformance](Seed-Conformance.md)
- [Browser playground host](Browser-Playground.md)
- [Experimental WebAssembly target and execution ABI](Windvale-WebAssembly.md)
- [`wvdump` structural core](Wv-Dump-Core.md)
- [`wvdump` report](Wv-Dump-Report.md)
- [Native WVB read-only front door](Windvale-Native-Wvb-Read-Only-Front-Door.md)
- [Native WVB unsafe rejection tests](Windvale-Native-Wvb-Unsafe-Rejection-Tests.md)
- [Native retirement test suite](Windvale-Native-Retirement-Test-Suite.md)

## Database experiment

- [Experimental Windvale Database reader and hosted snapshot adapter](Windvale-Database-Reader.md)

## Assembly, objects, and linking

- [WVO object core](Wvo-Object-Core.md)
- [Native WVO inspector application](Windvale-Native-Wvo-Inspector.md)
- [Native WVO read-only rejection tests](Windvale-Native-Wvo-Read-Only-Rejection-Tests.md)
- [Native WVO differential tests](Windvale-Native-Wvo-Differential-Tests.md)
- [Native WVO hostile-size tests](Windvale-Native-Wvo-Hostile-Size-Tests.md)
- [Native WVO publisher](Windvale-Native-Wvo-Publisher.md)
- [Windvale object format](Windvale-Object-Format.md)
- [Windvale textual assembly](Windvale-Assembly.md)
- [Windvale assembler core](Wva-Assembler-Core.md)
- [Native WVA assembler application](Windvale-Native-Wva-Assembler.md)
- [Native WVA assembler rejection tests](Windvale-Native-Wva-Assembler-Rejection-Tests.md)
- [Native WVA differential tests](Windvale-Native-Wva-Differential-Tests.md)
- [Windvale linking](Windvale-Linking.md)
- [Windvale linker core](Wv-Linker-Core.md)
- [Native Windvale linker application](Windvale-Native-Wv-Linker.md)
- [Native linker hostile-input tests](Windvale-Native-Linker-Hostile-Input-Tests.md)

## Native execution

- [Native execution context and ABI](Windvale-Native-Execution-Context.md)
- [Native execution-context construction](Windvale-Native-Execution-Context-Construction.md)
- [Native argument-table construction](Windvale-Native-Argument-Table-Construction.md)
- [Native entry-bridge construction](Windvale-Native-Entry-Bridge-Construction.md)
- [Native byte-result admission](Windvale-Native-Byte-Result-Admission.md)
- [Native hosted-tool runtime-header construction](Windvale-Native-Hosted-Tool-Runtime-Header.md)
- [Versioned verified native-fragment artifact](Native-Fragment-Artifact.md)
- [Windows x64 console application target](Windvale-Windows-Console-Application.md)
- [Linux x64 console application target](Windvale-Linux-Console-Application.md)
- [Hosted console application capability and metadata](Windvale-Hosted-Console-Application.md)
- [Hosted compiler application manifest](Windvale-Hosted-Compiler-Application.md)
- [Portable console-application layout plan](Windvale-Console-Application-Plan.md)
- [Portable console-application construction recipe](Windvale-Console-Application-Construction.md)
- [Native console-application packager](Windvale-Native-Console-Packager.md)
- [Native console-application publisher](Windvale-Native-Console-Application-Publisher.md)
- [Native console-container hostile-input tests](Windvale-Native-Console-Container-Hostile-Input-Tests.md)
- [Native console-container mutation tests](Windvale-Native-Console-Container-Mutation-Tests.md)
- [Native hosted-console container mutation tests](Windvale-Native-Hosted-Console-Container-Mutation-Tests.md)
- [Native console-application segmented-size tests](Windvale-Native-Console-Application-Segmented-Size-Tests.md)
- [Native console-application segmented construction](Windvale-Native-Console-Application-Segmented-Construction.md)
- [WVA native stencils](Wva-Native-Stencil.md)
- [Native publication plan](Windvale-Native-Publication-Plan.md)
- [Native publication lifetime](Windvale-Native-Publication-Lifetime.md)
- [Native service-bundle materialization](Windvale-Native-Service-Bundle-Materialization.md)
- [Native output-table construction](Windvale-Native-Output-Table-Construction.md)
- [Native file-output-table construction](Windvale-Native-File-Output-Table-Construction.md)
- [Native file-input-table construction](Windvale-Native-File-Input-Table-Construction.md)
- [Native service-table construction](Windvale-Native-Service-Table-Construction.md)
- [WVB publication transaction](Windvale-Wvb-Publication-Transaction.md)
- [Compiler build-driver application](Windvale-Compiler-Build-Driver.md)
- [Bounded x86-64 kernel target](Windvale-X64-Kernel-Target.md)

## Boot and operating system

- [Read-only resource store](Windvale-Resource-Store.md)
- [Resource-service IPC](Windvale-Resource-Service-Ipc.md)
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
