# Decision 0377: Windvale-owned native service table

- Status: Accepted current-host normal-path service-table construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0376](0376-Windvale-Owned-Native-File-Input-Table.md), [Decision 0373](0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native service-table construction](../../Specifications/Windvale-Native-Service-Table-Construction.md)
- Advanced by: [Decision 0378](0378-Windvale-Owned-Native-Execution-Context.md)

## Context

Decisions 0369 through 0371 moved all platform service leaves to Windvale or
direct exact artifacts, and Decisions 0372/0373 moved their image placement.
The normal executor still wrote service-table version, size, and twelve
pointer slots in C# through a separate service-to-byte-offset switch.

Executable publication and address calculation remain host duties. The closed
service order, required/absent presence, and exact table bytes do not: Windvale
can validate one bounded mask and copy opaque targets without interpreting
native pointers.

## Decision

- Define exact 112-byte `WVTQ 1` input and 32-byte `WVTR 1` response envelope.
- Let portable Windvale validate a nonempty low-twelve-bit required mask and
  exact required-nonzero/absent-zero target relation before constructing the
  unchanged 104-byte service-table version 5.
- Derive the mask from the fragment's verified canonical service list. Project
  each opaque target from the published executable image base and accepted
  service placement without retaining a C# byte-offset mapping.
- Retain host ownership of executable publication, placement-to-address
  calculation, native table allocation/copy, invocation, and teardown.
- Consume one exact digest-bound service-free WVNF in the normal runtime and
  keep its source/WVB only for reproduction, qualification, and recovery.
- Keep the constructor and artifact/response verifier outside the already large
  executor source.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Service-table core WVB | 3,065 | `ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26` |
| Retained service-table bridge WVB | 3,079 | `04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b` |
| Retained service-table bridge WVNF | 34,830 | `e1b838652150999d13b84cd6f1c527b4e82923190530f707ef8d163d39a1f58e` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities,
confirms that the runtime embeds no constructor WVB, compares single, sparse,
and all-twelve service subsets plus eight malformed requests through the
reference interpreter and verified native fragment, checks exact expected
table bytes, exercises managed projection without byte offsets, reproduces the
bridge through the ordinary native source front door, and executes a real
UTF-8 service call through the normal executor. The single selected test passes
1/1 in 1.161 seconds through the Release test application.

The broader hosted-service, file-I/O, exact-compiler, Development, Standard,
Qualification, and Linux gates were reviewed but not run under the goal's
deferred-broad-verification rule.

The normal executor no longer writes service-table fields or owns the
twelve-way service-to-byte-offset switch. It still verifies the fragment's
canonical required-service list, constructs and publishes exact service leaves,
computes their final opaque targets, independently verifies/copies the Windvale
response, invokes the application, and tears the table down. The execution
context is now the next static byte-construction seam; W^X authority, arenas,
invocation, result admission, and teardown remain later host slices.

## Reconsideration triggers

Replace this request boundary when native publication returns a directly
usable service table or a native host adapter owns the entire publication and
invocation lifetime. Keep dynamic targets out of WVB, WVO, caches, and retained
artifacts. Version the contract if the table or closed service order changes.
