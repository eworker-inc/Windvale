# Decision 0707: Echo independent-metadata package migration

- Date: 2026-08-16
- Status: Implemented with local Windows and Debian WSL2 evidence
- Advances: [Decision 0693](0693-Echo-Package-Approval-And-Launch-Record-3.md)
- Builds on: [Decision 0706](0706-Refresh-Metadata-Aware-Wvb-Publisher-Construction.md)
- Contracts: [Package 1](../../Specifications/Windvale-Package.md), [Bundle 1](../../Specifications/Windvale-Package-Bundle.md), and [capability approval and launch records](../../Specifications/Windvale-Capability-Approval-And-Launch.md)

## Context

The source compiler, lowerer, verifier, inspector, and publisher candidates could
already admit independent module metadata, but current packages still used the
legacy `module ... profile ...;` and source-level `capability` declarations.
That left the migration at fixture and tool reconstruction rather than proving a
real package from source through command execution.

Echo is the smallest current package with a meaningful hosted boundary: it has
one source part, supports Windows and Linux, and requires exactly line output plus
immutable argument value and count snapshots. Its native host applications are
derived from the semantic WVB view, so metadata framing can change while their
machine behavior remains byte-identical.

## Decision

- Replace Echo's legacy header with independent `platform`, `authority`, and
  versioned required-capability declarations. Preserve its derived `hosted`
  profile and executable capability directory.
- Pin the current reconstructed compiler and metadata-aware publisher candidates
  in the specialized Echo package builder. Do not promote either candidate into
  the ordinary native front door in this decision.
- Make the Bundle 1 writer and independent verifier normalize present WVB 1.11
  metadata before applying their existing executable semantic verifier. Continue
  to accept absent-form WVB through the same bounded normalizer.
- Rebind Echo Lock 1, provenance, Bundle 1, Approval 1, and both Launch Record 3
  records to the replacement WVB identity. Preserve the Windows and Linux native
  host applications byte for byte.
- Extend the metadata-aware reconstructed WVB Inspector owner to build and inspect
  the exact Echo package on both hosts.

## Evidence

The 845-byte source has SHA-256
`f843e69b9549a890aa808331f6ef503941c0a1d5240ecd5859e46f6f8ae044c7`.
Both hosts produce the exact 927-byte WVB at SHA-256
`b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713`.
Its metadata names `linux` and `windows`, `application` authority, and required
version 1 of `console.write_line`, `process.argument`, and
`process.argument_count`.

The package records are:

| Record | Bytes | SHA-256 |
| --- | ---: | --- |
| Lock 1 | 940 | `948a7ee6e1cddf54b5cec274862b5a17882b271f827f61a8cd0f6649865e65f6` |
| Provenance | 454 | `5c41b54660dd071bfbca00a7ed18f446ee947bf857cb1ab05be359345888387e` |
| Bundle 1 | 17,009 | `e8fdafd1b2577079e15e085c8640c7d175d0cfe0e60b43580d73e1e88148d385` |
| Approval 1 | 793 | `f65c1b3638c1e222b69c617d9c9866d069525c5bf01954496a0fa9f2dd4d636e` |
| Windows Launch Record 3 | 918 | `cf33ad3586f0cda9e29f8e206611951eea556f8fd81540ab504264809a491c94` |
| Linux Launch Record 3 | 914 | `a31e3a61ac79b4a324f64079f0bd5d967add0ff7c37673c6f5296419a81692a3` |

The bundle writer is 283,725 bytes at SHA-256
`6cf19d10d49cd27496ea7a3aa4ea11dec4baa792001697bf6e2835c0ed2c3a14`;
the independent verifier is 303,018 bytes at SHA-256
`1fa416cd151e10422d0e0034671a1f4c4f6085c1b66aed1d8876b4f04fc4f23c`.
Their combined self-test is 331,563 bytes at SHA-256
`c0ec85c19fc8647d1957b127637535567e45c7e3caf637393484bca83106ec64`.

Windows and Debian WSL2 each pass the nine-case Echo execution owner, the
ten-case installed-command owner, and the reconstructed inspector's absent-form,
metadata-fixture, and exact Echo-package inspections. The Windows host remains
22,016 bytes at SHA-256
`024cfac66fa760b705a48e72942103a79e24342d3e59886e9ccd127dfd3cdbcb`;
the Linux host remains 24,576 bytes at SHA-256
`0e5a91887381adb23a84d745ce06902be99e53d70e58a598465939881638b576`.

The broader Package Bundle owner keeps the WVB Inspector distribution boundary
explicitly frozen at its retained 76,527-byte pre-metadata WVB rather than
misrepresenting the migrated inspector source as a match for the historical lock.
Package resource admission explicitly expects the current inspector project and
part to differ from that lock.
This decision does not migrate that package's lock or claim a complete local
Qualification gate.

## Consequences

- Independent metadata now has one real, fully identity-bound package and command
  consumer on both maintained hosts.
- Size and digest changes are evidence of the new header and verifier path; the
  product behavior and target host bytes are unchanged.
- Bundle admission accepts both absent-form and metadata-present WVB without
  weakening semantic verification or treating metadata as execution authority.
- Remaining packages and source libraries still require owner-sized migration.
  Ordinary front-door promotion remains separately gated.

## Reconsideration triggers

Reconsider this decision if WVB metadata framing changes, if Echo gains another
platform or capability, if package admission stops preserving the normalized
semantic view, or when the compiler/publisher candidates are promoted into the
ordinary native front door.
