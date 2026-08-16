# Windvale capability approval and launch records

## Status

Approval 1 and Launch Record 1 are implemented for the exact WVDB Query package
selected by Milestone 2 under
[Decision 0564](../Documents/Decisions/0564-First-Installed-Capability-Approval-And-Launch-Records.md).
Milestone 4 adds exact Approval 1 and Launch Record 2 records for WVB Inspector
under [Decision 0570](../Documents/Decisions/0570-Wvb-Inspector-Launch-Record-2.md).
[Decision 0693](../Documents/Decisions/0693-Echo-Package-Approval-And-Launch-Record-3.md)
adds exact Approval 1 and Launch Record 3 records for Echo; [Decision 0707](../Documents/Decisions/0707-Echo-Independent-Metadata-Package-Migration.md)
rebinds them to Echo's independent module metadata. These
records make all three capability proofs inspectable as installation and launch
policy without creating a general launcher, approval UI, or capability database.
The 13-case approval owner and ten-case Echo command owner cover three
applications, nine records, thirteen approved capabilities, six target records,
and three exact command executions per host.

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
diagnostics, explicit-host-file read-only `file.read_bytes`, and immutable
argument count/value snapshots. It carries the same explicit denials as WVDB
Query while distinguishing one explicitly supplied host file from ambient
enumeration or mutation. Its exact identity is 923 bytes with SHA-256
`32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f`.

`Windvale-Echo.wvapproval` binds the exact Echo manifest, lock, Bundle 1,
provenance, and 927-byte metadata-bearing WVB identities. Its complete three-capability closure
is standard line output plus immutable argument value/count snapshots. Absence
and the explicit denials leave it without diagnostics, filesystem, environment,
network, process-launch, clock, or entropy authority. Its exact identity is 793
bytes with SHA-256
`cf2bf11b8b737466fad088e383004ee3fbdef45609ff046022fa6bf4a5c232b9`.

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
`213a59ecf1f9bde65ce596e2627bce1add249f936fc781b71dcba1eb88bcefe7`;
the Linux record is exactly 1,310 bytes with SHA-256
`8ff3152ad30951235abb3504a372c57b2cb1bbff1410bb47933136645580ab88`.

### WVB Inspector Launch Record 2

Launch Record 2 binds a portable package directly to one already installed,
target-specific native host application. It adds the exact lock identity and
uses a named `Main` entry rather than WVDB's composed WVO/image fields. Its
provider-table version 2 binds exactly one 1..4,096-byte UTF-8 host-path argument
to read-only `file.read_bytes`, plus immutable argument snapshots, output, and
diagnostics. It denies enumeration, mutation, environment, network,
process-launch, clock, and entropy authority.

The selected host applications are the exact `wvdump` front doors already
contained in the `v0.1.0` installers. Their embedded source product is the exact
76,527-byte package WVB with SHA-256
`293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753`.
The Windows Launch Record 2 is 1,000 bytes with SHA-256
`eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b`;
the Linux record is 996 bytes with SHA-256
`f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b`.

### Echo Launch Record 3

Launch Record 3 is the first direct native-host profile with a variable-length
argument vector. It binds the exact 17,009-byte Bundle 1 with SHA-256
`9abc97a4088ed60ba26015909ed4375ce92e27e9280fbe8be892c1b14ee7eb85`,
Lock 1, approval, 927-byte WVB, named `Main` entry, and target host application.
Provider table 3 has exactly three ordered bindings: standard output with LF
line behavior, immutable argument values, and the count from that same snapshot.

The `argument-vector strict-utf8 0 67 4096 65536` field freezes the existing
hosted-resource boundary: zero through 67 arguments, no more than 4,096 strict
UTF-8 bytes in one argument, and no more than 65,536 bytes in the complete
vector. Empty arguments are values, not omissions. The record explicitly denies
ambient filesystem, mutation, diagnostic output, environment, network,
process-launch, clock, and entropy providers.

The Windows Launch Record 3 is 918 bytes with SHA-256
`39839a75c852c46eec896bfe47f8c43228d5e2fff650a722ea72f08f55e7a8b8`;
the Linux record is 914 bytes with SHA-256
`1010e131f66c45dec68b29b2f2797bc6ef47c4c6c3b83554f1e0872949a670fb`.
Their exact host applications are respectively 22,016 and 24,576 bytes. These
records describe one Echo command profile; they do not establish a general
dynamic provider-table grammar.

An approval records what authority the application owner accepts. A launch
record proves which rights-limited providers and target bytes will be used.
Neither record opens a file, starts a process, or proves that a provider remains
available. Provider loss and denial retain the existing explicit application
outcomes.

## Admission and ownership

`Verify-Wvdb-Approval-Records.mjs` independently rereads both live package,
lock, and provenance identity sets; compares each package and lock capability
closure; admits both exact approvals and all four target records; verifies both
inspector host binaries; and executes the exact inspector command on each native
host. Its focused owner rejects added capability authority, substitution in
either application, host substitution, writable provider substitution, target
substitution, approval-identity substitution, and truncation.

`Verify-Echo-Command-Launch.mjs` independently activates one exact Generation 1,
resolves `echo` through the Windvale-written resolver, and dispatches its native
host without a command shell. Its paired-host owner proves argument and empty
execution, rejects bundle, approval, host, capability-binding, argument-budget,
and unknown-command substitutions, and requires private host cleanup after each
attempt.

These exact application records are an implemented product slice, not a general
approval grammar promise. A configurable directory, mutable provider, dynamic
approval UI, or durable approval store requires a successor contract rather
than unversioned fields.
