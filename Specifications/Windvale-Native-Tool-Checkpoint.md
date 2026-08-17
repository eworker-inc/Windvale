# Windvale native tool checkpoint 1

Status: Implemented local-development contract under Decisions 0546, 0553,
0554, 0555, 0559, 0560, and 0737 through 0743.

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

1. format `windvale-native-project-cache-key 2`;
2. namespace `database-project-object-v2`;
3. the exact `Windvale.wvws` bytes;
4. the canonical producer count followed by the exact build-driver and lowerer
   bytes in producer order;
5. the exact project-object checkpoint driver bytes. Those driver bytes bind
   the host-specific expected WVO inspector identity and admission procedure.
6. the repository-relative project identity and exact project bytes; and
7. each exact `root` and `source` path plus file bytes in declaration order.

The shared key core accepts only one canonical repository-owned `.wvproj`, exactly
one root, at most 1,024 declared project inputs, canonical
repository-contained source paths, ordinary inputs no larger than 67,108,864
bytes, an aggregate project closure no larger than 268,435,456 bytes, and
canonical non-link paths. It accepts one through 16 producers whose aggregate
is at most 134,217,728 bytes. Producer fields are streamed in 1 MiB chunks and
are not retained as whole buffers. The lowercase 64-hex SHA-256 names a
host-scoped `project-object-v2/<target>/<key>` entry. A source, manifest,
workspace, producer, order, format, or namespace change therefore selects a
different entry.

Format 2 places the workspace and ordered producer closure before the project
closure. One bounded owner session may validate and hash that common prefix
once, clone the SHA-256 state for each request, and then hash the exact project
manifest and source closure independently. The prepared context retains only
the hash state and small path, size, and digest evidence for producers; it does
not retain their file contents. The standalone command uses the same context
and request functions, so session and standalone requests have one key
implementation.

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

The database development owner extends its existing authenticated current-host
session with a read-only project-object operation. A session miss returns exit
75 without changing either output, after which the owner invokes the standalone
publisher. Corruption is an error and does not fall back. The publisher rechecks
the workspace, every producer, the project manifest, and every declared source
against the request evidence after building and admission but before writing
the record or renaming the candidate. Qualification does not start the session.

## Segmented-project key and record

`Build-Cached-Segmented-Project.mjs` uses the same length-framed project-key
format 2 core with namespace `database-segmented-project-v1`. After the format,
namespace, and workspace, its producer fields bind the exact build driver,
current-host segmented WVO producer, segmented image linker, image transport,
and checkpoint driver in that order; the exact project manifest and ordered
source closure follow that common producer prefix. The driver also
verifies the three host-specific producer digests before accepting either a
hit or a miss. The resulting lowercase 64-hex key names the host-scoped
`segmented-project-v1/<target>/<key>` entry.

The segmented-project `Checkpoint.txt` contains these exact ASCII lines:

```text
windvale-native-segmented-project-checkpoint 1
key <64-lowercase-hex>
entry-offset <canonical-unsigned-decimal>
fragments <1-through-8>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
manifest-bytes <canonical-positive-decimal>
manifest-sha256 <64-lowercase-hex>
fragment-0-bytes <canonical-positive-decimal>
fragment-0-sha256 <64-lowercase-hex>
...
```

The record is at most 4,096 bytes. The WVB is nonempty and no larger than
67,108,864 bytes. The canonical image is nonempty and no larger than
33,554,432 bytes across one through eight fragments of at most 4,194,304 bytes.
The exact `WVLI 1` manifest must declare its own size, image size, entry,
fragment count, fragment limit, and one contiguous ordered extent for every
measured fragment. A miss builds the WVB, stages WVO fragments, links and
transports the image, deletes intermediate products, validates the canonical
manifest and every product digest, and atomically publishes only the admitted
WVB, manifest, fragments, and record. A hit repeats the complete structural
manifest, size, digest, entry-set, and record validation; it then copies each
product to private owner paths and rehashes every copy. Corrupt entries fail
closed and are not repaired.

## Hosted-application key and record

`Get-Native-Hosted-Application-Cache-Key.mjs` and the bounded session adapter
use `Native-Hosted-Application-Cache-Core.mjs` to derive the same
length-framed SHA-256 key from these ordered fields:

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

The database development owner may start one current-host hosted-application
session after tool preparation. Session startup reads, inventory-validates,
hashes, and retains the exact shared producer fields listed in items 4 through
8 once. Each request still reads and hashes its exact WVB and ordered fragments,
replays the retained producer fields through the unchanged version-1 framing,
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

## Project-WVB key and record

`Build-Cached-Project-Wvb` and the OS x64 development batch invoke the shared
`Native-Project-Cache-Key-Core.mjs` framing through namespace `project-wvb-v2`.
The standalone key command is a thin command-line adapter over that core.
After the format, namespace, and workspace, the ordered producers are:

1. exact native-front-door `SHA256SUMS` inventory; and
2. exact current-host native build-driver application.

The exact project identity and bytes plus declared root/source closure follow
that common producer prefix.

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
hashes the inventory and build driver into one bounded format-2 context, clones
that state to derive every selected project key, and materializes one separately
validated checkpoint per selected project. A miss still launches a distinct
native build-driver process, rechecks the complete keyed input evidence after
the build, and atomically publishes one immutable entry. A hit rehashes its
product and compares the complete record before copying and rehashing the
private materialization. The paired owner then performs ordinary WVB
publication, fresh lower/link/package operations, current-host execution, and
all exact byte checks.

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

`Build-Cached-Linked-Image-Set.mjs` derives a length-framed SHA-256 key from
format `windvale-native-linked-image-cache-key 2`, namespace
`linked-image-v2`, current host family, canonical unsigned base address,
bounded entry-symbol identity, exact canonical input count, every exact input
WVO in command order, the producer script, current-host `Link-Wvo` front door,
and current-host native linker. It accepts one through 64 WVOs. Each input and
the aggregate immutable snapshot are bounded by the 32 MiB large-native linker
admission limit. Repository-owned producers require canonical non-link paths;
Windows-generated temporary WVOs may use an ordinary 8.3 alias whose canonical
target supplies the bytes. The lowercase key names
`linked-image-v2/<host-family>/<key>`.

The version-2 record contains exactly eight ASCII lines:

```text
windvale-native-linked-image-checkpoint 2
key <64-lowercase-hex>
input-count <canonical-positive-decimal>
entry-offset <canonical-unsigned-decimal>
image-bytes <canonical-positive-decimal>
image-sha256 <64-lowercase-hex>
map-bytes <canonical-positive-decimal>
map-sha256 <64-lowercase-hex>
```

On a miss, the producer writes the already-keyed input buffers to private
ordinary snapshot files, links only those snapshots, and proves that its own
script, the current-host front door, and the linker remain byte-exact before
publication. A `finally` boundary removes the exact locally created
`.new-<key>-<nonce>-*` sibling after linker, parsing, measurement, record, or
lost-race failure, but only after proving its parent and key prefix. A race
loser accepts the winner only after complete checkpoint validation. A hit
validates the exact three-entry directory, canonical map entry, record, image,
and map before copying and rehashing both private outputs. Owner outputs inside
the cache root are rejected.

## Publication and use

On a miss, the owner packages into a newly allocated sibling directory, hashes
the complete application, writes the record, atomically renames the directory
to its key, then validates it again through the ordinary hit path. Partial
directories retain a `.new-` prefix and are never cache hits. Version 1 does not
define eviction or automatic partial-directory cleanup.

The version-2 project-object and version-1 segmented-project drivers own each
`.new-<key>-<pid>-<nonce>` directory they create. Their `finally` boundaries remove
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
objects first through the read-only session and falls back on a true miss to
`Build-Cached-Project-Object`; it obtains the build-driver input through
`Build-Cached-Project-Wvb`, every ordinary one-or-more-object linked image
through `Build-Cached-Linked-Image-Set`, and current-host executables through
`Build-Cached-Hosted-Application`. Explicitly segmented development cases use
`Build-Cached-Segmented-Project` for the exact WVB and canonical image before
the unchanged hosted-application or provider-overlay path. `Verify-Changed.ps1`
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

After adding segmented-project checkpoints, the Windows 50-case database owner
passes with every tool, project, link, and application checkpoint reporting
`Hit` in 323,820 ms. The preceding project-object-v2 all-hit owner took 500,610
ms, so this boundary saves 176,790 ms or 35.31 percent and is 1.55 times faster.
The bounded regression independently passes creation, hit, corruption
preservation, failed-producer cleanup, and a four-way same-key publication race.
Linux construction, corruption, race, and database execution remain
independent-host evidence.

With hosted producer-session reuse, the same representative application hit
takes 129 through 165 ms instead of 1,573 through 2,393 ms. The change-aware
Windows 50-case database owner passes with every checkpoint reporting `Hit` in
281,240 ms, down from 323,820 ms; the portable section falls from 115,980 ms to
81,940 ms. The session regression proves exact standalone-key equivalence, four
concurrent serialized hits, corruption and miss output preservation, executable
mode, and clean teardown. Executable-mode preservation is asserted when the
regression runs on Linux; independent Linux session and owner execution remain
required.

Before version-2 ordered linked-image checkpoints, four measured host-root
links took 16,460, 15,180, 15,650, and 10,370 ms even though their project and
application checkpoints hit. Their corresponding project admissions took 210
through 310 ms and application materializations took 140 through 190 ms. The
warm Windows 50-case owner now passes in 101,370 ms instead of 281,240 ms,
saving 179,870 ms or 63.96 percent. Host-root-writer falls from 61,810 ms to
3,560 ms while retaining every normal, replay, interruption, recovery, fill,
split, and read execution. The version-2 regression proves four-way same-key
publication, one-input and multi-input exact hits, input-order key separation,
corruption preservation, failed-link cleanup, and malformed-count rejection.
Independent Linux execution remains required.

Replacing the obsolete single-input version-1 wrapper with the same version-2
publisher reduces a controlled warm link hit from a 641.6 ms mean to 107.0 ms,
an 83.32 percent reduction and 6.00-fold speedup. A measured TreeNode database
case falls from 1,410 through 1,490 ms to 940 through 960 ms. The all-hit
change-aware Windows owner falls from 101,370 ms to 85,010 ms, while its
portable section falls from 74,110 ms to 58,410 ms. The direct no-argument and
qualification link paths remain unchanged.

Project-key format 2 reduces repeated producer preparation without weakening
per-project identity. Ten empty Node invocations averaged 54.4 ms, while the
former standalone project-key command averaged 124.9 ms. In an isolated warm
project-object workload, eight standalone hits averaged 149.0 ms and eight hits
through the already-running owner session averaged 98.0 ms, a 34.23 percent
boundary reduction and 1.52-fold speedup. The representative TreeNode case fell
from 940 through 960 ms to 810 ms while retaining its fresh hosted execution.

On the same Windows host, a hosted-only ready session used 69.84 MiB working
set and 80.32 MiB private memory. A rejected whole-buffer project context used
107.45 MiB and 117.61 MiB. Streaming the producer fields produced the retained
design at 73.78 MiB and 83.14 MiB, only 3.94 MiB working-set and 2.82 MiB private
memory above the hosted-only session. The bounded regression proves four exact
session hits, miss and corruption output preservation, clean teardown, a stable
prehashed producer snapshot, excessive-producer rejection, failed-publication
cleanup, and same-key race convergence. Format 2 intentionally makes older
project-key entries inert. A one-time Windows population passed all 51 database
development steps in 736,610 ms and all 56 OS projects plus 336 code-emission
cases. The final change-aware warm database owner passed in 81,910 ms, down from
85,010 ms; its portable section fell from 58,410 ms to 55,340 ms. This saves
3.65 percent for the complete owner and 5.26 percent for the portable section.
Independent Linux runtime evidence remains required.
