# Decision 0165: Contain one Windvale service failure

- Status: Implemented candidate
- Date: 2026-08-03
- Owners: Windvale OS process, IPC, and service foundation
- Advances: [Decision 0084](0084-Windvale-Os-Architecture.md), [Decision 0159](0159-First-Guest-Directory-Service.md)

## Context

Probe 35 proved that an isolated init service can answer resource and directory requests for two rebuilt clients. It did not prove what happens when that service rejects a live request. A service fault could therefore still strand a waiting client, leave copied-message state reachable, or turn one user-process error into a kernel panic.

The next useful pressure is smaller than a scheduler, supervisor, registry, restart policy, or VFS: deliberately fault the existing init service after a malformed request and prove that the kernel closes the channel, wakes the blocked client with an exact transport failure, and continues to deterministic shutdown.

## Decision

Firmware Probe 36 adds a fifth `service-fault` scenario and advances the internal process and channel records to `WVPROC15` and `WVCHAN04`. ABI 22, context 7, service table 5, WVA seam 11, admission 4/bridge 2, retained bridge 10, `WVKMEM14`, paging 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1` remain unchanged.

The service-fault client first completes the existing 55-byte resource lookup and validates its 116-byte reply. It then sends a 37-byte `WVDQ 1` message whose embedded total length is 36. The init service rejects that inconsistent envelope and deliberately executes `CLI` at CPL3. The existing general-protection entry records vector 13 and error code 0 for the init process.

The kernel accepts containment only for that exact live state: init is the faulted role, the first directory request has been delivered, client generation 1 is the retained waiter, request/reply counts are `2/1`, and the copied message has exactly 37 bytes. It then:

1. marks init reference `65537` as the faulted channel peer;
2. closes the channel once and increments its wake count once;
3. clears state, sender, receiver, waiter, byte length, both destinations, and both capacities;
4. resumes the waiting client with exact syscall result `-1` (`0xFFFFFFFF`), which is a transport failure rather than a forged `WVDR 1` application reply;
5. requires the client to exit cleanly after exactly three syscalls with result `6`;
6. removes both granted client aliases and their private publication, reloads init's root, and verifies the allocator remains exactly exhausted; and
7. continues through the retained native and system-profile probes to clean Q35 shutdown.

The failure scenario intentionally stops after client generation 1. Normal and contained-client-fault scenarios retain the Probe-35 two-generation release, same-root rebuild, service exchanges, and cleanup proof. The artifact builder binds the selected scenario into its process image so a normal, client-fault, or service-fault image cannot be combined with the wrong machine coordinator.

Portable `Process-Foundation.wv` owns the new service-failure policy predicate. The malformed caller and service rejection remain in canonical WVA. Stage 0 still owns raw records, page tables, exception dispatch, machine coordination, and firmware packaging. This slice needs no compiler or ABI change.

## Evidence

The candidate carries these exact identities:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 10,333 | `32ee6cd53018a71dd7f2c2596f1a2242622a2f261539f70e164cd8b5d84ba43c` |
| Process-policy WVO | 70,050 | `2f71db723bbcbdedcf5d4f6512230e65537644d02fbda1709debeaf170af58ca` |
| Init WVA object | 3,119 | `792314c634fb1c4d701080a1b9cb12037e21ea242971413a94f86e8569ce766d` |
| Linked init image | 6,119 | `8ba61e354de025d8b02dedcca22d901936d8aecbf5ff2df728648abc714e5f64` |
| Service-fault client WVA object | 1,153 | `c5ed46d78cd8b8fba30b3425d23d1d8adcbe3638a7706f81945e2eeebb833214` |
| Linked service-fault client | 447,821 | `451364e3f4595bf9c44707da8dafae75ebc37c18860f94cebed743817f533bff` |
| Normal process-machine WVO | 490,972 | `fd4b79bd5fb55df6c6e0f884115947220cf6308e6fbe77f647d6821942cb2dc2` |
| User-fault process-machine WVO | 491,004 | `57a9874a76d26a1f9af9614051c504ff3022543604db0e9de08a9fe7725bee12` |
| Service-fault process-machine WVO | 479,556 | `d7220ed33de6f8dac1946a0dc44c87fe4937f7730f3f86997a38d65ddcf5716d` |

The focused Windows Release build has zero warnings and all 38 OS tests pass. All five pinned Windows QEMU 11.0/Q35/TCG scenarios pass with complete exact serial evidence. Cross-host qualification remains pending until the exact implementation commit passes the repository Qualification workflow on Windows and digest-pinned Debian.

## Consequences

- One user-space service failure no longer requires a kernel panic or an indefinitely blocked caller.
- Peer loss is an explicit transport result and cannot be confused with a service-defined response envelope.
- Terminal channel evidence retains the failed peer identity, status, close count, and wake count while clearing every transient pointer and byte count.
- The normal two-generation path stays intact, so containment does not weaken the existing reuse proof.
- `WVPROC15` and `WVCHAN04` remain internal experimental records, not compatibility promises.
- No general service discovery, separate endpoint object, multi-client concurrency, cancellation, timeout, restart, replacement, supervisor tree, scheduler, or VFS is implied.

## Reconsideration triggers

Reconsider this bounded contract when more than one waiter or service endpoint exists, when restart/replacement policy becomes real, when a scheduler can block and wake arbitrary threads, when transport failures need typed Windvale results wider than the syscall register, or when a Windvale-owned machine coordinator can replace the Stage 0 record transitions without weakening the independent checks.
