# Decision 0048: First kernel handoff and relative UEFI link

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Firmware probe version 3 continued executing after `ExitBootServices`, but all code still lived in one bootstrap object and no kernel boundary existed. The compiler-native integration goal needs a replaceable object-level seam: the OS loader must import a named kernel entry, the kernel artifact must export it, and the existing linker must resolve the call before PE32+ packaging.

UEFI application format version 1 rejected every link containing imports, multiple sections, or WVO relocations. A resolved x86-64 relative call is nevertheless position-independent when the complete PE image moves, while an absolute address would require a real PE base-relocation rule. The first extension must admit the former without silently admitting the latter or non-code section layouts.

## Decision

- Define kernel handoff version 1 with ASCII symbol `Windvale_kernel_entry`, one immutable 48-byte record in `RCX`, a 16-byte-aligned caller stack with 32-byte shadow space, and `RAX = 0` success.
- Carry only the validated retained memory-map address, byte length, descriptor stride, descriptor version, magic, version, record size, and reserved field. Pass no boot-service or invalidated system-table pointer.
- Construct the loader and first kernel entry as separate WVO objects. Link one imported/exported function pair through `relative-i32` with addend `-4` at an x86-64 `call rel32` displacement.
- Advance the UEFI application adapter to version 2. Accept one or more linked sections only when every section is code, every applied relocation is `relative-i32`, and the absolute relocation count is zero. Continue rejecting read-only data, writable data, zero-fill, absolute relocations, invalid entries, and nonzero link bases.
- Extend verified link evidence with code-section, absolute-relocation, and relative-relocation counts so the target adapter can enforce that boundary without parsing source objects again.
- After successful firmware exit, build the record in the still-live loader frame, call the imported kernel symbol, and require the kernel object to revalidate the record envelope before emitting `kernel-entry=pass`.
- Keep the first kernel implementation in the private Stage 0 x86-64 builder. It is a replaceable integration oracle, not compiler-generated Windvale code and not a second general assembler.

## Consequences

The accepted QEMU evidence now crosses a real object and symbol boundary after firmware shutdown. The loader and kernel entry are independently verified WVO inputs, the existing linker applies their relative relocation, the independently verified PE32+ adapter reproduces the linked image, and the callee validates the retained-map handoff before returning success.

This establishes the precise destination for the compiler agent: a code-only x86-64 WVO object exporting `Windvale_kernel_entry` and obeying handoff version 1. It does not claim that such a compiler object exists yet. The compiler currently emits verified WVB from WVIR, and native WVO output remains a separate compiler milestone.

UEFI application version 2 still cannot encode data sections or load-address-dependent addresses. A real compiler-produced object that needs static data, absolute address materialization, unwind data, or another relocation kind requires another explicit target-adapter decision and independent verification.

## Reconsider when

- The first compiler-produced kernel object requires a minimal read-only data layout or a specified address-materialization relocation.
- A kernel needs to retain the handoff beyond the loader stack lifetime or switch stacks before validation.
- The general Windvale native ABI supplies a better compatible entry convention.
- A second architecture or firmware environment cannot use this x86-64 relative-call boundary.
