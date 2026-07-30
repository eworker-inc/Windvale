# Windvale Seed command-line contract

## Commands

```text
windvale compile <source.wv> [-o <module.wvb>]
windvale assemble <source.wva> [-o <object.wvo>]
windvale inspect <module.wvb>
windvale verify <module.wvb>
windvale object-inspect <object.wvo>
windvale object-verify <object.wvo>
windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] [-- <argument>...]
windvale help
```

## Behavior

- `compile` reads strict UTF-8 source, compiles it, verifies the generated module, and writes deterministic bytes. The default output replaces the source extension with `.wvb`.
- `compile` requires `.wv` input and `.wvb` output paths and refuses to overwrite its source path.
- `assemble` reads strict UTF-8 WVA 1 source, validates and encodes it through the Stage 0 assembler, verifies the generated WVO, and writes deterministic bytes. The default output replaces `.wva` with `.wvo`; input and output paths must differ.
- `inspect` validates the module structure and prints canonical human-readable metadata and disassembly. It does not execute the module.
- `verify` performs complete structural and bytecode verification. Success prints the module name and SHA-256 digest.
- `object-inspect` performs complete WVO structural verification and prints the architecture, sections, symbols, relocations, and SHA-256 digest. It does not link or execute the object.
- `object-verify` performs complete WVO structural verification and prints the architecture and SHA-256 digest.
- `run` verifies before execution and invokes exported `Main() -> i32`.
- Hosted capabilities must be granted individually with `--allow`. Declaring a capability in the module does not authorize it.
- `--max-steps` is a positive integer and defaults to 1,000,000 executed instructions.
- Arguments after `--` are passed to the module as its immutable hosted argument snapshot. They are not interpreted as launcher options.
- The native file adapters resolve `file.read_bytes` and `file.write_bytes` resource names with host path rules and enforce the 4 MiB byte-value limit at the boundary. Writing creates or replaces the named file only after an explicit capability grant.
- `console.write` and `console.write_line` use standard output; `diagnostic.write_line` uses standard error. Windvale line output appends LF on every host.
- Successful `run` prints a final `Result: <i32>` line after program output.

## Exit codes

```text
0 command completed successfully
1 source compilation or assembly failed
2 module verification failed
3 runtime failed or a capability was not authorized
64 command usage was invalid
70 an unexpected internal software failure occurred
74 input or output failed
```

Diagnostics are written to standard error. Normal command output and program console output are written to standard output.
