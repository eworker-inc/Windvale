# Decision 0695: Bound FAT32 cluster-chain admission

Status: Accepted

Date: 2026-08-16

## Context

Decision 0691 admits volume geometry before format reads. The next provider
increment needs exact FAT-entry location and chain classification without
loading an unbounded FAT, treating reserved values as links, or moving format
parsing into the kernel.

## Decision

- Keep FAT-entry location and chain admission in the isolated filesystem
  service's portable policy layer.
- Locate entries only from separately admitted geometry and the selected FAT.
- Mask the reserved high nibble before classifying every entry.
- Restrict the strict volume profile so ordinary cluster links never overlap
  `0x0FFFFFF0` through `0x0FFFFFF6`.
- Require an exact ordered trace of raw entries, explicit EOC, no trailing
  entry, geometry-bounded links, cycle rejection, and a caller-selected ceiling
  no larger than 4,096 clusters.
- Report free, reserved, bad, out-of-range, cyclic, truncated, trailing, and
  over-budget states distinctly.

The 6,359-byte policy WVB has SHA-256
`75470d2a1c48c86754e2f91cd5919306fe73d76c567b87f7490fc87cc1eeeb1a`.
The paired volume policy is 7,654 bytes at SHA-256
`564793e2af919a9adf7623f28775f653ac89cc642c5bb0cd22624cde896645e8`.
Their 45-case shared native owner returns 47 and pins paired Windows/Linux
images.

## Consequences

The next block-provider loop can fetch only the exact FAT sectors needed and
submit a bounded trace for deterministic admission. This decision does not
claim block I/O, mirrored-copy comparison, directory parsing, file reads, or a
writable FAT implementation.

## Reconsideration triggers

Reconsider the 4,096-cluster operation ceiling or the strict high-cluster
compatibility limit only when a named workload cannot be served by chunked
reads and retains equivalent memory, work, cycle, and teardown bounds.
