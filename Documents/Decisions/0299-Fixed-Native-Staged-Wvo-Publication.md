# Decision 0299: Fixed native staged-WVO publication

- Date: 2026-08-06
- Status: Implemented candidate; Linux execution, complete-tool integration, and grouped dual-host qualification pending
- Advances: [Decision 0295](0295-Exact-Staged-Wvo-Snapshot-Admission.md), [Decision 0214](0214-Exact-Native-Wvb-Publication-Step.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0295 admits the exact input, manifest, and chunk snapshots in one
Windvale execution, but deliberately stops before destination mutation. The
remaining boundary needs fixed Windows and Linux code that can distrust the
native table, bind host file identities, consume only the admitted snapshots,
and perform the existing sibling-and-replace transaction without joining a
large WVO in one Windvale value.

The existing read-only verifier runtime has only one snapshot slot. Reusing it
would make the multi-resource contract fail during portable admission, so this
publisher requires the compiler-capacity 64-snapshot storage plan.

## Decision

- Expose focused private wrappers around the already-qualified portable
  publication transaction. Platform code calls `begin` and ordered `apply`
  transitions rather than duplicating transaction policy.
- Add one shared x86-64 WVA validator for the native `WVFI` table. It accepts
  only the exact platform, three through 64 initialized records, fixed strides,
  reserved-zero fields, canonical ordinal pointers, and at most 32 MiB of total
  chunk data.
- Add fixed Windows and Linux publication adapters. Each reruns portable
  admission, validates the native snapshot table, opens every resource by its
  retained name, captures native identity, rereads it completely, and compares
  those bytes with the immutable execution snapshot before mutation.
- Reject a destination whose Windows volume/file identity or Linux
  device/inode identity equals any input, manifest, or chunk resource.
- Create one exclusive sibling in the destination directory. Write snapshot
  ordinals two through the admitted count in order, handle partial progress,
  durably flush, seek and reread the complete sibling against the same snapshots,
  require exact EOF, and replace the destination atomically. Linux additionally
  synchronizes the directory; Windows flushes the renamed file handle.
- Cleanup owns every pre-replacement sibling failure. A failure after replacement
  remains distinct and is not silently replayed as another mutation.
- Package digest-bound Windows and Linux application candidates under
  `windows-x64-wvo-staging-publisher-v1` and
  `linux-x64-wvo-staging-publisher-v1`. The wrapper reuses the existing
  compiler-capacity runtime storage layout but its custom startup binds only
  the eight services required by the exact Windvale module.
- Keep the shared validator, platform adapters, package builders, and behavioral
  test in focused files. The large platform files remain cohesive owners of one
  host transaction and are not split into numbered fragments merely to reduce
  line count.

## Evidence and consequences

- The exact publisher WVB is 414,230 bytes at SHA-256
  `07b9e0eff09927208980a0acdd7d88acc6cb3f40981d0c0a582951e7d30517f1`.
- The Windows candidate is 6,010,880 bytes at SHA-256
  `7a319247b6f6aabbf185cb46b491650303840f1a30849f576af7cf2258b65b40`.
  Current-host execution publishes the complete three-chunk fixture, rejects a
  changed chunk without replacing the sentinel destination, rejects a hard-link
  destination alias, leaves no `.wvo-*` sibling, and loads no CLR component.
- The structurally built Linux candidate is 6,008,537 bytes at SHA-256
  `b297da74e2fc6608023cd166f9abfa6f8543aef77dd1a7b01922079d38a61bd0`.
  Its execution remains for the grouped Linux gate.
- The reviewed focused compiler selection passes 1/1 in 6.394 test seconds
  after a 13.18-second zero-warning Release build; the complete command takes
  24.2 seconds. No broader local verification level was run.
- C# changes are limited to Stage 0/recovery package construction, target
  routing, and the test harness. Publication admission, snapshot validation,
  native identity checks, mutation, and cleanup execute in Windvale/WVA. No
  WebAssembly implementation changed.

This does not yet prove the Linux executable, connect complete compiler
self-staging to the publisher as an ordinary workflow, promote either package,
replace the managed candidate constructor, run extended concurrency/fault
injection, or close the grouped retirement gate. Development, Standard,
Qualification, and the final dual-host gate remain deferred.

## Reconsideration triggers

Revisit the fixed adapter if snapshot ownership stops lasting for the complete
execution, a host cannot provide stable file identity, Windows gains a stronger
documented directory-durability primitive, or the staged object exceeds the
current 62-chunk/32-MiB contract. Any later asynchronous or concurrent publisher
must retain the same immutable evidence, explicit indeterminate-mutation state,
and no-replay rule.
