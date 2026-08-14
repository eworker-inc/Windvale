# Decision 0554: Content-addressed hosted-application development checkpoints

Status: Accepted

Date: 2026-08-14

Extends: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

Decision 0553 reduced the measured database development owner from 1,111.135
seconds to 190.863 seconds while preserving its real storage and recovery
behavior. Linking, hosted-container composition, and repeated application
execution remained. Two deterministic `Package-Hosted-Wvb` image-mode calls
accounted for 65.106 seconds of the next measured warm run.

A reusable application must not trust only a filename, WVB digest, or linked
image digest. Hosted publication also consumes the selected target and profile,
native entry, exact packaging driver, 72-artifact toolset, enum-request family,
target services, and target startup object.

## Decision

Add a host-local, content-addressed hosted-application checkpoint for eligible
development verification:

- derive a versioned length-framed SHA-256 key over the host, target, profile,
  entry, WVB, ordered native fragments, packaging driver, verified 72-entry
  toolset inventory and artifacts, enum-request family, target service leaves,
  and startup object;
- require canonical non-link inputs, canonical decimal values, one through
  eight bounded image fragments, and exact inventory agreement;
- publish the application and its complete target, size, and digest record into
  one immutable host-scoped entry;
- on every hit, reject links and malformed or oversized records, rehash the
  complete application, compare the exact manifest, materialize a fresh
  byte-identical executable, and retain executable mode on Linux;
- fail closed on an invalid existing entry without repairing or overwriting it;
  and
- use the checkpoint only for the current-host application in the two-case
  database development owner. Continue to link fresh inputs and execute every
  create, interruption, recovery, update, and stable-reopen scenario.

The no-argument retirement owner, cross-target duplicate evidence, GitHub
shards, and qualification remain cache-independent.

## Consequences

- The measured warm database development owner falls from 190.863 seconds to
  125.757 seconds, a further 34.1 percent reduction or 1.52 times speedup.
- Relative to the 1,111.135-second clean fourteen-case owner, the same focused
  feedback is 88.7 percent shorter and 8.84 times faster.
- Complete `Verify-Changed.ps1` wall time falls from 197.4 seconds to 141.120
  seconds, a 28.5 percent reduction or 1.40 times speedup including planning.
- Changing one fragment byte or only the target selects a distinct key.
- Appending one byte to an isolated cached application is rejected before
  application execution; the corrupt entry is not repaired.
- The complete inventory is validated and hashed in one Node.js process. Node
  remains a development-only framing dependency and defines no product
  semantics.
- Fresh linking and repeated process/recovery execution now dominate the warm
  path. Link-product reuse or scenario batching requires separate measurement
  and a new boundary.

## Reconsideration triggers

Reconsider this decision if any producer or target input can change without a
key change, a cache hit changes application bytes or executable mode, Linux
behavior differs, the development owner skips a behavioral scenario, or the
native packaging pipeline gains a faster independently admitted incremental
publication contract.
