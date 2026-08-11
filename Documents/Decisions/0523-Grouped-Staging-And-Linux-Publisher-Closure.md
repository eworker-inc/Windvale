# Decision 0523: Grouped staging and Linux publisher closure

## Status

Implemented and exercised through focused plus complete Standard Windows and
independent Linux evidence. Grouped repository Qualification, candidate
promotion, and final recovery retirement remain pending.

## Context

The native publisher chain had three related correctness gaps that became
visible only after composing the current large verifier-scale products.

First, the staged-content reader assumed that one manifest chunk corresponded
to one nonempty lowering publication step. That assumption held for the small
fixtures but not for a large function whose code is intentionally split into
several bounded publication values. The manifest correctly describes one
contiguous code chunk; the content reader rejected it before comparing the
complete grouped bytes.

Second, the shared Linux publication adapter created every admitted snapshot
with mode `0600`. That mode is correct for WVB and WVO data, but a canonical
ELF application published through the same durable transaction must be
executable. The exact bytes were correct while the resulting application was
not directly launchable.

Third, the Linux immutable-snapshot startup retained publication-policy cells
inside the region later used for the second 144-byte `stat` result. An existing
destination could therefore overwrite the selected snapshot sequence and final
mode before replacement. Absent-destination tests did not exercise that
overlap.

These fixes change the Linux publication-adapter object and every final Linux
publisher application that embeds it. The publisher construction requests,
target evidence, object-instantiation evidence, admission products, promoter,
and textual inventories consequently require one coherent identity refresh.

## Decision

### Admit grouped code publication without joining the whole object

`Compilerˉnativeˉx64ˉstagingˉcontentˉnext` now consumes consecutive nonempty
code publication steps until their checked total equals the current manifest
entry length. Every step must begin at the exact next position, remain within
the entry, retain code kind while grouping continues, and make nonzero
progress. The reader compares each bounded value directly with its matching
slice of the admitted chunk and still rejects length, position, kind, content,
or final-coverage disagreement.

This preserves the bounded segmented design: it neither constructs one whole
WVO value nor weakens the manifest's exact-position authority.

### Select Linux publication mode from the admitted snapshot

The Linux publication adapter retains mode `0600` for ordinary WVB and WVO
data. When the immutable admitted snapshot is at least four bytes and begins
with the canonical ELF magic, the exclusively created sibling receives mode
`0755` before the existing flush, reread, and atomic replacement sequence.
Collisions recompute the selected mode because the Linux syscall convention
may reuse the argument register.

The adapter object advances to 5,559 bytes with SHA-256
`1a97195d846626276f38dbb44be68a696dd057f701918f66eb46f6e9d7b5999e`.
Its code image is 3,415 bytes; the exported fragment is 3,355 bytes with 28
symbols and 49 relocations.

### Keep immutable-snapshot policy outside both stat records

The Linux immutable-snapshot startup moves its saved entry, resource ordinals,
snapshot count, and final mode beyond both complete `stat` result buffers.
Existing-destination metadata can no longer corrupt publication policy. The
refreshed object remains 3,503 bytes with SHA-256
`595398fc2fd80e4b29bc88e9de13731374c6783a6dc2ac0f86eecf7734eb41fc`.

### Refresh the complete publisher construction closure

Construction candidate 20 records Decision 0523 and pins a 5,064-byte
`SHA256SUMS` with SHA-256
`ac41be9f59a7db47f721e0c0485cfe7e10cfc888e902f67e91a3c1c6330b68eb`.
The primary final publisher identities are:

| Product | Host | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Hosted-verifier application publisher | Windows | 256,000 | `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` |
| Hosted-verifier application publisher | Linux | 254,965 | `510f5ce5d2a494eacf0adc7a613581bc2371c4ad0f5f985f501381edc1632fac` |
| WVB publisher | Windows | 1,340,928 | `71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3` |
| WVB publisher | Linux | 1,340,405 | `7024fc5f96181f819e01bc41bc5c34d9eaed4301ea459c0c2bc43b7f52b21095` |
| WVO publisher | Windows | 430,080 | `76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910` |
| WVO publisher | Linux | 426,997 | `2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2` |
| Console-application publisher | Windows | 1,158,656 | `0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e` |
| Console-application publisher | Linux | 1,156,085 | `e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925` |
| Publisher promoter | Windows | 681,472 | `86c72f5485bd6eeba1bdb65841102d7f388a8714b8e07ca3d519250de2886d8b` |
| Publisher promoter | Linux | 680,949 | `700f3df624611abad03cbd70811bad2ab015136ecdacc6dff9cdd97f5fc81395` |
| Publisher admitter | Windows | 570,368 | `72d1164fe2f47e1bec00437bf63b317d39f1ed011cea7cf01a1343ce01547765` |
| Publisher admitter | Linux | 569,344 | `18777615d60e1279cb855b05ba03933bb65c9a622036dad2e954e3df683216e2` |

The promoter's Windvale inputs are the 41,268-byte WVB at
`086bd4d93d93d51b0f9140a0adf9f54a7f205dc902d9cb5d732dc7a887e10edc`
and the 660,123-byte WVO at
`ee5274c86d680640d3ab75754faf63585a639a44fc9626ea5b9f9bcce9779e8e`.
The publisher-admission inputs are the 30,778-byte WVB at
`d43013f4a3b70f90ae83e5cd1b643421b2bf5ec4b4dbdec1cb844849d09024db`
and the 555,690-byte WVO at
`e348c41dcd96dbacedcc1820d42013e3c19795d89f7183ac7bc64311612dd927`.

The retained C# writer may reproduce these bytes for recovery evidence, but it
does not regain ownership of ordinary publication or installation.

The refreshed Linux hosted-verifier application publisher, publisher
admitter, publisher promoter, and WVB publisher are tracked with executable
mode. Their normal launchers must therefore work from a clean Git checkout;
local archive or overlay preparation is not permitted to supply missing
product permissions implicitly.

## Evidence

- The focused hosted-verifier publisher file pipeline passes 7/7 on Windows
  and reproduces both final generic publisher applications exactly.
- Focused Windows object-instantiation and extended deterministic WebAssembly
  closure tests pass over the refreshed generated identities.
- Independent Linux execution passes the real source-to-AOT chain, immutable
  hosted-container segment sets, publisher rejection and preservation cases,
  digest-bound WVO launchers, read-only WVO rejection families, and the 3/3
  WVO-inspector reconstruction owner.
- Linux publication writes WVB and WVO data with mode `0600` and writes the
  exact admitted ELF application with mode `0755`; existing-destination
  publication preserves the selected immutable snapshot and final mode.
- A clean-checkout focused Linux rejection run directly launches the four
  pinned publisher roles and preserves both invalid candidates and existing
  destinations without relying on overlay-restored executable bits.
- The full paired native helper already passes 105 artifacts and 185 cases on
  both hosts after Decision 0522. Repository Standard and GitHub grouped
  Qualification are separate gates and are not implied by those focused runs.
- The complete 224-test Standard Seed suite emits fresh zero-failure
  conformance reports on both Windows and Linux. The paired 39-test OS
  in-process suite passes 39/39 on each host.
- The report comparator confirms exact cross-host contract equality. The
  15,890-byte Windows report has SHA-256
  `4dba5c8327b0eef3247cdabf48470965c53d2090e7be57d0bf30ac7dfa19b44f`;
  the 15,797-byte Linux report has SHA-256
  `60a9af211e411c114e4404fbc25cbba51b364c16c35fbc232ab35507b6d6d7dc`.
  Host metadata is intentionally not part of the normalized equality check.

## Consequences

Verifier-scale staged content can cross the bounded multi-value code boundary
without false rejection. Linux publisher output is directly executable while
data publication remains private, and an existing destination cannot overwrite
saved publication policy through the stat buffers.

The native publisher construction closure is internally consistent again, but
the candidates remain candidates. T2 remains `managed-normal`; grouped
Qualification, promotion, clean or previous-seed renewal, remaining
capability-bearing managed executions, and the final Stage 0 recovery/archive
release stay open.

## Reconsideration triggers

Version the staging manifest if one entry may span kinds other than consecutive
code values. Replace ELF-magic mode selection only with a more explicit
admitted artifact-kind contract that preserves data privacy and executable
publication. Keep all publication-policy cells disjoint from host-structure
buffers whenever the Linux startup layout changes.
