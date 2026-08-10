# Windvale WVB publication transaction

## Status and scope

The portable transaction core and narrow native publisher candidates are
implemented under
[Decision 0214](../Documents/Decisions/0214-Exact-Native-Wvb-Publication-Step.md).
The core owns the platform-independent state and outcome rules for the exact native
WVB publisher. Windows and Linux adapters now implement resource identity,
exclusive sibling creation, durable write, byte reconstruction, atomic replacement,
directory durability, and deterministic raw tool packages. Current-host Windows
direct execution passes; independent Linux execution and the complete cross-host
fault/concurrency qualification matrix remain pending.

This module does not read or write files, verify an artifact, interpret paths, or
perform a replacement. It receives only confirmed adapter milestones and makes it
impossible for host wrappers to reinterpret a post-replacement failure as known
unchanged. Decisions 0307 and 0308 reuse the same format-neutral state machine
and native adapters after portable console-application or WVO admission; the
historical WVB names remain the canonical version-1 source identity.

Decision 0393 adds a capability-free x64 WVA state object exporting
`Native_publication_begin` and `Native_publication_apply`. The durable WVO and
hosted-container transactions call this shared native token owner directly;
their product packages no longer depend on private bridge functions extracted
from a Stage 0-compiled WVB fragment. The portable `.wv` module remains the
semantic source and recovery oracle for the complete typed result contract.

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
known unchanged rejection, post-replacement indeterminate completion, invalid
transition evidence, and native-bridge result encoding. Its exact composed WVB is
13,617 bytes with SHA-256
`a9c356ba0bcbd61fd6bac7afd40c10e752f3eedad729077d5abdc5518ae188a4`.

## Native boundary and remaining qualification

The native adapters must prove each action before submitting it to the state
machine. They may not submit `Flushˉsibling`, `Verifyˉsibling`,
`Replaceˉdestination`, `Flushˉdirectory`, or `Cleanˉsibling` merely because an API
call was attempted. Provider rejection before replacement must retain evidence that
the destination is unchanged. Any failure after replacement that lacks confirmed
directory durability remains indeterminate.

The implemented adapters retain this proof ordering and share the portable state
machine, verifier, SHA-256, and success transcript. Current publisher-family
applications, including the general WVB, WVO, and console-application roles,
have bounded retained-seed native reconstruction evidence; Stage 0 is no longer
their only application constructor. That construction evidence does not replace
the full Decision 0214 fault, concurrency, identity, direct no-.NET execution,
deterministic package, and cross-host matrix. Normal-path cutover must not
precede that qualification.
