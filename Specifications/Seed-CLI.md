# Windvale Seed command-line contract

## Commands

```text
windvale compile <source.wv> [--module <dependency.wv>]... [-o <module.wvb>]
windvale build <project.wvproj> [-o <module.wvb>]
windvale assemble <source.wva> [-o <object.wvo>]
windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...
windvale inspect <module.wvb>
windvale verify <module.wvb>
windvale object-inspect <object.wvo>
windvale object-verify <object.wvo>
windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] [--report-steps] [--report-function-steps] [-- <argument>...]
windvale help
```

## Behavior

- `compile` reads the strict UTF-8 root source and every explicit repeated `--module` dependency, resolves the complete bounded import graph, composes one module, verifies the generated WVB, and writes deterministic bytes. It performs no implicit source lookup. The default output replaces the root source extension with `.wvb`.
- `compile` requires `.wv` input and `.wvb` output paths, rejects duplicate source paths, and refuses to overwrite any source input. Compilation or import failure does not create or modify the output.
- `build` reads one bounded strict-UTF-8 Windvale Project 1 manifest, resolves its explicit root and dependency paths relative to the manifest, and passes that exact source set through the same compile and mandatory-verification path. The default output replaces `.wvproj` with `.wvb`. Project metadata and paths do not enter WVSS or WVB; [the project specification](Windvale-Project.md) defines the format, limits, and `WVP` diagnostics.
- `build` requires `.wvproj` input and `.wvb` output paths. A manifest, path, source, import, compilation, or verification failure does not create or modify the output.
- `assemble` reads strict UTF-8 WVA 1 source, validates and encodes it through the Stage 0 assembler, verifies the generated WVO, and writes deterministic bytes. The default output replaces `.wva` with `.wvo`; input and output paths must differ.
- `link` reads one or more bounded `.wvo` inputs in explicit command order, verifies every object, resolves imports, lays out and relocates the `flat-x86-64-v1` memory image at the required decimal `u32` base address, independently verifies the complete image, and writes one distinct `.bin` output. Success writes the canonical path-free link map to standard output. Link failure writes no map and does not create or modify the image.
- `inspect` validates the module structure and prints canonical human-readable metadata and disassembly. It does not execute the module.
- `verify` performs complete structural and bytecode verification. Success prints the module name and SHA-256 digest.
- `object-inspect` performs complete WVO structural verification and prints the architecture, sections, symbols, relocations, and SHA-256 digest. It does not link or execute the object.
- `object-verify` performs complete WVO structural verification and prints the architecture and SHA-256 digest.
- `run` verifies before execution and invokes exported `Main() -> i32`.
- Hosted capabilities must be granted individually with `--allow`. Declaring a capability in the module does not authorize it.
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
