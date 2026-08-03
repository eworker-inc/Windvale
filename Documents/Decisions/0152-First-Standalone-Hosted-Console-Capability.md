# Decision 0152: First standalone hosted console capability

- Date: 2026-08-03
- Status: Implemented and cross-host qualified
- Adds: `windows-x64-console-v2`, `linux-x64-console-v2`, and serialized `WVHC 1` metadata
- Retains: Native ABI 22, execution-context version 7, service-table version 5, WVB 1.6, WVO 1.0, and exact version-1 container bytes

## Context

The paired version-1 PE and ELF targets prove deterministic standalone scalar execution, but deliberately reject every runtime service. The qualified native backend and host executor already share exact Windows and Linux `console.write_line` leaves. Packaging those leaves without declaring their authority would make host access ambient and unverifiable; changing version 1 would also invalidate its exact cross-host evidence.

The next narrow Phase-10 gate is therefore one new versioned container per host, one service, one capability, and one independently checked serialized manifest. The compiler-sized WVO/link limit remains separate: combining a new authority model with a multi-megabyte admission change would obscure which boundary failed.

## Decision

- Preserve `windows-x64-console-v1` and `linux-x64-console-v1` unchanged. Add explicit version-2 target names.
- Admit only scalar ABI-22 fragments whose ordered required-service set is exactly `console.write_line`.
- Serialize one 192-byte `WVHC 1` record in the file-backed runtime header. Bind target, ABI/context/table/container versions, one service, one capability, table and arena offsets, adapter identity, native image/entry, exact leaf and image sizes, and both SHA-256 digests.
- Use the existing canonical native output leaves. Do not create target-specific compiler lowering or a general FFI.
- Express both startups in WVA. Install the execution context, service table, record/text arenas, and output table before calling unchanged native code.
- On Windows, import exactly `GetStdHandle` and `WriteFile` from `KERNEL32.dll`, then populate the output table at startup. On Linux, retain direct syscalls and initialize stdout as file descriptor 1.
- Reserve the full ABI-22 64 MiB dynamic text arena in both virtual images now. This avoids another container version when a later admitted application needs the qualified compiler arena.
- Keep the current 4 MiB WVO/link native-image ceiling in this slice. Native compiler packaging follows only after a deliberate object/link/container limit decision.
- Require the C# container verifier to parse untrusted PE/ELF structure and independently validate `WVHC 1`, startup relocation targets, exact output leaf, runtime tables, imports/syscalls, padding, digests, recovered native bytes, entry, and service set before publication.
- Add one paired test rather than cloning the version-1 suites. It constructs both containers twice, checks deterministic identity and cross-target native agreement, reconstructs both WVA startups, mutates metadata/leaf/tables/imports and file extents, and directly executes the current host artifact.

## Evidence

The focused paired test passes after a zero-warning Release solution build. Windows Development verification passes all 85 regular Seed tests in 161.191 suite seconds and the then-current 31 bounded OS tests after integrating the concurrent directory-capability change. A second OS-only upstream integration retains the zero-warning build and focused hosted-console result and passes all 34 resulting OS tests. On Windows, the public CLI compiles `Examples/Seed/Hello-Windvale.wv` to a 3,584-byte PE with SHA-256 `abb38c9de40d75eb8f4029aeb0d7f6dbba7b1ec776fefc8bc96aa376bd310f61`; the executable writes `Hello from Windvale`, then exits 0 without loading .NET. The same invocation produces a 9,216-byte Linux ELF with SHA-256 `5103d02b883a40757281ccce027445cab59c2330833efeb60ec57d1a39363b78`, and the Windows-side independent ELF verifier accepts it.

Exact implementation commit `ed4a0b40b9f59b45f1aaedf9d147cac0330fd1cb` passes GitHub [Verify run 30802630910](https://github.com/eworker-inc/Windvale/actions/runs/30802630910). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 86 Seed tests including the golden compiler contract, all 34 OS tests, and the complete native CLI gate. The paired hosted-console case directly executes the current host's version-2 application, observes `Hello from Windvale` and result 0, and checks deterministic canonical PE/ELF identities on both hosts. Windows Seed takes 364.810 seconds with a 205.826-second golden contract; Linux Seed takes 332.904 seconds with a 188.554-second golden contract. The complete verifier jobs finish in 12m03s and 10m39s. This qualifies the version-2 metadata, service binding, independent container verification, exact artifacts, and direct Windows/Linux execution as one committed cross-host baseline.

## Consequences

Windvale now has a real standalone hosted capability path rather than only capability-free process results. The authority is visible in source/WVB, native service requirements, serialized container metadata, the service table, and the platform adapter. Windows and Linux use the same native application bytes and semantic service identity while retaining explicit host mechanisms.

The implementation still uses Stage 0 to build and verify the outer container. It does not package the exact compiler, retire .NET, add more services, or transfer version-2 PE/ELF construction into portable Windvale. The next Phase-10 slice can lift the measured 4 MiB WVO/link/container boundary and package the already-reproducing compiler without redesigning the runtime arena or console authority.

## Reconsider when

- A second hosted service cannot compose through ordered records and fixed service-table slots.
- A platform needs an output adapter that cannot preserve the shared output-table contract.
- Compiler packaging proves that the retained arena or section layout is insufficient.
- A Windvale-owned version-2 constructor/verifier can replace Stage 0 while retaining independent recovery evidence.
