# Windvale OS FAT32 file-read transaction 1

## Status and scope

File-read transaction 1 is the implemented architecture-neutral composition
between an authorized shared read request, an admitted FAT32 file entry and
cluster trace, the bounded file-read planner, the capacity-one block exchange,
and the existing filesystem response validator.

[`Fat32-Chain-Position.wv`](../Operating-System/Services/Fat32-Chain-Position.wv)
first admits the complete trace through the existing chain contract and then
selects one exact ordinal without duplicating traversal validation.
[`Fat32-File-Read-Transaction.wv`](../Operating-System/Services/Fat32-File-Read-Transaction.wv)
owns the multi-exchange read state and final `WVFP 1` construction.

## Authority and lifecycle

The caller presents a nonzero file reference only after the filesystem provider
has authorized that reference for read. The transaction requires the same
reference in the admitted `WVFQ 1` request. It also binds one nonzero media
generation; every dispatched block exchange must carry that generation, the
exact planned device sector and count, nonzero endpoint and block references,
and the next capacity-one sequence.

The transaction takes one ready capacity-one exchange at admission, flattens its
exact grant, endpoint, block, generation, and next-sequence identity into owned
state, and internally begins each step as `Awaiting`. Only the transaction can
dispatch or complete that owned step; it reconstructs the exact exchange from
its immutable fields instead of accepting a separately completed exchange.
Completion is accepted only from the bound endpoint, generation, plan identity,
and consumed sequence. Unavailable media, provider loss, malformed payload,
changed generation, or mismatched identity fails the read without publishing
successful file bytes.

## Bounded accumulation and response

The target is exactly `min(request maximum, file length - position)` and is at
most 65,536 bytes. Each prepared step resolves the cluster for the current file
offset and inherits the planner's maximum eight sectors and 4,096-byte covered
window. Exact partial-sector bytes are sliced only after the block protocol has
validated the complete sector payload. Steps continue through the admitted
chain until the target is complete.

Zero-length and exact-end reads complete without block authority. A successful
transaction emits one `WVFP 1` response containing the exact target bytes,
original correlation, file reference and position, and admitted FAT32 file
length. The existing filesystem-service validator must accept the constructed
response before it is returned.

The complete transaction reply fits kernel
[endpoint transfer profile 3](Windvale-Os-Endpoint-Transfer-Profile.md), whose
exact caller capacity is 65,600 bytes. Individual `WVBR 1`/`WVBP 1` exchanges
fit profile 2's 48-byte request and 4,144-byte reply capacity.

## Evidence and limits

The chain-position module is a 7,186-byte WVB at SHA-256
`82eb95c9259e5ee851272c7698f5b2cbea69a9ef585398079346d0bdb7326393`.
The composed transaction module is a 73,587-byte WVB at SHA-256
`ed6219dee7ef97ff3bef1fc62bb6fac81c67cf7d51a4613f74c27584aa5da005`.
Its 18-case test returns 47, pins paired Windows/Linux images, and proves a
4,500-byte read across two exchanges and two clusters.

This is portable transaction-policy evidence, not a privileged endpoint syscall
or block-driver claim. The caller still owns the prior directory/file-reference
association and supplies trace and geometry from the same admitted media
generation. Media discovery, caching, concurrent reads, writable FAT32, and
hardware qualification remain separate contracts.
