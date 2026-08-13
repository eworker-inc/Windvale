# Windvale database storage publication and recovery

## Status

- Version: storage planner 1
- Profile: portable, capability-free action/observation core
- Source: `Libraries/Database/Storage-Publication.wv` and
  `Libraries/Database/Storage-Recovery.wv`
- Native provider dependency: not implemented for ABI 22

## Boundary

This contract composes validated `WVDS 1`, `WVPG 1`, and `WVCR 1` records
with exact storage actions. It does not perform I/O or grant storage authority.
A future hosted or Windvale OS executor must bind one fenced
`storage.random_access_v1` object and translate each action without changing
its completion meaning.

The publication state embeds the complete immutable commit-publication value.
Its nested generation, sequence, length, slot, and stage evidence therefore
survives chunking without a second flattened state vocabulary.

## Publication actions

Page writes are split into exact chunks of at most 65,536 bytes. A completed
chunk advances only the byte cursor; the embedded commit state advances from
`Write_pages` to `Flush_pages` only after the complete append is accepted.
The remaining sequence is unchanged:

```text
Write pages -> Flush(Content_and_length)
            -> Write inactive 256-byte superblock
            -> Flush(Content) -> Committed
```

Each observation echoes the provider generation and action position. Writes
report exact completed progress, exact positive partial progress, or no
progress for an indeterminate result. A changed storage length, stale
generation, malformed response, partial mutation, or indeterminate mutation
enters recovery. A rejected pre-dispatch mutation aborts, except uncertainty
after superblock publication still requires recovery.

## Reopen and tail truncation

Recovery begins only from a fresh valid dual-superblock selection and a
nonzero provider generation. The observed storage length must equal checked
`committed_length + tail_bytes` from that selection.

No tail means the object is ready immediately. A positive tail produces:

```text
Resize(committed_length)
  -> Flush(Content_and_length)
  -> Ready
```

The resize may remove only the selected unpublished tail. Completion must
report exactly the selected committed length and zero progress. The following
flush makes the shortened logical length durable before a new append begins.

`Indeterminate`, `Stale`, an unexpected length, a malformed response, or an
impossible partial resize/flush enters `Reopen`; the caller must reacquire and
describe the provider, read both superblocks again, and construct a new plan.
It must not retry the uncertain mutation. A valid pre-dispatch provider
rejection enters `Stopped` and retains its typed provider error.

## Exclusions

The portable core is not a writer fence, capability executor, crash harness,
path or directory API, transaction manager, page cache, or reclamation
policy. ABI 22 has no native `storage.random_access_v1` service-table slot;
adding one requires a separately versioned native ABI/service contract and
paired Windows/Linux provider evidence. Whole-file snapshot writes are not an
acceptable substitute for positioned mutation and flush observations.

## Verification

The nested-record compiler fixture proves source/WVB and native x64 aggregate
semantics. Database fixtures cover 65,536-plus-one page chunking, complete
publication ordering, rejected/partial/stale/changed observations, no-tail
reopen, tail resize and length flush, indeterminate recovery, and invalid
selection or provider evidence.
