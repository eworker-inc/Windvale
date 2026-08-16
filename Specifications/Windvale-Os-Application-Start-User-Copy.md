# Windvale OS application-start user-copy policy 1

## Status and scope

Application-start user-copy policy 1 is the implemented architecture-neutral
boundary between an untrusted caller's byte window and the checked
[`WVSR 1`](Windvale-Os-Application-Start-Request.md) decoder. It copies exactly
64 bytes into an immutable Windvale value before decoding and independently
compares the encoded caller reference with the caller identity supplied by the
future syscall adapter.

This policy does not walk x86-64 page tables, recover from a processor fault,
assign a syscall number or registers, or publish a user-callable start ABI.
Those mechanisms remain the responsibility of the architecture-specific
kernel adapter.

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

## Evidence and limits

The policy builds as an 8,313-byte WVB at SHA-256
`bd26f61a20867452a99f3db4d3c58988585656387ec93e424d965100a6134247`.
Its 11,695-byte self-test at SHA-256
`0ce6389c99c30c4875cb06da9bedb44d2f832d7b39898647f7a119ad82830f45`
passes nine boundary cases: exact copy, successful admission, caller
impersonation, wrong size, invalid window offset, oversized window, buffer
before or after the window, and malformed copied input.

This evidence does not prove safe reads from live user virtual memory. The
x86-64 adapter must still pin or otherwise stabilize the source mapping, derive
the caller from the current thread/process record, convert page-walk and access
faults into a defined rejection, and ensure that no decoder observes caller
memory directly.
