# Decision 0142: Immutable guest resource store and peer cleanup

- Date: 2026-08-03
- Status: Implemented candidate; fresh dual-host qualification pending
- Advances: Firmware Probe 34, WVA seam 10, `WVKMEM12`, `WVPROC13`, `WVCHAN03`, and `WVRES005`
- Retains: Canonical WVB 1.6, native ABI 21/context 7, paging 4, admission 4/bridge 2, retained bridge 10, `WVBR002`, interpreter profile 6, and the two-generation reclaim/rebuild proof

## Context

Decisions 0126, 0129, and 0135 established the deterministic `WVRS 1` image, bounded `WVRQ 1` / `WVRY 1` protocol, and a checked live guest exchange. Probe 33 still returned a fixed WVA-owned response. The init service did not own the store whose contents it purported to serve, and the channel did not retain explicit peer-death evidence after terminal cleanup.

Putting resource names or store parsing in the kernel would collapse service policy into transport. Mapping the store into every client would bypass the service boundary. The smallest honest next slice is one immutable init-owned store mapping, a format-blind kernel capability record, dynamic bounded lookup in the init guest, and explicit terminal peer cleanup.

## Decision

- Construct one canonical 1,195-byte three-entry `WVRS 1` image containing the admitted WVB, its four-byte execution budget, and `boot:main.configuration` bytes `[3,5,8,13]`. Bind the complete image to SHA-256 `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8`.
- Give init one additional user RX code page and one independently mapped RO/NX store page. Keep the store absent from both client roots.
- Publish a bounded store descriptor in init's RW/NX data page. Represent the mapping with a third kernel-owned resource record, identifier `4`, kind `WVRS store`, state `attached`, one live mapping, and no borrower.
- Advance resource records to `WVRES005`, process records to `WVPROC13`, memory to `WVKMEM12`, and the x86-64 WVA composition seam to version 10. The enlarged init extent grows the deterministic arena from 141 to 143 pages; the 120-page client extent and its same-root rebuild proof remain unchanged.
- Make the init WVA seam parse the request and the exact canonical three-entry `WVRS 1` profile from its descriptor, compare the requested opaque name against directory entries, and construct the `WVRY 1` success response in its RW/NX data page from the selected entry. Remove the fixed response data from init read-only code data.
- Validate checked header/directory/name/data extents, exact three-entry canonical layout, identifiers, kinds, attributes, reserved fields, strict name order, and digest-text grammar before publishing a result. Stage 0 independently binds the complete immutable store page to its SHA-256 identity before entry. The bounded guest seam does not yet recompute each entry's SHA-256 payload digest.
- Advance the channel to `WVCHAN03`. On terminal client exit or fault, clear retained scalar/message state, sender, receiver, waiter, byte length, request/reply destinations and capacities, then record the peer process, exit/fault status, and close count. Explicitly reopen a clean generation-1 channel before rebuilding generation 2; generation 2 ends terminally closed.
- Require both client generations to receive the dynamically constructed response, validate it completely, interpret the admitted WVB to result `6`, and complete the existing grant revocation and exact tail reuse proof.
- Advance firmware to Probe 34 and replace the serial marker with `ipc=dynamic-resource-store`.

## Local evidence

All 31 bounded OS tests pass on Windows. Deterministic pins cover the 1,929-byte init WVA object, 5,015-byte linked init image, immutable store image, process-machine objects, and all four firmware scenarios. Pinned QEMU 11.0/Q35/TCG completes normal, invalid-opcode, general-protection, and contained user-fault scenarios with exact Probe-34 serial evidence.

Fresh Debian execution and the complete dual-host qualification gate remain pending, so this decision does not replace the latest cross-host-qualified Probe-32 baseline.

## Consequences

The live guest now performs name-dependent resource selection from an independently lived immutable store without giving the kernel name, path, or store-format semantics. The store mapping is read-only and non-executable, absent from clients, machine-bound before construction, and retained across both client lifetimes. Terminal peer cleanup no longer leaves stale request or destination state that could cross a process generation.

This remains a resource-image service rather than a filesystem. It has no path component rules, directories, enumeration, handles, mounts, provider discovery, permissions beyond the fixed capability, replacement, writable state, block device, cache, persistence, or crash-consistency contract. The exact guest validator is intentionally limited to the measured canonical three-entry boot store; the portable hosted verifier remains the general `WVRS 1` oracle.

## Reconsider when

- A measured namespace needs path components, directory lookup, enumeration, or handles.
- Store bytes come from a block or package provider rather than the fixed boot image.
- Writable files require allocation, ordering, durability, recovery, or cache-coherence rules.
- More than one service or concurrent client needs generalized channel peer-close results and scheduling.
- The guest must accept arbitrary valid `WVRS 1` images or recompute payload digests instead of using this exact machine-bound boot profile.
