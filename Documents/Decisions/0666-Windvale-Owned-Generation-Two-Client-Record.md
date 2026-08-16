# Decision 0666: Windvale-owned generation-2 client record

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0665](0665-Windvale-Owned-Client-Memory-Recycle.md)
- Contract: [generation-2 client-record emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Record-Emission.md)

## Decision

Emit fixture offsets 19,742 through 20,240 as one private generation-2 record
reconstruction transaction. Clear the complete retained client record, then
rebuild its exact generation, identity, bounds, image digests, private extent
addresses, capability slots, and retained service endpoint bindings without
publishing it ready.

## Evidence and consequences

The exact payload SHA-256 is
`9ec3a038c6580b02d5b76cf7e60fdcfc6cc4a4a03ba9f57c6ca3495d176224fd`.
The focused owner advances to forty-one projects and 246 cases with results 50
through 90. Windvale source owns the first 20,241 process-machine bytes and 120
external relocation fields.

The recycled root now has a generation-distinct process record before any page
or readiness publication. Generation-2 paging, images, resources, context,
endpoint rebinding, and re-entry remain separate.

## Reconsideration triggers

Another record design must clear the complete retained record and preserve all
generation, identity, digest, address, limit, capability, and endpoint fields
before any mapping or ready-state publication.
