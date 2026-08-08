# Decision 0384: Windvale-owned hosted-startup instantiation

- Status: Accepted current-host normal-path startup transfer; native target projection, outer-container construction, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0383](0383-Windvale-Owned-Hosted-Tool-Metadata-Construction.md), [Decision 0164](0164-First-Exact-Compiler-Linux-Executable-Container.md), [Decision 0167](0167-First-Exact-Compiler-Windows-Executable-Container.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-startup instantiation](../../Specifications/Windvale-Native-Hosted-Startup-Instantiation.md)

## Context

The hosted compiler-family PE and ELF builders still asked C# to decode two
large base64 machine-code templates and apply platform and runtime addresses.
The same startups already have canonical WVA sources and exact WVO identities.
Copying their 2,275 instruction bytes into another Windvale implementation
would preserve the code-twice problem that native retirement is intended to
remove.

The general portable WVO verifier is broader than the exact native
source-compiler closure needed by this constructor. The new consumer therefore
needs a strict, bounded startup-specific WVO profile rather than weakening the
front door or duplicating the general verifier.

## Decision

- Keep `Linker/Startup/*-X64-Hosted-Compiler.wva` as the single maintained
  source of startup machine code and retain their exact WVO outputs.
- Define strict `WVSI 1` and `WVSD 1` envelopes carrying one bounded WVO and
  one absolute target per canonical relocation.
- Let portable Windvale validate the complete one-section startup profile and
  apply every relative-i32 relocation, including the Windows-local commit
  helper relocation.
- Make both normal hosted compiler startup builders execute and independently
  verify the digest-bound service-free WVNF.
- Rename the former C# template constructors to `Buildˉstage0`; normal
  packaging no longer calls them.
- Keep the managed target-address projection and WVNF invocation only as a
  deletion-bound bridge. Native outer-container planning must resolve the same
  symbols before final managed removal.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Startup-instantiation WVB | 20,078 | `4cd40719ecbfe8f42f5ded4b0b2ba4df4e48a8463f4ea236c7c0831d22a3eb52` |
| Startup-instantiation WVNF | 185,841 | `b499e5f6ec3fb09c4efc33aa364533c6c6b0daa680fd3847b0054e2c7f346311` |
| Windows hosted startup WVO | 4,334 | `55f4782e976038c2d68bb91aeabb75518103524e9d5caaf1cc9f0662ab5a0feb` |
| Linux hosted startup WVO | 2,390 | `0df0525b35bbeb63492929d974326f328c247ce9313111ee6a8c1e321a2c22ff` |

## Evidence and consequences

The reviewed focused owner test rebuilds the exact WVB through Stage 0 and the
native project front door, reproduces the retained WVNF, assembles both WVA
sources to the retained WVO bytes, and compares interpreter and native
execution. Both normal startup builders match their frozen Stage 0 outputs,
and the complete small-fixture PE and ELF applications pass their independent
verifiers. Fourteen malformed request/object/relocation cases agree across the
interpreter and native executor. The Release test application builds with zero
warnings and errors, and the focused case passes 1/1 in about four seconds.

Normal hosted startup bytes are now constructed by Windvale from the canonical
WVOs. C# still projects final target addresses, invokes and verifies the native
fragment, constructs the surrounding PE/ELF, and retains the independent
template oracle. The next slice should transfer target resolution and the
complete outer-container plan rather than create another startup encoding.

Broad Development, Standard, Qualification, Linux-host execution, and grouped
dual-host gates remain deferred under the active retirement goal.

## Reconsideration triggers

Version the request if the admitted section shape, address width, relocation
kind, object bounds, or hosted text address changes. Use the general WVO
verifier when its native closure becomes the smaller coherent dependency; do
not silently expand this startup-specific reader into a second general object
model.
