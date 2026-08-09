# Windvale native baseline-JIT publication

Status: implemented candidate

## Purpose

This candidate composes the closed [`WVJP 1`](Windvale-Baseline-Jit-Patch-Plan.md)
patch-plan profile with the exact [`WVLT 1`](Windvale-Native-Publication-Lifetime.md)
write-then-execute lifetime. It is the first standalone Windvale-native path
that admits a typed patch plan, materializes machine bytes into writable and
non-executable memory, removes write permission, invokes the verified entry,
and releases the mapping without loading .NET.

The profile remains deliberately small. A Windvale bridge submits two exact
canonical WVB modules to the real `WVJP 1` producer and returns their two
54-byte x86-64 plans as one 108-byte bundle. The plans materialize the six
bytes `B8 <i32> C3` and return `42` and `-1`. Publication reserves one
4,096-byte page, copies only the six admitted bytes, invokes the page once,
and releases the complete page.

## Shared admission

`Compiler/Windvale/Baseline-Jit-Patch-Plan-Bridge.wv` owns the canonical WVB
inputs and calls `Compiler/Windvale/Baseline-Jit-Patch-Plan-Core.wv` for both
valid plans and one invalid-WVB check. The shared WVA bridge runs that code in
a bounded 16 MiB RW/NX arena, validates the descriptor result and arena range,
copies exactly 108 bytes into private host-owned storage, and releases the
producer arena before any JIT allocation. No valid plan is embedded in WVA.

`Runtime/Native/Baseline-Jit-Patch-Plan-X64.wva` then independently checks
every fixed `WVJP 1` field before an executable-memory allocation is reachable.
The four immediate bytes may vary; all other header, record, and template
bytes must match version 1 exactly.

The same component binds the publisher to the exact 140-byte `WVLT 1` response
for an image extent of six bytes. All nine state/action/next-state records are
compared byte for byte. The host-specific owners expose no operation that can
create an executable-to-writable transition, copy twice, seal twice, invoke
twice, or retain the allocation after completion.

The raw mapping address stays inside the platform owner. It is not stored in a
patch plan, returned to portable Windvale code, or exposed through a general
foreign-function interface.

## Platform adapters

The Windows x64 owner imports exactly these `KERNEL32.dll` functions:

- `VirtualAlloc(NULL, 4096, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE)`
- `VirtualProtect(base, 4096, PAGE_EXECUTE_READ, &old_protect)`
- `FlushInstructionCache(current_process, base, 6)`
- `VirtualFree(base, 0, MEM_RELEASE)`

The candidate PE starts from the verified `windows-x64-console-v1` container.
The recovery constructor validates its complete pinned geometry, link-map
symbols, import lookup table, import-address table, descriptor, and names, then
sets only the PE import and IAT directory entries. The resulting named
candidate is not claimed as an ordinary version-1 console application.

The Linux x64 owner uses direct system calls:

- `mmap(NULL, 4096, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0)`
- `mprotect(base, 4096, PROT_READ | PROT_EXEC)`
- `munmap(base, 4096)`

Neither route requests a writable-and-executable mapping. Generated code is
not invoked until the permission transition succeeds; every post-allocation
failure route attempts release.

## Evidence cases

Each application performs these checks in a single native process:

1. reject a corrupted `WVLT 1` plan;
2. reject a corrupted `WVJP 1` plan before allocation;
3. publish, invoke, compare, and release a plan returning `42`;
4. publish, invoke, compare, and release a plan returning `-1`;
5. force a seal failure and verify that the still-writable mapping is released.

Windows forces failure with a zero-length `VirtualProtect`. Linux uses an
unaligned `mprotect` address. Success is process result `0` with no diagnostic
output. The Windows artifact has passed locally. The Linux artifact is
deterministically reconstructed but requires execution on Linux before this
candidate has paired-host evidence.

## Ownership and non-claims

- The shared WVA component owns exact plan admission, materialization inputs,
  and the fixed lifetime-plan identity.
- Each platform WVA component alone owns executable-memory authority, the raw
  address, invocation, and teardown.
- PowerShell is used only by the recovery constructor that adds the Windows PE
  import-directory bindings. Normal tests use the native `.cmd` or `.sh`
  route and the digest-bound candidate artifacts.
- The bridge WVB is reconstructed through the native source front door and
  compared with the retained artifact. Its WVO remains a verified Stage 0
  recovery artifact because the accepted-subset native lowerer does not yet
  admit descriptor-returning `Main() -> bytes`; normal build and execution do
  not load .NET. Native reconstruction of that WVO is the next explicit N1
  blocker.
- This candidate does not implement general WVB admission, general machine
  lowering, calls, control flow, runtime services, code-cache accounting,
  concurrent publication, or Windvale OS publication.
- This candidate is progress toward N2 and does not by itself qualify the
  complete baseline JIT or satisfy the .NET retirement gate.
