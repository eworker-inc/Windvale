# Decision 0544: First native durable-storage provider

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0541](0541-First-Abi-23-Storage-Describe-Execution.md), [Decision 0536](0536-Nested-Records-And-Database-Storage-Recovery.md)
- Defines: [Focused native `storage.random_access_v1` provider](../../Specifications/Random-Access-Storage-Capability.md#focused-native-provider)

## Context

ABI 23 could execute a synthetic describe response, while the durable database
formats and recovery planner remained pure algorithms. No forward native owner
opened a mutable object, executed positioned I/O, enforced a writer fence,
distinguished rejected from indeterminate mutations, or proved restart tail
repair. The composed database fixture also exposed one compiler closure gap:
record-storage analysis admitted capability kinds only through six even though
the lowerer already emitted the kind-seven storage call.

## Decision

- Admit capability kind seven in record-storage analysis, consume its exact
  five parameters, and retain its returned bytes descriptor as a scalar value.
- Add one common x64 host that derives context 9, constructs an exact one-entry
  `WVPT 1` table, revalidates every storage request, and serializes strict
  `WVSA 1` results from page-probed execution-owned scratch.
- Keep the native path and handle outside Windvale source. The focused shell
  opens only `Windvale-Database-Storage.bin` in its working directory.
- On Windows, reuse the hosted container's admitted file-function tables and
  resolve only `SetFilePointerEx` and `SetEndOfFile` from the same checked PE
  image as `CreateFileW`. Deny competing writers/deleters while permitting
  readers.
- On Linux, own only the bounded `openat`, `flock`, `lseek`, `pread64`,
  `pwrite64`, `ftruncate`, `fsync`, and `close` syscall leaves.
- Exercise describe, read, write, resize, both flush scopes, stale generation,
  outside-storage, and malformed request behavior. Publish one `WVPG 1` root
  and one `WVDS 1` superblock, inject a 17-byte tail after process exit, recover
  it on the next process, and require a byte-identical third reopen.

The numeric generation remains process-lifetime fencing evidence. This focused
shell is not a restartable multi-client service or a configurable product
launcher.

## Focused evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Host-storage WVB | 122,569 | `811cad2c3931f59eb59eb2fadac4232b5a89ffc0315070ae00e19af93981dedf` |
| Host-storage WVO | 2,417,628 | `8949088ecc68fc84df173869e611107d1760ee7efab6a4ec5050489d9475943f` |
| Common provider WVO | 2,837 | `72f52ab76e5490ed0cf2f42dfea812670a7a79ac8f387017bbf93066a18c8c62` |
| Windows leaf WVO | 2,534 | `ea5cb57ddbbc47990a56d0beab1ea074eb5e55f2e1e89e2b8cc276a4cf01dad6` |
| Linux leaf WVO | 1,073 | `3b75e90fad5d42a83fe9fa78c8ea2774db579356c7d11596b581030241b23117` |
| Windows hosted application | 2,441,728 | `1aad26ac76e2853364e26cc8200572a8bd91ea70d893411817c0e95aaa8f22b4` |
| Linux hosted application | 2,441,216 | `4beca95dd71dddd2ffcccf801d09c6fbc1b6e4e35673cd628455f0d10e06d915` |
| Recovered database | 4,608 | `4b170b537a0c2e7e51ccd6582e9e751dfddceb4e4d9f5f1181279c6847c07c95` |

The cached development compiler built the fixture in 3.270 seconds and lowered
it in 3.501 seconds. Windows created the database with result zero, recovered
4,625 bytes back to 4,608 bytes with result zero, and reopened it again with
result zero. Initial, recovered, and third-open SHA-256 identities matched. The
final coherent nine-case Windows owner passed in 658.777 seconds after the
compiler candidates were refreshed. Independent Linux execution remains
pending, so no cross-host qualification is claimed here.

## Consequences

- Durable database algorithms now cross the real native capability boundary
  without .NET or C# in build, execution, or recovery semantics.
- A platform I/O failure after mutation dispatch cannot be mislabeled as a safe
  rejection; it becomes `Indeterminate` and forces recovery.
- The application fragment remains RX. Its maximum response arena belongs to a
  page-probed stack frame rather than writable static data hidden in the image.
- A database server may now build its page/cache/transaction executor over one
  real object, but networking, supervision, concurrent clients, tree payloads,
  reclamation, and bounded human SQL remain future milestones.

## Reconsideration triggers

Replace the fixed shell when the ordinary container can bind an exact
rights-limited storage object from admitted metadata. Revisit the generation
scheme before service restart or concurrent clients, and replace advisory Linux
fencing if deployment needs protection from non-cooperating native writers.
