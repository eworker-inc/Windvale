# Decision 0513: Native fixed-service and publication build/inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

The broad Seed verification scripts still used the feature-frozen Stage 0 CLI
to build twelve exact products: the text-concatenation, text-quote, enum-name,
enum-metadata, native-publication, and service-bundle-materialization cores and
bridges. They then used the managed inspector on every product except the
service-bundle core. All source closures are already accepted by the native
Project 1 builder and all public ownership surfaces are accepted by the native
WVB inspector.

Four bridge manifests also remained at repository root even though they owned
only one Runtime or Compiler component. The service-bundle pair is different:
it aggregates Compiler publication, Foundation byte construction, and Runtime
materialization sources and therefore has a genuine common-ancestor owner.

## Decision

Extend the paired `Verify-Seed-Native-Front-Door` helpers with these exact
Project 1 products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| text-concatenation core | 10,253 | `6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73` |
| text-concatenation bridge | 10,232 | `87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08` |
| text-quote core | 1,471 | `b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453` |
| text-quote bridge | 1,435 | `306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631` |
| enum-name core | 625 | `b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948` |
| enum-name bridge | 592 | `46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c` |
| enum-metadata core | 15,414 | `8f22e1ba56985fc5a330fcb73cda84456ecc3ef51f9ddffd6bc2edd740f73659` |
| enum-metadata bridge | 15,292 | `052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072` |
| native-publication core | 7,190 | `3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f` |
| native-publication bridge | 6,758 | `111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c` |
| service-bundle-materialization core | 17,185 | `97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008` |
| service-bundle-materialization bridge | 17,150 | `327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902` |

The helpers bind each exact native build report. Native inspection binds the
portable profile, exported constructors or byte-result `Main`, exact export
counts, fixed leaf data, and the capability-free publication bridges. The
broad scripts consume these native-built WVBs and retain byte-for-byte checks
against every embedded bridge and executable leaf. They no longer repeat the
twelve managed builds or eleven managed inspections.

Ten manifests now live beside their Runtime or Compiler source. Four obsolete
root bridge manifests are removed. The service-bundle core and bridge
manifests remain at root because they are real cross-component aggregates. A
future workspace/package-reference contract may organize such aggregates more
deeply without weakening Project 1 source containment.

## Evidence

- All twelve native builds reproduce the established WVB identities.
- Eleven native inspections admit the exact intended ownership surfaces.
- `Verify-Seed-Native-Front-Door.ps1` passes its 76-case contract over 43
  artifacts in 39.3 seconds.
- The five directly affected frozen behavioral owners pass 5/5 in 12.158 test
  seconds; the Seed test project builds with zero warnings and errors.
- Both broad host scripts retain independent embedded-WVB and exact leaf or
  fragment comparisons after their managed build/inspection calls are removed.

This removes twenty-three additional managed invocations from each broad host
script, eighty-two cumulatively across Decisions 0505, 0506, 0508, 0509,
0510, 0511, 0512, and 0513. It does not remove a direct managed entry file:
the inventory remains three normal direct files plus nine recovery files, and
T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 31 to 43 exact artifacts and from 53 to 76
owned cases. Fixed service-leaf, enum-metadata, publication-plan, and
service-bundle source construction and inspection are now native in both
permanent-host scripts. Their frozen behavioral oracle, capability-bearing
execution, and retained runtime products remain separately owned.

Current evidence is Windows-host native build, inspection, and focused
differential evidence. It is not independent Linux execution, replacement of
the broad managed test harness, clean or previous-seed bootstrap, grouped
qualification, promotion, or recovery deletion.

## Reconsideration triggers

Transfer the next coherent managed call cluster rather than moving isolated
calls. Introduce workspace or package-reference semantics only with exact
source identity, containment, and changed-file ownership.
