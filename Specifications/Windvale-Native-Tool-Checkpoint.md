# Windvale native tool checkpoint 1

Status: Implemented local-development contract under Decisions 0546, 0553,
0554, 0555, 0559, 0560, 0737, and 0738.

## Purpose and boundary

Native tool checkpoint 1 is a host-local verification cache record. It avoids
repackaging an unchanged native compiler tool during a development loop. It is
not a product package, release artifact, bootstrap root, conformance report, or
qualification input. Clean retirement and dual-host qualification ignore it.

The cache root is outside the repository. Windows uses
`%LOCALAPPDATA%\Windvale\Native-Tool-Cache` by default. Linux uses
`${XDG_CACHE_HOME:-$HOME/.cache}/windvale/native-tool-cache`. An explicit
`WINDVALE_NATIVE_CACHE_ROOT` replaces that default. The owner rejects a cache
root, family, or entry reached through a Windows reparse point or Linux symbolic
link.

Project-object checkpoint 2 extends that boundary to one exact Project 2 source
closure and its WVB and WVO products. It remains development-only. It never
replaces the clean duplicate-output, paired-target, malformed-input, or
qualification owners.

Hosted-application checkpoint 1 extends the same boundary to one exact
image-mode hosted publication. It reuses deterministic container composition,
not linking or application behavior. The current-host executable is still run
through every selected database lifecycle and recovery scenario.

Project-WVB checkpoint 1 reuses one exact source-built WVB before the existing
build-driver application checkpoint is derived. It does not replace ordinary
source construction or compiler qualification.

Linked-image checkpoint 1 reuses the deterministic flat image and link map for
one exact WVO, base address, entry symbol, current-host linker front door, and
current-host linker. The current-host application is still repackaged or
revalidated and executed through every selected behavior.

## Version-1 key

The first owner caches the profile-2 compiler build-driver application. Its key
material is one ASCII line containing, in order:

1. format, host target, and profile identity;
2. SHA-256 of the exact current build-driver WVB;
3. SHA-256 of the segmented package launcher;
4. SHA-256 of the WVB staging launcher;
5. SHA-256 of the staged-image linker launcher;
6. SHA-256 of the canonical-image transport launcher;
7. SHA-256 of the hosted-container packager launcher; and
8. SHA-256 of the hosted-container toolset inventory.

Fields are joined with one ASCII hyphen. The Windows key-material line ends in
CRLF and the Linux line ends in LF because the entry is target-scoped and is
never shared as a portable cache object. The lowercase SHA-256 of that complete
line names the entry directory. A changed input, producer, inventory, target,
profile, or format therefore selects a different immutable entry.

## Checkpoint record

`Checkpoint.txt` contains exactly five ASCII lines in this order, with CRLF on
Windows and LF on Linux:

```text
windvale-native-tool-checkpoint 1
key <64-lowercase-hex>
input-sha256 <64-lowercase-hex>
output-bytes <canonical-positive-decimal>
output-sha256 <64-lowercase-hex>
```

The record is at most 512 bytes. The application is greater than zero and at
most 67,108,864 bytes. A hit recomputes the application size and SHA-256,
constructs the complete expected record, and compares it byte for byte. An
unknown, missing, duplicate, reordered, oversized, stale, truncated, or
otherwise malformed record is rejected. An existing invalid entry is never
overwritten or repaired implicitly.

## Project-object key and record

`Build-Cached-Project-Object.mjs` invokes the shared project-key core through
length-framed SHA-256 fields. In order, the fields are:

1. format `windvale-native-project-cache-key 1`;
2. namespace `database-project-object-v2`;
3. the exact `Windvale.wvws` bytes;
4. the repository-relative project identity and exact project bytes;
5. each exact `root` and `source` path plus file bytes in declaration order;
   and
6. the exact build-driver and lowerer bytes in producer order; and
7. the exact project-object checkpoint driver bytes. Those driver bytes bind
   the host-specific expected WVO inspector identity and admission procedure.

The shared key core accepts only one canonical repository-owned `.wvproj`, exactly
one root, canonical repository-contained source paths, ordinary inputs no
larger than 67,108,864 bytes, and canonical non-link paths. The lowercase
64-hex SHA-256 names a host-scoped `project-object-v2/<target>/<key>` entry.
A source, manifest, workspace, producer, order, format, or namespace change
therefore selects a different entry.

The project-object `Checkpoint.txt` contains exactly six ASCII lines:

```text
windvale-native-project-object-checkpoint 2
key <64-lowercase-hex>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
wvo-bytes <canonical-positive-decimal>
wvo-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. Both products are greater than zero and at
most 67,108,864 bytes. A miss runs the exact build driver and lowerer, admits
the candidate WVO through the digest-pinned current-host inspector, and only
then atomically publishes the complete entry. Every hit recomputes both sizes
and digests, constructs and compares the complete record byte for byte, copies
the immutable products to fresh owner output paths, and rehashes both copies.
It does not rerun WVO admission: the version-2 key binds the exact admission
driver and inspector identity, while the immutable record and copy digests
prove that the admitted bytes are unchanged. A corrupt existing entry fails
closed and is not silently repaired.

## Hosted-application key and record

`Get-Native-Hosted-Application-Cache-Key.mjs` derives a length-framed SHA-256
key from these ordered fields:

1. format `windvale-native-hosted-application-cache-key 1` and namespace
   `hosted-application-v1`;
2. current host family, selected target, profile, fragment count, and canonical
   unsigned entry;
3. exact input WVB and each native-image fragment in index order;
4. exact current-host `Package-Hosted-Wvb` driver;
5. exact hosted-toolset inventory plus all 72 inventory-verified artifacts;
6. exact enum-request WVB and paired applications;
7. the nine exact target service leaves; and
8. the exact target startup WVO.

Inputs must be canonical ordinary non-link files. Each input is nonempty and at
most 67,108,864 bytes. Image mode accepts one through eight fragments, every
nonfinal fragment is exactly 4 MiB, and every fragment is at most 4 MiB. The
lowercase 64-hex key names
`hosted-application-v1/<host-family>/<key>`; the target remains key material and
is also recorded explicitly.

The hosted-application `Checkpoint.txt` contains exactly five ASCII lines:

```text
windvale-native-hosted-application-checkpoint 1
key <64-lowercase-hex>
target <windows|linux>
application-bytes <canonical-positive-decimal>
application-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes and the application is greater than zero and
at most 67,108,864 bytes. Every hit rejects linked cache state, rehashes the
application, constructs and compares the complete expected record, copies the
application to a fresh owner path, and compares the copy byte for byte. Linux
also requires executable mode on both the checkpoint and materialized copy.

## Project-WVB key and record

`Build-Cached-Project-Wvb` and the OS x64 development batch invoke the shared
`Native-Project-Cache-Key-Core.mjs` framing through namespace `project-wvb-v2`.
The standalone key command is a thin command-line adapter over that core.
After the format, namespace, workspace, project identity and bytes, and declared
root/source closure, the ordered producers are:

1. exact native-front-door `SHA256SUMS` inventory; and
2. exact current-host native build-driver application.

The build driver writes only into a fresh private checkpoint candidate
directory after its mandatory compiler-aligned verification succeeds. The
cache hashes that candidate and atomically moves the complete directory into
the shared family. This avoids routing a private compiler-scale cache candidate
through the general WVB publisher and read-only front door, whose independent
ordinary-module size envelopes are narrower than the compiler build.

The resulting lowercase 64-hex key names
`project-wvb-v2/<host-family>/<key>`. The WVB checkpoint record contains exactly
four ASCII lines:

```text
windvale-native-project-wvb-checkpoint 1
key <64-lowercase-hex>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. The WVB is nonempty and at most 67,108,864
bytes. Every hit rejects links, recomputes the size and digest, compares the
complete expected record, and materializes a fresh byte-identical copy. The
keyed build driver and its mandatory verification are the admission boundary;
the cache does not reinterpret or execute the WVB.

`Build-Cached-Os-X64-Project-Wvbs.mjs` accepts the canonical target manifest,
one private output directory, one already staged and digest-verified build
driver, and either one target or `all`. It validates all 56 manifest rows,
derives every selected key in one process, and materializes one separately
validated checkpoint per selected project. A miss still launches a distinct
native build-driver process and atomically publishes one immutable entry. A
hit rehashes its product and compares the complete record before copying and
rehashing the private materialization. The paired owner then performs ordinary
WVB publication, fresh lower/link/package operations, current-host execution,
and all exact byte checks.

The batch owns each `.new-<key>-<pid>-<nonce>` directory it allocates. A
`finally` boundary removes that exact ordinary non-link directory after a
build, measurement, manifest, or lost-publication-race failure, but only after
proving its canonical parent remains the selected checkpoint family. A
successful atomic rename removes the temporary path and preserves the published
checkpoint. A race loser accepts the destination only after complete checkpoint
validation.

This batch is an explicit development path. No-argument owner execution,
verification-owner coordination, and qualification do not consult it.

## Linked-image key and record

`Get-Native-Linked-Image-Cache-Key.mjs` derives a length-framed SHA-256 key
from these ordered fields:

1. format `windvale-native-linked-image-cache-key 1`, namespace
   `linked-image-v1`, and current host family;
2. canonical unsigned base address and bounded entry-symbol identity;
3. exact input WVO bytes;
4. exact current-host `Link-Wvo` front door; and
5. exact current-host native linker application.

The WVO, front door, and linker must be nonempty ordinary files no larger than
67,108,864 bytes. Repository-owned producers retain canonical non-link path
requirements. Windows-generated temporary inputs may arrive through an 8.3
alias; the key helper rejects a linked final file, resolves the alias, and reads
the canonical target. The lowercase 64-hex key names
`linked-image-v1/<host-family>/<key>`.

The linked-image `Checkpoint.txt` contains exactly seven ASCII lines:

```text
windvale-native-linked-image-checkpoint 1
key <64-lowercase-hex>
entry-offset <canonical-unsigned-decimal>
image-bytes <canonical-positive-decimal>
image-sha256 <64-lowercase-hex>
map-bytes <canonical-positive-decimal>
map-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. The flat image and map are nonempty and no
larger than 67,108,864 bytes. Every hit rejects linked cache state, parses the
exact requested entry from the map, rehashes both products, reconstructs and
compares the complete record, materializes fresh copies, and compares both
copies byte for byte.

## Publication and use

On a miss, the owner packages into a newly allocated sibling directory, hashes
the complete application, writes the record, atomically renames the directory
to its key, then validates it again through the ordinary hit path. Partial
directories retain a `.new-` prefix and are never cache hits. Version 1 does not
define eviction or automatic partial-directory cleanup.

The version-2 project-object driver owns each
`.new-<key>-<pid>-<nonce>` directory it creates. Its `finally` boundary removes
that exact ordinary non-link directory after build, lowering, admission,
measurement, manifest, or lost-publication-race failure, but only after proving
the candidate remains directly inside the canonical checkpoint family. A race
loser accepts the destination only after complete record and product
validation. Other version-1 checkpoint families retain their specified cleanup
contracts.

`Test-Database-Storage.cmd --prepare-development-tools` and its shell peer build
the current build-driver WVB, prepare or validate the checkpoint, and stop.
`--development` then uses that driver plus the exact retained native lowerer to
run the target-aware 50-case database owner. It obtains ordinary project
objects through `Build-Cached-Project-Object`, the build-driver input through
`Build-Cached-Project-Wvb`, portable linked images through
`Build-Cached-Linked-Image`, and current-host executables through
`Build-Cached-Hosted-Application`; explicitly segmented cases retain their
separate staging path. `Verify-Changed.ps1`
selects that development owner only when every selected database-storage
boundary is eligible. Compiler, lowerer, specialized provider, nested-record,
and other broad changes mark the full database-storage owner mandatory. The
no-argument owner does not consult the cache and retains complete
reconstruction, duplicate-output, paired-target, and seventeen-case evidence.

## Implemented evidence

On the first Windows host, a valid hit prepares in 55.241 seconds. The complete
development lifecycle passes in 100.179 seconds, compared with 658.777 seconds
for the clean nine-case owner. A copied record with a forged output SHA-256 is
rejected with exit 1 before the cached application is executed. Linux script
syntax is checked locally; Linux cache creation, hit, corruption rejection, and
database execution remain independent-host evidence.

After the depth-two lifecycle expanded the clean owner, the measured Windows
reference was 1,111.135 seconds for fourteen cases. Creating both new
project-object entries and executing the two composed development cases took
224.928 seconds. The next validated hit took 190.863 seconds: 82.8 percent less
wall time than the clean owner, or 5.8 times faster. The project-object reuse
itself saved 34.065 seconds between those two development runs; the larger gain
comes from selecting the bounded development owner instead of the complete
qualification owner. These are host diagnostics, not portable timing claims.

Adding hosted-application checkpoints reduced the next warm two-case run from
190.863 seconds to 125.757 seconds. Both application entries were fully
rehashed and materialized, then all real database behaviors passed. Changing
one fragment byte and changing only the target produced distinct keys. An
isolated entry with one appended application byte was rejected with exit 1
before execution and was not repaired. Linux script syntax is checked locally;
independent Linux creation, hit, corruption rejection, mode preservation, and
execution remain paired-host evidence.

The complete change-aware front door, including classification and 100 planner
contract cases, passed in 141.120 seconds versus the preceding 197.4-second
measurement. The 15.363-second difference from the direct warm owner is
planner and wrapper overhead rather than database behavior.

The separate `--prepare-development-tools` measurement takes 70.704 seconds on
the same warm host even though its packaged build-driver application is a hit.
It still reconstructs the build-driver input WVB from source before deriving
the existing application-checkpoint key. That source-keyed WVB is the next
measured checkpoint boundary.

After implementing that boundary, first creation takes 78.894 seconds and a
validated preparation hit takes 9.417 seconds. The complete warm two-case owner
takes 71.048 seconds. Reordered producer inputs yield a different key. An
isolated cached WVB with one appended byte is rejected before the existing
packaged application checkpoint is consulted and is not repaired. Linux script
syntax is checked locally; independent Linux creation, hit, corruption
rejection, and execution remain paired-host evidence.

The complete change-aware front door, including classification and 101 planner
contract cases, passes in 73.531 seconds versus the preceding 141.120-second
measurement. These remain host development diagnostics rather than portable
timing or qualification claims.

After adding linked-image checkpoints, cache population for all six portable
targets took 130.240 seconds and the next direct all-hit eight-case Windows
owner took 87.800 seconds. Tool preparation took 9.190 seconds, the six
portable behaviors took 24.290 seconds, host storage took 28.570 seconds, and
the host tree reader took 25.660 seconds. A subsequent coherent changed-file
gate passed the current 79 native planner cases, GitHub workflow policy, and all
eight database behaviors; its database owner took 89.530 seconds. The direct
all-hit result is 78.19 percent below the earlier 402.638-second measurement.

The development workflow may preserve this external cache between ordinary
GitHub run attempts under a versioned host-specific key. Qualification jobs do
not bind `WINDVALE_NATIVE_CACHE_ROOT` and do not invoke the cache action. Cache
restoration changes construction cost only: every hit still revalidates its
record and every selected behavior still executes.

GitHub Verify run 31852544894 first populated both host caches, then exact
attempt 2 restored them. The complete Windows development job passed in 1m42s
with a 57,870 ms database owner; Linux passed in 1m15s with a 43,000 ms database
owner. Every reported tool, project, link, and current-host application was a
validated `Hit`, and all eight behaviors passed. The selected development scope
skipped every qualification job.
