# Decision 0391: Reusable Windows durable multi-chunk publication

- Status: Implemented candidate
- Date: 2026-08-08
- Advances: [Decision 0390](0390-Reusable-Linux-Durable-Multi-Chunk-Publication.md), [Decision 0389](0389-Shared-Immutable-Snapshot-Sequence.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native x64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md#selected-object)
- Advanced by: [Decision 0392](0392-Shared-Immutable-Snapshot-Publisher-Shells.md)

## Context

The Windows staged-WVO adapter combined WVO acquisition and identity policy
with full-path sibling creation, handle anchoring, partial I/O, flush, reread,
handle-relative replacement, cleanup, transaction state, and outcome mapping in
one 1,146-line source. Decision 0390 established the corresponding reusable
Linux boundary. Hosted-container publication needs the same Windows mutation
protocol with a different admitted snapshot selection.

Duplicating that block would create another very large source and a second
security-sensitive implementation. Moving argument or WVO manifest semantics
into the transaction would make it unusable by the hosted path.

## Decision

Add `Windows-X64-Durable-Multi-Chunk-Publication.wva` as a format-neutral
handle-relative transaction. It accepts the anchored directory handle and path,
validated destination basename and character count, first and one-past snapshot
ordinals, stride one or two, and a fixed per-snapshot header skip.

The transaction owns:

- exclusive `.wvpub-<hex>` full-path sibling creation;
- immediate no-replace anchoring below the verified directory handle;
- exact partial writes directly from immutable snapshot payloads;
- `FlushFileBuffers`, rewind, bounded reread/byte comparison, and exact EOF;
- the Windvale-owned begin/apply state transitions;
- replacement through `FILE_RENAME_INFO` relative to the directory handle;
- durability through the same renamed handle;
- delete-on-close cleanup before replacement; and
- distinct `0` complete, `1` rejected/unchanged, and `2` indeterminate results.

Keep argument conversion, Windvale admission, snapshot-name reopening, native
file identity checks, source/destination alias rejection, and directory handle
ownership in the format adapter. The WVO adapter calls `(2, count, 1, 0)`; the
hosted adapter will call `(3, count, 2, 40)`.

The extraction reduces the Windows WVO-specific adapter from 1,146 to 689
lines. The 569-line transaction is one cohesive platform mutation boundary,
not a numbered fragment created to satisfy a line target.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows staged-WVO adapter WVO | 6,144 | `89c3516eb56ecb274ba34b3168d1f33987b959cca44a70396eaa0cb5e1ffb258` |
| Windows durable multi-chunk WVO | 4,001 | `3795ab62b6dc5008748ba7c4332b885419a14479c9c11369bcc13885cad8974b` |
| Windows staged-WVO publisher | 6,458,880 | `7aa53023a347a8970956b4fe234e095074bbb8b2ef8abcaa5c22d946db9e313a` |

The reviewed focused current-host test assembles and pins both objects, rebuilds
both platform applications, and executes the changed Windows transaction. It
passes 1/1 in 6.773 test seconds after a 9.38-second zero-warning build. The
matrix covers successful atomic replacement, changed-content rejection,
hard-link destination alias rejection, preservation of both aliased files, and
zero `.wvo-*` scratch residue.

Linux execution remains pending under Decision 0390. No broad verifier was run.

## Consequences

- Both permanent hosts now have reusable durable multi-chunk transaction
  components independent of WVO policy.
- The hosted-container publisher can reuse the exact platform mutations while
  selecting response snapshots and skipping `WVHU` envelopes.
- C# remains deletion-bound object linking and identity pinning only; no product
  mutation semantics were added to managed code.
- Connecting the hosted admission root, snapshot policy, transaction, and paired
  native launchers is now the next complete retirement slice.

## Reconsideration triggers

Revisit this interface if Windows handle-relative rename or durability evidence
changes, sibling creation can become directory-relative without a full path, or
mutation gains an idempotency token. Never report delete failure or a
post-replacement durability failure as safely unchanged.
