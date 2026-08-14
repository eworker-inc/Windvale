# Windvale native tool checkpoint 1

Status: Implemented local-development contract under Decision 0546.

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

## Publication and use

On a miss, the owner packages into a newly allocated sibling directory, hashes
the complete application, writes the record, atomically renames the directory
to its key, then validates it again through the ordinary hit path. Partial
directories retain a `.new-` prefix and are never cache hits. Version 1 does not
define eviction or automatic partial-directory cleanup.

`Test-Database-Storage.cmd --prepare-development-tools` and its shell peer build
the current build-driver WVB, prepare or validate the checkpoint, and stop.
`--development` then uses that driver plus the exact retained native lowerer to
run the composed host-storage lifecycle. The no-argument owner does not consult
the cache and retains complete reconstruction, duplicate-output, paired-target,
and nine-case evidence.

## Implemented evidence

On the first Windows host, a valid hit prepares in 55.241 seconds. The complete
development lifecycle passes in 100.179 seconds, compared with 658.777 seconds
for the clean nine-case owner. A copied record with a forged output SHA-256 is
rejected with exit 1 before the cached application is executed. Linux script
syntax is checked locally; Linux cache creation, hit, corruption rejection, and
database execution remain independent-host evidence.
