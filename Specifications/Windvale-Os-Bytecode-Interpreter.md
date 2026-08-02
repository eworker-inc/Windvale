# First Windvale OS bytecode interpreter

## Status and purpose

Interpreter profile 2 is the first section-derived WVB computation inside Windvale OS. [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is portable Windvale source compiled AOT into the process-2 image. At CPL3 it discovers and validates the sections of the admitted 174-byte WVB before executing its code. [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) owns profile 2; [Decision 0093](../Documents/Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) retains the qualified fixed-offset profile-1 proof.

The interpreter itself is an AOT boot component; the embedded program is interpreted. This distinction keeps the kernel free of a language interpreter while proving that guest execution no longer depends on the admitted program's host-built AOT derivative.

## Accepted input and execution

Profile 2 still boots with the immutable embedded byte sequence whose SHA-256 is `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2`, but acceptance no longer depends on its fixed offsets. It checks:

- total length from 12 through 4,096 bytes, magic `WVB1`, format version `1.6`, and seven sections;
- exactly seven ordered section kinds, zero section flags and reserved fields, checked payload ranges, and no trailing bytes;
- a portable module with a nonempty module name no longer than 255 bytes;
- empty capability, data, and type sections;
- exactly one `Main() -> i32` function, one `i32` local, maximum stack depth one, a code range covering the complete code payload, and exactly one matching function export;
- section-derived code bounds rather than literal serialized offsets;
- opcode-specific operand bounds, scalar stack depth, local index `0`, and initialization; and
- one final return exactly at the code-envelope end.

The only accepted instruction forms are `i32.const`, `local.store 0`, `local.load 0`, and `return`. The interpreter holds one scalar operand-stack cell and one initialized local cell. Canonical execution returns `29` after exactly `4,671` verified Windvale instructions with maximum dynamic call depth `3`.

Malformed inputs return stable negative values grouped by envelope, section, semantic-shape, and execution failures. Focused tests cover truncation, bad magic, inconsistent section length, unknown opcode, changed constant, and changed local operand. A changed but structurally accepted constant returns the changed computation (`28`). A second canonical module with a longer module name moves the code-section payload and still returns `29`, proving both decoded execution and section-offset independence.

## Artifact and process bounds

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 12,359 | `909e624df86e614b6f7dcaa61e75ffa685467015015bfafd7b0772ee41a89920` |
| Interpreter WVO | 128,129 | `9788ad4159d783ebc35ee5af6c73b7c294643261bc00acfcf0f33a6bdf35c140` |
| Linked normal image | 127,598 | `c293f84199fecce07c3a0dbafb6406e7c2aad3521782df7095fe8ee6ca58a0e8` |
| Linked deliberate-fault image | 127,598 | `6364289e6ddaaa125969bb27626672f08f114ebd738b7b191d2c55125c45fc6e` |

The process maps 32 RX pages, four RW/NX stack pages, and one RW/NX execution-context page. Live pinned-QEMU execution showed that the eight-function AOT interpreter page-faulted with the preceding 8 KiB stack and completed with the measured 16 KiB bound. The process has no writable executable page and no capability to publish code.

## Trust boundary and deliberate limits

Trusted AOT admission still checks every byte of the exact WVB before the interpreter process starts. The interpreter then independently decodes its bounded execution subset. C# Stage 0 embeds, compiles, links, and checks the artifacts, but it is not invoked by the guest interpreter path.

Profile 2 is not an arbitrary WVB loader, complete verifier, multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. It does not accept a user pointer or boot-resource handle; the program remains embedded in the interpreter image. Generalization must introduce a checked runtime-input transport, preserve verify-before-execute, retain deterministic resource accounting, and keep code publication behind the kernel's W^X boundary.
