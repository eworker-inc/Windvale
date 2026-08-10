# Decision 0497: Native WVB-to-WVO reconstruction

- Status: Accepted
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the exact accepted-subset WVB-to-WVO candidate
- Extends: Decisions 0304, 0420, 0422, and 0496

## Context

The Windvale-written accepted-subset x86-64 lowerer already had native Windows
and Linux applications, fixed behavioral vectors, digest-bound launchers, and
earlier paired reconstruction evidence. Continued source work changed the
current candidate, however, so the active manifest and launchers described a
Stage 0-constructed generation rather than the live source closure. Earlier
reconstruction evidence remains valuable historical evidence but cannot prove
the identity or construction provenance of a newer candidate.

The retained segmented native compiler toolset can build the current lowerer
WVB, stage and link its native image, transport the canonical fragments, and
package both hosted targets. This can remove the managed application writer
from construction of the exact current candidate without pretending that the
retained native seed is a clean bootstrap.

## Decision

Windvale adopts a five-artifact reconstruction contract for the current
accepted-subset lowerer:

1. build `Windvale-Native-X64-Lowering-Tool.wvproj` and the fixed return-42
   project through the native source front door;
2. use the retained segmented native staging, image-linking, transport, and
   hosted-packaging applications to construct the lowerer image and both target
   containers;
3. execute the constructed Windows lowerer on the fixed WVB and require the
   unchanged canonical WVO;
4. require a caller-supplied existing output directory distinct from the
   committed candidate, and verify all five exact outputs before accepting the
   reconstruction; and
5. record candidate manifest format 2 with provenance Decision 0423,
   construction Decision 0497, and application construction
   `native-segmented-self-reconstruction`.

The exact identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-To-Wvo.wvb` | 412,871 | `01781356ae2a6cf10e14d178878102609fcfbe3b9340f71b723ac5caf54451f7` |
| `Wvb-To-Wvo.exe` | 5,958,144 | `927cbdf8b89269538ea2af1131276e4edca3e8810c1edaa3c7fd096e3528a267` |
| `Wvb-To-Wvo.elf` | 5,959,680 | `21a7c239d5236227da1abe202807170c077dad629e858f46cde4225f8efa2d3b` |
| `Return-42.wvb` | 174 | `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31` |
| `Return-42.wvo` | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The fixed vector is intentionally unchanged. This decision changes current
candidate identity and construction provenance, not WVB 1.11, WVO 1.0,
ABI 22, lowering semantics, diagnostics, or accepted instruction shapes.

## Evidence

The focused current-Windows-host retirement lane passed all three cases in
124.2 seconds: exact five-file candidate inventory, byte-identical native
reconstruction of the current WVB and paired applications, and current-host
reproduction of the unchanged return-42 WVO through the constructed lowerer.
No broader verification or independent Linux-host execution was run for this
slice.

## Consequences

- Stage 0 is no longer the only constructor for the exact current paired
  accepted-subset lowerer applications.
- The normal candidate launchers can bind the current Windows and Linux
  application identities without accepting an unmeasured descendant.
- The reconstruction consumes the retained segmented compiler and packaging
  toolset. It therefore does not prove a non-circular clean bootstrap or remove
  the need for a previous trusted seed.
- The constructed Linux application is an exact cross-target artifact from the
  current Windows host. Independent Linux reconstruction and execution remain
  open.
- C# remains the complete backend for unsupported modules and the explicit
  recovery and differential oracle.
- Stage 2 convergence, complete-backend closure, extended fault and concurrency
  evidence, promotion, grouped Windows/Linux qualification, and the final
  Decision 0057 retirement gate remain separate work.

## Reconsider when

Reconsider this decision if the retained segmented seed changes, either target
container format changes, the accepted-subset lowerer gains new semantics, the
fixed vector changes, or independent Linux evidence produces a different byte
identity.
