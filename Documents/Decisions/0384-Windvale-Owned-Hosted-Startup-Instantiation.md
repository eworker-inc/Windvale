# Decision 0384: Windvale-owned hosted-startup instantiation

- Status: Accepted current-host normal-path startup transfer; native target projection, outer-container construction, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0383](0383-Windvale-Owned-Hosted-Tool-Metadata-Construction.md), [Decision 0164](0164-First-Exact-Compiler-Linux-Executable-Container.md), [Decision 0167](0167-First-Exact-Compiler-Windows-Executable-Container.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-startup instantiation](../../Specifications/Windvale-Native-Hosted-Startup-Instantiation.md)
- Advanced by: [Decision 0385](0385-Windvale-Owned-Hosted-Container-Construction.md)

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
| Startup-instantiation WVB | 19,935 | `03b1324c25bfae312705a7de74919299aa417e7a27a5de5d465113f760ae359b` |
| Startup-instantiation WVNF | 185,819 | `c26980323050ccf8afc47ac1215d3203d4d4aa4b5621dc249529a7405570b6f8` |
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

Normal hosted startup bytes are constructed by Windvale from the canonical
WVOs. Decision 0385 now supplies their complete target lists from Windvale and
constructs the surrounding PE/ELF-owned bytes. C# retains only bounded native
dispatch, response verification, segment copying, and the independent oracle.

Broad Development, Standard, Qualification, Linux-host execution, and grouped
dual-host gates remain deferred under the active retirement goal.

## Reconsideration triggers

Version the request if the admitted section shape, address width, relocation
kind, object bounds, or hosted text address changes. Use the general WVO
verifier when its native closure becomes the smaller coherent dependency; do
not silently expand this startup-specific reader into a second general object
model.
