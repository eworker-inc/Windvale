# Windvale system-kernel target

## Status and purpose

This contract defines the Windvale-native normal path from a verified WVB 1.11
system module to the bounded x86-64 kernel-entry WVO described by
[Windvale x86-64 kernel target](Windvale-X64-Kernel-Target.md). It transfers the
special target from the frozen C# compiler without claiming a general native
backend.

The implementation is split between a portable WVB reader and a hosted target
shell. Fixed instruction fragments remain explicit architecture data; output
text, return value, code length, and relocation count are derived from the
verified input module.

## Public commands

```bat
Tools\Native\Lower-Os-Kernel-Wvb.cmd input.wvb output.wvo
```

```sh
./Tools/Native/Lower-Os-Kernel-Wvb.sh input.wvb output.wvo
```

The destination must not exist and its parent directory must exist. The
launcher verifies the input through the ordinary WVB verifier before invoking
the digest-bound platform target. It independently verifies a produced WVO and
removes a rejected or invalid candidate. It never replaces an existing file.

## Accepted WVB subset

The target accepts only canonical WVB 1.11 with seven ordered sections and:

- system profile and no profile metadata;
- exactly one `console.write_line(text) -> void` capability;
- text-only data entries;
- no nominal types;
- exactly one exported `Main() -> i32` function with no parameters;
- canonical locals and code extents;
- one repeated `text.const`, `local.store`, `local.load`, and
  `call.capability` group for each output line;
- one final `i32.const`, `local.store`, `local.load`, and `return` sequence;
- ASCII output only; and
- at most 4,096 emitted bytes, including one LF after each source text value.

Every offset, length, index, opcode, local type, capability reference, and final
section extent is revalidated. A valid WVB outside this subset is unsupported,
not malformed and not silently delegated to Stage 0.

## Generated WVO

The target emits canonical WVO 1.0 with one 16-byte-aligned `.text` section,
exports `Windvale_kernel_entry` and `Windvale_kernel_main`, and imports
`Windvale_kernel_memory_enter` and `Windvale_kernel_write_byte`. It emits one
`relative-i32` relocation with addend `-4` for the memory-entry call and one for
each derived byte-output call.

For the canonical Probe 40 source, the input WVB is 1,484 bytes at SHA-256
`7a0ef0dedba2a72177239c54fd670be82968e7c5156855bf36be7412da6d656c`.
The exact output is 12,134 bytes at SHA-256
`bf13c1b103c297e87f4aa14f5bf7eba57ef2a30caa21b4c67dba34abc0a7f7a8`.

## Focused evidence

`Test-Os-Kernel-Target` owns seven cases: exact native source/WVB/WVO
construction, existing-output preservation, rejection of a different valid
WVB, and direct truncated, trailing-data, invalid-UTF-8, and invalid-stack-bound
rejections without publication. Cross-host execution belongs to the final grouped
retirement gate; current-host evidence alone does not promote the target.
