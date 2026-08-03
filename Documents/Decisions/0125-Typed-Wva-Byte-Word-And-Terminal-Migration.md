# Decision 0125: Typed WVA byte/word operations and terminal migration

- Date: 2026-08-02
- Status: Implemented locally; cross-host and pinned-QEMU qualification pending

## Context

[Decision 0121](0121-Deterministic-Wva-Scalar-And-Memory-Operands.md) deliberately stopped at 32- and 64-bit operands while the independent C# oracle and Windvale-written assembler established deterministic immediate, shift, multiply, and SIB-memory behavior. That was an implementation slice, not an architectural reason to prohibit smaller scalar widths.

The retained kernel exception terminal supplied concrete pressure for the next boundary. Its normalized-frame policy was already expressible, but its polled COM1 writer required byte loads, `AL` port input/output, an 8-bit line-status test, and a bounded byte loop. Keeping a 4,126-byte raw C# machine-code emitter merely because WVA lacked those operations would preserve bootstrap ownership after the blocker was understood. Adding an opaque executable-byte directive would avoid the immediate parser work while weakening semantic validation, differential encoding, and reproducibility.

## Decision

- Retain WVA version 1 and WVO version 1.0. This expansion adds source statements and encoder rules but changes no serialized object contract.
- Admit the exact 8-bit register family `al cl dl bl spl bpl sil dil r8b..r15b` and the exact 16-bit family `ax cx dx bx sp bp si di r8w..r15w`.
- Reject legacy high-byte registers `ah`, `ch`, `dh`, and `bh`. Byte encodings force a REX prefix whenever an operand is `spl`, `bpl`, `sil`, or `dil`, and emit the ordinary extension bits for `r8b` through `r15b`, so an admitted name cannot silently select a legacy high-byte register.
- Extend same-width move, arithmetic, logical, compare, test, RIP-relative load/store, deterministic SIB load/store, and immediate rotate/shift operations to byte and word operands.
- Add exact unsigned `move_u8`/`move_u16` and signed `_i8`/`_i16` ALU, compare, and test immediates. Reject values outside the declared width instead of truncating them.
- Admit two-operand signed `multiply` for 16 bits while retaining its existing 32/64-bit forms. Do not invent an 8-bit analogue for x86's implicit-register multiply forms.
- Add `set_condition` into an exact byte register plus `zero_extend_u8`, `zero_extend_u16`, `sign_extend_i8`, and `sign_extend_i16` into exact 32/64-bit destinations.
- Add semantic `in_u8` and `out_u8` statements for the implicit `DX`/`AL` x86 port boundary. Keep their authority system-profile-only and explicit; ordinary Windvale source receives no port capability.
- Implement every form independently in `Assembler/Reference` and `Assembler/Windvale`. Do not share selected instruction bytes between the oracle and product implementation.
- Migrate `Windvale_kernel_x64_exception_terminal`, its three read-only marker records, and one local serial writer into `Operating-System/Kernel/X64-Kernel-Shims.wva`. Retain only checked IDT clearing, descriptor construction, live-CS capture, `CLI`, and `LIDT` in the version-3 Stage 0 exception object.
- Keep division, variable-count shifts, general 64-bit immediates, conditional moves, legacy high-byte registers, address expressions, displacement relaxation, raw executable bytes, and generic privileged operations outside this change.

## Consequences

WVA can now express practical typed scalar routines across all ordinary general-register widths while preserving one semantic syntax and deterministic encoding policy. Existing WVA sources, WVO readers, linker behavior, and relocation records remain valid and byte-identical.

The kernel terminal migration removes 4,126 bytes of hand-appended C# terminal machine code. Its WVA form uses 164 terminal bytes, one 48-byte local writer, and 162 read-only marker bytes; linker/file alignment determines the final firmware-size reduction. Stage 0 still constructs and independently verifies the object graph, but it no longer owns those terminal instruction bytes or marker publication.

The migration is intentionally machine mechanics rather than general exception policy. A future `.wv` dispatcher still needs explicit unsafe bounded memory and a specified kernel call convention. Other raw emitters now require individual review: the absence of byte/word access is no longer a sufficient blocker, but descriptor/MSR publication, broader implicit-register instructions, checked state transitions, or ABI evidence may still justify a bounded seam.

The Windvale assembler still uses repeated immutable passes and construction. This change does not raise source or object limits without measurement; bounded label tables and a shared production encoder with the native backend remain separate architecture work.

## Verification

Acceptance requires:

- exact bytes for low, forced-REX, and extended byte registers plus low and extended word registers;
- every byte/word ALU immediate group, multiply/shift group, condition code, and zero/sign-extension family;
- byte/word RIP-relative relocations and deterministic SIB addressing with low/extended base, index, and value registers;
- exact immediate and count boundaries, same-width enforcement, legacy-high-byte rejection, invalid 8-bit multiply rejection, data-symbol ownership, and malformed memory shapes;
- complete Windvale/C# WVO equality for the typed canonical example and every accepted/rejected differential case;
- independent WVO verification, exact linked bytes, and deterministic repeated output;
- exact kernel WVA sections, symbols, definitions, relocations, terminal loop opcodes, installer-only exception object, and all four deterministic firmware images; and
- the repository Development gate on Windows, followed by the same committed archive's Windows/Debian qualification and pinned-QEMU scenarios before qualification is claimed.

The typed canonical example produces WVO SHA-256 `860680074517025c69a2a6edf1dd9ff196475e05f9c50f95b53480c848c650c5`. The composed Windvale assembler WVB has SHA-256 `99c5b138eb42523bb5e653b9fe3fb0fd37950890f1f5c3fb2ac47b998dbc33ae`.

The locally verified version-9 kernel WVA object is 1,894 bytes with SHA-256 `845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193`. The version-3 installer-only exception object is 483 bytes with SHA-256 `9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c`. The rebased Windows Development gate builds with zero warnings and passes all 75 regular Seed tests plus all 25 OS tests. This is implementation evidence, not cross-host or live-emulator qualification.

## Reconsider when

- A real consumer requires division, variable-count shifts, conditional moves, or another implicit-register family.
- Repeated encoders drift enough that the native backend and assembler need one shared typed machine-encoding contract without erasing the independent oracle.
- Measured labels or source size justify bounded tables and a single-pass byte builder.
- Cross-host or pinned-QEMU evidence exposes an ABI, instruction, or timing assumption absent from the local structural suite.
