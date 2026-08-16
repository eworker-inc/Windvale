# Windvale OS x86-64 process-coordinator initialization emission

## Status and scope

This contract source-owns fixture offsets 1,119 through 1,427 of the current
46,678-byte Probe 40 process machine. Together with the checked entry and
dispatcher, Windvale source now accounts for the first 1,428 bytes and the
relocation information needed to reconstruct their current fixture form.

The 309-byte slice establishes the coordinator's privileged initialization
boundary. It does not publish a process, execute the policy call, replace the
fixture in the boot image, or make filesystem/network providers live.

## Constructed state

The slice:

- preserves the memory-arena base in `R12` and derives the handoff-copy base;
- validates nonzero memory state, `WVKMEM17` magic, and state version 17;
- initializes the 112-byte native execution-context format 7 header with the
  measured policy instruction/depth budgets 21,918 and 5;
- zeros every remaining context word before exposing the context pointer;
- reserves and restores the exact 40-byte policy-call frame;
- aligns the recovered arena base to 2 MiB, requires policy token 97, and
  revalidates the memory state after the call; and
- derives the fixed init, client, and directory process records into their
  coordinator stack slots.

Every code append remains subject to the checked 65,536-byte x86-64 emission
limits. The constructor publishes code only in the emitter's `Ready` state.

## Relocation surface

The independent slice leaves eight four-byte fields zero:

- failure-branch displacement offsets `12`, `32`, `44`, `216`, `225`, `245`,
  and `257`; and
- process-policy call displacement offset `192`.

When placed at fixture offset 1,119, every failure branch resolves to absolute
code offset 33,826 with respective positive displacements `32,691`, `32,671`,
`32,659`, `32,487`, `32,478`, `32,458`, and `32,446`. The policy call uses the
full process object's relative-i32 relocation to import symbol index 12 with
addend -4. These values are reconstruction evidence, not a public ABI.

## Verification

`Test-Os-X64-Code-Emission` compiles and executes the constructor, checks its
exact 309-byte normalized payload, all field offsets and displacement-to-target
relationships, the policy import metadata, deterministic Windows/Linux console
images, and local result 52. The normalized payload has SHA-256
`e5fc847bd3843f3db571ca779059362f62e1e6fd824aef43978573173ebc2464`.

[Decision 0623](../Documents/Decisions/0623-Windvale-Owned-Process-Channel-And-Endpoint-Initialization.md)
now owns the channel and endpoint region beginning at offset 1,428. Actual
fixture replacement waits until all remaining internal branches and WVO imports
can be composed and the complete linked process object passes the pinned QEMU
scenarios.
