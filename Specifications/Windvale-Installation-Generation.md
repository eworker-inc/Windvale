# Windvale Generation 1 and Activation 1

## Status and purpose

Generation 1 and Activation 1 define the first offline installed-generation and
rollback contract. They are portable canonical records. Host adapters own file
acquisition, immutable publication, durable replacement, command shims, and
rights-limited provider binding; they may not reinterpret these records.

This first contract deliberately excludes network discovery, automatic updates,
garbage collection, concurrent activation writers, Windvale OS A/B slots, and
security-policy rollback prevention.

## Canonical text

Both records are strict UTF-8 without a byte-order mark, use LF line endings,
end in LF, contain no blank lines or comments, and are at most 65,536 bytes.
Spaces and tokens follow Package 1 canonical-text rules. Digests are 64 lowercase
hexadecimal SHA-256 identities. Counts and serials are canonical unsigned decimal.

## Generation 1

A generation contains one exact target, one or more admitted packages, and one
or more installed commands:

```text
windvale-generation 1
target <target-id>
package <package-id> <version> <bundle-sha256> <lock-sha256>
command <command-id> <package-id> <part-id> <approval-sha256> <launch-sha256>
```

Package records are ordered by package identifier. Command records follow every
package and are ordered by command identifier. Identifiers and source versions
use the bounded Package 1 token rules. Every command names a package in the same
generation. A record admits at most 64 packages and 64 commands.

The SHA-256 of the complete canonical Generation 1 bytes is the generation
identity and immutable store key. The record does not contain its own identity.
Package versions and names are labels; the bundle and lock digests select bytes.
Approval and launch digests select policy but do not grant authority by
themselves.

## Activation 1

One small record selects the active and immediately previous generations:

```text
windvale-activation 1
serial <u64>
current <generation-sha256>
previous <none|generation-sha256>
```

The first activation has serial `1` and `previous none`. Every effective
activation or rollback increments the serial exactly once. Re-selecting the
current generation is idempotent and does not change the record. Current and
previous may not be the same. Serial overflow is terminal until a successor
contract is accepted; it never wraps.

## Activation transaction

Before publishing Activation 1, the host adapter must:

1. admit and immutably publish the requested Generation 1 record and all objects;
2. reread and verify the current Activation 1 and every referenced generation;
3. construct the exact next record from the portable transition plan;
4. write and flush one private sibling, reread it, and replace the activation
   record atomically;
5. make the containing-directory mutation durable where the host can report that
   guarantee; and
6. report rejection, completion, or indeterminate completion distinctly.

An interrupted private sibling is never active. Recovery rereads the public
activation record first, then removes only a verified unreferenced candidate.
It does not guess whether a failed replace completed.

Rollback swaps `current` and `previous`, increments the serial, and requires both
referenced generations to remain admitted. It does not rewrite package content,
lower a release high-water mark, or bypass a separate security policy.

## Portable implementation

`Libraries/Package/Installation-Generation.wv` parses both records, exposes
bounded package/command views, validates cross-record package references, and
plans idempotent activation and rollback transitions. It performs no I/O and
does not claim durable publication. Host lifecycle adapters must consume its
semantic result rather than implement a second record grammar.

## Active command resolution

`Tools/Windvale.Package/Installation-Command-Resolver-Tool.wv` is the first
Windvale-written active-command selector. A host supplies the public Activation
1 file, the Generation 1 file named by the host path, the current target, and
one command identifier. The resolver:

1. parses both records through the portable implementation;
2. hashes the complete generation and requires it to equal Activation 1's
   current identity;
3. requires the generation target to equal the caller's current target; and
4. returns the exact package, part, approval, and launch identities for one
   command record.

Unknown commands, malformed records, inactive generations, and wrong targets
fail explicitly. Resolution does not execute a process or grant the selected
approval. The later host dispatcher must independently bind the exact launch
record and may execute only after all referenced installed objects reverify.

## Host command dispatch

`Tools/Package/Dispatch-Installation-Command.mjs` is the first bounded
Windows/Linux process adapter. It opens the public Activation 1 record and its
digest-named immutable Generation 1 record, invokes the Windvale-written
resolver, and accepts only its exact canonical success report. Before process
creation it independently requires all of these identities and relationships:

1. the generation selected by Activation 1 remains byte-identical;
2. the supplied bundle matches the selected package's bundle identity;
3. the approval matches the selected approval, package, version, bundle, lock,
   and executable identities;
4. the target launch record matches the selected launch, approval, bundle,
   executable, target, and host-application identities; and
5. command arguments satisfy the exact `wvdump` or `wvquery` launch profile.

The adapter copies the verified host bytes into private storage, flushes and
rereads that copy, then starts it directly without a command shell and propagates
the application's exit status. The first profile supports only the
two exact Generation 1 commands. It grants no package-manager, signing, network,
environment, mutation, or process-launch capability to either application; the
launch records bind the existing inspector file reader and WVDB fixed read-only
directory provider. Unknown commands, unsupported profiles, substitutions,
tampering, and invalid arguments fail before process creation.

This development adapter still receives already acquired object paths from its
caller. A later installed launcher will resolve those paths beneath the durable
object store and add revocation observation without changing the portable
Generation 1 grammar.

## Activation planning adapter

`Tools/Windvale.Package/Installation-Activation-Planner-Tool.wv` exposes the
portable activation and rollback planners to the Windows/Linux host lifecycle.
It accepts one canonical Activation 1 record, an `activate` or `rollback`
request, the requested identity or `none`, and explicit availability of both
referenced generations. It emits no record when the transition is invalid,
unavailable, lacks a previous generation, or exhausts the serial.

A successful result is a canonical five-line `windvale-activation-plan 1`
report. The serial is represented by exact `serial-low` and `serial-high` `u32`
limbs because the current native AOT subset deliberately does not admit
`U64ˉformat`. The host joins those limbs as one unsigned `u64`, serializes the
decimal field without policy choices, and passes the constructed Activation 1
record back through the same Windvale parser/planner before publication.

The composed lifecycle owner publishes a one-package initial generation and the
two-package target generation, proves pre-publication interruption recovery,
activates the target at serial 2, and rolls back at serial 3. Command resolution
observes only complete public states, and both immutable generations retain
their original byte identities.

## Host generation publication

`Tools/Package/Publish-Installation-Generation.mjs` implements the bounded
Windows/Linux filesystem adapter for an already validated Generation 1 record.
It hashes the caller-supplied bytes, publishes them beneath
`generations/<generation-sha256>/Generation-1.txt` through a private sibling,
flushes and rereads before rename, never rewrites an existing generation, and
admits no extra inventory. Exact repeat publication is idempotent. Recovery
removes at most one complete digest-named unreferenced candidate and preserves
malformed or ambiguous candidates for inspection.

This host adapter deliberately does not parse Generation 1. The caller must
first consume the portable Windvale semantic result. The offline-stage owner
does that through the Windvale-written generation verifier before publishing
the signed target record.

## Host activation publication

`Tools/Package/Publish-Installation-Activation.mjs` implements the first
Windows/Linux filesystem adapter for a caller-supplied, already validated
Activation 1 transition. It deliberately does not parse Activation 1 or plan a
transition. The caller supplies the exact expected public-record SHA-256, the
exact next bytes, and their expected SHA-256.

`publish` compares the current public identity, recognizes an exact
already-published record without rewriting it, exclusively creates one
digest-named sibling, flushes and rereads that sibling, atomically replaces the
public record, and flushes the state directory where the host API can report
that guarantee. A stale expected identity is rejected before a candidate is
created. A failure after replacement is reported as indeterminate rather than
known unchanged.

`recover` reads the public record first. It removes at most one bounded ordinary
digest-named candidate only after its bytes match the name, and it never changes
the public record. Multiple, malformed, symbolic-link, or identity-mismatched
candidates are preserved for inspection and rejected.

The adapter requires an existing ordinary installation root and owns only its
`state` directory. This development adapter uses the pinned Node.js host runtime;
it is not included in the immutable `v0.1.0` installers and is not yet an
installed `wv` command. Its focused owner proves initial activation,
idempotency, effective replacement, stale-writer rejection, interruption
recovery, corrupt-candidate preservation, rollback publication, and empty
recovery on each permanent host.
