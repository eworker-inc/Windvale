# Decision 0127: Windvale-owned console-application layout

- Date: 2026-08-02
- Status: Implemented candidate; fresh dual-host qualification pending
- Targets: `windows-x64-console-v1` and `linux-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, WVA 1, WVO 1.0, both executable format versions, and the .NET retirement gate

## Context

Decision 0124 moved both exact process-entry templates into WVA, but Stage 0 C# still owned every PE and ELF layout calculation. Moving construction and verification in one step would combine three independently useful boundaries and make byte or rejection disagreements difficult to localize.

The first portable container slice therefore needs to be smaller: one bounded input, one deterministic serialized plan, and an independent host oracle that checks every field before existing writers can consume it. That establishes live Windvale ownership without weakening the C# recovery path.

## Decision

- Add a portable Windvale planner for both version-1 console targets and a minimal hosted bridge that reads one immutable 32-byte request and returns one 108-byte response.
- Version that internal exchange with `WVCQ` request and `WVCP` response identities, explicit total sizes, fixed little-endian `u32` fields, zero reserved words, stable status values, and exact failure offsets.
- Limit native images to the existing 4 MiB boundary and retain the accepted 4,196,352-byte PE and 4,202,608-byte ELF file ceilings.
- Make the portable module own request validation, alignment, file extents, virtual extents, startup/native placement, entry addresses, initialized data placement, metadata placement, and complete image size.
- Embed the exact hosted bridge WVB in the Stage 0 linker. Check its retained size and digest, authorize only `file.read_bytes`, and execute it under a two-million-instruction limit.
- Independently recompute every response field in C# with checked arithmetic. Reject every envelope, status, reserved-field, or value disagreement before construction.
- Make both existing writers consume only a verified plan for allocation and placement. Retain their separately implemented byte emission and their independent untrusted-container verifiers.
- Keep one planner-specific differential test. Compile the portable source and bridge, check the retained WVB identity, exercise both platforms and maximum inputs, cover request rejection families, prove deterministic responses, and mutate every serialized response word. Do not repeat the existing executable corpus.

The exact retained bridge is 8,806 bytes with SHA-256 `a4421adf6e46f31a5096099b1b164ea93901e97a66ff86b9b5b80ba5e753e790`. The portable core compiles to 8,957 bytes with SHA-256 `7fe718d644e426b9a90e3bd1dcc51c4e1bb1ac4af439cd4bbcda2cf7d01f276a` in the same source-set proof.

## Local evidence

The focused planner test passes with a zero-warning Release build. It proves exact plans for representative Windows and Linux inputs, both 4 MiB native-image boundaries, deterministic repeated evaluation, truncated and extended requests, every reachable request rejection family, and rejection after changing each four-byte response field.

The existing Windows and Linux console tests pass after the live writers begin consuming the Windvale plan. Their exact canonical outputs remain unchanged: the PE is 5,120 bytes with SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`, and the ELF is 8,304 bytes with SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.

Windows Development passes a zero-warning Release build, all 76 regular Seed tests, and all 28 bounded OS tests in 91.631 seconds wall time. Seed takes 74.940 seconds; the planner and existing Windows/Linux container cases take 64, 838, and 23 milliseconds. The qualification-only golden contract and direct Linux execution are not part of Development, so fresh dual-host Qualification remains pending rather than being inferred from this host.

## Consequences

The normal Stage 0 PE and ELF writers now depend on a digest-pinned Windvale artifact for every live allocation and placement decision. C# remains an independent fail-closed oracle, so a compromised, stale, or divergent planner cannot silently select container offsets.

The runtime dependency used to evaluate the retained planner is still a Stage 0 bootstrap dependency. The hosted bridge is an input adapter, not portable semantics, and its single file capability is explicit.

This decision does not move PE/ELF byte construction, untrusted-container verification, executable publication, native record storage, compiler reproduction, or hosted console services into Windvale. Those remain separate gates in the active goal.

## Reconsider when

- Portable Windvale construction can consume the same plan and reproduce both complete executable byte streams.
- A native Windvale runtime evaluates the planner without the Stage 0 reference runtime.
- A new container target needs fields that cannot be represented as a compatible new plan version.
- Measured planner cost requires a bounded arithmetic primitive that preserves the same serialized result.
