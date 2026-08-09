# Decision 0422: Atomic Linux hosted executable mode

- Status: Implemented candidate; paired Windows/Debian rerun pending
- Date: 2026-08-08
- Advances: [Decision 0420](0420-Multi-Fragment-Current-Lowerer-Reconstruction.md), [Decision 0414](0414-Native-Hosted-Wvb-Package-Composition.md), and [Decision 0390](0390-Linux-Durable-Multi-Chunk-Publication.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The first paired current-lowerer reconstruction reached genuine Debian hosted
composition with exact bytes, but the produced ELF was not executable. The
shared Linux durable publisher created every exclusive sibling with mode
`0600`, which is correct for staged WVO data but incomplete for a hosted
application. Applying `chmod` after replacement would expose a successfully
published, non-executable application and split one atomic publication into two
host mutations.

## Decision

Pass the exact final file mode into the reusable native Linux publication
transaction. Admit only `0600` (decimal 384) and `0755` (decimal 493). The
staged-WVO adapter selects `0600`; the hosted-container adapter selects `0755`.
Apply that mode when exclusively creating the private sibling, before writes,
flush, reread, and same-directory replacement. Host scripts do not decode the
format or repair permissions after publication.

The immutable-snapshot shell preserves the policy value across acquisition and
passes it to the durable transaction. This changes native WVA and regenerated
WVO objects. C# changes are limited to Stage 0 recovery builders and exact test
identity pins; they add no product behavior.

## Evidence and consequences

The regenerated Linux objects are:

| Object | Bytes | SHA-256 |
| --- | ---: | --- |
| staged-WVO policy | 287 | `c4f51956b86d477a93232091da491b0c8a4a117150125dbf24e817234d0a1da1` |
| immutable snapshot shell | 3,503 | `939c7dc5bf6a0ddf94bc4f406f40b0733f15f21d323b7eae5cd8e58103004481` |
| hosted-container policy | 300 | `7cdbf94f43d53252db293fda79c69773c5d40b03a1acbd120256614487583929` |
| durable transaction | 2,468 | `01eb64695ef008da75b282079fde1cb93766bdc37a518695b2377eecee7b3f85` |

The synchronized staging-publisher WVB is 433,523 bytes at SHA-256
`221b20ab5db8785ec495d2151088c532f53fc8c9c66fbb021156f05b62e32ca3`.
Its Windows application is 6,390,784 bytes at SHA-256
`adcdde6363b79e107f26e7042c2970996fabfa972d9a4fe91f5c8a5d5238faa6`;
its Linux application is 6,390,685 bytes at SHA-256
`eed27297af45813c824558aaee8ac515f62b36fdcfdd76845e2ed15a0f924ce4`.
The Linux hosted publisher is 377,789 bytes at SHA-256
`5d8eb97eff9c18e91f1dcb6d2060dd214530474ee046bc2e276e9c603262bd1b`.
The 5,426-byte hosted tool inventory is at SHA-256
`9d60316098f3854cc286a03982b59cce80ced7cd7ab08e8ceef6dc6ecf58b040`.

The focused staged-publisher and hosted-segment-set tests pass locally. The
ordinary and segmented Windows process smokes pass 2/2 each against the new
inventory. GitHub run
[`31289330407`](https://github.com/eworker-inc/Windvale/actions/runs/31289330407)
records the Debian mode failure that selected this boundary. Its paired rerun,
candidate promotion, ordinary-launcher cutover, Standard, Qualification, and
the grouped Decision 0057 gate remain pending.

## Reconsideration triggers

Version the publication policy if another output class, mode, ACL, extended
attribute, or ownership contract is required. Do not add a post-replacement
permission repair to host scripts.
