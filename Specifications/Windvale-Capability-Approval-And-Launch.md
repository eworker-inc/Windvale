# Windvale capability approval and launch records version 1

## Status

Approval 1 and Launch Record 1 are implemented for the exact WVDB Query package
selected by Milestone 2 under
[Decision 0564](../Documents/Decisions/0564-First-Installed-Capability-Approval-And-Launch-Records.md).
Milestone 4 adds an exact portable approval for WVB Inspector while leaving its
target launch records open until real host artifacts are measured. These records
make the capability proofs inspectable as installation and launch policy without
creating a general launcher, approval UI, or capability database. The expanded
ten-case owner covers two applications, four records, and ten approved
capabilities.

## Approval record

`Windvale-Wvdb-Query.wvapproval` is canonical strict UTF-8 with LF. It selects
the exact Package 1, Lock 1, Bundle 1, provenance, and WVB identities, followed
by the complete five-entry capability closure in canonical WVB order:

1. `console.write_line` → `standard-output-line-v1`;
2. `diagnostic.write_line` → `standard-diagnostic-line-v1`;
3. `filesystem.directory_read_v1` → `fixed-read-only-object-v1`;
4. `process.argument` → `immutable-argument-snapshot-v1`; and
5. `process.argument_count` → the same immutable snapshot.

The approval explicitly denies ambient filesystem access, file mutation,
environment access, networking, process launch, clocks, and entropy. Absence
from the approval is denial. Package metadata and a release signature are not
approval substitutes.

The exact Approval 1 identity is 927 bytes with SHA-256
`3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71`.

`Windvale-Wvb-Inspector.wvapproval` binds its exact manifest, lock, Bundle 1,
provenance, and WVB identities to five capabilities: standard output,
diagnostics, read-only `file.read_bytes`, and immutable argument count/value
snapshots. It carries the same explicit denials as WVDB Query. Its exact identity
is 917 bytes with SHA-256
`8654fa00406baed038c00abec993985eb3fb286b301d30eeb309fb92b0ae2328`.

## Target launch records

One Windows x64 and one Linux x64 record bind the approval to the exact
platform leaf, linked image, hosted application, ABI 23 entry, and five-entry
provider table already qualified by Decision 0561. Each record specifies:

- `Directory_host_entry` at image offset 235,440;
- standard output and diagnostics with Windvale LF line behavior;
- one fixed read-only object named `Windvale-Database-Storage.bin` with a
  maximum 3,072-byte chunk;
- exactly two immutable launch arguments: the fixed object name and one
  1..20-byte unsigned-decimal `u64` key; and
- explicit denial of native paths, mutation, directory enumeration,
  environment access, networking, process launch, clocks, and entropy.

The two target records share portable WVB, Bundle 1, approval, directory-host,
ABI, provider, and argument identities. They deliberately differ in their
platform leaf, linked image, hosted application, target, and generation.
The Windows record is exactly 1,315 bytes with SHA-256
`95d1a64007f487e57aec77f7466d091cc54247dcbec2f8534b5870e36715b0b3`;
the Linux record is exactly 1,310 bytes with SHA-256
`b0c976649936cf43cfa1ccb79a63093e584dda9b22cf905b954db6e3192eacd5`.

An approval records what authority the application owner accepts. A launch
record proves which rights-limited providers and target bytes will be used.
Neither record opens a file, starts a process, or proves that a provider remains
available. Provider loss and denial retain the existing explicit application
outcomes.

## Admission and ownership

`Verify-Wvdb-Approval-Records.mjs` independently rereads both live package,
lock, and provenance identity sets; compares each package and lock capability
closure; admits both exact approvals; and admits the two implemented WVDB target
records. Its focused owner rejects added capability authority, substitution in
either application, writable provider substitution, target substitution,
approval-identity substitution, and truncation.

These exact application records are an implemented product slice, not a general
approval grammar promise. Inspector target launch records, a configurable
directory, a new provider kind, or a durable approval store require a successor
contract rather than unversioned fields.
