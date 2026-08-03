# Decision 0135: Bounded guest resource request/reply

- Date: 2026-08-03
- Status: Implemented candidate; fresh dual-host qualification pending
- Advances: Firmware Probe 33, `WVPROC12`, and `WVCHAN02`
- Retains: Canonical WVB 1.6, native ABI 21/context 7, `WVKMEM11`, paging 4, `WVRES004`, `WVBR002`, interpreter profile 6, and the two-generation reclaim/rebuild proof

## Context

Decisions 0126 and 0129 established the `WVRS 1` store and `WVRQ 1` / `WVRY 1` protocol in hosted execution. The live guest still carried only one scalar `u32` result on `WVCHAN01`; it could neither copy a bounded request into the init service nor return a checked reply to the interpreter client.

Adding a shared writable page would weaken isolation and require another physical page in an already exact arena. Teaching the kernel to parse resource names or `WVRS 1` would put filesystem policy in the wrong layer. The smallest honest guest slice is therefore synchronous checked copying between existing isolated pages, with format validation remaining in user space.

## Decision

- Advance the process contract to `WVPROC12` and the channel record to `WVCHAN02`.
- Add three syscalls: service receive-request `5`, client call `6`, and service reply `7`.
- Add independent receive-request, call, and reply capability rights. Init has rights `46`; each client generation has rights `17`.
- Accept only nonempty messages of at most 4,096 bytes. Request and reply sources must remain wholly inside the caller's RX image. Destinations must remain wholly inside the registered caller RW/NX data page.
- Copy directly through the kernel's retained supervisor identity map after checked range arithmetic. Do not add a shared user mapping, kernel IPC page, name parser, store parser, unbounded queue, or compatibility branch.
- Keep the process data/context address immutable across syscalls. Resume `RDX` from the process record rather than trusting a caller-clobbered register.
- Use the init data window at offset 1,024 with capacity 1,056 for requests. Use the client data window at offset 2,048 with capacity 2,048 for replies; the lower 2 KiB remains owned by context and runtime records.
- Carry the exact 55-byte lookup request for `boot:main.configuration`. The init WVA seam validates it and returns the exact canonical 116-byte success reply for resource `(3, opaque-bytes, attributes 7)` and bytes `[3,5,8,13]`.
- Require the client WVA seam to validate response extent, every header field, all 64 digest bytes, and the four data bytes before starting the existing interpreter.
- Repeat the request/reply independently in both client generations. After each reply, retain the existing scalar result send, terminal cleanup, exact tail release, zeroing, and same-root rebuild proof.
- Increase init and client syscall budgets to `7` and `3`. The larger linked client still fits ABI 21's 109 RX pages, so its 120-page reclaimable extent and the exact 141-page `WVKMEM11` arena remain unchanged.
- Advance firmware to Probe 33 and replace the serial IPC marker with `ipc=resource-request-reply`.

## Local evidence

The bounded OS suite passes all 31 tests on Windows. Deterministic pins cover the WVA objects, linked user images, process machine, paging composition, and all four firmware scenarios. Pinned QEMU 11.0/Q35/TCG executes the exact guest request/reply in both client generations before normal completion or the selected terminal fault path.

Fresh Debian execution and the complete dual-host qualification gate remain pending, so this decision does not replace the latest cross-host-qualified Probe-32 baseline.

## Consequences

The live guest now proves one checked one-page user-space resource protocol exchange without making the kernel understand resource names. User pages remain isolated, the transport has explicit directional rights, and no new physical IPC page is required.

This is not yet a filesystem or a live `WVRS 1` service. The guest init seam returns one fixed canonical configuration response; it does not receive an immutable store capability, validate a store in the guest, perform dynamic lookup, propagate arbitrary service failures, or expose paths, directories, handles, enumeration, mutation, packages, block storage, or devices.

## Reconsider when

- The init service receives an independently lived immutable `WVRS 1` capability and can validate and query it in the guest.
- Service/client death needs a general peer-closed result rather than the current fixed coordinator failure.
- More than one request, more than one in-flight call, or buffers spanning pages are required.
- A general scheduler and capability table can replace the fixed two-process coordinator without weakening the checked-copy rules.
