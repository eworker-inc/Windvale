# Decision 0712: Add first typed singleton capability references

- Status: Implemented
- Date: 2026-08-16
- Advances: [post-.NET-retirement language and library stage](../Project/Post-Dotnet-Retirement-Language-And-Libraries.md)
- Contracts: [Seed language](../../Specifications/Seed-Language.md), [typed source IR](../../Specifications/Compiler-Source-Wir.md), and [source-to-WVB backend](../../Specifications/Compiler-Source-Wvb.md)

## Context

Capability declarations approve interfaces but do not let a library receive and
forward an exact capability reference. Facades therefore have to call a qualified
capability name directly, which hides the dependency in their signatures and
prevents a consumer from supplying the approved provider reference explicitly.

The first consumer is the read-only directory facade. It needs dependency
injection without introducing handles, dynamic provider selection, ownership, or
a new serialized authority format.

## Decision

- A required root capability's qualified name is also its exact source type and
  singleton value. It may appear in parameters, function results, and locals.
- A value call, such as `Output(Value)`, invokes the same catalog operation as the
  approved qualified capability represented by that value.
- References are freely copyable, shared, and non-owned. They have no `close`,
  `using`, comparison, or conversion behavior.
- Capability references cannot appear in records, variants, collections, module
  data, or constants. An optional-only capability cannot be acquired as a value.
- The internal shape is `0x10000000 + RootCapabilityDirectoryEntry`. Acquisition
  emits the existing typed `U32_constant` with zero payload. WVB parameters,
  results, locals, and temporaries erase that witness to `u32`; calls remain the
  existing statically ranked `call.capability` instruction.
- The zero witness cannot select or create authority. Runtime binding,
  authorization, revocation, restart, peer-loss, and operation failure behavior
  remain the existing capability-call contract and are checked on every call.

## Consequences

The read-only directory facade can expose a function that receives an exact
`filesystem.directory_read_v1` reference while retaining its direct-call wrapper.
Libraries can now make singleton capability dependencies visible in signatures
without changing WVB 1.11 or the runtime provider model.

This is deliberately not a general handle system. It does not support multiple
instances of one interface, provider generations as values, dynamic acquisition,
optional probing, transfer of ownership, or scoped cleanup.

## Reconsideration triggers

Introduce an instance-bearing serialized reference only when a measured consumer
needs more than one provider of the same interface and the contract defines
generation, revocation, restart, comparison, storage, transfer, teardown, and
forgery resistance. Add scoped ownership separately for an affine value with an
ordinary close operation and exact early-return cleanup ordering.
