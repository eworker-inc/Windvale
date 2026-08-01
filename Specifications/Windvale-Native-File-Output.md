# Windvale native file-output boundary

## Purpose

This contract closes the first explicit native-admission blocker in the qualified Windvale compiler WVB. It gives native Windows and Linux execution one bounded whole-file publication operation without exposing paths, handles, descriptors, platform error numbers, directory operations, or a general foreign-function interface to Windvale source.

## Source-visible contract

The source-visible capability remains the existing hosted-resource contract:

```text
file.write_bytes(Resourceˉname: text, Value: bytes) -> void
```

A module must declare the capability and the launcher must grant it. Native execution also requires an explicit host-file-system output adapter. Declaration, authorization, and implementation remain separate checks.

## Native call contract

The compiler-generated call passes:

- the borrowed byte pointer in `RCX` and its unsigned length in `EDX`;
- the borrowed strict-UTF-8 resource-name pointer in `R8` and its unsigned length in `R9D`;
- the verified execution-context pointer in `R15`.

The leaf returns zero in `EAX` on success or one on a contained service failure. A failure writes one existing `Nativeˉserviceˉfailureˉdetail` file classification into the execution context. No native exception, signal, or managed exception crosses the leaf.

## Runtime-private table

Execution-context version 7 appends one file-output-table pointer. `WVFO` version 1 is an 80-byte runtime-private table containing:

- magic, format version, byte size, and platform identity;
- one bounded path scratch buffer and its byte capacity;
- on Windows, exact pointers for strict UTF-8 conversion, file creation/replacement, writing, durable flush, close, and last-error capture;
- on Linux, zero function pointers because the leaf uses direct `openat`, `write`, `fsync`, and `close` system calls.

The C# Stage 0 owner constructs and independently verifies the complete static table before publication and again after native return. The native leaf may modify the named host file but may not modify the table.

## Bounds and semantics

- The resource name must be non-empty strict UTF-8, contain no embedded NUL, and fit the existing 1 MiB text-value limit.
- The byte value may be empty and must not exceed the existing 4 MiB byte-value limit.
- The operation creates or replaces exactly one named file, does not create parent directories, and performs a durable file flush before reporting success.
- Replacement is not atomic. A failed operation may leave a created, truncated, or partially written file, matching the existing hosted-resource contract.
- Partial operating-system writes are completed in a bounded loop. Interrupted Linux writes and flushes are retried.
- Invalid name, missing parent, access denial, operational failure, and oversized input map to `WVR3021` through `WVR3025`. Capability denial remains `WVR3010`; missing host output configuration remains `WVR3001`.

## Verifier and publication requirements

The native backend admits only the exact catalog signature `(text, bytes) -> void`, emits the canonical register sequence and appended service-table load, and includes the new service in sorted unique fragment metadata. The independent fragment verifier reconstructs the name and byte descriptors, their initialized provenance, the exact call slot, and the absence of a result value.

The runtime reconstructs and verifies the exact platform leaf before W^X publication. Focused tests must cover exact reconstruction and corruption, authorization and implementation preflight, empty/ASCII/Unicode byte publication, replacement and truncation, the 4 MiB boundary, invalid names, missing parents, rejected writes, direct JIT, linked WVO/AOT, and the qualified compiler WVB's native-admission preflight.

Repeating native preflight for that exact compiler WVB is an acceptance condition for this slice. `file.write_bytes` must no longer be the reported blocker. Complete compilation or execution is evidence only if it actually succeeds; any next independently observed blocker must be recorded without broadening this contract.

## Deliberate limits

This contract adds no append, random access, open handle, seek, read/write stream, directory, rename, delete, permission, metadata, environment, network, asynchronous I/O, or general platform import. C# remains the reference/recovery implementation and still owns table construction, W^X publication, invocation, status mapping, and process containment.
