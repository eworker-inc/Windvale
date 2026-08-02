# First Windvale OS bytecode interpreter

## Status and purpose

Interpreter profile 1 is the first canonical WVB computation interpreted inside Windvale OS. [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is portable Windvale source compiled AOT into the process-2 image. At CPL3 it decodes and executes the exact 174-byte WVB admitted earlier by the trusted boot policy. [Decision 0093](../Documents/Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) owns this bounded profile.

The interpreter itself is an AOT boot component; the embedded program is interpreted. This distinction keeps the kernel free of a language interpreter while proving that guest execution no longer depends on the admitted program's host-built AOT derivative.

## Accepted input and execution

Profile 1 accepts exactly one immutable embedded byte sequence with SHA-256 `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2`. It checks:

- total length `174`, magic `WVB1`, format version `1.6`, and seven sections;
- the fixed code-item count and 16-byte code payload envelope at offsets `113` and `117`;
- bounded cursor movement within offsets `121..136`;
- opcode-specific operand bounds, scalar stack depth, local index `0`, and initialization; and
- one final return exactly at the code-envelope end.

The only accepted instruction forms are `i32.const`, `local.store 0`, `local.load 0`, and `return`. The interpreter holds one scalar operand-stack cell and one initialized local cell. Canonical execution returns `29` after exactly `567` verified Windvale instructions with maximum dynamic call depth `2`.

Malformed inputs return stable negative values. Focused tests cover truncation, bad magic, inconsistent code length, unknown opcode, changed constant, and changed local operand. A changed but structurally accepted constant returns the changed computation (`28`), proving execution is derived from decoded bytes rather than a hard-coded final result.

## Artifact and process bounds

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 3,211 | `639e191af1844b6660750978854f5e168c25f4949f1d9282ca5777d65f617083` |
| Interpreter WVO | 30,457 | `fbe3592e5459723c2b36330ec93659fb387de497b31fa59b8e629668297aaac6` |
| Linked normal image | 30,270 | `72f81045c525f1ad055127f3bb7917dace22b0a3b35ff3b6fefec28b37a6058c` |
| Linked deliberate-fault image | 30,270 | `b24007c770c1ff9d0c8a05702a6b05ead8a9361f55b6394b34cc3202343622aa` |

The process maps eight RX pages, two RW/NX stack pages, and one RW/NX execution-context page. One stack page was insufficient for the measured ABI-16 native frame, so profile 1 records the two-page requirement explicitly. The process has no writable executable page and no capability to publish code.

## Trust boundary and deliberate limits

Trusted AOT admission still checks every byte of the exact WVB before the interpreter process starts. The interpreter then independently decodes its bounded execution subset. C# Stage 0 embeds, compiles, links, and checks the artifacts, but it is not invoked by the guest interpreter path.

Profile 1 is not an arbitrary WVB loader, general section reader, complete verifier, multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. It does not accept a user pointer or boot-resource handle. Generalization must replace fixed offsets with bounded format decoding, preserve verify-before-execute, retain deterministic resource accounting, and keep code publication behind the kernel's W^X boundary.
