# Language 1.0 localization workload 5: shipment, cache, and cross-host evidence

## Status

Complete owner-accepted first-author paper bundle for localization shipment,
installer selection, immutable-store deduplication, offline resolution, update/
rollback, compiler-service cache generations, cross-host comparison, and future
performance measurement. It reuses Windvale's existing package architecture and
does not claim a new installer/compiler implementation or measured qualification.

## Result first

Localized source support does not need to duplicate the compiler, libraries, or
runtime:

- a runtime-only installation carries no source-profile, diagnostic, or
  documentation localization objects;
- the minimal developer installation includes canonical `en@1` source support;
- `zh-Hans@1` source support is one optional, exact, independently versioned
  selection;
- diagnostics and documentation are separately optional because they do not
  affect source semantics;
- shared Unicode/token inputs and identical catalogs/objects occupy the immutable
  store once by SHA-256, regardless of how many packages/generations reference
  them;
- online and offline acquisition admit the same exact objects and logical package
  selection without compiler network access;
- updates construct a new immutable installation generation and never rewrite an
  active pack or source file;
- a compiler service hashes/parses one distinct pack object at most once per
  service generation, shares immutable validated state across requests, and keeps
  raw spans/diagnostics request-owned; and
- Windows and Linux compare portable localization bytes and semantic results,
  while host-native installer generations may legitimately contain different
  target objects.

## Bundle contents

| Item | Purpose |
| --- | --- |
| [Fixture inventory](Fixture-Inventory.md) | Exact Release 1 reference-object sizes, hashes, sharing, and honest size boundary. |
| [Shipment and installer](Shipment-And-Installer.md) | Logical packages, selections, offline resolution, install/remove, update, rollback, and GC. |
| [Cache generations](Compiler-Service-Cache-Generations.md) | Hash-once validation, single-flight publication, request isolation, retirement, and multi-agent implications. |
| [Cross-host and performance](Cross-Host-And-Performance.md) | Exact equality matrix, benchmark workloads, reports, and threshold timing. |
| [Accepted cases](Accepted-Cases.md) | Required installation, cache, update, and qualification successes. |
| [Rejected cases](Rejected-Cases.md) | Missing, stale, corrupt, race, fallback, cache, report, and status failures. |
| [Review findings](Review-Findings.md) | Owner-accepted product/performance decisions and implementation gates. |

## Existing architecture reused

Language packs are ordinary immutable package resources admitted through the
[package bundle and installation architecture](../../../Architecture/Windvale-Package-Bundle-And-Installation.md).
Content objects, bundles, installation generations, activation, rollback,
repair, and garbage collection keep their existing ownership. This workload adds
localization-specific package selection and evidence; it does not define a second
content store, updater, signature format, or path convention.

## Workload disposition

The paper shipment/cache/cross-host contract is accepted for replacement source
freeze. Actual compiler, package, installer, Windows/Linux, cold/warm time, and
memory results are implementation qualification gates. This separation avoids
the circular requirement to benchmark a Language 1.0 compiler before the design
freeze authorizes its implementation.
