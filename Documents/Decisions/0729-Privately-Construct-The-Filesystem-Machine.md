# Decision 0729: Privately construct the filesystem machine

- Status: Accepted; advanced by [Decision 0730](0730-Publish-Provider-Side-Filesystem-State.md)
- Date: 2026-08-16
- Advances: [Decision 0728](0728-Boot-Link-The-Filesystem-Machine.md)
- Contracts: [provider launch transaction](../../Specifications/Windvale-Os-Provider-Launch-Transaction.md), [process-object build](../../Specifications/Windvale-Os-Process-Object.md), and [filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md)

## Context

Probe 40 linked the generation-three filesystem image, paging, and process
record constructors but returned after the second client generation without
invoking them. That client still owned the reusable 122-page memory object.
Publishing a provider before releasing that exact generation, or releasing it
without immediately proving the replacement, would violate the selected
provider launch transaction.

The linked constructor sections were raw straight-line fragments. Treating
them as callable functions exposed two latent defects in the live boot path:
the fragments had no return instruction, and the filesystem record still
wrote generation 1 even though its contract selected generation 3.

## Decision

Replace only the retained five-byte final result instruction at process fixture
offset 33,801 with a typed relative call. The new WVA leaf receives the already
validated `WVKMEM17` state in `R12`, releases client memory reference `131074`,
requires cursor 13 and 122 free pages, first-fits reference `196610` for exactly
85 pages at the same physical root, and requires 37 free pages afterward.

Adapt each source-owned constructor fragment into a callable process-object
function by appending one `ret` byte. Existing 16-byte section alignment keeps
their link addresses and every later process-object section address unchanged.
Correct the `WVPROC17` constructor to publish generation 3 at offset `0x14`.
Invoke image copy, W^X paging construction, and process-record construction in
that order, then revalidate the complete memory identity and selected process
fields before returning result 6.

Do not advance endpoint `131072`, build a ready thread, enter user mode, publish
a live resource-domain charge, or expose the record to the dispatcher in this
slice. The retained generation-two thread remains terminal, so the constructed
record is not runnable.

## Consequences

The callable image, paging, and record sections are 59, 3,343, and 463 bytes at
the unchanged link addresses 780,192, 780,256, and 783,600. The process object
is 956,321 bytes at SHA-256
`9f310ad538580bbc00f5dcf38428eac7daef78a5f78fc1bc95b22a4b4dad7b45`,
with 14 sections, 34 symbols, and 61 relocations. The 1,068-byte construction
object has SHA-256
`755278057f3415f0ed1661364c2be636efb879875f0a9a74bd5d3a0f9238b763`.

All three current EFI images are 1,697,280 bytes. Their SHA-256 identities are
`be0f0f168bd801489737f60fa0ebef436f62b764175683dfbe8782a1c69588c1`
for normal,
`ad38552ad37ac444d8d0443c5942eb60fec5171a9cbddea6144b0f57c109aa7c`
for invalid opcode, and
`9f7c9d9d7ec36a3d8ed0c714fe7d32bc33bae27eb453db9bf8fe51c79d327acc`
for general protection. Pinned Windows QEMU 11.0/Q35/TCG passes the normal
shutdown and both terminal exception transcripts. Independent Linux build
execution remains part of the cross-host gate rather than a claim here.

The empty configuration digest remains a placeholder. The next transaction
must admit the FAT32 media/configuration identity, privately construct a fresh
thread/context publication, advance and bind endpoint `131072`, commit the
81-page domain charge with the process publication, enter the provider, and
complete one bounded read. Failure before atomic publication must remain
invisible and reclaim the generation-three object.

## Reconsideration triggers

Revisit this boundary if provider construction cannot remain non-runnable until
endpoint/domain publication, if a constructor needs a different private ABI,
or if rollback cannot deterministically retire the 85-page object. Do not make
the record runnable merely to simplify the next integration step.
