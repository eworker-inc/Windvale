# Windvale WVB publication transaction

## Status and scope

The portable transaction core is implemented under
[Decision 0214](../Documents/Decisions/0214-Exact-Native-Wvb-Publication-Step.md).
It owns the platform-independent state and outcome rules for the future exact native
WVB publisher. Windows/Linux resource identity, exclusive sibling creation, durable
write, byte reconstruction, atomic replacement, directory durability, and raw tool
packages remain pending.

This module does not read or write files, verify WVB, interpret paths, or perform a
replacement. It receives only confirmed adapter milestones and makes it impossible
for host wrappers to reinterpret a post-replacement failure as known unchanged.

## States and actions

The transaction begins only after the caller's candidate snapshot was admitted by
the shared compiler-aligned verifier.

| State | Meaning |
| --- | --- |
| `Candidateˉadmitted` | Exact immutable candidate bytes are admitted; no scratch exists. |
| `Siblingˉowned` | The publisher exclusively owns a sibling in the destination directory. |
| `Siblingˉdurable` | The complete sibling write received the required durable-file evidence. |
| `Siblingˉverified` | Reread length and SHA-256 match the admitted snapshot. |
| `Destinationˉreplaced` | One same-directory replacement completed; directory durability is not yet confirmed. |
| `Cleanupˉrequired` | A pre-replacement failure left publisher-owned scratch that must be removed. |
| `Rejectedˉunchanged` | The destination is known unchanged and publisher scratch is absent. |
| `Complete` | Replacement and the required directory-durability step completed. |
| `Indeterminate` | Replacement may be visible or durable; blind replay is forbidden. |

The success sequence is exact:

```text
Candidate admitted
  -> Create sibling
  -> Flush sibling
  -> Verify sibling
  -> Replace destination
  -> Flush directory
  -> Complete
```

`Fail` at `Candidateˉadmitted` produces `Rejectedˉunchanged`. `Fail` from any
sibling-owned pre-replacement state produces `Cleanupˉrequired`; only
`Cleanˉsibling` can then produce `Rejectedˉunchanged`. `Fail` after
`Destinationˉreplaced` produces `Indeterminate`. Terminal states reject every later
action. An invalid action retains the prior state, scratch ownership, and
destination-may-differ evidence while returning `Invalidˉtransition`.

## Exact module

The canonical source is
[`Tools/Windvale.Publish/Wvb-Publication-Transaction.wv`](../Tools/Windvale.Publish/Wvb-Publication-Transaction.wv).
It compiles to a portable 4,560-byte WVB with SHA-256
`6c579d06e481ff5a2cde04463ccc84e78c458eea2c7865bf8797f22136c11a52`,
four nominal types, five functions, and exactly two exports:

```text
Wvbˉpublicationˉbegin() -> Wvbˉpublicationˉresult
Wvbˉpublicationˉapply(
    Wvbˉpublicationˉresult,
    Wvbˉpublicationˉaction
) -> Wvbˉpublicationˉresult
```

The executable fixture is deliberately separate from the core and returns zero
only after checking the complete success path, each cleanup-producing failure,
known unchanged rejection, post-replacement indeterminate completion, and invalid
transition evidence. Its exact composed WVB is 7,684 bytes with SHA-256
`4be7e3948576498963f2858fd95d7273d6b63d467fbbb2b344c86c223a8864ce`.

## Remaining native boundary

The native adapters must prove each action before submitting it to the state
machine. They may not submit `Flushˉsibling`, `Verifyˉsibling`,
`Replaceˉdestination`, `Flushˉdirectory`, or `Cleanˉsibling` merely because an API
call was attempted. Provider rejection before replacement must retain evidence that
the destination is unchanged. Any failure after replacement that lacks confirmed
directory durability remains indeterminate.

The adapters and final publisher packages require the full Decision 0214 fault,
concurrency, identity, direct no-.NET execution, deterministic package, and
cross-host evidence before normal-path cutover.
