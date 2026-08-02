# First runtime-supplied Windvale OS bytecode interpreter

## Status and purpose

Interpreter profile 3 is the first Windvale OS runtime that obtains its WVB input at execution time instead of carrying the program bytes inside its own WVB or linked RX image. [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is hosted Windvale source with exactly one declared capability, `file.read_bytes`, and exactly one resource name, `boot:main.wvb`. Its AOT derivative runs at CPL3, receives the resource through the unchanged ABI-16 native service table, validates the bounded WVB profile, and executes it.

[Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) owns profile 3. [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) retains the qualified embedded-input profile-2 proof.

The interpreter itself remains an AOT boot component; the separately mapped program is interpreted. The kernel contains neither a language interpreter nor WVB semantic decoding.

## Runtime-input contract

Profile 3 performs one `file.read_bytes("boot:main.wvb")` call before parsing. Windows and Linux reference tests provide that resource through an immutable in-memory reader. Windvale OS provides the same logical capability through:

- native execution context offset `96`, pointing to one OS-private `WVBR` version-1 table;
- ABI-16 service-table slot `file.read_bytes`, pointing to one exact 199-byte x86-64 leaf;
- a 32-byte little-endian table containing magic/version/size, one data pointer, one length, and a zero reserved word; and
- one separate 4 KiB user-readable, read-only, non-executable page containing the admitted WVB followed by zeros.

The leaf accepts only the exact 13-byte UTF-8 name `boot:main.wvb`. It checks the table pointer, magic/version, 32-byte size, zero reserved field, nonzero data pointer, and length from 12 through 4,096 bytes before returning an ABI-16 borrowed-bytes descriptor. Wrong names fail with native file-not-found detail `6`; unavailable or malformed tables fail with detail `8`. The leaf preserves ABI-16 context and budget registers `R10`, `R11`, and `R15`.

The service is authored as a read-only WVA stencil because WVA deliberately forbids arbitrary byte statements in code sections. Stage 0 accepts only the exact 199-byte, relocation-free stencil and publishes those already verified bytes as one function object. This publication adapter is explicit bootstrap machinery, not a general code-generation or executable-publication API.

## Accepted input and execution

After acquisition, profile 3 retains profile 2's section-derived validation and execution rules:

- total length from 12 through 4,096 bytes, magic `WVB1`, format version `1.6`, and seven sections;
- exactly seven ordered section kinds, zero flags/reserved fields, checked payload ranges, and no trailing bytes;
- a portable module with a nonempty module name no longer than 255 bytes;
- empty capability, data, and type sections;
- exactly one `Main() -> i32` function, one `i32` local, maximum stack depth one, a code range covering the complete code payload, and exactly one matching function export;
- section-derived code bounds, opcode-specific operand bounds, scalar stack depth, local index `0`, and initialization; and
- one final return exactly at the code-envelope end.

The only accepted instruction forms are `i32.const`, `local.store 0`, `local.load 0`, and `return`. Canonical execution returns `29` after exactly `4,678` verified Windvale instructions with maximum dynamic call depth `3`.

Focused tests compile the interpreter once and vary only its supplied immutable WVB. They cover truncation, bad magic, inconsistent section length, unknown opcode, changed constant, changed local operand, and a second compiler-produced module whose longer name moves the code payload. Artifact tests also prove that the complete 174-byte admitted WVB occurs neither in the interpreter WVB nor in its linked RX client image.

## Artifact and process bounds

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| WVA read-only service stencil WVO | 314 | `1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816` |
| Published service code WVO | 314 | `610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d` |
| Linked normal client image | 128,157 | `5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655` |
| Linked deliberate-fault image | 128,157 | `1a56e471c06702e479ec7c1cee49d98415734e7d5fca24f46fbc3c66c8175a83` |

The process maps 32 RX pages, four RW/NX stack pages, one RW/NX execution-context page, and one RO/NX runtime-input page. It has no writable executable page and no executable-publication capability.

## Trust boundary and deliberate limits

The fixed AOT admission policy still checks every byte of the canonical WVB before process construction. The planner independently verifies that the supplied resource hashes to the program identity recorded in `WVPROC05`. The interpreter then decodes its semantic subset after fetching the resource.

Profile 3 is not an arbitrary resource namespace, filesystem, complete WVB verifier, multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. The resource is fixed at boot and borrowed immutably for the process lifetime. Generalization must decide whether an init/package service should transfer handles or bytes, expand semantic coverage under verify-before-execute, and keep executable publication behind the kernel's W^X boundary.
