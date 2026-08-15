# Windvale identity, time, entropy, and trust architecture

## Status

Recommended future architecture under proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md). It expands the accepted explicit-facility direction in [Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md) and supplies prerequisites for the [network](Network-Stack.md), [remote-terminal](Remote-Terminal-Protocol.md), package, and release paths. No secure-entropy provider, civil-time provider, key store, trust store, TLS provider, release-signing root, or Windvale OS identity service is implemented.

## Recommendation

Windvale should separate five concepts that conventional platforms often blend:

1. clocks and timers measure or schedule time;
2. entropy providers supply unpredictable seed material;
3. key stores perform private-key operations without exporting secrets;
4. identity providers turn verified credential evidence into a named principal; and
5. authorization providers bind that principal and current policy to exact capabilities.

Authentication is evidence about identity. It is never itself authority. A trusted package signer, TLS peer, local administrator, service identity, and boot image occupy different trust domains even when they use similar cryptographic primitives.

## Time contracts

Define separate semantic capabilities:

- `clock.monotonic` returns opaque monotonic instants and checked durations suitable for deadlines, retransmission, leases, accounting windows, and backoff;
- `timer.wait` creates bounded waits against a monotonic deadline;
- `clock.civil` returns UTC civil-time evidence plus provider generation, uncertainty, and synchronization status; and
- `clock.admin` changes or synchronizes civil time and is never granted with ordinary read access implicitly.

Monotonic instants cannot be serialized as global timestamps or compared across provider generations. Civil time can jump and is not used for scheduler quanta or elapsed-time accounting. Certificate, release, or revocation policy must declare whether it requires synchronized civil time, accepts a trusted signed checkpoint, or uses a pinned-key policy that does not depend on wall-clock validity.

Deterministic tests bind a virtual monotonic clock and explicit civil-time snapshot. Production code never detects test mode from a magic date or host variable.

## Entropy contracts

`entropy.secure` supplies bounded cryptographically secure bytes or fails explicitly. It never falls back to a timestamp, cycle counter, device serial, uninitialized memory, deterministic generator, or weak host API. The provider records health and reseed evidence without exposing internal state.

`entropy.deterministic-test` is a different interface identity. It accepts an explicit seed, produces reproducible output, and cannot satisfy a secure-entropy requirement through version or provider substitution. Test artifacts and diagnostics label it visibly.

Windvale should implement a reviewed construction aligned with the NIST SP 800-90 family only after entropy-source, conditioning, reseed, fork/snapshot, health-test, and failure rules are specified. [SP 800-90A Rev. 1](https://csrc.nist.gov/pubs/sp/800/90/a/r1/final) defines DRBG mechanisms, [SP 800-90B](https://csrc.nist.gov/pubs/sp/800/90/b/final) covers entropy sources, and final [SP 800-90C](https://csrc.nist.gov/pubs/sp/800/90/c/final) defines constructions combining them. Copying one algorithm without the source and lifecycle contracts is not a qualified secure provider.

Windows and Linux initially bind secure entropy to their qualified native facilities. Windvale OS should begin with a virtual entropy device only as explicitly reported VM evidence, then add measured CPU and hardware sources with conditioning and health tests. VM snapshot, clone, resume, and early boot must force reseed or fail secure use; duplicated generator state must not create duplicated keys or transport nonces.

## Keys and identities

A private key is represented by a rights-limited key-operation capability, not ordinary `bytes`. Its interface may permit selected signing, handshake, derivation, rotation, or destruction operations under exact algorithm and use policy. Export is absent by default. Diagnostic, serialization, equality, and general memory APIs cannot reveal secret material.

An identity record should contain:

- canonical identity kind and version;
- public-key or credential digest;
- provider and identity generation;
- bounded administrative label separated from the canonical identity;
- approved usages and algorithm profile; and
- optional issuer, validity, attestation, or hardware-protection evidence.

Machine, service, remote-client, package-publisher, release-channel, and administrative identities use separate kinds. Reusing one key across kinds is not the default and requires explicit policy.

The proposed
[persistent-self governance architecture](Persistent-Self-Ownership-And-Governance.md)
also distinguishes authenticated identity from a governance role. Proving that
a caller is an E-Worker developer, primary principal, constitutional steward,
domain owner, runtime custodian, auditor, or recovery party does not prove that
the caller currently occupies that role for one self. The admitted governance
manifest binds the identity and roster generation, exact role, scope, threshold,
expiry, and charter revision. A development/test roster may use explicit fixture
identities, but those identities and universal test access must be structurally
ineligible after an advanced-profile transition.

Trust stores are immutable, content-addressed snapshots. A trust update creates a new generation rather than mutating a live set in place. A connection, launch, or release verification record identifies the exact trust snapshot and policy used. Revocation creates an observable provider generation change and an explicit decision about existing sessions; it never rewrites old evidence.

## TLS and secure streams

The secure-transport provider implements current TLS 1.3 under [RFC 9846](https://www.rfc-editor.org/info/rfc9846/) and the application identity rules required by [RFC 9525](https://www.rfc-editor.org/info/rfc9525/). It returns a secure ordered-stream capability plus bounded peer evidence. Applications do not receive unrestricted access to private keys, session secrets, trust-store internals, or native TLS handles.

The first Windvale-controlled remote profile should use mutual authentication with explicitly provisioned small certificates whose subject-public-key digests are pinned. This fits ordinary Windows and Linux TLS providers while avoiding public-PKI path discovery. Certificate parsing and handshake processing remain strictly bounded, and the certificate's descriptive names do not replace the pinned canonical identity. TLS 1.3 also supports raw public keys through [RFC 7250](https://www.rfc-editor.org/info/rfc7250/); that is a later alternative for a Windvale-native provider if measured code size and interoperability justify a profile revision, not a second first path.

A pinned-key policy can avoid public-PKI path discovery and civil-time dependence for the first isolated deployment, but it still requires secure provisioning, protected private-key custody, explicit rotation overlap, revocation, recovery, and audit. Public-PKI validation is a later policy with exact peer names, trust anchors, usage checks, civil-time requirements, and bounded chain processing.

TLS early application data remains disabled for state-changing Windvale protocols. Downgrade, unexpected application protocol, invalid peer evidence, expired or revoked policy, entropy failure, key-store failure, handshake limit, and close failure are distinct outcomes. A TLS connection authenticates a peer; a separate authorization provider decides whether that peer receives a terminal, package, update, or other capability.

## Authorization records

An authorization decision binds:

- authenticated principal identity and generation;
- policy identity and generation;
- requested operation or session profile;
- exact granted capabilities and reductions;
- resource-domain limits and optional monotonic expiry;
- reason code and bounded audit correlation; and
- revocation behavior for an already active grant.

The decision is immutable. A policy update creates a new decision for new work and sends explicit revocation or replacement events where the interface permits them. An interactive prompt is only one input to policy; it is not the enforcement boundary.

A persistent-self amendment approval is likewise evidence, not a capability.
The governance owner first validates the required principal, steward,
domain/data, audit, or recovery roles against the exact manifest generation and
proposal revision. Existing authorization owners still decide whether any
resulting export, restore, provider binding, deletion, or external action may
execute.

## Provisioning and recovery

Development and production assurance remain visibly different:

- deterministic test keys may be committed only when clearly public, test-only, and rejected by production profiles;
- isolated QEMU qualification may use provisioned ephemeral test identities and makes no production-custody claim;
- a production listener remains disabled until the selected key provider protects private material at rest and in use;
- recovery keys and offline release roots are kept outside ordinary runtime stores; and
- key replacement always supports an explicit overlap or recovery path that cannot silently broaden authorization.

Windvale OS may initially use an encrypted key store unlocked through local administrative action. Hardware-backed TPM or platform security-module storage is a later stronger provider behind the same key-operation contract. Neither is required to redefine application identity semantics.

## Implementation sequence

1. Qualify monotonic clock and bounded timer capabilities over the scheduler's clocksource/clockevent split.
2. Define virtual time and deterministic test entropy for reproducible host tests.
3. Bind secure entropy on Windows and Linux and qualify health, exhaustion, process-clone, and provider-loss behavior.
4. Define one non-exportable key-operation interface plus one immutable trust-snapshot format.
5. Qualify mutually authenticated small-certificate TLS with pinned subject-public-key digests between Windows and Linux using current TLS 1.3, exact ALPN, no early data, and bounded closure.
6. Add the authorization-decision record and exercise grant, denial, revocation, policy replacement, and session teardown independently of TLS.
7. Bind a clearly labeled QEMU test entropy/key provider, then qualify the first Windvale OS secure connection on an isolated network.
8. Add protected persistent custody, rotation, release signing, public-PKI policy, TPM-backed providers, and broader identity directories only from measured consumers.

## Deliberately open details

The architecture does not yet freeze algorithms beyond a selected standards profile, certificate encoding profile, public-key digest, keystore encryption, hardware root, trust-snapshot format, civil-time synchronization protocol, revocation distribution, TLS implementation, or numeric limits. It does fix the first small-certificate/pinned-public-key direction, the separation among time, entropy, key operations, identity, authorization, and capability grants, explicit test providers, immutable trust generations, non-exportable secret handling, and no production network listener without qualified custody.
