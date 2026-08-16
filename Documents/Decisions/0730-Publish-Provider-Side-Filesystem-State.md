# Decision 0730: Publish provider-side filesystem state

- Status: Accepted; advanced by [Decision 0731](0731-Publish-Durable-Filesystem-Domain-Ledger.md)
- Date: 2026-08-16
- Advances: [Decision 0729](0729-Privately-Construct-The-Filesystem-Machine.md)
- Contracts: [provider launch transaction](../../Specifications/Windvale-Os-Provider-Launch-Transaction.md), [process-object build](../../Specifications/Windvale-Os-Process-Object.md), and [filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md)

## Context

The generation-three filesystem machine was privately allocated and validated,
but its process record still became ready inside the record constructor, its
configuration digest was the SHA-256 empty digest, and no fresh thread or
generation-two endpoint record existed. Publishing a fully usable endpoint was
not yet honest: terminal client generation 2 had been recycled into the
provider, so no surviving application could own the consumer side.

The fixed kernel state page also has no unallocated durable record range for a
new resource-domain ledger. The existing 81-user-page record evidence and
portable launch policy prove the selected accounting geometry, but they do not
constitute a live kernel domain publication.

## Decision

Keep the generated `WVPROC17` record private by writing state 0 in its source
constructor. Bind its configuration field to SHA-256
`0e34a46dd568fdf97fb72c005d11bc626e9c2950b706fec73cb166521ccfecf4`,
the exact admitted 80-byte `WVPR 1` filesystem launch request. This digest is
launch-configuration identity, not FAT32 media identity.

Before releasing client generation 2, require the exact terminal endpoint and
retained channel evidence. After the existing release, allocation, image,
paging, record, and validation steps, clear and reconstruct:

- channel slot 0 as one empty capacity-one `WVCHAN04`;
- endpoint slot 0 as provider-side generation 2, reference `131072`, provider
  process `196610`, and client reference 0; and
- thread slot 2 as generation 3, reference `196610`, with the filesystem image
  entry at extent offset 16,384 and stack top at extent offset 348,160.

Finish the private process kernel-stack field before publication. Commit the
endpoint, thread, and process ready states in that order. A zero endpoint client
means no application capability exists and the endpoint must not be resolved
for traffic until a later transaction binds a generation-safe consumer.

Do not enter the provider, dispatch the fresh thread, accept a request, claim a
live resource-domain ledger, or claim FAT32 media binding in this slice.

## Consequences

The rebuilt process object remains 956,321 bytes and is SHA-256
`ea07c502f0b3f45e650284426c136c601c9fdacf8addfa9f99fd890cc2a535a1`.
The process-object tool WVB is 42,667 bytes at SHA-256
`466d51363711ef6c1cf619ecfa885c39fd8408cd392bdb0d480dc00cbee3eae9`;
its Windows and Linux hosted applications remain 590,336 and 589,824 bytes at
SHA-256 `65f5f114cfa6192c3b363592dd4b38830d692f188e3838342665bfb42836972b`
and `ad506a348f23ce89242ab106e9cdc4a2981c58bec6a337028d780e28b5456520`.

The filesystem construction object is 2,167 bytes at SHA-256
`63c9c8397cf86b9f8af08b616b1152a287789fa77002d00bcac789dbf1bb180d`.
Linked executable code ends at byte 793,095, below the fixed 794,624-byte
supervisor RX boundary.

All three current EFI images are 1,698,304 bytes. Their SHA-256 identities are
`6ffec58edefd6c09c7c552858316da1be02cbceb515715bea36ac5ef0a140018`
for normal,
`84d0c66f9b6a0ea7ed4c1f3c9416884d80481d42117693298b96baa71e888e3c`
for invalid opcode, and
`9f0d51b6f057387f01e054ab22167fc261981748d5d593f37e6aee26256ef740`
for general protection. Pinned Windows QEMU 11.0/Q35/TCG passes the normal
shutdown and both terminal exception transcripts.

The current leaf still lacks complete rollback after memory ownership changes:
a deterministic post-constructor validation failure can leave the unpublished
generation-three allocation. The consumer-binding slice must either eliminate
those remaining fallible checks before mutation or add exact reverse-order
reclamation before it can claim a general failure-atomic launch transaction.

## Reconsideration triggers

Revisit the provider-only endpoint state if a reusable resolver cannot preserve
the zero-client non-authority rule, if the fixed state page cannot gain a
versioned domain record without breaking retained evidence, or if consumer
binding cannot remain atomic with endpoint finalization and first entry. Never
substitute a stale or exited process reference merely to make both endpoint
sides nonzero.
