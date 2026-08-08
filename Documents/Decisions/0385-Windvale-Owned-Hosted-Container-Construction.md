# Decision 0385: Windvale-owned hosted-container construction

- Status: Accepted current-host normal-path construction transfer; native publication, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0384](0384-Windvale-Owned-Hosted-Startup-Instantiation.md), [Decision 0164](0164-First-Exact-Compiler-Linux-Executable-Container.md), [Decision 0167](0167-First-Exact-Compiler-Windows-Executable-Container.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-container construction](../../Specifications/Windvale-Native-Hosted-Container-Construction.md)

## Context

After Decision 0384, C# still calculated the complete hosted PE/ELF layout,
projected every startup relocation target, constructed all outer headers and
Windows imports, allocated the final file, and copied each region. That left
the normal hosted compiler-family packaging path semantically owned by Stage 0
despite Windvale ownership of the bundle, metadata, runtime header, and startup
object instantiation.

The complete applications are about 27 MiB, above the ordinary 4 MiB Windvale
value limit. The current native source compiler also rejects one aggregate
source-binding closure containing the planner, startup-object parser, and both
platform byte constructors. A coherent replacement therefore needs bounded
composition rather than one oversized source or value.

## Decision

- Add one Windvale planner that validates the runtime-embedded metadata and
  derives every file offset, virtual address, size, and startup target.
- Add focused Windows and Linux Windvale constructors for their exact
  outer-container-owned bytes.
- Compose those fragments with the existing startup WVO constructor. Keep the
  large verified service bundle and final application outside Windvale values.
- Make normal C# builders invoke and independently verify those fragments, then
  perform only deletion-bound dispatch, allocation, and segment copying.
- Rename the former complete C# constructors to `Buildˉstage0`; use them only as
  differential/recovery oracles.
- Keep each source focused. The largest new Windvale file is the 247-line
  planner/target resolver; no numbered fragments or artificial line limit is
  introduced.

## Evidence and consequences

The focused owner case compiles and pins all three WVB/WVNF pairs, reconstructs
all three WVBs through the native project front door, compares interpreter and
native responses, exercises every hosted profile on both targets, compares all
twelve complete applications byte-for-byte with the frozen C# oracle, runs the
independent PE/ELF verifiers, and covers eleven malformed planner, eight
platform-constructor envelopes, and four malformed managed-relay responses.
It passes 1/1 in about six seconds. The affected startup case also passes 1/1.
The Release test application builds with zero warnings and errors.

Normal hosted layout, target projection, PE/ELF headers, imports, relocation,
and segment positions are now Windvale-owned. The managed relay no longer
invokes the former C# layout planners on the normal path; it bounds and checks
the returned envelope and non-overlapping regions without recomputing their
semantics. C# still dispatches the native fragments, materializes the large
result, and performs publication. Those remaining operations have a named
native-publication destination and are not permanent product architecture.

Broad Development, Standard, Qualification, Linux-host execution, and grouped
dual-host gates remain deferred under the active retirement goal.

## Reconsideration triggers

Version the envelopes if a hosted profile, runtime layout, service placement,
startup WVO shape, PE/ELF format, or payload extent changes. Recombine the
fragments only after the native compiler admits the complete source-binding
closure without weakening its limits; do not duplicate platform semantics to
make one artifact appear self-contained.
