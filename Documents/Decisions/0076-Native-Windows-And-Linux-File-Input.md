# Decision 0076: Native Windows and Linux file input

- Date: 2026-08-01
- Status: Candidate; Windows development evidence complete, cross-host qualification pending
- Extends: [Decision 0074](0074-Native-Windows-And-Linux-Output-Services.md)'s runtime-private host-I/O pattern
- Advances: Native ABI 14, execution-context version 6, kernel native bridge 9, and firmware probe 16
- Retains: Service-table version 4, WVB 1.6, WVO 1.0, and all generated service-call shapes

## Context

Decision 0074 leaves `file.read_bytes` as the only native runtime service that enters a managed callback. Windvale programs already pass a compiler-verified borrowed-text descriptor and a verified output cell. The remaining work is therefore a bounded host-I/O leaf and execution-owned resource state, not a language or generated-call change.

The portable contract must not expose Win32 handles, Linux descriptors, host error numbers, current process internals, or native pointers. Source still observes only a named immutable byte snapshot or a stable `WVR302x` failure.

## Decision

- Advance the shared target to `x86-64-wvb-baseline-v14` and append one file-input-table pointer to the 104-byte execution-context version 6.
- Define runtime-private `WVFI` table version 1 as 136 bytes. It carries the platform identity, a 64-record snapshot table, bounded name/data arenas, one path scratch buffer, and seven Windows function pointers. Linux requires all Windows pointers to be zero.
- Replace the managed file delegate and both platform adapter thunks with exact runtime-native x86-64 leaves. The generated fragment and version-4 service table remain unchanged.
- Use a 1,218-byte Windows leaf with SHA-256 `3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8`. It validates the name, converts strict UTF-8 with `MultiByteToWideChar`, and calls verified `CreateFileW`, `GetFileSizeEx`, `ReadFile`, `CloseHandle`, and `VirtualAlloc` pointers.
- Use a 991-byte Linux leaf with SHA-256 `15407274e8a0894f443ea77547175225c1a4327e7642c46903890e16358c8547`. It validates and copies the path, then issues direct `openat`, `read`, and `close` syscalls, retrying interrupted reads.
- Preserve exact ordinal first-success snapshots. A repeated name returns the original descriptor without reopening the file. Failed requests publish no record. A run may publish at most 64 distinct names, each name is at most 1 MiB of strict UTF-8, and each result is at most 4 MiB.
- Add service-failure details 5 through 10 for invalid name, not found, permission denied, unavailable, too large, and snapshot limit. The executor maps them to existing stable codes `WVR3021`, `WVR3022`, `WVR3023`, `WVR3024`, `WVR3025`, and `WVR3028`.
- Require explicit `Nativeˉfileˉinput.Hostˉfileˉsystem()` configuration in addition to capability authorization. A Stage 0 `IHostedˉfileˉreader` remains available to the reference interpreter but is neither accepted nor called by native file execution.
- Advance the service-free OS bridge to version 9 and firmware probe to version 16. The bridge constructs the complete context with a zero file-input-table pointer because the guest module requires no hosted service.

## Runtime-private state

Each run reserves 64 canonical snapshot records. Record `i` names canonical name slot `i * 1 MiB` and canonical data slot `i * 4 MiB`; the runtime independently verifies every published pointer, length, reserved field, strict-UTF-8 name, and ordinal uniqueness after native return. Windows reserves the arenas and commits the selected slots from the native leaf. Linux uses private anonymous mappings whose pages materialize on access. All state is released after `Main` returns, so no borrow escapes its execution.

The table also contains one platform path scratch area: up to 1 MiB plus NUL on Linux, or the corresponding UTF-16 capacity on Windows. Embedded NUL, empty, oversized, malformed Windows UTF-8, and invalid host paths fail before a snapshot is published.

## Safety and migration boundary

The fragment verifier proves that the incoming name and destination descriptor use compiler-generated, bounded provenance. The runtime reconstructs each platform leaf, verifies its exact size and digest, validates the complete table before W^X publication, and validates all published records afterward. Native code never calls a C# delegate and native failures never unwind through managed frames.

This completes native execution for all eleven service-table slots, but it does not complete .NET retirement. C# Stage 0 still selects and reconstructs leaves, creates and verifies tables and arenas, allocates W^X memory, applies relocations, invokes `Main`, maps traps, and packages the OS image. It also remains the independent reference/recovery compiler and interpreter. The next ownership step should move construction of one bounded runtime artifact into Windvale while retaining byte-for-byte comparison with this implementation.

## Candidate evidence

The existing hosted-input test now reconstructs and corrupts both platform leaf identities; runs the WVB inspector through direct JIT and linked WVO/AOT against a real OS file; proves its supplied Stage 0 reader remains at zero calls; verifies first-success cache reuse; and covers invalid, missing, oversized, and 65th-name failures. The complete 1,441-line Windvale `wvdump` also runs through JIT and linked WVO/AOT using the direct host-file boundary.

The focused Windows hosted-input case passes in 0.378 seconds in Debug and 0.134 seconds in the warm Release development run. Windows Development passes all 56 regular tests in 51.813 seconds. The zero-warning Windows Standard build passes all 57 tests in 275.825 suite seconds (280.2 seconds wall time); its cold hosted-input case takes 1.263 seconds. All 15 OS tests pass. Bridge 9 has 138 code bytes and produces a 350-byte object with SHA-256 `3cbf50a4828a1a69ca7441a667cb95e569055468c345ed26b8a580fda3facfc5`. Firmware probe 16 remains 15,872 bytes with candidate SHA-256 `206a036f8cbe3198544b6878bf52c80ef8d489c14d5437c6c7004ff1d6599504`.

Exact-commit Debian execution of the Linux leaf, cross-host Qualification, normalized reports, portable-artifact comparison, GitHub verification, and pinned-QEMU probe 16 remain required before this decision becomes qualified.
