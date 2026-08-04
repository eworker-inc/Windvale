# Windvale Seed command-line contract

## Commands

```text
windvale compile <source.wv> [--module <dependency.wv>]... [--target <wvb|windows-x64-console-v1|linux-x64-console-v1|windows-x64-console-v2|linux-x64-console-v2|windows-x64-console-v3|linux-x64-console-v3|windows-x64-verifier-v1|linux-x64-verifier-v1|windows-x64-build-driver-v1|linux-x64-build-driver-v1>] [-o <artifact>]
windvale build <project.wvproj> [-o <module.wvb>]
windvale aot <module.wvb> --target <windows-x64-console-v1|linux-x64-console-v1|windows-x64-console-v2|linux-x64-console-v2|windows-x64-console-v3|linux-x64-console-v3|windows-x64-verifier-v1|linux-x64-verifier-v1|windows-x64-build-driver-v1|linux-x64-build-driver-v1> [-o <artifact>]
windvale assemble <source.wva> [-o <object.wvo>]
windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...
windvale inspect <module.wvb>
windvale verify <module.wvb>
windvale object-inspect <object.wvo>
windvale object-verify <object.wvo>
windvale run <module.wvb> [--allow <capability>]... [--bind-read-only-directory <path>] [--max-steps <count>] [--report-steps] [--report-function-steps] [-- <argument>...]
windvale help
```

## Behavior

- `compile` reads the strict UTF-8 root source and every explicit repeated `--module` dependency, resolves the complete bounded import graph, composes one module, and verifies the generated WVB. It performs no implicit source lookup. Target `wvb` is the default and writes those deterministic module bytes with a `.wvb` extension.
- Target `windows-x64-console-v1` passes the verified WVB through the shared native backend, independently verified WVO/flat-link path, and the [Windows console application adapter](Windvale-Windows-Console-Application.md). It requires portable capability-free `Main() -> i32` with no runtime services, defaults to `.exe`, and produces an import-free executable that does not load .NET. Stage 0 and .NET remain build-time dependencies.
- Target `linux-x64-console-v1` consumes the same verified native fragment and WVO/flat-link evidence through the [Linux console application adapter](Windvale-Linux-Console-Application.md). It has the same capability-free scalar boundary, defaults to `.elf`, and produces a sectionless static-PIE ELF with no interpreter, dynamic loader, libc, or .NET dependency. On Linux, successful CLI publication sets exact mode `0755`.
- Targets `windows-x64-console-v2` and `linux-x64-console-v2` accept hosted `Main() -> i32` programs whose exact required-service set is `console.write_line`. They serialize the capability, service slot, platform adapter, ABI/runtime versions, extents, and exact native image/output-leaf digests under [`WVHC 1`](Windvale-Hosted-Console-Application.md). The Windows PE imports only `GetStdHandle` and `WriteFile`; the Linux static PIE uses direct syscalls. Both run without loading .NET, while Stage 0 remains a build-time dependency.
- Targets `windows-x64-console-v3` and `linux-x64-console-v3` accept only the exact compiler profile: one exported `Main`, the canonical six capabilities, and exact ten-service set. Their paired [hosted-compiler application contract](Windvale-Hosted-Compiler-Application.md) binds the ABI, limits, platform adapters, complete service bundle, native entry, runtime state, startup, and outer PE/ELF container. Both default to the platform extension and are independently parsed before publication. Their public writers, CLI/AOT publication boundary, and direct canonical Stage 2 reproduction without loading .NET are cross-host qualified on Windows and digest-pinned Debian 12 at exact commit `57d154c`.
- Targets `windows-x64-verifier-v1` and `linux-x64-verifier-v1` accept only the fixed [compiler-WVB verifier application profile](Windvale-Hosted-Verifier-Application.md): one exported `Main() -> i32`, five exact capabilities, and five exact fragment services. Startup adds only its internal UTF-8 validator. The containers retain one bounded file snapshot, no file-output binding, independent PE/ELF parsing, and stable verifier process output. They default to `.exe` and `.elf`; cross-host qualification is pending.
- Targets `windows-x64-build-driver-v1` and `linux-x64-build-driver-v1` accept the canonical `Windvaleˉcompilerˉbuildˉdriver` module under the fixed [`WVHB 1` build-driver profile](Windvale-Compiler-Build-Driver.md). The application composes the Windvale compiler, Project 1 parser, and portable compiler-aligned verifier in one process; its raw executable accepts explicit sources or `--project <project.wvproj> <output.wvb>` and calls file output only after verifier acceptance. The target defaults to `.exe` or `.elf`. The outer format-5 package is independently parsed and atomically published by this Stage 0 command; the WVB produced later by the raw driver uses its separately specified non-atomic `file.write_bytes` capability. Cross-host qualification is pending.
- `compile` requires `.wv` input and the target's `.wvb`, `.exe`, or `.elf` output extension, rejects duplicate source paths, and refuses to overwrite any source input. Executable containers are written and prepared under a unique sibling name before one atomic replacement; compilation, import, native selection, linking, packaging, or prepublication metadata failure leaves the requested executable output missing or unchanged.
- `build` reads one bounded strict-UTF-8 Windvale Project 1 manifest, resolves its explicit root and dependency paths relative to the manifest, and passes that exact source set through the same compile and mandatory-verification path. The default output replaces `.wvproj` with `.wvb`. Project metadata and paths do not enter WVSS or WVB; [the project specification](Windvale-Project.md) defines the format, limits, and `WVP` diagnostics.
- `build` requires `.wvproj` input and `.wvb` output paths. A manifest, path, source, import, compilation, or verification failure does not create or modify the output.
- `aot` reads one bounded `.wvb`, performs complete structural and semantic verification, lowers it once through the shared native backend, and sends the same verified fragment/capability evidence through the selected writer and atomic executable publisher used by `compile`. Input and output must differ and use `.wvb` plus the target's `.exe` or `.elf` extension. Verification failure produces no output; native selection or packaging failure leaves the requested output missing or unchanged.
- `assemble` reads strict UTF-8 WVA 1 source, validates and encodes it through the Stage 0 assembler, verifies the generated WVO, and writes deterministic bytes. The default output replaces `.wva` with `.wvo`; input and output paths must differ.
- `link` reads one or more bounded `.wvo` inputs in explicit command order, verifies every object, resolves imports, lays out and relocates the `flat-x86-64-v1` memory image at the required decimal `u32` base address, independently verifies the complete image, and writes one distinct `.bin` output. Success writes the canonical path-free link map to standard output. Link failure writes no map and does not create or modify the image.
- `inspect` validates the module structure and prints canonical human-readable metadata and disassembly. It does not execute the module.
- `verify` performs complete structural and bytecode verification. Success prints the module name and SHA-256 digest.
- `object-inspect` performs complete WVO structural verification and prints the architecture, sections, symbols, relocations, and SHA-256 digest. It does not link or execute the object.
- `object-verify` performs complete WVO structural verification and prints the architecture and SHA-256 digest.
- `run` verifies before execution and invokes exported `Main() -> i32`.
- Hosted capabilities must be granted individually with `--allow`. Declaring a capability in the module does not authorize it.
- `--bind-read-only-directory <path>` materializes one rights-limited immutable reference-host snapshot for `filesystem.directory_read_v1`. Binding does not grant authority; the capability still requires its own `--allow`. The provider retains at most 4,096 queryable immediate entries and 64 MiB of regular-file bytes, uses exact ordinal names, does not follow links, and exposes no native path to Windvale source.
- `--max-steps` is a positive integer and defaults to 1,000,000 executed instructions.
- `--report-steps` prints the exact executed-instruction count after a successful run. It is opt-in so existing scripts keep stable output.
- `--report-function-steps` writes one deterministic standard-error line for every executed function, ordered by descending instruction count then function index. It reports partial counts after a runtime failure and is opt-in; the default runtime does not allocate function counters.
- Arguments after `--` are passed to the module as its immutable hosted argument snapshot. They are not interpreted as launcher options.
- The native file adapters resolve `file.read_bytes` and `file.write_bytes` resource names with host path rules and enforce the 4 MiB byte-value limit at the boundary. Writing creates or replaces the named file only after an explicit capability grant.
- `console.write` and `console.write_line` use standard output; `diagnostic.write_line` uses standard error. Windvale line output appends LF on every host.
- Successful `run` prints a final `Result: <i32>` line after program output.

## Exit codes

```text
0 command completed successfully
1 source compilation, assembly, or linking failed
2 module verification failed
3 runtime failed or a capability was not authorized
64 command usage was invalid
70 an unexpected internal software failure occurred
74 input or output failed
```

Diagnostics are written to standard error. Normal command output and program console output are written to standard output.
