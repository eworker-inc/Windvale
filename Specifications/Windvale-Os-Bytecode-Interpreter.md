# Init-granted Windvale OS bytecode interpreter

## Status and purpose

Interpreter runtime profile 4 is the first Windvale OS runtime that begins without its WVB mapping or usable service pointers and receives them through an init-selected, kernel-mediated immutable grant. [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) remains hosted Windvale source with exactly one declared capability, `file.read_bytes`, and exactly one resource name, `boot:main.wvb`. Its byte-identical AOT derivative runs at CPL3 after the grant, validates the bounded WVB profile, and executes it.

Profile 4 is cross-host qualified through probe 27 and remains byte-identical in candidate probe 28. Probe 28 adds terminal cleanup around the interpreter without changing its WVB, WVO, linked images, accepted semantics, service leaf, or budgets.

Qualified [Decision 0096](../Documents/Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) owns profile 4. [Decision 0097](../Documents/Decisions/0097-First-Terminal-Resource-Borrow-Revocation.md) composes it unchanged with protected-process version 7.

The interpreter itself remains an AOT boot component; the separately mapped program is interpreted. The kernel contains neither a language interpreter nor WVB semantic decoding.

## Runtime-input contract

Profile 4 performs the same one `file.read_bytes("boot:main.wvb")` call before parsing. Windows and Linux reference tests provide that resource through an immutable in-memory reader. Windvale OS starts process `2` with context offsets 24 and 96 zero and its target PTE absent. Windvale init selects resource identifier `1`; fixed grant syscall `4` then provides the same logical capability through:

- native execution context offset `96`, pointing to one OS-private `WVBR` version-1 table;
- ABI-16 service-table slot `file.read_bytes`, pointing to one exact 199-byte x86-64 leaf;
- a 32-byte little-endian table containing magic/version/size, one data pointer, one length, and a zero reserved word; and
- one user-readable, read-only, non-executable alias of init's owned 4 KiB page containing the admitted WVB followed by zeros.

The grant publishes the service table, `WVBR` table, and both context pointers as one checked transition before the interpreter enters. Init remains the resource owner; process `2` is the single recorded borrower while it runs. After ordinary exit or contained fault, probe 28 clears the alias, both pointers, and both private tables before init resumes. This is one bounded immutable borrow, not a general capability or page-ownership transfer.

The leaf accepts only the exact 13-byte UTF-8 name `boot:main.wvb`. It checks the table pointer, magic/version, 32-byte size, zero reserved field, nonzero data pointer, and length from 12 through 4,096 bytes before returning an ABI-16 borrowed-bytes descriptor. Wrong names fail with native file-not-found detail `6`; unavailable or malformed tables fail with detail `8`. The leaf preserves ABI-16 context and budget registers `R10`, `R11`, and `R15`.

The service is authored as a read-only WVA stencil because WVA deliberately forbids arbitrary byte statements in code sections. Stage 0 accepts only the exact 199-byte, relocation-free stencil and publishes those already verified bytes as one function object. This publication adapter is explicit bootstrap machinery, not a general code-generation or executable-publication API.

## Accepted input and execution

After acquisition, profile 4 retains profile 3's section-derived validation and execution rules:

- total length from 12 through 4,096 bytes, magic `WVB1`, format version `1.6`, and seven sections;
- exactly seven ordered section kinds, zero flags/reserved fields, checked payload ranges, and no trailing bytes;
- a portable module with a nonempty module name no longer than 255 bytes;
- empty capability, data, and type sections;
- exactly one `Main() -> i32` function, one `i32` local, maximum stack depth one, a code range covering the complete code payload, and exactly one matching function export;
- section-derived code bounds, opcode-specific operand bounds, scalar stack depth, local index `0`, and initialization; and
- one final return exactly at the code-envelope end.

The only accepted instruction forms are `i32.const`, `local.store 0`, `local.load 0`, and `return`. Canonical execution returns `29` after exactly `4,678` verified Windvale instructions with maximum dynamic call depth `3`.

Focused tests compile the interpreter once and vary only its supplied immutable WVB. They cover truncation, bad magic, inconsistent section length, unknown opcode, changed constant, changed local operand, and a second compiler-produced module whose longer name moves the code payload. Artifact tests also prove that the complete 174-byte admitted WVB occurs in neither the interpreter WVB, linked init image, nor linked client image. Grant and cleanup tests lock the absent pre-grant target, exact RO/NX alias, the permitted hardware accessed bit, exact tables, one-shot transition, terminal PTE removal, and zeroed private publication.

## Artifact and process bounds

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| WVA read-only service stencil WVO | 314 | `1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816` |
| Published service code WVO | 314 | `610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d` |
| Linked normal client image | 128,157 | `5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655` |
| Linked deliberate-fault image | 128,157 | `1a56e471c06702e479ec7c1cee49d98415734e7d5fca24f46fbc3c66c8175a83` |

The process begins with 32 RX pages, four RW/NX stack pages, and one RW/NX execution-context page. The grant adds one RO/NX runtime-input alias, bringing the user-page count from 37 to the recorded budget of 38. It has no writable executable page and no executable-publication capability.

## Trust boundary and deliberate limits

The fixed AOT admission policy still checks every byte of the canonical WVB before process construction. The owner planner verifies that the init page hashes to the identity recorded in both `WVPROC07` records. Grant syscall `4` revalidates the exact kernel-owned `WVRES002` record, absent client PTE, bounds, digest, flags, service leaf, owner, borrower, and counts before publication. The interpreter then decodes its semantic subset after fetching the resource. Terminal cleanup revalidates the same live borrow, accepting only the processor-maintained leaf accessed bit in addition to the exact grant.

Profile 4 is not an arbitrary resource namespace, filesystem, complete WVB verifier, multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. The resource is fixed at boot, selected by identifier `1`, and borrowed immutably until the sole client becomes terminal. Generalization must add typed lookup only when a second real resource requires it, define reclamation/root reuse when teardown demands it, expand semantics under verify-before-execute, and keep executable publication behind the kernel's W^X boundary.
