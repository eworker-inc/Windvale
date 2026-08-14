# Decision 0540: First ABI-23 storage-call lowering

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows lowering evidence
- Requires: [bounded provider table](0537-Bounded-Native-Capability-Provider-Table.md), [provider-call emission](0538-First-Native-Capability-Provider-Call-Emission.md), and [execution-context 9 construction](0539-Bounded-Execution-Context-9-Construction.md)
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [`storage.random_access_v1` native lowering](../../Specifications/Windvale-Native-Provider-Call.md)
- Retains: Native ABI 22 for every module that does not execute a provider call, WVB 1.11, and WVO 1.0

## Context

The native provider table, exact five-cell x86-64 call sequence, and bounded
execution-context 9 constructor existed as separately executable data-level
evidence. The product lowerer still admitted only the six fixed ABI-22 hosted
capabilities, so a real `storage.random_access_v1` instruction stopped at
`Unsupportedˉcode`. Merely declaring the capability also could not be allowed
to change existing ABI-22 products or their exact WVO bytes.

The lowerer previously reported only a broad status on failure. During this
integration, an incorrectly ordered project source appeared as
`Sourceˉbindings`; the real source-set `Dependencyˉorder` rule was hidden. That
made a correct provider component look like a compiler closure limit.

## Decision

- Admit exactly
  `storage.random_access_v1(u32,u64,u64,u32,bytes)->bytes` as capability kind 7.
- Consume five typed stack values, copy their complete 16-byte cells through
  the independently verified provider-call emitter, and publish one returned
  `bytes` descriptor only after provider success.
- Measure 226 target bytes for the instruction: the exact 216-byte provider
  call plus the standard 10-byte allocation-budget guard required by a
  bytes-producing instruction.
- Select native ABI 23 only when executable function code actually contains a
  storage provider call. A declaration without a call and every existing
  fixed-service program remain ABI 22.
- Preserve the exact WVO 1.0 object contract. ABI selection remains lowering
  and hosting evidence until a successor hosted container binds context 9 and
  a matching provider table.
- Extend native build and lowerer rejection diagnostics with source-set, parse,
  body, graph, symbol, binding, plan, function, and detail status where the
  relevant phase provides them.
- Add one deterministic lower-only database-storage case. It proves two
  identical WVB builds, two identical ABI-23 WVO lowerings, and structural WVO
  admission without pretending the not-yet-integrated host storage leaf can
  execute.

## Evidence

The focused storage fixture builds to a deterministic 449-byte WVB with SHA-256
`535e4d35923e955a8b39d080e44f23f068d21700e53c4ae369eb4d440ca096b7`.
The clean native lowerer reports `abi=23` and emits a verified 2,758-byte WVO
with SHA-256
`5eea8f66666a474a096160fbb9cfae49f9af4627bfae61dafc5fc440242d8681`.
The object contains one 2,599-byte `Main`, one bounded data relocation, and one
exact 216-byte provider-call signature.

An unchanged four-function control fixture remains ABI 22 and its 6,216-byte
candidate WVO is byte-for-byte equal to the current baseline. The full modified
lowerer WVB compiled in 15.7 seconds; packaging that diagnostic lowerer took
52.9 seconds. These timings are measurements, not accepted performance goals.

## Consequences

- Native source-to-WVO compilation now reaches the real provider boundary for
  the first stateful capability without changing existing ABI-22 output.
- ABI 23 is not yet an executable hosted product contract. Windows and Linux
  storage leaves, context/table binding, WVB-to-provider identity admission,
  writer fencing, scratch ownership, revocation, and real-file recovery remain
  required before execution.
- Compiler failures now expose the phase-specific cause that previously needed
  a reconstructed diagnostic compiler to discover.
- The next database slice can concentrate on one pre-opened provider instance
  rather than revisiting language, WVB, or x86-64 call shape.

## Reconsideration triggers

Revisit the ABI selection when provider use becomes first-class metadata rather
than code inspection, when WVO or hosted containers must carry the selected ABI
explicitly, when a bytes result no longer uses the shared allocation guard, or
when another capability requires a different cell/result convention.
