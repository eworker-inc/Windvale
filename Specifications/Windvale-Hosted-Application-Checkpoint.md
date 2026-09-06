# Windvale hosted-application checkpoint

## Status

Implemented local-development contract. Hosted application keys use version 2;
the stored application record remains version 1.

## Purpose and boundary

This contract owns hosted application cache keys, records, and session behavior.
The [native tool checkpoint](Windvale-Native-Tool-Checkpoint.md) defines the
shared host-local cache root and broader development boundary. Cached products
do not replace fresh behavior or required independent reconstruction.

## Hosted-application key and record

`Get-Native-Hosted-Application-Cache-Key.mjs` and the bounded session adapter
use `Native-Hosted-Application-Cache-Core.mjs` to derive the same
length-framed SHA-256 key from these ordered fields:

1. format `windvale-native-hosted-application-cache-key 2` and namespace
   `hosted-application-v1`;
2. current host family, selected target, profile, fragment count, and canonical
   unsigned entry;
3. exact input WVB and each native-image fragment in index order;
4. exact current-host `Package-Hosted-Wvb` driver;
5. exact hosted-toolset inventory plus all 72 inventory-verified artifacts;
6. exact enum-request WVB and paired applications;
7. the nine exact target service leaves;
8. the exact target startup WVO;
9. the current cache-key implementation; and
10. the Node runtime version text.

Fields 4 through 10 retain a 40-byte fingerprint: the original byte count as
little-endian u64 followed by its 32-byte SHA-256 digest. All artifact bytes are
still read and the 72 toolset digests are checked against the inventory. At most
four inventory artifacts are read concurrently, and their fields retain
inventory order. These fingerprints replace raw producer payloads; version 1
keys are not reused. The profile-independent segmented-image key also binds the
Node runtime version.

Inputs must be canonical ordinary non-link files. Each input is nonempty and at
most 67,108,864 bytes. The reader checks the opened file identity and initial
size, allocates at most that size plus one byte, and rejects a different byte
count during the bounded read. It closes the file handle on every path.
Image mode accepts one through sixteen fragments, every
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

The database development owner may start one current-host hosted-application
session after tool preparation. Session startup reads, inventory-validates,
hashes, and retains the shared producer fingerprints listed in items 4 through
10 once. Each request still reads and hashes its exact WVB and ordered fragments,
replays the retained fingerprints through version-2 key framing,
and therefore selects the byte-identical standalone key. A hit independently
validates the exact checkpoint entry, record, product size and digest, copies
the application to its private owner path, rehashes the copy, and preserves
Linux executable mode.

The session is read-only with respect to checkpoint publication. A missing key
returns a distinguished miss without changing the owner output; the caller
then invokes the standalone checkpoint driver, which repeats complete producer
validation and retains its ordinary immutable publication boundary. Later
requests can consume that published entry. Corrupt existing entries fail
closed and do not fall back. The server binds one random 256-bit token to one
loopback-only port and one bounded readiness record inside the owner's private
temporary directory, serializes requests, rejects other targets and malformed
or oversized protocol messages, and removes the readiness record on shutdown.
No-argument and qualification verification do not start the session.

## Verification owner

The `segmented-hosted-wvb-cache` owner includes the hosted-session contract
checks. It verifies standalone/session key agreement, compact producer identity,
concurrent hits, invalid inputs, corruption, misses, readiness records, and
process cleanup. Hosted-session, key-tool, and this specification's changes
select that owner; database behavior remains separately owned.
