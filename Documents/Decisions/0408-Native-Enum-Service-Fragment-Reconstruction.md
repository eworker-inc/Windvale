# Decision 0408: Native enum-service fragment reconstruction

- Status: Implemented candidate; ordered resource orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0407](0407-Native-Hosted-Enum-Service-Production.md), [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted enum processes](../../Specifications/Windvale-Native-Hosted-Enum-Processes.md)
- Advanced by: [Decision 0409](0409-Native-Fixed-Service-Acquisition.md)

## Context and decision

Decision 0407 transferred variable enum-service byte construction into two
Windvale processes, but Stage 0 still produced the enum-service application's
native fragment. The portable Windvale WVB-to-WVO logic accepted that WVB when
run through the reference runtime, while the pinned native lowerer process
exited before publication. Inspection isolated the pressure to two generated
functions in the shared enum-metadata core: request validation generated
54,804 native bytes and response construction generated 28,524 native bytes.

Keep all native arena and format limits unchanged. Refactor the core along
existing semantic boundaries instead:

- isolate prior-member, per-type, and complete-directory validation with one
  explicit validation cursor;
- isolate response, metadata-header, directory, and member construction; and
- retain the complete envelope, canonical-name, duplicate-value/rank, lexical-
  order, size, and output-byte rules.

The resulting source remains one reviewable 416-line owned module rather than
numbered fragments. The same digest-bound native WVB-to-WVO launcher now lowers
the source-built enum-service WVB exactly. The digest-bound native linker then
links that WVO at base address zero and exported `Main`, producing the exact raw
fragment expected by the hosted-container pipeline. C# supplies independent
differential and recovery evidence only; it is not used by this candidate
production chain.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Enum-metadata core WVB | 15,507 | `e1a17bc17e8672bc2a6a7aba129875d6ef02569b77b1285e040da718d6be5501` |
| Enum-metadata bridge WVB | 15,385 | `5292abe7ab6f1bc31a15e49a0e182ded9abe0c0619cd408e44330bcc3e07cca2` |
| Enum-metadata retained fragment | 138,550 | `f529acc6dbf5e9dca9cb3a1c56d0fb2198104b5da249c7363bbf38cc2b38e806` |
| Enum-service WVB | 18,976 | `493226f5b61894cb43e3428555e96293310c03571f6cff905eb50fabc7721676` |
| Enum-service WVO | 168,342 | `0ded580f703ae2d982740fe673d1e04dee581cab8785bb5d0ba8894800cb2963` |
| Enum-service raw fragment | 167,274 | `cec5c423e32a3c0bc5602551e2b1da2e82929b2edd84b2756c4062bf0f223870` |
| Windows enum-service producer | 185,344 | `61d8b79ea57082c2ea85de5057a66e7c10045c44a9b8997d2ed491f3a1d90a83` |
| Linux enum-service producer | 184,320 | `cd6f3b01df9a57bfe1acf2fa226c58f10c8ba51d2096a75572628cfbea427cf0` |

The reviewed focused enum-process case now additionally requires exact native
WVO equality and exact native-linked fragment equality. It passes 1/1 in 9.885
seconds after a zero-warning 27.82-second build. This is the only local verifier
for the slice. Linux execution, candidate promotion, fixed-service acquisition,
ordered manifest/process lifecycle, and the final grouped dual-host retirement
gate remain pending.

## Reconsideration triggers

Revisit the helper boundaries only when a real accepted module again approaches
the measured native dynamic-value lifetime. Do not increase the arena merely to
hide one oversized generated function. Preserve the WVO as the structured
handoff and the base-zero link as the explicit raw-fragment projection.
