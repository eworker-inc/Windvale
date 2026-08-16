# Windvale OS application-start user-copy policy 1

## Status and scope

Application-start user-copy policy 1 is the implemented architecture-neutral
boundary between an untrusted caller's byte window and the checked
[`WVSR 1`](Windvale-Os-Application-Start-Request.md) decoder. It copies exactly
64 bytes into an immutable Windvale value before decoding and independently
compares the encoded caller reference with the caller identity supplied by the
future syscall adapter.

The architecture-specific x86-64 copy leaf is also implemented. It accepts one
already admitted page, snapshots and validates one exact request, and erases a
rejected copied request. It does not walk page tables, pin or stabilize the
mapping, recover from a processor fault, derive the current caller, assign a
syscall number, or publish a user-callable start ABI. Those mechanisms remain
the responsibility of the syscall adapter.

## Copy and admission contract

[`Application-Start-User-Copy.wv`](../Operating-System/Kernel/Application-Start-User-Copy.wv)
accepts a bounded user-memory image, one admitted window, one requested buffer,
and the current caller reference. It applies these checks before slicing:

1. the requested buffer length is exactly 64 bytes;
2. the admitted window begins within the supplied memory value;
3. the admitted window length fits using subtraction-first checked arithmetic;
4. the buffer does not begin before the admitted window; and
5. the complete buffer fits inside that window using subtraction-first checked
   arithmetic.

A successful copy returns a new immutable 64-byte value. A failed copy returns
an empty value and one of `Invalid_size`, `Invalid_window`, or `Invalid_range`.
Copy-and-admit maps every copy failure to a rejected zero-reference launch
transition, rejects an encoded caller that differs from the current caller as
`Invalid_caller`, and invokes the `WVSR 1` decoder only after both checks pass.

The caller comparison is separate from structural decoding: bytes controlled
by a process cannot establish that process's kernel identity.

## X64 native leaf

[`X64-Application-Start-User-Copy.wva`](../Operating-System/Kernel/X64-Application-Start-User-Copy.wva)
exports `Windvale_kernel_x64_application_start_copy` with this internal
register contract:

| Register | Meaning |
| --- | --- |
| `RCX`, `RDX` | start and exclusive end of one exact page-aligned current-process user window |
| `R8` | untrusted request address |
| `R9` | nonzero, eight-byte-aligned, kernel-owned 64-byte snapshot outside the user window |
| `R10D` | caller reference independently derived by the future syscall adapter |
| `R11D` | requested byte count, exactly 64 |
| `EAX` | `0` valid, `1` size, `2` window, `3` range, `4` caller, or `5` payload |

The leaf rejects a wrapped, unaligned, non-4,096-byte window before touching a
destination. After admitting the destination, it clears all eight qwords,
requires the complete source range to remain inside the page, copies exactly
eight qwords, validates every `WVSR 1` field, and compares the copied caller
with `R10D`. Caller and payload rejection clear all eight qwords again. The
loops are fixed at eight iterations; no input controls allocation, recursion,
diagnostic growth, or additional work.

This is an internal machine leaf, not the public start operation. Its caller
must prove that the page belongs to the current process, is readable and stable
for the bounded copy, and cannot fault across the privileged load sequence.

## Syscall-context adapter

[`X64-Application-Start-Syscall-Context.wva`](../Operating-System/Kernel/X64-Application-Start-Syscall-Context.wva)
is the next internal leaf. It accepts the machine-derived current-process
request-page start, untrusted request address, kernel snapshot, exact byte
count, process id, and process generation. Version 1 admits only init process 1
generation 1. The adapter rejects any other or unaligned context, derives
reference `65537` from the separate id and generation, constructs the exclusive
one-page end with checked addition, and invokes the copy leaf. Caller-controlled
request bytes never select that derived identity or page.

The adapter is intentionally independent of `GS` layout and syscall numbering.
The future retained-machine cutover must load these inputs from the current
thread/process record, prove that the selected RW/NX page is present and stable,
reserve a kernel-owned snapshot, enforce the syscall budget, and translate the
returned internal status. This keeps state-layout mechanics out of the reusable
copy contract while leaving no user-supplied identity boolean.

## Evidence and limits

The policy builds as an 8,313-byte WVB at SHA-256
`bd26f61a20867452a99f3db4d3c58988585656387ec93e424d965100a6134247`.
Its 11,695-byte self-test at SHA-256
`0ce6389c99c30c4875cb06da9bedb44d2f832d7b39898647f7a119ad82830f45`
passes nine boundary cases: exact copy, successful admission, caller
impersonation, wrong size, invalid window offset, oversized window, buffer
before or after the window, and malformed copied input.

The native leaf assembles to a 799-byte WVO at SHA-256
`74978b1f6124517b44205cba52aaf6c161cf5d00e39ff9ab3ad883d527c87ddb`.
Its ten-case self-test assembles to a 1,432-byte WVO, links with the leaf into
an exact 4,288-byte image at SHA-256
`19411b99859049d7453bd17c3d473e0141122213b39d9c9f4be5356c6b495cc1`,
returns 47 on the local host, and deterministically packages as both Windows
and Linux console images. The cases cover valid status, exact immutable copy,
wrong size, invalid envelope, cross-page source, zero and overlapping
destinations, caller impersonation, malformed magic, and a wrong page charge;
rejection erasure is checked directly.

This evidence still does not prove a safe public read from live user virtual
memory. The syscall adapter must pin or otherwise stabilize the source mapping,
derive the caller from the current thread/process record, convert page-walk and
access faults into a defined rejection, and ensure that no policy decoder
observes caller memory directly.

The syscall-context leaf adds a 344-byte WVO at SHA-256
`d639056eb9831f89ef3baa33b06b522437d2da4444f74e2db1d58229656dc04b`.
Its nine-case probe links both leaves into a 4,288-byte image at SHA-256
`3b5b95a0ceb544ca9beac65c3da9fb62ce4cce48dfb0a23e858a76827fd82b6f`,
returns 48, and has pinned Windows/Linux containers. It checks exact snapshot
success, byte identity, process and generation rejection, page alignment,
cross-page range, size, independently derived caller, payload rejection, and
rejection erasure.
