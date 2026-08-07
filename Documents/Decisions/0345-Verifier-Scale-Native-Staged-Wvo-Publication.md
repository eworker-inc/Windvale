# Decision 0345: Verifier-scale native staged WVO publication

- Status: Accepted local implementation; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0344](0344-Native-Console-Packager-Wvo-Reconstruction.md)
- Uses: [Decision 0308](0308-Native-Wvo-Publication.md)

## Context

Decision 0300 composed the native staging producer and publisher over a small
canonical fixture. Decision 0344 then proved exact native lowering for the two
console packagers, but the 105,006-byte verifier closure exposed two distinct
larger-scale boundaries.

First, the lowerer had accumulated complete-function byte values, repeatedly
rescanned growing instruction maps, retained record evidence beyond its last
use, and kept one oversized prologue routine. These were representation and
lifetime costs rather than semantic limits. Second, the native publication
adapters retained the snapshot ordinal in `RBX` while reusing that register as
the per-chunk byte-progress cursor. A chunk requiring another native write or
reread iteration therefore used byte progress as the next snapshot-table
index. The small fixture did not exercise that transition.

## Decision

- Keep the accepted WVB, WVO, function, and 128 MiB native text-arena limits
  unchanged.
- Emit function code in bounded 4 KiB chunks, rebase retained positions at the
  chunk boundary, build instruction-position evidence linearly, and release
  record-local evidence after its final consumer.
- Pack fixed lowering templates and extract the cohesive native function
  prologue construction from the main lowering coordinator. These changes
  preserve the object format and exact emitted bytes.
- Add the missing native `u32` bitwise and shift lowering used by the complete
  hosted closure, with exact differential coverage.
- Reload the immutable snapshot ordinal before every Windows and Linux native
  write and reread iteration. Preserve byte progress separately.
- Preserve packed native-runtime status at the process boundary so a runtime
  service failure remains diagnosable while ordinary portable `Main` failures
  keep their defined exit status.
- Extend the focused publisher contract with a generated code chunk larger
  than 8,192 bytes so a future multi-iteration regression cannot hide behind
  the small fixture.
- Treat the publisher's separate self-lowering text-arena exhaustion as the
  next lifetime investigation. Do not increase the arena to make that probe
  pass.

## Evidence

The exact verifier input is 105,006 bytes at SHA-256
`1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c`.
The native staging producer packages are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 403,791 | `229c7a940bae90592eedd4d9df2bc71a0a21191ce5b0e7bdacccc7cb01515c97` |
| Windows producer | 5,921,280 | `6eaea9c7f2aaa88c7c32ed7af49a2c9bc73fb2039de539c694330f5218aca3ac` |
| Linux producer | 5,922,816 | `e8c51f1c6c7edba344d125bb92aca01ec488a8bebad1ccf3307bd1e3c7866f1e` |

The portable publisher WVB is 423,241 bytes at SHA-256
`5d18bc2618938832f0e88ff9f19c6b6577e435e9174044467ae8cb9c8a65026d`.
The refreshed native adapters and packages are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows publication adapter WVO | 9,190 | `456705dd43ae9efff21a87b75971d306f0755aa9b243879064672a5ab2298f1c` |
| Linux publication adapter WVO | 5,242 | `741ca0cfc1931648f6dad80911448f2299a1e1d5b0c0de4300328fd123f8cba1` |
| Windows publisher | 6,209,024 | `a3a7d68543221a907012e677522c513e0a1ece04f029f4605e862f9301c085f2` |
| Linux publisher | 6,209,257 | `f539d3a41d7c99fbf8255ed3eb83d67d6a068afaf4f2ad3c738f3c72bdb17c88` |

Direct Windows execution lowers and stages the verifier as seven chunks plus
the manifest, publishes the exact 1,049,615-byte WVO at SHA-256
`6a550a83ea593ca45c3c13ee6b6a4f815d88f7e05117ae50691d9025c7e45f55`,
passes independent object verification, and reconstructs byte-for-byte from
the admitted chunks. The native lowerer reports a 3,148,072-byte peak dynamic
payload, 3,148,880 charged bytes, and a 4,014,976 maximum addressed position.

After test review, the focused publisher contract passes 1/1 in 6.463 test
seconds after a 12.63-second zero-warning Release build. It covers the original
success, content mismatch, and hard-link alias rejection plus one successful
publication whose generated code chunk crosses the native 8,192-byte I/O
iteration boundary. The earlier focused bitwise, exact chunked-emission, record
planner, and small producer/publisher checks also pass 1/1 each.

A separate native attempt to lower the 423,241-byte publisher WVB fails closed
before output with packed runtime status 5 and detail 2,
`Textˉarenaˉexhausted`, at the unchanged 128 MiB boundary. That is measured
next-slice evidence, not a completed self-hosting claim.

## Consequences

The current-host source-to-staged-WVO path now handles the real verifier closure
without a .NET child, and its publication transaction has multi-iteration
regression coverage. The Linux identities are constructed and pinned, but this
decision does not claim Linux execution.

Publisher self-lowering, native host-container reconstruction, Linux execution,
promotion, Standard, Qualification, and the grouped end-of-goal gate remain
open. The Stage 0 constructors remain an explicit recovery path until those
gates pass.

## Reconsideration triggers

Revisit the chunk size or retained evidence only if a measured successor shows
a smaller coherent representation or a new format contract requires it.
Rebuild and requalify the pinned identities if lowering, allocation, object,
runtime-service, or publication contracts change.
