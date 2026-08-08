# Decision 0365: Native publication-planner execution

- Status: Accepted current-host normal-path interpreter removal; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0082](0082-Windvale-Owned-Native-Publication-Layout.md), [Decision 0083](0083-Windvale-Owned-Native-Publication-Lifetime.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Advanced by: [Decision 0372](0372-Windvale-Owned-Bounded-Service-Bundle-Materialization.md)
- Contracts: [Windvale native publication plan](../../Specifications/Windvale-Native-Publication-Plan.md) and [Windvale native publication lifetime](../../Specifications/Windvale-Native-Publication-Lifetime.md)

## Context

Windvale already owned executable-image layout and the closed publication
lifetime graph, but both normal wrappers were hosted `Main() -> bytes` modules.
The managed reference interpreter supplied each request through a synthetic
`file.read_bytes` capability on every application publication.

Decision 0360 admits capability-free `Main(bytes) -> bytes` native entries, so
both planners can receive their bounded requests directly. A direct call to the
ordinary native executor would nevertheless recurse: that executor asks the
same layout and lifetime planners how to publish any native fragment.

## Decision

- Replace both hosted wrappers with capability-free portable
  `Main(bytes) -> bytes` bridges. Preserve the existing `WVPQ`/`WVPL` and
  `WVLQ`/`WVLT` formats, limits, statuses, response validation, and final
  application-image policy.
- Load, digest-check, verify, lower, and cache each retained planner WVB as one
  exact bytes-to-bytes native fragment. Normal planner evaluation uses native
  execution; it no longer creates a managed reference runtime, capability host,
  resource context, or synthetic file reader.
- Add one internal service-free planner bootstrap. It independently verifies
  the planner fragment, admits no runtime services, aligns that fragment into
  one bounded image, and supplies the already accepted nine-transition
  publication lifetime needed to start the planner itself.
- Keep the bootstrap outside the ordinary application path. After each planner
  starts, the host independently validates the Windvale response; final
  application layout and lifetime continue to come from Windvale.
- Keep the reference interpreter as differential and recovery evidence. Do not
  treat the small bootstrap as a second general planner, linker, service-bundle
  assembler, or native runtime.

## Exact retained planners

| Planner bridge | Bytes | SHA-256 |
| --- | ---: | --- |
| Executable-image layout | 6,758 | `111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c` |
| Publication lifetime | 4,442 | `f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554` |

The unchanged portable cores remain 7,190 bytes with SHA-256
`3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f`
and 4,955 bytes with SHA-256
`a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3`.

## Evidence and consequences

The focused Release test project builds with zero warnings and errors in 9.34
seconds. The reviewed layout case passes 1/1 in 1.240 seconds; the lifetime case
passes 1/1 in 0.916 seconds. Both compile and reproduce the retained portable
WVB, prove the native entry shape, compare valid and malformed requests against
the independent reference interpreter, and require the narrow bootstrap to
agree with the accepted Windvale plan. Both qualification scripts pass syntax
checks with the new portable identities and entry signatures.

Normal publication planning no longer runs Windvale under the managed
interpreter or binds `file.read_bytes`. Managed code still decodes and lowers
the two retained WVBs, owns the narrow planner bootstrap and platform W^X
adapter, assembles service bundles, constructs segmented enum metadata, owns
contexts and arenas, and invokes applications. Linux execution and the final
grouped retirement gate remain deferred.

Decision 0372 later generalizes the bootstrap name to its actual service-free
role and uses it for one additional internally selected, digest-bound WVNF:
bounded service-bundle materialization. It does not admit ambient WVBs or bind
services. The original two planner WVBs are also replaced in the normal path
by direct verified-fragment artifacts under Decisions 0367 and 0368.

## Reconsideration triggers

Replace the bootstrap when a qualified native loader can publish every exact
internally selected service-free fragment without managed code. It must remain
service-free, digest-bound, and bounded until then; widening it into ambient
loading or general publication policy would recreate the duplicate
implementation this decision removes.
