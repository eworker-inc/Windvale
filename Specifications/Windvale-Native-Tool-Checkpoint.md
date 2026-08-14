# Windvale native tool checkpoint 1

Status: Implemented local-development contract under Decisions 0546 and 0553.

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
project objects through `Build-Cached-Project-Object`. `Verify-Changed.ps1`
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
