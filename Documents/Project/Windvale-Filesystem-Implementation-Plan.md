# Windvale filesystem implementation plan

## Status and boundary

Selected implementation track after the typed application-start boundary. The
durable capability and service direction is already accepted by Decisions 0140,
0153, 0154, 0159, 0181, and the OS architecture. This plan sequences that work;
it does not claim a writable filesystem is implemented today.

Windvale defines semantic filesystem capabilities rather than inheriting POSIX,
Win32, NTFS, or ext4 behavior. Windows and Linux remain permanent providers of
those contracts through rights-limited native adapters. Windvale OS keeps block,
interrupt, memory, DMA/IOMMU, accounting, revocation, and teardown mechanisms at
the kernel/driver boundary while an isolated service owns paths, directories,
file data, allocation, metadata, and recovery policy.

## Format direction

No new Windvale on-disk format is selected initially. The implementation order
is:

1. preserve the implemented immutable `WVRS 1` store and `WVDS 1` directory
   snapshot as boot/resource inputs, without calling them a filesystem;
2. bind the shared semantic capability to native Windows and Linux host
   adapters, allowing each host OS to own NTFS/ext4 and native-handle details;
3. add a bounded FAT32 provider in an isolated Windvale OS service for boot and
   interchange media, beginning read-only and adding mutation only with exact
   partial-progress, flush, replacement, and recovery contracts;
4. add read-only NTFS and ext4 format adapters only when real disk-interchange
   or recovery cases select them; keep their parsers outside the kernel; and
5. consider a new Windvale-native disk format only after measured durability,
   snapshot, package-store, and update requirements cannot be met coherently by
   an existing format or a small semantic layer above it.

Using a Windows or Linux host adapter does not mean reimplementing NTFS or ext4.
Those adapters use the host kernel's filesystem and translate only the declared
Windvale contract. Direct Windvale OS access to such volumes is a separate,
format-specific service and qualification claim.

## Delivery slices

### Filesystem slice 1: semantic operation core — implemented candidate

Freeze versioned path-segment, directory, file, metadata, read-at, write-at,
flush, exact replacement, deadline/cancellation, and failure results. Separate
rejection, exact partial progress, completion, and indeterminate mutation.

The current candidate freezes the shared single-segment rule, four explicit
open profiles, generation-safe file references, `u64` offsets and lengths,
65,536-byte transfers, read/write/set-length/close, two flush classes, and
rejection/partial/completed/indeterminate validation. Metadata, exact
replacement, deadline/cancellation, and the provider wire envelope remain in
the next semantic/service increment rather than being claimed here.

### Filesystem slice 2: Windows and Linux host adapters — first read-only native path implemented candidate

Implement rights-limited providers over native host facilities. Test the same
logical corpus on both hosts, including Unicode policy, case behavior exposure,
links/reparse points, partial writes, replacement, flush, stale references,
revocation, and provider restart. Platform extensions remain separate from the
shared core.

The portable adapter core fixes exact no-link open plans for all four shared
profiles on Windows and Linux, including regular-file post-open enforcement.
The first ABI-23 read-only leaves now perform real host I/O: Linux uses
`O_NOFOLLOW` plus `fstat`, while Windows opens the reparse point itself and
rejects directory/reparse metadata. The same six-case owner covers exact data,
missing/denied/unavailable results, and a link/reparse traversal attempt. Windows
execution passes locally; independent Linux execution, configurable directory
instances, native error normalization, writable profiles, partial writes,
replacement, flush, revocation, and restart remain active work.

### Filesystem slice 3: Windvale OS service protocol — wire validation implemented candidate

Define bounded versioned IPC requests and responses, generation-safe file and
directory references, queue limits, resource-domain charges, peer loss, and
teardown. The kernel remains path- and format-blind.

`WVFQ 1` now supplies bounded request admission with correlation, separate
directory/file references, `u64` positions, exact payload geometry, and
pre-provider rejection. `WVFP 1` validates the echoed request identity,
generation-safe result references, read geometry, and complete, partial, or
indeterminate mutation outcomes. Provider dispatch, multi-handle inventory,
queue, resource-domain charge, peer-loss reply construction, and guest teardown
remain active work.
The first capacity-one handle state now separately proves ownership, profile
rights, generation reuse, stale rejection, close, and peer-exit reclamation;
dispatch and multi-client queueing remain open.

Endpoint transfer profiles 1 now close the previous message-size mismatch:
control traffic remains bounded to 4,096 bytes, block traffic reserves an exact
48-byte request and 4,144-byte reply window, and filesystem traffic reserves
the complete 65,600-byte envelope. Admission binds both peer generations,
checked user windows, mapping rights, non-overlap, and a 17-page ceiling. This
is portable kernel policy; the x86-64 syscall 6/7 adapter is still required.

### Filesystem slice 4: block and FAT32 provider — read transaction implemented candidate

Add one isolated block-device path and a bounded FAT32 service. Qualify malformed
boot sectors, directories, cluster chains, cycles, oversized geometry, media
removal, interrupted mutation, and deterministic read-only images before any
writable profile is enabled.

FAT32 volume admission 1 checks the exact 512-byte boot sector against an
independently supplied device extent, derives all geometry in `u64`, requires
FAT capacity for the strict compatible FAT32 cluster range, and bounds the root,
FSInfo, backup, reserved fields, and active-FAT selection. Twenty-five focused
volume cases pass. Cluster-chain admission 1 now locates selected-FAT entries,
masks the reserved high nibble, classifies every special value, rejects cycles
and truncated/trailing traces, and enforces a caller-selected ceiling no larger
than 4,096 clusters. The combined 45-case owner passes in paired native images.
FSInfo/backup content validation, mirrored-FAT comparison, long-name/path
mapping, live block-provider IPC, media removal, and completed file-data reads
remain before slice 4 is complete. Short-directory admission now bounds one through
4,096 entries, validates attributes and cluster/size fields, distinguishes
files and directories, detects duplicate targets, and separates an end marker
from an incomplete chain. Block-read transaction 1 admits a generation-safe,
rights-limited sector grant, plans at most eight exact sectors, and distinguishes
stale, unavailable, lost-provider, and invalid-payload completion. Its 14-case
native owner passes; this is policy evidence rather than a hardware-driver
claim.

`WVBR 1`/`WVBP 1` provide the exact capacity-one provider wire envelope. The
implemented exchange lifecycle now binds it to one endpoint and one grant,
separates construction from dispatch, rejects concurrent and duplicate
completion, consumes dispatched sequences exactly once, distinguishes
pre-dispatch from post-dispatch cancellation, and requires teardown after peer
loss. An immutable block-image provider now independently admits the request,
maps at most eight sectors inside one 64 MiB read-only image, and emits the exact
validated response. Endpoint profile 2 now admits that exact wire geometry, but
the privileged x86-64 copy/wait/reply adapter and hardware block driver remain
pending.

File-read plan 1 maps one admitted `u64` file offset plus the exact resolved
cluster into at most eight sectors and 4,096 covered bytes. File-read
transaction 1 now admits the complete chain, selects each required ordinal,
binds an authorized file reference and media generation, and owns every exact
block-exchange identity from begin through dispatch and completion. It copies
partial-sector bytes and accumulates the full 65,536-byte shared limit before
emitting a validator-accepted `WVFP 1` reply.
Its 18-case owner crosses two clusters and exchanges. Endpoint profile 3 now
admits the maximum shared reply, while the privileged machine adapter, block
driver, media discovery/change detection, and guest execution remain open.

### Filesystem slice 5: boot and application integration

Launch the provider through the typed application-start boundary, bind one
rights-reduced directory/file capability into a normal application, demonstrate
service failure containment and restart/teardown, and reproduce the logical
result on Windows, Linux, and Windvale OS where supported.

The first fixed boot-composition candidate now requires filesystem contract
version 1 and its 65,536-byte transfer ceiling before Probe 40 returns token 97.
This is selection evidence only. Checked provider start, resource-domain and IPC
queue binding, capability publication, consumer execution, restart, and teardown
remain the slice-5 gate.

`WVSR 1` checks the first fixed application request before typed admission.
`WVPR 1` now separately admits filesystem profile 2 with an isolated 64-page,
one-process, one-endpoint domain, three bindings, bounded rights, and the shared
four-slot/one-control-reserved queue. Live domain allocation, endpoint creation,
executable launch, and capability publication remain required.

The first separate filesystem user image now builds deterministically, returns
readiness token 46, and waits on endpoint `65538`. The current process-object
constructor embeds the immutable image in the boot object, but Probe 40 does not
yet allocate its domain, map it, bind its endpoint, publish it, or launch it.
Provider launch transaction 1 now admits its exact 48 RX plus 16 private-page
partition, one process, one endpoint, readiness publication, rollback, stale
rejection, and zero-charge teardown. Privileged machine binding remains open.

### Filesystem slice 6: optional format adapters or native format decision

Select read-only ext4/NTFS adapters or a Windvale-native format only from a named
recovery, interoperability, package-store, or durability requirement. Each gets
its own parser bounds, malformed corpus, write policy, and qualification claim.

## Stability gate

The filesystem track reaches its first stable level when slices 1 through 5
pass focused Windows/Linux tests and the pinned guest boot proves one bounded
provider, one application consumer, explicit denial, peer-loss cleanup, and
deterministic teardown. It does not require broad POSIX compatibility, every
NTFS/ext4 feature, memory mapping, links, watches, ACL emulation, or a new disk
format.
