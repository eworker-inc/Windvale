# Decision 0921: cache complete segmented development packaging

## Status

Accepted and implemented locally on Windows on 2026-09-02. This decision
changes development verification routing and cache reuse. It does not remove
the uncached paired-host reconstruction required for release qualification.

## Context

The WVB-runner reconstruction owner reported only three cases but spent 19
minutes 43 seconds rebuilding and packaging overlapping compiler stages. Its
`--development-cache` path cached only final hosted construction after staging,
linking, and transport had already repeated. A first attempt to route through
the complete cache also exposed that the old public verifier could not admit a
1.55 MiB compiler module.

Fast feedback and exact reconstruction are different products. Ordinary edits
need the narrowest causal check, while release qualification must still prove a
fresh cross-host artifact family.

## Decision

1. Route `Package-Segmented-Compiler-Wvb --development-cache` through the
   existing complete segmented-hosted-WVB cache before staging begins.
2. Key a checkpoint by the complete WVB bytes, host, target, capability profile,
   verifier, native stager, linker, transport, hosted packager, and loaded cache
   implementation identities. Revalidate the checkpoint record and product
   bytes before every materialization.
3. Run complete bytecode verification before creating a missing checkpoint.
   Reuse its passing evidence on an exact key hit instead of verifying and
   rebuilding unchanged input again. An invalid uncached WVB still fails closed
   before native construction and publishes no executable.
4. Refresh the authenticated public compiler verifier to the WVB 1.37
   source-built artifact so compiler-scale cache creation crosses the current
   verification boundary.
5. Add `--development` to the runner reconstruction owner. Development mode
   rebuilds current source, requires exact WVB and current-host executable
   equality, and executes valid/rejection cases through cached current-host
   packaging. Default mode retains fresh paired Windows/Linux construction.
6. Route development-scope `Verify-Changed` runner work to that development
   mode. Qualification scope continues to use the default paired-host owner.
7. Stream the existing bounded 13-phase reconstruction progress in development
   mode so a cold cache remains observable.
8. Extend the existing segmented-hosted cache owner with an integration guard
   proving the compiler packager routes malformed development input through
   complete verification and leaves no output.

## Measured result

All measurements used the Windows x64 local host and implementation commit
`91eb4115e1c7edd719d2614cb3299391d137cc39`:

| Workload | Cold or prior | Warm or optimized | Result |
| --- | ---: | ---: | --- |
| 408 KiB hosted fixture | 127.5 s | 0.625 s | 204x faster, identical bytes |
| Runner development owner | 2,552.2 s one-time population | 9.3 s | 274x faster after population |
| Runner paired-host qualification | 1,183.0 s prior | 167.2 s | 7x faster, fresh host artifacts |

The 42-minute first population is not an acceptable edit loop; it is retained
as cold-path evidence and as a separate optimization target. It created five
large compiler/tool checkpoints plus the runner checkpoint. Exact repeated
development work now completes in seconds. The cold hosted source/container
producers remain slower than comparable mature toolchains and require separate
profiling rather than being hidden behind aggregate test counts.

## Consequences

- Unchanged compiler packaging no longer repeats native staging, linking,
  transport, bytecode verification, or hosted container generation.
- A changed WVB invalidates only its own profile/host checkpoint. Unrelated
  database or library changes do not justify compiler reconstruction.
- Developers get a seconds-scale repeated runner check while release work keeps
  the full paired-host deterministic oracle.
- Cache corruption, producer drift, publication races, timeouts, output bounds,
  and malformed input remain covered by the existing ten-case cache owner.
- Cold compiler packaging is still expensive. This decision removes redundant
  execution; it does not claim the remaining cold algorithms are fast.

## Reconsideration triggers

Reconsider the cache boundary if an undeclared producer can affect output, if a
checkpoint can be materialized without exact input/product validation, if a
semantic or malformed-input boundary is reused across different verifier
identities, or if development routing accidentally replaces release
qualification. Profile cold staging and hosted-source construction before any
algorithmic optimization.
