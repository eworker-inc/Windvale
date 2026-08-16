# Decision 0650: Windvale-owned remaining client-resource validation

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0649](0649-Windvale-Owned-Init-Return-And-Program-Validation.md)
- Contracts: [budget validation](../../Specifications/Windvale-Os-X64-Process-Init-Return-Budget-Validation-Emission.md), [store/directory validation](../../Specifications/Windvale-Os-X64-Process-Init-Return-Store-Directory-Validation-Emission.md)

## Decision

Emit fixture offsets 13,787 through 14,402 as two contiguous fail-closed
transactions. First validate the exact generation-two budget resource. Then
validate the private backing-record geometry and the generation-one store and
generation-two directory bindings retained by the admitted client resources.
No client context may be selected until all identities, generations, rights,
page-table links, and mutable-state fields pass.

## Evidence and consequences

The normalized slice SHA-256 values are
`d02692175b1bc02c2ea76f23bea345f491c4fc24fe1da7b075502dcb0a371706`
and
`3f58fd5fb48e504b3980da7bdebfb7dfd5eca45c1f3ac91bb360a83eb17760c9`.
The focused owner advances to thirty projects and 180 cases with results 50
through 79. Windvale source owns the first 14,403 process-machine bytes and 100
external relocation fields.

Client context transfer, syscall and exception handler bodies, context
switching, and live QEMU application execution remain separate evidence.

## Reconsideration triggers

Another client-activation design must retain exact generation-scoped resource
identity, private backing-record and page-table checks, explicit rights, empty
mutable-state admission, and a common fail-closed rejection boundary.
