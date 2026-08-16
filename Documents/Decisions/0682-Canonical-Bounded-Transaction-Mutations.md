# Decision 0682: Canonical bounded transaction mutations

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [transaction mutations](../../Specifications/Windvale-Database-Transaction-Mutations.md)

## Context

Windvale Database can durably put or delete one key at any admitted tree depth,
and its commit batch can publish up to 63 prepared pages atomically. It did not
have a canonical way to describe several mutations before planning shared tree
paths. Calling the existing one-key operations in sequence would publish
several generations and would not be atomic.

EWDB validates a complete transaction, bounds operation and table counts,
orders owned resources, prepares copy-on-write roots, and publishes only after
preflight succeeds. Those principles are useful, but its JSON/.NET evidence
model and 1,000-operation ceiling are too large for this native storage layer.

## Decision

- Define portable `WVTM 1` as one versioned set of 1 through 32 puts/deletes.
- Limit the complete encoding to 256 KiB, keys to 4,096 bytes, and values to
  61,440 bytes, matching existing tree key/value limits.
- Require strictly increasing unsigned bytewise keys. Reject duplicate keys
  instead of assigning order-dependent meaning.
- Validate the complete framing and semantics before returning owned bytes.
- Keep this contract independent of storage authority and durable execution.
- Give it one focused native development target. The next transaction planner
  will consume it, prepare a shared tree rewrite, and use one existing commit
  batch.

## Evidence

The focused source build produces a deterministic 21,077-byte WVB with
SHA-256 `6acfe4e40a4d559ed17c3fbf78d66e9cd9f015b6420515f05179042e9664c358`.
It lowers to a 241,908-byte WVO with SHA-256
`eeb5f7fada47d7ef21db75102b5497cc7ec04dee459b25e96ff604ecfaa067e0`
and packages as a 259,072-byte Windows application with SHA-256
`d0e8aaa7fd838f6e0504bbebaf37d9a179c2dda77e3f3d9c6219c6e1a8f605b1`.
The application returns zero.

Twenty fresh whole-process runs measured 21.770 ms minimum, 23.865 ms median,
24.079 ms mean, and 32.514 ms maximum. Peak sampled working set across those
runs was 7,806,976 bytes. These numbers include process startup and test all
valid and malformed cases; they are development evidence, not transaction
throughput.

The normal Linux-focused wrapper could not start its cached build phase in
this Windows desktop environment because WSL has no Linux `node` executable;
the available bundled `node.exe` cannot consume WSL paths. The recorded result
uses the repository's Windows native front door, lowerer, linker, and hosted
packager directly. Changed-file planning passes 24 general and 131 native
routing cases. Native development dependency closure passes for 3 owners and
34 declarations; Bash syntax and diff checks also pass.

## Consequences

Windvale now has a deterministic and bounded transaction input boundary that
can also carry future primary-row and secondary-index key mutations. It does
not yet claim atomic multi-record execution; that claim begins only when one
planner and one durable publication consume the complete set.

The strict sorted-key rule moves normalization to callers that expose a more
convenient order. This is intentional: storage planning stays deterministic,
duplicate meaning stays explicit, and validation remains linear.

## Reconsideration triggers

Revisit the 32-operation or 256 KiB limits only with measured planner, server,
and memory evidence. Revisit duplicate rejection only if a versioned contract
defines one unambiguous normalization rule before storage planning.
