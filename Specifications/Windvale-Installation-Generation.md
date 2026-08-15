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
