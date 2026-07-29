# Windvale Seed command-line contract

## Commands

```text
windvale compile <source.wv> [-o <module.wvb>]
windvale inspect <module.wvb>
windvale verify <module.wvb>
windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>]
windvale help
```

## Behavior

- `compile` reads strict UTF-8 source, compiles it, verifies the generated module, and writes deterministic bytes. The default output replaces the source extension with `.wvb`.
- `compile` requires `.wv` input and `.wvb` output paths and refuses to overwrite its source path.
- `inspect` validates the module structure and prints canonical human-readable metadata and disassembly. It does not execute the module.
- `verify` performs complete structural and bytecode verification. Success prints the module name and SHA-256 digest.
- `run` verifies before execution and invokes exported `Main() -> i32`.
- Hosted capabilities must be granted individually with `--allow`. Declaring a capability in the module does not authorize it.
- `--max-steps` is a positive integer and defaults to 1,000,000 executed instructions.
- Successful `run` prints a final `Result: <i32>` line after program output.

## Exit codes

```text
0 command completed successfully
1 source compilation failed
2 module verification failed
3 runtime failed or a capability was not authorized
64 command usage was invalid
70 an unexpected internal software failure occurred
74 input or output failed
```

Diagnostics are written to standard error. Normal command output and program console output are written to standard output.
