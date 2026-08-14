# Windvale native tool checkpoint 1

Status: Implemented local-development contract under Decisions 0546, 0553,
0554, and 0555.

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

Project-object checkpoint 1 extends that boundary to one exact Project 2 source
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

`Get-Native-Project-Cache-Key.mjs` derives the project-object key through
length-framed SHA-256 fields. In order, the fields are:

1. format `windvale-native-project-cache-key 1`;
2. namespace `database-project-object-v1`;
3. the exact `Windvale.wvws` bytes;
4. the repository-relative project identity and exact project bytes;
5. each exact `root` and `source` path plus file bytes in declaration order;
   and
6. the exact build-driver and lowerer bytes in producer order.

The key helper accepts only one canonical repository-owned `.wvproj`, exactly
one root, canonical repository-contained source paths, ordinary inputs no
larger than 67,108,864 bytes, and canonical non-link paths. The lowercase
64-hex SHA-256 names a host-scoped `project-object-v1/<target>/<key>` entry.
A source, manifest, workspace, producer, order, format, or namespace change
therefore selects a different entry.

The project-object `Checkpoint.txt` contains exactly six ASCII lines:

```text
windvale-native-project-object-checkpoint 1
key <64-lowercase-hex>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
wvo-bytes <canonical-positive-decimal>
wvo-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. Both products are greater than zero and at
most 67,108,864 bytes. Every hit recomputes both sizes and digests, constructs
the complete expected record, compares it byte for byte, copies the immutable
products to fresh owner output paths, compares both copies byte for byte, and
runs complete structural WVO admission. A corrupt existing entry fails closed
and is not silently repaired.

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

`Build-Cached-Project-Wvb` invokes the existing
`Get-Native-Project-Cache-Key.mjs` framing with namespace `project-wvb-v1`.
After the format, namespace, workspace, project identity and bytes, and declared
root/source closure, the ordered producers are:

1. exact current-host `Build-Wvb` launcher;
2. exact native-front-door `SHA256SUMS` inventory;
3. exact current-host native build-driver application; and
4. exact current-host native WVB publisher application.

The resulting lowercase 64-hex key names
`project-wvb-v1/<host-family>/<key>`. The WVB checkpoint record contains exactly
four ASCII lines:

```text
windvale-native-project-wvb-checkpoint 1
key <64-lowercase-hex>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. The WVB is nonempty and at most 67,108,864
bytes. Every hit rejects links, recomputes the size and digest, compares the
complete expected record, materializes a fresh byte-identical copy, and invokes
the current native `Verify-Wvb` front door. The verifier remains a current
admission boundary rather than key material.

## Publication and use

On a miss, the owner packages into a newly allocated sibling directory, hashes
the complete application, writes the record, atomically renames the directory
to its key, then validates it again through the ordinary hit path. Partial
directories retain a `.new-` prefix and are never cache hits. Version 1 does not
define eviction or automatic partial-directory cleanup.

`Test-Database-Storage.cmd --prepare-development-tools` and its shell peer build
the current build-driver WVB, prepare or validate the checkpoint, and stop.
`--development` then uses that driver plus the exact retained native lowerer to
run the composed host-storage lifecycle. It obtains the two large host-storage
project objects through `Build-Cached-Project-Object`, the build-driver input
through `Build-Cached-Project-Wvb`, and the two current-host executables through
`Build-Cached-Hosted-Application`. `Verify-Changed.ps1`
selects that development owner only when every selected database-storage
boundary is eligible. Compiler, lowerer, specialized provider, nested-record,
and other broad changes mark the full database-storage owner mandatory. The
no-argument owner does not consult the cache and retains complete
reconstruction, duplicate-output, paired-target, and fourteen-case evidence.

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
