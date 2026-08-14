# Decision 0541: First ABI-23 storage-describe execution

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows execution and Linux image evidence
- Requires: [Decision 0540](0540-First-Abi-23-Storage-Call-Lowering.md)
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: First execution of [`storage.random_access_v1`](../../Specifications/Random-Access-Storage-Capability.md) through [`WVPT 1`](../../Specifications/Windvale-Native-Capability-Provider-Table.md)
- Retains: Every ABI-22 startup, package, service, and generated object unchanged

## Context

The main lowerer emitted the exact ABI-23 provider call, but the evidence ended
at a verified WVO. The next smallest useful boundary was not yet real file I/O:
it was proving that an existing hosted process could derive a context-9 call,
bind one exact provider table, cross the five-cell target ABI, receive an owned
response, and return through generated Windvale code.

Changing the shared hosted container first would have exposed every ABI-22
consumer to a candidate context and capability format. A focused bridge can
exercise the successor boundary without changing any existing startup byte.

## Decision

- Add one platform-neutral x64 execution probe. It allocates a 328-byte aligned
  stack frame containing Windows shadow space, a 136-byte context-9 value, an
  exact 94-byte one-entry `WVPT 1` table, and a 48-byte provider response.
- Copy only the admitted 112-byte context-7 prefix, set context version 9 and
  size 136, keep allocator offsets 112 and 120 zero, and bind the table pointer
  at offset 128 for the duration of one call.
- Construct the exact `storage.random_access_v1` identity and a nonzero local
  target/state pair. The state owns no path or host handle.
- Admit only `Describe(0, 0, 0, 0, empty)`. Revalidate the five complete cells,
  preserve `R10`, `R11`, and `R15`, and publish a complete `WVSA 1` response
  describing a fixed 4 KiB object.
- Make the Windvale fixture validate all twelve fixed response fields after the
  real generated provider call. A zero process result is the execution proof.
- Keep this probe test-only. It is not a file-backed provider, writer fence,
  revocation mechanism, product container binding, or storage durability claim.
- Do not treat legacy hosted packaging as ABI-23 admission. It does not yet
  prove WVB capability identity against the constructed provider table.

## Evidence

The fixture builds twice to the same 1,741-byte WVB and lowers twice with
`abi=23` to the same verified 12,342-byte WVO. The provider bridge assembles
twice to the same verified 940-byte WVO with SHA-256
`f11b5a54d967948f21b6edaad12494225f1987b210285eba342c5fcde1e9c113`.
The linked image is 12,928 bytes and places the probe entry at offset 12,192.

The 30,720-byte Windows package executes with result zero. The same WVB, native
image, entry, and bridge construct a 32,768-byte Linux package; independent
Linux execution remains part of the pushed dual-host gate. The focused native
database-storage owner advances from seven to eight cases.

The current-source lowerer compiled in 17.0 seconds and its segmented hosted
package took 56.9 seconds on the development machine. The 1,741-byte fixture
itself compiled in under one second. Reconstruction cost remains a separate
throughput problem rather than evidence that ordinary WVB compilation is fast.

## Consequences

- ABI 23 is now an executed native boundary, not only emitted data, while every
  existing ABI-22 artifact remains unchanged.
- The next provider slice can replace the describe-only state with one
  pre-opened Windows/Linux object and retain the same generated call shape.
- Product publication must add exact capability-to-provider admission before
  it can expose this path outside the focused test.
- Compiling the complete typed random-access storage library currently reaches
  a separate native-lowering rejection (`function=9 detail=7`). That compiler
  gap is retained as work; the library is not weakened to hide it.

## Reconsideration triggers

Replace the focused bridge when the ordinary host constructs context 9 and
`WVPT 1` from independently admitted WVB metadata, when provider state moves to
an isolated service, or when another capability needs a different response
ownership or call convention.
