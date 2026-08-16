# Decision 0726: Source-own the generation-three filesystem machine

- Status: Accepted; constructors boot-linked by [Decision 0728](0728-Boot-Link-The-Filesystem-Machine.md)
- Date: 2026-08-16
- Contract: [x86-64 filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md)

## Context

Decision 0725 selected sequential reuse of released process/object slot 2 and
closed endpoint slot 0 for the first filesystem provider. The boot fixture did
not yet have a source-owned constructor for that larger service image and its
81 charged user pages. Treating the record or page-table bytes as an informal
future patch would weaken W^X, generation, and deterministic-image evidence.

The provider needs 48 RX image pages, 17 RW/NX context/transfer pages, and 16
disjoint RW/NX native-stack pages. Its private paging structures are four
additional physical pages and must not be confused with the resource domain's
81-page user charge.

## Decision

Own record construction, paging construction, and service image/context setup
as three focused Windvale modules over the shared x86-64 emission model. Fix the
generation-three record, endpoint generation, exact image digest and length,
85/81/48/33 page geometry, disjoint receive/stack ranges, W^X mappings, and
single typed relocation in independently executable tests.

Keep this work in a dedicated three-case verification owner. Do not add these
paths to the saturated general process-machine fixture merely to reuse its
existing owner.

## Consequences

The next privileged boot slice can compose exact verified constructor bytes
instead of hand-maintained byte literals. Decision 0728 links those bytes but
makes no live-provider claim: the boot object has not yet allocated the extent,
invoked the constructors, advanced the endpoint, published the record, or
entered the service. The empty configuration digest remains an explicit
placeholder until the FAT32 image/configuration identity is admitted.

The constructor allocates 85 physical pages while accounting 81 user pages.
Review and tests must preserve this distinction or reject the construction.

## Reconsideration triggers

Revise the contract when boot integration supplies a real media/configuration
digest, when the service image identity changes, when endpoint or record layout
versions change, or when concurrent providers require a larger arena and object
table. Any such revision must regenerate exact cross-host evidence.
